using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MK3MoveBook;

namespace MK3MoveBook.Research
{
    internal static class MameResearchCapture
    {
        private const uint ProcessVmRead = 0x0010;
        private const uint ProcessQueryInformation = 0x0400;
        private const uint MemCommit = 0x1000;
        private const uint MemPrivate = 0x20000;
        private const uint PageGuard = 0x100;
        private const uint PageNoAccess = 0x01;

        private const int MainRamSize = 0x80000;
        private const int CaptureIntervalMilliseconds = 18;
        private const int FullHealth = 0xA6;
        private const int PlayerOneCharacterOffset = 0x0C146;
        private const int PlayerOneObjectPointerOffset = 0x0C13A;
        private const int PlayerOneHealthOffset = 0x0C14C;
        private const int PlayerOneRunOffset = 0x0C14E;
        private const int PlayerTwoCharacterOffset = 0x0C2BC;
        private const int PlayerTwoObjectPointerOffset = 0x0C2B0;
        private const int PlayerTwoHealthOffset = 0x0C2C2;
        private const int PlayerTwoRunOffset = 0x0C2C4;
        private const int PlayerOneInputOffset = 0x0B732;
        private const int PlayerTwoInputOffset = 0x0B733;
        private const int AuxiliaryInputOffset = 0x0B734;
        private const int KombatZoneOffset = 0x0C44C;
        private const int PowerbarsEnabledOffset = 0x0C424;
        private const int FinisherTypeCandidateAOffset = 0x0C426;
        private const int FinisherSuccessOffset = 0x0C442;
        private const int FinishPhaseCandidateOffset = 0x0C454;
        private const int FinisherTypeCandidateBOffset = 0x0C4E6;
        private const int NoThrowingOffset = 0x0C55C;
        private const int NoBlockingOffset = 0x0C560;
        private const int FastUppercutOffset = 0x0C57C;
        private const int UnlimitedRunOffset = 0x0C57E;
        private const uint TmsMainRamAddress = 0x01000000;
        private const int ObjectXOffset = 0x12;
        private const int ObjectYOffset = 0x16;
        private const int ObjectFlagsOffset = 0x2C;
        private const int ObjectCharacterOffset = 0x3E;

        private const long MaximumScanRegionSize = 64L * 1024L * 1024L;
        private const uint MainRamByteStart = 0x00200000;
        private const uint MainRamByteEnd = 0x0027FFFF;
        private const uint MainProgramByteMask = 0x1FFFFFFF;

        private static readonly string[] ProcessNames =
        {
            "mameppkgui",
            "mameppk",
            "mame",
            "mame64"
        };

        private static volatile bool stopRequested;

        public static int Main(string[] args)
        {
            int maximumSeconds = ParseMaximumSeconds(args);
            string sessionDirectory = CreateSessionDirectory();
            string streamPath = Path.Combine(
                sessionDirectory,
                "ram-xor-stream.bin.gz"
            );
            string eventsPath = Path.Combine(
                sessionDirectory,
                "known-fields.csv"
            );
            string infoPath = Path.Combine(sessionDirectory, "capture-info.txt");

            Console.OutputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs)
            {
                eventArgs.Cancel = true;
                stopRequested = true;
            };

            Console.WriteLine("MK3 MoveBook — исследовательский захват памяти");
            Console.WriteLine("Только чтение: запись в MAME не выполняется.");
            Console.WriteLine("Папка: " + sessionDirectory);
            Console.WriteLine("Для остановки нажмите Ctrl+C или закройте окно.");
            Console.WriteLine();

            Stopwatch lifetime = Stopwatch.StartNew();
            int frameCount = 0;
            int processSwitches = 0;
            long lastRamAddress = 0;
            byte[] previousRam = null;
            string lastKnownSignature = "";
            DateTime nextStatusUtc = DateTime.MinValue;
            DateTime nextRamScanUtc = DateTime.MinValue;
            int activeProcessId = 0;
            IntPtr processHandle = IntPtr.Zero;
            long bankPointerAddress = 0;
            MameFightTracker fightTracker = new MameFightTracker();

            timeBeginPeriod(1);
            try
            {
                using (FileStream streamFile = new FileStream(
                    streamPath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.Read
                ))
                using (GZipStream compressed = new GZipStream(
                    streamFile,
                    CompressionLevel.Fastest
                ))
                using (BinaryWriter binary = new BinaryWriter(
                    compressed,
                    Encoding.UTF8
                ))
                using (StreamWriter events = CreateEventsWriter(eventsPath))
                {
                    WriteStreamHeader(binary);
                    Stopwatch schedule = Stopwatch.StartNew();
                    long nextFrameAt = 0;

                    while (!stopRequested &&
                        (maximumSeconds <= 0 || lifetime.Elapsed.TotalSeconds < maximumSeconds))
                    {
                        if (processHandle == IntPtr.Zero)
                        {
                            Process process = FindMameProcess();
                            if (process == null)
                            {
                                PrintWaitingStatus(ref nextStatusUtc);
                                Thread.Sleep(250);
                                continue;
                            }
                            using (process)
                            {
                                processHandle = OpenProcess(
                                    ProcessQueryInformation | ProcessVmRead,
                                    false,
                                    process.Id
                                );
                                if (processHandle == IntPtr.Zero)
                                {
                                    PrintWaitingStatus(ref nextStatusUtc);
                                    Thread.Sleep(500);
                                    continue;
                                }
                                activeProcessId = process.Id;
                                processSwitches++;
                                nextRamScanUtc = DateTime.MinValue;
                                lastRamAddress = 0;
                                bankPointerAddress = 0;
                                previousRam = null;
                                lastKnownSignature = "";
                                fightTracker.Reset();
                                Console.WriteLine("MAME найден, PID " + activeProcessId + ".");
                            }
                        }

                        if (!ProcessStillExists(activeProcessId))
                        {
                            CloseHandle(processHandle);
                            processHandle = IntPtr.Zero;
                            activeProcessId = 0;
                            lastRamAddress = 0;
                            bankPointerAddress = 0;
                            previousRam = null;
                            lastKnownSignature = "";
                            fightTracker.Reset();
                            Console.WriteLine("MAME закрыт. Ожидаю новый процесс...");
                            continue;
                        }

                        if (DateTime.UtcNow >= nextRamScanUtc)
                        {
                            nextRamScanUtc = DateTime.UtcNow.AddSeconds(1);
                            long discovered = RefreshMainRamAddress(
                                processHandle,
                                ref bankPointerAddress
                            );
                            if (discovered != 0 && discovered != lastRamAddress)
                            {
                                lastRamAddress = discovered;
                                previousRam = null;
                                lastKnownSignature = "";
                                fightTracker.Reset();
                                Console.WriteLine(
                                    "Основная RAM: 0x" + discovered.ToString("X")
                                );
                            }
                        }

                        if (lastRamAddress == 0)
                        {
                            PrintWaitingStatus(ref nextStatusUtc);
                            Thread.Sleep(100);
                            continue;
                        }

                        byte[] currentRam;
                        if (!TryReadBytes(
                            processHandle,
                            lastRamAddress,
                            MainRamSize,
                            out currentRam
                        ))
                        {
                            lastRamAddress = 0;
                            bankPointerAddress = 0;
                            previousRam = null;
                            lastKnownSignature = "";
                            fightTracker.Reset();
                            continue;
                        }

                        WriteFrame(
                            binary,
                            DateTime.UtcNow.Ticks,
                            frameCount,
                            activeProcessId,
                            lastRamAddress,
                            currentRam,
                            previousRam
                        );
                        frameCount++;
                        WriteKnownFieldsIfChanged(
                            events,
                            currentRam,
                            frameCount,
                            activeProcessId,
                            lastRamAddress,
                            fightTracker,
                            ref lastKnownSignature
                        );
                        previousRam = currentRam;

                        if (DateTime.UtcNow >= nextStatusUtc)
                        {
                            nextStatusUtc = DateTime.UtcNow.AddSeconds(1);
                            Console.WriteLine(
                                "{0:HH:mm:ss} · кадров {1} · P1 {2:X2}/{3} HP · " +
                                "P2 {4:X2}/{5} HP · арена {6:X2}",
                                DateTime.Now,
                                frameCount,
                                currentRam[PlayerOneCharacterOffset],
                                currentRam[PlayerOneHealthOffset],
                                currentRam[PlayerTwoCharacterOffset],
                                currentRam[PlayerTwoHealthOffset],
                                currentRam[KombatZoneOffset]
                            );
                        }

                        nextFrameAt += CaptureIntervalMilliseconds;
                        long remaining = nextFrameAt - schedule.ElapsedMilliseconds;
                        if (remaining > 0)
                        {
                            Thread.Sleep((int)Math.Min(remaining, 50));
                        }
                        else if (remaining < -1000)
                        {
                            nextFrameAt = schedule.ElapsedMilliseconds;
                        }
                    }
                }
            }
            finally
            {
                if (processHandle != IntPtr.Zero)
                {
                    CloseHandle(processHandle);
                }
                timeEndPeriod(1);
                WriteCaptureInfo(
                    infoPath,
                    frameCount,
                    processSwitches,
                    lifetime.Elapsed,
                    streamPath,
                    eventsPath
                );
            }

            Console.WriteLine();
            Console.WriteLine("Захват завершён: " + frameCount + " кадров.");
            Console.WriteLine("Файл: " + streamPath);
            return frameCount > 0 ? 0 : 1;
        }

        private static int ParseMaximumSeconds(string[] args)
        {
            for (int index = 0; index + 1 < args.Length; index++)
            {
                if (!string.Equals(
                    args[index],
                    "--seconds",
                    StringComparison.OrdinalIgnoreCase
                ))
                {
                    continue;
                }
                int value;
                if (int.TryParse(
                    args[index + 1],
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out value
                ))
                {
                    return Math.Max(0, value);
                }
            }
            return 0;
        }

        private static string CreateSessionDirectory()
        {
            string root = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "research-captures"
            );
            Directory.CreateDirectory(root);
            string name = "capture-" + DateTime.Now.ToString(
                "yyyyMMdd-HHmmss",
                CultureInfo.InvariantCulture
            );
            string directory = Path.Combine(root, name);
            Directory.CreateDirectory(directory);
            return directory;
        }

        private static StreamWriter CreateEventsWriter(string path)
        {
            StreamWriter writer = new StreamWriter(
                new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read),
                new UTF8Encoding(true)
            );
            writer.WriteLine(
                "timestamp_utc;frame;process_id;ram_address;" +
                "p1_id;p1_name;p1_health;p1_run;" +
                "p1_object_pointer;p1_x;p1_y;p1_object_flags;p1_object_character;p1_facing;" +
                "p2_id;p2_name;p2_health;p2_run;" +
                "p2_object_pointer;p2_x;p2_y;p2_object_flags;p2_object_character;p2_facing;" +
                "p1_input;p2_input;aux_input;finish_phase;" +
                "finisher_type_a;finisher_success;finisher_type_b;finisher_kind;" +
                "fight_state;round;p1_rounds;p2_rounds;winner_side;winner;event_kind;zone;" +
                "powerbars_enabled;no_throwing;no_blocking;fast_uppercut;unlimited_run"
            );
            writer.Flush();
            return writer;
        }

        private static void WriteKnownFieldsIfChanged(
            StreamWriter writer,
            byte[] ram,
            int frame,
            int processId,
            long ramAddress,
            MameFightTracker fightTracker,
            ref string lastSignature
        )
        {
            int p1Id = ram[PlayerOneCharacterOffset];
            int p2Id = ram[PlayerTwoCharacterOffset];
            int p1Health = ram[PlayerOneHealthOffset];
            int p2Health = ram[PlayerTwoHealthOffset];
            PlayerObjectState p1Object = ReadPlayerObject(
                ram,
                PlayerOneObjectPointerOffset
            );
            PlayerObjectState p2Object = ReadPlayerObject(
                ram,
                PlayerTwoObjectPointerOffset
            );
            string p1Facing = Facing(p1Object.X, p2Object.X);
            string p2Facing = Facing(p2Object.X, p1Object.X);
            MameFightProgress progress = null;
            if (p1Health <= FullHealth && p2Health <= FullHealth)
            {
                progress = fightTracker.Observe(
                    p1Id,
                    CharacterName(p1Id),
                    p1Health,
                    p2Id,
                    CharacterName(p2Id),
                    p2Health
                );
            }
            string signature = string.Join(
                "|",
                p1Id,
                p1Health,
                ram[PlayerOneRunOffset],
                p1Object.Pointer,
                p1Object.X,
                p1Object.Y,
                p1Object.Flags,
                p1Object.CharacterId,
                p1Facing,
                p2Id,
                p2Health,
                ram[PlayerTwoRunOffset],
                p2Object.Pointer,
                p2Object.X,
                p2Object.Y,
                p2Object.Flags,
                p2Object.CharacterId,
                p2Facing,
                ram[PlayerOneInputOffset],
                ram[PlayerTwoInputOffset],
                ram[AuxiliaryInputOffset],
                ram[FinishPhaseCandidateOffset],
                ram[FinisherTypeCandidateAOffset],
                ram[FinisherSuccessOffset],
                ram[FinisherTypeCandidateBOffset],
                FinisherKind(ram[FinisherTypeCandidateBOffset]),
                progress == null ? "" : progress.State,
                progress == null ? 0 : progress.RoundNumber,
                progress == null ? 0 : progress.PlayerOneRounds,
                progress == null ? 0 : progress.PlayerTwoRounds,
                progress == null ? "" : progress.WinnerSide,
                progress == null ? "" : progress.EventKind,
                ram[KombatZoneOffset],
                ram[PowerbarsEnabledOffset],
                ram[NoThrowingOffset],
                ram[NoBlockingOffset],
                ram[FastUppercutOffset],
                ram[UnlimitedRunOffset]
            );
            if (string.Equals(signature, lastSignature, StringComparison.Ordinal))
            {
                return;
            }
            lastSignature = signature;
            writer.WriteLine(
                string.Join(
                    ";",
                    DateTime.UtcNow.ToString(
                        "yyyy-MM-ddTHH:mm:ss.fffZ",
                        CultureInfo.InvariantCulture
                    ),
                    frame.ToString(CultureInfo.InvariantCulture),
                    processId.ToString(CultureInfo.InvariantCulture),
                    "0x" + ramAddress.ToString("X"),
                    Hex((byte)p1Id),
                    CharacterName(p1Id),
                    p1Health.ToString(CultureInfo.InvariantCulture),
                    ram[PlayerOneRunOffset].ToString(CultureInfo.InvariantCulture),
                    ObjectPointer(p1Object),
                    ObjectNumber(p1Object, p1Object.X),
                    ObjectNumber(p1Object, p1Object.Y),
                    ObjectFlags(p1Object),
                    ObjectCharacter(p1Object),
                    p1Facing,
                    Hex((byte)p2Id),
                    CharacterName(p2Id),
                    p2Health.ToString(CultureInfo.InvariantCulture),
                    ram[PlayerTwoRunOffset].ToString(CultureInfo.InvariantCulture),
                    ObjectPointer(p2Object),
                    ObjectNumber(p2Object, p2Object.X),
                    ObjectNumber(p2Object, p2Object.Y),
                    ObjectFlags(p2Object),
                    ObjectCharacter(p2Object),
                    p2Facing,
                    Hex(ram[PlayerOneInputOffset]),
                    Hex(ram[PlayerTwoInputOffset]),
                    Hex(ram[AuxiliaryInputOffset]),
                    Hex(ram[FinishPhaseCandidateOffset]),
                    Hex(ram[FinisherTypeCandidateAOffset]),
                    Hex(ram[FinisherSuccessOffset]),
                    Hex(ram[FinisherTypeCandidateBOffset]),
                    FinisherKind(ram[FinisherTypeCandidateBOffset]),
                    progress == null ? "" : progress.State,
                    progress == null ? "" : progress.RoundNumber.ToString(CultureInfo.InvariantCulture),
                    progress == null ? "" : progress.PlayerOneRounds.ToString(CultureInfo.InvariantCulture),
                    progress == null ? "" : progress.PlayerTwoRounds.ToString(CultureInfo.InvariantCulture),
                    progress == null ? "" : progress.WinnerSide,
                    progress == null ? "" : progress.Winner,
                    progress == null ? "" : progress.EventKind,
                    Hex(ram[KombatZoneOffset]),
                    Hex(ram[PowerbarsEnabledOffset]),
                    Hex(ram[NoThrowingOffset]),
                    Hex(ram[NoBlockingOffset]),
                    Hex(ram[FastUppercutOffset]),
                    Hex(ram[UnlimitedRunOffset])
                )
            );
            writer.Flush();
        }

        private static string Hex(byte value)
        {
            return "0x" + value.ToString("X2");
        }

        private static string FinisherKind(byte value)
        {
            switch (value)
            {
                case 0x01:
                    return "fatality";
                case 0x05:
                    return "babality";
                default:
                    return "";
            }
        }

        private static PlayerObjectState ReadPlayerObject(
            byte[] ram,
            int pointerOffset
        )
        {
            uint pointer = BitConverter.ToUInt32(ram, pointerOffset);
            if (pointer < TmsMainRamAddress)
            {
                return new PlayerObjectState(pointer, false, 0, 0, 0, 0);
            }
            long objectOffset = (pointer - TmsMainRamAddress) / 8;
            if (objectOffset < 0 ||
                objectOffset + ObjectCharacterOffset >= ram.LongLength)
            {
                return new PlayerObjectState(pointer, false, 0, 0, 0, 0);
            }
            int offset = checked((int)objectOffset);
            return new PlayerObjectState(
                pointer,
                true,
                BitConverter.ToUInt16(ram, offset + ObjectXOffset),
                BitConverter.ToUInt16(ram, offset + ObjectYOffset),
                BitConverter.ToUInt16(ram, offset + ObjectFlagsOffset),
                ram[offset + ObjectCharacterOffset]
            );
        }

        private static string Facing(int ownX, int opponentX)
        {
            if (ownX < 0 || opponentX < 0 || ownX == opponentX)
            {
                return "unknown";
            }
            return ownX < opponentX ? "right" : "left";
        }

        private static string ObjectPointer(PlayerObjectState state)
        {
            return "0x" + state.Pointer.ToString("X8");
        }

        private static string ObjectNumber(PlayerObjectState state, int value)
        {
            return state.Valid
                ? value.ToString(CultureInfo.InvariantCulture)
                : "";
        }

        private static string ObjectFlags(PlayerObjectState state)
        {
            return state.Valid ? "0x" + state.Flags.ToString("X4") : "";
        }

        private static string ObjectCharacter(PlayerObjectState state)
        {
            return state.Valid ? Hex((byte)state.CharacterId) : "";
        }

        private struct PlayerObjectState
        {
            public readonly uint Pointer;
            public readonly bool Valid;
            public readonly int X;
            public readonly int Y;
            public readonly int Flags;
            public readonly int CharacterId;

            public PlayerObjectState(
                uint pointer,
                bool valid,
                int x,
                int y,
                int flags,
                int characterId
            )
            {
                Pointer = pointer;
                Valid = valid;
                X = valid ? x : -1;
                Y = valid ? y : -1;
                Flags = valid ? flags : -1;
                CharacterId = valid ? characterId : -1;
            }
        }

        private static string CharacterName(int characterId)
        {
            string[] names =
            {
                "Kano", "Sonya", "Jax", "Nightwolf", "Unmasked Sub-Zero",
                "Stryker", "Sindel", "Sektor", "Cyrax", "Kung Lao", "Kabal",
                "Sheeva", "Shang Tsung", "Liu Kang", "Smoke", "Kitana", "Jade",
                "Mileena", "Scorpion", "Reptile", "Ermac", "Classic Sub-Zero"
            };
            return characterId >= 0 && characterId < names.Length
                ? names[characterId]
                : "ID " + characterId.ToString("X2");
        }

        private static void WriteStreamHeader(BinaryWriter writer)
        {
            writer.Write(Encoding.ASCII.GetBytes("MK3RAM02"));
            writer.Write(2);
            writer.Write(MainRamSize);
            writer.Write(CaptureIntervalMilliseconds);
            writer.Write(DateTime.UtcNow.Ticks);
        }

        private static void WriteFrame(
            BinaryWriter writer,
            long timestampUtcTicks,
            int frameNumber,
            int processId,
            long ramAddress,
            byte[] current,
            byte[] previous
        )
        {
            writer.Write(timestampUtcTicks);
            writer.Write(frameNumber);
            writer.Write(processId);
            writer.Write(ramAddress);

            if (previous == null)
            {
                writer.Write(-1);
                writer.Write(current);
                return;
            }

            int segmentCount = CountChangedSegments(current, previous);
            writer.Write(segmentCount);
            int searchOffset = 0;
            for (int segment = 0; segment < segmentCount; segment++)
            {
                int start = FindNextChange(current, previous, searchOffset);
                int end = FindSegmentEnd(current, previous, start);
                int length = end - start;
                writer.Write(start);
                writer.Write(length);
                writer.Write(current, start, length);
                searchOffset = end;
            }
        }

        private static int CountChangedSegments(byte[] current, byte[] previous)
        {
            int count = 0;
            int searchOffset = 0;
            while (true)
            {
                int start = FindNextChange(current, previous, searchOffset);
                if (start < 0)
                {
                    return count;
                }
                count++;
                searchOffset = FindSegmentEnd(current, previous, start);
            }
        }

        private static int FindNextChange(
            byte[] current,
            byte[] previous,
            int start
        )
        {
            for (int index = start; index < current.Length; index++)
            {
                if (current[index] != previous[index])
                {
                    return index;
                }
            }
            return -1;
        }

        private static int FindSegmentEnd(
            byte[] current,
            byte[] previous,
            int start
        )
        {
            const int maximumLength = 65536;
            const int maximumUnchangedGap = 8;
            int lastChanged = start;
            int limit = Math.Min(current.Length, start + maximumLength);
            for (int index = start + 1; index < limit; index++)
            {
                if (current[index] != previous[index])
                {
                    lastChanged = index;
                }
                else if (index - lastChanged >= maximumUnchangedGap)
                {
                    break;
                }
            }
            return lastChanged + 1;
        }

        private static void WriteCaptureInfo(
            string path,
            int frameCount,
            int processSwitches,
            TimeSpan duration,
            string streamPath,
            string eventsPath
        )
        {
            string[] lines =
            {
                "MK3 MoveBook research capture",
                "created_local=" + DateTime.Now.ToString("O"),
                "read_only=true",
                "frames=" + frameCount.ToString(CultureInfo.InvariantCulture),
                "duration_seconds=" + duration.TotalSeconds.ToString(
                    "F3",
                    CultureInfo.InvariantCulture
                ),
                "process_switches=" + processSwitches.ToString(
                    CultureInfo.InvariantCulture
                ),
                "capture_interval_ms=" + CaptureIntervalMilliseconds.ToString(
                    CultureInfo.InvariantCulture
                ),
                "ram_size=" + MainRamSize.ToString(CultureInfo.InvariantCulture),
                "stream=" + streamPath,
                "known_fields=" + eventsPath
            };
            File.WriteAllLines(path, lines, new UTF8Encoding(true));
        }

        private static void PrintWaitingStatus(ref DateTime nextStatusUtc)
        {
            if (DateTime.UtcNow < nextStatusUtc)
            {
                return;
            }
            nextStatusUtc = DateTime.UtcNow.AddSeconds(1);
            Console.WriteLine("Ожидаю MAME и основную RAM...");
        }

        private static Process FindMameProcess()
        {
            foreach (string processName in ProcessNames)
            {
                Process[] processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0)
                {
                    continue;
                }
                Process result = processes[0];
                for (int index = 1; index < processes.Length; index++)
                {
                    processes[index].Dispose();
                }
                return result;
            }
            return null;
        }

        private static bool ProcessStillExists(int processId)
        {
            if (processId <= 0)
            {
                return false;
            }
            try
            {
                using (Process process = Process.GetProcessById(processId))
                {
                    return !process.HasExited;
                }
            }
            catch
            {
                return false;
            }
        }

        private static long RefreshMainRamAddress(
            IntPtr processHandle,
            ref long bankPointerAddress
        )
        {
            uint currentPointer;
            if (bankPointerAddress != 0 &&
                TryReadUInt32(processHandle, bankPointerAddress, out currentPointer) &&
                currentPointer != 0)
            {
                return currentPointer;
            }

            return FindMainRamAddress(processHandle, out bankPointerAddress);
        }

        private static long FindMainRamAddress(
            IntPtr processHandle,
            out long bankPointerAddress
        )
        {
            Dictionary<long, long> matches = new Dictionary<long, long>();
            bankPointerAddress = 0;
            long address = 0;
            int structureSize = Marshal.SizeOf(typeof(MemoryBasicInformation));
            while (address < 0x100000000L)
            {
                MemoryBasicInformation info;
                UIntPtr querySize = VirtualQueryEx(
                    processHandle,
                    new IntPtr(address),
                    out info,
                    new UIntPtr((uint)structureSize)
                );
                if (querySize == UIntPtr.Zero)
                {
                    break;
                }

                long regionBase = info.BaseAddress.ToInt64();
                long regionSize = unchecked((long)info.RegionSize.ToUInt64());
                if (IsScannable(info, regionSize))
                {
                    ScanRegionForRamPointer(
                        processHandle,
                        regionBase,
                        regionSize,
                        matches
                    );
                }

                long nextAddress = regionBase + Math.Max(regionSize, 0x1000L);
                if (nextAddress <= address)
                {
                    break;
                }
                address = nextAddress;
            }

            if (matches.Count != 1)
            {
                return 0;
            }
            foreach (KeyValuePair<long, long> match in matches)
            {
                bankPointerAddress = match.Value;
                return match.Key;
            }
            return 0;
        }

        private static bool IsScannable(
            MemoryBasicInformation info,
            long regionSize
        )
        {
            return info.State == MemCommit &&
                info.Type == MemPrivate &&
                (info.Protect & PageGuard) == 0 &&
                (info.Protect & PageNoAccess) == 0 &&
                regionSize >= 0x1000 &&
                regionSize <= MaximumScanRegionSize;
        }

        private static void ScanRegionForRamPointer(
            IntPtr processHandle,
            long regionBase,
            long regionSize,
            Dictionary<long, long> matches
        )
        {
            int length = checked((int)Math.Min(
                regionSize,
                MaximumScanRegionSize
            ));
            byte[] buffer = new byte[length];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(regionBase),
                buffer,
                buffer.Length,
                out bytesRead
            );
            int actualLength = checked((int)Math.Min(
                bytesRead.ToInt64(),
                buffer.LongLength
            ));
            if ((!read && actualLength < 16) || actualLength < 16)
            {
                return;
            }

            for (int offset = 0; offset + 16 <= actualLength; offset += 4)
            {
                if (BitConverter.ToUInt32(buffer, offset) != MainRamByteStart ||
                    BitConverter.ToUInt32(buffer, offset + 4) != MainRamByteEnd ||
                    BitConverter.ToUInt32(buffer, offset + 8) != MainProgramByteMask)
                {
                    continue;
                }

                uint bankPointerAddress = BitConverter.ToUInt32(
                    buffer,
                    offset + 12
                );
                uint ramPointer;
                if (!TryReadUInt32(
                    processHandle,
                    bankPointerAddress,
                    out ramPointer
                ))
                {
                    continue;
                }
                byte[] probe;
                if (ramPointer != 0 && TryReadBytes(
                    processHandle,
                    ramPointer,
                    MainRamSize,
                    out probe
                ))
                {
                    matches[ramPointer] = bankPointerAddress;
                }
            }
        }

        private static bool TryReadUInt32(
            IntPtr processHandle,
            long address,
            out uint value
        )
        {
            byte[] bytes;
            if (!TryReadBytes(
                processHandle,
                address,
                sizeof(uint),
                out bytes
            ))
            {
                value = 0;
                return false;
            }
            value = BitConverter.ToUInt32(bytes, 0);
            return true;
        }

        private static bool TryReadBytes(
            IntPtr processHandle,
            long address,
            int length,
            out byte[] buffer
        )
        {
            buffer = new byte[length];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(address),
                buffer,
                buffer.Length,
                out bytesRead
            );
            return read && bytesRead.ToInt64() == buffer.Length;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MemoryBasicInformation
        {
            public IntPtr BaseAddress;
            public IntPtr AllocationBase;
            public uint AllocationProtect;
            public UIntPtr RegionSize;
            public uint State;
            public uint Protect;
            public uint Type;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(
            uint desiredAccess,
            bool inheritHandle,
            int processId
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr process,
            IntPtr baseAddress,
            [Out] byte[] buffer,
            int size,
            out IntPtr bytesRead
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr VirtualQueryEx(
            IntPtr process,
            IntPtr address,
            out MemoryBasicInformation buffer,
            UIntPtr length
        );

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("winmm.dll")]
        private static extern uint timeBeginPeriod(uint period);

        [DllImport("winmm.dll")]
        private static extern uint timeEndPeriod(uint period);
    }
}

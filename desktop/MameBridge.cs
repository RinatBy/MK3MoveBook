using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace MK3MoveBook
{
    internal sealed class MameBridgeSnapshot
    {
        public string State { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public int ProcessId { get; set; }
        public string ProcessPath { get; set; }
        public string ProcessVersion { get; set; }
        public string WindowTitle { get; set; }
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public int PlayerOneHealth { get; set; }
        public int PlayerTwoHealth { get; set; }
        public string RomId { get; set; }
        public string RomLabel { get; set; }
        public string FightState { get; set; }
        public int RoundNumber { get; set; }
        public int PlayerOneRounds { get; set; }
        public int PlayerTwoRounds { get; set; }
        public string WinnerSide { get; set; }
        public string Winner { get; set; }
        public int EventSequence { get; set; }
        public string EventKind { get; set; }
        public string EventTitle { get; set; }
        public string EventDetails { get; set; }
        public bool PlayerDataAvailable { get; set; }
        public string MainRamAddress { get; set; }

        public string Signature()
        {
            return string.Join(
                "|",
                State ?? "",
                ProcessId.ToString(),
                Title ?? "",
                Details ?? "",
                PlayerOne ?? "",
                PlayerTwo ?? "",
                PlayerOneId.ToString(),
                PlayerTwoId.ToString(),
                PlayerOneHealth.ToString(),
                PlayerTwoHealth.ToString(),
                RomId ?? "",
                FightState ?? "",
                RoundNumber.ToString(),
                PlayerOneRounds.ToString(),
                PlayerTwoRounds.ToString(),
                WinnerSide ?? "",
                Winner ?? "",
                EventSequence.ToString(),
                PlayerDataAvailable.ToString(),
                MainRamAddress ?? ""
            );
        }
    }

    internal sealed class MameBridge : IDisposable
    {
        private const uint ProcessVmRead = 0x0010;
        private const uint ProcessQueryInformation = 0x0400;
        private const uint MemCommit = 0x1000;
        private const uint MemPrivate = 0x20000;
        private const uint PageGuard = 0x100;
        private const uint PageNoAccess = 0x01;

        private const int MainRamSize = 0x80000;
        private const int PlayerOneCharacterOffset = 0x0C146;
        private const int PlayerOneHealthOffset = 0x0C14C;
        private const int PlayerTwoCharacterOffset = 0x0C2BC;
        private const int PlayerTwoHealthOffset = 0x0C2C2;
        private const int FullHealth = 0xA6;
        private const long MaximumScanRegionSize = 64L * 1024L * 1024L;
        private const uint MainRamByteStart = 0x00200000;
        private const uint MainRamByteEnd = 0x0027FFFF;
        private const uint MainProgramByteMask = 0x1FFFFFFF;

        private static readonly string[] CharacterNames =
        {
            "Kano", "Sonya", "Jax", "Nightwolf", "Unmasked Sub-Zero",
            "Stryker", "Sindel", "Sektor", "Cyrax", "Kung Lao", "Kabal",
            "Sheeva", "Shang Tsung", "Liu Kang", "Smoke", "Kitana", "Jade",
            "Mileena", "Scorpion", "Reptile", "Ermac", "Classic Sub-Zero"
        };

        private readonly object syncRoot = new object();
        private readonly MameFightTracker fightTracker;
        private Timer pollTimer;
        private int pollActive;
        private int processId;
        private IntPtr processHandle;
        private long mainRamAddress;
        private DateTime nextMemoryScanUtc;
        private MameBridgeSnapshot currentSnapshot;
        private string lastSnapshotSignature;
        private bool disposed;

        public event Action<MameBridgeSnapshot> SnapshotChanged;
        public event Action<MameBridgeSnapshot> SnapshotObserved;

        public MameBridge()
        {
            fightTracker = new MameFightTracker();
            currentSnapshot = CreateNotRunningSnapshot();
            lastSnapshotSignature = "";
        }

        public MameBridgeSnapshot CurrentSnapshot
        {
            get
            {
                lock (syncRoot)
                {
                    return currentSnapshot;
                }
            }
        }

        public void Start()
        {
            lock (syncRoot)
            {
                if (disposed || pollTimer != null)
                {
                    return;
                }
                pollTimer = new Timer(Poll, null, 0, 1000);
            }
        }

        private void Poll(object state)
        {
            if (Interlocked.Exchange(ref pollActive, 1) != 0)
            {
                return;
            }

            try
            {
                MameBridgeSnapshot snapshot = ReadSnapshot();
                NotifyObserved(snapshot);
                Publish(snapshot);
            }
            catch (Exception exception)
            {
                MameBridgeSnapshot snapshot = new MameBridgeSnapshot
                {
                    State = "error",
                    Title = "Диагностика MAME приостановлена",
                    Details = exception.Message
                };
                NotifyObserved(snapshot);
                Publish(snapshot);
            }
            finally
            {
                Interlocked.Exchange(ref pollActive, 0);
            }
        }

        private MameBridgeSnapshot ReadSnapshot()
        {
            Process process = FindMameProcess();
            if (process == null)
            {
                ResetProcess();
                return CreateNotRunningSnapshot();
            }

            using (process)
            {
                string processPath = ReadProcessPath(process);
                string processVersion = ReadProcessVersion(processPath);
                string windowTitle = ReadWindowTitle(process);
                if (!EnsureProcessHandle(process.Id))
                {
                    return new MameBridgeSnapshot
                    {
                        State = "access-denied",
                        Title = "MAME найден, но память недоступна",
                        Details = "Portable-клиенту не удалось открыть процесс только для чтения.",
                        ProcessId = process.Id,
                        ProcessPath = processPath,
                        ProcessVersion = processVersion,
                        WindowTitle = windowTitle
                    };
                }

                PlayerState players;
                if (mainRamAddress != 0 && TryReadPlayers(mainRamAddress, out players))
                {
                    return CreateObservedSnapshot(
                        process.Id,
                        processPath,
                        processVersion,
                        windowTitle,
                        players
                    );
                }

                mainRamAddress = 0;
                if (DateTime.UtcNow >= nextMemoryScanUtc)
                {
                    nextMemoryScanUtc = DateTime.UtcNow.AddSeconds(3);
                    long discoveredAddress = FindMainRamAddress();
                    if (discoveredAddress != 0 &&
                        TryReadPlayers(discoveredAddress, out players))
                    {
                        mainRamAddress = discoveredAddress;
                        fightTracker.Reset();
                        return CreateObservedSnapshot(
                            process.Id,
                            processPath,
                            processVersion,
                            windowTitle,
                            players
                        );
                    }
                }

                bool gameLooksRecognized = LooksLikeUmk3(windowTitle);
                return new MameBridgeSnapshot
                {
                    State = gameLooksRecognized ? "game-detected" : "process-found",
                    Title = gameLooksRecognized
                        ? "Ultimate Mortal Kombat 3 распознана"
                        : "Процесс MAME найден",
                    Details = gameLooksRecognized
                        ? "Начните новый раунд с равными шкалами здоровья — мост подтвердит игровые данные."
                        : "Запустите Ultimate Mortal Kombat 3 и начните бой для проверки памяти.",
                    ProcessId = process.Id,
                    ProcessPath = processPath,
                    ProcessVersion = processVersion,
                    WindowTitle = windowTitle
                };
            }
        }

        private static Process FindMameProcess()
        {
            Process[] processes;
            try
            {
                processes = Process.GetProcesses();
            }
            catch
            {
                return null;
            }

            Process selected = null;
            foreach (Process process in processes)
            {
                string name;
                try
                {
                    name = process.ProcessName;
                }
                catch
                {
                    process.Dispose();
                    continue;
                }

                if (!IsMameProcessName(name))
                {
                    process.Dispose();
                    continue;
                }

                bool preferred = name.Equals(
                    "mameppkgui",
                    StringComparison.OrdinalIgnoreCase
                );
                if (selected == null || preferred)
                {
                    if (selected != null)
                    {
                        selected.Dispose();
                    }
                    selected = process;
                    if (preferred)
                    {
                        break;
                    }
                }
                else
                {
                    process.Dispose();
                }
            }
            return selected;
        }

        private static bool IsMameProcessName(string name)
        {
            return name.Equals("mameppkgui", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("mameppk", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("mame", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("mame64", StringComparison.OrdinalIgnoreCase);
        }

        private bool EnsureProcessHandle(int targetProcessId)
        {
            if (processHandle != IntPtr.Zero && processId == targetProcessId)
            {
                return true;
            }

            ResetProcess();
            processHandle = OpenProcess(
                ProcessQueryInformation | ProcessVmRead,
                false,
                targetProcessId
            );
            if (processHandle == IntPtr.Zero)
            {
                return false;
            }

            processId = targetProcessId;
            nextMemoryScanUtc = DateTime.MinValue;
            return true;
        }

        private long FindMainRamAddress()
        {
            if (processHandle == IntPtr.Zero)
            {
                return 0;
            }

            HashSet<long> matches = new HashSet<long>();
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
                    ScanRegionForRamPointer(regionBase, regionSize, matches);
                    if (matches.Count > 1)
                    {
                        return 0;
                    }
                }

                long nextAddress = regionBase + Math.Max(regionSize, 0x1000L);
                if (nextAddress <= address)
                {
                    break;
                }
                address = nextAddress;
            }
            foreach (long match in matches)
            {
                return match;
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

        private void ScanRegionForRamPointer(
            long regionBase,
            long regionSize,
            HashSet<long> matches
        )
        {
            int bytesToRead = checked((int)Math.Min(regionSize, MaximumScanRegionSize));
            byte[] buffer = new byte[bytesToRead];
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
                uint rangeStart = BitConverter.ToUInt32(buffer, offset);
                uint rangeEnd = BitConverter.ToUInt32(buffer, offset + 4);
                uint byteMask = BitConverter.ToUInt32(buffer, offset + 8);
                if (rangeStart == MainRamByteStart &&
                    rangeEnd == MainRamByteEnd &&
                    byteMask == MainProgramByteMask)
                {
                    uint bankPointerAddress = BitConverter.ToUInt32(
                        buffer,
                        offset + 12
                    );
                    // MAME stores a pointer to its bank pointer here. The
                    // second value is the actual backing RAM address.
                    uint ramPointer;
                    if (TryReadUInt32(bankPointerAddress, out ramPointer) &&
                        ramPointer != 0 &&
                        RamPointerIsReadable(ramPointer))
                    {
                        matches.Add(ramPointer);
                    }
                    if (matches.Count > 1)
                    {
                        return;
                    }
                }
            }
        }

        private bool TryReadUInt32(uint address, out uint value)
        {
            value = 0;
            if (address == 0)
            {
                return false;
            }

            byte[] buffer = new byte[sizeof(uint)];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(address),
                buffer,
                buffer.Length,
                out bytesRead
            );
            if (!read || bytesRead.ToInt64() != buffer.Length)
            {
                return false;
            }

            value = BitConverter.ToUInt32(buffer, 0);
            return true;
        }

        private bool RamPointerIsReadable(uint pointer)
        {
            byte[] probe = new byte[MainRamSize];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(pointer),
                probe,
                probe.Length,
                out bytesRead
            );
            return read && bytesRead.ToInt64() == probe.Length;
        }

        private bool TryReadPlayers(long ramAddress, out PlayerState players)
        {
            players = null;
            byte[] buffer = new byte[PlayerTwoHealthOffset + 1];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(ramAddress),
                buffer,
                buffer.Length,
                out bytesRead
            );
            if (!read || bytesRead.ToInt64() < buffer.Length)
            {
                return false;
            }

            int p1Character = buffer[PlayerOneCharacterOffset];
            int p2Character = buffer[PlayerTwoCharacterOffset];
            int p1Health = buffer[PlayerOneHealthOffset];
            int p2Health = buffer[PlayerTwoHealthOffset];
            if (p1Health > FullHealth ||
                p2Health > FullHealth)
            {
                return false;
            }

            players = new PlayerState
            {
                PlayerOneId = p1Character,
                PlayerTwoId = p2Character,
                PlayerOne = CharacterName(p1Character),
                PlayerTwo = CharacterName(p2Character),
                PlayerOneHealth = p1Health,
                PlayerTwoHealth = p2Health
            };
            return true;
        }

        private MameBridgeSnapshot CreateObservedSnapshot(
            int observedProcessId,
            string processPath,
            string processVersion,
            string windowTitle,
            PlayerState players
        )
        {
            MameFightProgress progress = fightTracker.Observe(
                players.PlayerOneId,
                players.PlayerOne,
                players.PlayerOneHealth,
                players.PlayerTwoId,
                players.PlayerTwo,
                players.PlayerTwoHealth
            );
            string romId = DetectRomId(windowTitle);
            string romLabel = RomLabel(romId);
            bool waiting = string.Equals(
                progress.State,
                "waiting",
                StringComparison.Ordinal
            );
            string title = waiting
                ? "Память Ultimate Mortal Kombat 3 подключена"
                : "Бой отслеживается";
            string details = waiting
                ? "Начните бой — появятся бойцы, раунды и журнал событий."
                : "Раунд " + progress.RoundNumber + " · счёт " +
                    progress.PlayerOneRounds + ":" + progress.PlayerTwoRounds + ".";
            if (string.Equals(
                progress.State,
                "round-ended",
                StringComparison.Ordinal
            ))
            {
                title = "Раунд завершён";
                details = "Ожидаем начало следующего раунда · счёт " +
                    progress.PlayerOneRounds + ":" + progress.PlayerTwoRounds + ".";
            }
            else if (string.Equals(
                progress.State,
                "match-ended",
                StringComparison.Ordinal
            ))
            {
                title = "Победитель определён — " + progress.Winner;
                details = "Матч завершён со счётом " +
                    progress.PlayerOneRounds + ":" + progress.PlayerTwoRounds + ".";
            }

            return new MameBridgeSnapshot
            {
                State = waiting ? "game-detected" : "data-confirmed",
                Title = title,
                Details = details,
                ProcessId = observedProcessId,
                ProcessPath = processPath,
                ProcessVersion = processVersion,
                WindowTitle = windowTitle,
                PlayerOne = players.PlayerOne,
                PlayerTwo = players.PlayerTwo,
                PlayerOneId = players.PlayerOneId,
                PlayerTwoId = players.PlayerTwoId,
                PlayerOneHealth = players.PlayerOneHealth,
                PlayerTwoHealth = players.PlayerTwoHealth,
                RomId = romId,
                RomLabel = romLabel,
                FightState = progress.State,
                RoundNumber = progress.RoundNumber,
                PlayerOneRounds = progress.PlayerOneRounds,
                PlayerTwoRounds = progress.PlayerTwoRounds,
                WinnerSide = progress.WinnerSide,
                Winner = progress.Winner,
                EventSequence = progress.EventSequence,
                EventKind = progress.EventKind,
                EventTitle = progress.EventTitle,
                EventDetails = progress.EventDetails,
                PlayerDataAvailable = true,
                MainRamAddress = "0x" + mainRamAddress.ToString("X")
            };
        }

        private static string CharacterName(int characterId)
        {
            if (characterId >= 0 && characterId < CharacterNames.Length)
            {
                return CharacterNames[characterId];
            }
            return "Боец ID 0x" + characterId.ToString("X2");
        }

        private static string DetectRomId(string windowTitle)
        {
            if (string.IsNullOrEmpty(windowTitle))
            {
                return "";
            }
            if (windowTitle.IndexOf(
                "Team Edition v3.0 (6 Players",
                StringComparison.OrdinalIgnoreCase
            ) >= 0)
            {
                return "umk3tm";
            }
            if (windowTitle.IndexOf(
                "Team Edition v3.0 (60fps)",
                StringComparison.OrdinalIgnoreCase
            ) >= 0)
            {
                return "umk3tm60";
            }
            if (windowTitle.IndexOf(
                "UMK3 Uk [260709] (60fps)",
                StringComparison.OrdinalIgnoreCase
            ) >= 0)
            {
                return "umk3uk2";
            }
            if (windowTitle.IndexOf(
                "UMK3 Uk [260709]",
                StringComparison.OrdinalIgnoreCase
            ) >= 0)
            {
                return "umk3uk";
            }
            return "";
        }

        private static string RomLabel(string romId)
        {
            switch (romId)
            {
                case "umk3tm":
                    return "UMK3 Team Edition v3.0 · umk3tm";
                case "umk3tm60":
                    return "UMK3 Team Edition v3.0 60fps · umk3tm60";
                case "umk3uk":
                    return "UMK3 UK [260709] · umk3uk";
                case "umk3uk2":
                    return "UMK3 UK [260709] 60fps · umk3uk2";
                default:
                    return "ROM определяется";
            }
        }

        private static MameBridgeSnapshot CreateNotRunningSnapshot()
        {
            return new MameBridgeSnapshot
            {
                State = "not-running",
                Title = "MAME не запущен",
                Details = "Запустите актуальный клиент MAME и начните бой в UMK3."
            };
        }

        private void Publish(MameBridgeSnapshot snapshot)
        {
            string signature = snapshot.Signature();
            Action<MameBridgeSnapshot> handler;
            lock (syncRoot)
            {
                currentSnapshot = snapshot;
                if (string.Equals(
                    lastSnapshotSignature,
                    signature,
                    StringComparison.Ordinal
                ))
                {
                    return;
                }
                lastSnapshotSignature = signature;
                handler = SnapshotChanged;
            }
            if (handler != null)
            {
                handler(snapshot);
            }
        }

        private void NotifyObserved(MameBridgeSnapshot snapshot)
        {
            Action<MameBridgeSnapshot> handler = SnapshotObserved;
            if (handler == null)
            {
                return;
            }
            try
            {
                handler(snapshot);
            }
            catch
            {
                // A diagnostic report must never interrupt memory polling.
            }
        }

        private static string ReadProcessPath(Process process)
        {
            try
            {
                return process.MainModule.FileName;
            }
            catch
            {
                return "";
            }
        }

        private static string ReadProcessVersion(string processPath)
        {
            try
            {
                return string.IsNullOrEmpty(processPath)
                    ? ""
                    : FileVersionInfo.GetVersionInfo(processPath).FileVersion;
            }
            catch
            {
                return "";
            }
        }

        private static string ReadWindowTitle(Process process)
        {
            try
            {
                return process.MainWindowTitle ?? "";
            }
            catch
            {
                return "";
            }
        }

        private static bool LooksLikeUmk3(string value)
        {
            return !string.IsNullOrEmpty(value) &&
                (value.IndexOf(
                    "Ultimate Mortal Kombat 3",
                    StringComparison.OrdinalIgnoreCase
                ) >= 0 ||
                 value.IndexOf("UMK3", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void ResetProcess()
        {
            if (processHandle != IntPtr.Zero)
            {
                CloseHandle(processHandle);
            }
            processHandle = IntPtr.Zero;
            processId = 0;
            mainRamAddress = 0;
            nextMemoryScanUtc = DateTime.MinValue;
            fightTracker.Reset();
        }

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;
                if (pollTimer != null)
                {
                    pollTimer.Dispose();
                    pollTimer = null;
                }
                ResetProcess();
            }
        }

        private sealed class PlayerState
        {
            public string PlayerOne { get; set; }
            public string PlayerTwo { get; set; }
            public int PlayerOneId { get; set; }
            public int PlayerTwoId { get; set; }
            public int PlayerOneHealth { get; set; }
            public int PlayerTwoHealth { get; set; }
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
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MK3MoveBook.Research
{
    internal static class MameCheatStateProbe
    {
        private const uint ProcessVmRead = 0x0010;
        private const uint ProcessQueryInformation = 0x0400;
        private const uint MemCommit = 0x1000;
        private const uint MemPrivate = 0x20000;
        private const uint PageGuard = 0x100;
        private const uint PageNoAccess = 0x01;

        private const int CheatDescriptionOffset = 4;
        private const int CheatParameterOffset = 12;
        private const int CheatOffScriptOffset = 16;
        private const int CheatOnScriptOffset = 20;
        private const int CheatRunScriptOffset = 24;
        private const int CheatChangeScriptOffset = 28;
        private const int CheatStateOffset = 36;
        private const int CheatTempCountOffset = 40;
        private const int CheatHeaderSize = 44;
        private const int AStringHeaderSize = 8;
        private const int DefaultAStringCapacity = 248;
        private const int MaximumCheatCount = 128;
        private const int MinimumExpectedCheatCount = 20;
        private const int ScanChunkSize = 4 * 1024 * 1024;
        private const long MaximumAddress = 0x100000000L;
        private const int StatePollMilliseconds = 50;
        private const int PopupScanMilliseconds = 1000;

        private const string AnchorDescription = "Infinite Time";
        private const string RequiredDescription = "Infinite Fatality Time";
        private const string ActivatedPrefix = "Activated ";

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
            Console.OutputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += delegate(object sender, ConsoleCancelEventArgs eventArgs)
            {
                eventArgs.Cancel = true;
                stopRequested = true;
            };

            int durationSeconds;
            if (!TryParseDuration(args, out durationSeconds))
            {
                Console.Error.WriteLine("Usage: MK3 Cheat State Probe.exe [--seconds N]");
                return 2;
            }

            DateTime? deadlineUtc = durationSeconds > 0
                ? (DateTime?)DateTime.UtcNow.AddSeconds(durationSeconds)
                : null;
            string outputDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "cheat-diagnostics"
            );
            Directory.CreateDirectory(outputDirectory);
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
            string csvPath = Path.Combine(outputDirectory, "mame-cheat-report-" + stamp + ".csv");
            string summaryPath = Path.Combine(outputDirectory, "mame-cheat-summary-" + stamp + ".txt");

            ProbeSummary summary = new ProbeSummary();
            using (StreamWriter writer = new StreamWriter(csvPath, false, new UTF8Encoding(true)))
            {
                writer.AutoFlush = true;
                writer.WriteLine("timestamp_local,process_id,event,cheat,kind,old_state,new_state,address,evidence");

                Console.WriteLine("MK3 MAME cheat-state diagnostic (read-only)");
                Console.WriteLine("Report: " + csvPath);
                Console.WriteLine("Start MAME 0.129 and the UMK3 ROM. Press Ctrl+C to stop.");

                RunProbe(writer, summary, deadlineUtc);
            }

            WriteSummary(summaryPath, csvPath, summary);
            Console.WriteLine("Diagnostic stopped.");
            Console.WriteLine("Summary: " + summaryPath);
            return summary.DiscoverySucceeded ? 0 : 1;
        }

        private static void RunProbe(
            StreamWriter writer,
            ProbeSummary summary,
            DateTime? deadlineUtc
        )
        {
            DateTime nextWaitingStatusUtc = DateTime.MinValue;
            while (!ShouldStop(deadlineUtc))
            {
                Process process = FindMameProcess();
                if (process == null)
                {
                    if (DateTime.UtcNow >= nextWaitingStatusUtc)
                    {
                        Console.WriteLine("Waiting for MAME...");
                        nextWaitingStatusUtc = DateTime.UtcNow.AddSeconds(2);
                    }
                    Thread.Sleep(250);
                    continue;
                }

                using (process)
                {
                    IntPtr handle = OpenProcess(
                        ProcessQueryInformation | ProcessVmRead,
                        false,
                        process.Id
                    );
                    if (handle == IntPtr.Zero)
                    {
                        Console.Error.WriteLine(
                            "Could not open MAME process. Win32 error " +
                            Marshal.GetLastWin32Error().ToString(CultureInfo.InvariantCulture) + "."
                        );
                        Thread.Sleep(1000);
                        continue;
                    }

                    try
                    {
                        Console.WriteLine(
                            "MAME found, PID " + process.Id.ToString(CultureInfo.InvariantCulture) + "."
                        );
                        WriteEvent(
                            writer,
                            process.Id,
                            "process-attached",
                            "",
                            "",
                            "",
                            "",
                            0,
                            "read-only"
                        );
                        MonitorProcess(handle, process, writer, summary, deadlineUtc);
                    }
                    finally
                    {
                        CloseHandle(handle);
                    }
                }
            }
        }

        private static void MonitorProcess(
            IntPtr processHandle,
            Process process,
            StreamWriter writer,
            ProbeSummary summary,
            DateTime? deadlineUtc
        )
        {
            CheatCatalog catalog = null;
            long popupTextAddress = 0;
            string lastPopupText = "";
            DateTime nextDiscoveryUtc = DateTime.MinValue;
            DateTime nextPopupScanUtc = DateTime.MinValue;
            DateTime nextDiscoveryStatusUtc = DateTime.MinValue;

            while (!ShouldStop(deadlineUtc) && ProcessStillExists(process.Id))
            {
                if (catalog == null && DateTime.UtcNow >= nextDiscoveryUtc)
                {
                    catalog = DiscoverCatalog(processHandle);
                    nextDiscoveryUtc = DateTime.UtcNow.AddSeconds(2);
                    if (catalog == null)
                    {
                        if (DateTime.UtcNow >= nextDiscoveryStatusUtc)
                        {
                            Console.WriteLine("Waiting for the UMK3 cheat list in MAME memory...");
                            nextDiscoveryStatusUtc = DateTime.UtcNow.AddSeconds(2);
                        }
                        Thread.Sleep(100);
                        continue;
                    }

                    summary.DiscoverySucceeded = true;
                    summary.DiscoveredCheatCount = catalog.ActionableCount;
                    Console.WriteLine(
                        "Cheat list found: " +
                        catalog.Entries.Count.ToString(CultureInfo.InvariantCulture) +
                        " entries, " +
                        catalog.ActionableCount.ToString(CultureInfo.InvariantCulture) +
                        " actionable."
                    );
                    WriteEvent(
                        writer,
                        process.Id,
                        "catalog-found",
                        "",
                        "",
                        "",
                        "",
                        catalog.HeadAddress,
                        catalog.Entries.Count.ToString(CultureInfo.InvariantCulture) + " entries"
                    );
                    RecordInitialStates(processHandle, process.Id, writer, catalog, summary);
                }

                if (catalog != null)
                {
                    if (!PollStates(processHandle, process.Id, writer, catalog, summary))
                    {
                        Console.WriteLine("Cheat list was lost; rediscovering after ROM restart.");
                        catalog = null;
                        popupTextAddress = 0;
                        lastPopupText = "";
                        continue;
                    }

                    string popupText;
                    if (popupTextAddress != 0)
                    {
                        popupText = ReadCString(processHandle, popupTextAddress, 512);
                        HandlePopup(
                            process.Id,
                            writer,
                            catalog,
                            summary,
                            popupText,
                            ref lastPopupText
                        );
                    }
                    else if (DateTime.UtcNow >= nextPopupScanUtc)
                    {
                        nextPopupScanUtc = DateTime.UtcNow.AddMilliseconds(PopupScanMilliseconds);
                        if (TryFindActivatedPopup(
                            processHandle,
                            catalog,
                            out popupTextAddress,
                            out popupText
                        ))
                        {
                            Console.WriteLine(
                                "One-shot activation buffer found at 0x" +
                                popupTextAddress.ToString("X8", CultureInfo.InvariantCulture) + "."
                            );
                            HandlePopup(
                                process.Id,
                                writer,
                                catalog,
                                summary,
                                popupText,
                                ref lastPopupText
                            );
                        }
                    }
                }

                Thread.Sleep(StatePollMilliseconds);
            }

            WriteEvent(
                writer,
                process.Id,
                "process-detached",
                "",
                "",
                "",
                "",
                0,
                "MAME exited or ROM restarted"
            );
        }

        private static CheatCatalog DiscoverCatalog(IntPtr processHandle)
        {
            byte[] anchorPattern = Encoding.ASCII.GetBytes(AnchorDescription + "\0");
            List<long> textAddresses = FindPatternAddresses(
                processHandle,
                anchorPattern,
                16
            );
            foreach (long textAddress in textAddresses)
            {
                long astringAddress = textAddress - AStringHeaderSize;
                uint textPointer;
                int capacity;
                if (!TryReadUInt32(processHandle, astringAddress, out textPointer) ||
                    textPointer != unchecked((uint)textAddress) ||
                    !TryReadInt32(processHandle, astringAddress + 4, out capacity) ||
                    capacity < DefaultAStringCapacity ||
                    capacity > 1024 * 1024)
                {
                    continue;
                }

                byte[] pointerPattern = BitConverter.GetBytes(unchecked((uint)astringAddress));
                List<long> references = FindPatternAddresses(
                    processHandle,
                    pointerPattern,
                    64
                );
                foreach (long referenceAddress in references)
                {
                    long candidateHead = referenceAddress - CheatDescriptionOffset;
                    CheatCatalog catalog = TryReadCatalog(processHandle, candidateHead);
                    if (catalog != null)
                    {
                        return catalog;
                    }
                }
            }
            return null;
        }

        private static CheatCatalog TryReadCatalog(IntPtr processHandle, long headAddress)
        {
            if (headAddress <= 0 || (headAddress & 3) != 0)
            {
                return null;
            }

            List<CheatEntry> entries = new List<CheatEntry>();
            HashSet<long> visited = new HashSet<long>();
            long address = headAddress;
            bool foundRequired = false;

            while (address != 0 && entries.Count < MaximumCheatCount)
            {
                if (!visited.Add(address))
                {
                    return null;
                }

                byte[] header;
                if (!TryReadBytes(processHandle, address, CheatHeaderSize, out header))
                {
                    return null;
                }

                uint next = BitConverter.ToUInt32(header, 0);
                uint descriptionPointer = BitConverter.ToUInt32(header, CheatDescriptionOffset);
                uint parameterPointer = BitConverter.ToUInt32(header, CheatParameterOffset);
                uint offScript = BitConverter.ToUInt32(header, CheatOffScriptOffset);
                uint onScript = BitConverter.ToUInt32(header, CheatOnScriptOffset);
                uint runScript = BitConverter.ToUInt32(header, CheatRunScriptOffset);
                uint changeScript = BitConverter.ToUInt32(header, CheatChangeScriptOffset);
                int state = BitConverter.ToInt32(header, CheatStateOffset);
                uint tempCount = BitConverter.ToUInt32(header, CheatTempCountOffset);
                string description = ReadAString(processHandle, descriptionPointer);

                if (description == null || state < 0 || state > 3 ||
                    tempCount == 0 || tempCount > 256)
                {
                    return null;
                }
                if (entries.Count == 0 && description != AnchorDescription)
                {
                    return null;
                }
                if (description == RequiredDescription)
                {
                    foundRequired = true;
                }

                string kind;
                if (parameterPointer == 0 && offScript == 0 && onScript == 0 &&
                    runScript == 0 && changeScript == 0)
                {
                    kind = "label";
                }
                else if (parameterPointer == 0 && offScript == 0 &&
                    runScript == 0 && onScript != 0)
                {
                    kind = "one-shot";
                }
                else
                {
                    kind = "persistent";
                }

                entries.Add(new CheatEntry(address, description, kind, state));
                address = next;
            }

            if (address != 0 || entries.Count < MinimumExpectedCheatCount || !foundRequired)
            {
                return null;
            }
            return new CheatCatalog(headAddress, entries);
        }

        private static void RecordInitialStates(
            IntPtr processHandle,
            int processId,
            StreamWriter writer,
            CheatCatalog catalog,
            ProbeSummary summary
        )
        {
            foreach (CheatEntry entry in catalog.Entries)
            {
                if (entry.Kind == "label")
                {
                    continue;
                }

                Console.WriteLine(
                    "  " + entry.Kind + ": " + entry.Description +
                    " [" + StateName(entry.LastState) + "]"
                );
                if (entry.Kind == "persistent" && entry.LastState != 0)
                {
                    RecordDetection(
                        processId,
                        writer,
                        summary,
                        "active-at-discovery",
                        entry,
                        "unknown",
                        StateName(entry.LastState),
                        "MAME cheat_entry state"
                    );
                }
            }
        }

        private static bool PollStates(
            IntPtr processHandle,
            int processId,
            StreamWriter writer,
            CheatCatalog catalog,
            ProbeSummary summary
        )
        {
            foreach (CheatEntry entry in catalog.Entries)
            {
                int currentState;
                if (!TryReadInt32(
                    processHandle,
                    entry.Address + CheatStateOffset,
                    out currentState
                ) || currentState < 0 || currentState > 3)
                {
                    return false;
                }
                if (currentState == entry.LastState)
                {
                    continue;
                }

                int oldState = entry.LastState;
                entry.LastState = currentState;
                if (entry.Kind != "label")
                {
                    WriteEvent(
                        writer,
                        processId,
                        "state-change",
                        entry.Description,
                        entry.Kind,
                        StateName(oldState),
                        StateName(currentState),
                        entry.Address,
                        "MAME cheat_entry state"
                    );
                    Console.WriteLine(
                        DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                        " " + entry.Description + ": " +
                        StateName(oldState) + " -> " + StateName(currentState)
                    );
                }
                if (entry.Kind == "persistent" && currentState != 0)
                {
                    RecordDetection(
                        processId,
                        writer,
                        summary,
                        "cheat-detected",
                        entry,
                        StateName(oldState),
                        StateName(currentState),
                        "MAME cheat_entry state"
                    );
                }
            }
            return true;
        }

        private static bool TryFindActivatedPopup(
            IntPtr processHandle,
            CheatCatalog catalog,
            out long popupTextAddress,
            out string popupText
        )
        {
            popupTextAddress = 0;
            popupText = null;
            byte[] prefix = Encoding.ASCII.GetBytes(ActivatedPrefix);
            List<long> matches = FindPatternAddresses(processHandle, prefix, 64);
            foreach (long match in matches)
            {
                string candidate = ReadCString(processHandle, match, 512);
                if (catalog.OneShotMessages.Contains(candidate))
                {
                    popupTextAddress = match;
                    popupText = candidate;
                    return true;
                }
            }
            return false;
        }

        private static void HandlePopup(
            int processId,
            StreamWriter writer,
            CheatCatalog catalog,
            ProbeSummary summary,
            string popupText,
            ref string lastPopupText
        )
        {
            if (popupText == null)
            {
                return;
            }
            if (popupText == lastPopupText)
            {
                return;
            }
            lastPopupText = popupText;
            if (!catalog.OneShotMessages.Contains(popupText))
            {
                return;
            }

            string description = popupText.Substring(ActivatedPrefix.Length);
            CheatEntry entry = catalog.Find(description);
            if (entry == null)
            {
                return;
            }
            RecordDetection(
                processId,
                writer,
                summary,
                "cheat-detected",
                entry,
                "OFF",
                "ONE_SHOT",
                "MAME popup: " + popupText
            );
            Console.WriteLine(
                DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture) +
                " one-shot detected: " + description
            );
        }

        private static void RecordDetection(
            int processId,
            StreamWriter writer,
            ProbeSummary summary,
            string eventName,
            CheatEntry entry,
            string oldState,
            string newState,
            string evidence
        )
        {
            summary.DetectedCheats.Add(entry.Description);
            WriteEvent(
                writer,
                processId,
                eventName,
                entry.Description,
                entry.Kind,
                oldState,
                newState,
                entry.Address,
                evidence
            );
        }

        private static List<long> FindPatternAddresses(
            IntPtr processHandle,
            byte[] pattern,
            int maximumMatches
        )
        {
            List<long> matches = new List<long>();
            foreach (MemoryRegion region in EnumeratePrivateRegions(processHandle))
            {
                long position = 0;
                int overlap = Math.Max(pattern.Length - 1, 0);
                while (position < region.Size && matches.Count < maximumMatches)
                {
                    int requested = checked((int)Math.Min(
                        ScanChunkSize,
                        region.Size - position
                    ));
                    byte[] buffer;
                    long chunkAddress = region.BaseAddress + position;
                    if (TryReadBytes(processHandle, chunkAddress, requested, out buffer))
                    {
                        int searchStart = 0;
                        while (searchStart <= buffer.Length - pattern.Length)
                        {
                            int found = IndexOf(buffer, pattern, searchStart);
                            if (found < 0)
                            {
                                break;
                            }
                            matches.Add(chunkAddress + found);
                            if (matches.Count >= maximumMatches)
                            {
                                break;
                            }
                            searchStart = found + 1;
                        }
                    }

                    if (requested == 0 || requested >= region.Size - position)
                    {
                        break;
                    }
                    position += Math.Max(requested - overlap, 1);
                }
                if (matches.Count >= maximumMatches)
                {
                    break;
                }
            }
            return matches;
        }

        private static IEnumerable<MemoryRegion> EnumeratePrivateRegions(IntPtr processHandle)
        {
            long address = 0;
            int structureSize = Marshal.SizeOf(typeof(MemoryBasicInformation));
            while (address < MaximumAddress)
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
                    yield break;
                }

                long regionBase = info.BaseAddress.ToInt64();
                long regionSize = unchecked((long)info.RegionSize.ToUInt64());
                if (regionSize <= 0)
                {
                    yield break;
                }
                if (info.State == MemCommit && info.Type == MemPrivate &&
                    (info.Protect & PageGuard) == 0 &&
                    (info.Protect & PageNoAccess) == 0)
                {
                    yield return new MemoryRegion(regionBase, regionSize);
                }

                long nextAddress = regionBase + Math.Max(regionSize, 0x1000L);
                if (nextAddress <= address)
                {
                    yield break;
                }
                address = nextAddress;
            }
        }

        private static int IndexOf(byte[] buffer, byte[] pattern, int start)
        {
            int limit = buffer.Length - pattern.Length;
            for (int index = start; index <= limit; index++)
            {
                int patternIndex = 0;
                while (patternIndex < pattern.Length &&
                    buffer[index + patternIndex] == pattern[patternIndex])
                {
                    patternIndex++;
                }
                if (patternIndex == pattern.Length)
                {
                    return index;
                }
            }
            return -1;
        }

        private static string ReadAString(IntPtr processHandle, uint astringAddress)
        {
            if (astringAddress == 0)
            {
                return null;
            }
            uint textPointer;
            int capacity;
            if (!TryReadUInt32(processHandle, astringAddress, out textPointer) ||
                !TryReadInt32(processHandle, astringAddress + 4L, out capacity) ||
                textPointer == 0 || capacity <= 0 || capacity > 1024 * 1024)
            {
                return null;
            }
            return ReadCString(processHandle, textPointer, 512);
        }

        private static string ReadCString(IntPtr processHandle, long address, int maximumLength)
        {
            byte[] buffer;
            if (address <= 0 || !TryReadBytes(processHandle, address, maximumLength, out buffer))
            {
                return null;
            }
            int length = Array.IndexOf<byte>(buffer, 0);
            if (length < 0)
            {
                return null;
            }
            return Encoding.UTF8.GetString(buffer, 0, length);
        }

        private static bool TryReadUInt32(IntPtr processHandle, long address, out uint value)
        {
            byte[] buffer;
            if (!TryReadBytes(processHandle, address, 4, out buffer))
            {
                value = 0;
                return false;
            }
            value = BitConverter.ToUInt32(buffer, 0);
            return true;
        }

        private static bool TryReadInt32(IntPtr processHandle, long address, out int value)
        {
            byte[] buffer;
            if (!TryReadBytes(processHandle, address, 4, out buffer))
            {
                value = 0;
                return false;
            }
            value = BitConverter.ToInt32(buffer, 0);
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
            if (read && bytesRead.ToInt64() == buffer.Length)
            {
                return true;
            }
            buffer = null;
            return false;
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

        private static bool TryParseDuration(string[] args, out int durationSeconds)
        {
            durationSeconds = 0;
            for (int index = 0; index < args.Length; index++)
            {
                if (!string.Equals(args[index], "--seconds", StringComparison.OrdinalIgnoreCase) ||
                    index + 1 >= args.Length ||
                    !int.TryParse(
                        args[++index],
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out durationSeconds
                    ) || durationSeconds <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private static bool ShouldStop(DateTime? deadlineUtc)
        {
            return stopRequested ||
                (deadlineUtc.HasValue && DateTime.UtcNow >= deadlineUtc.Value);
        }

        private static string StateName(int state)
        {
            switch (state)
            {
                case 0: return "OFF";
                case 1: return "ON";
                case 2: return "RUN";
                case 3: return "CHANGE";
                default: return "UNKNOWN(" + state.ToString(CultureInfo.InvariantCulture) + ")";
            }
        }

        private static void WriteEvent(
            StreamWriter writer,
            int processId,
            string eventName,
            string cheat,
            string kind,
            string oldState,
            string newState,
            long address,
            string evidence
        )
        {
            string[] fields =
            {
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture),
                processId.ToString(CultureInfo.InvariantCulture),
                eventName,
                cheat,
                kind,
                oldState,
                newState,
                address == 0 ? "" : "0x" + address.ToString("X8", CultureInfo.InvariantCulture),
                evidence
            };
            for (int index = 0; index < fields.Length; index++)
            {
                fields[index] = Csv(fields[index]);
            }
            writer.WriteLine(string.Join(",", fields));
        }

        private static string Csv(string value)
        {
            if (value == null)
            {
                return "";
            }
            if (value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) < 0)
            {
                return value;
            }
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private static void WriteSummary(
            string summaryPath,
            string csvPath,
            ProbeSummary summary
        )
        {
            List<string> lines = new List<string>();
            lines.Add("discovery_succeeded=" + summary.DiscoverySucceeded.ToString().ToLowerInvariant());
            lines.Add(
                "actionable_cheats_discovered=" +
                summary.DiscoveredCheatCount.ToString(CultureInfo.InvariantCulture)
            );
            lines.Add("cheat_use_detected=" + (summary.DetectedCheats.Count > 0 ? "true" : "false"));
            lines.Add("event_report=" + csvPath);
            foreach (string cheat in summary.DetectedCheats)
            {
                lines.Add("detected=" + cheat);
            }
            File.WriteAllLines(summaryPath, lines.ToArray(), new UTF8Encoding(true));
        }

        private sealed class CheatCatalog
        {
            public readonly long HeadAddress;
            public readonly List<CheatEntry> Entries;
            public readonly HashSet<string> OneShotMessages;
            public readonly int ActionableCount;

            public CheatCatalog(long headAddress, List<CheatEntry> entries)
            {
                HeadAddress = headAddress;
                Entries = entries;
                OneShotMessages = new HashSet<string>(StringComparer.Ordinal);
                int actionableCount = 0;
                foreach (CheatEntry entry in entries)
                {
                    if (entry.Kind != "label")
                    {
                        actionableCount++;
                    }
                    if (entry.Kind == "one-shot")
                    {
                        OneShotMessages.Add(ActivatedPrefix + entry.Description);
                    }
                }
                ActionableCount = actionableCount;
            }

            public CheatEntry Find(string description)
            {
                foreach (CheatEntry entry in Entries)
                {
                    if (entry.Description == description)
                    {
                        return entry;
                    }
                }
                return null;
            }
        }

        private sealed class CheatEntry
        {
            public readonly long Address;
            public readonly string Description;
            public readonly string Kind;
            public int LastState;

            public CheatEntry(long address, string description, string kind, int state)
            {
                Address = address;
                Description = description;
                Kind = kind;
                LastState = state;
            }
        }

        private sealed class ProbeSummary
        {
            public bool DiscoverySucceeded;
            public int DiscoveredCheatCount;
            public readonly HashSet<string> DetectedCheats = new HashSet<string>(
                StringComparer.Ordinal
            );
        }

        private struct MemoryRegion
        {
            public readonly long BaseAddress;
            public readonly long Size;

            public MemoryRegion(long baseAddress, long size)
            {
                BaseAddress = baseAddress;
                Size = size;
            }
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

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr handle);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern UIntPtr VirtualQueryEx(
            IntPtr process,
            IntPtr address,
            out MemoryBasicInformation buffer,
            UIntPtr length
        );
    }
}

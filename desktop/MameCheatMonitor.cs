using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MK3MoveBook
{
    internal sealed class MameCheatMonitorSnapshot
    {
        public string State { get; set; }
        public string RomId { get; set; }
        public int CatalogGeneration { get; set; }
        public int ActionableCheatCount { get; set; }
        public int UseSequence { get; set; }
        public int ActiveCheatCount { get; set; }
        public string ActiveCheats { get; set; }
        public string LastDetectedCheat { get; set; }
        public string LastDetectionKind { get; set; }
    }

    internal sealed class MameCheatMonitor : IDisposable
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
        private const int PopupScanMilliseconds = 750;

        private const string AnchorDescription = "Infinite Time";
        private const string RequiredDescription = "Infinite Fatality Time";
        private const string ActivatedPrefix = "Activated ";

        private readonly object syncRoot = new object();
        private readonly HashSet<string> activeCheats = new HashSet<string>(
            StringComparer.Ordinal
        );
        private WorkerContext worker;
        private Thread workerThread;
        private string state = "stopped";
        private string engineRomId = "";
        private int catalogGeneration;
        private int actionableCheatCount;
        private int useSequence;
        private string lastDetectedCheat = "";
        private string lastDetectionKind = "";
        private bool disposed;

        public void Start(int processId)
        {
            Stop();
            if (processId <= 0)
            {
                return;
            }

            WorkerContext context = new WorkerContext(processId);
            Thread thread = new Thread(delegate() { Run(context); });
            thread.IsBackground = true;
            thread.Name = "MK3 MAME cheat monitor";
            thread.Priority = ThreadPriority.BelowNormal;
            lock (syncRoot)
            {
                if (disposed)
                {
                    return;
                }
                worker = context;
                workerThread = thread;
                state = "scanning";
                engineRomId = "";
                catalogGeneration = 0;
                actionableCheatCount = 0;
                useSequence = 0;
                activeCheats.Clear();
                lastDetectedCheat = "";
                lastDetectionKind = "";
            }
            thread.Start();
        }

        public void Stop()
        {
            WorkerContext context;
            Thread thread;
            lock (syncRoot)
            {
                context = worker;
                thread = workerThread;
                worker = null;
                workerThread = null;
                if (context != null)
                {
                    context.StopRequested = true;
                }
            }
            if (thread != null && thread != Thread.CurrentThread)
            {
                thread.Join(1500);
            }
            lock (syncRoot)
            {
                if (worker == null)
                {
                    state = "stopped";
                    actionableCheatCount = 0;
                    activeCheats.Clear();
                }
            }
        }

        public MameCheatMonitorSnapshot Snapshot()
        {
            lock (syncRoot)
            {
                List<string> names = new List<string>(activeCheats);
                names.Sort(StringComparer.Ordinal);
                return new MameCheatMonitorSnapshot
                {
                    State = state,
                    RomId = engineRomId,
                    CatalogGeneration = catalogGeneration,
                    ActionableCheatCount = actionableCheatCount,
                    UseSequence = useSequence,
                    ActiveCheatCount = activeCheats.Count,
                    ActiveCheats = string.Join(", ", names.ToArray()),
                    LastDetectedCheat = lastDetectedCheat,
                    LastDetectionKind = lastDetectionKind
                };
            }
        }

        private void Run(WorkerContext context)
        {
            IntPtr processHandle = OpenProcess(
                ProcessQueryInformation | ProcessVmRead,
                false,
                context.ProcessId
            );
            if (processHandle == IntPtr.Zero)
            {
                SetState(context, "unavailable");
                return;
            }

            try
            {
                CheatCatalog catalog = null;
                MameCheatEngineReader engine = MameCheatEngineReader.TryCreate(context.ProcessId, processHandle);
                uint engineAddress = 0;
                uint engineHead = 0;
                string engineRom = "";
                long popupTextAddress = 0;
                string lastPopupText = "";
                DateTime nextPopupScanUtc = DateTime.MinValue;
                while (!context.StopRequested && ProcessStillExists(context.ProcessId))
                {
                    if (engine != null)
                    {
                        MameCheatEngineSample sample = engine.Read(DateTime.UtcNow);
                        if (!sample.Valid || sample.EngineAddress != engineAddress ||
                            sample.HeadAddress != engineHead || sample.RomId != engineRom)
                        {
                            catalog = null;
                            popupTextAddress = 0;
                            lastPopupText = "";
                            SetScanning(context);
                        }
                        engineAddress = sample.EngineAddress;
                        engineHead = sample.HeadAddress;
                        engineRom = sample.RomId;
                        lock (syncRoot)
                        {
                            if (IsCurrent(context)) engineRomId = sample.RomId;
                        }
                        if (!sample.Valid || sample.HeadAddress == 0)
                        {
                            if (sample.EmptyVerified) PublishEmptyCatalog(context);
                            else SetScanning(context);
                            Wait(context, StatePollMilliseconds);
                            continue;
                        }
                    }
                    if (catalog == null)
                    {
                        SetState(context, "scanning");
                        // For known builds follow the live owner, never a leftover catalog
                        // from a previous ROM. Unknown builds retain the established scanner.
                        catalog = engine == null
                            ? DiscoverCatalog(context, processHandle)
                            : TryReadCatalog(processHandle, engineHead);
                        if (catalog == null)
                        {
                            Wait(context, 500);
                            continue;
                        }
                        popupTextAddress = 0;
                        lastPopupText = "";
                        PublishCatalog(context, catalog);
                    }

                    if (!PollStates(context, processHandle, catalog))
                    {
                        catalog = null;
                        popupTextAddress = 0;
                        lastPopupText = "";
                        SetScanning(context);
                        continue;
                    }

                    string popupText;
                    if (popupTextAddress != 0)
                    {
                        popupText = ReadCString(processHandle, popupTextAddress, 512);
                        HandlePopup(context, catalog, popupText, ref lastPopupText);
                    }
                    else if (DateTime.UtcNow >= nextPopupScanUtc)
                    {
                        nextPopupScanUtc = DateTime.UtcNow.AddMilliseconds(
                            PopupScanMilliseconds
                        );
                        if (TryFindActivatedPopup(
                            context,
                            processHandle,
                            catalog,
                            out popupTextAddress,
                            out popupText
                        ))
                        {
                            HandlePopup(
                                context,
                                catalog,
                                popupText,
                                ref lastPopupText
                            );
                        }
                    }
                    Wait(context, StatePollMilliseconds);
                }
            }
            finally
            {
                CloseHandle(processHandle);
                SetState(context, "stopped");
            }
        }

        private CheatCatalog DiscoverCatalog(
            WorkerContext context,
            IntPtr processHandle
        )
        {
            byte[] anchorPattern = Encoding.ASCII.GetBytes(AnchorDescription + "\0");
            List<long> textAddresses = FindPatternAddresses(
                context,
                processHandle,
                anchorPattern,
                16,
                null
            );
            foreach (long textAddress in textAddresses)
            {
                if (context.StopRequested)
                {
                    return null;
                }
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

                byte[] pointerPattern = BitConverter.GetBytes(
                    unchecked((uint)astringAddress)
                );
                MemoryRegion preferredRegion;
                List<long> references = new List<long>();
                if (TryGetPrivateRegion(
                    processHandle,
                    astringAddress,
                    out preferredRegion
                ))
                {
                    references.AddRange(FindPatternAddresses(
                        context,
                        processHandle,
                        pointerPattern,
                        64,
                        preferredRegion
                    ));
                }
                foreach (long referenceAddress in references)
                {
                    CheatCatalog catalog = TryReadCatalog(
                        processHandle,
                        referenceAddress - CheatDescriptionOffset
                    );
                    if (catalog != null)
                    {
                        catalog.PreferredRegion = preferredRegion;
                        return catalog;
                    }
                }
                references = FindPatternAddresses(
                    context,
                    processHandle,
                    pointerPattern,
                    64,
                    null
                );
                foreach (long referenceAddress in references)
                {
                    CheatCatalog catalog = TryReadCatalog(
                        processHandle,
                        referenceAddress - CheatDescriptionOffset
                    );
                    if (catalog != null)
                    {
                        catalog.PreferredRegion = preferredRegion;
                        return catalog;
                    }
                }
            }
            return null;
        }

        private static CheatCatalog TryReadCatalog(
            IntPtr processHandle,
            long headAddress
        )
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
                uint descriptionPointer = BitConverter.ToUInt32(
                    header,
                    CheatDescriptionOffset
                );
                uint parameterPointer = BitConverter.ToUInt32(
                    header,
                    CheatParameterOffset
                );
                uint offScript = BitConverter.ToUInt32(header, CheatOffScriptOffset);
                uint onScript = BitConverter.ToUInt32(header, CheatOnScriptOffset);
                uint runScript = BitConverter.ToUInt32(header, CheatRunScriptOffset);
                uint changeScript = BitConverter.ToUInt32(
                    header,
                    CheatChangeScriptOffset
                );
                int entryState = BitConverter.ToInt32(header, CheatStateOffset);
                uint tempCount = BitConverter.ToUInt32(header, CheatTempCountOffset);
                string description = ReadAString(processHandle, descriptionPointer);
                if (description == null || entryState < 0 || entryState > 3 ||
                    tempCount == 0 || tempCount > 256)
                {
                    return null;
                }
                if (entries.Count == 0 && description != AnchorDescription)
                {
                    return null;
                }
                foundRequired = foundRequired || description == RequiredDescription;

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
                entries.Add(new CheatEntry(address, description, kind, entryState));
                address = next;
            }
            if (address != 0 || entries.Count < MinimumExpectedCheatCount ||
                !foundRequired)
            {
                return null;
            }
            return new CheatCatalog(entries);
        }

        private void PublishCatalog(WorkerContext context, CheatCatalog catalog)
        {
            lock (syncRoot)
            {
                if (!IsCurrent(context))
                {
                    return;
                }
                state = "ready";
                catalogGeneration++;
                actionableCheatCount = catalog.ActionableCount;
                useSequence = 0;
                activeCheats.Clear();
                lastDetectedCheat = "";
                lastDetectionKind = "";
                // Publish readiness and already active entries atomically, including
                // rediscovery after a pause or ROM change.
                foreach (CheatEntry entry in catalog.Entries)
                {
                    if (entry.Kind == "persistent" && entry.LastState != 0)
                    {
                        SetActive(context, entry.Description, true);
                        RecordUse(context, entry.Description, "persistent-active");
                    }
                }
            }
        }

        private void PublishEmptyCatalog(WorkerContext context)
        {
            lock (syncRoot)
            {
                if (!IsCurrent(context)) return;
                if (state != "ready-empty") catalogGeneration++;
                state = "ready-empty";
                actionableCheatCount = 0;
                activeCheats.Clear();
                // Do not erase a previously observed use; the CPU policy retains its latch.
            }
        }

        private bool PollStates(
            WorkerContext context,
            IntPtr processHandle,
            CheatCatalog catalog
        )
        {
            foreach (CheatEntry entry in catalog.Entries)
            {
                if (context.StopRequested)
                {
                    return false;
                }
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
                if (entry.Kind != "persistent")
                {
                    continue;
                }
                SetActive(context, entry.Description, currentState != 0);
                if (oldState == 0 && currentState != 0)
                {
                    RecordUse(context, entry.Description, "persistent");
                }
            }
            return true;
        }

        private bool TryFindActivatedPopup(
            WorkerContext context,
            IntPtr processHandle,
            CheatCatalog catalog,
            out long popupTextAddress,
            out string popupText
        )
        {
            popupTextAddress = 0;
            popupText = null;
            byte[] prefix = Encoding.ASCII.GetBytes(ActivatedPrefix);
            List<long> matches = FindPatternAddresses(
                context,
                processHandle,
                prefix,
                64,
                catalog.PreferredRegion
            );
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
            matches = FindPatternAddresses(
                context,
                processHandle,
                prefix,
                64,
                null
            );
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

        private void HandlePopup(
            WorkerContext context,
            CheatCatalog catalog,
            string popupText,
            ref string lastPopupText
        )
        {
            if (popupText == null || popupText == lastPopupText)
            {
                return;
            }
            lastPopupText = popupText;
            if (!catalog.OneShotMessages.Contains(popupText))
            {
                return;
            }
            RecordUse(
                context,
                popupText.Substring(ActivatedPrefix.Length),
                "one-shot"
            );
        }

        private void RecordUse(
            WorkerContext context,
            string cheat,
            string detectionKind
        )
        {
            lock (syncRoot)
            {
                if (!IsCurrent(context))
                {
                    return;
                }
                useSequence = useSequence == int.MaxValue ? 1 : useSequence + 1;
                lastDetectedCheat = cheat ?? "";
                lastDetectionKind = detectionKind ?? "";
            }
        }

        private void SetActive(
            WorkerContext context,
            string cheat,
            bool active
        )
        {
            lock (syncRoot)
            {
                if (!IsCurrent(context))
                {
                    return;
                }
                if (active)
                {
                    activeCheats.Add(cheat ?? "");
                }
                else
                {
                    activeCheats.Remove(cheat ?? "");
                }
            }
        }

        private void SetState(WorkerContext context, string value)
        {
            lock (syncRoot)
            {
                if (IsCurrent(context))
                {
                    state = value;
                }
            }
        }

        private void SetScanning(WorkerContext context)
        {
            lock (syncRoot)
            {
                if (!IsCurrent(context))
                {
                    return;
                }
                state = "scanning";
                actionableCheatCount = 0;
                activeCheats.Clear();
            }
        }

        private bool IsCurrent(WorkerContext context)
        {
            return worker == context && !context.StopRequested && !disposed;
        }

        private static void Wait(WorkerContext context, int milliseconds)
        {
            int remaining = milliseconds;
            while (!context.StopRequested && remaining > 0)
            {
                int slice = Math.Min(remaining, 50);
                Thread.Sleep(slice);
                remaining -= slice;
            }
        }

        private static List<long> FindPatternAddresses(
            WorkerContext context,
            IntPtr processHandle,
            byte[] pattern,
            int maximumMatches,
            MemoryRegion? onlyRegion
        )
        {
            List<long> matches = new List<long>();
            IEnumerable<MemoryRegion> regions = onlyRegion.HasValue
                ? (IEnumerable<MemoryRegion>)new[] { onlyRegion.Value }
                : EnumeratePrivateRegions(processHandle);
            foreach (MemoryRegion region in regions)
            {
                long position = 0;
                int overlap = Math.Max(pattern.Length - 1, 0);
                while (position < region.Size && matches.Count < maximumMatches &&
                    !context.StopRequested)
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
                if (matches.Count >= maximumMatches || context.StopRequested)
                {
                    break;
                }
            }
            return matches;
        }

        private static IEnumerable<MemoryRegion> EnumeratePrivateRegions(
            IntPtr processHandle
        )
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
                if (IsPrivateRegion(info))
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

        private static bool TryGetPrivateRegion(
            IntPtr processHandle,
            long address,
            out MemoryRegion region
        )
        {
            MemoryBasicInformation info;
            UIntPtr result = VirtualQueryEx(
                processHandle,
                new IntPtr(address),
                out info,
                new UIntPtr((uint)Marshal.SizeOf(typeof(MemoryBasicInformation)))
            );
            if (result == UIntPtr.Zero || !IsPrivateRegion(info))
            {
                region = new MemoryRegion();
                return false;
            }
            region = new MemoryRegion(
                info.BaseAddress.ToInt64(),
                unchecked((long)info.RegionSize.ToUInt64())
            );
            return true;
        }

        private static bool IsPrivateRegion(MemoryBasicInformation info)
        {
            return info.State == MemCommit && info.Type == MemPrivate &&
                (info.Protect & PageGuard) == 0 &&
                (info.Protect & PageNoAccess) == 0;
        }

        private static int IndexOf(byte[] buffer, byte[] pattern, int start)
        {
            int limit = buffer.Length - pattern.Length;
            int index = start;
            while (index <= limit)
            {
                index = Array.IndexOf(buffer, pattern[0], index);
                if (index < 0 || index > limit)
                {
                    return -1;
                }
                int patternIndex = 1;
                while (patternIndex < pattern.Length &&
                    buffer[index + patternIndex] == pattern[patternIndex])
                {
                    patternIndex++;
                }
                if (patternIndex == pattern.Length)
                {
                    return index;
                }
                index++;
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

        private static string ReadCString(
            IntPtr processHandle,
            long address,
            int maximumLength
        )
        {
            byte[] buffer;
            if (address <= 0 ||
                !TryReadBytes(processHandle, address, maximumLength, out buffer))
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

        private static bool TryReadUInt32(
            IntPtr processHandle,
            long address,
            out uint value
        )
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

        private static bool TryReadInt32(
            IntPtr processHandle,
            long address,
            out int value
        )
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

        public void Dispose()
        {
            lock (syncRoot)
            {
                if (disposed)
                {
                    return;
                }
                disposed = true;
            }
            Stop();
        }

        private sealed class WorkerContext
        {
            public readonly int ProcessId;
            public volatile bool StopRequested;

            public WorkerContext(int processId)
            {
                ProcessId = processId;
            }
        }

        private sealed class CheatCatalog
        {
            public readonly List<CheatEntry> Entries;
            public readonly HashSet<string> OneShotMessages;
            public readonly int ActionableCount;
            public MemoryRegion? PreferredRegion;

            public CheatCatalog(List<CheatEntry> entries)
            {
                Entries = entries;
                OneShotMessages = new HashSet<string>(StringComparer.Ordinal);
                int count = 0;
                foreach (CheatEntry entry in entries)
                {
                    if (entry.Kind != "label")
                    {
                        count++;
                    }
                    if (entry.Kind == "one-shot")
                    {
                        OneShotMessages.Add(ActivatedPrefix + entry.Description);
                    }
                }
                ActionableCount = count;
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

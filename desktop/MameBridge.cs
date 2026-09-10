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
        public int TrackingSessionSequence { get; set; }
        public string ProcessPath { get; set; }
        public string ProcessVersion { get; set; }
        public string WindowTitle { get; set; }
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public int PlayerOneHealth { get; set; }
        public int PlayerTwoHealth { get; set; }
        public int GamePlayerOneRounds { get; set; }
        public int GamePlayerTwoRounds { get; set; }
        public string RomId { get; set; }
        public string RomLabel { get; set; }
        public string FightState { get; set; }
        public string MatchMode { get; set; }
        public string MatchModeLabel { get; set; }
        public string MatchModeDetails { get; set; }
        public string CheatMonitorState { get; set; }
        public int CheatCatalogCount { get; set; }
        public int CheatUseSequence { get; set; }
        public int ActiveCheatCount { get; set; }
        public string ActiveCheats { get; set; }
        public string LastDetectedCheat { get; set; }
        public string CpuAchievementPolicy { get; set; }
        public string CpuAchievementPolicyDetails { get; set; }
        public bool CpuCheatDetected { get; set; }
        public bool TowerContext { get; set; }
        public string TowerPlayerSide { get; set; }
        public int TowerId { get; set; }
        public string TowerName { get; set; }
        public int TowerFightNumber { get; set; }
        public int PowerBarsEnabled { get; set; }
        public int FinishPhase { get; set; }
        public int TowerStateA { get; set; }
        public int TowerStateB { get; set; }
        public int PlayerTwoControlState { get; set; }
        public int PlayerOneParticipantState { get; set; }
        public int PlayerTwoParticipantState { get; set; }
        public int TeamSizeCode { get; set; }
        public int TeamSize { get; set; }
        public int TeamActivePlayerCode { get; set; }
        public int TeamLeftPlayerCount { get; set; }
        public bool TeamLocalPlayerActive { get; set; }
        public string TeamLocalPlayerSide { get; set; }
        public bool NetplayLayoutSupported { get; set; }
        public bool NetplayActive { get; set; }
        public int NetplayPlayerNumber { get; set; }
        public int NetplayTotalPlayers { get; set; }
        public int NetplayControllerSlot { get; set; }
        public string NetplayControllerOrder { get; set; }
        public int RoundNumber { get; set; }
        public int PlayerOneRounds { get; set; }
        public int PlayerTwoRounds { get; set; }
        public int MatchSequence { get; set; }
        public bool FightMatchFullyObserved { get; set; }
        public bool PlayerOneDangerSeen { get; set; }
        public bool PlayerTwoDangerSeen { get; set; }
        public string RoundWinningHit { get; set; }
        public string WinnerSide { get; set; }
        public string Winner { get; set; }
        public int EventSequence { get; set; }
        public int EventTeamActivePlayerCode { get; set; }
        public int EventTeamLeftPlayerCount { get; set; }
        public int EventTeamControllerSlot { get; set; }
        public string EventKind { get; set; }
        public string EventTitle { get; set; }
        public string EventDetails { get; set; }
        public bool PlayerDataAvailable { get; set; }
        public string MainRamAddress { get; set; }
        public bool FightParticipantsConfirmed { get; set; }
        public string FightPlayerOne { get; set; }
        public string FightPlayerTwo { get; set; }
        public int FightPlayerOneId { get; set; }
        public int FightPlayerTwoId { get; set; }
        public int FinisherSequence { get; set; }
        public string FinisherKind { get; set; }
        public string FinisherWinnerSide { get; set; }
        public int FinisherFighterId { get; set; }
        public int FinisherOpponentId { get; set; }
        public int FinisherTeamActivePlayerCode { get; set; }
        public int FinisherTeamLeftPlayerCount { get; set; }
        public int FinisherTeamControllerSlot { get; set; }
        public string FinisherTitle { get; set; }
        public string FinisherDetails { get; set; }
        public int SpecialSequence { get; set; }
        public string SpecialKind { get; set; }
        public string SpecialActorSide { get; set; }
        public int SpecialTeamActivePlayerCode { get; set; }
        public int SpecialTeamLeftPlayerCount { get; set; }
        public int SpecialTeamControllerSlot { get; set; }
        public string SpecialTitle { get; set; }
        public string SpecialDetails { get; set; }

        public string Signature()
        {
            return string.Join(
                "|",
                State ?? "",
                ProcessId.ToString(),
                TrackingSessionSequence.ToString(),
                Title ?? "",
                Details ?? "",
                PlayerOne ?? "",
                PlayerTwo ?? "",
                PlayerOneId.ToString(),
                PlayerTwoId.ToString(),
                PlayerOneHealth.ToString(),
                PlayerTwoHealth.ToString(),
                GamePlayerOneRounds.ToString(),
                GamePlayerTwoRounds.ToString(),
                RomId ?? "",
                FightState ?? "",
                MatchMode ?? "",
                CheatMonitorState ?? "",
                CheatCatalogCount.ToString(),
                CheatUseSequence.ToString(),
                ActiveCheatCount.ToString(),
                ActiveCheats ?? "",
                LastDetectedCheat ?? "",
                CpuAchievementPolicy ?? "",
                CpuCheatDetected.ToString(),
                TowerContext.ToString(),
                TowerPlayerSide ?? "",
                TowerId.ToString(),
                TowerFightNumber.ToString(),
                PowerBarsEnabled.ToString(),
                FinishPhase.ToString(),
                TowerStateA.ToString(),
                TowerStateB.ToString(),
                PlayerTwoControlState.ToString(),
                PlayerOneParticipantState.ToString(),
                PlayerTwoParticipantState.ToString(),
                TeamSizeCode.ToString(),
                TeamSize.ToString(),
                TeamActivePlayerCode.ToString(),
                TeamLeftPlayerCount.ToString(),
                TeamLocalPlayerActive.ToString(),
                TeamLocalPlayerSide ?? "",
                NetplayLayoutSupported.ToString(),
                NetplayActive.ToString(),
                NetplayPlayerNumber.ToString(),
                NetplayTotalPlayers.ToString(),
                NetplayControllerSlot.ToString(),
                NetplayControllerOrder ?? "",
                RoundNumber.ToString(),
                PlayerOneRounds.ToString(),
                PlayerTwoRounds.ToString(),
                MatchSequence.ToString(),
                FightMatchFullyObserved.ToString(),
                PlayerOneDangerSeen.ToString(),
                PlayerTwoDangerSeen.ToString(),
                RoundWinningHit ?? "",
                WinnerSide ?? "",
                Winner ?? "",
                EventSequence.ToString(),
                EventTeamActivePlayerCode.ToString(),
                EventTeamLeftPlayerCount.ToString(),
                EventTeamControllerSlot.ToString(),
                PlayerDataAvailable.ToString(),
                MainRamAddress ?? "",
                FightParticipantsConfirmed.ToString(),
                FightPlayerOne ?? "",
                FightPlayerTwo ?? "",
                FightPlayerOneId.ToString(),
                FightPlayerTwoId.ToString(),
                FinisherSequence.ToString(),
                FinisherKind ?? "",
                FinisherWinnerSide ?? "",
                FinisherFighterId.ToString(),
                FinisherOpponentId.ToString(),
                FinisherTeamActivePlayerCode.ToString(),
                FinisherTeamLeftPlayerCount.ToString(),
                FinisherTeamControllerSlot.ToString(),
                SpecialSequence.ToString(),
                SpecialKind ?? "",
                SpecialTeamActivePlayerCode.ToString(),
                SpecialTeamLeftPlayerCount.ToString(),
                SpecialTeamControllerSlot.ToString(),
                SpecialActorSide ?? ""
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
        private const int TowerFightNumberOffset = 0x0B77A;
        private const int PlayerOneObjectPointerOffset = 0x0C13A;
        private const int PlayerOneParticipantStateOffset = 0x0C144;
        private const int PlayerOneCharacterOffset = 0x0C146;
        private const int PlayerOneHealthOffset = 0x0C14C;
        private const int PlayerOneRoundsOffset = 0x0C152;
        private const int PlayerTwoControlStateOffset = 0x0C2AA;
        private const int PlayerTwoObjectPointerOffset = 0x0C2B0;
        private const int PlayerTwoParticipantStateOffset = 0x0C2BA;
        private const int PlayerTwoCharacterOffset = 0x0C2BC;
        private const int PlayerTwoHealthOffset = 0x0C2C2;
        private const int PlayerTwoRoundsOffset = 0x0C2C8;
        private const int PowerBarsEnabledOffset = 0x0C424;
        private const int FinisherTypeAOffset = 0x0C426;
        private const int FinisherSuccessOffset = 0x0C442;
        private const int FinishPhaseOffset = 0x0C454;
        private const int FinisherTypeBOffset = 0x0C4E6;
        private const int KombatZoneOffset = 0x0C44C;
        private const int TowerStateAOffset = 0x0C5D7;
        private const int TowerStateBOffset = 0x0C5D9;
        private const int TowerIdOffset = 0x0C5F6;
        private const int TeamSizeCodeOffset = 0x0C5FE;
        private const int TeamActivePlayerCodeOffset = 0x6003F;
        private const uint TmsMainRamAddress = 0x01000000;
        private const int ObjectFlagsOffset = 0x2C;
        private const int ProcessTableOffset = 0x0D442;
        private const int ProcessCount = 94;
        private const int ProcessStride = 0xDE;
        private const int ProcessIdFieldOffset = 0xC2;
        private const int ProcessActionFromIdOffset = 0x3A;
        private const int ProcessHitByFromIdOffset = 0x68;
        private const int ProcessComboDamageFromIdOffset = 0x6A;
        private const int ProcessComboHitsFromIdOffset = 0x6E;
        private const int ProcessArgument11FromIdOffset = 0x10;
        private const int PlayerOneProcessId = 0x0001;
        private const int PlayerTwoProcessId = 0x0002;
        private const int FordenProcessId = 0x010F;
        private const int ToastyArgument = 0x001B;
        private const int FrostyArgument = 0x001C;
        private const int FullHealth = 0xA6;
        private const long MaximumScanRegionSize = 64L * 1024L * 1024L;
        private const uint MainRamByteStart = 0x00200000;
        private const uint MainRamByteEnd = 0x0027FFFF;
        private const uint MainProgramByteMask = 0x1FFFFFFF;
        // MAME++ 0.129 stores the Kaillera SDK gameCallback arguments outside
        // guest RAM. Each known non-ASLR build has a separate fixed layout.
        // The callback signatures prevent applying one layout to another
        // emulator binary which happens to report the same product version.
        private static readonly NetplayLayout[] NetplayLayouts =
        {
            new NetplayLayout(
                0x0076C8F0,
                0x020B5D14,
                0x0232EBE0,
                0x0232EBE4,
                0x0232E990,
                new byte[]
                {
                    0x55, 0x89, 0xE5, 0x57, 0x56, 0x53,
                    0x81, 0xEC, 0x5C, 0x0F, 0x00, 0x00,
                    0x8B, 0x15, 0x14, 0x5D, 0x0B, 0x02
                }
            ),
            new NetplayLayout(
                0x0076CE80,
                0x020BAD34,
                0x02333FF0,
                0x02333FF4,
                0x02333DA0,
                new byte[]
                {
                    0x55, 0xB8, 0x5C, 0x10, 0x00, 0x00,
                    0x89, 0xE5, 0x57, 0x56, 0x53,
                    0xE8, 0xD0, 0x1F, 0x90, 0x00,
                    0x8B, 0x15, 0x34, 0xAD, 0x0B, 0x02
                }
            )
        };

        private static readonly string[] CharacterNames =
        {
            "Kano", "Sonya", "Jax", "Nightwolf", "Unmasked Sub-Zero",
            "Stryker", "Sindel", "Sektor", "Cyrax", "Kung Lao", "Kabal",
            "Sheeva", "Shang Tsung", "Liu Kang", "Smoke", "Kitana", "Jade",
            "Mileena", "Scorpion", "Reptile", "Ermac", "Classic Sub-Zero"
        };

        private readonly object syncRoot = new object();
        private readonly MameFightTracker fightTracker;
        private readonly MameMatchModeTracker matchModeTracker;
        private readonly MameTeamLayoutTracker teamLayoutTracker;
        private readonly MameFinisherTracker finisherTracker;
        private readonly MameSpecialTracker specialTracker;
        private readonly MameCheatMonitor cheatMonitor;
        private readonly MameCpuCheatPolicy cpuCheatPolicy;
        private Timer pollTimer;
        private int pollActive;
        private int processId;
        private IntPtr processHandle;
        private long mainRamAddress;
        private int trackingSessionSequence;
        private int teamLayoutMatchSequence;
        private DateTime nextMemoryScanUtc;
        private bool netplayLayoutChecked;
        private NetplayLayout netplayLayout;
        private MameBridgeSnapshot currentSnapshot;
        private string lastSnapshotSignature;
        private bool disposed;

        public event Action<MameBridgeSnapshot> SnapshotChanged;
        public event Action<MameBridgeSnapshot> SnapshotObserved;

        public MameBridge()
        {
            fightTracker = new MameFightTracker();
            matchModeTracker = new MameMatchModeTracker();
            teamLayoutTracker = new MameTeamLayoutTracker();
            finisherTracker = new MameFinisherTracker();
            specialTracker = new MameSpecialTracker();
            cheatMonitor = new MameCheatMonitor();
            cpuCheatPolicy = new MameCpuCheatPolicy();
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
                pollTimer = new Timer(Poll, null, 0, 250);
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

                NetplayState netplay = ReadNetplayState();

                PlayerState players;
                if (mainRamAddress != 0 && TryReadPlayers(mainRamAddress, out players))
                {
                    return CreateObservedSnapshot(
                        process.Id,
                        processPath,
                        processVersion,
                        windowTitle,
                        players,
                        netplay
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
                        trackingSessionSequence = trackingSessionSequence == int.MaxValue
                            ? 1
                            : trackingSessionSequence + 1;
                        fightTracker.Reset();
                        matchModeTracker.Reset();
                        teamLayoutTracker.Reset();
                        finisherTracker.Reset();
                        specialTracker.Reset();
                        return CreateObservedSnapshot(
                            process.Id,
                            processPath,
                            processVersion,
                            windowTitle,
                            players,
                            netplay
                        );
                    }
                }

                bool gameLooksRecognized = LooksLikeUmk3(windowTitle);
                string detectedRomId = DetectRomId(windowTitle);
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
                    WindowTitle = windowTitle,
                    RomId = detectedRomId,
                    RomLabel = RomLabel(detectedRomId),
                    NetplayLayoutSupported = netplay.LayoutSupported,
                    NetplayActive = netplay.Active,
                    NetplayPlayerNumber = netplay.PlayerNumber,
                    NetplayTotalPlayers = netplay.TotalPlayers,
                    NetplayControllerSlot = netplay.ControllerSlot,
                    NetplayControllerOrder = netplay.ControllerOrder
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
            cheatMonitor.Start(targetProcessId);
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

        private NetplayState ReadNetplayState()
        {
            NetplayState state = new NetplayState
            {
                ControllerSlot = -1,
                ControllerOrder = ""
            };
            if (processHandle == IntPtr.Zero)
            {
                return state;
            }

            if (!netplayLayoutChecked)
            {
                netplayLayout = FindNetplayLayout();
                netplayLayoutChecked = true;
            }
            state.LayoutSupported = netplayLayout != null;
            if (!state.LayoutSupported)
            {
                return state;
            }

            uint active;
            uint playerNumber;
            uint totalPlayers;
            if (!TryReadUInt32(netplayLayout.ActiveAddress, out active) ||
                !TryReadUInt32(
                    netplayLayout.PlayerNumberAddress,
                    out playerNumber
                ) ||
                !TryReadUInt32(
                    netplayLayout.TotalPlayersAddress,
                    out totalPlayers
                ))
            {
                return state;
            }

            state.Active = active == 1;
            if (!state.Active ||
                playerNumber > 8 ||
                totalPlayers < 1 ||
                totalPlayers > 8 ||
                (playerNumber > 0 && playerNumber > totalPlayers))
            {
                return state;
            }

            state.PlayerNumber = checked((int)playerNumber);
            state.TotalPlayers = checked((int)totalPlayers);
            if (state.PlayerNumber > 0)
            {
                TryReadNetplayControllerMapping(state);
            }
            return state;
        }

        private void TryReadNetplayControllerMapping(NetplayState state)
        {
            int count = state.TotalPlayers;
            byte[] buffer = new byte[count * sizeof(int)];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(netplayLayout.ControllerMapAddress),
                buffer,
                buffer.Length,
                out bytesRead
            );
            if (!read || bytesRead.ToInt64() != buffer.Length)
            {
                return;
            }

            int[] controllerMap = new int[count];
            for (int index = 0; index < count; index++)
            {
                controllerMap[index] = BitConverter.ToInt32(
                    buffer,
                    index * sizeof(int)
                );
            }

            string rawControllerMap = string.Join(",", controllerMap);

            int controllerSlot;
            string controllerOrder;
            if (MameTeamPlayerAttribution.TryResolveControllerSlot(
                count,
                state.PlayerNumber,
                controllerMap,
                out controllerSlot,
                out controllerOrder
            ))
            {
                state.ControllerSlot = controllerSlot;
                state.ControllerOrder =
                    "ACTIVE " + controllerOrder +
                    " RAW " + rawControllerMap;
                return;
            }

            // Never substitute room order for an invalid live permutation.
            // Doing so can attribute the active P3 fighter to local P2.
            state.ControllerOrder = "RAW " + rawControllerMap;
        }

        private NetplayLayout FindNetplayLayout()
        {
            foreach (NetplayLayout layout in NetplayLayouts)
            {
                if (NetplayLayoutMatches(layout))
                {
                    return layout;
                }
            }
            return null;
        }

        private bool NetplayLayoutMatches(NetplayLayout layout)
        {
            byte[] actual = new byte[layout.CallbackSignature.Length];
            IntPtr bytesRead;
            bool read = ReadProcessMemory(
                processHandle,
                new IntPtr(layout.CallbackAddress),
                actual,
                actual.Length,
                out bytesRead
            );
            if (!read || bytesRead.ToInt64() != actual.Length)
            {
                return false;
            }

            for (int index = 0; index < actual.Length; index++)
            {
                if (actual[index] != layout.CallbackSignature[index])
                {
                    return false;
                }
            }
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
            byte[] buffer = new byte[MainRamSize];
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

            CombatProcessState combat = ReadCombatProcessState(buffer);
            players = new PlayerState
            {
                PlayerOneId = p1Character,
                PlayerTwoId = p2Character,
                PlayerOne = CharacterName(p1Character),
                PlayerTwo = CharacterName(p2Character),
                PlayerOneHealth = p1Health,
                PlayerTwoHealth = p2Health,
                GamePlayerOneRounds = buffer[PlayerOneRoundsOffset] <= 2
                    ? buffer[PlayerOneRoundsOffset]
                    : -1,
                GamePlayerTwoRounds = buffer[PlayerTwoRoundsOffset] <= 2
                    ? buffer[PlayerTwoRoundsOffset]
                    : -1,
                PowerBarsEnabled = buffer[PowerBarsEnabledOffset],
                TowerStateA = buffer[TowerStateAOffset],
                TowerStateB = buffer[TowerStateBOffset],
                TowerId = buffer[TowerIdOffset],
                TowerFightNumber = buffer[TowerFightNumberOffset],
                PlayerTwoControlState = buffer[PlayerTwoControlStateOffset],
                PlayerOneParticipantState =
                    buffer[PlayerOneParticipantStateOffset],
                PlayerTwoParticipantState =
                    buffer[PlayerTwoParticipantStateOffset],
                TeamSizeCode = buffer[TeamSizeCodeOffset],
                TeamActivePlayerCode = buffer[TeamActivePlayerCodeOffset],
                PlayerOneFlags = ReadObjectFlags(
                    buffer,
                    PlayerOneObjectPointerOffset
                ),
                PlayerTwoFlags = ReadObjectFlags(
                    buffer,
                    PlayerTwoObjectPointerOffset
                ),
                FinishPhase = buffer[FinishPhaseOffset],
                FinisherTypeA = buffer[FinisherTypeAOffset],
                FinisherSuccess = buffer[FinisherSuccessOffset],
                FinisherTypeB = buffer[FinisherTypeBOffset],
                KombatZone = buffer[KombatZoneOffset],
                PlayerOneAction = combat.PlayerOneAction,
                PlayerTwoAction = combat.PlayerTwoAction,
                PlayerOneHitBy = combat.PlayerOneHitBy,
                PlayerTwoHitBy = combat.PlayerTwoHitBy,
                PlayerOneComboDamage = combat.PlayerOneComboDamage,
                PlayerTwoComboDamage = combat.PlayerTwoComboDamage,
                PlayerOneComboHits = combat.PlayerOneComboHits,
                PlayerTwoComboHits = combat.PlayerTwoComboHits,
                FrostyActive = combat.FrostyActive,
                ToastyActive = combat.ToastyActive
            };
            return true;
        }

        private static CombatProcessState ReadCombatProcessState(byte[] ram)
        {
            CombatProcessState state = new CombatProcessState();
            for (int index = 0; index < ProcessCount; index++)
            {
                int processBase = ProcessTableOffset + index * ProcessStride;
                int processIdOffset = processBase + ProcessIdFieldOffset;
                if (processIdOffset + ProcessComboHitsFromIdOffset + 1 >=
                    ram.Length)
                {
                    break;
                }
                int processId = BitConverter.ToUInt16(ram, processIdOffset);
                if (processId == PlayerOneProcessId)
                {
                    ReadPlayerProcess(ram, processIdOffset, state, true);
                }
                else if (processId == PlayerTwoProcessId)
                {
                    ReadPlayerProcess(ram, processIdOffset, state, false);
                }
                else if (processId == FordenProcessId)
                {
                    int argument = BitConverter.ToUInt16(
                        ram,
                        processIdOffset + ProcessArgument11FromIdOffset
                    );
                    state.ToastyActive = state.ToastyActive ||
                        argument == ToastyArgument;
                    state.FrostyActive = state.FrostyActive ||
                        argument == FrostyArgument;
                }
            }
            return state;
        }

        private static void ReadPlayerProcess(
            byte[] ram,
            int processIdOffset,
            CombatProcessState state,
            bool playerOne
        )
        {
            int action = BitConverter.ToUInt16(
                ram,
                processIdOffset + ProcessActionFromIdOffset
            );
            int hitBy = BitConverter.ToUInt16(
                ram,
                processIdOffset + ProcessHitByFromIdOffset
            );
            int comboDamage = BitConverter.ToUInt16(
                ram,
                processIdOffset + ProcessComboDamageFromIdOffset
            );
            int comboHits = BitConverter.ToUInt16(
                ram,
                processIdOffset + ProcessComboHitsFromIdOffset
            );
            if (playerOne)
            {
                state.PlayerOneAction = action;
                state.PlayerOneHitBy = hitBy;
                state.PlayerOneComboDamage = comboDamage;
                state.PlayerOneComboHits = comboHits;
            }
            else
            {
                state.PlayerTwoAction = action;
                state.PlayerTwoHitBy = hitBy;
                state.PlayerTwoComboDamage = comboDamage;
                state.PlayerTwoComboHits = comboHits;
            }
        }

        private static int ReadObjectFlags(byte[] ram, int pointerOffset)
        {
            uint pointer = BitConverter.ToUInt32(ram, pointerOffset);
            if (pointer < TmsMainRamAddress)
            {
                return -1;
            }
            long objectOffset = (pointer - TmsMainRamAddress) / 8;
            if (objectOffset < 0 ||
                objectOffset + ObjectFlagsOffset + 1 >= ram.LongLength)
            {
                return -1;
            }
            return BitConverter.ToUInt16(
                ram,
                checked((int)objectOffset) + ObjectFlagsOffset
            );
        }

        private MameBridgeSnapshot CreateObservedSnapshot(
            int observedProcessId,
            string processPath,
            string processVersion,
            string windowTitle,
            PlayerState players,
            NetplayState netplay
        )
        {
            string romId = DetectRomId(windowTitle);
            string romLabel = RomLabel(romId);
            MameMatchModeResult observedMatchMode =
                MameMatchModeClassifier.Classify(
                    players.PowerBarsEnabled,
                    players.TowerStateA,
                    players.TowerStateB,
                    players.TowerId,
                    players.TowerFightNumber,
                    players.PlayerTwoControlState,
                    players.PlayerOneParticipantState,
                    players.PlayerTwoParticipantState
                );
            int teamLeftPlayerCount = teamLayoutTracker.Observe(
                romId,
                netplay.TotalPlayers,
                players.TeamActivePlayerCode,
                netplay.ControllerOrder
            );
            // In every Kaillera match the sides and round score outlive the
            // currently displayed fighters. This also covers the random-
            // select Kombat code. Offline tower fights remain participant-
            // bound so a new CPU opponent starts a separate match.
            MameFightProgress progress = fightTracker.Observe(
                players.PlayerOneId,
                players.PlayerOne,
                players.PlayerOneHealth,
                players.PlayerTwoId,
                players.PlayerTwo,
                players.PlayerTwoHealth,
                netplay.Active,
                players.TeamActivePlayerCode,
                teamLeftPlayerCount,
                netplay.ControllerSlot,
                netplay.TotalPlayers,
                players.FinishPhase,
                players.GamePlayerOneRounds,
                players.GamePlayerTwoRounds
            );
            if (netplay.TotalPlayers == 3 &&
                progress.MatchSequence != teamLayoutMatchSequence)
            {
                teamLayoutMatchSequence = progress.MatchSequence;
                teamLayoutTracker.Reset();
                teamLeftPlayerCount = teamLayoutTracker.Observe(
                    romId,
                    netplay.TotalPlayers,
                    players.TeamActivePlayerCode,
                    netplay.ControllerOrder
                );
            }
            MameMatchModeResult matchMode = matchModeTracker.Resolve(
                observedMatchMode,
                progress,
                players.PowerBarsEnabled,
                players.TowerStateA,
                players.TowerStateB,
                players.TowerId,
                players.TowerFightNumber,
                players.PlayerTwoControlState,
                players.PlayerOneParticipantState,
                players.PlayerTwoParticipantState
            );
            MameCheatMonitorSnapshot cheatState = cheatMonitor.Snapshot();
            MameCpuCheatPolicyResult cheatPolicy = cpuCheatPolicy.Resolve(
                trackingSessionSequence,
                matchMode.Mode,
                cheatState,
                romId
            );
            int teamSize = string.Equals(
                matchMode.Mode,
                "two-player",
                StringComparison.Ordinal
            )
                ? MameMatchModeClassifier.DecodeTeamSize(players.TeamSizeCode)
                : 0;
            string teamLocalPlayerSide;
            bool teamLocalPlayerActive =
                MameTeamPlayerAttribution.TryResolveActiveSide(
                    romId,
                    netplay.TotalPlayers,
                    netplay.PlayerNumber,
                    netplay.ControllerSlot,
                    players.TeamActivePlayerCode,
                    teamLeftPlayerCount,
                    out teamLocalPlayerSide
                );
            MameFinisherProgress finisher = finisherTracker.Observe(
                romId,
                players.PlayerOneId,
                players.PlayerOne,
                players.PlayerOneHealth,
                players.PlayerOneFlags,
                players.PlayerTwoId,
                players.PlayerTwo,
                players.PlayerTwoHealth,
                players.PlayerTwoFlags,
                players.FinishPhase,
                players.FinisherTypeA,
                players.FinisherSuccess,
                players.FinisherTypeB,
                players.TeamActivePlayerCode,
                teamLeftPlayerCount,
                netplay.ControllerSlot
            );
            MameSpecialProgress special = specialTracker.Observe(
                progress.MatchSequence,
                progress.ParticipantsConfirmed,
                progress.State,
                players.PlayerOneId,
                players.PlayerOneHealth,
                players.PlayerOneAction,
                players.PlayerOneHitBy,
                players.PlayerOneComboDamage,
                players.PlayerOneComboHits,
                players.PlayerTwoId,
                players.PlayerTwoHealth,
                players.PlayerTwoAction,
                players.PlayerTwoHitBy,
                players.PlayerTwoComboDamage,
                players.PlayerTwoComboHits,
                players.KombatZone,
                players.FinishPhase,
                players.FrostyActive,
                players.ToastyActive,
                players.TeamActivePlayerCode,
                teamLeftPlayerCount,
                netplay.ControllerSlot
            );
            string roundWinningHit = "";
            bool waiting = string.Equals(
                progress.State,
                "waiting",
                StringComparison.Ordinal
            );
            bool synchronizing = string.Equals(
                progress.State,
                "synchronizing",
                StringComparison.Ordinal
            );
            string title = synchronizing
                ? "Синхронизация с текущим матчем"
                : waiting
                    ? "Память Ultimate Mortal Kombat 3 подключена"
                    : "Бой отслеживается";
            string details = synchronizing
                ? "Текущий матч начался до подключения и пропущен. " +
                    "Прогресс начнётся со следующего полного матча."
                : waiting
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
                State = synchronizing
                    ? "match-synchronizing"
                    : waiting ? "game-detected" : "data-confirmed",
                Title = title,
                Details = details,
                ProcessId = observedProcessId,
                TrackingSessionSequence = trackingSessionSequence,
                ProcessPath = processPath,
                ProcessVersion = processVersion,
                WindowTitle = windowTitle,
                PlayerOne = players.PlayerOne,
                PlayerTwo = players.PlayerTwo,
                PlayerOneId = players.PlayerOneId,
                PlayerTwoId = players.PlayerTwoId,
                PlayerOneHealth = players.PlayerOneHealth,
                PlayerTwoHealth = players.PlayerTwoHealth,
                GamePlayerOneRounds = players.GamePlayerOneRounds,
                GamePlayerTwoRounds = players.GamePlayerTwoRounds,
                RomId = romId,
                RomLabel = romLabel,
                FightState = progress.State,
                MatchMode = matchMode.Mode,
                MatchModeLabel = matchMode.Label,
                MatchModeDetails = matchMode.Details,
                CheatMonitorState = cheatState.State,
                CheatCatalogCount = cheatState.ActionableCheatCount,
                CheatUseSequence = cheatState.UseSequence,
                ActiveCheatCount = cheatState.ActiveCheatCount,
                ActiveCheats = cheatState.ActiveCheats,
                LastDetectedCheat = cheatState.LastDetectedCheat,
                CpuAchievementPolicy = cheatPolicy.State,
                CpuAchievementPolicyDetails = cheatPolicy.Details,
                CpuCheatDetected = cheatPolicy.CheatDetected,
                TowerContext = matchMode.TowerContext,
                TowerPlayerSide = matchMode.TowerPlayerSide,
                TowerId = matchMode.TowerId,
                TowerName = matchMode.TowerName,
                TowerFightNumber = matchMode.TowerFightNumber,
                PowerBarsEnabled = players.PowerBarsEnabled,
                FinishPhase = players.FinishPhase,
                TowerStateA = players.TowerStateA,
                TowerStateB = players.TowerStateB,
                PlayerTwoControlState = players.PlayerTwoControlState,
                PlayerOneParticipantState =
                    players.PlayerOneParticipantState,
                PlayerTwoParticipantState =
                    players.PlayerTwoParticipantState,
                TeamSizeCode = players.TeamSizeCode,
                TeamSize = teamSize,
                TeamActivePlayerCode = players.TeamActivePlayerCode,
                TeamLeftPlayerCount = teamLeftPlayerCount,
                TeamLocalPlayerActive = teamLocalPlayerActive,
                TeamLocalPlayerSide = teamLocalPlayerSide,
                NetplayLayoutSupported = netplay.LayoutSupported,
                NetplayActive = netplay.Active,
                NetplayPlayerNumber = netplay.PlayerNumber,
                NetplayTotalPlayers = netplay.TotalPlayers,
                NetplayControllerSlot = netplay.ControllerSlot,
                NetplayControllerOrder = netplay.ControllerOrder,
                RoundNumber = progress.RoundNumber,
                PlayerOneRounds = progress.PlayerOneRounds,
                PlayerTwoRounds = progress.PlayerTwoRounds,
                MatchSequence = progress.MatchSequence,
                FightMatchFullyObserved = progress.FullyObserved,
                PlayerOneDangerSeen = progress.PlayerOneDangerSeen,
                PlayerTwoDangerSeen = progress.PlayerTwoDangerSeen,
                RoundWinningHit = roundWinningHit,
                WinnerSide = progress.WinnerSide,
                Winner = progress.Winner,
                EventSequence = progress.EventSequence,
                EventTeamActivePlayerCode =
                    progress.EventTeamActivePlayerCode,
                EventTeamLeftPlayerCount =
                    progress.EventTeamLeftPlayerCount,
                EventTeamControllerSlot =
                    progress.EventTeamControllerSlot,
                EventKind = progress.EventKind,
                EventTitle = progress.EventTitle,
                EventDetails = progress.EventDetails,
                PlayerDataAvailable = true,
                MainRamAddress = "0x" + mainRamAddress.ToString("X"),
                FightParticipantsConfirmed = progress.ParticipantsConfirmed,
                FightPlayerOne = progress.PlayerOne,
                FightPlayerTwo = progress.PlayerTwo,
                FightPlayerOneId = progress.PlayerOneId,
                FightPlayerTwoId = progress.PlayerTwoId,
                FinisherSequence = finisher.Sequence,
                FinisherKind = finisher.Kind,
                FinisherWinnerSide = finisher.WinnerSide,
                FinisherFighterId = finisher.FighterId,
                FinisherOpponentId = finisher.OpponentId,
                FinisherTeamActivePlayerCode =
                    finisher.TeamActivePlayerCode,
                FinisherTeamLeftPlayerCount =
                    finisher.TeamLeftPlayerCount,
                FinisherTeamControllerSlot =
                    finisher.TeamControllerSlot,
                FinisherTitle = finisher.Title,
                FinisherDetails = finisher.Details,
                SpecialSequence = special.Sequence,
                SpecialKind = special.Kind,
                SpecialActorSide = special.ActorSide,
                SpecialTeamActivePlayerCode =
                    special.TeamActivePlayerCode,
                SpecialTeamLeftPlayerCount =
                    special.TeamLeftPlayerCount,
                SpecialTeamControllerSlot =
                    special.TeamControllerSlot,
                SpecialTitle = special.Title,
                SpecialDetails = special.Details
            };
        }

        private static string CharacterName(int characterId)
        {
            if (characterId == 0x18)
            {
                return "Motaro";
            }
            if (characterId == 0x19)
            {
                return "Shao Kahn";
            }
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
            netplayLayoutChecked = false;
            netplayLayout = null;
            teamLayoutMatchSequence = 0;
            fightTracker.Reset();
            matchModeTracker.Reset();
            teamLayoutTracker.Reset();
            finisherTracker.Reset();
            specialTracker.Reset();
            cheatMonitor.Stop();
            cpuCheatPolicy.Reset();
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
                cheatMonitor.Dispose();
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
            public int GamePlayerOneRounds { get; set; }
            public int GamePlayerTwoRounds { get; set; }
            public int PowerBarsEnabled { get; set; }
            public int TowerStateA { get; set; }
            public int TowerStateB { get; set; }
            public int TowerId { get; set; }
            public int TowerFightNumber { get; set; }
            public int PlayerTwoControlState { get; set; }
            public int PlayerOneParticipantState { get; set; }
            public int PlayerTwoParticipantState { get; set; }
            public int TeamSizeCode { get; set; }
            public int TeamActivePlayerCode { get; set; }
            public int PlayerOneFlags { get; set; }
            public int PlayerTwoFlags { get; set; }
            public int FinishPhase { get; set; }
            public int FinisherTypeA { get; set; }
            public int FinisherSuccess { get; set; }
            public int FinisherTypeB { get; set; }
            public int KombatZone { get; set; }
            public int PlayerOneAction { get; set; }
            public int PlayerTwoAction { get; set; }
            public int PlayerOneHitBy { get; set; }
            public int PlayerTwoHitBy { get; set; }
            public int PlayerOneComboDamage { get; set; }
            public int PlayerTwoComboDamage { get; set; }
            public int PlayerOneComboHits { get; set; }
            public int PlayerTwoComboHits { get; set; }
            public bool FrostyActive { get; set; }
            public bool ToastyActive { get; set; }
        }

        private sealed class CombatProcessState
        {
            public CombatProcessState()
            {
                PlayerOneAction = -1;
                PlayerTwoAction = -1;
                PlayerOneHitBy = -1;
                PlayerTwoHitBy = -1;
                PlayerOneComboDamage = -1;
                PlayerTwoComboDamage = -1;
                PlayerOneComboHits = -1;
                PlayerTwoComboHits = -1;
            }

            public int PlayerOneAction { get; set; }
            public int PlayerTwoAction { get; set; }
            public int PlayerOneHitBy { get; set; }
            public int PlayerTwoHitBy { get; set; }
            public int PlayerOneComboDamage { get; set; }
            public int PlayerTwoComboDamage { get; set; }
            public int PlayerOneComboHits { get; set; }
            public int PlayerTwoComboHits { get; set; }
            public bool FrostyActive { get; set; }
            public bool ToastyActive { get; set; }
        }

        private sealed class NetplayState
        {
            public bool LayoutSupported { get; set; }
            public bool Active { get; set; }
            public int PlayerNumber { get; set; }
            public int TotalPlayers { get; set; }
            public int ControllerSlot { get; set; }
            public string ControllerOrder { get; set; }
        }

        private sealed class NetplayLayout
        {
            public NetplayLayout(
                uint callbackAddress,
                uint activeAddress,
                uint playerNumberAddress,
                uint totalPlayersAddress,
                uint controllerMapAddress,
                byte[] callbackSignature
            )
            {
                CallbackAddress = callbackAddress;
                ActiveAddress = activeAddress;
                PlayerNumberAddress = playerNumberAddress;
                TotalPlayersAddress = totalPlayersAddress;
                ControllerMapAddress = controllerMapAddress;
                CallbackSignature = callbackSignature;
            }

            public uint CallbackAddress { get; private set; }
            public uint ActiveAddress { get; private set; }
            public uint PlayerNumberAddress { get; private set; }
            public uint TotalPlayersAddress { get; private set; }
            public uint ControllerMapAddress { get; private set; }
            public byte[] CallbackSignature { get; private set; }
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

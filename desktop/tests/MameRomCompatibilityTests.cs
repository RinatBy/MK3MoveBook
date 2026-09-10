using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MK3MoveBook
{
    internal static class MameRomCompatibilityTests
    {
        private static readonly string[] Roms = { "umk3tm", "umk3tm60", "umk3uk", "umk3uk2" };
        private static readonly string[] Titles = {
            "MAME: UMK3 Team Edition v3.0 (6 Players for kaillera) [umk3tm]",
            "MAME: UMK3 Team Edition v3.0 (60fps) [umk3tm60]",
            "MAME: UMK3 UK [260709] [umk3uk]",
            "MAME: UMK3 UK [260709] (60fps) [umk3uk2]"
        };
        private static int checks;
        private static readonly BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;

        private static int Main(string[] args)
        {
            try
            {
                for (int i = 0; i < Roms.Length; i++)
                {
                    TestFullMatch(i, false, 1);
                    TestFullMatch(i, false, 2);
                    TestFullMatch(i, true, 1);
                    TestFullMatch(i, true, 2);
                    TestPartialMatch(i);
                    TestFullMatch(i, false, 1, true);
                    TestFullMatch(i, false, 2, true);
                    TestPartialMatch(i, true);
                    TestFinishers(Roms[i]);
                    TestGateMatrix(Roms[i]);
                    TestTeamDiagnostics(Roms[i], i < 2);
                }
                ReplayCapture(args[0]);
                string ukCapture = Path.Combine(Path.GetDirectoryName(args[0]), "umk3uk2-cpu-test18.csv");
                ReplayCapture(ukCapture, 3, 0, 1, false);
                // Keep the recorded game stream identical; replace only the separately
                // verified empty-engine input to expose the original CPU gate failure.
                ReplayCapture(ukCapture, 3, 1, 0, true);
                TestDiagnosticReport();
                Console.WriteLine("MameRomCompatibility: " + checks + " checks passed (four ROMs + Test 17/18 game-stream replays).");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static object Nested(string name)
        {
            return Activator.CreateInstance(typeof(MameBridge).GetNestedType(name,
                BindingFlags.NonPublic), true);
        }

        private static void Set(object target, string name, object value)
        {
            target.GetType().GetProperty(name).SetValue(target, value, null);
        }

        private static void SetField(object target, string name, object value)
        {
            target.GetType().GetField(name, Hidden).SetValue(target, value);
        }

        private static object Players(bool online, int local, int h1, int h2, int phase, int s1, int s2)
        {
            object p = Nested("PlayerState");
            Set(p, "PlayerOneId", 2); Set(p, "PlayerTwoId", 0);
            Set(p, "PlayerOne", "Jax"); Set(p, "PlayerTwo", "Kano");
            Set(p, "PlayerOneHealth", h1); Set(p, "PlayerTwoHealth", h2);
            Set(p, "GamePlayerOneRounds", s1); Set(p, "GamePlayerTwoRounds", s2);
            Set(p, "FinishPhase", phase); Set(p, "PowerBarsEnabled", h1 + h2 > 0 ? 1 : 0);
            Set(p, "TowerStateA", online ? 0 : 1); Set(p, "TowerStateB", online ? 0 : 1);
            Set(p, "TowerId", 0); Set(p, "TowerFightNumber", 1);
            Set(p, "PlayerTwoControlState", online || local == 2 ? 1 : 0);
            Set(p, "PlayerOneParticipantState", 6); Set(p, "PlayerTwoParticipantState", 6);
            return p;
        }

        private static void Monitor(MameBridge bridge, string state, int count, int use, int active)
        {
            // Replay the independent cheat monitor's recorded observations;
            // this test never starts a worker or changes emulator memory.
            object monitor = typeof(MameBridge).GetField("cheatMonitor", Hidden).GetValue(bridge);
            SetField(monitor, "state", state);
            SetField(monitor, "actionableCheatCount", count);
            SetField(monitor, "catalogGeneration", 1);
            SetField(monitor, "useSequence", use);
            HashSet<string> names = (HashSet<string>)monitor.GetType()
                .GetField("activeCheats", Hidden).GetValue(monitor);
            names.Clear();
            for (int i = 0; i < active; i++) names.Add("recorded-cheat-" + i);
        }

        private static MameBridgeSnapshot Observe(MameBridge bridge, int rom, object players,
            bool online = false, int local = 1)
        {
            object net = Nested("NetplayState");
            Set(net, "LayoutSupported", true); Set(net, "Active", online);
            Set(net, "PlayerNumber", online ? local : 0);
            Set(net, "TotalPlayers", online ? 2 : 0);
            Set(net, "ControllerSlot", online ? local - 1 : -1);
            return (MameBridgeSnapshot)typeof(MameBridge).GetMethod("CreateObservedSnapshot", Hidden)
                .Invoke(bridge, new object[] { 100, "", "", Titles[rom], players, net });
        }

        private static bool Eligible(MameBridgeSnapshot s)
        {
            return MameAchievementEligibilityPolicy.IsSnapshotEligible(s.RomId,
                s.FightMatchFullyObserved, s.MatchMode, s.CpuAchievementPolicy,
                s.NetplayLayoutSupported, s.NetplayActive, s.NetplayPlayerNumber,
                s.NetplayTotalPlayers, s.TeamSize);
        }

        private static MoveBookAchievementEvent Normalize(MameBridgeSnapshot s)
        {
            MoveBookAchievementEvent e = new MoveBookAchievementEvent {
                EventKey = "test:match:" + s.RomId, RomId = s.RomId,
                EventType = s.EventKind, MatchFullyObserved = s.FightMatchFullyObserved,
                CheatPolicyState = s.CpuAchievementPolicy == "verified-clean" ? "disabled" : "unknown",
                TeamSize = s.TeamSize, NetplayTotalPlayers = s.NetplayTotalPlayers,
                PlayerOneId = s.FightPlayerOneId, PlayerTwoId = s.FightPlayerTwoId,
                PlayerOneHealth = s.PlayerOneHealth, PlayerTwoHealth = s.PlayerTwoHealth,
                PlayerOneRounds = s.PlayerOneRounds, PlayerTwoRounds = s.PlayerTwoRounds,
                WinnerSide = s.WinnerSide
            };
            Check(MameAchievementAttribution.TryNormalizeForProfile(s.MatchMode, s.TowerPlayerSide,
                s.TowerId, s.PlayerTwoId, s.RomId, 0, 0, -1,
                s.NetplayLayoutSupported, s.NetplayActive, s.NetplayPlayerNumber,
                s.NetplayTotalPlayers, e), "normalize " + s.RomId);
            Check(MameAchievementEligibilityPolicy.IsQueuedEventEligible(e), "outbox " + s.RomId);
            return e;
        }

        private static void TestFullMatch(int rom, bool online, int local, bool empty = false)
        {
            using (MameBridge b = new MameBridge())
            {
                Monitor(b, empty ? "ready-empty" : "ready", empty ? 0 : 23, 0, 0);
                if (empty) SetField(typeof(MameBridge).GetField("cheatMonitor", Hidden).GetValue(b), "engineRomId", Roms[rom]);
                Observe(b, rom, Players(online, local, 0, 0, 0, 0, 0), online, local);
                MameBridgeSnapshot s = Observe(b, rom, Players(online, local, 166, 166, 0, 0, 0), online, local);
                Check(s.RomId == Roms[rom] && s.EventKind == "match-started" && Eligible(s), "full start");
                int h1 = local == 1 ? 70 : 0, h2 = local == 1 ? 0 : 70;
                int r1 = local == 1 ? 1 : 0, r2 = local == 1 ? 0 : 1;
                s = Observe(b, rom, Players(online, local, h1, h2, local, r1, r2), online, local);
                Check(s.EventKind == "round-ended" && Eligible(s), "single result sample");
                int sequence = s.EventSequence;
                for (int n = 0; n < 15; n++)
                    s = Observe(b, rom, Players(online, local, h1, h2, local, r1, r2), online, local);
                Check(s.EventSequence == sequence, "held result counted once");
                Observe(b, rom, Players(online, local, 166, 166, 0, r1, r2), online, local);
                s = Observe(b, rom, Players(online, local, h1, h2, 3, r1 * 2, r2 * 2), online, local);
                Check(s.EventKind == "round-started", "finish window alone is not round result");
                s = Observe(b, rom, Players(online, local, h1, h2, local, r1 * 2, r2 * 2), online, local);
                Check(s.EventKind == "match-ended" && Eligible(s), "full match reaches gate");
                MoveBookAchievementEvent e = Normalize(s);
                Check(e.WinnerSide == "p1" && e.PlayerOneId == (local == 1 ? 2 : 0), "local profile attribution");
                Check(e.MatchScope == (online ? "online" : "computer"), "scope");
            }
        }

        private static void TestPartialMatch(int rom, bool empty = false)
        {
            using (MameBridge b = new MameBridge())
            {
                Monitor(b, empty ? "ready-empty" : "ready", empty ? 0 : 23, 0, 0);
                if (empty) SetField(typeof(MameBridge).GetField("cheatMonitor", Hidden).GetValue(b), "engineRomId", Roms[rom]);
                MameBridgeSnapshot s = Observe(b, rom, Players(false, 1, 90, 60, 0, 1, 0));
                Check(s.FightState == "synchronizing" && !Eligible(s), "partial starts blocked");
                s = Observe(b, rom, Players(false, 1, 50, 0, 1, 2, 0));
                Check(!Eligible(s) && s.EventSequence == 0, "partial result produces no event");
                s = Observe(b, rom, Players(false, 1, 166, 166, 0, 0, 0));
                Check(s.EventKind == "match-started" && Eligible(s), "next full match recovers");
                Monitor(b, "ready", 23, 1, 0);
                s = Observe(b, rom, Players(false, 1, 70, 0, 1, 1, 0));
                Check(!Eligible(s) && s.CpuAchievementPolicy == "blocked-cheat", "one-shot cheat blocked");
            }
        }

        private static void TestFinishers(string rom)
        {
            MameFinisherTracker f = new MameFinisherTracker();
            f.Observe(rom, 10, "Kabal", 166, 0x12, 0, "Kano", 0, 0x216, 3, 0, 0, 0);
            MameFinisherProgress s = f.Observe(rom, 10, "Kabal", 166, 0x22, 0, "Kano", 0, 0x216, 3, 0, 0, 0);
            Check(s.Sequence == (rom == "umk3uk" || rom == "umk3uk2" ? 1 : 0), "UK-only antifatality");
            foreach (int code in new int[] { 1, 3, 4, 5, 6 })
            {
                f.Reset();
                f.Observe(rom, 2, "Jax", 90, 0x12, 0, "Kano", 0, 0x216, 3, 0, 0, 0);
                s = f.Observe(rom, 2, "Jax", 90, 0x12, 0, "Kano", 0, 0x216, 3, 1, 1, code);
                Check(s.Sequence == 1 && s.WinnerSide == "p1", "generic finisher " + code);
            }
            f.Reset();
            f.Observe(rom, 2, "Jax", 90, 0x12, 0, "Kano", 0, 0x216, 3, 0, 0, 0);
            Check(f.Observe(rom, 2, "Jax", 90, 0x12, 0, "Kano", 0, 0x216, 3, 2, 0, 0).Kind ==
                "stage-fatality", "stage finisher");
        }

        private static void TestGateMatrix(string rom)
        {
            foreach (string mode in new string[] { "tower-cpu", "two-player", "unknown" })
            foreach (string cheat in new string[] { "verified-clean", "checking", "blocked-cheat" })
            foreach (bool full in new bool[] { false, true })
            foreach (bool online in new bool[] { false, true })
            for (int count = 0; count <= 6; count++)
            for (int team = 0; team <= 3; team++)
            {
                bool expected = full && (mode == "tower-cpu"
                    ? !online && cheat == "verified-clean"
                    : mode == "two-player" && online && count == 2 && team == 1);
                Check(MameAchievementEligibilityPolicy.IsSnapshotEligible(rom, full,
                    mode, cheat, true, online, 1, count, team) == expected, "gate matrix " + rom);
                MoveBookAchievementEvent e = new MoveBookAchievementEvent {
                    RomId = rom, MatchFullyObserved = full, MatchScope = mode == "tower-cpu"
                        ? "computer" : mode == "two-player" && online ? "online" : "",
                    CheatPolicyState = cheat == "verified-clean" ? "disabled" : "unknown",
                    NetplayTotalPlayers = count, TeamSize = team
                };
                bool queued = full && (e.MatchScope == "computer"
                    ? count == 0 && e.CheatPolicyState == "disabled"
                    : e.MatchScope == "online" && count == 2 && team == 1);
                Check(MameAchievementEligibilityPolicy.IsQueuedEventEligible(e) == queued, "outbox matrix");
            }
        }

        private static void TestTeamDiagnostics(string rom, bool teamRom)
        {
            MameTeamLayoutTracker layout = new MameTeamLayoutTracker();
            Check(layout.Observe(rom, 3, 0x10) == (teamRom ? 2 : 0), "three-player layout");
            string side;
            Check(MameTeamPlayerAttribution.TryResolveActiveSide(rom, 6, 2, 0, 0x21, 3, out side)
                == teamRom, "active team diagnostic");
            Check(!MameTeamPlayerAttribution.TryResolveActiveSide(rom, 6, 2, 3, 0x21, 3, out side),
                "waiting team member");
        }

        private static void ReplayCapture(string fixture, int rom = 1,
            int expectedEligible = 23, int expectedBlocked = 1, bool emptyVerified = false)
        {
            string[] lines = File.ReadAllLines(fixture);
            MameBridge b = null;
            string ram = "";
            int lastEvent = 0, eligibleMatches = 0, blockedMatches = 0;
            try
            {
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] v = lines[i].Split(',');
                    if (v[1] != ram)
                    {
                        if (b != null) b.Dispose();
                        b = new MameBridge(); ram = v[1]; lastEvent = 0;
                    }
                    object p = Nested("PlayerState");
                    string[] properties = { "PlayerOneId", "PlayerTwoId", "PlayerOneHealth",
                        "PlayerTwoHealth", "GamePlayerOneRounds", "GamePlayerTwoRounds", "FinishPhase",
                        "PowerBarsEnabled", "TowerStateA", "TowerStateB", "TowerId", "TowerFightNumber",
                        "PlayerTwoControlState", "PlayerOneParticipantState", "PlayerTwoParticipantState",
                        "TeamSizeCode" };
                    for (int j = 0; j < properties.Length; j++)
                    {
                        // The CSV suppresses tower metadata outside a tower.
                        int value = v[j + 2].Length == 0
                            ? (properties[j] == "TowerId" ? -1 : 0)
                            : int.Parse(v[j + 2]);
                        Set(p, properties[j], value);
                    }
                    Set(p, "PlayerOne", "P1"); Set(p, "PlayerTwo", "P2");
                    Monitor(b, emptyVerified ? "ready-empty" : v[18], int.Parse(v[19]), int.Parse(v[20]), int.Parse(v[21]));
                    if (emptyVerified) SetField(typeof(MameBridge).GetField("cheatMonitor", Hidden).GetValue(b), "engineRomId", Roms[rom]);
                    MameBridgeSnapshot s = Observe(b, rom, p);
                    if (s.EventSequence == lastEvent) continue;
                    lastEvent = s.EventSequence;
                    if (s.EventKind != "match-ended") continue;
                    if (Eligible(s)) { eligibleMatches++; Normalize(s); }
                    else blockedMatches++;
                }
            }
            finally { if (b != null) b.Dispose(); }
            Check(eligibleMatches == expectedEligible, "recorded " + Roms[rom] + " CPU matches eligible: " + eligibleMatches);
            Check(blockedMatches == expectedBlocked, "recorded " + Roms[rom] + " matches blocked: " + blockedMatches);
        }

        private static void TestDiagnosticReport()
        {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                "diagnostic-test-" + Guid.NewGuid().ToString("N"));
            MoveBookAchievementDiagnosticReport disabled =
                new MoveBookAchievementDiagnosticReport(dir, false, "");
            Check(disabled.ReportPath == "" && !Directory.Exists(dir), "trace opt-in");
            MoveBookAchievementDiagnosticReport report = new MoveBookAchievementDiagnosticReport(
                dir, true, "https://secret-user:secret-pass@example.invalid/api?token=secret-token");
            MameBridgeSnapshot s = new MameBridgeSnapshot { RomId = "umk3uk2" };
            report.Observe(s, false, false, 0, "1.1.17");
            report.Observe(s, false, false, 0, "1.1.17");
            report.Record("server-response", new MoveBookAchievementEvent { RomId = "umk3uk2" },
                0, "", new MoveBookAchievementSubmitResult {
                    Accepted = true, NewUnlocks = new string[0],
                    Progress = new MoveBookAchievementProgress { Victories = 0, MatchesPlayed = 0 }
                });
            report.Consumed("partial:test:1", "umk3uk2", "match-ended", "partial-match", 0);
            string[] lines = File.ReadAllLines(report.ReportPath);
            Check(lines.Length == 4, "trace deduplicates unchanged observations");
            Check(lines[3].Contains("consumed-without-queue") && lines[3].Contains("partial-match"),
                "trace explains a permanently consumed event");
            string text = string.Join("", lines);
            Check(!text.Contains("secret-") && text.Contains("signed-out") && text.Contains("\"matches\":0"),
                "trace has reasons and server counters, no URL credentials");
            File.Delete(report.ReportPath);
            Directory.Delete(Path.GetDirectoryName(report.ReportPath));
            Directory.Delete(dir);
        }

        private static void Check(bool value, string label)
        {
            checks++;
            if (!value) throw new Exception(label);
        }
    }
}

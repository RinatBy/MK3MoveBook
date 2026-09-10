using System;

namespace MK3MoveBook
{
    internal static class MameCpuCheatPolicyTests
    {
        private static int failures;

        public static int Main()
        {
            TestOnlineIgnoresUnavailableMonitor();
            TestTowerWaitsForCatalog();
            TestCleanTowerIsVerified();
            TestPersistentUseBlocksTower();
            TestActiveCheatBlocksTower();
            TestOnlineUseDoesNotBlockLaterTower();
            TestOnlineActiveCheatBlocksWhenTowerStarts();
            TestBlockLatchesAfterCheatIsDisabled();
            TestNewTrackingSessionClearsOldBlock();
            TestVerifiedEmptyCatalog();

            if (failures == 0)
            {
                Console.WriteLine("MameCpuCheatPolicy: 10 scenarios passed (including four ROM empty-catalog matrix).");
                return 0;
            }
            Console.Error.WriteLine(
                "MameCpuCheatPolicy: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static void TestOnlineIgnoresUnavailableMonitor()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "two-player",
                Monitor("scanning", 0, 0, 0, "")
            );
            Equal("online unavailable", "not-applicable", result.State);
        }

        private static void TestTowerWaitsForCatalog()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("scanning", 0, 0, 0, "")
            );
            Equal("tower scanning", "checking", result.State);
        }

        private static void TestCleanTowerIsVerified()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 0, 0, "")
            );
            Equal("clean tower", "verified-clean", result.State);
        }

        private static void TestPersistentUseBlocksTower()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            policy.Resolve(1, "tower-cpu", Monitor("ready", 1, 0, 0, ""));
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 1, 1, "Infinite Time")
            );
            Equal("persistent use", "blocked-cheat", result.State);
            True("persistent flag", result.CheatDetected);
        }

        private static void TestActiveCheatBlocksTower()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 0, 1, "Infinite Energy PL1")
            );
            Equal("active at attach", "blocked-cheat", result.State);
        }

        private static void TestOnlineUseDoesNotBlockLaterTower()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            policy.Resolve(
                1,
                "two-player",
                Monitor("ready", 1, 1, 0, "Drain all Energy Now! PL2")
            );
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 1, 0, "Drain all Energy Now! PL2")
            );
            Equal("online use ignored", "verified-clean", result.State);
        }

        private static void TestOnlineActiveCheatBlocksWhenTowerStarts()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            policy.Resolve(
                1,
                "two-player",
                Monitor("ready", 1, 1, 1, "Infinite Time")
            );
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 1, 1, "Infinite Time")
            );
            Equal("active entering tower", "blocked-cheat", result.State);
        }

        private static void TestBlockLatchesAfterCheatIsDisabled()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 1, 1, "Infinite Time")
            );
            MameCpuCheatPolicyResult result = policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 1, 0, "Infinite Time")
            );
            Equal("latched block", "blocked-cheat", result.State);
        }

        private static void TestNewTrackingSessionClearsOldBlock()
        {
            MameCpuCheatPolicy policy = new MameCpuCheatPolicy();
            policy.Resolve(
                1,
                "tower-cpu",
                Monitor("ready", 1, 1, 1, "Infinite Time")
            );
            MameCpuCheatPolicyResult result = policy.Resolve(
                2,
                "tower-cpu",
                Monitor("ready", 1, 1, 0, "Infinite Time")
            );
            Equal("new tracking session", "verified-clean", result.State);
        }

        private static void TestVerifiedEmptyCatalog()
        {
            foreach (string rom in new[] { "umk3tm", "umk3tm60", "umk3uk", "umk3uk2" })
            {
                var policy = new MameCpuCheatPolicy();
                var snapshot = Monitor("ready-empty", 1, 0, 0, "");
                snapshot.RomId = rom;
                Equal("verified empty " + rom, "verified-clean", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                Equal("stale ROM", "checking", policy.Resolve(1, "tower-cpu", snapshot, "different").State);
                snapshot.RomId = "";
                Equal("unbound empty", "checking", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                snapshot.RomId = rom; snapshot.State = "scanning";
                Equal("zero count alone", "checking", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                snapshot.State = "ready";
                Equal("ready zero alone", "checking", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                snapshot.State = "ready-empty"; snapshot.ActionableCheatCount = 23;
                Equal("contradictory empty", "checking", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                snapshot.ActionableCheatCount = 0; snapshot.UseSequence = 1;
                Equal("use still blocks", "blocked-cheat", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                snapshot.UseSequence = 0; snapshot.CatalogGeneration = 2;
                Equal("empty does not clear latch", "blocked-cheat", policy.Resolve(1, "tower-cpu", snapshot, rom).State);
                Equal("new session", "verified-clean", policy.Resolve(2, "tower-cpu", snapshot, rom).State);
                snapshot.ActiveCheatCount = 1;
                Equal("active still blocks", "blocked-cheat", policy.Resolve(2, "tower-cpu", snapshot, rom).State);
                Equal("online unaffected", "not-applicable", policy.Resolve(2, "two-player", snapshot, rom).State);
            }
        }

        private static MameCheatMonitorSnapshot Monitor(
            string state,
            int generation,
            int useSequence,
            int activeCount,
            string cheat
        )
        {
            return new MameCheatMonitorSnapshot
            {
                State = state,
                CatalogGeneration = generation,
                ActionableCheatCount = state == "ready" ? 23 : 0,
                UseSequence = useSequence,
                ActiveCheatCount = activeCount,
                ActiveCheats = activeCount > 0 ? cheat : "",
                LastDetectedCheat = cheat,
                LastDetectionKind = "test"
            };
        }

        private static void Equal(string name, string expected, string actual)
        {
            if (expected == actual)
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(
                name + ": expected " + expected + ", got " + actual
            );
        }

        private static void True(string name, bool value)
        {
            if (value)
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(name + ": expected true.");
        }
    }
}

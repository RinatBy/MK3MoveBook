using System;

namespace MK3MoveBook
{
    internal static class MameFinisherTrackerTests
    {
        private static int failures;

        private static int Main()
        {
            TestAntifatalityPlayerOne();
            TestAntifatalityPlayerTwo();
            TestAntifatalityRejectedInTeamEdition();
            TestStageFatality();
            TestStandardFatality();

            if (failures == 0)
            {
                Console.WriteLine("MameFinisherTracker: 5 tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MameFinisherTracker: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static void TestAntifatalityPlayerOne()
        {
            MameFinisherTracker tracker = new MameFinisherTracker();
            Observe(tracker, "umk3uk", 0x0A, 166, 0x0012, 0x10, 0, 0x0216);
            MameFinisherProgress result = Observe(
                tracker,
                "umk3uk",
                0x0A,
                166,
                0x0022,
                0x10,
                0,
                0x0216
            );
            Equal("P1 Antifatality kind", "antifatality", result.Kind);
            Equal("P1 Antifatality side", "p1", result.WinnerSide);
            Equal("P1 Antifatality fighter", 0x0A, result.FighterId);
            Equal("P1 Antifatality sequence", 1, result.Sequence);

            result = Observe(
                tracker,
                "umk3uk",
                0x0A,
                166,
                0x0022,
                0x10,
                0,
                0x0216
            );
            Equal("Antifatality is not repeated", 1, result.Sequence);
        }

        private static void TestAntifatalityPlayerTwo()
        {
            MameFinisherTracker tracker = new MameFinisherTracker();
            Observe(tracker, "umk3uk", 0x10, 0, 0x0016, 0x0A, 166, 0x0212);
            MameFinisherProgress result = Observe(
                tracker,
                "umk3uk",
                0x10,
                0,
                0x0016,
                0x0A,
                166,
                0x0222
            );
            Equal("P2 Antifatality kind", "antifatality", result.Kind);
            Equal("P2 Antifatality side", "p2", result.WinnerSide);
            Equal("P2 Antifatality fighter", 0x0A, result.FighterId);
        }

        private static void TestAntifatalityRejectedInTeamEdition()
        {
            MameFinisherTracker tracker = new MameFinisherTracker();
            Observe(tracker, "umk3tm", 0x0A, 166, 0x0012, 0x10, 0, 0x0216);
            MameFinisherProgress result = Observe(
                tracker,
                "umk3tm",
                0x0A,
                166,
                0x0022,
                0x10,
                0,
                0x0216
            );
            Equal("Team Edition has no Antifatality", 0, result.Sequence);
        }

        private static void TestStageFatality()
        {
            MameFinisherTracker tracker = new MameFinisherTracker();
            Observe(tracker, "umk3uk", 0x02, 166, 0x0012, 0x00, 0, 0x0216);
            MameFinisherProgress result = tracker.Observe(
                "umk3uk",
                0x02,
                "Jax",
                166,
                0x0012,
                0x00,
                "Kano",
                0,
                0x0216,
                0x03,
                0x02,
                0,
                0
            );
            Equal("Stage Fatality kind", "stage-fatality", result.Kind);
        }

        private static void TestStandardFatality()
        {
            MameFinisherTracker tracker = new MameFinisherTracker();
            Observe(tracker, "umk3uk", 0x02, 166, 0x0012, 0x00, 0, 0x0216);
            MameFinisherProgress result = tracker.Observe(
                "umk3uk",
                0x02,
                "Jax",
                166,
                0x0012,
                0x00,
                "Kano",
                0,
                0x0216,
                0x03,
                0x01,
                0x01,
                0x01
            );
            Equal("Standard Fatality kind", "fatality", result.Kind);
        }

        private static MameFinisherProgress Observe(
            MameFinisherTracker tracker,
            string romId,
            int playerOneId,
            int playerOneHealth,
            int playerOneFlags,
            int playerTwoId,
            int playerTwoHealth,
            int playerTwoFlags
        )
        {
            return tracker.Observe(
                romId,
                playerOneId,
                "P1",
                playerOneHealth,
                playerOneFlags,
                playerTwoId,
                "P2",
                playerTwoHealth,
                playerTwoFlags,
                0x03,
                0,
                0,
                0
            );
        }

        private static void Equal(string name, object expected, object actual)
        {
            if (object.Equals(expected, actual))
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(
                name + ": expected " + expected + ", got " + actual + "."
            );
        }
    }
}

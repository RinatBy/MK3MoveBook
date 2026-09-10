using System;

namespace MK3MoveBook
{
    internal static class MameSpecialTrackerTests
    {
        private static int failures;

        private static int Main()
        {
            TestSheevaComboUsesDamageAndHitCounters();
            TestToastyUsesFordenArgumentSignal();
            TestFrostyIsAttributedToTheFreezingPlayer();
            TestUppercutStageTransition();
            TestPersistentSignalsAreNotDuplicated();
            TestSpecialActivePlayerIsFrozen();

            if (failures == 0)
            {
                Console.WriteLine("MameSpecialTracker: 6 tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MameSpecialTracker: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static void TestSheevaComboUsesDamageAndHitCounters()
        {
            MameSpecialTracker tracker = new MameSpecialTracker();
            Observe(tracker);
            Observe(
                tracker,
                playerOneId: 0x0B,
                playerTwoComboDamage: 70,
                playerTwoComboHits: 7
            );
            MameSpecialProgress result = Observe(
                tracker,
                playerOneId: 0x0B
            );
            Equal("Sheeva combo kind", "sheeva-42-combo", result.Kind);
            Equal("Sheeva combo actor", "p1", result.ActorSide);
        }

        private static void TestToastyUsesFordenArgumentSignal()
        {
            MameSpecialTracker tracker = new MameSpecialTracker();
            Observe(tracker, playerOneAction: 0x0116);
            MameSpecialProgress result = Observe(
                tracker,
                playerOneAction: 0x0116,
                toastyActive: true
            );
            Equal("Toasty kind", "toasty", result.Kind);
            Equal("Toasty actor", "p1", result.ActorSide);
        }

        private static void TestFrostyIsAttributedToTheFreezingPlayer()
        {
            MameSpecialTracker tracker = new MameSpecialTracker();
            Observe(tracker);
            MameSpecialProgress result = Observe(
                tracker,
                playerTwoHealth: 8,
                playerTwoAction: 0x0610,
                frostyActive: true
            );
            Equal("Frosty kind", "frosty", result.Kind);
            Equal("Frosty actor", "p1", result.ActorSide);
        }

        private static void TestUppercutStageTransition()
        {
            MameSpecialTracker tracker = new MameSpecialTracker();
            Observe(tracker, kombatZone: 0x0C);
            MameSpecialProgress result = Observe(
                tracker,
                playerTwoHitBy: 0x010E,
                kombatZone: 0x0D
            );
            Equal(
                "stage transition kind",
                "stage-transition-uppercut",
                result.Kind
            );
            Equal("stage transition actor", "p1", result.ActorSide);
        }

        private static void TestPersistentSignalsAreNotDuplicated()
        {
            MameSpecialTracker tracker = new MameSpecialTracker();
            Observe(tracker, playerOneAction: 0x0116);
            MameSpecialProgress first = Observe(
                tracker,
                playerOneAction: 0x0116,
                toastyActive: true
            );
            MameSpecialProgress second = Observe(
                tracker,
                playerOneAction: 0x0116,
                toastyActive: true
            );
            Equal("persistent Toasty first sequence", 1, first.Sequence);
            Equal("persistent Toasty unchanged", 1, second.Sequence);
        }

        private static void TestSpecialActivePlayerIsFrozen()
        {
            MameSpecialTracker tracker = new MameSpecialTracker();
            Observe(
                tracker,
                playerOneAction: 0x0116,
                teamActivePlayerCode: 0x21,
                teamLeftPlayerCount: 2
            );
            MameSpecialProgress detected = Observe(
                tracker,
                playerOneAction: 0x0116,
                toastyActive: true,
                teamActivePlayerCode: 0x21,
                teamLeftPlayerCount: 2
            );
            Equal("special active player at detection", 0x21,
                detected.TeamActivePlayerCode);
            Equal("special team layout at detection", 2,
                detected.TeamLeftPlayerCount);

            MameSpecialProgress later = Observe(
                tracker,
                playerOneAction: 0x0116,
                toastyActive: true,
                teamActivePlayerCode: 0x00,
                teamLeftPlayerCount: 1
            );
            Equal("special active player remains frozen", 0x21,
                later.TeamActivePlayerCode);
            Equal("special team layout remains frozen", 2,
                later.TeamLeftPlayerCount);
        }

        private static MameSpecialProgress Observe(
            MameSpecialTracker tracker,
            int playerOneId = 0x02,
            int playerOneHealth = 166,
            int playerOneAction = 0,
            int playerOneHitBy = 0,
            int playerOneComboDamage = 0,
            int playerOneComboHits = 0,
            int playerTwoId = 0x12,
            int playerTwoHealth = 166,
            int playerTwoAction = 0,
            int playerTwoHitBy = 0,
            int playerTwoComboDamage = 0,
            int playerTwoComboHits = 0,
            int kombatZone = 0x0C,
            int finishPhase = 0,
            bool frostyActive = false,
            bool toastyActive = false,
            int teamActivePlayerCode = 0,
            int teamLeftPlayerCount = 0
        )
        {
            return tracker.Observe(
                1,
                true,
                "round-active",
                playerOneId,
                playerOneHealth,
                playerOneAction,
                playerOneHitBy,
                playerOneComboDamage,
                playerOneComboHits,
                playerTwoId,
                playerTwoHealth,
                playerTwoAction,
                playerTwoHitBy,
                playerTwoComboDamage,
                playerTwoComboHits,
                kombatZone,
                finishPhase,
                frostyActive,
                toastyActive,
                teamActivePlayerCode,
                teamLeftPlayerCount
            );
        }

        private static void Equal<T>(string name, T expected, T actual)
        {
            if (!object.Equals(expected, actual))
            {
                failures++;
                Console.Error.WriteLine(
                    name + ": expected " + expected + ", actual " + actual
                );
            }
        }
    }
}

using System;
using System.Collections.Generic;

namespace MK3MoveBook
{
    internal static class MameMatchModeTrackerTests
    {
        private static int failures;

        private static int Main()
        {
            TestRecoversCompletedShaoKahnMatch();
            TestDoesNotRecoverWithoutConfirmedMode();
            TestDoesNotRecoverChangedParticipants();
            TestDoesNotRecoverInvalidCurrentSignals();
            TestWaitingStateClearsConfirmedMode();

            if (failures == 0)
            {
                Console.WriteLine("MameMatchModeTracker: 5 tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MameMatchModeTracker: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static void TestRecoversCompletedShaoKahnMatch()
        {
            MameMatchModeTracker tracker = ConfirmCpuTower();
            MameMatchModeResult result = tracker.Resolve(
                ObserveCpuTower(0, 6, 6),
                CompletedFight(11, 0x19),
                0,
                1,
                1,
                3,
                1,
                0,
                6,
                6
            );
            Equal("Shao Kahn completion mode", "tower-cpu", result.Mode);
            Equal("Shao Kahn completion side", "p1", result.TowerPlayerSide);
        }

        private static void TestDoesNotRecoverWithoutConfirmedMode()
        {
            MameMatchModeTracker tracker = new MameMatchModeTracker();
            MameMatchModeResult result = ResolveHiddenCompletion(
                tracker,
                CompletedFight(11, 0x19),
                6,
                6
            );
            Equal("unconfirmed completion", "unknown", result.Mode);
        }

        private static void TestDoesNotRecoverChangedParticipants()
        {
            MameMatchModeTracker tracker = ConfirmCpuTower();
            MameMatchModeResult result = ResolveHiddenCompletion(
                tracker,
                CompletedFight(11, 0x18),
                6,
                6
            );
            Equal("changed participant completion", "unknown", result.Mode);
        }

        private static void TestDoesNotRecoverInvalidCurrentSignals()
        {
            MameMatchModeTracker tracker = ConfirmCpuTower();
            MameMatchModeResult result = ResolveHiddenCompletion(
                tracker,
                CompletedFight(11, 0x19),
                9,
                6
            );
            Equal("invalid participant completion", "unknown", result.Mode);
        }

        private static void TestWaitingStateClearsConfirmedMode()
        {
            MameMatchModeTracker tracker = ConfirmCpuTower();
            tracker.Resolve(
                ObserveCpuTower(0, 6, 6),
                Fight("waiting", "", false, 11, 0x19),
                0,
                1,
                1,
                3,
                1,
                0,
                6,
                6
            );
            MameMatchModeResult result = ResolveHiddenCompletion(
                tracker,
                CompletedFight(11, 0x19),
                6,
                6
            );
            Equal("completion after waiting", "unknown", result.Mode);
        }

        private static MameMatchModeTracker ConfirmCpuTower()
        {
            MameMatchModeTracker tracker = new MameMatchModeTracker();
            tracker.Resolve(
                ObserveCpuTower(1, 6, 6),
                Fight("round-active", "match-started", true, 11, 0x19),
                1,
                1,
                1,
                3,
                1,
                0,
                6,
                6
            );
            return tracker;
        }

        private static MameMatchModeResult ResolveHiddenCompletion(
            MameMatchModeTracker tracker,
            MameFightProgress fight,
            int playerOneParticipantState,
            int playerTwoParticipantState
        )
        {
            return tracker.Resolve(
                ObserveCpuTower(
                    0,
                    playerOneParticipantState,
                    playerTwoParticipantState
                ),
                fight,
                0,
                1,
                1,
                3,
                1,
                0,
                playerOneParticipantState,
                playerTwoParticipantState
            );
        }

        private static MameMatchModeResult ObserveCpuTower(
            int powerBarsEnabled,
            int playerOneParticipantState,
            int playerTwoParticipantState
        )
        {
            return MameMatchModeClassifier.Classify(
                powerBarsEnabled,
                1,
                1,
                3,
                1,
                0,
                playerOneParticipantState,
                playerTwoParticipantState
            );
        }

        private static MameFightProgress CompletedFight(int p1Id, int p2Id)
        {
            return Fight("match-ended", "match-ended", true, p1Id, p2Id);
        }

        private static MameFightProgress Fight(
            string state,
            string eventKind,
            bool participantsConfirmed,
            int p1Id,
            int p2Id
        )
        {
            return new MameFightProgress
            {
                State = state,
                EventKind = eventKind,
                ParticipantsConfirmed = participantsConfirmed,
                PlayerOneId = p1Id,
                PlayerTwoId = p2Id
            };
        }

        private static void Equal<T>(string name, T expected, T actual)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(
                name + ": expected " + expected + ", got " + actual
            );
        }
    }
}

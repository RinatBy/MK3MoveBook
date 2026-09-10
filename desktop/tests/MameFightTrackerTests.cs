using System;

namespace MK3MoveBook
{
    internal static class MameFightTrackerTests
    {
        private static int failures;

        private static int Main()
        {
            TestDangerAndRoundWinnerArePreservedOnTheEvent();
            TestOpponentChangeBetweenRoundsStartsANewMatch();
            TestOnlineOpponentChangePreservesTheMatchScore();
            TestOnlineRandomFighterIsFrozenAtTheKnockout();
            TestOnlineTeamSwitchContinuesTheSameRound();
            TestOnlineTeamFinalMemberEndsTheRound();
            TestFinishPhaseKeepsCpuEnduranceInOneRound();
            TestFinishPhaseTwoAwardsPlayerTwoRound();
            TestRepeatedResultPhaseCountsOnlyOnce();
            TestLiveControllerPositionIsFrozenAtRoundResult();
            TestFinalPhaseWaitsForConfirmedPlayerOneResult();
            TestFinalPhaseWaitsForConfirmedPlayerTwoResult();
            TestTowerOpponentIsFrozenThroughTheRoundEnd();
            TestAttachBeforeMatchTracksTheFirstFullMatch();
            TestAttachDuringSecondRoundSkipsUntilNextMatch();
            TestAttachAtFirstRoundKnockoutAlsoSkipsTheMatch();

            if (failures == 0)
            {
                Console.WriteLine("MameFightTracker: all tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MameFightTracker: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static void TestOpponentChangeBetweenRoundsStartsANewMatch()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(0x03, "Nightwolf", 166, 0x12, "Scorpion", 166);
            tracker.Observe(0x03, "Nightwolf", 166, 0x12, "Scorpion", 0);
            tracker.Observe(0x03, "Nightwolf", 166, 0x12, "Scorpion", 0);
            MameFightProgress scorpionRound = tracker.Observe(
                0x03,
                "Nightwolf",
                166,
                0x12,
                "Scorpion",
                0
            );
            Equal("transient round event", "round-ended", scorpionRound.EventKind);
            Equal("transient score P1", 1, scorpionRound.PlayerOneRounds);

            MameFightProgress motaroStart = tracker.Observe(
                0x03,
                "Nightwolf",
                166,
                0x18,
                "Motaro",
                166
            );
            Equal("new opponent event", "match-started", motaroStart.EventKind);
            Equal("new opponent match sequence", 2, motaroStart.MatchSequence);
            Equal("new opponent round", 1, motaroStart.RoundNumber);
            Equal("new opponent score P1", 0, motaroStart.PlayerOneRounds);
            Equal("new opponent ID", 0x18, motaroStart.PlayerTwoId);
            Equal("new opponent name", "Motaro", motaroStart.PlayerTwo);
        }

        private static void TestDangerAndRoundWinnerArePreservedOnTheEvent()
        {
            MameFightTracker tracker = new MameFightTracker();
            MameFightProgress start = tracker.Observe(
                0x02,
                "Jax",
                166,
                0x12,
                "Scorpion",
                166
            );
            Equal("match sequence", 1, start.MatchSequence);

            tracker.Observe(0x02, "Jax", 5, 0x12, "Scorpion", 40);
            tracker.Observe(0x02, "Jax", 5, 0x12, "Scorpion", 0);
            tracker.Observe(0x02, "Jax", 5, 0x12, "Scorpion", 0);
            MameFightProgress ended = tracker.Observe(
                0x02,
                "Jax",
                5,
                0x12,
                "Scorpion",
                0
            );
            Equal("round event", "round-ended", ended.EventKind);
            Equal("round winner", "p1", ended.WinnerSide);
            Equal("danger preserved", true, ended.PlayerOneDangerSeen);
            Equal("round number", 1, ended.RoundNumber);
            Equal("score P1", 1, ended.PlayerOneRounds);

            MameFightProgress secondRound = tracker.Observe(
                0x02,
                "Jax",
                166,
                0x12,
                "Scorpion",
                166
            );
            Equal("danger reset", false, secondRound.PlayerOneDangerSeen);
            Equal("winner reset", "", secondRound.WinnerSide);
            Equal("second round", 2, secondRound.RoundNumber);
            Equal("same match group", 1, secondRound.MatchSequence);
        }

        private static void TestOnlineOpponentChangePreservesTheMatchScore()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 166, true, 0x00
            );
            tracker.Observe(0x02, "Jax", 166, 0x00, "Kano", 0, true, 0x00);
            tracker.Observe(0x02, "Jax", 166, 0x00, "Kano", 0, true, 0x00);
            tracker.Observe(0x02, "Jax", 166, 0x00, "Kano", 0, true, 0x00);

            MameFightProgress secondRound = tracker.Observe(
                0x02, "Jax", 166, 0x01, "Sonya", 166, true, 0x01
            );
            Equal("online next fighter event", "round-started", secondRound.EventKind);
            Equal("online same match", 1, secondRound.MatchSequence);
            Equal("online second round", 2, secondRound.RoundNumber);
            Equal("online preserved score", 1, secondRound.PlayerOneRounds);
            Equal("online current opponent", 0x01, secondRound.PlayerTwoId);

            tracker.Observe(0x02, "Jax", 166, 0x01, "Sonya", 0, true, 0x01);
            tracker.Observe(0x02, "Jax", 166, 0x01, "Sonya", 0, true, 0x01);
            MameFightProgress matchEnd = tracker.Observe(
                0x02, "Jax", 166, 0x01, "Sonya", 0, true, 0x01
            );
            Equal("online match end", "match-ended", matchEnd.EventKind);
            Equal("online final score P1", 2, matchEnd.PlayerOneRounds);
            Equal("online final score P2", 0, matchEnd.PlayerTwoRounds);
            Equal("online decisive opponent", "Sonya", matchEnd.PlayerTwo);
            Equal("online decisive active player", 0x01,
                matchEnd.EventTeamActivePlayerCode);
        }

        private static void TestOnlineRandomFighterIsFrozenAtTheKnockout()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 166, true, 0x00
            );
            tracker.Observe(
                0x12, "Scorpion", 120, 0x01, "Sonya", 90, true, 0x11
            );
            tracker.Observe(
                0x12, "Scorpion", 120, 0x01, "Sonya", 0, true, 0x11, 2
            );
            tracker.Observe(
                0x00, "Kano", 120, 0x11, "Mileena", 0, true, 0x22, 1
            );
            MameFightProgress ended = tracker.Observe(
                0x00, "Kano", 120, 0x11, "Mileena", 0, true, 0x22, 1
            );

            Equal("random event", "round-ended", ended.EventKind);
            Equal("random knockout fighter", "Scorpion", ended.PlayerOne);
            Equal("random knockout opponent", "Sonya", ended.PlayerTwo);
            Equal("random active player frozen", 0x11,
                ended.EventTeamActivePlayerCode);
            Equal("random team layout frozen", 2,
                ended.EventTeamLeftPlayerCount);
        }

        private static void TestOnlineTeamSwitchContinuesTheSameRound()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x03, "Nightwolf", 166, 0x12, "Scorpion", 166,
                true, 0x00, 2, 0, 3
            );
            tracker.Observe(
                0x03, "Nightwolf", 5, 0x12, "Scorpion", 54,
                true, 0x00, 2, 0, 3
            );
            tracker.Observe(
                0x03, "Nightwolf", 0, 0x12, "Scorpion", 54,
                true, 0x00, 2, 0, 3
            );
            tracker.Observe(
                0x03, "Nightwolf", 0, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );
            tracker.Observe(
                0x03, "Nightwolf", 0, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );
            MameFightProgress replacement = tracker.Observe(
                0x0D, "Liu Kang", 166, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );

            Equal("team switch state", "round-active", replacement.State);
            Equal("team switch same round", 1, replacement.RoundNumber);
            Equal("team switch P1 score", 0, replacement.PlayerOneRounds);
            Equal("team switch P2 score", 0, replacement.PlayerTwoRounds);
            Equal("team switch current fighter", "Liu Kang",
                replacement.PlayerOne);
            Equal("team switch no knockout event", 1,
                replacement.EventSequence);
        }

        private static void TestOnlineTeamFinalMemberEndsTheRound()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x0D, "Liu Kang", 166, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );
            tracker.Observe(
                0x0D, "Liu Kang", 0, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );
            tracker.Observe(
                0x0D, "Liu Kang", 0, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );
            MameFightProgress ended = tracker.Observe(
                0x0D, "Liu Kang", 0, 0x12, "Scorpion", 54,
                true, 0x10, 2, 1, 3
            );

            Equal("team final event", "round-ended", ended.EventKind);
            Equal("team final loser", "Liu Kang", ended.PlayerOne);
            Equal("team final opponent", "Scorpion", ended.PlayerTwo);
            Equal("team final winner", "Scorpion", ended.Winner);
            Equal("team final P2 score", 1, ended.PlayerTwoRounds);
            Equal("team final active player frozen", 0x10,
                ended.EventTeamActivePlayerCode);
            Equal("team final layout detected", 2,
                ended.EventTeamLeftPlayerCount);
            Equal("team final dynamic slot frozen", 1,
                ended.EventTeamControllerSlot);
        }

        private static void TestFinishPhaseKeepsCpuEnduranceInOneRound()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x00, "Kano", 166, 0x0F, "Kitana", 166,
                false, 0, 0, -1, 0, 0x00
            );
            tracker.Observe(
                0x00, "Kano", 166, 0x0F, "Kitana", 0,
                false, 0, 0, -1, 0, 0x00
            );
            tracker.Observe(
                0x00, "Kano", 166, 0x0F, "Kitana", 0,
                false, 0, 0, -1, 0, 0x00
            );
            MameFightProgress waitingForReplacement = tracker.Observe(
                0x00, "Kano", 166, 0x0F, "Kitana", 0,
                false, 0, 0, -1, 0, 0x00
            );
            Equal("endurance zero health keeps round active", "round-active",
                waitingForReplacement.State);
            Equal("endurance zero health does not score", 0,
                waitingForReplacement.PlayerOneRounds);

            MameFightProgress replacement = tracker.Observe(
                0x00, "Kano", 166, 0x12, "Scorpion", 166,
                false, 0, 0, -1, 0, 0x00
            );
            Equal("endurance replacement same round", 1,
                replacement.RoundNumber);
            Equal("endurance replacement fighter", "Scorpion",
                replacement.PlayerTwo);

            MameFightProgress actualRoundEnd = tracker.Observe(
                0x00, "Kano", 166, 0x12, "Scorpion", 42,
                false, 0, 0, -1, 0, 0x01
            );
            Equal("endurance phase ends round", "round-ended",
                actualRoundEnd.State);
            Equal("endurance phase scores P1", 1,
                actualRoundEnd.PlayerOneRounds);

            MameFightProgress nextRound = tracker.Observe(
                0x00, "Kano", 166, 0x0F, "Kitana", 166,
                false, 0, 0, -1, 0, 0x00
            );
            Equal("endurance next actual round", 2, nextRound.RoundNumber);
            Equal("endurance same match sequence", 1,
                nextRound.MatchSequence);
            Equal("endurance score preserved", 1,
                nextRound.PlayerOneRounds);
        }

        private static void TestFinishPhaseTwoAwardsPlayerTwoRound()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );
            tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 0,
                false, 0, 0, -1, 0, 0x01
            );
            tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );
            MameFightProgress ended = tracker.Observe(
                0x01, "Sonya", 0, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x02
            );

            Equal("phase 02 ends round", "round-ended", ended.State);
            Equal("phase 02 P1 score", 1, ended.PlayerOneRounds);
            Equal("phase 02 P2 score", 1, ended.PlayerTwoRounds);
            Equal("phase 02 winner", "p2", ended.WinnerSide);

            MameFightProgress decidingRound = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );
            Equal("phase 02 deciding round starts", 3,
                decidingRound.RoundNumber);
        }

        private static void TestFinalPhaseWaitsForConfirmedPlayerOneResult()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 166,
                true, 0x21, 2, 1, 3, 0x00
            );
            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 40,
                true, 0x21, 2, 1, 3, 0x01
            );
            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 166,
                true, 0x21, 2, 1, 3, 0x00
            );

            MameFightProgress pending = tracker.Observe(
                0x12, "Scorpion", 0, 0x01, "Sonya", 166,
                true, 0x00, 1, 0, 3, 0x03
            );
            Equal("phase 03 keeps final round active", "round-active",
                pending.State);
            Equal("phase 03 does not change score", 1,
                pending.PlayerOneRounds);
            Equal("phase 03 does not end match", "round-started",
                pending.EventKind);

            MameFightProgress confirmed = tracker.Observe(
                0x12, "Scorpion", 0, 0x01, "Sonya", 166,
                true, 0x00, 1, 0, 3, 0x01
            );
            Equal("phase 01 confirms final match", "match-ended",
                confirmed.EventKind);
            Equal("phase 01 ignores contradictory health", "p1",
                confirmed.WinnerSide);
            Equal("result keeps last combat P1 fighter", 0x02,
                confirmed.PlayerOneId);
            Equal("result keeps last combat P2 fighter", 0x00,
                confirmed.PlayerTwoId);
            Equal("result keeps last combat active player", 0x21,
                confirmed.EventTeamActivePlayerCode);
            Equal("result keeps last combat team layout", 2,
                confirmed.EventTeamLeftPlayerCount);
            Equal("result keeps last combat controller slot", 1,
                confirmed.EventTeamControllerSlot);
        }

        private static void TestRepeatedResultPhaseCountsOnlyOnce()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );
            MameFightProgress firstResult = tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 0,
                false, 0, 0, -1, 0, 0x01
            );
            int firstEventSequence = firstResult.EventSequence;
            Equal("first PHASE 01 adds one point", 1,
                firstResult.PlayerOneRounds);

            MameFightProgress repeatedResult = tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 0,
                false, 0, 0, -1, 0, 0x01
            );
            Equal("repeated PHASE 01 does not add a point", 1,
                repeatedResult.PlayerOneRounds);
            Equal("repeated PHASE 01 does not create an event",
                firstEventSequence, repeatedResult.EventSequence);

            tracker.Observe(
                0x02, "Jax", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );
            MameFightProgress matchEnd = tracker.Observe(
                0x02, "Jax", 0, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x01
            );
            Equal("second distinct PHASE 01 ends match", "match-ended",
                matchEnd.EventKind);
            Equal("two distinct PHASE 01 results make score 2", 2,
                matchEnd.PlayerOneRounds);
            Equal("result phase overrides health", "p1",
                matchEnd.WinnerSide);
        }

        private static void TestLiveControllerPositionIsFrozenAtRoundResult()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x00, "Kano", 166, 0x02, "Jax", 166,
                true, 0x12, 3, 0, 6, 0x00
            );
            MameFightProgress p2Round = tracker.Observe(
                0x00, "Kano", 40, 0x02, "Jax", 0,
                true, 0x12, 3, 2, 6, 0x01
            );
            Equal("active P2 position frozen from combat", 0,
                p2Round.EventTeamControllerSlot);

            tracker.Observe(
                0x12, "Scorpion", 166, 0x10, "Unmasked Sub-Zero", 166,
                true, 0x12, 3, 3, 6, 0x00
            );
            tracker.Observe(
                0x12, "Scorpion", 53, 0x10, "Unmasked Sub-Zero", 0,
                true, 0x12, 3, 1, 6, 0x03
            );
            MameFightProgress p3Result = tracker.Observe(
                0x12, "Scorpion", 53, 0x10, "Unmasked Sub-Zero", 0,
                true, 0x12, 3, 1, 6, 0x01
            );
            Equal("waiting P2 position frozen while P3 fights", 3,
                p3Result.EventTeamControllerSlot);
        }

        private static void TestFinalPhaseWaitsForConfirmedPlayerTwoResult()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );
            tracker.Observe(
                0x01, "Sonya", 20, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x02
            );
            tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00
            );

            MameFightProgress pending = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 0,
                false, 0, 0, -1, 0, 0x03
            );
            Equal("P2 phase 03 does not end match", "round-active",
                pending.State);
            Equal("P2 phase 03 keeps score", 1,
                pending.PlayerTwoRounds);

            MameFightProgress confirmed = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 0,
                false, 0, 0, -1, 0, 0x02
            );
            Equal("phase 02 confirms final match", "match-ended",
                confirmed.EventKind);
            Equal("phase 02 ignores contradictory health", "p2",
                confirmed.WinnerSide);
            Equal("phase 02 final score", 2,
                confirmed.PlayerTwoRounds);
        }

        private static void TestTowerOpponentIsFrozenThroughTheRoundEnd()
        {
            MameFightTracker tracker = new MameFightTracker();
            tracker.Observe(0x03, "Nightwolf", 166, 0x18, "Motaro", 166);
            tracker.Observe(0x03, "Nightwolf", 166, 0x19, "Shao Kahn", 0);
            tracker.Observe(0x03, "Nightwolf", 166, 0x19, "Shao Kahn", 0);
            MameFightProgress ended = tracker.Observe(
                0x03, "Nightwolf", 166, 0x19, "Shao Kahn", 0
            );

            Equal("tower frozen opponent ID", 0x18, ended.PlayerTwoId);
            Equal("tower frozen opponent", "Motaro", ended.PlayerTwo);
        }

        private static void TestAttachBeforeMatchTracksTheFirstFullMatch()
        {
            MameFightTracker tracker = new MameFightTracker();
            MameFightProgress waiting = tracker.Observe(
                0x01, "Sonya", 0, 0x00, "Kano", 0,
                false, 0, 0, -1, 0, 0x00, 0, 0
            );
            Equal("pre-match waits", "waiting", waiting.State);
            Equal("pre-match is not complete evidence", false,
                waiting.FullyObserved);

            MameFightProgress start = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                false, 0, 0, -1, 0, 0x00, 0, 0
            );
            Equal("full match starts", "match-started", start.EventKind);
            Equal("full match sequence", 1, start.MatchSequence);
            Equal("full match evidence", true, start.FullyObserved);
        }

        private static void TestAttachDuringSecondRoundSkipsUntilNextMatch()
        {
            MameFightTracker tracker = new MameFightTracker();
            MameFightProgress partial = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                true, 0, 0, 0, 2, 0x00, 1, 0
            );
            Equal("partial match synchronizes", "synchronizing", partial.State);
            Equal("partial match has no sequence", 0, partial.MatchSequence);
            Equal("partial match has no event", 0, partial.EventSequence);
            Equal("partial match is ineligible", false, partial.FullyObserved);

            tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 0,
                true, 0, 0, 0, 2, 0x01, 2, 0
            );
            MameFightProgress next = tracker.Observe(
                0x03, "Nightwolf", 166, 0x12, "Scorpion", 166,
                true, 0, 0, 0, 2, 0x00, 0, 0
            );
            Equal("next match starts after score reset", "match-started",
                next.EventKind);
            Equal("next match sequence", 1, next.MatchSequence);
            Equal("next match is fully observed", true, next.FullyObserved);
            Equal("next match begins at round one", 1, next.RoundNumber);
        }

        private static void TestAttachAtFirstRoundKnockoutAlsoSkipsTheMatch()
        {
            MameFightTracker tracker = new MameFightTracker();
            MameFightProgress knockout = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 0,
                true, 0, 0, 0, 2, 0x00, 0, 0
            );
            Equal("knockout attach synchronizes", "synchronizing",
                knockout.State);

            MameFightProgress secondRound = tracker.Observe(
                0x01, "Sonya", 166, 0x00, "Kano", 166,
                true, 0, 0, 0, 2, 0x00, 1, 0
            );
            Equal("second round remains skipped", "synchronizing",
                secondRound.State);
            Equal("skipped match still has no event", 0,
                secondRound.EventSequence);

            MameFightProgress next = tracker.Observe(
                0x03, "Nightwolf", 166, 0x12, "Scorpion", 166,
                true, 0, 0, 0, 2, 0x00, 0, 0
            );
            Equal("post-knockout next match starts", "match-started",
                next.EventKind);
            Equal("post-knockout next match is full", true,
                next.FullyObserved);
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

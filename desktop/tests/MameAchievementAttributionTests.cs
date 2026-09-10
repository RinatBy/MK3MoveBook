using System;

namespace MK3MoveBook
{
    internal static class MameAchievementAttributionTests
    {
        private static int failures;

        private static int Main()
        {
            TestTowerEventIsAllowedUnchanged();
            TestPlayerTwoTowerEventIsNormalized();
            TestFourTowerCompletionsAreDetected();
            TestPlayerTwoTowerCompletionIsDetected();
            TestTowerCompletionRequiresShaoKahnVictory();
            TestTransitionToShaoKahnDoesNotCompleteMotaroMatch();
            TestTowerWithUnknownPlayerSideIsBlocked();
            TestLocalVersusPlayerOneIsBlocked();
            TestLocalVersusPlayerTwoIsBlocked();
            TestUnknownModeIsBlocked();
            TestUnknownModeWithConfirmedNetplayIsBlocked();
            TestUnsupportedNetplayIsBlocked();
            TestTwoPlayerNetplayPlayerOneIsUnchanged();
            TestTwoPlayerNetplayPlayerTwoIsNormalized();
            TestSpectatorIsBlocked();
            TestMoreThanTwoNetplayParticipantsAreBlocked();
            TestEligibilityPolicyAllowsOnlyReleaseModes();

            if (failures == 0)
            {
                Console.WriteLine("MameAchievementAttribution: all tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MameAchievementAttribution: " + failures +
                " test(s) failed."
            );
            return 1;
        }

        private static void TestTowerEventIsAllowedUnchanged()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = Normalize(
                "tower-cpu",
                "p1",
                false,
                false,
                0,
                0,
                achievementEvent
            );
            Equal("tower allowed", true, allowed);
            Equal("tower winner unchanged", "p1", achievementEvent.WinnerSide);
            Equal("tower P1 unchanged", 0, achievementEvent.PlayerOneId);
            Equal("tower scope", "computer", achievementEvent.MatchScope);
            Equal("tower id", 0, achievementEvent.TowerId);
        }

        private static void TestPlayerTwoTowerEventIsNormalized()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = Normalize(
                "tower-cpu",
                "p2",
                false,
                false,
                0,
                0,
                achievementEvent
            );
            Equal("P2 tower allowed", true, allowed);
            Equal("P2 tower winner normalized", "p2", achievementEvent.WinnerSide);
            Equal("P2 tower fighter moved to P1", 1, achievementEvent.PlayerOneId);
            Equal("P2 tower health moved to P1", 0, achievementEvent.PlayerOneHealth);
            Equal("P2 tower rounds moved to P1", 1, achievementEvent.PlayerOneRounds);
            Equal("P2 tower danger moved to P2", true, achievementEvent.PlayerTwoDangerSeen);
            Equal(
                "P2 tower finisher normalized",
                "p1",
                achievementEvent.FinisherWinnerSide
            );
            Equal("P2 tower special normalized", "p1", achievementEvent.SpecialActorSide);
            Equal("P2 tower scope", "computer", achievementEvent.MatchScope);
        }

        private static void TestFourTowerCompletionsAreDetected()
        {
            for (int towerId = 0; towerId <= 3; towerId++)
            {
                MoveBookAchievementEvent achievementEvent = SampleEvent();
                achievementEvent.EventType = "match-ended";
                achievementEvent.PlayerTwoId = 0x19;
                bool allowed = NormalizeTower(
                    "p1",
                    towerId,
                    0x19,
                    achievementEvent
                );
                Equal("tower completion allowed " + towerId, true, allowed);
                Equal(
                    "tower completion detected " + towerId,
                    true,
                    achievementEvent.TowerCompleted
                );
            }
        }

        private static void TestPlayerTwoTowerCompletionIsDetected()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            achievementEvent.EventType = "match-ended";
            achievementEvent.WinnerSide = "p2";
            achievementEvent.PlayerOneId = 0x19;
            achievementEvent.PlayerTwoId = 0x03;
            bool allowed = NormalizeTower(
                "p2",
                1,
                0x19,
                achievementEvent
            );
            Equal("P2 tower completion allowed", true, allowed);
            Equal("P2 tower winner normalized", "p1", achievementEvent.WinnerSide);
            Equal("P2 tower completion detected", true, achievementEvent.TowerCompleted);
        }

        private static void TestTowerCompletionRequiresShaoKahnVictory()
        {
            MoveBookAchievementEvent regularFight = SampleEvent();
            regularFight.EventType = "match-ended";
            NormalizeTower("p1", 2, 0x18, regularFight);
            Equal("Motaro is not tower completion", false, regularFight.TowerCompleted);

            MoveBookAchievementEvent loss = SampleEvent();
            loss.EventType = "match-ended";
            loss.WinnerSide = "p2";
            loss.PlayerTwoId = 0x19;
            NormalizeTower("p1", 2, 0x19, loss);
            Equal("Shao Kahn loss is not completion", false, loss.TowerCompleted);
        }

        private static void TestTransitionToShaoKahnDoesNotCompleteMotaroMatch()
        {
            MoveBookAchievementEvent motaroMatch = SampleEvent();
            motaroMatch.EventType = "match-ended";
            motaroMatch.PlayerTwoId = 0x18;

            // Reproduces the capture: the completed event still belongs to
            // Motaro, but the live character slot has already changed to 0x19.
            NormalizeTower("p1", 0, 0x19, motaroMatch);

            Equal(
                "Motaro-to-Shao transition does not complete tower",
                false,
                motaroMatch.TowerCompleted
            );
        }

        private static void TestTowerWithUnknownPlayerSideIsBlocked()
        {
            Equal(
                "tower without player side blocked",
                false,
                Normalize(
                    "tower-cpu",
                    "",
                    false,
                    false,
                    0,
                    0,
                    SampleEvent()
                )
            );
        }

        private static void TestLocalVersusPlayerOneIsBlocked()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            achievementEvent.EventType = "match-ended";
            Equal(
                "local P1 blocked",
                false,
                Normalize("two-player", "", true, false, 0, 0, achievementEvent)
            );
        }

        private static void TestLocalVersusPlayerTwoIsBlocked()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            achievementEvent.EventType = "match-ended";
            achievementEvent.WinnerSide = "p2";
            Equal(
                "local P2 blocked",
                false,
                Normalize("two-player", "", true, false, 0, 0, achievementEvent)
            );
        }

        private static void TestUnknownModeIsBlocked()
        {
            Equal(
                "unknown mode blocked",
                false,
                Normalize("unknown", "", false, false, 0, 0, SampleEvent())
            );
        }

        private static void TestUnknownModeWithConfirmedNetplayIsBlocked()
        {
            Equal(
                "unknown mode with netplay blocked",
                false,
                Normalize("unknown", "", true, true, 1, 2, SampleEvent())
            );
        }

        private static void TestUnsupportedNetplayIsBlocked()
        {
            Equal(
                "unsupported netplay blocked",
                false,
                Normalize("two-player", "", false, true, 2, 2, SampleEvent())
            );
        }

        private static void TestTwoPlayerNetplayPlayerOneIsUnchanged()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = Normalize(
                "two-player",
                "",
                true,
                true,
                1,
                2,
                achievementEvent
            );
            Equal("P1 allowed", true, allowed);
            Equal("P1 winner unchanged", "p1", achievementEvent.WinnerSide);
            Equal("P1 fighter unchanged", 0, achievementEvent.PlayerOneId);
            Equal("P2 fighter unchanged", 1, achievementEvent.PlayerTwoId);
            Equal("P1 online scope", "online", achievementEvent.MatchScope);
        }

        private static void TestTwoPlayerNetplayPlayerTwoIsNormalized()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = Normalize(
                "two-player",
                "",
                true,
                true,
                2,
                2,
                achievementEvent
            );
            Equal("P2 allowed", true, allowed);
            Equal("P2 loss normalized", "p2", achievementEvent.WinnerSide);
            Equal("profile fighter moved to P1", 1, achievementEvent.PlayerOneId);
            Equal("opponent fighter moved to P2", 0, achievementEvent.PlayerTwoId);
            Equal("profile health moved to P1", 0, achievementEvent.PlayerOneHealth);
            Equal("opponent health moved to P2", 166, achievementEvent.PlayerTwoHealth);
            Equal("profile rounds moved to P1", 1, achievementEvent.PlayerOneRounds);
            Equal("profile danger moved to P2", true, achievementEvent.PlayerTwoDangerSeen);
            Equal(
                "P2 finisher normalized",
                "p1",
                achievementEvent.FinisherWinnerSide
            );
            Equal("P2 online scope", "online", achievementEvent.MatchScope);
            Equal("P2 special normalized", "p1", achievementEvent.SpecialActorSide);
        }

        private static void TestSpectatorIsBlocked()
        {
            Equal(
                "spectator blocked",
                false,
                Normalize("two-player", "", true, true, 0, 2, SampleEvent())
            );
        }

        private static void TestMoreThanTwoNetplayParticipantsAreBlocked()
        {
            Equal(
                "unsupported four-player mapping blocked",
                false,
                Normalize("two-player", "", true, true, 3, 4, SampleEvent())
            );
        }

        private static void TestFivePlayerP3IsNormalizedWhenActive()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = NormalizeTeam(5, 3, 0x10, achievementEvent, 2, 1);
            Equal("five-player P3 active", true, allowed);
            Equal("five-player P3 side normalized", "p2", achievementEvent.WinnerSide);
        }

        private static void TestThreePlayerLayoutsUseDynamicMapping()
        {
            Equal(
                "three-player P3 active on left",
                true,
                NormalizeTeam(3, 3, 0x00, SampleEvent(), 2, 0)
            );
            Equal(
                "three-player P1 active on right",
                true,
                NormalizeTeam(3, 1, 0x00, SampleEvent(), 2, 1)
            );
            Equal(
                "three-player P2 waiting",
                false,
                NormalizeTeam(3, 2, 0x00, SampleEvent(), 2, 2)
            );
            Equal(
                "three-player split does not change live left",
                true,
                NormalizeTeam(3, 2, 0x00, SampleEvent(), 1, 0)
            );
            Equal(
                "three-player split does not change live right",
                true,
                NormalizeTeam(3, 3, 0x00, SampleEvent(), 1, 1)
            );
            Equal(
                "unknown split still resolves live left",
                true,
                NormalizeTeam(3, 3, 0x00, SampleEvent(), 0, 0)
            );
            Equal(
                "unknown split still resolves live right",
                true,
                NormalizeTeam(3, 1, 0x00, SampleEvent(), 0, 1)
            );
        }

        private static void TestAllFourPlayerSlotsAreAcceptedWhenActive()
        {
            Equal("four-player P1 left", true,
                NormalizeTeam(4, 1, 0x00, SampleEvent(), 2, 0));
            Equal("four-player P2 left", true,
                NormalizeTeam(4, 2, 0x10, SampleEvent(), 2, 0));
            Equal("four-player P3 right", true,
                NormalizeTeam(4, 3, 0x00, SampleEvent(), 2, 1));
            Equal("four-player P4 right", true,
                NormalizeTeam(4, 4, 0x01, SampleEvent(), 2, 1));
        }

        private static void TestSixPlayerP3IsUnchangedWhenActive()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = NormalizeTeam(
                6, 3, 0x21, achievementEvent, 3, 0
            );
            Equal("six-player P3 active", true, allowed);
            Equal("six-player P3 side unchanged", "p1", achievementEvent.WinnerSide);
        }

        private static void TestSixPlayerP6IsNormalizedWhenActive()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = NormalizeTeam(
                6, 6, 0x22, achievementEvent, 3, 1
            );
            Equal("six-player P6 active", true, allowed);
            Equal("six-player P6 side normalized", "p2", achievementEvent.WinnerSide);
        }

        private static void TestSixPlayerDynamicReorderIsUsed()
        {
            MoveBookAchievementEvent achievementEvent = SampleEvent();
            bool allowed = NormalizeTeam(
                6,
                6,
                0x00,
                achievementEvent,
                3,
                0
            );
            Equal("reordered P6 active", true, allowed);
            Equal(
                "reordered P6 follows left controller slot",
                "p1",
                achievementEvent.WinnerSide
            );
        }

        private static void TestAllSixPlayerSlotsAreAcceptedWhenActive()
        {
            Equal("six-player P1", true,
                NormalizeTeam(6, 1, 0x00, SampleEvent(), 3, 0));
            Equal("six-player P2", true,
                NormalizeTeam(6, 2, 0x10, SampleEvent(), 3, 0));
            Equal("six-player P3", true,
                NormalizeTeam(6, 3, 0x20, SampleEvent(), 3, 0));
            Equal("six-player P4", true,
                NormalizeTeam(6, 4, 0x00, SampleEvent(), 3, 1));
            Equal("six-player P5", true,
                NormalizeTeam(6, 5, 0x01, SampleEvent(), 3, 1));
            Equal("six-player P6", true,
                NormalizeTeam(6, 6, 0x02, SampleEvent(), 3, 1));
        }

        private static void TestInactiveTeamPlayerIsBlocked()
        {
            Equal(
                "inactive team player blocked",
                false,
                NormalizeTeam(6, 2, 0x21, SampleEvent(), 3, 3)
            );
        }

        private static MoveBookAchievementEvent SampleEvent()
        {
            return new MoveBookAchievementEvent
            {
                WinnerSide = "p1",
                PlayerOneId = 0,
                PlayerTwoId = 1,
                PlayerOneHealth = 166,
                PlayerTwoHealth = 0,
                PlayerOneRounds = 2,
                PlayerTwoRounds = 1,
                PlayerOneDangerSeen = true,
                PlayerTwoDangerSeen = false,
                FinisherWinnerSide = "p2",
                SpecialActorSide = "p2"
            };
        }

        private static void TestEligibilityPolicyAllowsOnlyReleaseModes()
        {
            Equal("clean CPU full match allowed", true,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3uk", true, "tower-cpu", "verified-clean",
                    false, false, 0, 0, 0
                ));
            Equal("CPU cheat blocked", false,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3uk", true, "tower-cpu", "blocked-cheat",
                    false, false, 0, 0, 0
                ));
            Equal("partial CPU match blocked", false,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3uk", false, "tower-cpu", "verified-clean",
                    false, false, 0, 0, 0
                ));
            Equal("Team Edition 1x1 allowed", true,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3tm", true, "two-player", "not-applicable",
                    true, true, 1, 2, 1
                ));
            Equal("standard UMK3 1x1 P2 allowed", true,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3uk", true, "two-player", "not-applicable",
                    true, true, 2, 2, 1
                ));
            Equal("two players with 3x3 selection blocked", false,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3tm", true, "two-player", "not-applicable",
                    true, true, 1, 2, 3
                ));
            Equal("local Versus blocked", false,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3uk", true, "two-player", "not-applicable",
                    false, false, 0, 0, 1
                ));
            Equal("six-player Team Edition blocked", false,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3tm", true, "two-player", "not-applicable",
                    true, true, 3, 6, 1
                ));
            Equal("unsupported ROM blocked", false,
                MameAchievementEligibilityPolicy.IsSnapshotEligible(
                    "umk3p", true, "two-player", "not-applicable",
                    true, true, 1, 2, 1
                ));

            MoveBookAchievementEvent queued = SampleEvent();
            queued.RomId = "umk3tm";
            queued.MatchScope = "online";
            queued.MatchFullyObserved = true;
            queued.NetplayTotalPlayers = 2;
            queued.TeamSize = 1;
            Equal("eligible 1x1 event may enter outbox", true,
                MameAchievementEligibilityPolicy.IsQueuedEventEligible(queued));
            queued.TeamSize = 3;
            Equal("3x3 selection may not enter outbox", false,
                MameAchievementEligibilityPolicy.IsQueuedEventEligible(queued));
            queued.TeamSize = 1;
            queued.NetplayTotalPlayers = 6;
            Equal("six-player event may not enter outbox", false,
                MameAchievementEligibilityPolicy.IsQueuedEventEligible(queued));
            queued.MatchScope = "computer";
            queued.NetplayTotalPlayers = 0;
            queued.TeamSize = 0;
            queued.CheatPolicyState = "unknown";
            Equal("unverified CPU event may not enter outbox", false,
                MameAchievementEligibilityPolicy.IsQueuedEventEligible(queued));
        }

        private static bool Normalize(
            string matchMode,
            string towerPlayerSide,
            bool layoutSupported,
            bool active,
            int playerNumber,
            int totalPlayers,
            MoveBookAchievementEvent achievementEvent
        )
        {
            return MameAchievementAttribution.TryNormalizeForProfile(
                matchMode,
                towerPlayerSide,
                0,
                0,
                "umk3tm",
                0,
                layoutSupported,
                active,
                playerNumber,
                totalPlayers,
                achievementEvent
            );
        }

        private static bool NormalizeTower(
            string towerPlayerSide,
            int towerId,
            int towerOpponentId,
            MoveBookAchievementEvent achievementEvent
        )
        {
            return MameAchievementAttribution.TryNormalizeForProfile(
                "tower-cpu",
                towerPlayerSide,
                towerId,
                towerOpponentId,
                "umk3uk",
                0,
                false,
                false,
                0,
                0,
                achievementEvent
            );
        }

        private static bool NormalizeTeam(
            int totalPlayers,
            int playerNumber,
            int activePlayerCode,
            MoveBookAchievementEvent achievementEvent,
            int teamLeftPlayerCount = 0,
            int controllerSlot = -1
        )
        {
            return MameAchievementAttribution.TryNormalizeForProfile(
                "two-player",
                "",
                -1,
                -1,
                "umk3tm",
                activePlayerCode,
                teamLeftPlayerCount == 0 && totalPlayers != 3
                    ? totalPlayers / 2
                    : teamLeftPlayerCount,
                controllerSlot,
                true,
                true,
                playerNumber,
                totalPlayers,
                achievementEvent
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

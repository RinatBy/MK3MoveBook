using System;
using System.Collections.Generic;

namespace MK3MoveBook
{
    internal static class MameMatchModeClassifierTests
    {
        private static int failures;

        private static int Main()
        {
            TestNoviceCpuTower();
            TestWarriorCpuTower();
            TestMasterCpuTower();
            TestFourthMasterCpuTower();
            TestCpuTowerParticipantSixSeven();
            TestCpuTowerParticipantSevenSix();
            TestCpuTowerParticipantEightEight();
            TestCpuTowerParticipantNineIsUnknown();
            TestDirectTwoPlayerMatch();
            TestTeamMatchParticipantSwap();
            TestTeamTransitionBothSix();
            TestThreeOnThreeParticipantState();
            TestTeamSizeCodes();
            TestPlayerTwoContinuesTower();
            TestSelectionTransitionIsUnknown();
            TestContradictorySignalsAreUnknown();

            if (failures == 0)
            {
                Console.WriteLine(
                    "MameMatchModeClassifier: 16 tests passed."
                );
                return 0;
            }
            Console.Error.WriteLine(
                "MameMatchModeClassifier: " + failures +
                " test(s) failed."
            );
            return 1;
        }

        private static void TestNoviceCpuTower()
        {
            MameMatchModeResult result = ClassifyCpuTower(0, 1);
            Equal("novice mode", "tower-cpu", result.Mode);
            Equal("novice name", "NOVICE", result.TowerName);
            Equal("novice id", 0, result.TowerId);
            Equal("novice fight", 1, result.TowerFightNumber);
            Equal("novice player side", "p1", result.TowerPlayerSide);
        }

        private static void TestWarriorCpuTower()
        {
            MameMatchModeResult result = ClassifyCpuTower(1, 2);
            Equal("warrior mode", "tower-cpu", result.Mode);
            Equal("warrior name", "WARRIOR", result.TowerName);
            Equal("warrior fight", 2, result.TowerFightNumber);
        }

        private static void TestMasterCpuTower()
        {
            MameMatchModeResult result = ClassifyCpuTower(2, 1);
            Equal("master mode", "tower-cpu", result.Mode);
            Equal("master name", "MASTER", result.TowerName);
        }

        private static void TestFourthMasterCpuTower()
        {
            MameMatchModeResult result = ClassifyCpuTower(3, 1);
            Equal("fourth master mode", "tower-cpu", result.Mode);
            Equal("fourth master id", 3, result.TowerId);
            Equal(
                "fourth master name",
                "MASTER · 4-я башня",
                result.TowerName
            );
        }

        private static void TestCpuTowerParticipantSixSeven()
        {
            MameMatchModeResult result = ClassifyCpuTower(0, 2, 6, 7);
            Equal("CPU tower participant states 6/7", "tower-cpu", result.Mode);
            Equal("CPU tower participant states 6/7 side", "p1", result.TowerPlayerSide);
        }

        private static void TestCpuTowerParticipantSevenSix()
        {
            MameMatchModeResult result = ClassifyCpuTower(0, 3, 7, 6);
            Equal("CPU tower participant states 7/6", "tower-cpu", result.Mode);
            Equal("CPU tower participant states 7/6 side", "p1", result.TowerPlayerSide);
        }

        private static void TestCpuTowerParticipantEightEight()
        {
            MameMatchModeResult result = ClassifyCpuTower(0, 4, 8, 8);
            Equal("CPU tower participant states 8/8", "tower-cpu", result.Mode);
            Equal("CPU tower participant states 8/8 side", "p1", result.TowerPlayerSide);
        }

        private static void TestCpuTowerParticipantNineIsUnknown()
        {
            MameMatchModeResult result = ClassifyCpuTower(0, 5, 9, 6);
            Equal("CPU tower participant state 9", "unknown", result.Mode);
        }

        private static void TestDirectTwoPlayerMatch()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                1,
                0,
                0,
                0,
                1,
                1,
                7,
                7
            );
            Equal("direct two-player mode", "two-player", result.Mode);
            Equal("direct two-player tower", false, result.TowerContext);
            Equal("direct two-player tower id", -1, result.TowerId);
            Equal(
                "two-player source is checked separately",
                "Память игры подтверждает двухсторонний матч; " +
                    "источник проверяется отдельно.",
                result.Details
            );
        }

        private static void TestTeamMatchParticipantSwap()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                1,
                0,
                0,
                0,
                1,
                1,
                6,
                7
            );
            Equal("team swap mode", "two-player", result.Mode);
        }

        private static void TestTeamTransitionBothSix()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                1,
                0,
                0,
                0,
                1,
                1,
                6,
                6
            );
            Equal("team transition mode", "two-player", result.Mode);
        }

        private static void TestThreeOnThreeParticipantState()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                1,
                0,
                0,
                0,
                2,
                1,
                7,
                8
            );
            Equal("three-on-three mode", "two-player", result.Mode);
        }

        private static void TestTeamSizeCodes()
        {
            Equal("one fighter", 1, MameMatchModeClassifier.DecodeTeamSize(0));
            Equal("two fighters", 2, MameMatchModeClassifier.DecodeTeamSize(1));
            Equal("three fighters", 3, MameMatchModeClassifier.DecodeTeamSize(2));
            Equal("unknown team size", 0, MameMatchModeClassifier.DecodeTeamSize(3));
        }

        private static void TestPlayerTwoContinuesTower()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                1,
                1,
                1,
                0,
                1,
                1,
                7,
                7
            );
            Equal("P2 tower mode", "tower-cpu", result.Mode);
            Equal("P2 tower context", true, result.TowerContext);
            Equal("P2 tower name", "NOVICE", result.TowerName);
            Equal("P2 tower player side", "p2", result.TowerPlayerSide);
        }

        private static void TestSelectionTransitionIsUnknown()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                0,
                4,
                4,
                2,
                0x11,
                0,
                0,
                0
            );
            Equal("selection mode", "unknown", result.Mode);
            Equal("selection tower context", false, result.TowerContext);
            Equal("selection fight hidden", 0, result.TowerFightNumber);
        }

        private static void TestContradictorySignalsAreUnknown()
        {
            MameMatchModeResult result = MameMatchModeClassifier.Classify(
                1,
                1,
                1,
                0,
                1,
                1,
                5,
                7
            );
            Equal("contradictory mode", "unknown", result.Mode);
        }

        private static MameMatchModeResult ClassifyCpuTower(
            int towerId,
            int fightNumber,
            int playerOneParticipantState = 6,
            int playerTwoParticipantState = 6
        )
        {
            return MameMatchModeClassifier.Classify(
                1,
                1,
                1,
                towerId,
                fightNumber,
                0,
                playerOneParticipantState,
                playerTwoParticipantState
            );
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

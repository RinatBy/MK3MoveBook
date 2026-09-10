using System;

namespace MK3MoveBook
{
    internal sealed class MameMatchModeResult
    {
        public string Mode { get; set; }
        public string Label { get; set; }
        public string Details { get; set; }
        public bool TowerContext { get; set; }
        public string TowerPlayerSide { get; set; }
        public int TowerId { get; set; }
        public string TowerName { get; set; }
        public int TowerFightNumber { get; set; }
    }

    internal static class MameMatchModeClassifier
    {
        public static MameMatchModeResult Classify(
            int powerBarsEnabled,
            int towerStateA,
            int towerStateB,
            int towerId,
            int towerFightNumber,
            int playerTwoControlState,
            int playerOneParticipantState,
            int playerTwoParticipantState
        )
        {
            bool towerContext = towerStateA == 1 && towerStateB == 1;
            bool fightActive = powerBarsEnabled == 1;
            bool cpuTowerParticipants =
                playerTwoControlState == 0 &&
                IsTwoPlayerParticipantState(playerOneParticipantState) &&
                IsTwoPlayerParticipantState(playerTwoParticipantState);
            bool humanParticipants =
                playerTwoControlState == 1 &&
                IsTwoPlayerParticipantState(playerOneParticipantState) &&
                IsTwoPlayerParticipantState(playerTwoParticipantState);
            bool playerTwoTowerParticipants = towerContext && humanParticipants;

            string mode = "unknown";
            string label = "Режим не подтверждён";
            string details = "Ожидаем устойчивые игровые признаки.";
            string towerPlayerSide = "";
            if (fightActive && towerContext &&
                (cpuTowerParticipants || playerTwoTowerParticipants))
            {
                mode = "tower-cpu";
                label = "Башня против компьютера";
                towerPlayerSide = playerTwoTowerParticipants ? "p2" : "p1";
                details = "Одиночный бой башни против CPU подтверждён " +
                    "по памяти MAME; локальная сторона " +
                    towerPlayerSide.ToUpperInvariant() + ".";
            }
            else if (fightActive && humanParticipants)
            {
                mode = "two-player";
                label = "Матч двух игроков";
                details = "Память игры подтверждает двухсторонний матч; " +
                    "источник проверяется отдельно.";
            }

            return new MameMatchModeResult
            {
                Mode = mode,
                Label = label,
                Details = details,
                TowerContext = towerContext,
                TowerPlayerSide = towerPlayerSide,
                TowerId = towerContext ? towerId : -1,
                TowerName = towerContext ? TowerName(towerId) : "",
                TowerFightNumber = fightActive && towerContext &&
                    towerFightNumber > 0 && towerFightNumber <= 32
                        ? towerFightNumber
                        : 0
            };
        }

        private static bool IsTwoPlayerParticipantState(int value)
        {
            return value >= 6 && value <= 8;
        }

        public static int DecodeTeamSize(int teamSizeCode)
        {
            return teamSizeCode >= 0 && teamSizeCode <= 2
                ? teamSizeCode + 1
                : 0;
        }

        private static string TowerName(int towerId)
        {
            switch (towerId)
            {
                case 0:
                    return "NOVICE";
                case 1:
                    return "WARRIOR";
                case 2:
                    return "MASTER";
                case 3:
                    return "MASTER · 4-я башня";
                default:
                    return "Башня " + (towerId + 1) + " · не подтверждена";
            }
        }
    }
}

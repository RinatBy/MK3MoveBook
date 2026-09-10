using System;

namespace MK3MoveBook
{
    internal sealed class MameFinisherProgress
    {
        public int Sequence { get; set; }
        public string Kind { get; set; }
        public string WinnerSide { get; set; }
        public int FighterId { get; set; }
        public int OpponentId { get; set; }
        public int TeamActivePlayerCode { get; set; }
        public int TeamLeftPlayerCount { get; set; }
        public int TeamControllerSlot { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }

    internal sealed class MameFinisherTracker
    {
        private const int KabalCharacterId = 0x0A;
        private const int AntifatalityFlagsBefore = 0x12;
        private const int AntifatalityFlagsAfter = 0x22;

        private int previousSuccess;
        private int previousTypeA;
        private int previousPlayerOneFlags;
        private int previousPlayerTwoFlags;
        private int sequence;
        private string kind;
        private string winnerSide;
        private int fighterId;
        private int opponentId;
        private int teamActivePlayerCode;
        private int teamLeftPlayerCount;
        private int teamControllerSlot;
        private string title;
        private string details;

        public MameFinisherTracker()
        {
            Reset();
        }

        public void Reset()
        {
            previousSuccess = -1;
            previousTypeA = -1;
            previousPlayerOneFlags = -1;
            previousPlayerTwoFlags = -1;
            sequence = 0;
            kind = "";
            winnerSide = "";
            fighterId = -1;
            opponentId = -1;
            teamActivePlayerCode = 0;
            teamLeftPlayerCount = 0;
            teamControllerSlot = -1;
            title = "";
            details = "";
        }

        public MameFinisherProgress Observe(
            string romId,
            int playerOneId,
            string playerOne,
            int playerOneHealth,
            int playerOneFlags,
            int playerTwoId,
            string playerTwo,
            int playerTwoHealth,
            int playerTwoFlags,
            int finishPhase,
            int typeA,
            int success,
            int typeB,
            int currentTeamActivePlayerCode = 0,
            int currentTeamLeftPlayerCount = 0,
            int currentTeamControllerSlot = -1
        )
        {
            string detectedKind = "";
            bool genericStarted = previousSuccess == 0 && success != 0;
            bool stageStarted = finishPhase == 0x03 &&
                typeA == 0x02 && previousTypeA != 0x02;
            bool playerOneAntifatality = IsAntifatality(
                romId,
                playerOneId,
                playerOneHealth,
                playerTwoHealth,
                previousPlayerOneFlags,
                playerOneFlags,
                finishPhase,
                typeA,
                success,
                typeB
            );
            bool playerTwoAntifatality = IsAntifatality(
                romId,
                playerTwoId,
                playerTwoHealth,
                playerOneHealth,
                previousPlayerTwoFlags,
                playerTwoFlags,
                finishPhase,
                typeA,
                success,
                typeB
            );

            if (playerOneAntifatality || playerTwoAntifatality)
            {
                detectedKind = "antifatality";
            }
            else if (stageStarted)
            {
                detectedKind = "stage-fatality";
            }
            else if (genericStarted)
            {
                detectedKind = FinisherKind(typeB);
            }

            if (!string.IsNullOrEmpty(detectedKind))
            {
                if (playerOneHealth > 0 && playerTwoHealth == 0)
                {
                    Record(
                        detectedKind,
                        "p1",
                        playerOneId,
                        playerOne,
                        playerTwoId,
                        playerTwo,
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                }
                else if (playerTwoHealth > 0 && playerOneHealth == 0)
                {
                    Record(
                        detectedKind,
                        "p2",
                        playerTwoId,
                        playerTwo,
                        playerOneId,
                        playerOne,
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                }
            }

            previousSuccess = success;
            previousTypeA = typeA;
            previousPlayerOneFlags = playerOneFlags;
            previousPlayerTwoFlags = playerTwoFlags;
            return Snapshot();
        }

        private static bool IsAntifatality(
            string romId,
            int characterId,
            int ownHealth,
            int opponentHealth,
            int previousFlags,
            int currentFlags,
            int finishPhase,
            int typeA,
            int success,
            int typeB
        )
        {
            // Both UK profiles ship the same program ROM. Apply the same
            // guarded Kabal flag transition (see research findings).
            return (string.Equals(romId, "umk3uk", StringComparison.Ordinal) ||
                    string.Equals(romId, "umk3uk2", StringComparison.Ordinal)) &&
                characterId == KabalCharacterId &&
                ownHealth > 0 &&
                opponentHealth == 0 &&
                finishPhase == 0x03 &&
                typeA == 0 &&
                success == 0 &&
                typeB == 0 &&
                previousFlags >= 0 &&
                currentFlags >= 0 &&
                previousFlags >> 8 == currentFlags >> 8 &&
                (previousFlags & 0xFF) == AntifatalityFlagsBefore &&
                (currentFlags & 0xFF) == AntifatalityFlagsAfter;
        }

        private void Record(
            string detectedKind,
            string detectedWinnerSide,
            int detectedFighterId,
            string fighter,
            int detectedOpponentId,
            string opponent,
            int detectedTeamActivePlayerCode,
            int detectedTeamLeftPlayerCount,
            int detectedTeamControllerSlot
        )
        {
            sequence++;
            kind = detectedKind;
            winnerSide = detectedWinnerSide;
            fighterId = detectedFighterId;
            opponentId = detectedOpponentId;
            teamActivePlayerCode = detectedTeamActivePlayerCode;
            teamLeftPlayerCount = detectedTeamLeftPlayerCount;
            teamControllerSlot = detectedTeamControllerSlot;
            title = FinisherTitle(detectedKind);
            details = fighter + " против " + opponent + ".";
        }

        private MameFinisherProgress Snapshot()
        {
            return new MameFinisherProgress
            {
                Sequence = sequence,
                Kind = kind,
                WinnerSide = winnerSide,
                FighterId = fighterId,
                OpponentId = opponentId,
                TeamActivePlayerCode = teamActivePlayerCode,
                TeamLeftPlayerCount = teamLeftPlayerCount,
                TeamControllerSlot = teamControllerSlot,
                Title = title,
                Details = details
            };
        }

        private static string FinisherKind(int value)
        {
            switch (value)
            {
                case 0x01:
                    return "fatality";
                case 0x03:
                    return "animality";
                case 0x04:
                    return "friendship";
                case 0x05:
                    return "babality";
                case 0x06:
                    return "brutality";
                default:
                    return "";
            }
        }

        private static string FinisherTitle(string value)
        {
            switch (value)
            {
                case "fatality":
                    return "Зафиксировано Fatality";
                case "stage-fatality":
                    return "Зафиксировано Stage Fatality";
                case "animality":
                    return "Зафиксировано Animality";
                case "friendship":
                    return "Зафиксировано Friendship";
                case "babality":
                    return "Зафиксировано Babality";
                case "brutality":
                    return "Зафиксировано Brutality";
                case "antifatality":
                    return "Зафиксировано Antifatality";
                default:
                    return "Зафиксировано добивание";
            }
        }
    }
}

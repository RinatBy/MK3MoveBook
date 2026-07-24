using System;

namespace MK3MoveBook
{
    internal sealed class MameFightProgress
    {
        public string State { get; set; }
        public bool ParticipantsConfirmed { get; set; }
        public int PlayerOneId { get; set; }
        public int PlayerTwoId { get; set; }
        public string PlayerOne { get; set; }
        public string PlayerTwo { get; set; }
        public int RoundNumber { get; set; }
        public int PlayerOneRounds { get; set; }
        public int PlayerTwoRounds { get; set; }
        public string WinnerSide { get; set; }
        public string Winner { get; set; }
        public int EventSequence { get; set; }
        public string EventKind { get; set; }
        public string EventTitle { get; set; }
        public string EventDetails { get; set; }
    }

    internal sealed class MameFightTracker
    {
        private bool matchInProgress;
        private bool matchCompleted;
        private bool roundActive;
        private int roundNumber;
        private int playerOneRounds;
        private int playerTwoRounds;
        private int playerOneId;
        private int playerTwoId;
        private string playerOneName;
        private string playerTwoName;
        private string winnerSide;
        private string winner;
        private int eventSequence;
        private string eventKind;
        private string eventTitle;
        private string eventDetails;

        public MameFightTracker()
        {
            Reset();
        }

        public void Reset()
        {
            matchInProgress = false;
            matchCompleted = false;
            roundActive = false;
            roundNumber = 0;
            playerOneRounds = 0;
            playerTwoRounds = 0;
            playerOneId = -1;
            playerTwoId = -1;
            playerOneName = "";
            playerTwoName = "";
            winnerSide = "";
            winner = "";
            eventSequence = 0;
            eventKind = "";
            eventTitle = "";
            eventDetails = "";
        }

        public MameFightProgress Observe(
            int currentPlayerOneId,
            string currentPlayerOneName,
            int playerOneHealth,
            int currentPlayerTwoId,
            string currentPlayerTwoName,
            int playerTwoHealth
        )
        {
            bool playerOneAlive = playerOneHealth > 0;
            bool playerTwoAlive = playerTwoHealth > 0;
            bool bothAlive = playerOneAlive && playerTwoAlive;

            if (bothAlive && !roundActive)
            {
                if (!matchInProgress || matchCompleted)
                {
                    StartMatch(
                        currentPlayerOneId,
                        currentPlayerOneName,
                        currentPlayerTwoId,
                        currentPlayerTwoName
                    );
                }
                else
                {
                    roundNumber++;
                    roundActive = true;
                    RecordEvent(
                        "round-started",
                        "Раунд " + roundNumber + " начался",
                        ScoreDetails()
                    );
                }
            }
            else if (roundActive && (!playerOneAlive || !playerTwoAlive))
            {
                FinishRound(playerOneAlive, playerTwoAlive);
            }

            return Snapshot();
        }

        private void StartMatch(
            int currentPlayerOneId,
            string currentPlayerOneName,
            int currentPlayerTwoId,
            string currentPlayerTwoName
        )
        {
            matchInProgress = true;
            matchCompleted = false;
            roundActive = true;
            roundNumber = 1;
            playerOneRounds = 0;
            playerTwoRounds = 0;
            playerOneId = currentPlayerOneId;
            playerTwoId = currentPlayerTwoId;
            playerOneName = currentPlayerOneName;
            playerTwoName = currentPlayerTwoName;
            winnerSide = "";
            winner = "";
            RecordEvent(
                "match-started",
                "Бой начался",
                playerOneName + " против " + playerTwoName + ". Раунд 1."
            );
        }

        private void FinishRound(bool playerOneAlive, bool playerTwoAlive)
        {
            roundActive = false;
            if (playerOneAlive && !playerTwoAlive)
            {
                playerOneRounds++;
                if (playerOneRounds >= 2)
                {
                    FinishMatch("p1", playerOneName);
                    return;
                }
                RecordEvent(
                    "round-ended",
                    "Раунд за P1 — " + playerOneName,
                    ScoreDetails()
                );
                return;
            }

            if (playerTwoAlive && !playerOneAlive)
            {
                playerTwoRounds++;
                if (playerTwoRounds >= 2)
                {
                    FinishMatch("p2", playerTwoName);
                    return;
                }
                RecordEvent(
                    "round-ended",
                    "Раунд за P2 — " + playerTwoName,
                    ScoreDetails()
                );
                return;
            }

            RecordEvent(
                "round-draw",
                "Раунд завершён без победителя",
                ScoreDetails()
            );
        }

        private void FinishMatch(string side, string fighterName)
        {
            matchInProgress = false;
            matchCompleted = true;
            winnerSide = side;
            winner = fighterName;
            RecordEvent(
                "match-ended",
                "Победитель матча — " + fighterName,
                "Итоговый счёт " + playerOneRounds + ":" + playerTwoRounds + "."
            );
        }

        private string ScoreDetails()
        {
            return "Счёт по раундам " + playerOneRounds + ":" +
                playerTwoRounds + ".";
        }

        private void RecordEvent(string kind, string title, string details)
        {
            eventSequence++;
            eventKind = kind;
            eventTitle = title;
            eventDetails = details;
        }

        private MameFightProgress Snapshot()
        {
            string state = "waiting";
            if (matchCompleted)
            {
                state = "match-ended";
            }
            else if (roundActive)
            {
                state = "round-active";
            }
            else if (matchInProgress)
            {
                state = "round-ended";
            }

            return new MameFightProgress
            {
                State = state,
                ParticipantsConfirmed = matchInProgress || matchCompleted,
                PlayerOneId = playerOneId,
                PlayerTwoId = playerTwoId,
                PlayerOne = playerOneName,
                PlayerTwo = playerTwoName,
                RoundNumber = roundNumber,
                PlayerOneRounds = playerOneRounds,
                PlayerTwoRounds = playerTwoRounds,
                WinnerSide = winnerSide,
                Winner = winner,
                EventSequence = eventSequence,
                EventKind = eventKind,
                EventTitle = eventTitle,
                EventDetails = eventDetails
            };
        }
    }
}

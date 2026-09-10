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
        public int MatchSequence { get; set; }
        public bool FullyObserved { get; set; }
        public bool PlayerOneDangerSeen { get; set; }
        public bool PlayerTwoDangerSeen { get; set; }
        public string WinnerSide { get; set; }
        public string Winner { get; set; }
        public int EventSequence { get; set; }
        public int EventTeamActivePlayerCode { get; set; }
        public int EventTeamLeftPlayerCount { get; set; }
        public int EventTeamControllerSlot { get; set; }
        public string EventKind { get; set; }
        public string EventTitle { get; set; }
        public string EventDetails { get; set; }
    }

    internal sealed class MameFightTracker
    {
        private const int RequiredRoundEndSamples = 3;

        private bool matchInProgress;
        private bool matchCompleted;
        private bool roundActive;
        private int roundNumber;
        private int playerOneRounds;
        private int playerTwoRounds;
        private int matchSequence;
        private bool playerOneDangerSeen;
        private bool playerTwoDangerSeen;
        private int playerOneId;
        private int playerTwoId;
        private string playerOneName;
        private string playerTwoName;
        private string winnerSide;
        private string winner;
        private int eventSequence;
        private int eventTeamActivePlayerCode;
        private int eventTeamLeftPlayerCount;
        private int eventTeamControllerSlot;
        private string eventKind;
        private string eventTitle;
        private string eventDetails;
        private string roundEndCandidate;
        private int roundEndCandidateSamples;
        private int roundEndCandidateTeamActivePlayerCode;
        private int roundEndCandidateTeamLeftPlayerCount;
        private int roundEndCandidateTeamControllerSlot;
        private int lastStableTeamActivePlayerCode;
        private int lastStableTeamLeftPlayerCount;
        private int lastStableTeamControllerSlot;
        private bool observationInitialized;
        private bool synchronizationRequired;
        private bool synchronizationSawScore;
        private bool matchFullyObserved;

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
            matchSequence = 0;
            playerOneDangerSeen = false;
            playerTwoDangerSeen = false;
            playerOneId = -1;
            playerTwoId = -1;
            playerOneName = "";
            playerTwoName = "";
            winnerSide = "";
            winner = "";
            eventSequence = 0;
            eventTeamActivePlayerCode = 0;
            eventTeamLeftPlayerCount = 0;
            eventTeamControllerSlot = -1;
            eventKind = "";
            eventTitle = "";
            eventDetails = "";
            roundEndCandidate = "";
            roundEndCandidateSamples = 0;
            roundEndCandidateTeamActivePlayerCode = 0;
            roundEndCandidateTeamLeftPlayerCount = 0;
            roundEndCandidateTeamControllerSlot = -1;
            lastStableTeamActivePlayerCode = 0;
            lastStableTeamLeftPlayerCount = 0;
            lastStableTeamControllerSlot = -1;
            observationInitialized = false;
            synchronizationRequired = false;
            synchronizationSawScore = false;
            matchFullyObserved = false;
        }

        public MameFightProgress Observe(
            int currentPlayerOneId,
            string currentPlayerOneName,
            int playerOneHealth,
            int currentPlayerTwoId,
            string currentPlayerTwoName,
            int playerTwoHealth,
            bool onlineMatch = false,
            int currentTeamActivePlayerCode = 0,
            int currentTeamLeftPlayerCount = 0,
            int currentTeamControllerSlot = -1,
            int currentOnlinePlayerCount = 0,
            int currentFinishPhase = -1,
            int currentGamePlayerOneRounds = -1,
            int currentGamePlayerTwoRounds = -1
        )
        {
            bool playerOneAlive = playerOneHealth > 0;
            bool playerTwoAlive = playerTwoHealth > 0;
            bool bothAlive = playerOneAlive && playerTwoAlive;
            bool finishPhaseAvailable = currentFinishPhase >= 0;
            bool combatPhase = !finishPhaseAvailable ||
                currentFinishPhase == 0x00;
            bool roundEndedByPhase = finishPhaseAvailable &&
                (currentFinishPhase == 0x01 ||
                 currentFinishPhase == 0x02);
            bool gameScoreAvailable =
                currentGamePlayerOneRounds >= 0 &&
                currentGamePlayerOneRounds <= 2 &&
                currentGamePlayerTwoRounds >= 0 &&
                currentGamePlayerTwoRounds <= 2;
            bool gameScoreIsZero = gameScoreAvailable &&
                currentGamePlayerOneRounds == 0 &&
                currentGamePlayerTwoRounds == 0;

            if (!observationInitialized)
            {
                observationInitialized = true;
                if (gameScoreAvailable &&
                    (playerOneAlive || playerTwoAlive || !gameScoreIsZero))
                {
                    // The bridge first saw live match state. It cannot prove
                    // that round one was observed, even when the raw score is
                    // still 0:0 at the knockout frame, so skip this match.
                    synchronizationRequired = true;
                    synchronizationSawScore = !gameScoreIsZero;
                }
            }

            if (synchronizationRequired)
            {
                UpdateParticipants(
                    currentPlayerOneId,
                    currentPlayerOneName,
                    currentPlayerTwoId,
                    currentPlayerTwoName
                );
                if (gameScoreAvailable && !gameScoreIsZero)
                {
                    synchronizationSawScore = true;
                }
                if (synchronizationSawScore &&
                    gameScoreIsZero &&
                    bothAlive &&
                    combatPhase)
                {
                    synchronizationRequired = false;
                    synchronizationSawScore = false;
                    StartMatch(
                        currentPlayerOneId,
                        currentPlayerOneName,
                        currentPlayerTwoId,
                        currentPlayerTwoName,
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                }
                return Snapshot();
            }

            if (onlineMatch && bothAlive && combatPhase)
            {
                // Team Edition rewrites fighter slots and the active-controller
                // code on the same frame as a knockout. Preserve the last
                // stable combat frame so the event belongs to the fighter who
                // actually played, not to the teammate entering afterwards.
                lastStableTeamActivePlayerCode =
                    currentTeamActivePlayerCode;
                lastStableTeamLeftPlayerCount =
                    currentTeamLeftPlayerCount;
                lastStableTeamControllerSlot =
                    currentTeamControllerSlot;
            }

            if (roundActive)
            {
                if (combatPhase)
                {
                    playerOneDangerSeen = playerOneDangerSeen ||
                        (playerOneHealth > 0 && playerOneHealth <= 6);
                    playerTwoDangerSeen = playerTwoDangerSeen ||
                        (playerTwoHealth > 0 && playerTwoHealth <= 6);
                    if (bothAlive &&
                        (onlineMatch ||
                         !string.IsNullOrEmpty(roundEndCandidate)))
                    {
                        // Online teams and the CPU endurance fight can replace
                        // one combatant without ending the current round.
                        UpdateParticipants(
                            currentPlayerOneId,
                            currentPlayerOneName,
                            currentPlayerTwoId,
                            currentPlayerTwoName
                        );
                    }
                }
            }

            if (roundActive && roundEndedByPhase)
            {
                bool candidateAvailable =
                    !string.IsNullOrEmpty(roundEndCandidate);
                int eventActivePlayerCode = candidateAvailable
                    ? roundEndCandidateTeamActivePlayerCode
                    : (onlineMatch
                        ? lastStableTeamActivePlayerCode
                        : currentTeamActivePlayerCode);
                int eventLeftPlayerCount = candidateAvailable
                    ? roundEndCandidateTeamLeftPlayerCount
                    : (onlineMatch && lastStableTeamLeftPlayerCount > 0
                        ? lastStableTeamLeftPlayerCount
                        : currentTeamLeftPlayerCount);
                int eventControllerSlot = candidateAvailable
                    ? roundEndCandidateTeamControllerSlot
                    : (onlineMatch
                        ? lastStableTeamControllerSlot
                        : currentTeamControllerSlot);
                ClearRoundEndCandidate();
                FinishRound(
                    currentFinishPhase == 0x01,
                    currentFinishPhase == 0x02,
                    eventActivePlayerCode,
                    eventLeftPlayerCount,
                    eventControllerSlot
                );
            }
            else if (bothAlive && !roundActive && combatPhase)
            {
                ClearRoundEndCandidate();
                if (
                    !matchInProgress ||
                    matchCompleted ||
                    (!finishPhaseAvailable &&
                     !onlineMatch && ParticipantsChanged(
                        currentPlayerOneId,
                        currentPlayerTwoId
                    ))
                )
                {
                    StartMatch(
                        currentPlayerOneId,
                        currentPlayerOneName,
                        currentPlayerTwoId,
                        currentPlayerTwoName,
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                }
                else
                {
                    UpdateParticipants(
                        currentPlayerOneId,
                        currentPlayerOneName,
                        currentPlayerTwoId,
                        currentPlayerTwoName
                    );
                    roundNumber++;
                    roundActive = true;
                    playerOneDangerSeen = false;
                    playerTwoDangerSeen = false;
                    winnerSide = "";
                    winner = "";
                    RecordEvent(
                        "round-started",
                        "Раунд " + roundNumber + " начался",
                        ScoreDetails(),
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                }
            }
            else if (roundActive && combatPhase &&
                (!playerOneAlive || !playerTwoAlive))
            {
                if (!finishPhaseAvailable &&
                    onlineMatch &&
                    currentOnlinePlayerCount > 2 &&
                    TeamMemberAdvanced(
                        playerOneAlive,
                        playerTwoAlive,
                        lastStableTeamActivePlayerCode,
                        currentTeamActivePlayerCode
                    ))
                {
                    // In multiplayer Team Edition a zero health bar can mean
                    // that only the current team member was defeated. The
                    // corresponding nibble advances before the replacement
                    // fighter enters. Keep the same round and score until the
                    // last member of one side is actually defeated.
                    ClearRoundEndCandidate();
                    return Snapshot();
                }

                string candidate = RoundEndCandidate(playerOneAlive, playerTwoAlive);
                if (string.Equals(
                    candidate,
                    roundEndCandidate,
                    StringComparison.Ordinal
                ))
                {
                    roundEndCandidateSamples++;
                }
                else
                {
                    roundEndCandidate = candidate;
                    roundEndCandidateSamples = 1;
                    roundEndCandidateTeamActivePlayerCode =
                        onlineMatch
                            ? lastStableTeamActivePlayerCode
                            : currentTeamActivePlayerCode;
                    roundEndCandidateTeamLeftPlayerCount =
                        currentTeamLeftPlayerCount > 0
                            ? currentTeamLeftPlayerCount
                            : lastStableTeamLeftPlayerCount;
                    roundEndCandidateTeamControllerSlot = onlineMatch
                        ? lastStableTeamControllerSlot
                        : currentTeamControllerSlot;
                }
                if (!finishPhaseAvailable &&
                    roundEndCandidateSamples >= RequiredRoundEndSamples)
                {
                    int eventActivePlayerCode =
                        roundEndCandidateTeamActivePlayerCode;
                    int eventLeftPlayerCount =
                        roundEndCandidateTeamLeftPlayerCount;
                    int eventControllerSlot =
                        roundEndCandidateTeamControllerSlot;
                    ClearRoundEndCandidate();
                    FinishRound(
                        playerOneAlive,
                        playerTwoAlive,
                        eventActivePlayerCode,
                        eventLeftPlayerCount,
                        eventControllerSlot
                    );
                }
            }
            else if (!roundActive || !bothAlive || !combatPhase)
            {
                ClearRoundEndCandidate();
            }
            else if (bothAlive && combatPhase)
            {
                // A replacement fighter entered while the actual round phase
                // stayed active. The pending zero-health combatant was not the
                // end of the round.
                ClearRoundEndCandidate();
            }

            return Snapshot();
        }

        private void StartMatch(
            int currentPlayerOneId,
            string currentPlayerOneName,
            int currentPlayerTwoId,
            string currentPlayerTwoName,
            int currentTeamActivePlayerCode,
            int currentTeamLeftPlayerCount,
            int currentTeamControllerSlot
        )
        {
            matchInProgress = true;
            matchCompleted = false;
            roundActive = true;
            matchFullyObserved = true;
            matchSequence++;
            roundNumber = 1;
            playerOneRounds = 0;
            playerTwoRounds = 0;
            playerOneDangerSeen = false;
            playerTwoDangerSeen = false;
            UpdateParticipants(
                currentPlayerOneId,
                currentPlayerOneName,
                currentPlayerTwoId,
                currentPlayerTwoName
            );
            winnerSide = "";
            winner = "";
            RecordEvent(
                "match-started",
                "Бой начался",
                playerOneName + " против " + playerTwoName + ". Раунд 1.",
                currentTeamActivePlayerCode,
                currentTeamLeftPlayerCount,
                currentTeamControllerSlot
            );
        }

        private void UpdateParticipants(
            int currentPlayerOneId,
            string currentPlayerOneName,
            int currentPlayerTwoId,
            string currentPlayerTwoName
        )
        {
            playerOneId = currentPlayerOneId;
            playerTwoId = currentPlayerTwoId;
            playerOneName = currentPlayerOneName;
            playerTwoName = currentPlayerTwoName;
        }

        private bool ParticipantsChanged(
            int currentPlayerOneId,
            int currentPlayerTwoId
        )
        {
            return currentPlayerOneId != playerOneId ||
                currentPlayerTwoId != playerTwoId;
        }

        private void FinishRound(
            bool playerOneWon,
            bool playerTwoWon,
            int currentTeamActivePlayerCode,
            int currentTeamLeftPlayerCount,
            int currentTeamControllerSlot
        )
        {
            roundActive = false;
            if (playerOneWon && !playerTwoWon)
            {
                winnerSide = "p1";
                winner = playerOneName;
                playerOneRounds++;
                if (playerOneRounds >= 2)
                {
                    FinishMatch(
                        "p1",
                        playerOneName,
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                    return;
                }
                RecordEvent(
                    "round-ended",
                    "Раунд за P1 — " + playerOneName,
                    ScoreDetails(),
                    currentTeamActivePlayerCode,
                    currentTeamLeftPlayerCount,
                    currentTeamControllerSlot
                );
                return;
            }

            if (playerTwoWon && !playerOneWon)
            {
                winnerSide = "p2";
                winner = playerTwoName;
                playerTwoRounds++;
                if (playerTwoRounds >= 2)
                {
                    FinishMatch(
                        "p2",
                        playerTwoName,
                        currentTeamActivePlayerCode,
                        currentTeamLeftPlayerCount,
                        currentTeamControllerSlot
                    );
                    return;
                }
                RecordEvent(
                    "round-ended",
                    "Раунд за P2 — " + playerTwoName,
                    ScoreDetails(),
                    currentTeamActivePlayerCode,
                    currentTeamLeftPlayerCount,
                    currentTeamControllerSlot
                );
                return;
            }

            winnerSide = "";
            winner = "";
            RecordEvent(
                "round-draw",
                "Раунд завершён без победителя",
                ScoreDetails(),
                currentTeamActivePlayerCode,
                currentTeamLeftPlayerCount,
                currentTeamControllerSlot
            );
        }

        private void FinishMatch(
            string side,
            string fighterName,
            int currentTeamActivePlayerCode,
            int currentTeamLeftPlayerCount,
            int currentTeamControllerSlot
        )
        {
            matchInProgress = false;
            matchCompleted = true;
            winnerSide = side;
            winner = fighterName;
            RecordEvent(
                "match-ended",
                "Победитель матча — " + fighterName,
                "Итоговый счёт " + playerOneRounds + ":" + playerTwoRounds + ".",
                currentTeamActivePlayerCode,
                currentTeamLeftPlayerCount,
                currentTeamControllerSlot
            );
        }

        private string ScoreDetails()
        {
            return "Счёт по раундам " + playerOneRounds + ":" +
                playerTwoRounds + ".";
        }

        private void RecordEvent(
            string kind,
            string title,
            string details,
            int currentTeamActivePlayerCode,
            int currentTeamLeftPlayerCount,
            int currentTeamControllerSlot
        )
        {
            eventSequence++;
            eventTeamActivePlayerCode = currentTeamActivePlayerCode;
            eventTeamLeftPlayerCount = currentTeamLeftPlayerCount;
            eventTeamControllerSlot = currentTeamControllerSlot;
            eventKind = kind;
            eventTitle = title;
            eventDetails = details;
        }

        private static string RoundEndCandidate(
            bool playerOneAlive,
            bool playerTwoAlive
        )
        {
            if (playerOneAlive && !playerTwoAlive)
            {
                return "p1";
            }
            if (playerTwoAlive && !playerOneAlive)
            {
                return "p2";
            }
            return "draw";
        }

        private static bool TeamMemberAdvanced(
            bool playerOneAlive,
            bool playerTwoAlive,
            int stableActivePlayerCode,
            int currentActivePlayerCode
        )
        {
            int stableLeftMember = (stableActivePlayerCode >> 4) & 0x0F;
            int stableRightMember = stableActivePlayerCode & 0x0F;
            int currentLeftMember = (currentActivePlayerCode >> 4) & 0x0F;
            int currentRightMember = currentActivePlayerCode & 0x0F;

            return (!playerOneAlive &&
                    currentLeftMember != stableLeftMember) ||
                (!playerTwoAlive &&
                    currentRightMember != stableRightMember);
        }

        private void ClearRoundEndCandidate()
        {
            roundEndCandidate = "";
            roundEndCandidateSamples = 0;
            roundEndCandidateTeamActivePlayerCode = 0;
            roundEndCandidateTeamLeftPlayerCount = 0;
            roundEndCandidateTeamControllerSlot = -1;
        }

        private MameFightProgress Snapshot()
        {
            string state = "waiting";
            if (synchronizationRequired)
            {
                state = "synchronizing";
            }
            else if (matchCompleted)
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
                MatchSequence = matchSequence,
                FullyObserved = matchFullyObserved,
                PlayerOneDangerSeen = playerOneDangerSeen,
                PlayerTwoDangerSeen = playerTwoDangerSeen,
                WinnerSide = winnerSide,
                Winner = winner,
                EventSequence = eventSequence,
                EventTeamActivePlayerCode = eventTeamActivePlayerCode,
                EventTeamLeftPlayerCount = eventTeamLeftPlayerCount,
                EventTeamControllerSlot = eventTeamControllerSlot,
                EventKind = eventKind,
                EventTitle = eventTitle,
                EventDetails = eventDetails
            };
        }
    }
}

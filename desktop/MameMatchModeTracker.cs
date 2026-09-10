using System;

namespace MK3MoveBook
{
    internal sealed class MameMatchModeTracker
    {
        private bool hasConfirmedMode;
        private string confirmedMode;
        private string confirmedTowerPlayerSide;
        private int confirmedTowerId;
        private int confirmedPlayerOneId;
        private int confirmedPlayerTwoId;

        public MameMatchModeTracker()
        {
            Reset();
        }

        public void Reset()
        {
            hasConfirmedMode = false;
            confirmedMode = "";
            confirmedTowerPlayerSide = "";
            confirmedTowerId = -1;
            confirmedPlayerOneId = -1;
            confirmedPlayerTwoId = -1;
        }

        public MameMatchModeResult Resolve(
            MameMatchModeResult observedMode,
            MameFightProgress fight,
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
            if (observedMode == null)
            {
                throw new ArgumentNullException("observedMode");
            }
            if (fight == null)
            {
                Reset();
                return observedMode;
            }

            if (IsNewMatch(fight))
            {
                Reset();
            }
            if (string.Equals(
                fight.State,
                "waiting",
                StringComparison.Ordinal
            ))
            {
                Reset();
                return observedMode;
            }

            if (powerBarsEnabled == 1 &&
                fight.ParticipantsConfirmed &&
                IsConfirmedMode(observedMode.Mode))
            {
                Capture(observedMode, fight);
                return observedMode;
            }

            if (!string.Equals(
                    fight.State,
                    "match-ended",
                    StringComparison.Ordinal
                ) ||
                powerBarsEnabled != 0 ||
                !MatchesConfirmedFight(fight))
            {
                return observedMode;
            }

            MameMatchModeResult completionMode =
                MameMatchModeClassifier.Classify(
                    1,
                    towerStateA,
                    towerStateB,
                    towerId,
                    towerFightNumber,
                    playerTwoControlState,
                    playerOneParticipantState,
                    playerTwoParticipantState
                );
            return MatchesConfirmedMode(completionMode)
                ? completionMode
                : observedMode;
        }

        private void Capture(
            MameMatchModeResult mode,
            MameFightProgress fight
        )
        {
            hasConfirmedMode = true;
            confirmedMode = mode.Mode ?? "";
            confirmedTowerPlayerSide = mode.TowerPlayerSide ?? "";
            confirmedTowerId = mode.TowerId;
            confirmedPlayerOneId = fight.PlayerOneId;
            confirmedPlayerTwoId = fight.PlayerTwoId;
        }

        private bool MatchesConfirmedFight(MameFightProgress fight)
        {
            return hasConfirmedMode &&
                fight.ParticipantsConfirmed &&
                fight.PlayerOneId == confirmedPlayerOneId &&
                fight.PlayerTwoId == confirmedPlayerTwoId;
        }

        private bool MatchesConfirmedMode(MameMatchModeResult mode)
        {
            if (mode == null ||
                !string.Equals(
                    mode.Mode,
                    confirmedMode,
                    StringComparison.Ordinal
                ) ||
                !string.Equals(
                    mode.TowerPlayerSide ?? "",
                    confirmedTowerPlayerSide,
                    StringComparison.Ordinal
                ))
            {
                return false;
            }
            return !mode.TowerContext || mode.TowerId == confirmedTowerId;
        }

        private static bool IsConfirmedMode(string mode)
        {
            return string.Equals(
                    mode,
                    "tower-cpu",
                    StringComparison.Ordinal
                ) ||
                string.Equals(
                    mode,
                    "two-player",
                    StringComparison.Ordinal
                );
        }

        private static bool IsNewMatch(MameFightProgress fight)
        {
            return string.Equals(
                    fight.State,
                    "round-active",
                    StringComparison.Ordinal
                ) &&
                string.Equals(
                    fight.EventKind,
                    "match-started",
                    StringComparison.Ordinal
                );
        }
    }
}

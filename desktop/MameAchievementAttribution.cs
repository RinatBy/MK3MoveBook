using System;

namespace MK3MoveBook
{
    internal static class MameAchievementAttribution
    {
        public static bool TryNormalizeForProfile(
            string matchMode,
            string towerPlayerSide,
            int towerId,
            int currentTowerOpponentId,
            string romId,
            int teamActivePlayerCode,
            bool netplayLayoutSupported,
            bool netplayActive,
            int netplayPlayerNumber,
            int netplayTotalPlayers,
            MoveBookAchievementEvent achievementEvent
        )
        {
            int teamLeftPlayerCount = netplayTotalPlayers == 3
                ? 0
                : netplayTotalPlayers / 2;
            return TryNormalizeForProfile(
                matchMode,
                towerPlayerSide,
                towerId,
                currentTowerOpponentId,
                romId,
                teamActivePlayerCode,
                teamLeftPlayerCount,
                -1,
                netplayLayoutSupported,
                netplayActive,
                netplayPlayerNumber,
                netplayTotalPlayers,
                achievementEvent
            );
        }

        public static bool TryNormalizeForProfile(
            string matchMode,
            string towerPlayerSide,
            int towerId,
            int currentTowerOpponentId,
            string romId,
            int teamActivePlayerCode,
            int teamLeftPlayerCount,
            int teamControllerSlot,
            bool netplayLayoutSupported,
            bool netplayActive,
            int netplayPlayerNumber,
            int netplayTotalPlayers,
            MoveBookAchievementEvent achievementEvent
        )
        {
            if (achievementEvent == null)
            {
                return false;
            }
            if (!MameAchievementEligibilityPolicy.IsSupportedRom(romId))
            {
                return false;
            }
            if (string.Equals(
                matchMode,
                "tower-cpu",
                StringComparison.Ordinal
            ))
            {
                bool playerTwoTower = string.Equals(
                    towerPlayerSide,
                    "p2",
                    StringComparison.Ordinal
                );
                bool playerOneTower = string.Equals(
                    towerPlayerSide,
                    "p1",
                    StringComparison.Ordinal
                );
                if (!playerOneTower && !playerTwoTower)
                {
                    return false;
                }
                if (playerTwoTower)
                {
                    SwapPlayers(achievementEvent);
                }

                achievementEvent.MatchScope = "computer";
                achievementEvent.NetplayTotalPlayers = 0;
                achievementEvent.TowerId = towerId >= 0 && towerId <= 3
                    ? towerId
                    : -1;
                // The raw character slot may already contain the next tower
                // opponent while the fight tracker is publishing the previous
                // match result. Use the participants frozen into the event.
                achievementEvent.TowerCompleted =
                    achievementEvent.TowerId >= 0 &&
                    achievementEvent.PlayerTwoId == 0x19 &&
                    string.Equals(
                        achievementEvent.EventType,
                        "match-ended",
                        StringComparison.Ordinal
                    ) &&
                    string.Equals(
                        achievementEvent.WinnerSide,
                        "p1",
                        StringComparison.Ordinal
                    );
                return true;
            }

            if (string.Equals(
                    matchMode,
                    "two-player",
                    StringComparison.Ordinal
                ) &&
                !netplayActive)
            {
                // Local Versus must never affect a server profile. Test builds
                // used to normalize these events with an empty scope, which
                // allowed them to enter the offline queue and general stats.
                return false;
            }

            // Unknown modes are intentionally excluded. Achievement
            // attribution is limited to a strict two-participant Kaillera
            // match; the team tracker remains available for diagnostics.
            if (!string.Equals(
                    matchMode,
                    "two-player",
                    StringComparison.Ordinal
                ) ||
                !netplayLayoutSupported ||
                !netplayActive ||
                netplayTotalPlayers != 2 ||
                (netplayPlayerNumber != 1 && netplayPlayerNumber != 2))
            {
                return false;
            }

            string localSide = netplayPlayerNumber == 1 ? "p1" : "p2";

            if (string.Equals(localSide, "p2", StringComparison.Ordinal))
            {
                SwapPlayers(achievementEvent);
            }
            achievementEvent.MatchScope = "online";
            achievementEvent.NetplayTotalPlayers = netplayTotalPlayers;
            achievementEvent.TowerId = -1;
            achievementEvent.TowerCompleted = false;
            return true;
        }

        private static void SwapPlayers(
            MoveBookAchievementEvent achievementEvent
        )
        {
            achievementEvent.WinnerSide = SwapSide(
                achievementEvent.WinnerSide
            );
            int playerOneId = achievementEvent.PlayerOneId;
            achievementEvent.PlayerOneId = achievementEvent.PlayerTwoId;
            achievementEvent.PlayerTwoId = playerOneId;
            int playerOneHealth = achievementEvent.PlayerOneHealth;
            achievementEvent.PlayerOneHealth =
                achievementEvent.PlayerTwoHealth;
            achievementEvent.PlayerTwoHealth = playerOneHealth;
            int playerOneRounds = achievementEvent.PlayerOneRounds;
            achievementEvent.PlayerOneRounds =
                achievementEvent.PlayerTwoRounds;
            achievementEvent.PlayerTwoRounds = playerOneRounds;
            achievementEvent.FinisherWinnerSide = SwapSide(
                achievementEvent.FinisherWinnerSide
            );
            bool playerOneDangerSeen = achievementEvent.PlayerOneDangerSeen;
            achievementEvent.PlayerOneDangerSeen =
                achievementEvent.PlayerTwoDangerSeen;
            achievementEvent.PlayerTwoDangerSeen = playerOneDangerSeen;
            achievementEvent.SpecialActorSide = SwapSide(
                achievementEvent.SpecialActorSide
            );
        }

        private static string SwapSide(string side)
        {
            if (string.Equals(side, "p1", StringComparison.Ordinal))
            {
                return "p2";
            }
            if (string.Equals(side, "p2", StringComparison.Ordinal))
            {
                return "p1";
            }
            return side ?? "";
        }
    }
}

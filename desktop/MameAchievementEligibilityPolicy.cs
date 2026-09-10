using System;

namespace MK3MoveBook
{
    internal static class MameAchievementEligibilityPolicy
    {
        public static bool IsSupportedRom(string romId)
        {
            return string.Equals(romId, "umk3tm", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(romId, "umk3tm60", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(romId, "umk3uk", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(romId, "umk3uk2", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsSnapshotEligible(
            string romId,
            bool matchFullyObserved,
            string matchMode,
            string cpuAchievementPolicy,
            bool netplayLayoutSupported,
            bool netplayActive,
            int netplayPlayerNumber,
            int netplayTotalPlayers,
            int teamSize
        )
        {
            return SnapshotExclusionReason(
                romId, matchFullyObserved, matchMode, cpuAchievementPolicy,
                netplayLayoutSupported, netplayActive, netplayPlayerNumber,
                netplayTotalPlayers, teamSize).Length == 0;
        }

        internal static string SnapshotExclusionReason(
            string romId, bool matchFullyObserved, string matchMode,
            string cpuAchievementPolicy, bool netplayLayoutSupported,
            bool netplayActive, int netplayPlayerNumber,
            int netplayTotalPlayers, int teamSize)
        {
            if (!IsSupportedRom(romId)) return "unsupported-rom";
            if (!matchFullyObserved) return "partial-match";
            if (string.Equals(matchMode, "tower-cpu", StringComparison.Ordinal))
            {
                if (netplayActive) return "cpu-with-netplay";
                return string.Equals(cpuAchievementPolicy, "verified-clean",
                    StringComparison.Ordinal) ? "" : "cpu-cheats-not-verified-clean";
            }
            if (!string.Equals(matchMode, "two-player", StringComparison.Ordinal))
                return "unknown-mode";
            if (!netplayActive) return "local-versus";
            if (!netplayLayoutSupported) return "unknown-netplay-layout";
            if (netplayTotalPlayers != 2) return "netplay-not-two-players";
            if (teamSize != 1) return "team-not-one-fighter";
            if (netplayPlayerNumber != 1 && netplayPlayerNumber != 2)
                return "unknown-local-player";
            return "";
        }

        public static bool IsQueuedEventEligible(
            MoveBookAchievementEvent achievementEvent
        )
        {
            if (achievementEvent == null ||
                !IsSupportedRom(achievementEvent.RomId) ||
                !achievementEvent.MatchFullyObserved)
            {
                return false;
            }
            if (string.Equals(
                achievementEvent.MatchScope,
                "computer",
                StringComparison.Ordinal
            ))
            {
                return achievementEvent.NetplayTotalPlayers == 0 &&
                    string.Equals(
                        achievementEvent.CheatPolicyState,
                        "disabled",
                        StringComparison.Ordinal
                    );
            }
            return string.Equals(
                    achievementEvent.MatchScope,
                    "online",
                    StringComparison.Ordinal
                ) &&
                achievementEvent.NetplayTotalPlayers == 2 &&
                achievementEvent.TeamSize == 1;
        }
    }
}

using System;
using System.Collections.Generic;

namespace MK3MoveBook
{
    internal sealed class MameTeamLayoutTracker
    {
        private int observedTotalPlayers;
        private int leftTeamPlayers;

        public void Reset()
        {
            observedTotalPlayers = 0;
            leftTeamPlayers = 0;
        }

        public int Observe(
            string romId,
            int totalPlayers,
            int activePlayerCode
        )
        {
            return Observe(romId, totalPlayers, activePlayerCode, "");
        }

        public int Observe(
            string romId,
            int totalPlayers,
            int activePlayerCode,
            string controllerOrder
        )
        {
            if (!MameTeamPlayerAttribution.IsTeamEdition(romId) ||
                totalPlayers < 3 || totalPlayers > 6)
            {
                Reset();
                return 0;
            }

            if (totalPlayers != observedTotalPlayers)
            {
                observedTotalPlayers = totalPlayers;
                leftTeamPlayers = 0;
            }

            if (totalPlayers == 3)
            {
                int activeLeftMember = (activePlayerCode >> 4) & 0x0F;
                int activeRightMember = activePlayerCode & 0x0F;
                if (activeLeftMember == 1 && activeRightMember == 0)
                {
                    leftTeamPlayers = 2;
                }
                else if (activeLeftMember == 0 && activeRightMember == 1)
                {
                    leftTeamPlayers = 1;
                }
                return leftTeamPlayers;
            }

            leftTeamPlayers = totalPlayers / 2;
            return leftTeamPlayers;
        }
    }

    internal static class MameTeamPlayerAttribution
    {
        internal static bool IsTeamEdition(string romId)
        {
            // The paired program ROMs are byte-identical. This only enables
            // team diagnostics; the achievement gate still excludes 3-6 players.
            return string.Equals(romId, "umk3tm", StringComparison.Ordinal) ||
                string.Equals(romId, "umk3tm60", StringComparison.Ordinal);
        }

        public static bool TryResolveControllerSlot(
            int totalPlayers,
            int localPlayerNumber,
            IList<int> controllerMap,
            out int localControllerSlot,
            out string controllerOrder
        )
        {
            localControllerSlot = -1;
            controllerOrder = "";
            if (totalPlayers < 1 || totalPlayers > 8 ||
                localPlayerNumber < 1 ||
                localPlayerNumber > totalPlayers ||
                controllerMap == null ||
                controllerMap.Count < totalPlayers)
            {
                return false;
            }

            bool[] seen = new bool[totalPlayers];
            string[] order = new string[totalPlayers];
            int localIdentity = localPlayerNumber - 1;
            for (int slot = 0; slot < totalPlayers; slot++)
            {
                int identity = controllerMap[slot];
                if (identity < 0 || identity >= totalPlayers ||
                    seen[identity])
                {
                    return false;
                }
                seen[identity] = true;
                order[slot] = "P" + (identity + 1);
                if (identity == localIdentity)
                {
                    localControllerSlot = slot;
                }
            }

            if (localControllerSlot < 0)
            {
                return false;
            }
            controllerOrder = string.Join("/", order);
            return true;
        }

        public static bool TryResolveActiveSide(
            string romId,
            int totalPlayers,
            int localPlayerNumber,
            int activePlayerCode,
            out string localSide
        )
        {
            // A room number alone cannot tell whether that participant is
            // currently one of the two active controllers.
            localSide = "";
            return false;
        }

        public static bool TryResolveActiveSide(
            string romId,
            int totalPlayers,
            int localPlayerNumber,
            int activePlayerCode,
            int leftTeamPlayers,
            out string localSide
        )
        {
            // The active permutation read from MAME is required. Do not fall
            // back to the participant's room position.
            localSide = "";
            return false;
        }

        public static bool TryResolveActiveSide(
            string romId,
            int totalPlayers,
            int localPlayerNumber,
            int localControllerSlot,
            int activePlayerCode,
            int leftTeamPlayers,
            out string localSide
        )
        {
            localSide = "";
            if (!IsTeamEdition(romId))
            {
                return false;
            }

            if (totalPlayers < 3 || totalPlayers > 6)
            {
                return false;
            }

            if (localPlayerNumber < 1 || localPlayerNumber > totalPlayers)
            {
                return false;
            }

            if (localControllerSlot < 0 ||
                localControllerSlot >= totalPlayers)
            {
                return false;
            }

            // The researched MAME table is a live controller permutation:
            // element 0 is the active controller on the left, element 1 is
            // the active controller on the right, and elements 2+ are the
            // waiting teammates. This remains true when fighters rotate.
            if (localControllerSlot > 1)
            {
                return false;
            }

            localSide = localControllerSlot == 0 ? "p1" : "p2";
            return true;
        }
    }
}

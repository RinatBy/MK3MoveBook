using System;

namespace MK3MoveBook
{
    internal static class MameTeamPlayerAttributionTests
    {
        private static int failures;

        private static int Main()
        {
            TestLiveControllerPermutation();
            TestLatestSixPlayerCaptureMapping();
            TestThreePlayerMapping();
            TestThreePlayerLayoutMemory();
            TestInvalidInputsAreBlocked();

            if (failures == 0)
            {
                Console.WriteLine(
                    "MameTeamPlayerAttribution: all tests passed."
                );
                return 0;
            }
            Console.Error.WriteLine(
                "MameTeamPlayerAttribution: " + failures +
                " test(s) failed."
            );
            return 1;
        }

        private static void TestLiveControllerPermutation()
        {
            int slot;
            string order;
            Equal(
                "live map resolves active P2",
                true,
                MameTeamPlayerAttribution.TryResolveControllerSlot(
                    6,
                    2,
                    new int[] { 1, 5, 0, 2, 3, 4 },
                    out slot,
                    out order
                )
            );
            Equal("active P2 position", 0, slot);
            Equal(
                "live controller order",
                "P2/P6/P1/P3/P4/P5",
                order
            );

            Equal(
                "live map resolves waiting P2",
                true,
                MameTeamPlayerAttribution.TryResolveControllerSlot(
                    6,
                    2,
                    new int[] { 2, 3, 0, 1, 4, 5 },
                    out slot,
                    out order
                )
            );
            Equal("waiting P2 position", 3, slot);
        }

        private static void TestLatestSixPlayerCaptureMapping()
        {
            ActivePosition("P2 active on left", 6, 2, 0, "p1");
            ActivePosition("P5 active on right", 6, 5, 1, "p2");
            BlockedPosition("P2 waiting while P3 fights", 6, 2, 3);
        }

        private static void TestThreePlayerMapping()
        {
            ActivePosition("three-player P2 left", 3, 2, 0, "p1");
            ActivePosition("three-player P3 right", 3, 3, 1, "p2");
            BlockedPosition("three-player P1 waiting", 3, 1, 2);
        }

        private static void TestThreePlayerLayoutMemory()
        {
            MameTeamLayoutTracker tracker = new MameTeamLayoutTracker();
            Equal(
                "three-player split initially unknown",
                0,
                tracker.Observe("umk3tm", 3, 0x00, "P1/P2/P3")
            );
            Equal(
                "detect 2x1 split",
                2,
                tracker.Observe("umk3tm", 3, 0x10, "P2/P3/P1")
            );
            Equal(
                "live permutation change keeps split",
                2,
                tracker.Observe("umk3tm", 3, 0x00, "P1/P3/P2")
            );
            tracker.Reset();
            Equal(
                "detect 1x2 split",
                1,
                tracker.Observe("umk3tm", 3, 0x01, "P1/P3/P2")
            );
        }

        private static void TestInvalidInputsAreBlocked()
        {
            int slot;
            string order;
            Equal(
                "duplicate permutation blocked",
                false,
                MameTeamPlayerAttribution.TryResolveControllerSlot(
                    3,
                    2,
                    new int[] { 0, 1, 1 },
                    out slot,
                    out order
                )
            );
            Equal(
                "out-of-range permutation blocked",
                false,
                MameTeamPlayerAttribution.TryResolveControllerSlot(
                    3,
                    2,
                    new int[] { 0, 1, 3 },
                    out slot,
                    out order
                )
            );
            string side;
            Equal(
                "room number without live position blocked",
                false,
                MameTeamPlayerAttribution.TryResolveActiveSide(
                    "umk3tm",
                    6,
                    2,
                    0x10,
                    out side
                )
            );
            Equal(
                "other ROM blocked",
                false,
                MameTeamPlayerAttribution.TryResolveActiveSide(
                    "umk3uk",
                    6,
                    2,
                    0,
                    0,
                    3,
                    out side
                )
            );
            Equal(
                "unsupported player count blocked",
                false,
                MameTeamPlayerAttribution.TryResolveActiveSide(
                    "umk3tm",
                    7,
                    2,
                    0,
                    0,
                    3,
                    out side
                )
            );
        }

        private static void ActivePosition(
            string name,
            int totalPlayers,
            int playerNumber,
            int livePosition,
            string expectedSide
        )
        {
            string side;
            bool resolved = MameTeamPlayerAttribution.TryResolveActiveSide(
                "umk3tm",
                totalPlayers,
                playerNumber,
                livePosition,
                0,
                totalPlayers / 2,
                out side
            );
            Equal(name + " resolved", true, resolved);
            Equal(name + " side", expectedSide, side);
        }

        private static void BlockedPosition(
            string name,
            int totalPlayers,
            int playerNumber,
            int livePosition
        )
        {
            string side;
            Equal(
                name,
                false,
                MameTeamPlayerAttribution.TryResolveActiveSide(
                    "umk3tm",
                    totalPlayers,
                    playerNumber,
                    livePosition,
                    0,
                    totalPlayers / 2,
                    out side
                )
            );
        }

        private static void Equal<T>(string name, T expected, T actual)
        {
            if (object.Equals(expected, actual))
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(
                name + ": expected " + expected + ", actual " + actual
            );
        }
    }
}

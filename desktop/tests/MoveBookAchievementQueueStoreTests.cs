using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;

namespace MK3MoveBook
{
    internal static class MoveBookAchievementQueueStoreTests
    {
        private static int failures;

        private static int Main()
        {
            string root = Path.Combine(
                Path.GetTempPath(),
                "MK3MoveBookAchievementQueueStoreTests-" +
                    Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(root);
            try
            {
                TestRoundTrip(root);
                TestProfileAndApiIsolation(root);
                TestClearKeepsOtherProfiles(root);
                TestAcknowledgementKeepsOnlyPendingEvents(root);
                TestCompletedTemporaryFileRecovery(root);
                TestUnreadableFileIsQuarantined(root);
            }
            finally
            {
                try
                {
                    Directory.Delete(root, true);
                }
                catch
                {
                    // Test output is intentionally best-effort cleanup.
                }
            }

            if (failures == 0)
            {
                Console.WriteLine(
                    "MoveBookAchievementQueueStore: 6 tests passed."
                );
                return 0;
            }
            Console.Error.WriteLine(
                "MoveBookAchievementQueueStore: " + failures +
                " test(s) failed."
            );
            return 1;
        }

        private static void TestRoundTrip(string root)
        {
            string directory = Path.Combine(root, "round-trip");
            MoveBookAchievementQueueStore store = CreateStore(
                directory,
                "https://api.movebook.ru"
            );
            store.Save(7, new[] { CreateEvent("event-key-round-trip") });

            MoveBookAchievementQueueLoadResult loaded = store.Load(7);

            Equal("round-trip readable", false, loaded.HadUnreadableData);
            Equal("round-trip count", 1, loaded.Events.Count);
            Equal(
                "round-trip key",
                "event-key-round-trip",
                loaded.Events[0].EventKey
            );
            Equal(
                "round-trip UTC",
                DateTimeKind.Utc,
                loaded.Events[0].OccurredAtUtc.Kind
            );
            Equal("round-trip scope", "computer", loaded.Events[0].MatchScope);
            Equal(
                "round-trip cheat policy",
                "disabled",
                loaded.Events[0].CheatPolicyState
            );
            Equal("round-trip full match", true,
                loaded.Events[0].MatchFullyObserved);
            Equal("round-trip net player count", 0,
                loaded.Events[0].NetplayTotalPlayers);
            Equal("round-trip team size", 0,
                loaded.Events[0].TeamSize);
            Equal("round-trip tower id", 3, loaded.Events[0].TowerId);
            Equal(
                "round-trip tower completion",
                true,
                loaded.Events[0].TowerCompleted
            );
        }

        private static void TestProfileAndApiIsolation(string root)
        {
            string directory = Path.Combine(root, "isolation");
            MoveBookAchievementQueueStore production = CreateStore(
                directory,
                "https://api.movebook.ru"
            );
            MoveBookAchievementQueueStore local = CreateStore(
                directory,
                "http://127.0.0.1:8010"
            );
            production.Save(11, new[] { CreateEvent("event-key-production") });

            Equal("other user empty", 0, production.Load(12).Events.Count);
            Equal("other API empty", 0, local.Load(11).Events.Count);
            Equal("source profile kept", 1, production.Load(11).Events.Count);
        }

        private static void TestClearKeepsOtherProfiles(string root)
        {
            string directory = Path.Combine(root, "clear");
            MoveBookAchievementQueueStore store = CreateStore(
                directory,
                "https://api.movebook.ru"
            );
            store.Save(21, new[] { CreateEvent("event-key-user-21") });
            store.Save(22, new[] { CreateEvent("event-key-user-22") });

            store.Clear(21);

            Equal("cleared profile empty", 0, store.Load(21).Events.Count);
            Equal("other profile retained", 1, store.Load(22).Events.Count);
        }

        private static void TestCompletedTemporaryFileRecovery(string root)
        {
            string directory = Path.Combine(root, "temporary-recovery");
            MoveBookAchievementQueueStore store = CreateStore(
                directory,
                "https://api.movebook.ru"
            );
            store.Save(31, new[] { CreateEvent("event-key-temporary") });
            string queueDirectory = Path.Combine(directory, "AchievementOutbox");
            string queuePath = Directory.GetFiles(
                queueDirectory,
                "outbox-v1-*.bin"
            )[0];
            File.Move(queuePath, queuePath + ".tmp");

            MoveBookAchievementQueueLoadResult loaded = store.Load(31);

            Equal("temporary recovered", 1, loaded.Events.Count);
            Equal("temporary promoted", true, File.Exists(queuePath));
        }

        private static void TestAcknowledgementKeepsOnlyPendingEvents(
            string root
        )
        {
            string directory = Path.Combine(root, "acknowledgement");
            MoveBookAchievementQueueStore store = CreateStore(
                directory,
                "https://api.movebook.ru"
            );
            store.Save(51, new[]
            {
                CreateEvent("event-key-first-pending"),
                CreateEvent("event-key-second-pending")
            });

            List<MoveBookAchievementEvent> pending = store.Load(51).Events;
            pending.RemoveAt(0);
            store.Save(51, pending);

            MoveBookAchievementQueueLoadResult afterFirstAck = store.Load(51);
            Equal("one pending after ack", 1, afterFirstAck.Events.Count);
            Equal(
                "second event retained",
                "event-key-second-pending",
                afterFirstAck.Events[0].EventKey
            );

            store.Save(51, new MoveBookAchievementEvent[0]);
            Equal("empty queue clears file", 0, store.Load(51).Events.Count);
        }

        private static void TestUnreadableFileIsQuarantined(string root)
        {
            string directory = Path.Combine(root, "quarantine");
            MoveBookAchievementQueueStore store = CreateStore(
                directory,
                "https://api.movebook.ru"
            );
            store.Save(41, new[] { CreateEvent("event-key-corrupt") });
            string queueDirectory = Path.Combine(directory, "AchievementOutbox");
            string queuePath = Directory.GetFiles(
                queueDirectory,
                "outbox-v1-*.bin"
            )[0];
            File.WriteAllBytes(queuePath, new byte[] { 1, 2, 3, 4 });

            MoveBookAchievementQueueLoadResult loaded = store.Load(41);

            Equal("corrupt reported", true, loaded.HadUnreadableData);
            Equal("corrupt not loaded", 0, loaded.Events.Count);
            Equal(
                "corrupt quarantined",
                1,
                Directory.GetFiles(
                    queueDirectory,
                    "*.unreadable-*"
                ).Length
            );
        }

        private static MoveBookAchievementQueueStore CreateStore(
            string directory,
            string apiBaseUrl
        )
        {
            return new MoveBookAchievementQueueStore(
                directory,
                apiBaseUrl,
                new JavaScriptSerializer()
            );
        }

        private static MoveBookAchievementEvent CreateEvent(string eventKey)
        {
            return new MoveBookAchievementEvent
            {
                EventKey = eventKey,
                EventType = "match-ended",
                RomId = "umk3tm",
                MatchScope = "computer",
                CheatPolicyState = "disabled",
                MatchFullyObserved = true,
                NetplayTotalPlayers = 0,
                TeamSize = 0,
                TowerId = 3,
                TowerCompleted = true,
                WinnerSide = "p1",
                PlayerOneId = 3,
                PlayerTwoId = 18,
                PlayerOneHealth = 166,
                PlayerTwoHealth = 0,
                FinisherKind = "",
                FinisherWinnerSide = "",
                FinisherFighterId = -1,
                FinisherOpponentId = -1,
                OccurredAtUtc = new DateTime(
                    2026,
                    7,
                    26,
                    15,
                    34,
                    9,
                    DateTimeKind.Utc
                )
            };
        }

        private static void Equal<T>(string name, T expected, T actual)
        {
            if (EqualityComparer<T>.Default.Equals(expected, actual))
            {
                return;
            }
            failures += 1;
            Console.Error.WriteLine(
                name + ": expected " + expected + ", got " + actual
            );
        }
    }
}

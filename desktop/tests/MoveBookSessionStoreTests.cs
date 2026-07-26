using System;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace MK3MoveBook
{
    internal static class MoveBookSessionStoreTests
    {
        private static int failures;

        private static int Main()
        {
            string testDirectory = Path.Combine(
                Path.GetTempPath(),
                "MK3MoveBookSessionStoreTests-" + Guid.NewGuid().ToString("N")
            );
            try
            {
                TestRoundTrip(testDirectory);
                TestApiBinding(testDirectory);
                TestCorruptPayload(testDirectory);
                TestClear(testDirectory);
            }
            finally
            {
                if (Directory.Exists(testDirectory))
                {
                    Directory.Delete(testDirectory, true);
                }
            }

            if (failures == 0)
            {
                Console.WriteLine("MoveBookSessionStore: 4 tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MoveBookSessionStore: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static void TestRoundTrip(string testDirectory)
        {
            MoveBookSessionStore store = CreateStore(
                testDirectory,
                "http://127.0.0.1:8010"
            );
            store.Save("refresh-secret-value");
            MoveBookStoredSession session = store.Load();
            Equal(
                "round-trip token",
                "refresh-secret-value",
                session == null ? "" : session.RefreshToken
            );

            string storedText = Encoding.UTF8.GetString(File.ReadAllBytes(
                Path.Combine(testDirectory, "auth-session-v1.bin")
            ));
            Equal(
                "token is not plaintext",
                false,
                storedText.Contains("refresh-secret-value")
            );
        }

        private static void TestApiBinding(string testDirectory)
        {
            MoveBookSessionStore otherApi = CreateStore(
                testDirectory,
                "https://api.movebook.ru"
            );
            Equal("different API ignored", null, otherApi.Load());
        }

        private static void TestCorruptPayload(string testDirectory)
        {
            File.WriteAllText(
                Path.Combine(testDirectory, "auth-session-v1.bin"),
                "not-a-protected-session"
            );
            MoveBookSessionStore store = CreateStore(
                testDirectory,
                "http://127.0.0.1:8010"
            );
            Equal("corrupt payload ignored", null, store.Load());
            Equal(
                "corrupt payload removed",
                false,
                File.Exists(Path.Combine(
                    testDirectory,
                    "auth-session-v1.bin"
                ))
            );
        }

        private static void TestClear(string testDirectory)
        {
            MoveBookSessionStore store = CreateStore(
                testDirectory,
                "http://127.0.0.1:8010"
            );
            store.Save("another-refresh-token");
            store.Clear();
            Equal("cleared session", null, store.Load());
        }

        private static MoveBookSessionStore CreateStore(
            string testDirectory,
            string apiBaseUrl
        )
        {
            return new MoveBookSessionStore(
                testDirectory,
                apiBaseUrl,
                new JavaScriptSerializer()
            );
        }

        private static void Equal(string name, object expected, object actual)
        {
            if (object.Equals(expected, actual))
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(
                name + ": expected " + expected + ", got " + actual + "."
            );
        }
    }
}

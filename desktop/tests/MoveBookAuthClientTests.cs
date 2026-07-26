using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace MK3MoveBook
{
    internal static class MoveBookAuthClientTests
    {
        private static int failures;

        private static int Main()
        {
            RunAsync(TestCreateDeviceSession, "Create device session");
            RunAsync(TestPendingAndAuthorizedPoll, "Poll authorization");
            RunAsync(TestRefresh, "Refresh authorization");
            RunAsync(TestLogout, "Logout");
            TestRemoteHttpIsRejected();

            if (failures == 0)
            {
                Console.WriteLine("MoveBookAuthClient: 5 tests passed.");
                return 0;
            }
            Console.Error.WriteLine(
                "MoveBookAuthClient: " + failures + " test(s) failed."
            );
            return 1;
        }

        private static async Task TestCreateDeviceSession()
        {
            QueueHandler handler = new QueueHandler();
            handler.Enqueue(
                HttpStatusCode.Created,
                "{\"device_code\":\"device-secret\"," +
                "\"verification_uri\":\"http://127.0.0.1:8010/start\"," +
                "\"expires_in\":300,\"interval\":2}"
            );
            using (MoveBookAuthClient client = CreateClient(handler))
            {
                MoveBookDeviceSession session =
                    await client.CreateDeviceSessionAsync(
                        CancellationToken.None
                    );
                Equal("device code", "device-secret", session.DeviceCode);
                Equal("expires", 300, session.ExpiresIn);
                Equal("interval", 2, session.Interval);
                Equal("create method", "POST", handler.Requests[0].Method);
                Equal(
                    "create path",
                    "/api/v1/auth/device-sessions",
                    handler.Requests[0].Path
                );
            }
        }

        private static async Task TestPendingAndAuthorizedPoll()
        {
            QueueHandler handler = new QueueHandler();
            handler.Enqueue(
                HttpStatusCode.Accepted,
                "{\"status\":\"pending\",\"interval\":3}"
            );
            handler.Enqueue(
                HttpStatusCode.OK,
                "{\"status\":\"authorized\"," +
                "\"access_token\":\"access\"," +
                "\"refresh_token\":\"refresh-secret\"," +
                "\"user\":{\"id\":7,\"display_name\":\"uPlay\"," +
                "\"avatar_url\":null,\"providers\":[\"discord\"]}}"
            );
            using (MoveBookAuthClient client = CreateClient(handler))
            {
                MoveBookAuthPollResult pending =
                    await client.PollDeviceSessionAsync(
                        "device-secret",
                        CancellationToken.None
                    );
                Equal("pending", true, pending.IsPending);
                Equal("pending interval", 3, pending.Interval);

                MoveBookAuthPollResult authorized =
                    await client.PollDeviceSessionAsync(
                        "device-secret",
                        CancellationToken.None
                    );
                Equal("authorized", false, authorized.IsPending);
                Equal("user id", 7, authorized.User.Id);
                Equal("display name", "uPlay", authorized.User.DisplayName);
                Equal("refresh token", "refresh-secret", authorized.RefreshToken);
                Contains(
                    "poll body",
                    handler.Requests[0].Body,
                    "device-secret"
                );
            }
        }

        private static async Task TestLogout()
        {
            QueueHandler handler = new QueueHandler();
            handler.Enqueue(HttpStatusCode.OK, "{\"ok\":true}");
            using (MoveBookAuthClient client = CreateClient(handler))
            {
                await client.LogoutAsync(
                    "refresh-secret",
                    CancellationToken.None
                );
            }
            Equal(
                "logout path",
                "/api/v1/auth/logout",
                handler.Requests[0].Path
            );
            Contains("logout body", handler.Requests[0].Body, "refresh-secret");
        }

        private static async Task TestRefresh()
        {
            QueueHandler handler = new QueueHandler();
            handler.Enqueue(
                HttpStatusCode.OK,
                "{\"status\":\"authorized\"," +
                "\"access_token\":\"new-access\"," +
                "\"refresh_token\":\"new-refresh\"," +
                "\"user\":{\"id\":7,\"display_name\":\"uPlay\"," +
                "\"avatar_url\":null,\"providers\":[\"discord\"]}}"
            );
            using (MoveBookAuthClient client = CreateClient(handler))
            {
                MoveBookAuthPollResult result = await client.RefreshAsync(
                    "old-refresh",
                    CancellationToken.None
                );
                Equal("refresh access", "new-access", result.AccessToken);
                Equal("refresh token", "new-refresh", result.RefreshToken);
                Equal("refresh user", 7, result.User.Id);
            }
            Equal("refresh path", "/api/v1/auth/refresh", handler.Requests[0].Path);
            Contains("refresh body", handler.Requests[0].Body, "old-refresh");
        }

        private static void TestRemoteHttpIsRejected()
        {
            bool rejected = false;
            try
            {
                using (MoveBookAuthClient client = new MoveBookAuthClient(
                    "http://movebook.example",
                    new JavaScriptSerializer()
                ))
                {
                }
            }
            catch (MoveBookAuthException)
            {
                rejected = true;
            }
            Equal("remote HTTP rejected", true, rejected);
        }

        private static MoveBookAuthClient CreateClient(QueueHandler handler)
        {
            return new MoveBookAuthClient(
                "http://127.0.0.1:8010",
                new JavaScriptSerializer(),
                handler
            );
        }

        private static void RunAsync(Func<Task> test, string name)
        {
            try
            {
                test().GetAwaiter().GetResult();
            }
            catch (Exception exception)
            {
                failures++;
                Console.Error.WriteLine(name + ": " + exception.Message);
            }
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

        private static void Contains(string name, string actual, string expected)
        {
            if (actual != null && actual.Contains(expected))
            {
                return;
            }
            failures++;
            Console.Error.WriteLine(
                name + ": response does not contain expected value."
            );
        }

        private sealed class QueueHandler : HttpMessageHandler
        {
            private readonly Queue<HttpResponseMessage> responses =
                new Queue<HttpResponseMessage>();

            public QueueHandler()
            {
                Requests = new List<RecordedRequest>();
            }

            public List<RecordedRequest> Requests { get; private set; }

            public void Enqueue(HttpStatusCode statusCode, string json)
            {
                responses.Enqueue(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json"
                    )
                });
            }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken
            )
            {
                string body = request.Content == null
                    ? ""
                    : await request.Content.ReadAsStringAsync();
                Requests.Add(new RecordedRequest
                {
                    Method = request.Method.Method,
                    Path = request.RequestUri.AbsolutePath,
                    Body = body
                });
                return responses.Dequeue();
            }
        }

        private sealed class RecordedRequest
        {
            public string Method { get; set; }
            public string Path { get; set; }
            public string Body { get; set; }
        }
    }
}

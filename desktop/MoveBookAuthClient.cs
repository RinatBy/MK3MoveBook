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
    internal sealed class MoveBookAuthException : Exception
    {
        public MoveBookAuthException(string message)
            : this(message, null)
        {
        }

        public MoveBookAuthException(
            string message,
            HttpStatusCode? statusCode
        )
            : base(message)
        {
            StatusCode = statusCode;
        }

        public HttpStatusCode? StatusCode { get; private set; }
    }

    internal sealed class MoveBookDeviceSession
    {
        public string DeviceCode { get; set; }
        public string VerificationUri { get; set; }
        public int ExpiresIn { get; set; }
        public int Interval { get; set; }
    }

    internal sealed class MoveBookAuthUser
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string AvatarUrl { get; set; }
    }

    internal sealed class MoveBookAuthPollResult
    {
        public bool IsPending { get; set; }
        public int Interval { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public MoveBookAuthUser User { get; set; }
    }

    internal sealed class MoveBookAuthClient : IDisposable
    {
        private const string DefaultApiBaseUrl = "http://127.0.0.1:8010";

        private readonly HttpClient httpClient;
        private readonly JavaScriptSerializer serializer;
        private readonly string apiBaseUrl;

        public MoveBookAuthClient(
            string baseUrl,
            JavaScriptSerializer jsonSerializer
        )
            : this(baseUrl, jsonSerializer, null)
        {
        }

        internal MoveBookAuthClient(
            string baseUrl,
            JavaScriptSerializer jsonSerializer,
            HttpMessageHandler messageHandler
        )
        {
            serializer = jsonSerializer ?? new JavaScriptSerializer();
            apiBaseUrl = NormalizeBaseUrl(baseUrl);
            httpClient = messageHandler == null
                ? new HttpClient()
                : new HttpClient(messageHandler, true);
            httpClient.Timeout = TimeSpan.FromSeconds(15);
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
                "MK3MoveBook/0.3"
            );
        }

        public static string ResolveApiBaseUrl()
        {
            string configured = Environment.GetEnvironmentVariable(
                "MOVEBOOK_API_BASE_URL"
            );
            return string.IsNullOrWhiteSpace(configured)
                ? DefaultApiBaseUrl
                : configured.Trim();
        }

        public async Task<MoveBookDeviceSession> CreateDeviceSessionAsync(
            CancellationToken cancellationToken
        )
        {
            AuthHttpResult result = await SendForResultAsync(
                HttpMethod.Post,
                "/api/v1/auth/device-sessions",
                null,
                new[] { HttpStatusCode.Created },
                cancellationToken
            );
            Dictionary<string, object> payload = result.Payload;

            return new MoveBookDeviceSession
            {
                DeviceCode = RequireString(payload, "device_code"),
                VerificationUri = RequireString(payload, "verification_uri"),
                ExpiresIn = RequirePositiveInt(payload, "expires_in"),
                Interval = RequirePositiveInt(payload, "interval")
            };
        }

        public async Task<MoveBookAuthPollResult> PollDeviceSessionAsync(
            string deviceCode,
            CancellationToken cancellationToken
        )
        {
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["device_code"] = deviceCode;
            AuthHttpResult result = await SendForResultAsync(
                HttpMethod.Post,
                "/api/v1/auth/device-sessions/token",
                body,
                new[] { HttpStatusCode.OK, HttpStatusCode.Accepted },
                cancellationToken
            );
            Dictionary<string, object> payload = result.Payload;

            if (result.StatusCode == HttpStatusCode.Accepted)
            {
                return new MoveBookAuthPollResult
                {
                    IsPending = true,
                    Interval = RequirePositiveInt(payload, "interval")
                };
            }

            return ParseAuthorizedResult(payload);
        }

        public async Task<MoveBookAuthPollResult> RefreshAsync(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new MoveBookAuthException(
                    "Сохранённая сессия MoveBook повреждена."
                );
            }
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["refresh_token"] = refreshToken;
            AuthHttpResult result = await SendForResultAsync(
                HttpMethod.Post,
                "/api/v1/auth/refresh",
                body,
                new[] { HttpStatusCode.OK },
                cancellationToken
            );
            return ParseAuthorizedResult(result.Payload);
        }

        public async Task LogoutAsync(
            string refreshToken,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                return;
            }
            Dictionary<string, object> body = new Dictionary<string, object>();
            body["refresh_token"] = refreshToken;
            await SendForResultAsync(
                HttpMethod.Post,
                "/api/v1/auth/logout",
                body,
                new[] { HttpStatusCode.OK },
                cancellationToken
            );
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        private async Task<AuthHttpResult> SendForResultAsync(
            HttpMethod method,
            string path,
            Dictionary<string, object> body,
            HttpStatusCode[] acceptedStatuses,
            CancellationToken cancellationToken
        )
        {
            using (HttpRequestMessage request = new HttpRequestMessage(
                method,
                apiBaseUrl + path
            ))
            {
                if (body != null)
                {
                    request.Content = new StringContent(
                        serializer.Serialize(body),
                        Encoding.UTF8,
                        "application/json"
                    );
                }

                using (HttpResponseMessage response = await httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseContentRead,
                    cancellationToken
                ))
                {
                    string json = response.Content == null
                        ? ""
                        : await response.Content.ReadAsStringAsync();
                    bool accepted = Array.IndexOf(
                        acceptedStatuses,
                        response.StatusCode
                    ) >= 0;
                    if (!accepted)
                    {
                        throw CreateResponseException(response.StatusCode);
                    }
                    return new AuthHttpResult
                    {
                        StatusCode = response.StatusCode,
                        Payload = ParseObject(json)
                    };
                }
            }
        }

        private static string NormalizeBaseUrl(string value)
        {
            Uri uri;
            if (string.IsNullOrWhiteSpace(value) ||
                !Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri))
            {
                throw new MoveBookAuthException(
                    "Некорректный адрес сервера MoveBook."
                );
            }
            bool secure = string.Equals(
                uri.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase
            );
            if (!secure && !uri.IsLoopback)
            {
                throw new MoveBookAuthException(
                    "Удалённый сервер MoveBook должен использовать HTTPS."
                );
            }
            if (!secure && !string.Equals(
                uri.Scheme,
                Uri.UriSchemeHttp,
                StringComparison.OrdinalIgnoreCase
            ))
            {
                throw new MoveBookAuthException(
                    "Сервер MoveBook должен использовать HTTP или HTTPS."
                );
            }
            return value.Trim().TrimEnd('/');
        }

        private Dictionary<string, object> ParseObject(string json)
        {
            try
            {
                Dictionary<string, object> result =
                    serializer.Deserialize<Dictionary<string, object>>(json);
                if (result == null)
                {
                    throw new InvalidOperationException();
                }
                return result;
            }
            catch
            {
                throw new MoveBookAuthException(
                    "Сервер MoveBook вернул некорректный ответ."
                );
            }
        }

        private static MoveBookAuthException CreateResponseException(
            HttpStatusCode statusCode
        )
        {
            if (statusCode == HttpStatusCode.BadRequest)
            {
                return new MoveBookAuthException(
                    "Discord не подтвердил вход. Попробуйте ещё раз.",
                    statusCode
                );
            }
            if (statusCode == HttpStatusCode.Unauthorized)
            {
                return new MoveBookAuthException(
                    "Сохранённая сессия истекла. Войдите через Discord снова.",
                    statusCode
                );
            }
            if (statusCode == HttpStatusCode.Gone)
            {
                return new MoveBookAuthException(
                    "Время входа истекло. Попробуйте ещё раз.",
                    statusCode
                );
            }
            if (statusCode == HttpStatusCode.ServiceUnavailable)
            {
                return new MoveBookAuthException(
                    "Вход через Discord пока не настроен на сервере.",
                    statusCode
                );
            }
            return new MoveBookAuthException(
                "Сервер MoveBook отклонил запрос (" +
                (int)statusCode + ").",
                statusCode
            );
        }

        private static MoveBookAuthPollResult ParseAuthorizedResult(
            Dictionary<string, object> payload
        )
        {
            Dictionary<string, object> userPayload = RequireDictionary(
                payload,
                "user"
            );
            return new MoveBookAuthPollResult
            {
                IsPending = false,
                AccessToken = RequireString(payload, "access_token"),
                RefreshToken = RequireString(payload, "refresh_token"),
                User = new MoveBookAuthUser
                {
                    Id = RequirePositiveInt(userPayload, "id"),
                    DisplayName = RequireString(userPayload, "display_name"),
                    AvatarUrl = OptionalString(userPayload, "avatar_url")
                }
            };
        }

        private static Dictionary<string, object> RequireDictionary(
            Dictionary<string, object> payload,
            string key
        )
        {
            object value;
            Dictionary<string, object> result;
            if (!payload.TryGetValue(key, out value) ||
                (result = value as Dictionary<string, object>) == null)
            {
                throw new MoveBookAuthException(
                    "Сервер MoveBook вернул неполные данные входа."
                );
            }
            return result;
        }

        private static string RequireString(
            Dictionary<string, object> payload,
            string key
        )
        {
            string value = OptionalString(payload, key);
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new MoveBookAuthException(
                    "Сервер MoveBook вернул неполные данные входа."
                );
            }
            return value;
        }

        private static string OptionalString(
            Dictionary<string, object> payload,
            string key
        )
        {
            object value;
            return payload.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value)
                : "";
        }

        private static int RequirePositiveInt(
            Dictionary<string, object> payload,
            string key
        )
        {
            object value;
            int result;
            if (!payload.TryGetValue(key, out value) ||
                !int.TryParse(Convert.ToString(value), out result) ||
                result <= 0)
            {
                throw new MoveBookAuthException(
                    "Сервер MoveBook вернул неполные данные входа."
                );
            }
            return result;
        }

        private sealed class AuthHttpResult
        {
            public HttpStatusCode StatusCode { get; set; }
            public Dictionary<string, object> Payload { get; set; }
        }
    }
}

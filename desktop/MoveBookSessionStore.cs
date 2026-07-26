using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace MK3MoveBook
{
    internal sealed class MoveBookStoredSession
    {
        public string RefreshToken { get; set; }
    }

    internal sealed class MoveBookSessionStore
    {
        private const string SessionFileName = "auth-session-v1.bin";
        private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes(
            "MK3MoveBook.AuthSession.v1"
        );

        private readonly JavaScriptSerializer serializer;
        private readonly string apiBaseUrl;
        private readonly string sessionPath;
        private readonly string temporaryPath;

        public MoveBookSessionStore(
            string dataDirectory,
            string currentApiBaseUrl,
            JavaScriptSerializer jsonSerializer
        )
        {
            if (string.IsNullOrWhiteSpace(dataDirectory))
            {
                throw new ArgumentException("Data directory is required.");
            }
            if (string.IsNullOrWhiteSpace(currentApiBaseUrl))
            {
                throw new ArgumentException("API base URL is required.");
            }

            serializer = jsonSerializer ?? new JavaScriptSerializer();
            apiBaseUrl = currentApiBaseUrl.Trim().TrimEnd('/');
            sessionPath = Path.Combine(dataDirectory, SessionFileName);
            temporaryPath = sessionPath + ".tmp";
        }

        public MoveBookStoredSession Load()
        {
            if (!File.Exists(sessionPath))
            {
                return null;
            }

            try
            {
                byte[] protectedPayload = File.ReadAllBytes(sessionPath);
                byte[] payload = ProtectedData.Unprotect(
                    protectedPayload,
                    AdditionalEntropy,
                    DataProtectionScope.CurrentUser
                );
                Dictionary<string, object> values =
                    serializer.Deserialize<Dictionary<string, object>>(
                        Encoding.UTF8.GetString(payload)
                    );
                if (values == null || ReadInt(values, "version") != 1)
                {
                    return null;
                }

                string storedApiBaseUrl = ReadString(values, "api_base_url")
                    .Trim()
                    .TrimEnd('/');
                string refreshToken = ReadString(values, "refresh_token");
                if (!string.Equals(
                    storedApiBaseUrl,
                    apiBaseUrl,
                    StringComparison.OrdinalIgnoreCase
                ) || string.IsNullOrWhiteSpace(refreshToken))
                {
                    return null;
                }

                return new MoveBookStoredSession
                {
                    RefreshToken = refreshToken
                };
            }
            catch
            {
                Clear();
                return null;
            }
        }

        public void Save(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
            {
                throw new ArgumentException("Refresh token is required.");
            }

            Dictionary<string, object> values =
                new Dictionary<string, object>();
            values["version"] = 1;
            values["api_base_url"] = apiBaseUrl;
            values["refresh_token"] = refreshToken;

            byte[] payload = Encoding.UTF8.GetBytes(
                serializer.Serialize(values)
            );
            byte[] protectedPayload = ProtectedData.Protect(
                payload,
                AdditionalEntropy,
                DataProtectionScope.CurrentUser
            );

            string directory = Path.GetDirectoryName(sessionPath);
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllBytes(temporaryPath, protectedPayload);
                if (File.Exists(sessionPath))
                {
                    File.Replace(temporaryPath, sessionPath, null);
                }
                else
                {
                    File.Move(temporaryPath, sessionPath);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        public void Clear()
        {
            try
            {
                if (File.Exists(sessionPath))
                {
                    File.Delete(sessionPath);
                }
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
            catch
            {
                // A locked file will be retried on the next explicit logout.
            }
        }

        private static string ReadString(
            Dictionary<string, object> values,
            string key
        )
        {
            object value;
            return values.TryGetValue(key, out value) && value != null
                ? Convert.ToString(value)
                : "";
        }

        private static int ReadInt(
            Dictionary<string, object> values,
            string key
        )
        {
            object value;
            int result;
            return values.TryGetValue(key, out value) &&
                int.TryParse(Convert.ToString(value), out result)
                ? result
                : 0;
        }
    }
}

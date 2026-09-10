using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web.Script.Serialization;

namespace MK3MoveBook
{
    internal sealed class MoveBookStoredAchievementQueue
    {
        public int Version { get; set; }
        public string ApiBaseUrl { get; set; }
        public int UserId { get; set; }
        public List<MoveBookAchievementEvent> Events { get; set; }
    }

    internal sealed class MoveBookAchievementQueueLoadResult
    {
        public MoveBookAchievementQueueLoadResult(
            List<MoveBookAchievementEvent> events,
            bool hadUnreadableData
        )
        {
            Events = events ?? new List<MoveBookAchievementEvent>();
            HadUnreadableData = hadUnreadableData;
        }

        public List<MoveBookAchievementEvent> Events { get; private set; }
        public bool HadUnreadableData { get; private set; }
    }

    internal sealed class MoveBookAchievementQueueStore
    {
        internal const int MaximumEventCount = 4096;

        private const int MaximumFileSize = 8 * 1024 * 1024;
        private const string QueueDirectoryName = "AchievementOutbox";
        private static readonly byte[] AdditionalEntropy = Encoding.UTF8.GetBytes(
            "MK3MoveBook.AchievementOutbox.v1"
        );

        private readonly JavaScriptSerializer serializer;
        private readonly string apiBaseUrl;
        private readonly string queueDirectory;

        public MoveBookAchievementQueueStore(
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
            serializer.MaxJsonLength = MaximumFileSize;
            apiBaseUrl = NormalizeApiBaseUrl(currentApiBaseUrl);
            queueDirectory = Path.Combine(
                dataDirectory,
                QueueDirectoryName
            );
        }

        public MoveBookAchievementQueueLoadResult Load(int userId)
        {
            ValidateUserId(userId);
            string queuePath = GetQueuePath(userId);
            string temporaryPath = queuePath + ".tmp";
            RecoverTemporaryFile(queuePath, temporaryPath);

            string candidatePath = File.Exists(queuePath)
                ? queuePath
                : (File.Exists(temporaryPath) ? temporaryPath : "");
            if (string.IsNullOrEmpty(candidatePath))
            {
                return new MoveBookAchievementQueueLoadResult(
                    new List<MoveBookAchievementEvent>(),
                    false
                );
            }

            try
            {
                FileInfo file = new FileInfo(candidatePath);
                if (file.Length <= 0 || file.Length > MaximumFileSize)
                {
                    throw new InvalidDataException(
                        "Achievement outbox has an invalid size."
                    );
                }

                byte[] protectedPayload = File.ReadAllBytes(candidatePath);
                byte[] payload = ProtectedData.Unprotect(
                    protectedPayload,
                    AdditionalEntropy,
                    DataProtectionScope.CurrentUser
                );
                MoveBookStoredAchievementQueue stored =
                    serializer.Deserialize<MoveBookStoredAchievementQueue>(
                        Encoding.UTF8.GetString(payload)
                    );
                if (stored == null ||
                    stored.Version != 1 ||
                    stored.UserId != userId ||
                    !string.Equals(
                        NormalizeApiBaseUrl(stored.ApiBaseUrl),
                        apiBaseUrl,
                        StringComparison.OrdinalIgnoreCase
                    ) ||
                    stored.Events == null ||
                    stored.Events.Count > MaximumEventCount)
                {
                    throw new InvalidDataException(
                        "Achievement outbox belongs to another profile or version."
                    );
                }

                foreach (MoveBookAchievementEvent achievementEvent in stored.Events)
                {
                    ValidateAndNormalizeEvent(achievementEvent);
                }
                return new MoveBookAchievementQueueLoadResult(
                    stored.Events,
                    false
                );
            }
            catch
            {
                QuarantineUnreadableFile(candidatePath);
                return new MoveBookAchievementQueueLoadResult(
                    new List<MoveBookAchievementEvent>(),
                    true
                );
            }
        }

        public void Save(
            int userId,
            IEnumerable<MoveBookAchievementEvent> events
        )
        {
            ValidateUserId(userId);
            if (events == null)
            {
                throw new ArgumentNullException("events");
            }

            List<MoveBookAchievementEvent> storedEvents =
                new List<MoveBookAchievementEvent>();
            foreach (MoveBookAchievementEvent achievementEvent in events)
            {
                ValidateAndNormalizeEvent(achievementEvent);
                storedEvents.Add(achievementEvent);
                if (storedEvents.Count > MaximumEventCount)
                {
                    throw new InvalidOperationException(
                        "Achievement outbox is full."
                    );
                }
            }

            if (storedEvents.Count == 0)
            {
                Clear(userId);
                return;
            }

            MoveBookStoredAchievementQueue stored =
                new MoveBookStoredAchievementQueue
                {
                    Version = 1,
                    ApiBaseUrl = apiBaseUrl,
                    UserId = userId,
                    Events = storedEvents
                };
            byte[] payload = Encoding.UTF8.GetBytes(serializer.Serialize(stored));
            byte[] protectedPayload = ProtectedData.Protect(
                payload,
                AdditionalEntropy,
                DataProtectionScope.CurrentUser
            );
            if (protectedPayload.Length > MaximumFileSize)
            {
                throw new InvalidOperationException(
                    "Achievement outbox is too large."
                );
            }

            Directory.CreateDirectory(queueDirectory);
            string queuePath = GetQueuePath(userId);
            string temporaryPath = queuePath + ".tmp";
            try
            {
                using (FileStream output = new FileStream(
                    temporaryPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    4096,
                    FileOptions.WriteThrough
                ))
                {
                    output.Write(protectedPayload, 0, protectedPayload.Length);
                    output.Flush(true);
                }

                if (File.Exists(queuePath))
                {
                    File.Replace(temporaryPath, queuePath, null);
                }
                else
                {
                    File.Move(temporaryPath, queuePath);
                }
            }
            finally
            {
                TryDelete(temporaryPath);
            }
        }

        public void Clear(int userId)
        {
            ValidateUserId(userId);
            string queuePath = GetQueuePath(userId);
            TryDelete(queuePath);
            TryDelete(queuePath + ".tmp");
        }

        private string GetQueuePath(int userId)
        {
            string identity = apiBaseUrl.ToLowerInvariant() + "\n" +
                userId.ToString(CultureInfo.InvariantCulture);
            byte[] source = Encoding.UTF8.GetBytes(identity);
            byte[] digest;
            using (SHA256 hasher = SHA256.Create())
            {
                digest = hasher.ComputeHash(source);
            }

            StringBuilder name = new StringBuilder("outbox-v1-");
            foreach (byte value in digest)
            {
                name.Append(value.ToString("x2", CultureInfo.InvariantCulture));
            }
            name.Append(".bin");
            return Path.Combine(queueDirectory, name.ToString());
        }

        private static void RecoverTemporaryFile(
            string queuePath,
            string temporaryPath
        )
        {
            if (File.Exists(queuePath))
            {
                TryDelete(temporaryPath);
                return;
            }
            if (!File.Exists(temporaryPath))
            {
                return;
            }
            try
            {
                File.Move(temporaryPath, queuePath);
            }
            catch
            {
                // Load can still read the completed temporary file below.
            }
        }

        private static void QuarantineUnreadableFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return;
            }
            string quarantinePath = path + ".unreadable-" +
                DateTime.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) +
                "-" + Guid.NewGuid().ToString("N").Substring(0, 8);
            try
            {
                File.Move(path, quarantinePath);
            }
            catch
            {
                // Preserve the original file if quarantine is not possible.
            }
        }

        private static void ValidateAndNormalizeEvent(
            MoveBookAchievementEvent achievementEvent
        )
        {
            if (achievementEvent == null)
            {
                throw new InvalidDataException(
                    "Achievement outbox contains an empty event."
                );
            }

            achievementEvent.EventKey = (achievementEvent.EventKey ?? "").Trim();
            achievementEvent.RewardGroupKey =
                (achievementEvent.RewardGroupKey ?? "").Trim();
            achievementEvent.EventType = (achievementEvent.EventType ?? "").Trim();
            achievementEvent.RomId = (achievementEvent.RomId ?? "").Trim();
            achievementEvent.MatchScope =
                (achievementEvent.MatchScope ?? "").Trim();
            achievementEvent.CheatPolicyState = string.IsNullOrWhiteSpace(
                achievementEvent.CheatPolicyState
            )
                ? "unknown"
                : achievementEvent.CheatPolicyState.Trim();
            achievementEvent.WinnerSide = (achievementEvent.WinnerSide ?? "").Trim();
            achievementEvent.FinisherKind =
                (achievementEvent.FinisherKind ?? "").Trim();
            achievementEvent.FinisherWinnerSide =
                (achievementEvent.FinisherWinnerSide ?? "").Trim();
            achievementEvent.RoundWinningHit =
                (achievementEvent.RoundWinningHit ?? "").Trim();
            achievementEvent.SpecialKind =
                (achievementEvent.SpecialKind ?? "").Trim();
            achievementEvent.SpecialActorSide =
                (achievementEvent.SpecialActorSide ?? "").Trim();

            if (achievementEvent.EventKey.Length < 8 ||
                achievementEvent.EventKey.Length > 160 ||
                achievementEvent.RewardGroupKey.Length > 160 ||
                !IsEventType(achievementEvent.EventType) ||
                achievementEvent.RomId.Length < 1 ||
                achievementEvent.RomId.Length > 64 ||
                !IsMatchScope(achievementEvent.MatchScope) ||
                !IsCheatPolicyState(achievementEvent.CheatPolicyState) ||
                achievementEvent.NetplayTotalPlayers < 0 ||
                achievementEvent.NetplayTotalPlayers > 8 ||
                achievementEvent.TeamSize < 0 ||
                achievementEvent.TeamSize > 3 ||
                achievementEvent.TowerId < -1 ||
                achievementEvent.TowerId > 3 ||
                (achievementEvent.TowerCompleted &&
                    (!string.Equals(
                        achievementEvent.EventType,
                        "match-ended",
                        StringComparison.Ordinal
                    ) ||
                     !string.Equals(
                         achievementEvent.MatchScope,
                         "computer",
                         StringComparison.Ordinal
                     ) ||
                     achievementEvent.TowerId < 0)) ||
                !IsPlayerSide(achievementEvent.WinnerSide) ||
                !IsPlayerSide(achievementEvent.FinisherWinnerSide) ||
                !IsPlayerSide(achievementEvent.SpecialActorSide) ||
                achievementEvent.FinisherKind.Length > 32 ||
                achievementEvent.SpecialKind.Length > 64 ||
                achievementEvent.RoundWinningHit.Length > 64 ||
                !IsFighterId(achievementEvent.PlayerOneId) ||
                !IsFighterId(achievementEvent.PlayerTwoId) ||
                !IsFighterId(achievementEvent.FinisherFighterId) ||
                !IsFighterId(achievementEvent.FinisherOpponentId) ||
                achievementEvent.PlayerOneHealth < 0 ||
                achievementEvent.PlayerOneHealth > 1000 ||
                achievementEvent.PlayerTwoHealth < 0 ||
                achievementEvent.PlayerTwoHealth > 1000 ||
                achievementEvent.RoundNumber < 0 ||
                achievementEvent.RoundNumber > 10 ||
                achievementEvent.PlayerOneRounds < 0 ||
                achievementEvent.PlayerOneRounds > 10 ||
                achievementEvent.PlayerTwoRounds < 0 ||
                achievementEvent.PlayerTwoRounds > 10 ||
                achievementEvent.OccurredAtUtc == default(DateTime))
            {
                throw new InvalidDataException(
                    "Achievement outbox contains an invalid event."
                );
            }

            achievementEvent.OccurredAtUtc =
                achievementEvent.OccurredAtUtc.Kind == DateTimeKind.Utc
                    ? achievementEvent.OccurredAtUtc
                    : achievementEvent.OccurredAtUtc.ToUniversalTime();
        }

        private static bool IsEventType(string value)
        {
            return string.Equals(value, "round-ended", StringComparison.Ordinal) ||
                string.Equals(value, "match-ended", StringComparison.Ordinal) ||
                string.Equals(value, "finisher", StringComparison.Ordinal) ||
                string.Equals(value, "special", StringComparison.Ordinal);
        }

        private static bool IsPlayerSide(string value)
        {
            return string.IsNullOrEmpty(value) ||
                string.Equals(value, "p1", StringComparison.Ordinal) ||
                string.Equals(value, "p2", StringComparison.Ordinal);
        }

        private static bool IsMatchScope(string value)
        {
            return string.IsNullOrEmpty(value) ||
                string.Equals(value, "computer", StringComparison.Ordinal) ||
                string.Equals(value, "online", StringComparison.Ordinal);
        }

        private static bool IsCheatPolicyState(string value)
        {
            return string.Equals(value, "unknown", StringComparison.Ordinal) ||
                string.Equals(value, "disabled", StringComparison.Ordinal) ||
                string.Equals(value, "enabled", StringComparison.Ordinal);
        }

        private static bool IsFighterId(int value)
        {
            return value >= -1 && value <= 255;
        }

        private static string NormalizeApiBaseUrl(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? ""
                : value.Trim().TrimEnd('/');
        }

        private static void ValidateUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new ArgumentOutOfRangeException("userId");
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // A stale file is safer than losing the committed queue.
            }
        }
    }
}

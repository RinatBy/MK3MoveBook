using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace MK3MoveBook
{
    // Opt-in, narrow event-path trace. Never serialize auth objects, HTTP
    // headers, response bodies, or exception messages into a diagnostic file.
    internal sealed class MoveBookAchievementDiagnosticReport
    {
        private readonly JavaScriptSerializer serializer = new JavaScriptSerializer();
        private string lastObservation = "";
        public string ReportPath { get; private set; }

        public MoveBookAchievementDiagnosticReport(
            string applicationDirectory, bool enabled, string apiBaseUrl)
        {
            ReportPath = "";
            if (!enabled) return;
            try
            {
                string directory = Path.Combine(applicationDirectory, "diagnostics");
                Directory.CreateDirectory(directory);
                ReportPath = Path.Combine(directory, "achievement-path-" +
                    DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) +
                    "-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".jsonl");
                File.WriteAllText(ReportPath, "", new UTF8Encoding(false));
                Uri endpoint;
                Write(new Dictionary<string, object>
                {
                    { "stage", "started" },
                    { "api_origin", Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out endpoint)
                        ? endpoint.Scheme + "://" + endpoint.Host + ":" + endpoint.Port
                        : "unknown" }
                });
            }
            catch { ReportPath = ""; }
        }

        public void Observe(MameBridgeSnapshot snapshot, bool captureEnabled,
            bool signedIn, int pending, string contentVersion)
        {
            if (snapshot == null || string.IsNullOrEmpty(ReportPath)) return;
            try
            {
                string reason = !signedIn ? "signed-out" : !captureEnabled
                    ? "capture-disabled" : MameAchievementEligibilityPolicy.SnapshotExclusionReason(
                        snapshot.RomId, snapshot.FightMatchFullyObserved,
                        snapshot.MatchMode, snapshot.CpuAchievementPolicy,
                        snapshot.NetplayLayoutSupported, snapshot.NetplayActive,
                        snapshot.NetplayPlayerNumber, snapshot.NetplayTotalPlayers,
                        snapshot.TeamSize);
                Dictionary<string, object> row = new Dictionary<string, object>
                {
                    { "stage", "observed" }, { "content_version", contentVersion },
                    { "rom_id", snapshot.RomId }, { "process_id", snapshot.ProcessId },
                    { "tracking_session", snapshot.TrackingSessionSequence },
                    { "match_sequence", snapshot.MatchSequence },
                    { "event_sequence", snapshot.EventSequence }, { "event_kind", snapshot.EventKind },
                    { "finisher_sequence", snapshot.FinisherSequence },
                    { "finisher_kind", snapshot.FinisherKind },
                    { "special_sequence", snapshot.SpecialSequence },
                    { "special_kind", snapshot.SpecialKind },
                    { "bridge_state", snapshot.State }, { "fight_state", snapshot.FightState },
                    { "match_mode", snapshot.MatchMode }, { "cheat_policy", snapshot.CpuAchievementPolicy },
                    { "fully_observed", snapshot.FightMatchFullyObserved },
                    { "team_size", snapshot.TeamSize }, { "netplay_active", snapshot.NetplayActive },
                    { "netplay_players", snapshot.NetplayTotalPlayers },
                    { "local_player", snapshot.NetplayPlayerNumber },
                    { "reason", reason.Length == 0 ? "eligible" : reason }
                };
                string signature = serializer.Serialize(row);
                if (signature == lastObservation) return;
                lastObservation = signature;
                row["pending"] = pending;
                Write(row);
            }
            catch { /* Diagnostic I/O must not affect achievement delivery. */ }
        }

        public void Consumed(string key, string rom, string kind, string reason, int pending)
        {
            if (string.IsNullOrEmpty(ReportPath)) return;
            Write(new Dictionary<string, object>
            {
                { "stage", "consumed-without-queue" }, { "event_key", key },
                { "rom_id", rom }, { "event_type", kind }, { "reason", reason },
                { "pending", pending }
            });
        }

        public void Record(string stage, MoveBookAchievementEvent value,
            int pending, string detail, MoveBookAchievementSubmitResult result = null)
        {
            if (string.IsNullOrEmpty(ReportPath)) return;
            try
            {
                Dictionary<string, object> row = new Dictionary<string, object>
                {
                    { "stage", stage }, { "pending", pending }, { "detail", detail },
                    { "event_key", value == null ? "" : value.EventKey },
                    { "rom_id", value == null ? "" : value.RomId },
                    { "event_type", value == null ? "" : value.EventType }
                };
                if (value != null)
                {
                    row["scope"] = value.MatchScope;
                    row["fully_observed"] = value.MatchFullyObserved;
                    row["cheat_policy"] = value.CheatPolicyState;
                    row["netplay_players"] = value.NetplayTotalPlayers;
                    row["team_size"] = value.TeamSize;
                    row["winner"] = value.WinnerSide;
                    row["p1_id"] = value.PlayerOneId;
                    row["p2_id"] = value.PlayerTwoId;
                    row["finisher_kind"] = value.FinisherKind;
                }
                if (result != null)
                {
                    row["accepted"] = result.Accepted;
                    row["new_unlocks"] = result.NewUnlocks;
                    if (result.Progress != null)
                    {
                        row["victories"] = result.Progress.Victories;
                        row["matches"] = result.Progress.MatchesPlayed;
                        row["computer_victories"] = result.Progress.ComputerVictories;
                        row["online_victories"] = result.Progress.OnlineVictories;
                        row["finishers"] = result.Progress.Finishers;
                        row["unlock_count"] = result.Progress.Unlocked == null
                            ? 0 : result.Progress.Unlocked.Count;
                    }
                }
                Write(row);
            }
            catch { /* No exception or server response text is logged. */ }
        }

        private void Write(Dictionary<string, object> row)
        {
            try
            {
                row["schema"] = 1;
                row["utc"] = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                File.AppendAllText(ReportPath, serializer.Serialize(row) +
                    Environment.NewLine, new UTF8Encoding(false));
            }
            catch { /* Diagnostic I/O is best effort. */ }
        }
    }
}

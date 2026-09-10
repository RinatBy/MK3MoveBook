using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;

namespace MK3MoveBook
{
    internal sealed class MameDiagnosticReport
    {
        private static readonly string[] Columns =
        {
            "timestamp_local",
            "session_id",
            "sample_no",
            "player_data_available",
            "bridge_state",
            "main_ram_address",
            "rom_id",
            "rom_label",
            "process_id",
            "process_path",
            "process_version",
            "window_title",
            "p1_id_dec",
            "p1_id_hex",
            "p1_name",
            "p1_health_raw",
            "p2_id_dec",
            "p2_id_hex",
            "p2_name",
            "p2_health_raw",
            "game_p1_rounds",
            "game_p2_rounds",
            "fight_participants_confirmed",
            "fight_p1_id_dec",
            "fight_p1_id_hex",
            "fight_p1_name",
            "fight_p2_id_dec",
            "fight_p2_id_hex",
            "fight_p2_name",
            "fight_state",
            "match_mode",
            "match_mode_label",
            "cheat_monitor_state",
            "cheat_catalog_count",
            "cheat_use_sequence",
            "active_cheat_count",
            "active_cheats",
            "last_detected_cheat",
            "cpu_achievement_policy",
            "cpu_achievement_policy_details",
            "cpu_cheat_detected",
            "tower_context",
            "tower_player_side",
            "tower_id",
            "tower_name",
            "tower_fight_number",
            "powerbars_enabled",
            "finish_phase",
            "tower_state_a",
            "tower_state_b",
            "p2_control_state",
            "p1_participant_state",
            "p2_participant_state",
            "team_size_code",
            "team_size",
            "team_active_player_code",
            "team_left_player_count",
            "team_local_player_active",
            "team_local_player_side",
            "netplay_layout_supported",
            "netplay_active",
            "netplay_player_number",
            "netplay_total_players",
            "netplay_controller_slot",
            "netplay_controller_order",
            "round_number",
            "p1_rounds",
            "p2_rounds",
            "match_sequence",
            "match_fully_observed",
            "p1_danger_seen",
            "p2_danger_seen",
            "round_winning_hit",
            "winner_side",
            "winner",
            "event_sequence",
            "event_team_active_player_code",
            "event_team_left_player_count",
            "event_team_controller_slot",
            "event_kind",
            "event_title",
            "event_details",
            "finisher_sequence",
            "finisher_kind",
            "finisher_winner_side",
            "finisher_fighter_id_dec",
            "finisher_opponent_id_dec",
            "finisher_team_active_player_code",
            "finisher_team_left_player_count",
            "finisher_team_controller_slot",
            "finisher_title",
            "finisher_details",
            "special_sequence",
            "special_kind",
            "special_actor_side",
            "special_team_active_player_code",
            "special_team_left_player_count",
            "special_team_controller_slot",
            "special_title",
            "special_details",
            "bridge_title",
            "bridge_details"
        };

        private readonly object syncRoot = new object();
        private readonly string sessionId;
        private int sampleNumber;

        public MameDiagnosticReport(
            string applicationDirectory,
            bool diagnosticsEnabled
        )
        {
            string startedAt = DateTime.Now.ToString(
                "yyyyMMdd-HHmmss",
                CultureInfo.InvariantCulture
            );
            sessionId = startedAt + "-P" +
                Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
            ReportPath = "";
            if (!diagnosticsEnabled)
            {
                return;
            }
            try
            {
                string directory = Path.Combine(
                    applicationDirectory,
                    "diagnostics"
                );
                Directory.CreateDirectory(directory);
                ReportPath = Path.Combine(
                    directory,
                    "mame-fight-report-" + sessionId + ".csv"
                );
                File.WriteAllText(
                    ReportPath,
                    string.Join(";", Columns) + Environment.NewLine,
                    new UTF8Encoding(true)
                );
            }
            catch
            {
                ReportPath = "";
            }
        }

        public string ReportPath { get; private set; }

        public void Append(MameBridgeSnapshot snapshot)
        {
            if (snapshot == null || string.IsNullOrEmpty(ReportPath))
            {
                return;
            }

            lock (syncRoot)
            {
                sampleNumber++;
                string[] values =
                {
                    DateTimeOffset.Now.ToString(
                        "yyyy-MM-dd HH:mm:ss.fff zzz",
                        CultureInfo.InvariantCulture
                    ),
                    sessionId,
                    Number(sampleNumber),
                    snapshot.PlayerDataAvailable ? "1" : "0",
                    snapshot.State,
                    snapshot.MainRamAddress,
                    snapshot.RomId,
                    snapshot.RomLabel,
                    Number(snapshot.ProcessId),
                    snapshot.ProcessPath,
                    snapshot.ProcessVersion,
                    snapshot.WindowTitle,
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerOneId)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Hex(snapshot.PlayerOneId)
                        : "",
                    snapshot.PlayerDataAvailable ? snapshot.PlayerOne : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerOneHealth)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerTwoId)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Hex(snapshot.PlayerTwoId)
                        : "",
                    snapshot.PlayerDataAvailable ? snapshot.PlayerTwo : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerTwoHealth)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.GamePlayerOneRounds)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.GamePlayerTwoRounds)
                        : "",
                    snapshot.FightParticipantsConfirmed ? "1" : "0",
                    snapshot.FightParticipantsConfirmed
                        ? Number(snapshot.FightPlayerOneId)
                        : "",
                    snapshot.FightParticipantsConfirmed
                        ? Hex(snapshot.FightPlayerOneId)
                        : "",
                    snapshot.FightParticipantsConfirmed
                        ? snapshot.FightPlayerOne
                        : "",
                    snapshot.FightParticipantsConfirmed
                        ? Number(snapshot.FightPlayerTwoId)
                        : "",
                    snapshot.FightParticipantsConfirmed
                        ? Hex(snapshot.FightPlayerTwoId)
                        : "",
                    snapshot.FightParticipantsConfirmed
                        ? snapshot.FightPlayerTwo
                        : "",
                    snapshot.FightState,
                    snapshot.MatchMode,
                    snapshot.MatchModeLabel,
                    snapshot.CheatMonitorState,
                    Number(snapshot.CheatCatalogCount),
                    Number(snapshot.CheatUseSequence),
                    Number(snapshot.ActiveCheatCount),
                    snapshot.ActiveCheats,
                    snapshot.LastDetectedCheat,
                    snapshot.CpuAchievementPolicy,
                    snapshot.CpuAchievementPolicyDetails,
                    snapshot.CpuCheatDetected ? "1" : "0",
                    snapshot.TowerContext ? "1" : "0",
                    snapshot.TowerPlayerSide,
                    snapshot.TowerContext ? Number(snapshot.TowerId) : "",
                    snapshot.TowerName,
                    snapshot.TowerFightNumber > 0
                        ? Number(snapshot.TowerFightNumber)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PowerBarsEnabled)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.FinishPhase)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.TowerStateA)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.TowerStateB)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerTwoControlState)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerOneParticipantState)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.PlayerTwoParticipantState)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.TeamSizeCode)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.TeamSize)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.TeamActivePlayerCode)
                        : "",
                    snapshot.PlayerDataAvailable
                        ? Number(snapshot.TeamLeftPlayerCount)
                        : "",
                    snapshot.TeamLocalPlayerActive ? "1" : "0",
                    snapshot.TeamLocalPlayerSide,
                    snapshot.NetplayLayoutSupported ? "1" : "0",
                    snapshot.NetplayActive ? "1" : "0",
                    snapshot.NetplayActive
                        ? Number(snapshot.NetplayPlayerNumber)
                        : "",
                    snapshot.NetplayActive
                        ? Number(snapshot.NetplayTotalPlayers)
                        : "",
                    snapshot.NetplayActive
                        ? Number(snapshot.NetplayControllerSlot)
                        : "",
                    snapshot.NetplayControllerOrder,
                    Number(snapshot.RoundNumber),
                    Number(snapshot.PlayerOneRounds),
                    Number(snapshot.PlayerTwoRounds),
                    Number(snapshot.MatchSequence),
                    snapshot.FightMatchFullyObserved ? "1" : "0",
                    snapshot.PlayerOneDangerSeen ? "1" : "0",
                    snapshot.PlayerTwoDangerSeen ? "1" : "0",
                    snapshot.RoundWinningHit,
                    snapshot.WinnerSide,
                    snapshot.Winner,
                    Number(snapshot.EventSequence),
                    Number(snapshot.EventTeamActivePlayerCode),
                    Number(snapshot.EventTeamLeftPlayerCount),
                    Number(snapshot.EventTeamControllerSlot),
                    snapshot.EventKind,
                    snapshot.EventTitle,
                    snapshot.EventDetails,
                    Number(snapshot.FinisherSequence),
                    snapshot.FinisherKind,
                    snapshot.FinisherWinnerSide,
                    Number(snapshot.FinisherFighterId),
                    Number(snapshot.FinisherOpponentId),
                    Number(snapshot.FinisherTeamActivePlayerCode),
                    Number(snapshot.FinisherTeamLeftPlayerCount),
                    Number(snapshot.FinisherTeamControllerSlot),
                    snapshot.FinisherTitle,
                    snapshot.FinisherDetails,
                    Number(snapshot.SpecialSequence),
                    snapshot.SpecialKind,
                    snapshot.SpecialActorSide,
                    Number(snapshot.SpecialTeamActivePlayerCode),
                    Number(snapshot.SpecialTeamLeftPlayerCount),
                    Number(snapshot.SpecialTeamControllerSlot),
                    snapshot.SpecialTitle,
                    snapshot.SpecialDetails,
                    snapshot.Title,
                    snapshot.Details
                };

                StringBuilder line = new StringBuilder();
                for (int index = 0; index < values.Length; index++)
                {
                    if (index > 0)
                    {
                        line.Append(';');
                    }
                    line.Append(Escape(values[index]));
                }
                line.AppendLine();
                try
                {
                    File.AppendAllText(
                        ReportPath,
                        line.ToString(),
                        new UTF8Encoding(false)
                    );
                }
                catch
                {
                    // Reporting is diagnostic and must not stop the bridge.
                }
            }
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Hex(int value)
        {
            return "0x" + value.ToString("X2", CultureInfo.InvariantCulture);
        }

        private static string Escape(string value)
        {
            string cell = value ?? "";
            if (cell.IndexOfAny(new[] { ';', '"', '\r', '\n' }) < 0)
            {
                return cell;
            }
            return "\"" + cell.Replace("\"", "\"\"") + "\"";
        }
    }
}

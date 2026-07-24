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
            "fight_participants_confirmed",
            "fight_p1_id_dec",
            "fight_p1_id_hex",
            "fight_p1_name",
            "fight_p2_id_dec",
            "fight_p2_id_hex",
            "fight_p2_name",
            "fight_state",
            "round_number",
            "p1_rounds",
            "p2_rounds",
            "winner_side",
            "winner",
            "event_sequence",
            "event_kind",
            "event_title",
            "event_details",
            "bridge_title",
            "bridge_details"
        };

        private readonly object syncRoot = new object();
        private readonly string sessionId;
        private int sampleNumber;

        public MameDiagnosticReport(string applicationDirectory)
        {
            string startedAt = DateTime.Now.ToString(
                "yyyyMMdd-HHmmss",
                CultureInfo.InvariantCulture
            );
            sessionId = startedAt + "-P" +
                Process.GetCurrentProcess().Id.ToString(CultureInfo.InvariantCulture);
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
                    Number(snapshot.RoundNumber),
                    Number(snapshot.PlayerOneRounds),
                    Number(snapshot.PlayerTwoRounds),
                    snapshot.WinnerSide,
                    snapshot.Winner,
                    Number(snapshot.EventSequence),
                    snapshot.EventKind,
                    snapshot.EventTitle,
                    snapshot.EventDetails,
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

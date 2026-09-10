using System;

namespace MK3MoveBook
{
    internal sealed class MameCpuCheatPolicyResult
    {
        public string State { get; set; }
        public string Details { get; set; }
        public bool CheatDetected { get; set; }
        public string DetectedCheat { get; set; }
    }

    internal sealed class MameCpuCheatPolicy
    {
        private int trackingSessionSequence;
        private int catalogGeneration;
        private int lastUseSequence;
        private bool cheatDetected;
        private string detectedCheat;

        public MameCpuCheatPolicy()
        {
            Reset();
        }

        public void Reset()
        {
            trackingSessionSequence = 0;
            catalogGeneration = 0;
            lastUseSequence = 0;
            cheatDetected = false;
            detectedCheat = "";
        }

        public MameCpuCheatPolicyResult Resolve(
            int currentTrackingSessionSequence,
            string matchMode,
            MameCheatMonitorSnapshot monitor,
            string romId = null
        )
        {
            monitor = monitor ?? new MameCheatMonitorSnapshot
            {
                State = "stopped"
            };
            bool firstTrackingSession = trackingSessionSequence == 0;
            if (currentTrackingSessionSequence != trackingSessionSequence)
            {
                trackingSessionSequence = currentTrackingSessionSequence;
                cheatDetected = false;
                detectedCheat = "";
                lastUseSequence = firstTrackingSession
                    ? 0
                    : monitor.UseSequence;
            }
            if (monitor.CatalogGeneration != catalogGeneration)
            {
                catalogGeneration = monitor.CatalogGeneration;
                lastUseSequence = 0;
            }

            bool towerCpu = string.Equals(
                matchMode,
                "tower-cpu",
                StringComparison.Ordinal
            );
            bool romMatches = string.IsNullOrEmpty(monitor.RomId) ||
                string.Equals(monitor.RomId, romId, StringComparison.Ordinal);
            bool emptyVerified = monitor.State == "ready-empty" &&
                !string.IsNullOrEmpty(monitor.RomId) && romMatches &&
                monitor.ActionableCheatCount == 0 && monitor.ActiveCheatCount == 0;
            if (romMatches && (monitor.State == "ready" || monitor.State == "ready-empty"))
            {
                bool newUse = monitor.UseSequence != lastUseSequence;
                if (towerCpu && (newUse || monitor.ActiveCheatCount > 0))
                {
                    cheatDetected = true;
                    if (!string.IsNullOrWhiteSpace(monitor.LastDetectedCheat))
                    {
                        detectedCheat = monitor.LastDetectedCheat;
                    }
                    else if (!string.IsNullOrWhiteSpace(monitor.ActiveCheats))
                    {
                        detectedCheat = monitor.ActiveCheats;
                    }
                }
                lastUseSequence = monitor.UseSequence;
            }

            if (!towerCpu)
            {
                return Result("not-applicable", "Online и локальные PvP-матчи не проверяются.");
            }
            if (cheatDetected)
            {
                string suffix = string.IsNullOrWhiteSpace(detectedCheat)
                    ? ""
                    : ": " + detectedCheat;
                return Result(
                    "blocked-cheat",
                    "CPU-достижения отключены до перезапуска ROM — применён чит" + suffix + "."
                );
            }
            if (!romMatches || (!emptyVerified &&
                (monitor.State != "ready" || monitor.ActionableCheatCount != 23)))
            {
                return Result(
                    "checking",
                    "CPU-достижения ожидают проверки списка читов MAME."
                );
            }
            return Result(
                "verified-clean",
                emptyVerified
                    ? "Движок MAME проверен: список читов текущего ROM пуст."
                    : "Список читов MAME проверен: активные читы не обнаружены."
            );
        }

        private MameCpuCheatPolicyResult Result(string state, string details)
        {
            return new MameCpuCheatPolicyResult
            {
                State = state,
                Details = details,
                CheatDetected = cheatDetected,
                DetectedCheat = detectedCheat
            };
        }
    }
}

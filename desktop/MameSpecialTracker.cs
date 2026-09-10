using System;
using System.Collections.Generic;

namespace MK3MoveBook
{
    internal sealed class MameSpecialProgress
    {
        public int Sequence { get; set; }
        public string Kind { get; set; }
        public string ActorSide { get; set; }
        public int TeamActivePlayerCode { get; set; }
        public int TeamLeftPlayerCount { get; set; }
        public int TeamControllerSlot { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
    }

    internal sealed class MameSpecialTracker
    {
        private const int SheevaCharacterId = 0x0B;
        private const int FrozenAction = 0x0610;
        private const int ComboAction = 0x0116;
        private const int UppercutAction = 0x010E;
        private const int FrostyMaximumHealth = 8;
        private const int SheevaComboDamage = 70;
        private const int SheevaComboHits = 7;

        private readonly Queue<SpecialEvent> pendingEvents =
            new Queue<SpecialEvent>();
        private int sequence;
        private int observedMatchSequence;
        private bool frostyActive;
        private bool toastyActive;
        private bool playerOneComboActive;
        private bool playerTwoComboActive;
        private int previousZone;
        private int previousPlayerOneHitBy;
        private int previousPlayerTwoHitBy;
        private string recentActorSide;
        private string kind;
        private string actorSide;
        private int teamActivePlayerCode;
        private int teamLeftPlayerCount;
        private int teamControllerSlot;
        private int observedTeamActivePlayerCode;
        private int observedTeamLeftPlayerCount;
        private int observedTeamControllerSlot;
        private string title;
        private string details;

        public MameSpecialTracker()
        {
            Reset();
        }

        public void Reset()
        {
            pendingEvents.Clear();
            sequence = 0;
            observedMatchSequence = 0;
            frostyActive = false;
            toastyActive = false;
            playerOneComboActive = false;
            playerTwoComboActive = false;
            previousZone = -1;
            previousPlayerOneHitBy = -1;
            previousPlayerTwoHitBy = -1;
            recentActorSide = "";
            kind = "";
            actorSide = "";
            teamActivePlayerCode = 0;
            teamLeftPlayerCount = 0;
            teamControllerSlot = -1;
            observedTeamActivePlayerCode = 0;
            observedTeamLeftPlayerCount = 0;
            observedTeamControllerSlot = -1;
            title = "";
            details = "";
        }

        public MameSpecialProgress Observe(
            int matchSequence,
            bool participantsConfirmed,
            string fightState,
            int playerOneId,
            int playerOneHealth,
            int playerOneAction,
            int playerOneHitBy,
            int playerOneComboDamage,
            int playerOneComboHits,
            int playerTwoId,
            int playerTwoHealth,
            int playerTwoAction,
            int playerTwoHitBy,
            int playerTwoComboDamage,
            int playerTwoComboHits,
            int kombatZone,
            int finishPhase,
            bool currentFrostyActive,
            bool currentToastyActive,
            int currentTeamActivePlayerCode = 0,
            int currentTeamLeftPlayerCount = 0,
            int currentTeamControllerSlot = -1
        )
        {
            observedTeamActivePlayerCode = currentTeamActivePlayerCode;
            observedTeamLeftPlayerCount = currentTeamLeftPlayerCount;
            observedTeamControllerSlot = currentTeamControllerSlot;
            if (matchSequence != observedMatchSequence)
            {
                observedMatchSequence = matchSequence;
                frostyActive = currentFrostyActive;
                toastyActive = currentToastyActive;
                playerOneComboActive = false;
                playerTwoComboActive = false;
                previousZone = kombatZone;
                previousPlayerOneHitBy = playerOneHitBy;
                previousPlayerTwoHitBy = playerTwoHitBy;
                recentActorSide = "";
            }

            UpdateRecentActor(
                playerOneHitBy,
                playerTwoHitBy,
                playerOneComboDamage,
                playerTwoComboDamage
            );

            bool playerOneCombo = playerOneId == SheevaCharacterId &&
                playerTwoComboDamage == SheevaComboDamage &&
                playerTwoComboHits == SheevaComboHits;
            bool playerTwoCombo = playerTwoId == SheevaCharacterId &&
                playerOneComboDamage == SheevaComboDamage &&
                playerOneComboHits == SheevaComboHits;
            if (participantsConfirmed && !playerOneCombo && playerOneComboActive)
            {
                Enqueue(
                    "sheeva-42-combo",
                    "p1",
                    "Комбинация Sheeva: 7 ударов, 42%",
                    "Зафиксировано 70 единиц урона из 166."
                );
            }
            if (participantsConfirmed && !playerTwoCombo && playerTwoComboActive)
            {
                Enqueue(
                    "sheeva-42-combo",
                    "p2",
                    "Комбинация Sheeva: 7 ударов, 42%",
                    "Зафиксировано 70 единиц урона из 166."
                );
            }

            if (participantsConfirmed && currentFrostyActive && !frostyActive)
            {
                string detectedActor = FrostyActor(
                    playerOneHealth,
                    playerOneAction,
                    playerTwoHealth,
                    playerTwoAction
                );
                Enqueue(
                    "frosty",
                    detectedActor,
                    "Зафиксировано Frosty!",
                    "Заморозка сработала при критическом здоровье соперника."
                );
            }

            if (participantsConfirmed && currentToastyActive && !toastyActive)
            {
                string detectedActor = ToastyActor(
                    playerOneAction,
                    playerOneComboDamage,
                    playerOneComboHits,
                    playerTwoAction,
                    playerTwoComboDamage,
                    playerTwoComboHits
                );
                Enqueue(
                    "toasty",
                    detectedActor,
                    "Зафиксировано Toasty!",
                    "В памяти появился процесс Дэна Фордена."
                );
            }

            bool roundActive = string.Equals(
                fightState,
                "round-active",
                StringComparison.Ordinal
            );
            if (participantsConfirmed && roundActive && previousZone >= 0 &&
                kombatZone >= 0 && kombatZone != previousZone)
            {
                string detectedActor = UppercutActor(
                    playerOneHitBy,
                    playerTwoHitBy
                );
                if (!string.IsNullOrEmpty(detectedActor))
                {
                    Enqueue(
                        "stage-transition-uppercut",
                        detectedActor,
                        "Апперкот сменил арену",
                        "Переход зоны 0x" + previousZone.ToString("X2") +
                            " → 0x" + kombatZone.ToString("X2") + "."
                    );
                }
            }

            frostyActive = currentFrostyActive;
            toastyActive = currentToastyActive;
            playerOneComboActive = playerOneCombo;
            playerTwoComboActive = playerTwoCombo;
            previousZone = kombatZone;
            previousPlayerOneHitBy = playerOneHitBy;
            previousPlayerTwoHitBy = playerTwoHitBy;

            if (pendingEvents.Count > 0)
            {
                SpecialEvent next = pendingEvents.Dequeue();
                sequence++;
                kind = next.Kind;
                actorSide = next.ActorSide;
                teamActivePlayerCode = next.TeamActivePlayerCode;
                teamLeftPlayerCount = next.TeamLeftPlayerCount;
                teamControllerSlot = next.TeamControllerSlot;
                title = next.Title;
                details = next.Details;
            }
            return Snapshot();
        }

        private void UpdateRecentActor(
            int playerOneHitBy,
            int playerTwoHitBy,
            int playerOneComboDamage,
            int playerTwoComboDamage
        )
        {
            if ((playerTwoHitBy > 0 &&
                    playerTwoHitBy != previousPlayerTwoHitBy) ||
                playerTwoComboDamage > 0)
            {
                recentActorSide = "p1";
            }
            if ((playerOneHitBy > 0 &&
                    playerOneHitBy != previousPlayerOneHitBy) ||
                playerOneComboDamage > 0)
            {
                recentActorSide = "p2";
            }
        }

        private string FrostyActor(
            int playerOneHealth,
            int playerOneAction,
            int playerTwoHealth,
            int playerTwoAction
        )
        {
            if (playerOneAction == FrozenAction &&
                playerOneHealth > 0 &&
                playerOneHealth <= FrostyMaximumHealth)
            {
                return "p2";
            }
            if (playerTwoAction == FrozenAction &&
                playerTwoHealth > 0 &&
                playerTwoHealth <= FrostyMaximumHealth)
            {
                return "p1";
            }
            return recentActorSide;
        }

        private string ToastyActor(
            int playerOneAction,
            int playerOneComboDamage,
            int playerOneComboHits,
            int playerTwoAction,
            int playerTwoComboDamage,
            int playerTwoComboHits
        )
        {
            if (playerOneAction == ComboAction ||
                playerTwoComboDamage > 0 ||
                playerTwoComboHits > 0)
            {
                return "p1";
            }
            if (playerTwoAction == ComboAction ||
                playerOneComboDamage > 0 ||
                playerOneComboHits > 0)
            {
                return "p2";
            }
            return recentActorSide;
        }

        private static string UppercutActor(
            int playerOneHitBy,
            int playerTwoHitBy
        )
        {
            if (playerTwoHitBy == UppercutAction)
            {
                return "p1";
            }
            if (playerOneHitBy == UppercutAction)
            {
                return "p2";
            }
            return "";
        }

        private void Enqueue(
            string eventKind,
            string eventActorSide,
            string eventTitle,
            string eventDetails
        )
        {
            pendingEvents.Enqueue(new SpecialEvent
            {
                Kind = eventKind,
                ActorSide = eventActorSide ?? "",
                TeamActivePlayerCode = observedTeamActivePlayerCode,
                TeamLeftPlayerCount = observedTeamLeftPlayerCount,
                TeamControllerSlot = observedTeamControllerSlot,
                Title = eventTitle,
                Details = eventDetails
            });
        }

        private MameSpecialProgress Snapshot()
        {
            return new MameSpecialProgress
            {
                Sequence = sequence,
                Kind = kind,
                ActorSide = actorSide,
                TeamActivePlayerCode = teamActivePlayerCode,
                TeamLeftPlayerCount = teamLeftPlayerCount,
                TeamControllerSlot = teamControllerSlot,
                Title = title,
                Details = details
            };
        }

        private sealed class SpecialEvent
        {
            public string Kind { get; set; }
            public string ActorSide { get; set; }
            public int TeamActivePlayerCode { get; set; }
            public int TeamLeftPlayerCount { get; set; }
            public int TeamControllerSlot { get; set; }
            public string Title { get; set; }
            public string Details { get; set; }
        }
    }
}

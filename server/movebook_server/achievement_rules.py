from __future__ import annotations

from dataclasses import dataclass, field

from .achievement_schemas import AchievementEventIn


FULL_HEALTH = 166

MAME_FIGHTER_IDS = {
    0x00: "kano",
    0x01: "sonya",
    0x02: "jax",
    0x03: "nightwolf",
    0x04: "unmasked-sub-zero",
    0x05: "stryker",
    0x06: "sindel",
    0x07: "sektor",
    0x08: "cyrax",
    0x09: "kung-lao",
    0x0A: "kabal",
    0x0B: "sheeva",
    0x0C: "shang-tsung",
    0x0D: "liu-kang",
    0x0F: "kitana",
    0x10: "jade",
    0x11: "mileena",
    0x12: "scorpion",
    0x13: "reptile",
    0x14: "ermac",
    0x15: "classic-sub-zero",
}

VICTORY_MILESTONES = (
    (5, "victories-5"),
    (10, "victories-10"),
    (25, "victories-25"),
    (50, "victories-50"),
    (75, "victories-75"),
    (100, "victories-100"),
)

FIGHTER_DEBUTS = {
    "nightwolf": "fighter-debut-nightwolf",
    "sindel": "fighter-debut-sindel",
    "stryker": "fighter-debut-stryker",
    "jade": "fighter-debut-jade",
    "liu-kang": "fighter-debut-liu-kang",
}

LORE_MATCHUPS = {
    ("sonya", "kano"): "old-scores",
    ("unmasked-sub-zero", "sektor"): "free-will",
    ("kitana", "mileena"): "true-heir",
    ("kabal", "kano"): "leave-black-dragon",
}

GENERAL_FINISHERS = {
    "fatality": "first-fatality",
    "animality": "first-animality",
    "friendship": "first-friendship",
    "babality": "first-babality",
    "stage-fatality": "first-stage-fatality",
}


@dataclass
class AchievementEvaluation:
    victories: int = 0
    matches_played: int = 0
    finishers: int = 0
    unlocks: list[str] = field(default_factory=list)

    def unlock(self, achievement_id: str | None) -> None:
        if achievement_id and achievement_id not in self.unlocks:
            self.unlocks.append(achievement_id)


def evaluate_event(
    event: AchievementEventIn,
    *,
    current_victories: int,
) -> AchievementEvaluation:
    evaluation = AchievementEvaluation()
    if event.event_type in {"round-ended", "match-ended"}:
        _evaluate_round_or_match(
            event,
            evaluation,
            current_victories=current_victories,
        )
    elif event.event_type == "finisher":
        _evaluate_finisher(event, evaluation)
    return evaluation


def _evaluate_round_or_match(
    event: AchievementEventIn,
    evaluation: AchievementEvaluation,
    *,
    current_victories: int,
) -> None:
    player_one = MAME_FIGHTER_IDS.get(event.player_one_id)
    player_two = MAME_FIGHTER_IDS.get(event.player_two_id)

    if event.event_type == "match-ended":
        evaluation.matches_played = 1
        if event.winner_side == "p1" and player_one:
            evaluation.victories = 1
            new_total = current_victories + 1
            for required, achievement_id in VICTORY_MILESTONES:
                if new_total >= required:
                    evaluation.unlock(achievement_id)
            evaluation.unlock(FIGHTER_DEBUTS.get(player_one))
            if player_two:
                evaluation.unlock(LORE_MATCHUPS.get((player_one, player_two)))

    if (
        event.player_one_health == FULL_HEALTH
        and event.player_two_health == 0
        and event.winner_side != "p2"
    ):
        evaluation.unlock("flawless-victory")


def _evaluate_finisher(
    event: AchievementEventIn,
    evaluation: AchievementEvaluation,
) -> None:
    if event.finisher_winner_side != "p1" or not event.finisher_kind:
        return

    evaluation.finishers = 1
    fighter = MAME_FIGHTER_IDS.get(event.finisher_fighter_id)
    evaluation.unlock(GENERAL_FINISHERS.get(event.finisher_kind))
    if event.finisher_kind == "brutality":
        if fighter == "jax":
            evaluation.unlock("jax-brutality")
        elif fighter == "sheeva":
            evaluation.unlock("sheeva-brutality")
    if (
        event.finisher_kind == "antifatality"
        and fighter == "kabal"
        and event.rom_id == "umk3uk"
    ):
        evaluation.unlock("kabal-antifatality")

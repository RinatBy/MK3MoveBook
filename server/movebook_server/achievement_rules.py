from __future__ import annotations

from dataclasses import dataclass, field

from .achievement_schemas import AchievementEventIn


FULL_HEALTH = 166
SUPPORTED_ACHIEVEMENT_ROMS = frozenset(
    {"umk3tm", "umk3tm60", "umk3uk", "umk3uk2"}
)

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
    0x0E: "robot-smoke",
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
    (250, "victories-250"),
    (500, "victories-500"),
    (750, "victories-750"),
    (1000, "victories-1000"),
)

TOWER_COMPLETIONS = {
    0: "tower-novice-complete",
    1: "tower-warrior-complete",
    2: "tower-master-complete",
    3: "tower-master-fourth-complete",
}

FIGHTER_DEBUTS = {
    "nightwolf": "fighter-debut-nightwolf",
    "sindel": "fighter-debut-sindel",
    "stryker": "fighter-debut-stryker",
    "liu-kang": "fighter-debut-liu-kang",
    "jax": "fighter-debut-jax",
    "kabal": "fighter-debut-kabal",
    "sonya": "fighter-debut-sonya",
    "shang-tsung": "fighter-debut-shang-tsung",
    "kano": "fighter-debut-kano",
    "unmasked-sub-zero": "fighter-debut-unmasked-sub-zero",
    "kung-lao": "fighter-debut-kung-lao",
    "sektor": "fighter-debut-sektor",
    "cyrax": "fighter-debut-cyrax",
    "robot-smoke": "fighter-debut-robot-smoke",
}

ALL_FIGHTER_DEBUT_ACHIEVEMENTS = frozenset(FIGHTER_DEBUTS.values())

LORE_MATCHUPS = {
    ("sonya", "kano"): "old-scores",
    ("unmasked-sub-zero", "sektor"): "free-will",
    ("kitana", "mileena"): "true-heir",
    ("kabal", "kano"): "leave-black-dragon",
    ("scorpion", "unmasked-sub-zero"): "fire-and-ice",
    ("liu-kang", "shang-tsung"): "earthrealm-defender",
}

ACHIEVEMENT_PREREQUISITES = {
    "stryker-friendship": frozenset({"first-friendship"}),
}

SPECIAL_ACHIEVEMENTS = {
    "frosty": "frosty",
    "toasty": "toasty",
    "sheeva-42-combo": "sheeva-42-combo",
    "stage-transition-uppercut": "stage-transition-uppercut",
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
    computer_victories: int = 0
    computer_matches_played: int = 0
    online_victories: int = 0
    online_matches_played: int = 0
    finishers: int = 0
    unlocks: list[str] = field(default_factory=list)

    def unlock(self, achievement_id: str | None) -> None:
        if achievement_id and achievement_id not in self.unlocks:
            self.unlocks.append(achievement_id)


def evaluate_event(
    event: AchievementEventIn,
    *,
    current_victories: int,
    unlocked_achievement_ids: set[str] | frozenset[str] = frozenset(),
) -> AchievementEvaluation:
    evaluation = AchievementEvaluation()
    if event.rom_id.lower() not in SUPPORTED_ACHIEVEMENT_ROMS:
        return evaluation
    if not event.match_fully_observed:
        # Achievement events are fail-closed: a client that attached after
        # the match started may drain its outbox, but that partial match must
        # never affect server-side progress.
        return evaluation
    if event.match_scope not in {"computer", "online"}:
        # Old clients and temporary test builds could persist local Versus
        # events with an empty scope. Keep accepting them for idempotent outbox
        # draining, but never let them affect counters or achievements.
        return evaluation
    if (
        event.match_scope == "computer"
        and (
            event.cheat_policy_state != "disabled"
            or event.netplay_total_players != 0
        )
    ):
        # CPU progress is fail-closed. The desktop bridge sends "disabled"
        # only after the live MAME cheat list has been found and remained
        # clean, and a CPU match must have no netplay participants. Online
        # play deliberately ignores the local MAME cheat signal.
        return evaluation
    if (
        event.match_scope == "online"
        and (
            event.netplay_total_players != 2
            or event.team_size != 1
        )
    ):
        # Only a strict Kaillera 1x1 with one fighter on each side is
        # eligible. Team selection, team multiplayer and unknown evidence are
        # accepted as idempotent no-ops.
        return evaluation
    if event.event_type in {"round-ended", "match-ended"}:
        _evaluate_round_or_match(
            event,
            evaluation,
            current_victories=current_victories,
            unlocked_achievement_ids=unlocked_achievement_ids,
        )
    elif event.event_type == "finisher":
        _evaluate_finisher(
            event,
            evaluation,
            unlocked_achievement_ids=unlocked_achievement_ids,
        )
    elif event.event_type == "special":
        _evaluate_special(
            event,
            evaluation,
            unlocked_achievement_ids=unlocked_achievement_ids,
        )
    return evaluation


def _evaluate_round_or_match(
    event: AchievementEventIn,
    evaluation: AchievementEvaluation,
    *,
    current_victories: int,
    unlocked_achievement_ids: set[str] | frozenset[str],
) -> None:
    player_one = MAME_FIGHTER_IDS.get(event.player_one_id)
    player_two = MAME_FIGHTER_IDS.get(event.player_two_id)
    gated_categories_unlocked = _gated_categories_unlocked(
        unlocked_achievement_ids
    )

    if event.event_type == "match-ended":
        evaluation.matches_played = 1
        if event.match_scope == "computer":
            evaluation.computer_matches_played = 1
        elif event.match_scope == "online":
            evaluation.online_matches_played = 1
        if event.winner_side == "p1" and player_one:
            evaluation.victories = 1
            if event.match_scope == "computer":
                evaluation.computer_victories = 1
            elif event.match_scope == "online":
                evaluation.online_victories = 1
            if event.match_scope == "computer" and event.tower_completed:
                evaluation.unlock(TOWER_COMPLETIONS.get(event.tower_id))
            if player_two and gated_categories_unlocked:
                lore_achievement = LORE_MATCHUPS.get((player_one, player_two))
                if _prerequisites_met(
                    lore_achievement,
                    unlocked_achievement_ids,
                ):
                    evaluation.unlock(lore_achievement)
            evaluation.unlock(FIGHTER_DEBUTS.get(player_one))
            new_total = current_victories + 1
            for required, achievement_id in VICTORY_MILESTONES:
                if new_total >= required:
                    evaluation.unlock(achievement_id)

    if (
        gated_categories_unlocked
        and event.winner_side == "p1"
        and event.player_one_danger_seen
        and event.round_number == 1
    ):
        evaluation.unlock("last-strength")

    if (
        gated_categories_unlocked
        and event.winner_side == "p1"
        and event.player_one_danger_seen
        and event.round_number == 2
        and event.player_one_rounds == 1
        and event.player_two_rounds == 1
    ):
        evaluation.unlock("comeback")

    if (
        gated_categories_unlocked
        and event.player_one_health == FULL_HEALTH
        and event.player_two_health == 0
        and event.winner_side != "p2"
    ):
        evaluation.unlock("flawless-victory")


def _evaluate_finisher(
    event: AchievementEventIn,
    evaluation: AchievementEvaluation,
    *,
    unlocked_achievement_ids: set[str] | frozenset[str],
) -> None:
    if event.finisher_winner_side != "p1" or not event.finisher_kind:
        return

    if not _gated_categories_unlocked(unlocked_achievement_ids):
        return
    evaluation.finishers = 1
    fighter = MAME_FIGHTER_IDS.get(event.finisher_fighter_id)
    if (
        event.finisher_kind == "friendship"
        and fighter == "stryker"
        and _prerequisites_met(
            "stryker-friendship",
            unlocked_achievement_ids,
        )
    ):
        evaluation.unlock("stryker-friendship")
    evaluation.unlock(GENERAL_FINISHERS.get(event.finisher_kind))
    if event.finisher_kind == "brutality":
        if fighter == "jax":
            evaluation.unlock("jax-brutality")
        elif fighter == "sheeva":
            evaluation.unlock("sheeva-brutality")
    if (
        event.finisher_kind == "antifatality"
        and fighter == "kabal"
        and event.rom_id.lower() in {"umk3uk", "umk3uk2"}
    ):
        evaluation.unlock("kabal-antifatality")


def _evaluate_special(
    event: AchievementEventIn,
    evaluation: AchievementEvaluation,
    *,
    unlocked_achievement_ids: set[str] | frozenset[str],
) -> None:
    if (
        event.special_actor_side != "p1"
        or not _gated_categories_unlocked(unlocked_achievement_ids)
    ):
        return
    evaluation.unlock(SPECIAL_ACHIEVEMENTS.get(event.special_kind))


def _gated_categories_unlocked(
    unlocked_achievement_ids: set[str] | frozenset[str],
) -> bool:
    return ALL_FIGHTER_DEBUT_ACHIEVEMENTS.issubset(
        unlocked_achievement_ids
    )


def _prerequisites_met(
    achievement_id: str | None,
    unlocked_achievement_ids: set[str] | frozenset[str],
) -> bool:
    if not achievement_id:
        return False
    required = ACHIEVEMENT_PREREQUISITES.get(achievement_id, frozenset())
    return required.issubset(unlocked_achievement_ids)

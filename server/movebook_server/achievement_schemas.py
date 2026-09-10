from __future__ import annotations

from datetime import datetime
from typing import Literal

from pydantic import BaseModel, Field, model_validator


AchievementEventType = Literal[
    "round-ended",
    "match-ended",
    "finisher",
    "special",
]
PlayerSide = Literal["", "p1", "p2"]
MatchScope = Literal["", "computer", "online"]
CheatPolicyState = Literal["unknown", "disabled", "enabled"]


class AchievementEventIn(BaseModel):
    event_key: str = Field(min_length=8, max_length=160)
    reward_group_key: str = Field(default="", max_length=160)
    event_type: AchievementEventType
    rom_id: str = Field(default="unknown", min_length=1, max_length=64)
    match_scope: MatchScope = ""
    cheat_policy_state: CheatPolicyState = "unknown"
    match_fully_observed: bool = False
    netplay_total_players: int = Field(default=0, ge=0, le=8)
    team_size: int = Field(default=0, ge=0, le=3)
    tower_id: int = Field(default=-1, ge=-1, le=3)
    tower_completed: bool = False
    winner_side: PlayerSide = ""
    player_one_id: int = Field(default=-1, ge=-1, le=255)
    player_two_id: int = Field(default=-1, ge=-1, le=255)
    player_one_health: int = Field(default=0, ge=0, le=1000)
    player_two_health: int = Field(default=0, ge=0, le=1000)
    round_number: int = Field(default=0, ge=0, le=10)
    player_one_rounds: int = Field(default=0, ge=0, le=10)
    player_two_rounds: int = Field(default=0, ge=0, le=10)
    player_one_danger_seen: bool = False
    player_two_danger_seen: bool = False
    round_winning_hit: str = Field(default="", max_length=64)
    finisher_kind: str = Field(default="", max_length=32)
    finisher_winner_side: PlayerSide = ""
    finisher_fighter_id: int = Field(default=-1, ge=-1, le=255)
    finisher_opponent_id: int = Field(default=-1, ge=-1, le=255)
    special_kind: str = Field(default="", max_length=64)
    special_actor_side: PlayerSide = ""
    occurred_at: datetime | None = None

    @model_validator(mode="after")
    def validate_tower_completion(self) -> AchievementEventIn:
        if self.tower_completed and (
            self.event_type != "match-ended"
            or self.match_scope != "computer"
            or self.tower_id < 0
        ):
            raise ValueError(
                "tower_completed requires a computer match and a known tower"
            )
        return self


class AchievementUnlockOut(BaseModel):
    achievement_id: str
    unlocked_at: datetime


class AchievementProgressOut(BaseModel):
    user_id: int
    victories: int
    matches_played: int
    computer_victories: int
    computer_matches_played: int
    online_victories: int
    online_matches_played: int
    unclassified_victories: int
    unclassified_matches_played: int
    finishers: int
    verification_state: Literal["unverified-local"] = "unverified-local"
    unlocks: list[AchievementUnlockOut]


class AchievementEventResponse(BaseModel):
    accepted: bool
    new_unlocks: list[str]
    progress: AchievementProgressOut

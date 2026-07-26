from __future__ import annotations

from datetime import datetime
from typing import Literal

from pydantic import BaseModel, Field


AchievementEventType = Literal["round-ended", "match-ended", "finisher"]
PlayerSide = Literal["", "p1", "p2"]


class AchievementEventIn(BaseModel):
    event_key: str = Field(min_length=8, max_length=160)
    event_type: AchievementEventType
    rom_id: str = Field(default="unknown", min_length=1, max_length=64)
    winner_side: PlayerSide = ""
    player_one_id: int = Field(default=-1, ge=-1, le=255)
    player_two_id: int = Field(default=-1, ge=-1, le=255)
    player_one_health: int = Field(default=0, ge=0, le=1000)
    player_two_health: int = Field(default=0, ge=0, le=1000)
    finisher_kind: str = Field(default="", max_length=32)
    finisher_winner_side: PlayerSide = ""
    finisher_fighter_id: int = Field(default=-1, ge=-1, le=255)
    finisher_opponent_id: int = Field(default=-1, ge=-1, le=255)
    occurred_at: datetime | None = None


class AchievementUnlockOut(BaseModel):
    achievement_id: str
    unlocked_at: datetime


class AchievementProgressOut(BaseModel):
    user_id: int
    victories: int
    matches_played: int
    finishers: int
    verification_state: Literal["unverified-local"] = "unverified-local"
    unlocks: list[AchievementUnlockOut]


class AchievementEventResponse(BaseModel):
    accepted: bool
    new_unlocks: list[str]
    progress: AchievementProgressOut

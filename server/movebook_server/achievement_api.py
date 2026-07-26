from __future__ import annotations

from datetime import datetime, timezone

from fastapi import APIRouter, Depends
from sqlalchemy import select
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from .achievement_rules import evaluate_event
from .achievement_schemas import (
    AchievementEventIn,
    AchievementEventResponse,
    AchievementProgressOut,
    AchievementUnlockOut,
)
from .auth_api import get_current_user
from .db import get_db
from .models import AchievementEvent, AchievementUnlock, User, UserStat


router = APIRouter(prefix="/achievements", tags=["achievements"])


def _progress(db: Session, user: User) -> AchievementProgressOut:
    stats = db.get(UserStat, user.id)
    unlocks = db.scalars(
        select(AchievementUnlock)
        .where(AchievementUnlock.user_id == user.id)
        .order_by(AchievementUnlock.unlocked_at, AchievementUnlock.id)
    ).all()
    return AchievementProgressOut(
        user_id=user.id,
        victories=0 if stats is None else stats.victories,
        matches_played=0 if stats is None else stats.matches_played,
        finishers=0 if stats is None else stats.finishers,
        unlocks=[
            AchievementUnlockOut(
                achievement_id=unlock.achievement_id,
                unlocked_at=unlock.unlocked_at,
            )
            for unlock in unlocks
        ],
    )


@router.get("/me", response_model=AchievementProgressOut)
def get_my_achievement_progress(
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
) -> AchievementProgressOut:
    return _progress(db, user)


@router.post("/events", response_model=AchievementEventResponse)
def submit_achievement_event(
    body: AchievementEventIn,
    user: User = Depends(get_current_user),
    db: Session = Depends(get_db),
) -> AchievementEventResponse:
    existing = db.scalar(
        select(AchievementEvent.id).where(
            AchievementEvent.user_id == user.id,
            AchievementEvent.source == "mame-bridge",
            AchievementEvent.event_key == body.event_key,
        )
    )
    if existing is not None:
        return AchievementEventResponse(
            accepted=False,
            new_unlocks=[],
            progress=_progress(db, user),
        )

    stats = db.get(UserStat, user.id)
    if stats is None:
        stats = UserStat(user_id=user.id)
        db.add(stats)
        db.flush()

    evaluation = evaluate_event(body, current_victories=stats.victories)
    event = AchievementEvent(
        user_id=user.id,
        source="mame-bridge",
        event_key=body.event_key,
        event_type=body.event_type,
        verification_state="unverified-local",
        payload=body.model_dump(mode="json"),
        occurred_at=body.occurred_at or datetime.now(timezone.utc),
    )
    db.add(event)
    try:
        db.flush()
    except IntegrityError:
        db.rollback()
        return AchievementEventResponse(
            accepted=False,
            new_unlocks=[],
            progress=_progress(db, user),
        )

    stats.victories += evaluation.victories
    stats.matches_played += evaluation.matches_played
    stats.finishers += evaluation.finishers

    existing_unlocks = set(
        db.scalars(
            select(AchievementUnlock.achievement_id).where(
                AchievementUnlock.user_id == user.id
            )
        ).all()
    )
    new_unlocks = [
        achievement_id
        for achievement_id in evaluation.unlocks
        if achievement_id not in existing_unlocks
    ]
    for achievement_id in new_unlocks:
        db.add(
            AchievementUnlock(
                user_id=user.id,
                achievement_id=achievement_id,
                source_event=event,
            )
        )

    db.commit()
    return AchievementEventResponse(
        accepted=True,
        new_unlocks=new_unlocks,
        progress=_progress(db, user),
    )

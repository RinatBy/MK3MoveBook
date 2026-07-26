from __future__ import annotations

import pytest
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from movebook_server.db import create_database_engine
from movebook_server.models import AchievementEvent, AchievementUnlock, User


def test_achievement_unlock_is_unique_per_user(
    migrated_database: str,
) -> None:
    engine = create_database_engine(migrated_database)
    try:
        with Session(engine, expire_on_commit=False) as session:
            user = User(display_name="Test player")
            session.add(user)
            session.commit()

            session.add(
                AchievementUnlock(
                    user_id=user.id,
                    achievement_id="flawless-victory",
                )
            )
            session.commit()

            session.add(
                AchievementUnlock(
                    user_id=user.id,
                    achievement_id="flawless-victory",
                )
            )
            with pytest.raises(IntegrityError):
                session.commit()
            session.rollback()
    finally:
        engine.dispose()


def test_achievement_event_is_idempotent_per_user_and_source(
    migrated_database: str,
) -> None:
    engine = create_database_engine(migrated_database)
    try:
        with Session(engine, expire_on_commit=False) as session:
            user = User(display_name="Test player")
            session.add(user)
            session.commit()

            session.add(
                AchievementEvent(
                    user_id=user.id,
                    source="mame-bridge",
                    event_key="match-42",
                    event_type="match-ended",
                )
            )
            session.commit()

            session.add(
                AchievementEvent(
                    user_id=user.id,
                    source="mame-bridge",
                    event_key="match-42",
                    event_type="match-ended",
                )
            )
            with pytest.raises(IntegrityError):
                session.commit()
            session.rollback()

            session.add(
                AchievementEvent(
                    user_id=user.id,
                    source="manual-test",
                    event_key="match-42",
                    event_type="match-ended",
                )
            )
            session.commit()
    finally:
        engine.dispose()

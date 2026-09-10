from __future__ import annotations

from sqlalchemy import func, select
from sqlalchemy.orm import Session

from movebook_server.db import create_database_engine
from movebook_server.models import (
    AchievementEvent,
    AchievementUnlock,
    ExternalIdentity,
    User,
    UserStat,
)
from tools.reset_achievement_test_progress import main


def count_for_user(db: Session, model: type[object], user_id: int) -> int:
    return int(
        db.scalar(
            select(func.count()).select_from(model).where(model.user_id == user_id)
        )
        or 0
    )


def test_reset_tool_is_dry_run_by_default_and_preserves_account(
    migrated_database: str,
    monkeypatch,
) -> None:
    engine = create_database_engine(migrated_database)
    with Session(engine) as db:
        user = User(display_name="Reset Tool Tester")
        db.add(user)
        db.flush()
        user_id = user.id
        db.add(
            ExternalIdentity(
                user_id=user_id,
                provider="discord",
                subject="reset-tool-subject",
            )
        )
        event = AchievementEvent(
            user_id=user_id,
            source="mame-bridge",
            event_key="reset-tool:event:1",
            event_type="match-ended",
            payload={},
        )
        db.add(event)
        db.flush()
        db.add(
            AchievementUnlock(
                user_id=user_id,
                achievement_id="old-scores",
                source_event_id=event.id,
            )
        )
        db.add(UserStat(user_id=user_id, victories=4, matches_played=5))
        db.commit()

    monkeypatch.setattr(
        "sys.argv",
        [
            "reset_achievement_test_progress.py",
            "--discord-subject",
            "reset-tool-subject",
            "--database-url",
            migrated_database,
        ],
    )
    assert main() == 0

    with Session(engine) as db:
        assert count_for_user(db, AchievementUnlock, user_id) == 1
        assert count_for_user(db, AchievementEvent, user_id) == 1
        assert count_for_user(db, UserStat, user_id) == 1

    monkeypatch.setattr(
        "sys.argv",
        [
            "reset_achievement_test_progress.py",
            "--user-id",
            str(user_id),
            "--database-url",
            migrated_database,
            "--apply",
        ],
    )
    assert main() == 0

    with Session(engine) as db:
        assert count_for_user(db, AchievementUnlock, user_id) == 0
        assert count_for_user(db, AchievementEvent, user_id) == 0
        assert count_for_user(db, UserStat, user_id) == 0
        assert db.get(User, user_id) is not None
        assert db.scalar(
            select(ExternalIdentity.id).where(
                ExternalIdentity.user_id == user_id
            )
        ) is not None
    engine.dispose()

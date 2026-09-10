from __future__ import annotations

import argparse
import sys
from pathlib import Path

from sqlalchemy import delete, func, select
from sqlalchemy.orm import Session


SERVER_ROOT = Path(__file__).resolve().parents[1]
if str(SERVER_ROOT) not in sys.path:
    sys.path.insert(0, str(SERVER_ROOT))

from movebook_server.config import Settings  # noqa: E402
from movebook_server.db import create_database_engine  # noqa: E402
from movebook_server.models import (  # noqa: E402
    AchievementEvent,
    AchievementUnlock,
    ExternalIdentity,
    User,
    UserStat,
)


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(
        description=(
            "Preview or reset all achievement progress for one explicit "
            "test account. Authentication data is never deleted."
        )
    )
    selector = parser.add_mutually_exclusive_group(required=True)
    selector.add_argument("--user-id", type=int)
    selector.add_argument("--discord-subject")
    parser.add_argument(
        "--database-url",
        help="Override MOVEBOOK_DATABASE_URL for this invocation.",
    )
    parser.add_argument(
        "--apply",
        action="store_true",
        help="Perform the reset. Without this flag the command is dry-run only.",
    )
    return parser.parse_args()


def resolve_user(db: Session, args: argparse.Namespace) -> User:
    if args.user_id is not None:
        user = db.get(User, args.user_id)
    else:
        identity = db.scalar(
            select(ExternalIdentity).where(
                ExternalIdentity.provider == "discord",
                ExternalIdentity.subject == args.discord_subject,
            )
        )
        user = None if identity is None else db.get(User, identity.user_id)
    if user is None:
        raise SystemExit("Selected test account was not found; nothing changed.")
    return user


def row_count(db: Session, model: type[object], user_id: int) -> int:
    return int(
        db.scalar(
            select(func.count()).select_from(model).where(model.user_id == user_id)
        )
        or 0
    )


def main() -> int:
    args = parse_args()
    settings = Settings(_env_file=None)
    database_url = args.database_url or settings.database_url
    engine = create_database_engine(database_url)
    try:
        with Session(engine) as db:
            user = resolve_user(db, args)
            counts = {
                "unlocks": row_count(db, AchievementUnlock, user.id),
                "events": row_count(db, AchievementEvent, user.id),
                "stats": row_count(db, UserStat, user.id),
            }
            print(
                f"target user_id={user.id} display_name={user.display_name!r}; "
                f"unlocks={counts['unlocks']}, events={counts['events']}, "
                f"stats={counts['stats']}"
            )
            if not args.apply:
                print("DRY RUN: add --apply only after verifying the test account.")
                return 0

            db.execute(
                delete(AchievementUnlock).where(
                    AchievementUnlock.user_id == user.id
                )
            )
            db.execute(
                delete(AchievementEvent).where(
                    AchievementEvent.user_id == user.id
                )
            )
            db.execute(delete(UserStat).where(UserStat.user_id == user.id))
            db.commit()
            print("Achievement progress reset; account and login data preserved.")
            return 0
    finally:
        engine.dispose()


if __name__ == "__main__":
    raise SystemExit(main())

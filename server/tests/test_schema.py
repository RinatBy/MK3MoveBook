from __future__ import annotations

from pathlib import Path

from alembic import command
from alembic.config import Config
from sqlalchemy import inspect

from movebook_server.db import create_database_engine


SERVER_ROOT = Path(__file__).resolve().parents[1]


def test_migration_creates_expected_tables(migrated_database: str) -> None:
    engine = create_database_engine(migrated_database)
    try:
        table_names = set(inspect(engine).get_table_names())
    finally:
        engine.dispose()

    assert table_names == {
        "achievement_events",
        "achievement_unlocks",
        "alembic_version",
        "external_identities",
        "refresh_tokens",
        "user_stats",
        "users",
    }


def test_migration_matches_model_metadata(migrated_database: str) -> None:
    config = Config(str(SERVER_ROOT / "alembic.ini"))
    config.set_main_option("script_location", str(SERVER_ROOT / "migrations"))
    config.set_main_option("sqlalchemy.url", migrated_database)

    command.check(config)

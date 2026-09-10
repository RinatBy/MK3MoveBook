from __future__ import annotations

from pathlib import Path

from alembic import command
from alembic.config import Config
from sqlalchemy import inspect, text

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
        "device_login_sessions",
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


def test_discord_login_migration_can_downgrade_and_upgrade(
    migrated_database: str,
) -> None:
    config = Config(str(SERVER_ROOT / "alembic.ini"))
    config.set_main_option("script_location", str(SERVER_ROOT / "migrations"))

    command.downgrade(config, "0001_initial")
    engine = create_database_engine(migrated_database)
    try:
        assert "device_login_sessions" not in inspect(engine).get_table_names()
    finally:
        engine.dispose()

    command.upgrade(config, "head")
    engine = create_database_engine(migrated_database)
    try:
        assert "device_login_sessions" in inspect(engine).get_table_names()
    finally:
        engine.dispose()


def test_split_statistics_migration_preserves_legacy_totals(
    migrated_database: str,
) -> None:
    config = Config(str(SERVER_ROOT / "alembic.ini"))
    config.set_main_option("script_location", str(SERVER_ROOT / "migrations"))
    command.downgrade(config, "0002_discord_login")

    engine = create_database_engine(migrated_database)
    try:
        with engine.begin() as connection:
            connection.execute(
                text("INSERT INTO users (id, display_name) VALUES (77, 'Legacy')")
            )
            connection.execute(
                text(
                    "INSERT INTO user_stats "
                    "(user_id, victories, matches_played, finishers) "
                    "VALUES (77, 36, 48, 3)"
                )
            )
    finally:
        engine.dispose()

    command.upgrade(config, "head")
    engine = create_database_engine(migrated_database)
    try:
        with engine.connect() as connection:
            row = connection.execute(
                text(
                    "SELECT victories, matches_played, computer_victories, "
                    "computer_matches_played, online_victories, "
                    "online_matches_played FROM user_stats WHERE user_id = 77"
                )
            ).one()
            assert tuple(row) == (36, 48, 0, 0, 0, 0)
    finally:
        engine.dispose()

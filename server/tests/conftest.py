from __future__ import annotations

import sys
from pathlib import Path

import pytest
from alembic import command
from alembic.config import Config


SERVER_ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(SERVER_ROOT))


@pytest.fixture
def migrated_database(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> str:
    database_path = tmp_path / "movebook_test.db"
    database_url = f"sqlite:///{database_path.as_posix()}"
    monkeypatch.setenv("MOVEBOOK_DATABASE_URL", database_url)

    config = Config(str(SERVER_ROOT / "alembic.ini"))
    config.set_main_option("script_location", str(SERVER_ROOT / "migrations"))
    command.upgrade(config, "head")

    return database_url

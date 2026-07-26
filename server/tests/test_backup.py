from __future__ import annotations

from datetime import datetime, timedelta, timezone
import importlib.util
import os
from pathlib import Path
import sqlite3
import sys

import pytest


SCRIPT_PATH = (
    Path(__file__).resolve().parents[1] / "deploy" / "backup_sqlite.py"
)
SPEC = importlib.util.spec_from_file_location("movebook_backup_sqlite", SCRIPT_PATH)
assert SPEC is not None
assert SPEC.loader is not None
BACKUP_MODULE = importlib.util.module_from_spec(SPEC)
sys.modules[SPEC.name] = BACKUP_MODULE
SPEC.loader.exec_module(BACKUP_MODULE)


def _create_database(path: Path) -> sqlite3.Connection:
    connection = sqlite3.connect(path)
    connection.execute("PRAGMA journal_mode=WAL")
    connection.execute(
        "CREATE TABLE achievements (id INTEGER PRIMARY KEY, name TEXT NOT NULL)"
    )
    connection.execute(
        "INSERT INTO achievements (name) VALUES (?)",
        ("flawless-victory",),
    )
    connection.commit()
    return connection


def test_backup_is_consistent_while_source_is_open(tmp_path: Path) -> None:
    source_path = tmp_path / "movebook.db"
    output_directory = tmp_path / "backups"
    source = _create_database(source_path)
    try:
        source.execute(
            "INSERT INTO achievements (name) VALUES (?)",
            ("fighter-debut-nightwolf",),
        )
        source.commit()

        backup_path, removed = BACKUP_MODULE.backup_database(
            source_path,
            output_directory,
        )
    finally:
        source.close()

    assert removed == 0
    assert backup_path.parent == output_directory.resolve()
    assert backup_path.name.startswith("movebook-")
    assert backup_path.name.endswith(".sqlite3")

    with sqlite3.connect(backup_path) as backup:
        rows = backup.execute(
            "SELECT name FROM achievements ORDER BY id"
        ).fetchall()
        integrity = backup.execute("PRAGMA integrity_check").fetchone()

    assert rows == [
        ("flawless-victory",),
        ("fighter-debut-nightwolf",),
    ]
    assert integrity == ("ok",)


def test_backup_prunes_only_expired_managed_files(tmp_path: Path) -> None:
    source_path = tmp_path / "movebook.db"
    output_directory = tmp_path / "backups"
    source = _create_database(source_path)
    source.close()
    output_directory.mkdir()

    now = datetime(2026, 7, 26, 12, 0, tzinfo=timezone.utc)
    expired = output_directory / "movebook-expired.sqlite3"
    retained = output_directory / "movebook-retained.sqlite3"
    unrelated = output_directory / "notes.txt"
    expired.write_bytes(b"expired")
    retained.write_bytes(b"retained")
    unrelated.write_text("keep", encoding="utf-8")
    expired_time = (now - timedelta(days=31)).timestamp()
    retained_time = (now - timedelta(days=29)).timestamp()
    os.utime(expired, (expired_time, expired_time))
    os.utime(retained, (retained_time, retained_time))

    backup_path, removed = BACKUP_MODULE.backup_database(
        source_path,
        output_directory,
        retention_days=30,
        now=now,
    )

    assert removed == 1
    assert not expired.exists()
    assert retained.exists()
    assert unrelated.exists()
    assert backup_path.exists()


def test_backup_rejects_invalid_retention(tmp_path: Path) -> None:
    source_path = tmp_path / "movebook.db"
    source = _create_database(source_path)
    source.close()

    with pytest.raises(ValueError, match="at least 1"):
        BACKUP_MODULE.backup_database(
            source_path,
            tmp_path / "backups",
            retention_days=0,
        )

"""Create and retain verified online backups of the MoveBook SQLite database."""

from __future__ import annotations

import argparse
from datetime import datetime, timedelta, timezone
import os
from pathlib import Path
import sqlite3
import sys
import uuid


BACKUP_PREFIX = "movebook-"
BACKUP_SUFFIX = ".sqlite3"


def _sqlite_read_only_uri(path: Path) -> str:
    return f"{path.resolve(strict=True).as_uri()}?mode=ro"


def _remove_expired_backups(
    output_directory: Path,
    retention_days: int,
    now: datetime,
    current_backup: Path,
) -> int:
    cutoff = now - timedelta(days=retention_days)
    removed = 0

    for candidate in output_directory.glob(
        f"{BACKUP_PREFIX}*{BACKUP_SUFFIX}"
    ):
        if candidate == current_backup or candidate.is_symlink():
            continue
        if not candidate.is_file():
            continue
        modified_at = datetime.fromtimestamp(
            candidate.stat().st_mtime,
            tz=timezone.utc,
        )
        if modified_at < cutoff:
            candidate.unlink()
            removed += 1

    return removed


def backup_database(
    database: Path,
    output_directory: Path,
    retention_days: int = 30,
    now: datetime | None = None,
) -> tuple[Path, int]:
    if retention_days < 1:
        raise ValueError("retention_days must be at least 1")

    source_path = database.resolve(strict=True)
    if not source_path.is_file():
        raise ValueError(f"database is not a regular file: {source_path}")

    output_directory.mkdir(parents=True, exist_ok=True)
    output_path = output_directory.resolve(strict=True)
    if not output_path.is_dir():
        raise ValueError(f"backup target is not a directory: {output_path}")

    timestamp = now or datetime.now(timezone.utc)
    if timestamp.tzinfo is None:
        raise ValueError("now must include timezone information")
    timestamp = timestamp.astimezone(timezone.utc)
    stamp = timestamp.strftime("%Y%m%dT%H%M%S.%fZ")
    unique_suffix = uuid.uuid4().hex[:8]
    backup_path = output_path / (
        f"{BACKUP_PREFIX}{stamp}-{unique_suffix}{BACKUP_SUFFIX}"
    )
    temporary_path = output_path / f".{backup_path.name}.tmp"

    try:
        source_connection: sqlite3.Connection | None = None
        backup_connection: sqlite3.Connection | None = None
        try:
            source_connection = sqlite3.connect(
                _sqlite_read_only_uri(source_path),
                uri=True,
                timeout=30,
            )
            backup_connection = sqlite3.connect(str(temporary_path), timeout=30)
            source_connection.backup(backup_connection)
            integrity_result = backup_connection.execute(
                "PRAGMA integrity_check"
            ).fetchone()
            if integrity_result != ("ok",):
                raise RuntimeError(
                    f"backup integrity check failed: {integrity_result!r}"
                )
        finally:
            if backup_connection is not None:
                backup_connection.close()
            if source_connection is not None:
                source_connection.close()

        os.chmod(temporary_path, 0o600)
        with temporary_path.open("rb+") as backup_file:
            os.fsync(backup_file.fileno())
        os.replace(temporary_path, backup_path)
    except Exception:
        temporary_path.unlink(missing_ok=True)
        raise

    removed = _remove_expired_backups(
        output_path,
        retention_days,
        timestamp,
        backup_path,
    )
    return backup_path, removed


def _build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(
        description="Create a verified online backup of a SQLite database."
    )
    parser.add_argument("--database", required=True, type=Path)
    parser.add_argument("--output-directory", required=True, type=Path)
    parser.add_argument("--retention-days", type=int, default=30)
    return parser


def main(argv: list[str] | None = None) -> int:
    args = _build_parser().parse_args(argv)
    try:
        backup_path, removed = backup_database(
            args.database,
            args.output_directory,
            args.retention_days,
        )
    except Exception as exc:
        print(f"MoveBook backup failed: {exc}", file=sys.stderr)
        return 1

    print(f"MoveBook backup created: {backup_path}")
    print(f"Expired backups removed: {removed}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

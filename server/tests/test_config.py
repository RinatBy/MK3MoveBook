from __future__ import annotations

import pytest

from movebook_server.config import Settings


def test_only_movebook_environment_prefix_is_used(
    monkeypatch: pytest.MonkeyPatch,
) -> None:
    monkeypatch.delenv("MOVEBOOK_DATABASE_URL", raising=False)
    monkeypatch.setenv("ORI_DATABASE_URL", "sqlite:///./ori_auth.db")

    settings = Settings(_env_file=None)
    assert settings.database_url == "sqlite:///./movebook_dev.db"

    monkeypatch.setenv(
        "MOVEBOOK_DATABASE_URL",
        "sqlite:///./movebook_test.db",
    )
    settings = Settings(_env_file=None)
    assert settings.database_url == "sqlite:///./movebook_test.db"

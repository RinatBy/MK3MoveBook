from __future__ import annotations

import asyncio
from collections.abc import Generator
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from urllib.parse import parse_qs, urlencode, urlparse

import httpx
import pytest
from fastapi import FastAPI
from sqlalchemy import Engine, func, select
from sqlalchemy.orm import Session, sessionmaker

from movebook_server.config import Settings, get_settings
from movebook_server.db import create_database_engine, get_db
from movebook_server.discord_oauth import (
    DiscordOAuthProvider,
    DiscordUser,
    get_discord_oauth_client,
)
from movebook_server.main import create_app
from movebook_server.models import (
    DeviceLoginSession,
    ExternalIdentity,
    RefreshToken,
    User,
)


class FakeDiscordOAuth(DiscordOAuthProvider):
    def __init__(self) -> None:
        self.last_state: str | None = None
        self.authenticated_codes: list[str] = []

    def authorization_url(self, settings: Settings, state: str) -> str:
        self.last_state = state
        return f"https://discord.test/authorize?{urlencode({'state': state})}"

    async def authenticate(self, code: str, settings: Settings) -> DiscordUser:
        self.authenticated_codes.append(code)
        return DiscordUser(
            subject="112233445566",
            display_name="Discord Tester",
            avatar_url="https://cdn.discord.test/avatar.png",
        )


@dataclass
class AuthContext:
    app: FastAPI
    engine: Engine
    sessions: sessionmaker[Session]
    settings: Settings
    discord: FakeDiscordOAuth


@pytest.fixture
def auth_context(migrated_database: str) -> Generator[AuthContext, None, None]:
    engine = create_database_engine(migrated_database)
    sessions = sessionmaker(
        bind=engine,
        autoflush=False,
        expire_on_commit=False,
        class_=Session,
    )
    settings = Settings(
        _env_file=None,
        database_url=migrated_database,
        public_base_url="http://testserver",
        discord_client_id="test-client-id",
        discord_client_secret="test-client-secret",
        discord_redirect_uri=(
            "http://testserver/api/v1/auth/discord/callback"
        ),
        jwt_secret="test-jwt-secret-that-is-longer-than-32-characters",
        access_ttl_min=15,
        refresh_ttl_days=30,
        login_ttl_seconds=300,
        login_poll_interval_seconds=2,
    )
    discord = FakeDiscordOAuth()
    app = create_app()

    def override_get_db() -> Generator[Session, None, None]:
        with sessions() as session:
            yield session

    app.dependency_overrides[get_db] = override_get_db
    app.dependency_overrides[get_settings] = lambda: settings
    app.dependency_overrides[get_discord_oauth_client] = lambda: discord

    try:
        yield AuthContext(app, engine, sessions, settings, discord)
    finally:
        engine.dispose()


async def complete_login(
    client: httpx.AsyncClient,
    context: AuthContext,
) -> dict[str, object]:
    created = await client.post("/api/v1/auth/device-sessions")
    assert created.status_code == 201
    device = created.json()

    started = await client.get(device["verification_uri"], follow_redirects=False)
    assert started.status_code == 302
    assert started.headers["location"].startswith(
        "https://discord.test/authorize?"
    )
    assert context.discord.last_state is not None

    callback = await client.get(
        "/api/v1/auth/discord/callback",
        params={"code": "valid-code", "state": context.discord.last_state},
    )
    assert callback.status_code == 200

    token = await client.post(
        "/api/v1/auth/device-sessions/token",
        json={"device_code": device["device_code"]},
    )
    assert token.status_code == 200
    return {"device": device, "token": token.json()}


def test_full_device_flow_is_one_time_and_stores_only_hashes(
    auth_context: AuthContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=auth_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            created = await client.post("/api/v1/auth/device-sessions")
            assert created.status_code == 201
            device = created.json()
            assert device["device_code"] not in device["verification_uri"]

            browser_code = parse_qs(
                urlparse(device["verification_uri"]).query
            )["browser_code"][0]
            with auth_context.sessions() as db:
                login_session = db.execute(
                    select(DeviceLoginSession)
                ).scalar_one()
                assert login_session.device_code_hash != device["device_code"]
                assert login_session.browser_code_hash != browser_code
                assert len(login_session.device_code_hash) == 64
                assert len(login_session.browser_code_hash) == 64

            pending = await client.post(
                "/api/v1/auth/device-sessions/token",
                json={"device_code": device["device_code"]},
            )
            assert pending.status_code == 202
            assert pending.headers["retry-after"] == "2"
            assert pending.json() == {"status": "pending", "interval": 2}

            started = await client.get(
                device["verification_uri"],
                follow_redirects=False,
            )
            assert started.status_code == 302
            assert auth_context.discord.last_state is not None

            wrong_state = await client.get(
                "/api/v1/auth/discord/callback",
                params={"code": "valid-code", "state": "x" * 43},
            )
            assert wrong_state.status_code == 400
            assert 'class="auth-result is-error"' in wrong_state.text
            assert "Не удалось войти" in wrong_state.text

            callback = await client.get(
                "/api/v1/auth/discord/callback",
                params={
                    "code": "valid-code",
                    "state": auth_context.discord.last_state,
                },
            )
            assert callback.status_code == 200
            assert 'class="auth-result is-success"' in callback.text
            assert "Вход выполнен" in callback.text
            assert "Эту страницу можно закрыть." in callback.text
            assert 'class="brand-logo-frame"' in callback.text
            assert 'class="brand-logo"' in callback.text
            assert "MK3 MoveBook" not in callback.text
            assert "closeWindow" not in callback.text
            assert "window.history.replaceState" in callback.text

            logo = await client.get("/api/v1/auth/movebook-logo.ico")
            assert logo.status_code == 200
            assert logo.headers["content-type"] == "image/x-icon"
            assert len(logo.content) > 100_000

            repeated_callback = await client.get(
                "/api/v1/auth/discord/callback",
                params={
                    "code": "valid-code",
                    "state": auth_context.discord.last_state,
                },
            )
            assert repeated_callback.status_code == 400

            token = await client.post(
                "/api/v1/auth/device-sessions/token",
                json={"device_code": device["device_code"]},
            )
            assert token.status_code == 200
            token_body = token.json()
            assert token_body["status"] == "authorized"
            assert token_body["user"]["providers"] == ["discord"]

            with auth_context.sessions() as db:
                refresh_row = db.execute(select(RefreshToken)).scalar_one()
                assert refresh_row.token_hash != token_body["refresh_token"]
                assert len(refresh_row.token_hash) == 64

            me = await client.get(
                "/api/v1/auth/me",
                headers={
                    "Authorization": f"Bearer {token_body['access_token']}"
                },
            )
            assert me.status_code == 200
            assert me.json()["display_name"] == "Discord Tester"

            invalid_me = await client.get(
                "/api/v1/auth/me",
                headers={"Authorization": "Bearer invalid-token"},
            )
            assert invalid_me.status_code == 401

            reused = await client.post(
                "/api/v1/auth/device-sessions/token",
                json={"device_code": device["device_code"]},
            )
            assert reused.status_code == 410

    asyncio.run(scenario())


def test_expired_device_session_cannot_issue_tokens(
    auth_context: AuthContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=auth_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            created = await client.post("/api/v1/auth/device-sessions")
            assert created.status_code == 201
            device = created.json()

            with auth_context.sessions() as db:
                login_session = db.execute(
                    select(DeviceLoginSession)
                ).scalar_one()
                login_session.expires_at = datetime.now(timezone.utc) - timedelta(
                    seconds=1
                )
                db.commit()

            expired = await client.post(
                "/api/v1/auth/device-sessions/token",
                json={"device_code": device["device_code"]},
            )
            assert expired.status_code == 410

    asyncio.run(scenario())


def test_refresh_rotation_logout_and_existing_identity_reuse(
    auth_context: AuthContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=auth_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            first = await complete_login(client, auth_context)
            first_token = first["token"]

            refreshed = await client.post(
                "/api/v1/auth/refresh",
                json={"refresh_token": first_token["refresh_token"]},
            )
            assert refreshed.status_code == 200
            second_token = refreshed.json()
            assert second_token["refresh_token"] != first_token["refresh_token"]

            replay = await client.post(
                "/api/v1/auth/refresh",
                json={"refresh_token": first_token["refresh_token"]},
            )
            assert replay.status_code == 401

            second_login = await complete_login(client, auth_context)
            assert second_login["token"]["user"]["id"] == first_token["user"]["id"]

            logout = await client.post(
                "/api/v1/auth/logout",
                json={"refresh_token": second_token["refresh_token"]},
            )
            assert logout.status_code == 200
            assert logout.json() == {"ok": True}

            after_logout = await client.post(
                "/api/v1/auth/refresh",
                json={"refresh_token": second_token["refresh_token"]},
            )
            assert after_logout.status_code == 401

        with auth_context.sessions() as db:
            assert db.scalar(select(func.count()).select_from(User)) == 1
            assert (
                db.scalar(select(func.count()).select_from(ExternalIdentity))
                == 1
            )

    asyncio.run(scenario())


def test_unconfigured_discord_login_is_unavailable(
    auth_context: AuthContext,
) -> None:
    auth_context.app.dependency_overrides[get_settings] = lambda: (
        auth_context.settings.model_copy(
            update={"discord_client_id": "", "discord_client_secret": ""}
        )
    )

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=auth_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            response = await client.post("/api/v1/auth/device-sessions")
        assert response.status_code == 503
        assert response.json()["detail"] == "Discord login is not configured"

    asyncio.run(scenario())

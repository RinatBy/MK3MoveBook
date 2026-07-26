from __future__ import annotations

import asyncio
from collections.abc import Generator
from dataclasses import dataclass

import httpx
import pytest
from fastapi import FastAPI
from sqlalchemy import Engine, func, select
from sqlalchemy.orm import Session, sessionmaker

from movebook_server.config import Settings, get_settings
from movebook_server.db import create_database_engine, get_db
from movebook_server.main import create_app
from movebook_server.models import AchievementEvent, User
from movebook_server.security import create_access_token


@dataclass
class AchievementContext:
    app: FastAPI
    engine: Engine
    sessions: sessionmaker[Session]
    settings: Settings
    user_id: int
    access_token: str


@pytest.fixture
def achievement_context(
    migrated_database: str,
) -> Generator[AchievementContext, None, None]:
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
        jwt_secret="test-jwt-secret-that-is-longer-than-32-characters",
        access_ttl_min=15,
    )
    with sessions() as db:
        user = User(display_name="Achievement Tester")
        db.add(user)
        db.commit()
        user_id = user.id

    app = create_app()

    def override_get_db() -> Generator[Session, None, None]:
        with sessions() as session:
            yield session

    app.dependency_overrides[get_db] = override_get_db
    app.dependency_overrides[get_settings] = lambda: settings
    try:
        yield AchievementContext(
            app=app,
            engine=engine,
            sessions=sessions,
            settings=settings,
            user_id=user_id,
            access_token=create_access_token(user_id, settings),
        )
    finally:
        engine.dispose()


def _headers(context: AchievementContext) -> dict[str, str]:
    return {"Authorization": f"Bearer {context.access_token}"}


def _match_event(
    event_key: str,
    *,
    winner_side: str,
    player_one_id: int = 0x01,
    player_two_id: int = 0x00,
    player_one_health: int = 80,
    player_two_health: int = 0,
) -> dict[str, object]:
    return {
        "event_key": event_key,
        "event_type": "match-ended",
        "rom_id": "umk3uk",
        "winner_side": winner_side,
        "player_one_id": player_one_id,
        "player_two_id": player_two_id,
        "player_one_health": player_one_health,
        "player_two_health": player_two_health,
    }


def test_progress_requires_authentication_and_starts_empty(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            unauthorized = await client.get("/api/v1/achievements/me")
            assert unauthorized.status_code == 401

            progress = await client.get(
                "/api/v1/achievements/me",
                headers=_headers(achievement_context),
            )
            assert progress.status_code == 200
            assert progress.json() == {
                "user_id": achievement_context.user_id,
                "victories": 0,
                "matches_played": 0,
                "finishers": 0,
                "verification_state": "unverified-local",
                "unlocks": [],
            }

    asyncio.run(scenario())


def test_match_events_are_idempotent_and_count_only_p1_victories(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            loss = await client.post(
                "/api/v1/achievements/events",
                json=_match_event("session:loss", winner_side="p2"),
                headers=_headers(achievement_context),
            )
            assert loss.status_code == 200
            assert loss.json()["progress"]["matches_played"] == 1
            assert loss.json()["progress"]["victories"] == 0
            assert loss.json()["new_unlocks"] == []

            last_body: dict[str, object] | None = None
            for sequence in range(1, 6):
                response = await client.post(
                    "/api/v1/achievements/events",
                    json=_match_event(
                        f"session:win:{sequence}",
                        winner_side="p1",
                    ),
                    headers=_headers(achievement_context),
                )
                assert response.status_code == 200
                last_body = response.json()

            assert last_body is not None
            assert last_body["progress"]["victories"] == 5
            assert last_body["progress"]["matches_played"] == 6
            unlock_ids = {
                item["achievement_id"]
                for item in last_body["progress"]["unlocks"]
            }
            assert unlock_ids == {"old-scores", "victories-5"}

            duplicate = await client.post(
                "/api/v1/achievements/events",
                json=_match_event("session:win:5", winner_side="p1"),
                headers=_headers(achievement_context),
            )
            assert duplicate.status_code == 200
            assert duplicate.json()["accepted"] is False
            assert duplicate.json()["progress"]["victories"] == 5
            assert duplicate.json()["new_unlocks"] == []

        with achievement_context.sessions() as db:
            assert db.scalar(
                select(func.count()).select_from(AchievementEvent)
            ) == 6

    asyncio.run(scenario())


def test_flawless_and_finishers_are_server_evaluated(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            flawless = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "session:round:1",
                    "event_type": "round-ended",
                    "rom_id": "umk3uk",
                    "player_one_id": 0x0A,
                    "player_two_id": 0x00,
                    "player_one_health": 166,
                    "player_two_health": 0,
                },
                headers=_headers(achievement_context),
            )
            assert flawless.status_code == 200
            assert flawless.json()["new_unlocks"] == ["flawless-victory"]

            cpu_finisher = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "session:finisher:cpu",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "finisher_kind": "fatality",
                    "finisher_winner_side": "p2",
                    "finisher_fighter_id": 0x00,
                },
                headers=_headers(achievement_context),
            )
            assert cpu_finisher.status_code == 200
            assert cpu_finisher.json()["progress"]["finishers"] == 0

            player_finisher = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "session:finisher:p1",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "finisher_kind": "fatality",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x0A,
                    "finisher_opponent_id": 0x00,
                },
                headers=_headers(achievement_context),
            )
            assert player_finisher.status_code == 200
            assert player_finisher.json()["progress"]["finishers"] == 1
            assert player_finisher.json()["new_unlocks"] == ["first-fatality"]

    asyncio.run(scenario())


def test_event_key_is_scoped_to_user(
    achievement_context: AchievementContext,
) -> None:
    with achievement_context.sessions() as db:
        second_user = User(display_name="Second Tester")
        db.add(second_user)
        db.commit()
        second_token = create_access_token(second_user.id, achievement_context.settings)

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            event = _match_event("shared:event:key", winner_side="p1")
            first = await client.post(
                "/api/v1/achievements/events",
                json=event,
                headers=_headers(achievement_context),
            )
            second = await client.post(
                "/api/v1/achievements/events",
                json=event,
                headers={"Authorization": f"Bearer {second_token}"},
            )
            assert first.status_code == 200
            assert second.status_code == 200
            assert first.json()["accepted"] is True
            assert second.json()["accepted"] is True

    asyncio.run(scenario())

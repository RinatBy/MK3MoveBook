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
from movebook_server.models import AchievementEvent, AchievementUnlock, User, UserStat
from movebook_server.security import create_access_token


FIGHTER_DEBUTS = (
    (0x0C, "fighter-debut-shang-tsung"),
    (0x06, "fighter-debut-sindel"),
    (0x02, "fighter-debut-jax"),
    (0x00, "fighter-debut-kano"),
    (0x0D, "fighter-debut-liu-kang"),
    (0x01, "fighter-debut-sonya"),
    (0x05, "fighter-debut-stryker"),
    (0x04, "fighter-debut-unmasked-sub-zero"),
    (0x08, "fighter-debut-cyrax"),
    (0x07, "fighter-debut-sektor"),
    (0x03, "fighter-debut-nightwolf"),
    (0x09, "fighter-debut-kung-lao"),
    (0x0A, "fighter-debut-kabal"),
    (0x0E, "fighter-debut-robot-smoke"),
)
FIGHTER_DEBUT_IDS = tuple(
    achievement_id for _, achievement_id in FIGHTER_DEBUTS
)


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


def _seed_unlocks(
    context: AchievementContext,
    achievement_ids: tuple[str, ...],
) -> None:
    with context.sessions() as db:
        existing = set(
            db.scalars(
                select(AchievementUnlock.achievement_id).where(
                    AchievementUnlock.user_id == context.user_id
                )
            )
        )
        for achievement_id in achievement_ids:
            if achievement_id not in existing:
                db.add(
                    AchievementUnlock(
                        user_id=context.user_id,
                        achievement_id=achievement_id,
                    )
                )
        db.commit()


def _match_event(
    event_key: str,
    *,
    winner_side: str,
    match_scope: str = "online",
    cheat_policy_state: str = "disabled",
    rom_id: str = "umk3uk",
    tower_id: int = -1,
    tower_completed: bool = False,
    player_one_id: int = 0x01,
    player_two_id: int = 0x00,
    player_one_health: int = 80,
    player_two_health: int = 0,
) -> dict[str, object]:
    return {
        "event_key": event_key,
        "event_type": "match-ended",
        "rom_id": rom_id,
        "match_scope": match_scope,
        "cheat_policy_state": cheat_policy_state,
        "match_fully_observed": True,
        "netplay_total_players": 2 if match_scope == "online" else 0,
        "team_size": 1 if match_scope == "online" else 0,
        "tower_id": tower_id,
        "tower_completed": tower_completed,
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
                "computer_victories": 0,
                "computer_matches_played": 0,
                "online_victories": 0,
                "online_matches_played": 0,
                "unclassified_victories": 0,
                "unclassified_matches_played": 0,
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
            assert last_body["progress"]["online_victories"] == 5
            assert last_body["progress"]["online_matches_played"] == 6
            assert last_body["progress"]["computer_victories"] == 0
            assert last_body["progress"]["unclassified_victories"] == 0
            unlock_ids = {
                item["achievement_id"]
                for item in last_body["progress"]["unlocks"]
            }
            assert unlock_ids == {
                "fighter-debut-sonya",
                "victories-5",
            }

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


def test_unclassified_events_are_noop_for_working_profiles(
    achievement_context: AchievementContext,
) -> None:
    _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            events = [
                _match_event(
                    "local:match:win",
                    winner_side="p1",
                    match_scope="",
                ),
                {
                    "event_key": "local:round:flawless",
                    "event_type": "round-ended",
                    "rom_id": "umk3uk",
                    "cheat_policy_state": "disabled",
                    "winner_side": "p1",
                    "player_one_health": 166,
                    "player_two_health": 0,
                },
                {
                    "event_key": "local:finisher:fatality",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "cheat_policy_state": "disabled",
                    "finisher_kind": "fatality",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x01,
                },
                {
                    "event_key": "local:special:frosty",
                    "event_type": "special",
                    "rom_id": "umk3uk",
                    "cheat_policy_state": "disabled",
                    "special_kind": "frosty",
                    "special_actor_side": "p1",
                },
            ]

            response = None
            for event in events:
                response = await client.post(
                    "/api/v1/achievements/events",
                    json=event,
                    headers=_headers(achievement_context),
                )
                assert response.status_code == 200
                assert response.json()["accepted"] is True
                assert response.json()["new_unlocks"] == []

            assert response is not None
            progress = response.json()["progress"]
            assert progress["victories"] == 0
            assert progress["matches_played"] == 0
            assert progress["computer_victories"] == 0
            assert progress["computer_matches_played"] == 0
            assert progress["online_victories"] == 0
            assert progress["online_matches_played"] == 0
            assert progress["unclassified_victories"] == 0
            assert progress["unclassified_matches_played"] == 0
            assert progress["finishers"] == 0

        with achievement_context.sessions() as db:
            assert db.scalar(
                select(func.count()).select_from(AchievementEvent)
            ) == len(events)

    asyncio.run(scenario())


def test_cheat_policy_blocks_only_unverified_computer_progress(
    achievement_context: AchievementContext,
) -> None:
    _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            online = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "cheat-policy:online-enabled",
                    winner_side="p1",
                    cheat_policy_state="enabled",
                ),
                headers=_headers(achievement_context),
            )
            assert online.status_code == 200
            assert online.json()["accepted"] is True
            assert online.json()["progress"]["online_victories"] == 1

            for state in ("unknown", "enabled"):
                response = await client.post(
                    "/api/v1/achievements/events",
                    json=_match_event(
                        f"cheat-policy:computer-{state}",
                        winner_side="p1",
                        match_scope="computer",
                        cheat_policy_state=state,
                    ),
                    headers=_headers(achievement_context),
                )
                assert response.status_code == 200
                assert response.json()["accepted"] is True
                progress = response.json()["progress"]
                assert progress["victories"] == 1
                assert progress["matches_played"] == 1
                assert progress["computer_victories"] == 0
                assert progress["computer_matches_played"] == 0
                assert progress["online_victories"] == 1
                assert progress["online_matches_played"] == 1

            verified = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "cheat-policy:computer-disabled",
                    winner_side="p1",
                    match_scope="computer",
                    cheat_policy_state="disabled",
                ),
                headers=_headers(achievement_context),
            )
            assert verified.status_code == 200
            progress = verified.json()["progress"]
            assert progress["victories"] == 2
            assert progress["matches_played"] == 2
            assert progress["computer_victories"] == 1
            assert progress["computer_matches_played"] == 1
            assert progress["online_victories"] == 1
            assert progress["online_matches_played"] == 1

    asyncio.run(scenario())


@pytest.mark.parametrize(("fighter_id", "achievement_id"), FIGHTER_DEBUTS)
def test_every_standard_mk3_fighter_has_a_combat_debut(
    achievement_context: AchievementContext,
    fighter_id: int,
    achievement_id: str,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            response = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    f"debut:fighter:{fighter_id}",
                    winner_side="p1",
                    player_one_id=fighter_id,
                    player_two_id=0x15,
                ),
                headers=_headers(achievement_context),
            )
            assert response.status_code == 200
            assert response.json()["new_unlocks"] == [achievement_id]

    asyncio.run(scenario())


def test_jade_victory_does_not_unlock_removed_debut(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            response = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "session:jade:win",
                    winner_side="p1",
                    player_one_id=0x10,
                    player_two_id=0x12,
                ),
                headers=_headers(achievement_context),
            )
            assert response.status_code == 200
            assert response.json()["progress"]["victories"] == 1
            assert response.json()["new_unlocks"] == []

    asyncio.run(scenario())


def test_four_tower_achievements_and_computer_stats(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            unlocks: set[str] = set()
            for tower_id in range(4):
                response = await client.post(
                    "/api/v1/achievements/events",
                    json=_match_event(
                        f"tower:complete:{tower_id}",
                        winner_side="p1",
                        match_scope="computer",
                        tower_id=tower_id,
                        tower_completed=True,
                        player_one_id=0x03,
                        player_two_id=0x19,
                    ),
                    headers=_headers(achievement_context),
                )
                assert response.status_code == 200
                unlocks.update(response.json()["new_unlocks"])

            assert {
                achievement_id
                for achievement_id in unlocks
                if achievement_id.startswith("tower-")
            } == {
                "tower-novice-complete",
                "tower-warrior-complete",
                "tower-master-complete",
                "tower-master-fourth-complete",
            }
            progress = response.json()["progress"]
            assert progress["victories"] == 4
            assert progress["matches_played"] == 4
            assert progress["computer_victories"] == 4
            assert progress["computer_matches_played"] == 4
            assert progress["online_victories"] == 0
            assert progress["online_matches_played"] == 0
            assert progress["unclassified_victories"] == 0
            assert progress["unclassified_matches_played"] == 0

            tower_loss = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "tower:boss:loss",
                    winner_side="p2",
                    match_scope="computer",
                    tower_id=0,
                    tower_completed=True,
                    player_one_id=0x03,
                    player_two_id=0x19,
                ),
                headers=_headers(achievement_context),
            )
            assert tower_loss.status_code == 200
            assert tower_loss.json()["new_unlocks"] == []
            assert tower_loss.json()["progress"]["computer_victories"] == 4
            assert tower_loss.json()["progress"]["computer_matches_played"] == 5

            invalid_context = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "tower:invalid:online",
                    winner_side="p1",
                    match_scope="online",
                    tower_id=0,
                    tower_completed=True,
                ),
                headers=_headers(achievement_context),
            )
            assert invalid_context.status_code == 422

    asyncio.run(scenario())


def test_flawless_and_finishers_are_server_evaluated(
    achievement_context: AchievementContext,
) -> None:
    _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)

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
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
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
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
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
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
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


def test_unrelated_unlocks_can_share_a_match_group(
    achievement_context: AchievementContext,
) -> None:
    _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            debut = _match_event(
                "grouped:match:1",
                winner_side="p1",
                player_one_id=0x01,
                player_two_id=0x00,
            )
            debut["reward_group_key"] = "session:match:1"
            debut["player_one_health"] = 166
            debut["player_two_health"] = 0
            response = await client.post(
                "/api/v1/achievements/events",
                json=debut,
                headers=_headers(achievement_context),
            )
            assert response.status_code == 200
            assert response.json()["new_unlocks"] == [
                "old-scores",
                "flawless-victory",
            ]

            finisher = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "grouped:finisher:1",
                    "reward_group_key": "session:match:1",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "finisher_kind": "fatality",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x01,
                    "finisher_opponent_id": 0x00,
                },
                headers=_headers(achievement_context),
            )
            assert finisher.status_code == 200
            assert finisher.json()["new_unlocks"] == ["first-fatality"]
            assert finisher.json()["progress"]["finishers"] == 1

            second_match = _match_event(
                "grouped:match:2",
                winner_side="p1",
                player_one_id=0x01,
                player_two_id=0x00,
            )
            second_match["reward_group_key"] = "session:match:2"
            lore = await client.post(
                "/api/v1/achievements/events",
                json=second_match,
                headers=_headers(achievement_context),
            )
            assert lore.status_code == 200
            assert lore.json()["new_unlocks"] == []

    asyncio.run(scenario())


def test_prerequisite_chains_require_an_earlier_match(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            blocked = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "liu:lore:blocked",
                    winner_side="p1",
                    player_one_id=0x0D,
                    player_two_id=0x0C,
                ),
                headers=_headers(achievement_context),
            )
            assert blocked.status_code == 200
            assert blocked.json()["new_unlocks"] == [
                "fighter-debut-liu-kang"
            ]

            still_blocked = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "liu:lore:still-blocked",
                    winner_side="p1",
                    player_one_id=0x0D,
                    player_two_id=0x0C,
                ),
                headers=_headers(achievement_context),
            )
            assert still_blocked.status_code == 200
            assert still_blocked.json()["new_unlocks"] == []

            _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)
            unlocked = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "liu:lore:unlocked",
                    winner_side="p1",
                    player_one_id=0x0D,
                    player_two_id=0x0C,
                ),
                headers=_headers(achievement_context),
            )
            assert unlocked.status_code == 200
            assert unlocked.json()["new_unlocks"] == [
                "earthrealm-defender"
            ]

            first_friendship = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "stryker:friendship:first",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "finisher_kind": "friendship",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x05,
                    "finisher_opponent_id": 0x12,
                },
                headers=_headers(achievement_context),
            )
            assert first_friendship.json()["new_unlocks"] == [
                "first-friendship"
            ]

            stryker_friendship = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "stryker:friendship:personal",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "finisher_kind": "friendship",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x05,
                    "finisher_opponent_id": 0x12,
                },
                headers=_headers(achievement_context),
            )
            assert stryker_friendship.json()["new_unlocks"] == [
                "stryker-friendship"
            ]

    asyncio.run(scenario())


def test_low_health_rounds_and_memory_specials_are_evaluated(
    achievement_context: AchievementContext,
) -> None:
    _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            last_strength = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "danger:round:one",
                    "event_type": "round-ended",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "winner_side": "p1",
                    "player_one_id": 0x02,
                    "player_two_id": 0x00,
                    "player_one_health": 4,
                    "player_two_health": 0,
                    "round_number": 1,
                    "player_one_rounds": 1,
                    "player_two_rounds": 0,
                    "player_one_danger_seen": True,
                },
                headers=_headers(achievement_context),
            )
            assert last_strength.json()["new_unlocks"] == ["last-strength"]

            comeback = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "danger:round:two",
                    "event_type": "round-ended",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "winner_side": "p1",
                    "player_one_id": 0x02,
                    "player_two_id": 0x00,
                    "player_one_health": 5,
                    "player_two_health": 0,
                    "round_number": 2,
                    "player_one_rounds": 1,
                    "player_two_rounds": 1,
                    "player_one_danger_seen": True,
                },
                headers=_headers(achievement_context),
            )
            assert comeback.json()["new_unlocks"] == ["comeback"]

            for sequence, special_kind in enumerate(
                (
                    "frosty",
                    "toasty",
                    "sheeva-42-combo",
                    "stage-transition-uppercut",
                ),
                start=1,
            ):
                special = await client.post(
                    "/api/v1/achievements/events",
                    json={
                        "event_key": f"special:event:{sequence}",
                        "event_type": "special",
                        "rom_id": "umk3uk",
                        "match_scope": "online",
                        "match_fully_observed": True,
                        "netplay_total_players": 2,
                        "team_size": 1,
                        "cheat_policy_state": "disabled",
                        "special_kind": special_kind,
                        "special_actor_side": "p1",
                    },
                    headers=_headers(achievement_context),
                )
                assert special.json()["new_unlocks"] == [special_kind]

    asyncio.run(scenario())


def test_special_achievements_wait_for_every_fighter_debut(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            blocked_special = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "gate:special:blocked",
                    "event_type": "special",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "special_kind": "frosty",
                    "special_actor_side": "p1",
                },
                headers=_headers(achievement_context),
            )
            assert blocked_special.json()["new_unlocks"] == []

            blocked_finisher = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "gate:finisher:blocked",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "finisher_kind": "fatality",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x01,
                },
                headers=_headers(achievement_context),
            )
            assert blocked_finisher.json()["new_unlocks"] == []
            assert blocked_finisher.json()["progress"]["finishers"] == 0

            _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)
            unlocked_special = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "gate:special:unlocked",
                    "event_type": "special",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "special_kind": "frosty",
                    "special_actor_side": "p1",
                },
                headers=_headers(achievement_context),
            )
            assert unlocked_special.json()["new_unlocks"] == ["frosty"]

            unlocked_finisher = await client.post(
                "/api/v1/achievements/events",
                json={
                    "event_key": "gate:finisher:unlocked",
                    "event_type": "finisher",
                    "rom_id": "umk3uk",
                    "match_scope": "online",
                    "match_fully_observed": True,
                    "netplay_total_players": 2,
                    "team_size": 1,
                    "cheat_policy_state": "disabled",
                    "finisher_kind": "fatality",
                    "finisher_winner_side": "p1",
                    "finisher_fighter_id": 0x01,
                },
                headers=_headers(achievement_context),
            )
            assert unlocked_finisher.json()["new_unlocks"] == [
                "first-fatality"
            ]

    asyncio.run(scenario())


def test_mode_and_match_evidence_are_fail_closed(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        partial = _match_event(
            "eligibility:partial",
            winner_side="p1",
        )
        partial["match_fully_observed"] = False
        too_many = _match_event(
            "eligibility:three-players",
            winner_side="p1",
        )
        too_many["netplay_total_players"] = 3
        unknown_players = _match_event(
            "eligibility:unknown-players",
            winner_side="p1",
        )
        unknown_players["netplay_total_players"] = 0
        team_selection = _match_event(
            "eligibility:team-selection",
            winner_side="p1",
        )
        team_selection["team_size"] = 3
        unsupported = _match_event(
            "eligibility:unsupported-rom",
            winner_side="p1",
            rom_id="umk3p",
        )
        cpu_with_netplay = _match_event(
            "eligibility:cpu-with-netplay",
            winner_side="p1",
            match_scope="computer",
        )
        cpu_with_netplay["netplay_total_players"] = 2

        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            for event in (
                partial,
                too_many,
                unknown_players,
                team_selection,
                unsupported,
                cpu_with_netplay,
            ):
                first = await client.post(
                    "/api/v1/achievements/events",
                    json=event,
                    headers=_headers(achievement_context),
                )
                assert first.status_code == 200
                assert first.json()["accepted"] is True
                assert first.json()["new_unlocks"] == []
                assert first.json()["progress"]["matches_played"] == 0

                duplicate = await client.post(
                    "/api/v1/achievements/events",
                    json=event,
                    headers=_headers(achievement_context),
                )
                assert duplicate.status_code == 200
                assert duplicate.json()["accepted"] is False
                assert duplicate.json()["progress"]["matches_played"] == 0

    asyncio.run(scenario())


def test_supported_team_and_standard_online_one_on_one_roms_progress(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            for sequence, rom_id in enumerate(("umk3tm", "umk3tm60", "umk3uk", "umk3uk2"), start=1):
                response = await client.post(
                    "/api/v1/achievements/events",
                    json=_match_event(
                        f"supported-rom:match:{sequence}",
                        winner_side="p1",
                        rom_id=rom_id,
                    ),
                    headers=_headers(achievement_context),
                )
                assert response.status_code == 200
                assert response.json()["progress"]["online_victories"] == sequence

    asyncio.run(scenario())


@pytest.mark.parametrize("rom_id", ("umk3tm", "umk3tm60", "umk3uk", "umk3uk2"))
def test_last_debut_only_opens_gated_categories_for_future_events(
    achievement_context: AchievementContext,
    rom_id: str,
) -> None:
    _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS[:-1])

    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            last_debut = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "gate:last-debut",
                    winner_side="p1",
                    rom_id=rom_id,
                    player_one_id=0x0E,
                    player_two_id=0x00,
                ),
                headers=_headers(achievement_context),
            )
            assert last_debut.status_code == 200
            assert last_debut.json()["new_unlocks"] == [
                "fighter-debut-robot-smoke"
            ]

            replay = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "gate:last-debut",
                    winner_side="p1",
                    rom_id=rom_id,
                    player_one_id=0x0E,
                    player_two_id=0x00,
                ),
                headers=_headers(achievement_context),
            )
            assert replay.json()["accepted"] is False
            assert replay.json()["new_unlocks"] == []

            future_match = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "gate:future-match",
                    winner_side="p1",
                    rom_id=rom_id,
                    player_one_id=0x01,
                    player_two_id=0x00,
                ),
                headers=_headers(achievement_context),
            )
            assert future_match.json()["new_unlocks"] == ["old-scores"]

    asyncio.run(scenario())


def test_new_high_victory_milestones_are_reachable(
    achievement_context: AchievementContext,
) -> None:
    async def scenario() -> None:
        with achievement_context.sessions() as db:
            db.add(
                UserStat(
                    user_id=achievement_context.user_id,
                    victories=249,
                    matches_played=249,
                    online_victories=249,
                    online_matches_played=249,
                )
            )
            for achievement_id in (
                "victories-5",
                "victories-10",
                "victories-25",
                "victories-50",
                "victories-75",
                "victories-100",
            ):
                db.add(
                    AchievementUnlock(
                        user_id=achievement_context.user_id,
                        achievement_id=achievement_id,
                    )
                )
            db.commit()

        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(
            transport=transport,
            base_url="http://testserver",
        ) as client:
            response = await client.post(
                "/api/v1/achievements/events",
                json=_match_event(
                    "milestone:250",
                    winner_side="p1",
                    player_one_id=0x14,
                    player_two_id=0x12,
                ),
                headers=_headers(achievement_context),
            )
            assert response.status_code == 200
            assert response.json()["new_unlocks"] == ["victories-250"]
            assert response.json()["progress"]["victories"] == 250

    asyncio.run(scenario())

@pytest.mark.parametrize("rom_id", ("umk3tm", "umk3tm60", "umk3uk", "umk3uk2"))
def test_rom_cpu_debut_and_uk_finisher_pipeline(
    achievement_context: AchievementContext, rom_id: str,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(transport=transport, base_url="http://testserver") as client:
            event = _match_event(
                f"rom-cpu:{rom_id}", winner_side="p1", match_scope="computer",
                rom_id=rom_id, player_one_id=0x03, player_two_id=0x10,
                player_one_health=127,
            )
            response = await client.post("/api/v1/achievements/events", json=event,
                                         headers=_headers(achievement_context))
            assert response.status_code == 200
            body = response.json()
            assert body["new_unlocks"] == ["fighter-debut-nightwolf"]
            assert body["progress"]["computer_victories"] == 1
            duplicate = await client.post("/api/v1/achievements/events", json=event,
                                          headers=_headers(achievement_context))
            assert duplicate.json()["accepted"] is False
            assert duplicate.json()["progress"]["computer_victories"] == 1

            finisher = dict(event, event_key=f"rom-finisher:locked:{rom_id}",
                            event_type="finisher", finisher_kind="antifatality",
                            finisher_winner_side="p1", finisher_fighter_id=0x0A)
            locked = await client.post("/api/v1/achievements/events", json=finisher,
                                       headers=_headers(achievement_context))
            assert locked.json()["new_unlocks"] == []
            assert locked.json()["progress"]["finishers"] == 0
            _seed_unlocks(achievement_context, FIGHTER_DEBUT_IDS)
            replay = await client.post("/api/v1/achievements/events", json=finisher,
                                       headers=_headers(achievement_context))
            assert replay.json()["accepted"] is False
            assert replay.json()["progress"]["finishers"] == 0
            finisher["event_key"] = f"rom-finisher:unlocked:{rom_id}"
            response = await client.post("/api/v1/achievements/events", json=finisher,
                                         headers=_headers(achievement_context))
            assert response.json()["new_unlocks"] == (
                ["kabal-antifatality"] if rom_id in {"umk3uk", "umk3uk2"} else []
            )
    asyncio.run(scenario())


@pytest.mark.parametrize("rom_id", ("umk3tm", "umk3tm60", "umk3uk", "umk3uk2"))
def test_rom_rejected_events_do_not_accumulate_progress(
    achievement_context: AchievementContext, rom_id: str,
) -> None:
    async def scenario() -> None:
        transport = httpx.ASGITransport(app=achievement_context.app)
        async with httpx.AsyncClient(transport=transport, base_url="http://testserver") as client:
            cases = [
                {"match_scope": "computer", "netplay_total_players": 0,
                 "cheat_policy_state": state} for state in ("unknown", "enabled")
            ] + [
                {"match_scope": ""},
                {"match_fully_observed": False},
                {"team_size": 0}, {"team_size": 2}, {"team_size": 3},
            ] + [{"netplay_total_players": n} for n in (0, 1, 3, 4, 5, 6)]
            for number, patch in enumerate(cases):
                event = _match_event(f"rom-blocked:{rom_id}:{number}",
                                     winner_side="p1", rom_id=rom_id)
                event.update(patch)
                response = await client.post("/api/v1/achievements/events", json=event,
                                             headers=_headers(achievement_context))
                assert response.status_code == 200
                assert response.json()["new_unlocks"] == []
                progress = response.json()["progress"]
                assert progress["matches_played"] == progress["victories"] == progress["finishers"] == 0
    asyncio.run(scenario())

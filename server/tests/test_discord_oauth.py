from __future__ import annotations

import asyncio
from base64 import b64decode
from urllib.parse import parse_qs, urlparse

import httpx

from movebook_server.config import Settings
from movebook_server.discord_oauth import (
    DISCORD_AUTHORIZE_URL,
    DISCORD_CURRENT_USER_URL,
    DISCORD_TOKEN_REVOKE_URL,
    DISCORD_TOKEN_URL,
    DiscordOAuthClient,
)


def discord_settings() -> Settings:
    return Settings(
        _env_file=None,
        discord_client_id="123456789",
        discord_client_secret="test-discord-secret",
        discord_redirect_uri=(
            "http://127.0.0.1:8010/api/v1/auth/discord/callback"
        ),
    )


def test_authorization_url_uses_code_grant_state_and_identify_only() -> None:
    settings = discord_settings()
    client = DiscordOAuthClient()

    url = client.authorization_url(settings, "unique-oauth-state")
    parsed = urlparse(url)
    query = parse_qs(parsed.query)

    assert f"{parsed.scheme}://{parsed.netloc}{parsed.path}" == DISCORD_AUTHORIZE_URL
    assert query == {
        "response_type": ["code"],
        "client_id": [settings.discord_client_id],
        "scope": ["identify"],
        "state": ["unique-oauth-state"],
        "redirect_uri": [settings.discord_redirect_uri],
    }


def test_code_exchange_uses_form_data_and_bearer_profile_request() -> None:
    settings = discord_settings()
    requests: list[httpx.Request] = []

    async def handler(request: httpx.Request) -> httpx.Response:
        requests.append(request)
        if str(request.url) == DISCORD_TOKEN_URL:
            form = parse_qs((await request.aread()).decode("utf-8"))
            assert request.headers["content-type"].startswith(
                "application/x-www-form-urlencoded"
            )
            auth_scheme, encoded_auth = request.headers["authorization"].split(
                " ", 1
            )
            assert auth_scheme == "Basic"
            assert b64decode(encoded_auth).decode("utf-8") == (
                f"{settings.discord_client_id}:{settings.discord_client_secret}"
            )
            assert form == {
                "grant_type": ["authorization_code"],
                "code": ["discord-code"],
                "redirect_uri": [settings.discord_redirect_uri],
            }
            return httpx.Response(
                200,
                json={
                    "access_token": "discord-access-token",
                    "token_type": "Bearer",
                },
            )

        if str(request.url) == DISCORD_CURRENT_USER_URL:
            assert request.headers["authorization"] == "Bearer discord-access-token"
            return httpx.Response(
                200,
                json={
                    "id": "9988776655",
                    "username": "movebook-player",
                    "global_name": "MoveBook Player",
                    "avatar": "avatar-hash",
                },
            )

        assert str(request.url) == DISCORD_TOKEN_REVOKE_URL
        revoke_form = parse_qs((await request.aread()).decode("utf-8"))
        assert revoke_form == {
            "token": ["discord-access-token"],
            "token_type_hint": ["access_token"],
        }
        return httpx.Response(200)

    client = DiscordOAuthClient(transport=httpx.MockTransport(handler))
    user = asyncio.run(client.authenticate("discord-code", settings))

    assert len(requests) == 3
    assert user.subject == "9988776655"
    assert user.display_name == "MoveBook Player"
    assert user.avatar_url == (
        "https://cdn.discordapp.com/avatars/9988776655/avatar-hash.png?size=128"
    )

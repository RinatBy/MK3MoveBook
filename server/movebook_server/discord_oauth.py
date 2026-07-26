from __future__ import annotations

from dataclasses import dataclass
from typing import Protocol
from urllib.parse import quote, urlencode

import httpx

from .config import Settings


DISCORD_AUTHORIZE_URL = "https://discord.com/oauth2/authorize"
DISCORD_TOKEN_URL = "https://discord.com/api/oauth2/token"
DISCORD_TOKEN_REVOKE_URL = "https://discord.com/api/oauth2/token/revoke"
DISCORD_CURRENT_USER_URL = "https://discord.com/api/v10/users/@me"


class DiscordOAuthError(RuntimeError):
    pass


@dataclass(frozen=True)
class DiscordUser:
    subject: str
    display_name: str
    avatar_url: str | None


class DiscordOAuthProvider(Protocol):
    def authorization_url(self, settings: Settings, state: str) -> str: ...

    async def authenticate(self, code: str, settings: Settings) -> DiscordUser: ...


class DiscordOAuthClient:
    def __init__(
        self,
        *,
        transport: httpx.AsyncBaseTransport | None = None,
        timeout_seconds: float = 10.0,
    ) -> None:
        self._transport = transport
        self._timeout_seconds = timeout_seconds

    def authorization_url(self, settings: Settings, state: str) -> str:
        query = urlencode(
            {
                "response_type": "code",
                "client_id": settings.discord_client_id,
                "scope": "identify",
                "state": state,
                "redirect_uri": settings.discord_redirect_uri,
            }
        )
        return f"{DISCORD_AUTHORIZE_URL}?{query}"

    async def authenticate(self, code: str, settings: Settings) -> DiscordUser:
        headers = {"User-Agent": "MK3MoveBook/0.2"}
        try:
            async with httpx.AsyncClient(
                transport=self._transport,
                timeout=self._timeout_seconds,
                headers=headers,
            ) as client:
                token_response = await client.post(
                    DISCORD_TOKEN_URL,
                    data={
                        "grant_type": "authorization_code",
                        "code": code,
                        "redirect_uri": settings.discord_redirect_uri,
                    },
                    auth=httpx.BasicAuth(
                        settings.discord_client_id,
                        settings.discord_client_secret,
                    ),
                )
                if token_response.status_code != 200:
                    raise DiscordOAuthError("Discord token exchange failed")

                token_payload = token_response.json()
                access_token = token_payload.get("access_token")
                token_type = str(token_payload.get("token_type", ""))
                if not access_token or token_type.lower() != "bearer":
                    raise DiscordOAuthError("Discord returned an invalid token")

                try:
                    user_response = await client.get(
                        DISCORD_CURRENT_USER_URL,
                        headers={"Authorization": f"Bearer {access_token}"},
                    )
                    if user_response.status_code != 200:
                        raise DiscordOAuthError("Discord profile request failed")
                    user_payload = user_response.json()
                finally:
                    try:
                        await client.post(
                            DISCORD_TOKEN_REVOKE_URL,
                            data={
                                "token": access_token,
                                "token_type_hint": "access_token",
                            },
                            auth=httpx.BasicAuth(
                                settings.discord_client_id,
                                settings.discord_client_secret,
                            ),
                        )
                    except httpx.RequestError:
                        # Profile data is already verified. A transient revoke
                        # failure must not discard the completed user login.
                        pass
        except (httpx.RequestError, ValueError) as exc:
            raise DiscordOAuthError("Discord returned an invalid response") from exc

        subject = str(user_payload.get("id", "")).strip()
        username = str(user_payload.get("username", "")).strip()
        global_name = user_payload.get("global_name")
        display_name = (
            str(global_name).strip() if global_name else username
        )
        if not subject or not display_name:
            raise DiscordOAuthError("Discord returned an invalid profile")

        avatar_hash = user_payload.get("avatar")
        avatar_url = None
        if avatar_hash:
            avatar_url = (
                "https://cdn.discordapp.com/avatars/"
                f"{quote(subject, safe='')}/{quote(str(avatar_hash), safe='')}.png"
                "?size=128"
            )

        return DiscordUser(
            subject=subject,
            display_name=display_name,
            avatar_url=avatar_url,
        )


def get_discord_oauth_client() -> DiscordOAuthProvider:
    return DiscordOAuthClient()

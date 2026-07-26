from __future__ import annotations

import hashlib
import secrets
from datetime import datetime, timedelta, timezone

import jwt

from .config import Settings


ACCESS_TOKEN_ISSUER = "mk3-movebook-server"
ACCESS_TOKEN_AUDIENCE = "mk3-movebook-client"
ACCESS_TOKEN_ALGORITHM = "HS256"


class AuthConfigurationError(RuntimeError):
    pass


class InvalidAccessToken(ValueError):
    pass


def new_opaque_token(byte_count: int = 32) -> str:
    return secrets.token_urlsafe(byte_count)


def hash_opaque_token(raw_token: str) -> str:
    return hashlib.sha256(raw_token.encode("utf-8")).hexdigest()


def create_access_token(
    user_id: int,
    settings: Settings,
    *,
    now: datetime | None = None,
) -> str:
    if not settings.jwt_is_configured():
        raise AuthConfigurationError("MoveBook JWT signing is not configured")

    issued_at = now or datetime.now(timezone.utc)
    expires_at = issued_at + timedelta(minutes=settings.access_ttl_min)
    payload = {
        "sub": str(user_id),
        "type": "access",
        "iss": ACCESS_TOKEN_ISSUER,
        "aud": ACCESS_TOKEN_AUDIENCE,
        "iat": int(issued_at.timestamp()),
        "exp": int(expires_at.timestamp()),
        "jti": new_opaque_token(16),
    }
    return jwt.encode(
        payload,
        settings.jwt_secret,
        algorithm=ACCESS_TOKEN_ALGORITHM,
    )


def decode_access_token(token: str, settings: Settings) -> int:
    if not settings.jwt_is_configured():
        raise AuthConfigurationError("MoveBook JWT signing is not configured")

    try:
        payload = jwt.decode(
            token,
            settings.jwt_secret,
            algorithms=[ACCESS_TOKEN_ALGORITHM],
            audience=ACCESS_TOKEN_AUDIENCE,
            issuer=ACCESS_TOKEN_ISSUER,
        )
        if payload.get("type") != "access":
            raise InvalidAccessToken("Unexpected token type")
        return int(payload["sub"])
    except (KeyError, TypeError, ValueError, jwt.InvalidTokenError) as exc:
        raise InvalidAccessToken("Invalid access token") from exc

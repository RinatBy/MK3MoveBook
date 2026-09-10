from __future__ import annotations

from datetime import datetime, timedelta, timezone
from html import escape
from pathlib import Path
from urllib.parse import quote

from fastapi import APIRouter, Depends, HTTPException, Query, Response, status
from fastapi.responses import FileResponse, HTMLResponse, RedirectResponse
from fastapi.security import HTTPAuthorizationCredentials, HTTPBearer
from sqlalchemy import select, update
from sqlalchemy.exc import IntegrityError
from sqlalchemy.orm import Session

from .config import Settings, get_settings
from .db import get_db
from .discord_oauth import (
    DiscordOAuthError,
    DiscordOAuthProvider,
    DiscordUser,
    get_discord_oauth_client,
)
from .models import DeviceLoginSession, ExternalIdentity, RefreshToken, User
from .schemas import (
    AuthTokenResponse,
    DeviceSessionCreateResponse,
    DeviceSessionStatusResponse,
    DeviceTokenRequest,
    LogoutResponse,
    RefreshRequest,
    UserOut,
)
from .security import (
    AuthConfigurationError,
    InvalidAccessToken,
    create_access_token,
    decode_access_token,
    hash_opaque_token,
    new_opaque_token,
)


router = APIRouter(prefix="/auth", tags=["auth"])
bearer_scheme = HTTPBearer(auto_error=False)
AUTH_LOGO_PATH = Path(__file__).resolve().parent / "static" / "movebook-logo.ico"


def _as_utc(value: datetime) -> datetime:
    if value.tzinfo is None:
        return value.replace(tzinfo=timezone.utc)
    return value.astimezone(timezone.utc)


def _is_expired(login_session: DeviceLoginSession, now: datetime) -> bool:
    return _as_utc(login_session.expires_at) <= now


def _require_discord_login_configuration(settings: Settings) -> None:
    if not settings.discord_is_configured() or not settings.jwt_is_configured():
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Discord login is not configured",
        )


def _require_jwt_configuration(settings: Settings) -> None:
    if not settings.jwt_is_configured():
        raise HTTPException(
            status_code=status.HTTP_503_SERVICE_UNAVAILABLE,
            detail="Token signing is not configured",
        )


def _user_out(user: User) -> UserOut:
    providers = sorted({identity.provider for identity in user.identities})
    return UserOut(
        id=user.id,
        display_name=user.display_name,
        avatar_url=user.avatar_url,
        providers=providers,
    )


@router.get(
    "/movebook-logo.ico",
    include_in_schema=False,
    response_class=FileResponse,
)
def movebook_logo() -> FileResponse:
    return FileResponse(
        AUTH_LOGO_PATH,
        media_type="image/x-icon",
        headers={"Cache-Control": "public, max-age=604800, immutable"},
    )


def _browser_page(message: str, *, status_code: int = 200) -> HTMLResponse:
    is_success = status_code < 400
    result_class = "is-success" if is_success else "is-error"
    headline = "Вход выполнен" if is_success else "Не удалось войти"
    display_message = "Эту страницу можно закрыть." if is_success else message
    html = (
        """<!doctype html>
<html lang="ru">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<meta name="theme-color" content="#140c1d">
<title>MoveBook — результат входа</title>
<link rel="icon" href="/api/v1/auth/movebook-logo.ico" type="image/x-icon">
<style>
* { box-sizing: border-box; }
html, body { min-height: 100%; margin: 0; }
body {
    display: grid;
    min-height: 100vh;
    padding: 28px;
    overflow-x: hidden;
    color: #2f2028;
    background:
        radial-gradient(circle at 18% 18%, rgba(166, 42, 67, .28), transparent 30%),
        radial-gradient(circle at 82% 74%, rgba(109, 58, 140, .3), transparent 34%),
        linear-gradient(135deg, #0d0713, #21142d 52%, #120a1a);
    font-family: Georgia, "Times New Roman", serif;
    place-items: center;
}
body::before {
    position: fixed;
    inset: 20px;
    pointer-events: none;
    content: "";
    border: 1px solid rgba(226, 189, 76, .22);
    box-shadow: inset 0 0 80px rgba(0, 0, 0, .36);
}
.auth-result-card {
    position: relative;
    width: min(560px, 100%);
    padding: 48px 44px 40px;
    overflow: hidden;
    text-align: center;
    background:
        linear-gradient(90deg, rgba(117, 64, 45, .07) 1px, transparent 1px) 0 0 / 26px 26px,
        linear-gradient(rgba(117, 64, 45, .055) 1px, transparent 1px) 0 0 / 26px 26px,
        linear-gradient(145deg, #f7ecd7, #e5d1af);
    border: 2px solid #c8a64d;
    border-radius: 12px;
    box-shadow:
        0 0 0 6px rgba(86, 24, 43, .88),
        0 28px 90px rgba(0, 0, 0, .62),
        inset 0 0 48px rgba(91, 49, 34, .15);
}
.auth-result-card::before,
.auth-result-card::after {
    position: absolute;
    width: 78px;
    height: 78px;
    content: "";
    border-color: rgba(117, 33, 53, .42);
}
.auth-result-card::before {
    top: 14px;
    left: 14px;
    border-top: 2px solid;
    border-left: 2px solid;
}
.auth-result-card::after {
    right: 14px;
    bottom: 14px;
    border-right: 2px solid;
    border-bottom: 2px solid;
}
.brand-logo-frame {
    display: grid;
    width: 112px;
    height: 112px;
    margin: -12px auto 4px;
    overflow: hidden;
    background: radial-gradient(circle, #5d2337, #25172f 72%);
    border: 2px solid #d1ae4c;
    border-radius: 50%;
    box-shadow: 0 0 0 5px rgba(87, 22, 39, .15);
    place-items: center;
}
.brand-logo {
    display: block;
    width: 130px;
    height: 130px;
    object-fit: contain;
    filter: drop-shadow(0 7px 10px rgba(48, 18, 24, .28));
}
h1 {
    margin: 12px 0 16px;
    color: #3d2029;
    font: 700 clamp(28px, 6vw, 40px)/1.1 Georgia, serif;
}
p {
    max-width: 430px;
    margin: 0 auto;
    color: #65515a;
    font-size: 17px;
    line-height: 1.65;
}

@media (max-width: 560px) {
    body { padding: 18px; }
    body::before { inset: 9px; }
    .auth-result-card { padding: 38px 24px 32px; }
}
</style>
</head>
<body class="auth-result __RESULT_CLASS__">
<main class="auth-result-card" role="status" aria-live="polite">
    <span class="brand-logo-frame" aria-hidden="true">
        <img class="brand-logo" src="/api/v1/auth/movebook-logo.ico" alt="">
    </span>
    <h1>__HEADLINE__</h1>
    <p>__MESSAGE__</p>
</main>
<script>
if (window.history && window.history.replaceState) {
    window.history.replaceState(null, document.title, window.location.pathname);
}
</script>
</body>
</html>"""
        .replace("__RESULT_CLASS__", result_class)
        .replace("__HEADLINE__", headline)
        .replace("__MESSAGE__", escape(display_message))
    )
    return HTMLResponse(html, status_code=status_code)


@router.post(
    "/device-sessions",
    response_model=DeviceSessionCreateResponse,
    status_code=status.HTTP_201_CREATED,
)
def create_device_session(
    settings: Settings = Depends(get_settings),
    db: Session = Depends(get_db),
) -> DeviceSessionCreateResponse:
    _require_discord_login_configuration(settings)

    device_code = new_opaque_token(48)
    browser_code = new_opaque_token(32)
    now = datetime.now(timezone.utc)
    expires_at = now + timedelta(seconds=settings.login_ttl_seconds)
    login_session = DeviceLoginSession(
        provider="discord",
        device_code_hash=hash_opaque_token(device_code),
        browser_code_hash=hash_opaque_token(browser_code),
        status="pending",
        created_at=now,
        expires_at=expires_at,
    )
    db.add(login_session)
    db.commit()

    verification_uri = (
        f"{settings.public_base_url.rstrip('/')}/api/v1/auth/discord/start"
        f"?browser_code={quote(browser_code, safe='')}"
    )
    return DeviceSessionCreateResponse(
        device_code=device_code,
        verification_uri=verification_uri,
        expires_in=settings.login_ttl_seconds,
        interval=settings.login_poll_interval_seconds,
    )


@router.get(
    "/discord/start",
    include_in_schema=False,
    response_class=RedirectResponse,
)
def start_discord_login(
    browser_code: str = Query(min_length=32, max_length=256),
    settings: Settings = Depends(get_settings),
    db: Session = Depends(get_db),
    discord: DiscordOAuthProvider = Depends(get_discord_oauth_client),
) -> RedirectResponse:
    _require_discord_login_configuration(settings)

    login_session = db.execute(
        select(DeviceLoginSession).where(
            DeviceLoginSession.browser_code_hash
            == hash_opaque_token(browser_code),
            DeviceLoginSession.provider == "discord",
        )
    ).scalar_one_or_none()
    if login_session is None:
        raise HTTPException(status_code=404, detail="Login link not found")

    now = datetime.now(timezone.utc)
    if _is_expired(login_session, now):
        login_session.status = "expired"
        db.commit()
        raise HTTPException(status_code=410, detail="Login link expired")
    if login_session.status != "pending":
        raise HTTPException(status_code=409, detail="Login link already used")

    oauth_state = new_opaque_token(32)
    login_session.oauth_state_hash = hash_opaque_token(oauth_state)
    db.commit()
    return RedirectResponse(
        discord.authorization_url(settings, oauth_state),
        status_code=status.HTTP_302_FOUND,
    )


def _find_or_create_discord_user(
    db: Session,
    discord_user: DiscordUser,
) -> User:
    identity = db.execute(
        select(ExternalIdentity).where(
            ExternalIdentity.provider == "discord",
            ExternalIdentity.subject == discord_user.subject,
        )
    ).scalar_one_or_none()
    if identity is not None:
        identity.display_name = discord_user.display_name[:100]
        identity.avatar_url = discord_user.avatar_url
        identity.user.display_name = discord_user.display_name[:100]
        identity.user.avatar_url = discord_user.avatar_url
        return identity.user

    user = User(
        display_name=discord_user.display_name[:100],
        avatar_url=discord_user.avatar_url,
    )
    user.identities.append(
        ExternalIdentity(
            provider="discord",
            subject=discord_user.subject,
            display_name=discord_user.display_name[:100],
            avatar_url=discord_user.avatar_url,
        )
    )
    db.add(user)
    db.flush()
    return user


@router.get(
    "/discord/callback",
    include_in_schema=False,
    response_class=HTMLResponse,
)
async def discord_callback(
    state_value: str = Query(alias="state", min_length=32, max_length=256),
    code: str | None = Query(default=None, max_length=512),
    error: str | None = Query(default=None, max_length=128),
    settings: Settings = Depends(get_settings),
    db: Session = Depends(get_db),
    discord: DiscordOAuthProvider = Depends(get_discord_oauth_client),
) -> HTMLResponse:
    _require_discord_login_configuration(settings)

    login_session = db.execute(
        select(DeviceLoginSession).where(
            DeviceLoginSession.oauth_state_hash
            == hash_opaque_token(state_value),
            DeviceLoginSession.provider == "discord",
        )
    ).scalar_one_or_none()
    if login_session is None or login_session.status != "pending":
        return _browser_page(
            "Ссылка входа недействительна или уже использована.",
            status_code=400,
        )

    now = datetime.now(timezone.utc)
    if _is_expired(login_session, now):
        login_session.status = "expired"
        login_session.oauth_state_hash = None
        db.commit()
        return _browser_page("Время входа истекло.", status_code=410)

    if error or not code:
        login_session.status = "failed"
        login_session.failure_code = "provider_denied" if error else "missing_code"
        login_session.oauth_state_hash = None
        db.commit()
        return _browser_page("Вход через Discord отменён.", status_code=400)

    try:
        discord_user = await discord.authenticate(code, settings)
    except DiscordOAuthError:
        login_session.status = "failed"
        login_session.failure_code = "provider_error"
        login_session.oauth_state_hash = None
        db.commit()
        return _browser_page(
            "Discord не подтвердил вход. Попробуйте ещё раз.",
            status_code=502,
        )

    user = _find_or_create_discord_user(db, discord_user)
    login_session.user = user
    login_session.status = "authorized"
    login_session.authorized_at = now
    login_session.oauth_state_hash = None
    login_session_id = login_session.id
    try:
        db.commit()
    except IntegrityError:
        db.rollback()
        identity = db.execute(
            select(ExternalIdentity).where(
                ExternalIdentity.provider == "discord",
                ExternalIdentity.subject == discord_user.subject,
            )
        ).scalar_one_or_none()
        login_session = db.get(DeviceLoginSession, login_session_id)
        if (
            identity is None
            or login_session is None
            or login_session.status != "pending"
        ):
            return _browser_page(
                "Не удалось связать аккаунт. Создайте новую ссылку входа.",
                status_code=409,
            )
        identity.display_name = discord_user.display_name[:100]
        identity.avatar_url = discord_user.avatar_url
        identity.user.display_name = discord_user.display_name[:100]
        identity.user.avatar_url = discord_user.avatar_url
        login_session.user = identity.user
        login_session.status = "authorized"
        login_session.authorized_at = now
        login_session.oauth_state_hash = None
        db.commit()
    return _browser_page("Эту страницу можно закрыть.")


def _new_refresh_token(user: User, settings: Settings, now: datetime) -> tuple[str, RefreshToken]:
    raw_refresh = new_opaque_token(48)
    refresh_row = RefreshToken(
        user_id=user.id,
        token_hash=hash_opaque_token(raw_refresh),
        created_at=now,
        expires_at=now + timedelta(days=settings.refresh_ttl_days),
    )
    return raw_refresh, refresh_row


def _token_response(
    user: User,
    raw_refresh: str,
    settings: Settings,
) -> AuthTokenResponse:
    try:
        access_token = create_access_token(user.id, settings)
    except AuthConfigurationError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc
    return AuthTokenResponse(
        access_token=access_token,
        refresh_token=raw_refresh,
        expires_in=settings.access_ttl_min * 60,
        user=_user_out(user),
    )


@router.post(
    "/device-sessions/token",
    response_model=AuthTokenResponse | DeviceSessionStatusResponse,
    responses={202: {"model": DeviceSessionStatusResponse}},
)
def poll_device_session(
    body: DeviceTokenRequest,
    response: Response,
    settings: Settings = Depends(get_settings),
    db: Session = Depends(get_db),
) -> AuthTokenResponse | DeviceSessionStatusResponse:
    _require_jwt_configuration(settings)

    login_session = db.execute(
        select(DeviceLoginSession)
        .where(
            DeviceLoginSession.device_code_hash
            == hash_opaque_token(body.device_code)
        )
        .with_for_update()
    ).scalar_one_or_none()
    if login_session is None:
        raise HTTPException(status_code=404, detail="Device session not found")

    now = datetime.now(timezone.utc)
    if _is_expired(login_session, now):
        login_session.status = "expired"
        db.commit()
        raise HTTPException(status_code=410, detail="Device session expired")
    if login_session.status == "pending":
        response.status_code = status.HTTP_202_ACCEPTED
        response.headers["Retry-After"] = str(
            settings.login_poll_interval_seconds
        )
        return DeviceSessionStatusResponse(
            interval=settings.login_poll_interval_seconds
        )
    if login_session.status == "failed":
        raise HTTPException(status_code=400, detail="Discord login failed")
    if login_session.status in {"consumed", "expired"}:
        raise HTTPException(status_code=410, detail="Device session already used")
    if login_session.status != "authorized" or login_session.user is None:
        raise HTTPException(status_code=409, detail="Invalid device session state")

    claim = db.execute(
        update(DeviceLoginSession)
        .where(
            DeviceLoginSession.id == login_session.id,
            DeviceLoginSession.status == "authorized",
        )
        .values(status="consumed", consumed_at=now)
    )
    if claim.rowcount != 1:
        db.rollback()
        raise HTTPException(status_code=410, detail="Device session already used")

    raw_refresh, refresh_row = _new_refresh_token(
        login_session.user,
        settings,
        now,
    )
    db.add(refresh_row)
    result = _token_response(login_session.user, raw_refresh, settings)
    db.commit()
    return result


@router.post("/refresh", response_model=AuthTokenResponse)
def refresh_access_token(
    body: RefreshRequest,
    settings: Settings = Depends(get_settings),
    db: Session = Depends(get_db),
) -> AuthTokenResponse:
    _require_jwt_configuration(settings)
    refresh_row = db.execute(
        select(RefreshToken)
        .where(RefreshToken.token_hash == hash_opaque_token(body.refresh_token))
        .with_for_update()
    ).scalar_one_or_none()

    now = datetime.now(timezone.utc)
    if (
        refresh_row is None
        or refresh_row.revoked_at is not None
        or _as_utc(refresh_row.expires_at) <= now
        or not refresh_row.user.is_active
    ):
        raise HTTPException(status_code=401, detail="Invalid refresh token")

    claim = db.execute(
        update(RefreshToken)
        .where(
            RefreshToken.id == refresh_row.id,
            RefreshToken.revoked_at.is_(None),
        )
        .values(last_used_at=now, revoked_at=now)
    )
    if claim.rowcount != 1:
        db.rollback()
        raise HTTPException(status_code=401, detail="Invalid refresh token")

    raw_refresh, new_row = _new_refresh_token(refresh_row.user, settings, now)
    db.add(new_row)
    result = _token_response(refresh_row.user, raw_refresh, settings)
    db.commit()
    return result


@router.post("/logout", response_model=LogoutResponse)
def logout(
    body: RefreshRequest,
    db: Session = Depends(get_db),
) -> LogoutResponse:
    refresh_row = db.execute(
        select(RefreshToken).where(
            RefreshToken.token_hash == hash_opaque_token(body.refresh_token)
        )
    ).scalar_one_or_none()
    if refresh_row is not None and refresh_row.revoked_at is None:
        refresh_row.revoked_at = datetime.now(timezone.utc)
        db.commit()
    return LogoutResponse()


def get_current_user(
    credentials: HTTPAuthorizationCredentials | None = Depends(bearer_scheme),
    settings: Settings = Depends(get_settings),
    db: Session = Depends(get_db),
) -> User:
    if credentials is None or credentials.scheme.lower() != "bearer":
        raise HTTPException(
            status_code=401,
            detail="Unauthorized",
            headers={"WWW-Authenticate": "Bearer"},
        )
    try:
        user_id = decode_access_token(credentials.credentials, settings)
    except AuthConfigurationError as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc
    except InvalidAccessToken as exc:
        raise HTTPException(
            status_code=401,
            detail="Unauthorized",
            headers={"WWW-Authenticate": "Bearer"},
        ) from exc

    user = db.get(User, user_id)
    if user is None or not user.is_active:
        raise HTTPException(status_code=401, detail="Unauthorized")
    return user


@router.get("/me", response_model=UserOut)
def get_me(user: User = Depends(get_current_user)) -> UserOut:
    return _user_out(user)

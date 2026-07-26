from __future__ import annotations

from typing import Literal

from pydantic import BaseModel, Field


class HealthResponse(BaseModel):
    status: Literal["ok"] = "ok"
    service: Literal["mk3-movebook-server"] = "mk3-movebook-server"
    api_version: Literal["v1"] = "v1"


class DeviceSessionCreateResponse(BaseModel):
    device_code: str
    verification_uri: str
    expires_in: int
    interval: int


class DeviceTokenRequest(BaseModel):
    device_code: str = Field(min_length=32, max_length=256)


class DeviceSessionStatusResponse(BaseModel):
    status: Literal["pending"] = "pending"
    interval: int


class UserOut(BaseModel):
    id: int
    display_name: str | None
    avatar_url: str | None
    providers: list[str]


class AuthTokenResponse(BaseModel):
    status: Literal["authorized"] = "authorized"
    access_token: str
    refresh_token: str
    token_type: Literal["bearer"] = "bearer"
    expires_in: int
    user: UserOut


class RefreshRequest(BaseModel):
    refresh_token: str = Field(min_length=32, max_length=256)


class LogoutResponse(BaseModel):
    ok: Literal[True] = True

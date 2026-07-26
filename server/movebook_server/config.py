from __future__ import annotations

from functools import lru_cache

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Server settings loaded only from MOVEBOOK_* variables."""

    app_name: str = "MK3 MoveBook Server"
    database_url: str = "sqlite:///./movebook_dev.db"
    sql_echo: bool = False
    public_base_url: str = "http://127.0.0.1:8010"

    discord_client_id: str = ""
    discord_client_secret: str = ""
    discord_redirect_uri: str = (
        "http://127.0.0.1:8010/api/v1/auth/discord/callback"
    )

    jwt_secret: str = ""
    access_ttl_min: int = Field(default=15, ge=1, le=1440)
    refresh_ttl_days: int = Field(default=30, ge=1, le=365)
    login_ttl_seconds: int = Field(default=300, ge=60, le=900)
    login_poll_interval_seconds: int = Field(default=2, ge=1, le=10)

    model_config = SettingsConfigDict(
        env_file=".env",
        env_prefix="MOVEBOOK_",
        extra="ignore",
    )

    def discord_is_configured(self) -> bool:
        values = (self.discord_client_id, self.discord_client_secret)
        return all(value and not value.startswith("replace_") for value in values)

    def jwt_is_configured(self) -> bool:
        return len(self.jwt_secret) >= 32 and not self.jwt_secret.startswith(
            "replace_"
        )


@lru_cache
def get_settings() -> Settings:
    return Settings()

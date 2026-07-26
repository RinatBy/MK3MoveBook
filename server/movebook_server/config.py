from __future__ import annotations

from functools import lru_cache

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    """Server settings loaded only from MOVEBOOK_* variables."""

    app_name: str = "MK3 MoveBook Server"
    database_url: str = "sqlite:///./movebook_dev.db"
    sql_echo: bool = False

    model_config = SettingsConfigDict(
        env_file=".env",
        env_prefix="MOVEBOOK_",
        extra="ignore",
    )


@lru_cache
def get_settings() -> Settings:
    return Settings()

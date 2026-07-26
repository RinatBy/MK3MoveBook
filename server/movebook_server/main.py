from __future__ import annotations

from fastapi import APIRouter, FastAPI

from . import __version__
from .config import get_settings


def create_app() -> FastAPI:
    settings = get_settings()
    app = FastAPI(
        title=settings.app_name,
        version=__version__,
    )
    api = APIRouter(prefix="/api/v1")

    @api.get("/health", tags=["system"])
    def health() -> dict[str, str]:
        return {
            "status": "ok",
            "service": "mk3-movebook-server",
            "api_version": "v1",
        }

    app.include_router(api)
    return app


app = create_app()

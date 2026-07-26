from __future__ import annotations

from fastapi import APIRouter, FastAPI

from . import __version__
from .auth_api import router as auth_router
from .config import get_settings
from .schemas import HealthResponse


def create_app() -> FastAPI:
    settings = get_settings()
    app = FastAPI(
        title=settings.app_name,
        version=__version__,
    )
    api = APIRouter(prefix="/api/v1")

    @api.get("/health", tags=["system"], response_model=HealthResponse)
    def health() -> HealthResponse:
        return HealthResponse()

    api.include_router(auth_router)
    app.include_router(api)
    return app


app = create_app()

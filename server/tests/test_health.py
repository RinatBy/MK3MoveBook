from __future__ import annotations

import asyncio

import httpx
from movebook_server.main import create_app


async def request_health() -> httpx.Response:
    transport = httpx.ASGITransport(app=create_app())
    async with httpx.AsyncClient(
        transport=transport,
        base_url="http://testserver",
    ) as client:
        return await client.get("/api/v1/health")


def test_health_endpoint() -> None:
    response = asyncio.run(request_health())
    assert response.status_code == 200
    assert response.json() == {
        "status": "ok",
        "service": "mk3-movebook-server",
        "api_version": "v1",
    }

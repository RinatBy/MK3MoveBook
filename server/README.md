# MK3 MoveBook Server

Отдельный FastAPI-сервис для будущих аккаунтов и серверных достижений
MK3 MoveBook. Он не использует базу, конфигурацию или секреты OriLauncher.

Первый checkpoint содержит только:

- конфигурацию с префиксом `MOVEBOOK_`;
- отдельную SQLite-базу для локальной разработки и PostgreSQL URL для
  будущего развёртывания;
- endpoint `GET /api/v1/health`;
- модели пользователей, внешних идентификаторов, refresh-токенов, событий и
  разблокировок достижений, статистики;
- начальную Alembic-миграцию и тесты ограничений схемы.

OAuth, JWT, синхронизация достижений и изменения Portable-клиента в этот этап
не входят.

## Локальная настройка

```powershell
cd server
python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements-dev.txt
Copy-Item .env.example .env
alembic upgrade head
uvicorn movebook_server.main:app --host 127.0.0.1 --port 8010
```

Проверка сервера:

```powershell
Invoke-RestMethod http://127.0.0.1:8010/api/v1/health
```

Запуск тестов:

```powershell
pytest
```

Файлы `.env`, SQLite и виртуального окружения исключены из Git. Перед будущим
production-развёртыванием нужно отдельно настроить PostgreSQL и выполнить
`alembic upgrade head`; автоматического `create_all` при старте приложения нет.

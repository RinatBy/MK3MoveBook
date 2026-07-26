# MK3 MoveBook Server

Отдельный FastAPI-сервис для будущих аккаунтов и серверных достижений
MK3 MoveBook. Он не использует базу, конфигурацию или секреты OriLauncher.

Текущий серверный каркас содержит:

- конфигурацию с префиксом `MOVEBOOK_`;
- отдельную SQLite-базу для локальной разработки и PostgreSQL URL для
  будущего развёртывания;
- endpoint `GET /api/v1/health`;
- модели пользователей, внешних идентификаторов, refresh-токенов, событий и
  разблокировок достижений, статистики;
- Alembic-миграции и тесты ограничений схемы;
- серверный Discord Authorization Code flow через системный браузер;
- одноразовые device login-сессии, короткие access JWT и ротацию refresh-токенов.

Синхронизация достижений, определение стороны аккаунта P1/P2 и изменения
Portable-клиента пока не входят.

## Локальная настройка

```powershell
cd server
python -m venv .venv
.\.venv\Scripts\Activate.ps1
python -m pip install -r requirements-dev.txt
Copy-Item .env.example .env
alembic upgrade head
uvicorn movebook_server.main:app --host 127.0.0.1 --port 8010 --no-access-log
```

Проверка сервера:

```powershell
Invoke-RestMethod http://127.0.0.1:8010/api/v1/health
```

Запуск тестов:

```powershell
pytest
```

## Локальная проверка Discord

1. Создайте приложение в Discord Developer Portal.
2. В OAuth2 Redirects добавьте точный адрес:
   `http://127.0.0.1:8010/api/v1/auth/discord/callback`.
3. В локальном `.env` замените фиктивные значения:

```dotenv
MOVEBOOK_DISCORD_CLIENT_ID=<client id>
MOVEBOOK_DISCORD_CLIENT_SECRET=<client secret>
MOVEBOOK_JWT_SECRET=<случайная строка не короче 32 символов>
```

Случайный JWT secret можно создать локально:

```powershell
.\.venv\Scripts\python.exe -c "import secrets; print(secrets.token_urlsafe(48))"
```

После `alembic upgrade head` и запуска uvicorn выполните во втором PowerShell:

```powershell
.\tools\test-discord-login.ps1
```

Скрипт создаёт короткую login-сессию, открывает Discord в системном браузере,
дожидается callback, проверяет `/auth/me` и отзывает выданный refresh-токен. Он
не печатает токены и не сохраняет их в файл.

Файлы `.env`, SQLite и виртуального окружения исключены из Git. Перед будущим
production-развёртыванием нужно отдельно настроить PostgreSQL и выполнить
`alembic upgrade head`; автоматического `create_all` при старте приложения нет.

# MK3 MoveBook Server

Отдельный FastAPI-сервис аккаунтов и серверных достижений MK3 MoveBook. Он не
использует базу, конфигурацию или секреты OriLauncher.

Сервис содержит:

- конфигурацию с префиксом `MOVEBOOK_`;
- отдельную SQLite-базу и настраиваемый SQLAlchemy URL;
- endpoint `GET /api/v1/health`;
- модели пользователей, внешних идентификаторов, refresh-токенов, событий и
  разблокировок достижений, статистики;
- Alembic-миграции и тесты ограничений схемы;
- серверный Discord Authorization Code flow через системный браузер;
- одноразовые device login-сессии, короткие access JWT и ротацию refresh-токенов.

Сервис принимает события MAME от авторизованного Portable-клиента, вычисляет
достижения и возвращает серверный снимок прогресса. Серверный контракт всегда
представляет владельца профиля как P1. Допустимы только полностью наблюдаемые
матчи в башне против CPU с подтверждённо выключенными читами и сетевые матчи
Kaillera ровно на двух участников при выборе одного бойца на каждую сторону.
Локальный Versus, внутриигровые команды 2×2/3×3, Team Edition на 3–6 игроков и
неизвестные режимы сервер принимает только как идемпотентные события без
изменения профиля. Статистика допустимых режимов хранится раздельно, а
победа над Shao Kahn в каждой из четырёх башен открывает отдельное достижение.
Подробности описаны в
[`docs/achievement-sync.md`](docs/achievement-sync.md).

Production API развёрнут отдельно и доступен по адресу
`https://api.movebook.ru`. Локальный сервер нужен только для разработки и
проверки изменений backend.

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

## Очистка прогресса тестового профиля

Перед очисткой сделайте резервную копию БД. Утилита требует выбрать ровно один
аккаунт и по умолчанию только показывает объём очистки:

```powershell
.\.venv\Scripts\python.exe .\tools\reset_achievement_test_progress.py --user-id 123
.\.venv\Scripts\python.exe .\tools\reset_achievement_test_progress.py --discord-subject 1234567890
```

После проверки выбранного тестового аккаунта повторите ту же команду с
`--apply`. Будут удалены только его события, разблокировки и агрегированная
статистика достижений; сам аккаунт, Discord-привязка и токены не удаляются.

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

Файлы `.env`, SQLite и виртуального окружения исключены из Git. Перед запуском
новой версии нужно выполнить `alembic upgrade head`; автоматического
`create_all` при старте приложения нет.

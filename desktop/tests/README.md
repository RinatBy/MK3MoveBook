# Тесты диагностического моста

Запуск проверок desktop-логики:

```powershell
powershell -ExecutionPolicy Bypass -File .\run-tests.ps1
```

Тесты проверяют Discord device login-контракт клиента, ротацию refresh-токена,
защищённое Windows-хранилище постоянной сессии, отказ от небезопасного
удалённого HTTP, а также Fatality, Animality, Friendship, Babality, Brutality,
Stage Fatality и отдельный ROM-зависимый сигнал Antifatality Kabal для обеих
сторон игрока.

## Four-ROM compatibility and capture replay

Run from E:/monorepo/MK3MoveBook/app:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File desktop/tests/run-tests.ps1
node web/tests/achievement-engine.test.mjs
server/.venv/Scripts/python.exe -m pytest server/tests -c server/pytest.ini --basetemp=../work/pytest-achievements-local -p no:cacheprovider
```

Use a new, disposable basetemp directory for each server run. These tests use
temporary SQLite databases and ASGI clients, without contacting the deployed API.

MameRomCompatibilityTests exercises all four ROM titles through MameBridge's
snapshot construction, fight/mode/cheat tracking, profile attribution and the
outbox policy. It covers both CPU sides and both online positions, partial
matches, repeated phase samples, the negative eligibility matrix, ROM-specific
finishers and team diagnostics.

fixtures/umk3tm60-cpu-test17.csv is a sanitized real capture, not fabricated RAM.
Its replay expects 23 eligible CPU match completions and one cheat-blocked match.
The report suppresses tower metadata outside tower context. The replay supplies
-1/0 for these absent fields. It feeds recorded cheat observations into the
monitor without starting a worker.

fixtures/umk3uk2-cpu-test18.csv contains 300 real UK 60 FPS samples. Its original
scanning/count=0 monitor input rejects the completed CPU match. Replaying the
same game stream with an independently verified empty-engine input admits it.
This does not substitute for a live Test 19 win and server response.

MameCheatEngineReaderTests runs the production reader on controlled memory for
two exact executable profiles and four ROMs. It covers live empty catalogs,
code/ownership validation, incomplete reads, pause/exit/reset, frozen counters,
ROM changes and nonempty catalogs. CPU-policy and bridge tests cover the new
ready-empty state without relaxing cheat latches, partial matches or outbox.
See ../research/findings/2026-09-07-empty-cheat-catalog.md for live evidence.

MoveBookAchievementDiagnosticReport is also checked for opt-in behavior,
deduplication, exclusion reasons, server counters and absence of URL credentials.
See ../research/findings/2026-09-05-60fps-achievement-path.md for provenance
and the remaining manual work.

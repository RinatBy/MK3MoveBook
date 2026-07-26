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

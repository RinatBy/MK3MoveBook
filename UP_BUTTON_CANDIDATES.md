# Кандидаты на проверку HOLD BL для приёмов с `↑`

Источник: `web/data/moves.js` и `web/data/sega-moves.js`.

Правило отбора: только `Special Moves` и `Finishing Moves`, только комбинации, где в `notation` есть `_8` (`↑`). `Autokombos` не трогались.

Если платформа не указана, комбинация одинакова для Arcade и SEGA. Платформа отмечена только там, где запись относится к одной версии игры.

## Проверено — HOLD BL не нужен

- Human Smoke — Finishing Moves — Stage Fatality: `→ › ↑ › ↑ › LP`
- Kano — Finishing Moves — Stage Fatality: `↑ › ↑ › ← › LK`
- Reptile — Arcade — Special Moves — Invisibility: `↑ › ↓ › HK`
- Reptile — Arcade — Finishing Moves — Animality (close): `↓ › ↓ › ↓ › ↑ › HK`
- Reptile — Finishing Moves — Fatality 2 (sweep): `→ › → › ↑ › ↑ › HK`
- Ermac — Finishing Moves — Fatality 1 (sweep): `↓ › ↑ › ↓ › ↓ › ↓ › BL`
- Nightwolf — Finishing Moves — Fatality 1 (close): `↑ › ↑ › ← › → › BL`
- Robot Smoke — Special Moves — Invisibility: `↑ › ↑ › RUN`
- Robot Smoke — Finishing Moves — Fatality 1 (far): `↑ › ↑ › → › ↓`
- Sindel — Arcade — Finishing Moves — Animality: `→ › → › ↑ › HP`
- Sindel — Finishing Moves — Babality: `RUN › RUN › RUN › ↑`
- Sindel — Finishing Moves — Friendship: `RUN › RUN › RUN › RUN › RUN › ↑`
- Stryker — Finishing Moves — Stage Fatality: `→ › ↑ › ↑ › HK`
- Scorpion — Finishing Moves — Fatality 1 (jump): `↓ › ↓ › ↑ › HK`
- Scorpion — Finishing Moves — Fatality 2 (close): `→ › → › ↓ › ↑ › RUN`
- Unmasked Sub-Zero — Finishing Moves — Friendship (sweep): `LK › RUN › RUN › ↑`
- Liu Kang — SEGA — Special Moves — Fireball-high (also in air): `↑ › → › → › HP`
- Liu Kang — Arcade — Finishing Moves — Animality (sweep): `↓ › ↓ › ↑`
- Liu Kang — Finishing Moves — Fatality 2: `↑ › ↓ › ↑ › ↑ › BL+RUN`
- Sektor — Arcade — Finishing Moves — Animality (close): `→ › → › ↓ › ↑`
- Cyrax — Arcade — Finishing Moves — Animality (close): `↑ › ↑ › ↓ › ↓`
- Cyrax — Finishing Moves — Fatality 1: `↓ › ↓ › ↑ › ↓ › HP`
- Cyrax — Finishing Moves — Fatality 2 (close): `↓ › ↓ › → › ↑ › RUN`
- Cyrax — Finishing Moves — Friendship: `RUN › RUN › RUN › ↑`
- Shang Tsung — Finishing Moves — Stage Fatality: `↑ › ↑ › ← › LP`
- Sheeva — Arcade — Special Moves — Teleport Stomp: `↓ › ↑`
- Noob Saibot — Special Moves — Teleport: `↓ › ↑`
- Motaro — Special Moves — Teleport: `↓ › ↑`

## Подтверждено — HOLD BL добавлен в данные

- Scorpion — Arcade — Finishing Moves — Animality (close): `HOLD BL › → › ↑ › ↑ › RELEASE BL › HK`
  - Важно: отпустить `BL` перед `HK`.
- Scorpion — Finishing Moves — Stage Fatality: `HOLD BL › → › ↑ › ↑ › RELEASE BL › LP`
  - Важно: оба нажатия `↑` выполнить с зажатым `BL`, затем отпустить `BL` перед `LP`.
- Unmasked Sub-Zero — Arcade — Finishing Moves — Animality (close): `HOLD BL › → › ↑ › ↑ › RELEASE BL`
  - Важно: оба нажатия `↑` выполнить с зажатым `BL`, затем отпустить `BL`.
- Jade — Finishing Moves — Fatality 1 (close): `HOLD BL › ↑ › ↑ › ↓ › → › RELEASE BL › HP`
  - Важно: оба нажатия `↑` выполнить с зажатым `BL`, затем отпустить `BL` перед `HP`.

## Уже есть HOLD BL / HOLD BL+RUN — для контроля

- Reptile — SEGA — Special Moves — Invisibility: `HOLD BL › ↑ › ↓ › HK`
- Robot Smoke — Finishing Moves — Fatality 2 (sweep): `HOLD BL+RUN › ↓ › ↓ › → › ↑`
- Jax — Finishing Moves — Fatality 1 (close): `HOLD BL › ↑ › ↓ › → › ↑`
- Sonya — Finishing Moves — Fatality 1 (far): `HOLD BL+RUN › ↑ › ↑ › ← › ↓`

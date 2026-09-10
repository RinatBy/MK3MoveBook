# UK 60 FPS: пустой движок читов, Test 19

10 сентября пользователь подтвердил регистрацию достижений в Test 19.
Логи подтверждают полный путь UK 60 FPS CPU: ready-empty -> verified-clean ->
queued -> server-response accepted=true, дебют Shang Tsung, CPU-победы 9 -> 10.
Этот CPU-блокер закрыт. Полная регрессионная матрица релиза ещё не закрыта.

## Наблюдения

Test 18, diagnostics/achievement-path-20260905-162054-569fef03.jsonl:
- umk3tm60: match-ended принят сервером; fighter-debut-sektor выдан;
  computer_victories вырос до 9, matches до 10, pending=0.
- umk3uk2: полный CPU-матч распознан (в CSV P1=8/Cyrax, P2=18/Scorpion),
  round-ended и match-ended пропущены с cpu-cheats-not-verified-clean.
  До очереди и отправки событие не дошло.

Test 18, mame-fight-report-20260905-192054-P26812.csv:
300 игровых сэмплов UK 60 FPS; RAM 0x09ADF038, фазы 0/1/3, счёт 0/1/2,
233 сэмпла tower-cpu/checking. Монитор во всех 300 сэмплах scanning/count=0.

В обеих установках cheat.zip содержит umk3tm.xml, umk3tm60.xml, umk3uk.xml,
но не umk3uk2.xml. Это объясняет отсутствие каталога, однако сам файл или
его отсутствие не используются как доказательство выключенных читов.

7 сентября пользователь открыл UK 60 FPS для узкого чтения. PID 19100:
running_machine=0x0631C810, cheat_data=0x06DC8BB0, cheatlist=0;
счётчик cheat_frame вырос с 8534 до 8674 за девять интервалов по 250 мс.
Новый MameCheatEngineReader вернул EmptyVerified=false на первом чтении,
затем true на следующих девяти. После выхода из ROM интегрированный
монитор вернул scanning, CPU-политика checking.
Логи: work/umk3-60fps-20260907/live-uk60-engine.txt и live-uk60-monitor.txt.
Второй лог снят уже после выхода, он не доказывает успех полного матча.

## Происхождение адресов

Использованы исходники MAME 0.129 и дизассемблирование конкретных EXE:
- https://raw.githubusercontent.com/mamedev/mame/mame0129/src/emu/cheat.c
- https://raw.githubusercontent.com/mamedev/mame/mame0129/src/emu/mame.h
- https://raw.githubusercontent.com/mamedev/mame/mame0129/src/emu/mame.c

cheat_init загружает XML по machine->basename и записывает cheat_data.
cheat_frame обходит cheatlist, выполняет RUN-скрипты и увеличивает framecount,
даже если список пуст. Соответствующий x86-код проверен в обеих сборках.

| EXE SHA256 | Machine (VA) | cheat_frame (VA) | запись Machine (VA) |
| --- | --- | --- | --- |
| 194fa64e618774f301deff80c2cb6ec67e3e1b45f4ecd335e02a66643c11e5f6 | 0x21ED710 | 0xA65FB0 | 0x8C9376 |
| 0d0dd7eeb81f289e49c7aff6e7bc79bbed05726a13554224ee090a79c27fdcce | 0x21E83F0 | 0xA61690 | 0x8C5A9F |

Первый EXE — E:/games/MAME/MAME 0.129/mameppkgui.exe, второй —
runtimes/mame-0.129/mameppkgui.exe. База обоих PE 0x400000, relocations stripped.
reader отказывает при иной базе/хэше, проверяет кодовые сигнатуры в памяти.
Скрипты анализа и engine-profiles.json лежат в work/umk3-60fps-20260907.

Подтверждённые смещения running_machine: basename=0x2C, mame_data=0xD4,
cheat_data=0xFC. В cheat_private: head=0, UINT64 framecount=8, output[50]=0x10,
numlines=0x10A. Это внутренние структуры эмулятора, не новые адреса ROM RAM.

## Исправление и границы

MameCheatEngineReader требует работающую, не приостановленную игру без
pending reset/exit, один из четырёх ROM, корректное владение структурой,
нулевой head, отсутствие script output и два согласованных чтения с ростом
счётчика. Подтверждение истекает через 500 мс без нового кадра. Повторные
чтения указателей после данных исключают смену владельца во время чтения.
Ошибка, смена ROM/списка/движка или сброс счётчика требуют нового подтверждения.

MameCheatMonitor публикует ready-empty только по этому доказательству.
CPU-политика дополнительно сверяет ROM, нулевые размеры и сохраняет защёлку
использования читов. Простой count=0/scanning/ready ничего не разрешает.

Для известных EXE непустой список читается через фактический head текущей игры,
что исключает найденный сканером остаток предыдущего ROM. Дальше действуют
прежние проверки 23 читов, активных состояний и разовых команд.
Другие EXE сохраняют прежний сканер и не могут подтверждать пустой движок.
Отключённый/неинициализированный cheat_data=NULL также остаётся checking.

Изменений файлов MAME, cheat.zip, INI или памяти процесса нет. Игровые адреса,
частота опроса боя, правила режимов, outbox и блокировка категорий не ослаблены.

## Проверки

- Все desktop-наборы проходят; 100 проверок reader для обоих EXE и четырёх ROM,
  ошибок/неполных чтений, повреждённых сигнатур, паузы/выхода, нулевого
  указателя, замершего/сброшенного счётчика и смены владельца.
- CPU policy проверяет ready-empty, привязку ROM, противоречивые размеры,
  сохранение блокировки читов и неизменность online.
- 8410 проверок совместимости. Обезличенная fixture umk3uk2-cpu-test18.csv
  содержит исходные 300 сэмплов. С записанным scanning матч отклоняется;
  замена только входа монитора на отдельно подтверждённый ready-empty
  допускает тот же игровой поток в outbox. Это проверка исправления gate,
  а не запись вручную сыгранного успешного Test 19.
- Server: 60 passed, локальные ASGI/SQLite. Web: все сценарии прошли.
- Test 19 собран из app, web 1.1.17, 91 web-файл совпадает по SHA256.

## Ручная проверка Test 19, подтверждена 10 сентября

Локальный achievement-path-20260910-131849-3127a17e.jsonl:
13:20:58.184 UTC, umk3uk2, match-ended, computer, fully_observed=true,
cheat_policy=disabled, winner=p1, accepted=true, fighter-debut-shang-tsung.
Счётчики computer_victories 9 -> 10, matches 14 -> 15, pending=0.
Все четыре отправленных события приняты, ошибок отправки нет.
CSV mame-fight-report-20260910-161849-P25540.csv: 507 игровых сэмплов,
из них 441 tower-cpu/ready-empty/verified-clean и 66 unknown/not-applicable.
Игра в неизвестном режиме не получила допуска по пустому списку читов.

Также разобраны логи 7 сентября, ранее не проверенные:
achievement-path-20260907-160539-fd8558e9.jsonl содержит четыре завершённых
online 1x1 матча UK 60 FPS: два выигрыша, два проигрыша. Во всех событиях
netplay_players=2, team_size=1; десять отправленных событий приняты сервером.
Online-победы 0 -> 2; выданы дебюты Sonya и Cyrax. Проверка читов для online
остаётся not-applicable, как и предусмотрено политикой.
В том же логе umk3tm60 с тремя участниками: 35 событий пропущены по
netplay-not-two-players, ни одного queued. Это подтверждает запрет online 3,
но не подменяет ещё не пройденные случаи 4–6 и команд 2x2/3x3.

Все файлы BUILD-MANIFEST.json Test 19 и SHA256 EXE совпадают со сборкой
7 сентября. Сводка и резервные копии документации: work/umk3-60fps-20260910.
Исходные логи и кандидат не изменялись. unlock_count=14 не означает 14/14
дебютов: счётчик включает награды других типов. Gate категорий этим прогоном
не считается проверенным; его прежние автотесты сохраняются.

Следующий шаг: контроль остальных ROM, активных/разовых читов и оставшихся
режимов ручной матрицы. portable-current и публикация требуют отдельной команды.

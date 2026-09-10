window.MOVEBOOK_ACHIEVEMENTS = {
    preview: false,
    general: [
        {
            id: "victories-5",
            icon: "assets/achievements/victories-5.png",
            title: "Первые пять",
            description: "Одержать 5 побед в завершённых матчах.",
            label: "5 побед",
            condition: { type: "victories", count: 5 }
        },
        {
            id: "victories-10",
            icon: "assets/achievements/victories-10.png",
            title: "Первая десятка",
            description: "Одержать 10 побед в завершённых матчах.",
            label: "10 побед",
            condition: { type: "victories", count: 10 }
        },
        {
            id: "victories-25",
            icon: "assets/achievements/victories-25.png",
            title: "Боец",
            description: "Одержать 25 побед в завершённых матчах.",
            label: "25 побед",
            condition: { type: "victories", count: 25 }
        },
        {
            id: "victories-50",
            icon: "assets/achievements/50.png",
            title: "Закалённый боец",
            description: "Одержать 50 побед в завершённых матчах.",
            label: "50 побед",
            condition: { type: "victories", count: 50 }
        },
        {
            id: "victories-75",
            icon: "assets/achievements/75.png",
            title: "Опытный боец",
            description: "Одержать 75 побед в завершённых матчах.",
            label: "75 побед",
            condition: { type: "victories", count: 75 }
        },
        {
            id: "victories-100",
            icon: "assets/achievements/100.png",
            title: "Воин",
            description: "Одержать 100 побед в завершённых матчах.",
            label: "100 побед",
            condition: { type: "victories", count: 100 }
        },
        {
            id: "victories-250",
            icon: "assets/achievements/250.png",
            title: "Опытный воин",
            description: "Одержать 250 побед в завершённых матчах.",
            label: "250 побед",
            condition: { type: "victories", count: 250 }
        },
        {
            id: "victories-500",
            icon: "assets/achievements/500.png",
            title: "Мастер",
            description: "Одержать 500 побед в завершённых матчах.",
            label: "500 побед",
            condition: { type: "victories", count: 500 }
        },
        {
            id: "victories-750",
            icon: "assets/achievements/750.png",
            title: "Легендарный мастер",
            description: "Одержать 750 побед в завершённых матчах.",
            label: "750 побед",
            condition: { type: "victories", count: 750 }
        },
        {
            id: "victories-1000",
            icon: "assets/achievements/1000.png",
            title: "Легенда арены",
            description: "Одержать 1000 побед в завершённых матчах.",
            label: "1000 побед",
            condition: { type: "victories", count: 1000 }
        }
    ],
    computer: [
        {
            id: "tower-novice-complete",
            icon: "assets/achievements/novice.png",
            title: "Первое восхождение",
            description: "Пройти башню NOVICE.",
            label: "Башня NOVICE"
        },
        {
            id: "tower-warrior-complete",
            icon: "assets/achievements/warrior.png",
            title: "Путь воина",
            description: "Пройти башню WARRIOR.",
            label: "Башня WARRIOR"
        },
        {
            id: "tower-master-complete",
            icon: "assets/achievements/master_03.jpg",
            title: "Испытание мастера",
            description: "Пройти третью башню MASTER.",
            label: "Башня MASTER"
        },
        {
            id: "tower-master-fourth-complete",
            icon: "assets/achievements/master_04.jpg",
            title: "Мастер",
            description: "Пройти четвёртую башню MASTER.",
            label: "4-я башня MASTER"
        }
    ],
    fighters: [
        {
            id: "fighter-debut-nightwolf",
            icon: "assets/achievements/debutnightwolf.png",
            fighterId: "nightwolf",
            title: "Хранитель священной земли",
            description: "Одержать победу, играя за Nightwolf.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-sindel",
            icon: "assets/achievements/debutSindel.png",
            fighterId: "sindel",
            title: "Пробуждение королевы",
            description: "Одержать победу, играя за Sindel.",
            label: "Боевой дебют",
            loreNote: "Перед событиями MK3 Шао Кан возвращает Синдел к жизни, чтобы открыть путь для вторжения в Земное царство."
        },
        {
            id: "fighter-debut-stryker",
            icon: "assets/achievements/debutStryker.png",
            fighterId: "stryker",
            title: "Вы арестованы",
            description: "Одержать победу, играя за Stryker.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-liu-kang",
            icon: "assets/achievements/debutLiuKang.png",
            fighterId: "liu-kang",
            title: "Путь дракона",
            description: "Одержать победу, играя за Liu Kang.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-jax",
            icon: "assets/achievements/debutJax.png",
            fighterId: "jax",
            title: "Сила правосудия",
            description: "Одержать победу, играя за Jax.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-kabal",
            icon: "assets/achievements/debutkabal.png",
            fighterId: "kabal",
            title: "На предельной скорости",
            description: "Одержать победу, играя за Kabal.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-sonya",
            icon: "assets/achievements/debutSonya.png",
            fighterId: "sonya",
            title: "Агент спецназа",
            description: "Одержать победу, играя за Sonya.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-shang-tsung",
            icon: "assets/achievements/debutstsung.png",
            fighterId: "shang-tsung",
            title: "Похититель душ",
            description: "Одержать победу, играя за Shang Tsung.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-kano",
            icon: "assets/achievements/debutkano.png",
            fighterId: "kano",
            title: "Чёрный Дракон",
            description: "Одержать победу, играя за Kano.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-unmasked-sub-zero",
            icon: "assets/achievements/debutunmsub.png",
            fighterId: "unmasked-sub-zero",
            title: "Холодный расчёт",
            description: "Одержать победу, играя за Unmasked Sub-Zero.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-kung-lao",
            icon: "assets/achievements/debutkunglao.png",
            fighterId: "kung-lao",
            title: "Острый край",
            description: "Одержать победу, играя за Kung Lao.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-sektor",
            icon: "assets/achievements/debutsector.png",
            fighterId: "sektor",
            title: "Протокол уничтожения",
            description: "Одержать победу, играя за Sektor.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-cyrax",
            icon: "assets/achievements/debutcyrax.png",
            fighterId: "cyrax",
            title: "Пойманный в сети",
            description: "Одержать победу, играя за Cyrax.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-robot-smoke",
            icon: "assets/achievements/debutsmoke.png",
            fighterId: "robot-smoke",
            title: "Дымовая завеса",
            description: "Одержать победу, играя за Robot Smoke.",
            label: "Боевой дебют"
        }
    ],
    lore: [
        {
            id: "old-scores",
            icon: "assets/achievements/sonyavskano.png",
            fighterId: "sonya",
            opponentId: "kano",
            title: "Старые счёты",
            description: "Победить Кано, играя за Соню.",
            label: "Противостояние",
            loreNote: "Соня преследует Кано и преступную организацию Black Dragon ещё со времён первого Mortal Kombat."
        },
        {
            id: "free-will",
            icon: "assets/achievements/unmaskedsubzerovssektor.png",
            fighterId: "unmasked-sub-zero",
            opponentId: "sektor",
            title: "Свободная воля",
            description: "Победить Sektor, играя за Unmasked Sub-Zero.",
            label: "Противостояние",
            loreNote: "Саб-Зиро покинул Lin Kuei и стал целью кибернетических воинов клана, включая Сектора."
        },
        {
            id: "true-heir",
            icon: "assets/achievements/kitanavs,ileena.png",
            fighterId: "kitana",
            opponentId: "mileena",
            title: "Кровные узы",
            description: "Победить Mileena, играя за Kitana.",
            label: "Противостояние",
            loreNote: "Милина была создана как искажённый двойник Китаны и претендентка на её место при дворе Шао Кана."
        },
        {
            id: "leave-black-dragon",
            icon: "assets/achievements/kabalvskano.png",
            fighterId: "kabal",
            opponentId: "kano",
            title: "Порвать с Black Dragon",
            description: "Победить Kano, играя за Kabal.",
            label: "Противостояние",
            loreNote: "Кабал и Кано состояли в Black Dragon, но в концовке MK3 Кабал выбирает борьбу с преступностью."
        },
        {
            id: "fire-and-ice",
            icon: "assets/achievements/Fire and Ice.png",
            fighterId: "scorpion",
            opponentId: "unmasked-sub-zero",
            title: "Огонь и Лед",
            description: "Победить Unmasked Sub-Zero, играя за Scorpion.",
            label: "Противостояние"
        },
        {
            id: "earthrealm-defender",
            icon: "assets/achievements/luikangvsshangtsung.png",
            fighterId: "liu-kang",
            opponentId: "shang-tsung",
            title: "Защитник Земли",
            description: "Победить Shang Tsung, играя за Liu Kang.",
            label: "Противостояние"
        }
    ],
    special: [
        {
            id: "flawless-victory",
            title: "Без единой царапины",
            description: "Завершить раунд с полным запасом здоровья.",
            label: "Flawless Victory",
            condition: { type: "flawless-round", fullHealth: 166 }
        },
        {
            id: "last-strength",
            title: "Из последних сил",
            description: "В первом раунде войти в состояние Danger и всё равно победить.",
            label: "Победа с критическим HP"
        },
        {
            id: "comeback",
            title: "Камбэчная",
            description: "Проиграв первый раунд, во втором войти в Danger и сравнять счёт.",
            label: "Камбэк"
        },
        {
            id: "frosty",
            icon: "assets/achievements/Frosty.png",
            title: "Frosty!",
            description: "Заморозить соперника, когда у него осталось не более 8 HP.",
            label: "Секретное событие"
        },
        {
            id: "toasty",
            icon: "assets/achievements/toasty.png",
            title: "Toasty!",
            description: "Вызвать появление Дэна Фордена во время боя.",
            label: "Секретное событие"
        },
        {
            id: "sheeva-42-combo",
            icon: "assets/achievements/42prcnt.png",
            fighterId: "sheeva",
            title: "42%",
            description: "Выполнить за Sheeva комбинацию из 7 ударов на 42% урона.",
            label: "Комбинация Sheeva"
        },
        {
            id: "stage-transition-uppercut",
            title: "Новый уровень",
            description: "Апперкотом отправить соперника на другую арену.",
            label: "Переход между аренами"
        },
        {
            id: "first-fatality",
            title: "Точка поставлена",
            description: "Выполнить Fatality любым бойцом.",
            label: "Fatality",
            condition: { type: "finisher", kind: "fatality" }
        },
        {
            id: "first-animality",
            icon: "assets/achievements/animality.png",
            title: "Звериный инстинкт",
            description: "Выполнить Animality любым бойцом.",
            label: "Animality",
            condition: { type: "finisher", kind: "animality" }
        },
        {
            id: "first-friendship",
            title: "Мирный исход",
            description: "Выполнить Friendship любым бойцом.",
            label: "Friendship",
            condition: { type: "finisher", kind: "friendship" }
        },
        {
            id: "first-babality",
            title: "Снова ребёнок",
            description: "Выполнить Babality любым бойцом.",
            label: "Babality",
            condition: { type: "finisher", kind: "babality" }
        },
        {
            id: "first-stage-fatality",
            title: "Опасная арена",
            description: "Выполнить Stage Fatality любым бойцом.",
            label: "Stage Fatality",
            condition: { type: "finisher", kind: "stage-fatality" }
        },
        {
            id: "jax-brutality",
            fighterId: "jax",
            title: "Стальные кулаки",
            description: "Выполнить Brutality, играя за Jax.",
            label: "Brutality",
            condition: { type: "finisher", kind: "brutality" }
        },
        {
            id: "sheeva-brutality",
            icon: "assets/achievements/sheeva-brutality.png",
            fighterId: "sheeva",
            title: "Quad Damage",
            description: "Выполнить Brutality, играя за Sheeva.",
            label: "Brutality",
            condition: { type: "finisher", kind: "brutality" }
        },
        {
            id: "kabal-antifatality",
            icon: "assets/achievements/antifatality.png",
            fighterId: "kabal",
            title: "Отказ от финала",
            description: "Выполнить Antifatality за Kabal в UMK3 UK [260709].",
            label: "Особое добивание",
            condition: {
                type: "finisher",
                kind: "antifatality",
                romIds: ["umk3uk", "umk3uk2"]
            }
        },
        {
            id: "stryker-friendship",
            fighterId: "stryker",
            title: "Служить и защищать",
            description: "Выполнить Friendship, играя за Stryker.",
            label: "Friendship",
            prerequisites: [
                "first-friendship"
            ],
            condition: { type: "finisher", kind: "friendship" }
        }
    ]
};

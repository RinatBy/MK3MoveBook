window.MOVEBOOK_ACHIEVEMENTS = {
    preview: false,
    general: [
        {
            id: "victories-5",
            title: "Первые пять",
            description: "Одержать 5 побед в завершённых матчах.",
            label: "5 побед",
            condition: { type: "victories", count: 5 }
        },
        {
            id: "victories-10",
            title: "Первая десятка",
            description: "Одержать 10 побед в завершённых матчах.",
            label: "10 побед",
            condition: { type: "victories", count: 10 }
        },
        {
            id: "victories-25",
            title: "Закалённый боец",
            description: "Одержать 25 побед в завершённых матчах.",
            label: "25 побед",
            condition: { type: "victories", count: 25 }
        },
        {
            id: "victories-50",
            title: "Полсотни побед",
            description: "Одержать 50 побед в завершённых матчах.",
            label: "50 побед",
            condition: { type: "victories", count: 50 }
        },
        {
            id: "victories-75",
            title: "Опытный комбатант",
            description: "Одержать 75 побед в завершённых матчах.",
            label: "75 побед",
            condition: { type: "victories", count: 75 }
        },
        {
            id: "victories-100",
            title: "Легенда арены",
            description: "Одержать 100 побед в завершённых матчах.",
            label: "100 побед",
            condition: { type: "victories", count: 100 }
        }
    ],
    fighters: [
        {
            id: "fighter-debut-nightwolf",
            fighterId: "nightwolf",
            title: "Хранитель священной земли",
            description: "Одержать победу, играя за Nightwolf.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-sindel",
            fighterId: "sindel",
            title: "Пробуждение королевы",
            description: "Одержать победу, играя за Sindel.",
            label: "Боевой дебют",
            loreNote: "Перед событиями MK3 Шао Кан возвращает Синдел к жизни, чтобы открыть путь для вторжения в Земное царство."
        },
        {
            id: "fighter-debut-stryker",
            fighterId: "stryker",
            title: "Последний защитник города",
            description: "Одержать победу, играя за Stryker.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-jade",
            fighterId: "jade",
            title: "Боевой посох",
            description: "Одержать победу, играя за Jade.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-liu-kang",
            fighterId: "liu-kang",
            title: "Путь дракона",
            description: "Одержать победу, играя за Liu Kang.",
            label: "Боевой дебют"
        }
    ],
    lore: [
        {
            id: "old-scores",
            fighterId: "sonya",
            opponentId: "kano",
            title: "Старые счёты",
            description: "Победить Кано, играя за Соню.",
            label: "Противостояние",
            loreNote: "Соня преследует Кано и преступную организацию Black Dragon ещё со времён первого Mortal Kombat."
        },
        {
            id: "free-will",
            fighterId: "unmasked-sub-zero",
            opponentId: "sektor",
            title: "Свободная воля",
            description: "Победить Sektor, играя за Unmasked Sub-Zero.",
            label: "Противостояние",
            loreNote: "Саб-Зиро покинул Lin Kuei и стал целью кибернетических воинов клана, включая Сектора."
        },
        {
            id: "true-heir",
            fighterId: "kitana",
            opponentId: "mileena",
            title: "Истинная наследница",
            description: "Победить Mileena, играя за Kitana.",
            label: "Противостояние",
            loreNote: "Милина была создана как искажённый двойник Китаны и претендентка на её место при дворе Шао Кана."
        },
        {
            id: "leave-black-dragon",
            fighterId: "kabal",
            opponentId: "kano",
            title: "Порвать с Black Dragon",
            description: "Победить Kano, играя за Kabal.",
            label: "Противостояние",
            loreNote: "Кабал и Кано состояли в Black Dragon, но в концовке MK3 Кабал выбирает борьбу с преступностью."
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
            id: "first-fatality",
            title: "Точка поставлена",
            description: "Выполнить Fatality любым бойцом.",
            label: "Fatality",
            condition: { type: "finisher", kind: "fatality" }
        },
        {
            id: "first-animality",
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
            fighterId: "sheeva",
            title: "Четверная сила",
            description: "Выполнить Brutality, играя за Sheeva.",
            label: "Brutality",
            condition: { type: "finisher", kind: "brutality" }
        },
        {
            id: "kabal-antifatality",
            fighterId: "kabal",
            title: "Отказ от финала",
            description: "Выполнить Antifatality за Kabal в UMK3 UK [260709].",
            label: "Особое добивание",
            condition: {
                type: "finisher",
                kind: "antifatality",
                romIds: ["umk3uk"]
            }
        }
    ]
};

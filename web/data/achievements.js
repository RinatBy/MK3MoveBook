window.MOVEBOOK_ACHIEVEMENTS = {
    preview: true,
    general: [
        {
            id: "victories-1",
            title: "Первая победа",
            description: "Одержать первую победу в завершённом матче.",
            label: "1 победа",
            condition: { type: "victories", count: 1 }
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
            id: "victories-100",
            title: "Легенда арены",
            description: "Одержать 100 побед в завершённых матчах.",
            label: "100 побед",
            condition: { type: "victories", count: 100 }
        }
    ],
    fighters: [
        {
            id: "fighter-debut-kabal",
            fighterId: "kabal",
            title: "Новая дорога",
            description: "Одержать победу, играя за Kabal.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-kung-lao",
            fighterId: "kung-lao",
            title: "Наследие Великого Кун Лао",
            description: "Одержать победу, играя за Kung Lao.",
            label: "Боевой дебют"
        },
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
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-jax",
            fighterId: "jax",
            title: "Полная боевая готовность",
            description: "Одержать победу, играя за Jax.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-sonya",
            fighterId: "sonya",
            title: "Спецназ в деле",
            description: "Одержать победу, играя за Sonya.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-kitana",
            fighterId: "kitana",
            title: "Наследница Эдении",
            description: "Одержать победу, играя за Kitana.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-stryker",
            fighterId: "stryker",
            title: "Последний защитник города",
            description: "Одержать победу, играя за Stryker.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-unmasked-sub-zero",
            fighterId: "unmasked-sub-zero",
            title: "Вне закона Lin Kuei",
            description: "Одержать победу, играя за Unmasked Sub-Zero.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-jade",
            fighterId: "jade",
            title: "Верность подруге",
            description: "Одержать победу, играя за Jade.",
            label: "Боевой дебют"
        },
        {
            id: "fighter-debut-liu-kang",
            fighterId: "liu-kang",
            title: "Чемпион Mortal Kombat",
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
            label: "Противостояние"
        },
        {
            id: "protect-the-princess",
            fighterId: "jade",
            opponentId: "reptile",
            title: "Защитить принцессу",
            description: "Победить Reptile, играя за Jade.",
            label: "Противостояние"
        },
        {
            id: "free-will",
            fighterId: "unmasked-sub-zero",
            opponentId: "sektor",
            title: "Свободная воля",
            description: "Победить Sektor, играя за Unmasked Sub-Zero.",
            label: "Противостояние"
        },
        {
            id: "true-heir",
            fighterId: "kitana",
            opponentId: "mileena",
            title: "Истинная наследница",
            description: "Победить Mileena, играя за Kitana.",
            label: "Противостояние"
        },
        {
            id: "leave-black-dragon",
            fighterId: "kabal",
            opponentId: "kano",
            title: "Порвать с Black Dragon",
            description: "Победить Kano, играя за Kabal.",
            label: "Противостояние"
        }
    ],
    special: [
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

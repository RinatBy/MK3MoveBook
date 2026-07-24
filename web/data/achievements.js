window.MOVEBOOK_ACHIEVEMENTS = {
    preview: true,
    general: [
        {
            id: "first-victory",
            title: "Первая победа",
            description: "Одержать первую победу в завершённом матче.",
            label: "Общее"
        }
    ],
    fighterTemplate: {
        idPrefix: "fighter-debut-",
        title: "Боевой дебют",
        description: "Одержать победу, играя за {fighter}.",
        label: "Боец",
        excludedFighterIds: [
            "rain",
            "noob-saibot",
            "motaro",
            "shao-kahn"
        ]
    },
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
            id: "escaped-again",
            fighterId: "kano",
            opponentId: "sonya",
            title: "Снова ускользнул",
            description: "Победить Соню, играя за Кано.",
            label: "Противостояние"
        }
    ]
};

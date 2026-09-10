(function (root) {
    "use strict";

    const mameFighterIds = {
        0x00: "kano",
        0x01: "sonya",
        0x02: "jax",
        0x03: "nightwolf",
        0x04: "unmasked-sub-zero",
        0x05: "stryker",
        0x06: "sindel",
        0x07: "sektor",
        0x08: "cyrax",
        0x09: "kung-lao",
        0x0A: "kabal",
        0x0B: "sheeva",
        0x0C: "shang-tsung",
        0x0D: "liu-kang",
        0x0F: "kitana",
        0x10: "jade",
        0x11: "mileena",
        0x12: "scorpion",
        0x13: "reptile",
        0x14: "ermac",
        0x15: "classic-sub-zero"
    };

    function emptyProgress() {
        return {
            victories: 0,
            unlocked: {},
            processedEvents: []
        };
    }

    function normalizeProgress(saved) {
        if (!saved || typeof saved !== "object") {
            return emptyProgress();
        }
        return {
            victories: Math.max(0, Number(saved.victories) || 0),
            unlocked: saved.unlocked && typeof saved.unlocked === "object"
                ? { ...saved.unlocked }
                : {},
            processedEvents: Array.isArray(saved.processedEvents)
                ? saved.processedEvents.slice(-100).map(String)
                : []
        };
    }

    function definitions(data) {
        return [
            ...(data.general || []),
            ...(data.computer || []),
            ...(data.fighters || []),
            ...(data.lore || []),
            ...(data.special || [])
        ];
    }

    function rememberEvent(progress, key) {
        if (!key || progress.processedEvents.includes(key)) {
            return false;
        }
        progress.processedEvents.push(key);
        progress.processedEvents = progress.processedEvents.slice(-100);
        return true;
    }

    function unlock(progress, definition, unlocked, timestamp) {
        if (!definition || progress.unlocked[definition.id]) {
            return;
        }
        progress.unlocked[definition.id] = timestamp;
        unlocked.push(definition);
    }

    function applyVictoryMilestones(progress, data, unlocked, timestamp) {
        (data.general || []).forEach(definition => {
            const condition = definition.condition || {};
            if (condition.type === "victories" &&
                progress.victories >= Number(condition.count)) {
                unlock(progress, definition, unlocked, timestamp);
            }
        });
    }

    function processVictory(progress, message, data, unlocked, timestamp) {
        if (message.eventKind !== "match-ended" ||
            Number(message.eventSequence) <= 0) {
            return false;
        }
        const eventKey = [
            "match",
            Number(message.processId) || 0,
            String(message.romId || "unknown"),
            Number(message.eventSequence) || 0
        ].join(":");
        if (!rememberEvent(progress, eventKey)) {
            return false;
        }

        const winnerSide = String(message.winnerSide || "");
        const playerOneId = mameFighterIds[Number(message.fightPlayerOneId)];
        const playerTwoId = mameFighterIds[Number(message.fightPlayerTwoId)];
        const winnerId = winnerSide === "p1" ? playerOneId : "";
        const opponentId = winnerSide === "p1" ? playerTwoId : "";

        if (winnerId) {
            progress.victories++;
            applyVictoryMilestones(progress, data, unlocked, timestamp);
            (data.fighters || [])
                .filter(definition => definition.fighterId === winnerId)
                .forEach(definition =>
                    unlock(progress, definition, unlocked, timestamp)
                );
            (data.lore || [])
                .filter(definition => definition.fighterId === winnerId &&
                    definition.opponentId === opponentId)
                .forEach(definition =>
                    unlock(progress, definition, unlocked, timestamp)
                );
        }
        return true;
    }

    function processFinisher(progress, message, data, unlocked, timestamp) {
        const sequence = Number(message.finisherSequence) || 0;
        const kind = String(message.finisherKind || "");
        const winnerSide = String(message.finisherWinnerSide || "");
        if (!sequence || !kind || winnerSide !== "p1") {
            return false;
        }
        const eventKey = [
            "finisher",
            Number(message.processId) || 0,
            String(message.romId || "unknown"),
            sequence,
            kind
        ].join(":");
        if (!rememberEvent(progress, eventKey)) {
            return false;
        }

        const fighterId = mameFighterIds[Number(message.finisherFighterId)];
        const romId = String(message.romId || "");
        (data.special || []).forEach(definition => {
            const condition = definition.condition || {};
            const validRom = !Array.isArray(condition.romIds) ||
                condition.romIds.includes(romId);
            const validFighter = !definition.fighterId ||
                definition.fighterId === fighterId;
            if (condition.type === "finisher" &&
                condition.kind === kind &&
                validFighter &&
                validRom) {
                unlock(progress, definition, unlocked, timestamp);
            }
        });
        return true;
    }

    function processFlawlessRound(progress, message, data, unlocked, timestamp) {
        const kind = String(message.eventKind || "");
        const sequence = Number(message.eventSequence) || 0;
        if (!sequence || (kind !== "round-ended" && kind !== "match-ended")) {
            return false;
        }

        const playerOneHealth = Number(message.playerOneHealth);
        const playerTwoHealth = Number(message.playerTwoHealth);
        const healthWinnerSide = playerOneHealth > 0 && playerTwoHealth === 0
            ? "p1"
            : "";
        const reportedWinnerSide = String(message.winnerSide || "");
        if (!healthWinnerSide ||
            (reportedWinnerSide && reportedWinnerSide !== healthWinnerSide)) {
            return false;
        }

        const eventKey = [
            "round-result",
            Number(message.processId) || 0,
            String(message.romId || "unknown"),
            sequence
        ].join(":");
        if (!rememberEvent(progress, eventKey)) {
            return false;
        }

        const winnerHealth = healthWinnerSide === "p1"
            ? playerOneHealth
            : playerTwoHealth;
        (data.special || []).forEach(definition => {
            const condition = definition.condition || {};
            if (condition.type === "flawless-round" &&
                winnerHealth === Number(condition.fullHealth)) {
                unlock(progress, definition, unlocked, timestamp);
            }
        });
        return true;
    }

    function processStatus(current, message, data, timestamp) {
        const progress = normalizeProgress(current);
        const unlocked = [];
        const unlockedAt = timestamp || new Date().toISOString();
        const victoryChanged = processVictory(
            progress,
            message,
            data,
            unlocked,
            unlockedAt
        );
        const finisherChanged = processFinisher(
            progress,
            message,
            data,
            unlocked,
            unlockedAt
        );
        const flawlessChanged = processFlawlessRound(
            progress,
            message,
            data,
            unlocked,
            unlockedAt
        );
        if (unlocked.length > 1) {
            unlocked.splice(1).forEach(definition => {
                delete progress.unlocked[definition.id];
            });
        }
        return {
            progress,
            unlocked,
            changed: victoryChanged || finisherChanged || flawlessChanged
        };
    }

    function reconcileProgress(current, data, timestamp) {
        const progress = normalizeProgress(current);
        const unlocked = [];
        applyVictoryMilestones(
            progress,
            data,
            unlocked,
            timestamp || new Date().toISOString()
        );
        return {
            progress,
            unlocked,
            changed: unlocked.length > 0
        };
    }

    root.MOVEBOOK_ACHIEVEMENT_ENGINE = {
        emptyProgress,
        normalizeProgress,
        definitions,
        processStatus,
        reconcileProgress
    };
}(typeof window === "undefined" ? globalThis : window));

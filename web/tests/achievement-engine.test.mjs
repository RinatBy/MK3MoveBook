import assert from "node:assert/strict";
import fs from "node:fs";
import path from "node:path";
import vm from "node:vm";
import { fileURLToPath } from "node:url";

const testDirectory = path.dirname(fileURLToPath(import.meta.url));
const webDirectory = path.resolve(testDirectory, "..");
const context = vm.createContext({});
context.window = context;
context.globalThis = context;

for (const relativePath of [
    "data/achievements.js",
    "achievement-engine.js"
]) {
    const filename = path.join(webDirectory, relativePath);
    vm.runInContext(fs.readFileSync(filename, "utf8"), context, { filename });
}

const data = context.MOVEBOOK_ACHIEVEMENTS;
const engine = context.MOVEBOOK_ACHIEVEMENT_ENGINE;

function status(overrides = {}) {
    return {
        processId: 100,
        romId: "umk3tm",
        eventSequence: 0,
        eventKind: "",
        winnerSide: "",
        fightPlayerOneId: 0,
        fightPlayerTwoId: 0,
        playerOneHealth: 0,
        playerTwoHealth: 0,
        finisherSequence: 0,
        finisherKind: "",
        finisherWinnerSide: "",
        finisherFighterId: -1,
        ...overrides
    };
}

function unlockedIds(result) {
    return Array.from(result.unlocked, item => item.id).sort();
}

let progress = engine.emptyProgress();
let result = engine.processStatus(
    progress,
    status({
        eventSequence: 1,
        eventKind: "match-ended",
        winnerSide: "p1",
        fightPlayerOneId: 0x01,
        fightPlayerTwoId: 0x00
    }),
    data,
    "2026-07-25T12:00:00.000Z"
);
progress = result.progress;
assert.deepEqual(unlockedIds(result), ["old-scores"]);
assert.equal(progress.victories, 1);

result = engine.processStatus(
    progress,
    status({
        eventSequence: 1,
        eventKind: "match-ended",
        winnerSide: "p1",
        fightPlayerOneId: 0x01,
        fightPlayerTwoId: 0x00
    }),
    data
);
progress = result.progress;
assert.equal(result.changed, false);
assert.equal(progress.victories, 1);
assert.deepEqual(unlockedIds(result), []);

result = engine.processStatus(
    progress,
    status({
        processId: 200,
        romId: "umk3tm",
        finisherSequence: 1,
        finisherKind: "antifatality",
        finisherWinnerSide: "p1",
        finisherFighterId: 0x0A
    }),
    data
);
progress = result.progress;
assert.equal(Boolean(progress.unlocked["kabal-antifatality"]), false);

result = engine.processStatus(
    progress,
    status({
        processId: 200,
        romId: "umk3tm",
        finisherSequence: 2,
        finisherKind: "fatality",
        finisherWinnerSide: "p1",
        finisherFighterId: 0x0A
    }),
    data
);
progress = result.progress;
assert.deepEqual(unlockedIds(result), ["first-fatality"]);

result = engine.processStatus(
    progress,
    status({
        processId: 200,
        romId: "umk3tm",
        finisherSequence: 3,
        finisherKind: "brutality",
        finisherWinnerSide: "p1",
        finisherFighterId: 0x02
    }),
    data
);
progress = result.progress;
assert.deepEqual(unlockedIds(result), ["jax-brutality"]);
assert.equal(Boolean(progress.unlocked["sheeva-brutality"]), false);

result = engine.processStatus(
    progress,
    status({
        processId: 200,
        romId: "umk3tm",
        finisherSequence: 4,
        finisherKind: "brutality",
        finisherWinnerSide: "p1",
        finisherFighterId: 0x0B
    }),
    data
);
progress = result.progress;
assert.deepEqual(unlockedIds(result), ["sheeva-brutality"]);

result = engine.processStatus(
    progress,
    status({
        processId: 201,
        romId: "umk3uk",
        finisherSequence: 1,
        finisherKind: "antifatality",
        finisherWinnerSide: "p1",
        finisherFighterId: 0x0A
    }),
    data,
    "2026-07-25T12:01:00.000Z"
);
progress = result.progress;
assert.deepEqual(unlockedIds(result), ["kabal-antifatality"]);

for (let sequence = 2; sequence <= 10; sequence++) {
    result = engine.processStatus(
        progress,
        status({
            eventSequence: sequence,
            eventKind: "match-ended",
            winnerSide: "p2",
            fightPlayerOneId: 0x12,
            fightPlayerTwoId: 0x0A
        }),
        data
    );
    progress = result.progress;
}
assert.equal(progress.victories, 1);
assert.equal(Boolean(progress.unlocked["victories-5"]), false);
assert.equal(Boolean(progress.unlocked["victories-10"]), false);
assert.equal(Boolean(progress.unlocked["victories-25"]), false);

result = engine.processStatus(
    progress,
    status({
        processId: 202,
        romId: "umk3tm",
        finisherSequence: 1,
        finisherKind: "friendship",
        finisherWinnerSide: "p2",
        finisherFighterId: 0x00
    }),
    data
);
assert.deepEqual(unlockedIds(result), []);

let flawlessProgress = engine.emptyProgress();
result = engine.processStatus(
    flawlessProgress,
    status({
        processId: 300,
        eventSequence: 2,
        eventKind: "round-ended",
        playerOneHealth: 166,
        playerTwoHealth: 0
    }),
    data
);
flawlessProgress = result.progress;
assert.deepEqual(unlockedIds(result), ["flawless-victory"]);

result = engine.processStatus(
    flawlessProgress,
    status({
        processId: 300,
        eventSequence: 2,
        eventKind: "round-ended",
        playerOneHealth: 166,
        playerTwoHealth: 0
    }),
    data
);
assert.equal(result.changed, false);
assert.deepEqual(unlockedIds(result), []);

result = engine.processStatus(
    engine.emptyProgress(),
    status({
        processId: 301,
        eventSequence: 2,
        eventKind: "round-ended",
        playerOneHealth: 165,
        playerTwoHealth: 0
    }),
    data
);
assert.deepEqual(unlockedIds(result), []);

result = engine.processStatus(
    engine.emptyProgress(),
    status({
        processId: 302,
        eventSequence: 4,
        eventKind: "match-ended",
        winnerSide: "p2",
        fightPlayerOneId: 0x00,
        fightPlayerTwoId: 0x12,
        playerOneHealth: 0,
        playerTwoHealth: 166
    }),
    data
);
assert.deepEqual(unlockedIds(result), []);

const debutFighters = new Set(Array.from(data.fighters, item => item.fighterId));
const confrontationFighters = Array.from(data.lore, item => item.fighterId);
const personalFinisherFighters = Array.from(
    data.special,
    item => item.fighterId
).filter(Boolean);
assert.deepEqual(
    confrontationFighters.filter(fighterId => debutFighters.has(fighterId)),
    []
);
assert.deepEqual(
    personalFinisherFighters.filter(fighterId => debutFighters.has(fighterId)),
    []
);

const reconciled = engine.reconcileProgress({
    victories: 15,
    unlocked: {},
    processedEvents: []
}, data, "2026-07-26T12:00:00.000Z");
assert.deepEqual(unlockedIds(reconciled), ["victories-10", "victories-5"]);
assert.equal(reconciled.progress.victories, 15);
assert.equal(engine.definitions(data).length, 24);

console.log("Achievement engine: all scenarios passed.");

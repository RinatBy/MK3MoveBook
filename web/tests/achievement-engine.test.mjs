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
        finisherSequence: 0,
        finisherKind: "",
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
assert.deepEqual(unlockedIds(result), [
    "fighter-debut-sonya",
    "old-scores",
    "victories-1"
]);
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
        finisherFighterId: 0x0A
    }),
    data
);
progress = result.progress;
assert.equal(Boolean(progress.unlocked["kabal-antifatality"]), false);

result = engine.processStatus(
    progress,
    status({
        processId: 201,
        romId: "umk3uk",
        finisherSequence: 1,
        finisherKind: "antifatality",
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
assert.equal(progress.victories, 10);
assert.equal(Boolean(progress.unlocked["victories-10"]), true);
assert.equal(engine.definitions(data).length, 21);

console.log("Achievement engine: 5 scenarios passed.");

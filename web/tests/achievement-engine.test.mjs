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
const appSource = fs.readFileSync(path.join(webDirectory, "app.js"), "utf8");
const indexSource = fs.readFileSync(path.join(webDirectory, "index.html"), "utf8");
const styleSource = fs.readFileSync(path.join(webDirectory, "styles.css"), "utf8");

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
assert.deepEqual(unlockedIds(result), ["fighter-debut-sonya"]);
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
        eventSequence: 2,
        eventKind: "match-ended",
        winnerSide: "p1",
        fightPlayerOneId: 0x01,
        fightPlayerTwoId: 0x00
    }),
    data
);
progress = result.progress;
assert.deepEqual(unlockedIds(result), ["old-scores"]);

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

for (let sequence = 3; sequence <= 10; sequence++) {
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
assert.equal(progress.victories, 2);
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
    ["sonya", "unmasked-sub-zero", "kabal", "liu-kang"]
);
assert.deepEqual(
    personalFinisherFighters.filter(fighterId => debutFighters.has(fighterId)),
    ["jax", "kabal", "stryker"]
);

const reconciled = engine.reconcileProgress({
    victories: 15,
    unlocked: {},
    processedEvents: []
}, data, "2026-07-26T12:00:00.000Z");
assert.deepEqual(unlockedIds(reconciled), ["victories-10", "victories-5"]);
assert.equal(reconciled.progress.victories, 15);
const catalog = new Map(engine.definitions(data).map(item => [item.id, item]));
assert.equal(catalog.get("victories-25").title, "Боец");
assert.equal(catalog.get("victories-50").title, "Закалённый боец");
assert.equal(catalog.get("victories-75").title, "Опытный боец");
assert.equal(catalog.get("victories-100").title, "Воин");
assert.equal(catalog.get("victories-250").title, "Опытный воин");
assert.equal(catalog.get("victories-500").title, "Мастер");
assert.equal(catalog.get("victories-750").title, "Легендарный мастер");
assert.equal(catalog.get("victories-1000").title, "Легенда арены");
assert.equal(catalog.get("tower-master-complete").title, "Испытание мастера");
assert.equal(catalog.get("tower-master-fourth-complete").title, "Мастер");
assert.equal(catalog.get("fighter-debut-stryker").title, "Вы арестованы");
assert.equal(catalog.has("fighter-debut-jade"), false);
assert.equal(
    data.computer.some(item => item.description.includes("победить Shao Kahn")),
    false
);
assert.equal(appSource.includes("Ранее без разделения"), false);
assert.equal(appSource.includes("Полный CSV-отчёт"), false);
assert.equal(
    appSource.includes("Boolean(mameDiagnostic.diagnosticMode)"),
    true
);
assert.equal(
    appSource.includes('diagnosticState !== "match-synchronizing"'),
    true
);
assert.equal(
    appSource.includes("ТЕКУЩИЙ МАТЧ ПРОПУЩЕН"),
    true
);
assert.equal(indexSource.includes('id="logoutDialog"'), true);
assert.equal(indexSource.includes('aria-modal="true"'), true);
assert.equal(indexSource.includes('class="movebook-dialog-logo"'), true);
assert.equal(indexSource.includes('class="movebook-dialog-logo-frame"'), true);
assert.equal(indexSource.includes('class="brand-mark"'), true);
assert.equal(indexSource.includes('class="brand-copy"'), true);
assert.equal(indexSource.includes("<strong>MOVEBOOK</strong>"), true);
assert.equal(indexSource.includes("MK3 MOVEBOOK"), false);
assert.equal(styleSource.includes(".movebook-dialog-backdrop"), true);
assert.equal(styleSource.includes(".movebook-dialog-logo-frame"), true);
assert.equal(styleSource.includes("border-radius: 7px;"), true);
assert.equal(styleSource.includes('img[src$="fatality.gif"]'), true);
assert.match(styleSource, /\.brand-mark\s*\{[\s\S]*?justify-content:\s*center;/);
assert.doesNotMatch(styleSource, /\.brand-mark\s*\{[^}]*(?:border-radius|overflow:\s*hidden)/s);
assert.doesNotMatch(styleSource, /\.brand-mark img\s*\{[^}]*transform:/s);
assert.equal(indexSource.includes('href="styles.css?v='), true);
assert.equal(appSource.includes("function openLogoutDialog()"), true);
assert.equal(appSource.includes('bridge.postMessage("logout-movebook")'), true);

assert.deepEqual(
    Array.from(debutFighters).sort(),
    [
        "cyrax",
        "jax",
        "kabal",
        "kano",
        "kung-lao",
        "liu-kang",
        "nightwolf",
        "robot-smoke",
        "sektor",
        "shang-tsung",
        "sindel",
        "sonya",
        "stryker",
        "unmasked-sub-zero"
    ]
);
for (const definition of data.fighters) {
    assert.ok(definition.icon, `Missing icon for ${definition.id}`);
    assert.equal(
        fs.existsSync(path.join(webDirectory, definition.icon)),
        true,
        `Missing achievement icon for ${definition.id}`
    );
}
assert.equal(data.fighters.length, 14);
assert.equal(engine.definitions(data).length, 50);

console.log("Achievement engine: all scenarios passed.");

for (const romId of ["umk3tm", "umk3tm60", "umk3uk", "umk3uk2"]) {
    const debut = engine.processStatus(engine.emptyProgress(), status({
        romId, eventSequence: 1, eventKind: "match-ended", winnerSide: "p1",
        fightPlayerOneId: 0x03, fightPlayerTwoId: 0x10
    }), data);
    assert.equal(debut.progress.victories, 1, romId);
    assert.deepEqual(unlockedIds(debut), ["fighter-debut-nightwolf"], romId);
    const anti = engine.processStatus(engine.emptyProgress(), status({
        romId, finisherSequence: 1, finisherKind: "antifatality",
        finisherWinnerSide: "p1", finisherFighterId: 0x0A
    }), data);
    assert.deepEqual(unlockedIds(anti),
        ["umk3uk", "umk3uk2"].includes(romId) ? ["kabal-antifatality"] : [], romId);
}
console.log("Four-ROM achievement compatibility checks passed.");

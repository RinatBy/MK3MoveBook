(() => {
    "use strict";

    const data = window.MOVEBOOK_DATA;
    const lore = window.MOVEBOOK_LORE;
    const versionLabels = {
        umk3uk: "UMK3 UK",
        umk3tm: "Team Edition"
    };
    const visibleVersions = ["umk3uk", "umk3tm"];
    const tabLabels = {
        moves: "Приёмы",
        history: "История",
        facts: "Факты"
    };
    const fighterAccents = {
        "Kabal": "#2f8c61",
        "Human Smoke": "#8f9398",
        "Kung Lao": "#b77924",
        "Kano": "#c73434",
        "Reptile": "#2d9d4f",
        "Ermac": "#c83b3e",
        "Nightwolf": "#c69a2e",
        "Robot Smoke": "#85818e",
        "Sindel": "#7f3a9c",
        "Jax": "#4d4da2",
        "Sonya": "#2f8f4e",
        "Kitana": "#3d9dca",
        "Stryker": "#2187a5",
        "Classic Sub-Zero": "#3c86c4",
        "Cyrax": "#c5ae24",
        "Shang Tsung": "#a24a54",
        "Mileena": "#a63a91",
        "Sheeva": "#2d8c55",
        "Rain": "#7c4eb1",
        "Motaro": "#a9633d",
        "Shao Kahn": "#8c4bc1",
        "Scorpion": "#d8a527",
        "Unmasked Sub-Zero": "#2f95bd",
        "Jade": "#32a867",
        "Liu Kang": "#c93337",
        "Sektor": "#d13a3d",
        "Noob Saibot": "#3d3747"
    };
    const segaLabels = {
        HP: "X",
        LP: "A",
        RUN: "Y",
        BL: "B",
        HK: "Z",
        LK: "C"
    };
    const actionNames = {
        HP: "High Punch",
        LP: "Low Punch",
        RUN: "Run",
        BL: "Block",
        HK: "High Kick",
        LK: "Low Kick"
    };
    const historyImageFighters = new Set([
        "jade", "kitana", "reptile", "scorpion",
        "cyrax", "jax", "kabal", "kano", "kung-lao", "liu-kang",
        "nightwolf", "sektor", "shang-tsung", "sheeva", "sindel",
        "robot-smoke", "sonya", "stryker", "unmasked-sub-zero"
    ]);
    const moveAnimations = window.MOVEBOOK_ANIMATIONS || {};
    const changelogEntries = window.MOVEBOOK_CHANGELOG || [];
    const latestReleaseUrl =
        "https://github.com/RinatBy/MK3MoveBook/releases/latest";

    const state = {
        version: "umk3uk",
        fighterId: "kabal",
        tab: "moves",
        notation: loadNotation(),
        query: "",
        selectedMoveKey: ""
    };

    const elements = {
        versionSelect: document.querySelector("#versionSelect"),
        searchInput: document.querySelector("#searchInput"),
        rosterList: document.querySelector("#rosterList"),
        fighterCount: document.querySelector("#fighterCount"),
        routeBar: document.querySelector("#routeBar"),
        updateButton: document.querySelector("#updateButton"),
        updatePanel: document.querySelector("#updatePanel"),
        updatePanelClose: document.querySelector("#updatePanelClose"),
        updatePanelStatus: document.querySelector("#updatePanelStatus"),
        updatePanelVersion: document.querySelector("#updatePanelVersion"),
        updateActionButton: document.querySelector("#updateActionButton"),
        updateNotesTitle: document.querySelector("#updateNotesTitle"),
        updateNotesList: document.querySelector("#updateNotesList"),
        secretsButton: document.querySelector("#secretsButton"),
        fighterSeal: document.querySelector(".fighter-seal"),
        heroPortrait: document.querySelector("#heroPortrait"),
        fighterTitle: document.querySelector("#fighterTitle"),
        fighterSubtitle: document.querySelector("#fighterSubtitle"),
        categoryNav: document.querySelector("#categoryNav"),
        movePreview: document.querySelector("#movePreview"),
        movePreviewVideo: document.querySelector("#movePreviewVideo"),
        sectionTabs: document.querySelector("#sectionTabs"),
        notationToggle: document.querySelector("#notationToggle"),
        pageContent: document.querySelector("#pageContent"),
        comboFocus: document.querySelector("#comboFocus"),
        communityButton: document.querySelector("#communityButton"),
        statusText: document.querySelector("#statusText"),
        leftPageNumber: document.querySelector("#leftPageNumber"),
        rightPageNumber: document.querySelector("#rightPageNumber"),
        editionLabel: document.querySelector(".edition-label"),
        contentsTitle: document.querySelector(".contents-title"),
        bookNoteLabel: document.querySelector("#bookNoteLabel"),
        bookNoteValue: document.querySelector("#bookNoteValue"),
        backButton: document.querySelector("#backButton"),
        forwardButton: document.querySelector("#forwardButton")
    };

    function loadNotation() {
        try {
            return localStorage.getItem("movebook-notation") === "sega"
                ? "sega"
                : "umk3";
        } catch {
            return "umk3";
        }
    }

    const slug = value => value.toLowerCase().replace(/[^a-z0-9]+/g, "-").replace(/^-|-$/g, "");

    function currentVersion() {
        return data.versions[state.version] || data.versions.umk3uk;
    }

    function currentFighter() {
        const fighters = currentVersion().fighters;
        return fighters.find(fighter => fighter.id === state.fighterId) || fighters[0];
    }

    function setRoute(replace = false) {
        const route = state.tab === "community"
            ? "#/community"
            : (state.tab === "secrets"
                ? `#/${state.version}/secrets`
                : `#/${state.version}/${state.fighterId}/${state.tab}`);
        if (window.location.hash === route) {
            render();
            return;
        }
        if (replace) {
            history.replaceState(null, "", route);
            render();
        } else {
            window.location.hash = route;
        }
    }

    function readRoute() {
        const parts = window.location.hash.replace(/^#\/?/, "").split("/").filter(Boolean);
        const requestedVersion = parts[0];
        const requestedFighter = parts[1];
        const requestedTab = parts[2];
        if (requestedVersion === "community") {
            const fighters = currentVersion().fighters;
            if (!fighters.some(item => item.id === state.fighterId)) {
                state.fighterId = fighters[0].id;
            }
            state.tab = "community";
            return;
        }
        if (requestedVersion === "umk3tm60") {
            state.version = "umk3tm";
        } else if (requestedVersion && visibleVersions.includes(requestedVersion)) {
            state.version = requestedVersion;
        }
        const fighters = currentVersion().fighters;
        if (requestedFighter === "secrets") {
            if (!fighters.some(item => item.id === state.fighterId)) {
                state.fighterId = fighters[0].id;
            }
            state.tab = "secrets";
            return;
        }
        state.fighterId = fighters.some(item => item.id === requestedFighter)
            ? requestedFighter
            : fighters[0].id;
        state.tab = Object.prototype.hasOwnProperty.call(tabLabels, requestedTab)
            ? requestedTab
            : "moves";
    }

    function portraitPath(fighter) {
        return `assets/portraits/${fighter.id}.png`;
    }

    function fighterMatches(fighter, query) {
        if (!query) {
            return true;
        }
        const needle = query.toLocaleLowerCase("ru");
        if (fighter.name.toLocaleLowerCase("ru").includes(needle)) {
            return true;
        }
        const fighterLore = lore.fighters[fighter.id];
        const loreText = fighterLore
            ? [
                fighterLore.intro,
                fighterLore.ending,
                fighterLore.note,
                ...(fighterLore.facts || [])
            ].filter(Boolean).join(" ").toLocaleLowerCase("ru")
            : "";
        if (loreText.includes(needle)) {
            return true;
        }
        return fighter.categories.some(category =>
            category.name.toLocaleLowerCase("ru").includes(needle) ||
            category.moves.some(move =>
                `${move.label} ${move.notation}`.toLocaleLowerCase("ru").includes(needle)
            )
        );
    }

    function renderVersionSelect() {
        elements.versionSelect.innerHTML = visibleVersions
            .map(version => `<option value="${version}">${versionLabels[version] || version}</option>`)
            .join("");
        elements.versionSelect.value = state.version;
    }

    function renderRoster() {
        const fighters = currentVersion().fighters;
        const filtered = fighters.filter(fighter => fighterMatches(fighter, state.query));
        elements.fighterCount.textContent = `${filtered.length} / ${fighters.length}`;
        elements.rosterList.innerHTML = filtered.map(fighter => {
            const selected = !["secrets", "community"].includes(state.tab)
                && fighter.id === state.fighterId;
            const accent = fighterAccents[fighter.name] || "#a62a42";
            return `
                <button class="fighter-card" type="button" role="option"
                    data-fighter="${fighter.id}" aria-selected="${selected}"
                    style="--card-accent:${accent}">
                    <img src="${portraitPath(fighter)}" alt="">
                    <span>${escapeHtml(fighter.name)}</span>
                    <small>${String(fighter.number).padStart(2, "0")}</small>
                </button>`;
        }).join("");
    }

    function renderFighterPage() {
        if (state.tab === "community") {
            renderCommunityPage();
            return;
        }
        if (state.tab === "secrets") {
            renderSecretsPage();
            return;
        }

        const fighter = currentFighter();
        const fighterLore = lore.fighters[fighter.id] || {};
        const accent = fighterAccents[fighter.name] || "#c39032";
        document.documentElement.style.setProperty("--accent", accent);
        elements.fighterSeal.classList.remove("is-secrets", "is-community");
        elements.sectionTabs.hidden = false;
        elements.sectionTabs.parentElement.classList.remove("is-global-page");
        elements.categoryNav.hidden = false;
        elements.editionLabel.textContent = "ULTIMATE KOMBAT DOSSIER";
        elements.contentsTitle.textContent = "Содержание";
        elements.heroPortrait.src = portraitPath(fighter);
        elements.heroPortrait.alt = fighter.name;
        elements.fighterTitle.textContent = fighter.name;
        const moveCount = fighter.categories.reduce((sum, category) => sum + category.moves.length, 0);
        elements.fighterSubtitle.textContent =
            `${versionLabels[state.version] || state.version} · ${moveCount} приёмов`;
        elements.leftPageNumber.textContent = `БОЕЦ ${String(fighter.number).padStart(2, "0")}`;
        elements.rightPageNumber.textContent = (tabLabels[state.tab] || "").toUpperCase();
        elements.bookNoteLabel.textContent =
            state.tab === "moves" ? "Источник комбинаций" : "Источник перевода";
        elements.bookNoteValue.textContent =
            state.tab === "moves" ? "command.dat" : lore.source.title;

        [...elements.sectionTabs.querySelectorAll("[data-tab]")].forEach(button => {
            button.setAttribute("aria-selected", String(button.dataset.tab === state.tab));
        });
        renderRouteBar(fighter);
        if (state.tab === "moves") {
            elements.categoryNav.innerHTML = fighter.categories.map(category => `
                <button class="category-link" type="button" data-category="${slug(category.name)}">
                    <span>${escapeHtml(category.name)}</span>
                    <b>${category.moves.length}</b>
                </button>`).join("");
            renderMoves(fighter);
        } else if (state.tab === "history") {
            const historyLinks = [
                fighterLore.intro && ["Роль в UMK3", "lore-intro"],
                fighterLore.ending && ["Финал персонажа", "lore-ending"],
                fighterLore.note && ["Примечание", "lore-note"]
            ].filter(Boolean);
            elements.categoryNav.innerHTML = historyLinks.map(item => `
                <button class="category-link" type="button" data-category="${item[1]}">
                    <span>${item[0]}</span><b>◆</b>
                </button>`).join("");
            renderHistory(fighter, fighterLore);
        } else {
            elements.categoryNav.innerHTML = `
                <button class="category-link" type="button" data-category="lore-facts">
                    <span>Из книги</span><b>${(fighterLore.facts || []).length}</b>
                </button>`;
            renderFacts(fighter, fighterLore);
        }
    }

    function renderRouteBar(fighter) {
        elements.routeBar.innerHTML =
            `MOVEBOOK&nbsp;&nbsp;/&nbsp;&nbsp;${escapeHtml(versionLabels[state.version] || state.version)}` +
            `&nbsp;&nbsp;/&nbsp;&nbsp;<b>${escapeHtml(fighter.name)}</b>` +
            `&nbsp;&nbsp;/&nbsp;&nbsp;${tabLabels[state.tab]}`;
    }

    function renderSecretsPage() {
        hideMovePreview();
        const sections = [
            ["Как вводить", "codes-how"],
            ["Символы 0–9", "codes-symbols"],
            ["Правила боя", "codes-game"],
            ["Арены", "codes-arenas"],
            ["Сообщения", "codes-messages"],
            ["Соперники", "codes-opponents"],
            ["Открытие бойцов", "codes-unlocks"]
        ];
        document.documentElement.style.setProperty("--accent", "#d8ad31");
        elements.fighterSeal.classList.remove("is-community");
        elements.fighterSeal.classList.add("is-secrets");
        elements.sectionTabs.hidden = false;
        elements.sectionTabs.parentElement.classList.remove("is-global-page");
        [...elements.sectionTabs.querySelectorAll("[data-tab]")].forEach(button => {
            button.setAttribute("aria-selected", String(button.dataset.tab === "secrets"));
        });
        elements.categoryNav.hidden = false;
        elements.editionLabel.textContent = "ULTIMATE KOMBAT DOSSIER";
        elements.contentsTitle.textContent = "Содержание";
        elements.heroPortrait.src = "assets/mk-dragon-logo.png";
        elements.heroPortrait.alt = "Эмблема Mortal Kombat";
        elements.fighterTitle.textContent = "Секреты и коды";
        elements.fighterSubtitle.textContent = "Аркадная UMK3 · справочник ввода";
        elements.leftPageNumber.textContent = "СЕКРЕТЫ";
        elements.rightPageNumber.textContent = "KOMBAT KODES";
        elements.bookNoteLabel.textContent = "Источник";
        elements.bookNoteValue.textContent = "Книга UMK3 · стр. 27–29";
        elements.categoryNav.innerHTML = sections.map(item => `
            <button class="category-link" type="button" data-category="${item[1]}">
                <span>${item[0]}</span><b>◆</b>
            </button>`).join("");
        elements.routeBar.innerHTML =
            `MOVEBOOK&nbsp;&nbsp;/&nbsp;&nbsp;${escapeHtml(versionLabels[state.version] || state.version)}` +
            `&nbsp;&nbsp;/&nbsp;&nbsp;<b>СЕКРЕТЫ И КОДЫ</b>`;
        elements.statusText.textContent = "Коды аркадной Ultimate Mortal Kombat 3";
        renderSecrets();
    }

    function renderCommunityPage() {
        hideMovePreview();
        document.documentElement.style.setProperty("--accent", "#b66b32");
        elements.fighterSeal.classList.remove("is-secrets");
        elements.fighterSeal.classList.add("is-community");
        elements.sectionTabs.hidden = true;
        elements.sectionTabs.parentElement.classList.add("is-global-page");
        elements.categoryNav.hidden = false;
        elements.heroPortrait.src = "assets/community/ori-logo.gif";
        elements.heroPortrait.alt = "Логотип сообщества Онлайн ретро-игры";
        elements.editionLabel.textContent = "СООБЩЕСТВО РЕТРО-ИГР";
        elements.fighterTitle.textContent = "Онлайн ретро-игры";
        elements.fighterSubtitle.textContent =
            "Играем вместе · сохраняем классику";
        elements.contentsTitle.textContent = "Сообщество";
        elements.leftPageNumber.textContent = "ОНЛАЙН РЕТРО-ИГРЫ";
        elements.rightPageNumber.textContent = "СВЯЗАТЬСЯ С НАМИ";
        elements.bookNoteLabel.textContent = "MK3 MoveBook";
        elements.bookNoteValue.textContent = "Сделано сообществом игроков";
        elements.categoryNav.innerHTML = `
            <button class="category-link" type="button"
                data-category="community-about">
                <span>О нас</span><b>◆</b>
            </button>
            <button class="category-link" type="button"
                data-category="community-links">
                <span>Telegram и Discord</span><b>◆</b>
            </button>`;
        elements.routeBar.innerHTML =
            `MOVEBOOK&nbsp;&nbsp;/&nbsp;&nbsp;<b>ОНЛАЙН РЕТРО-ИГРЫ</b>`;
        elements.comboFocus.hidden = true;
        elements.pageContent.innerHTML = `
            <div class="community-page">
                <section id="category-community-about"
                    class="community-intro">
                    <p class="community-kicker">СДЕЛАНО ИГРОКАМИ ДЛЯ ИГРОКОВ</p>
                    <h2>Онлайн ретро-игры</h2>
                    <p>
                        MK3 MoveBook создан сообществом «Онлайн ретро-игры».
                        Мы собираем поклонников классических игр, играем
                        вместе и помогаем ретро-играм оставаться живыми.
                    </p>
                </section>
                <section id="category-community-links"
                    class="community-links">
                    <a class="community-card is-telegram"
                        href="https://t.me/uPlay2ru" target="_blank"
                        rel="noopener noreferrer">
                        <img src="assets/community/telegram.webp"
                            alt="Telegram">
                        <span>
                            <small>Telegram</small>
                            <strong>@uPlay2ru</strong>
                            <em>Открыть канал</em>
                        </span>
                    </a>
                    <a class="community-card is-discord"
                        href="https://discord.com/invite/r3kCQNxX5Z"
                        target="_blank" rel="noopener noreferrer">
                        <img src="assets/community/discord.png"
                            alt="Discord">
                        <span>
                            <small>Discord</small>
                            <strong>Сервер сообщества</strong>
                            <em>Присоединиться</em>
                        </span>
                    </a>
                </section>
                <p class="community-footer-note">
                    Заходите знакомиться, находить соперников и играть
                    в любимую классику вместе.
                </p>
            </div>`;
        elements.pageContent.scrollTop = 0;
        elements.statusText.textContent =
            "MK3 MoveBook · сообщество Онлайн ретро-игры";
    }

    function renderMoves(fighter) {
        hideMovePreview();
        elements.comboFocus.hidden = true;
        elements.pageContent.innerHTML = fighter.categories.map((category, categoryIndex) => `
            <section class="chapter" id="category-${slug(category.name)}">
                <h2 class="chapter-heading">${escapeHtml(category.name)}</h2>
                <div class="move-list">
                    ${category.moves.map((move, moveIndex) => {
                        const key = `${categoryIndex}:${moveIndex}`;
                        return `
                            <article class="move-card" tabindex="0" data-move="${key}">
                                <div class="move-name">${escapeHtml(move.label || "Без названия")}</div>
                                <div class="move-command">${renderSequence(move.notation)}</div>
                            </article>`;
                    }).join("")}
                </div>
            </section>`).join("");
        const selectedCard = state.selectedMoveKey
            && elements.pageContent.querySelector(`[data-move="${state.selectedMoveKey}"]`);
        const initialKey = selectedCard
            ? state.selectedMoveKey
            : (fighter.categories[0]?.moves.length ? "0:0" : "");
        if (initialKey) {
            selectMove(initialKey);
        }
        elements.statusText.textContent = `${fighter.name} · книга приёмов`;
    }

    function renderHistory(fighter, fighterLore) {
        hideMovePreview();
        elements.comboFocus.hidden = true;
        const chapters = [];
        const historyHero = historyImageFighters.has(fighter.id)
            ? `<figure class="lore-hero">
                    <img src="assets/history/${fighter.id}.webp"
                        alt="Иллюстрация ${escapeHtml(fighter.name)}">
                </figure>`
            : "";
        if (fighterLore.intro) {
            chapters.push(`
                <section class="lore-chapter" id="category-lore-intro">
                    <div class="lore-chapter-heading">
                        <span>Глава I</span>
                        <h2>Роль в событиях UMK3</h2>
                        <em>${escapeHtml(fighterLore.sources?.intro || "")}</em>
                    </div>
                    <p>${escapeHtml(fighterLore.intro)}</p>
                </section>`);
        }
        if (fighterLore.ending) {
            chapters.push(`
                <section class="lore-chapter" id="category-lore-ending">
                    <div class="lore-chapter-heading">
                        <span>Глава II</span>
                        <h2>Финал персонажа</h2>
                        <em>${escapeHtml(fighterLore.sources?.ending || "")}</em>
                    </div>
                    <p>${escapeHtml(fighterLore.ending)}</p>
                </section>`);
        }
        if (fighterLore.note) {
            chapters.push(`
                <section class="lore-chapter lore-note" id="category-lore-note">
                    <div class="lore-chapter-heading">
                        <span>Примечание редакции</span>
                        <h2>Текст отсутствует в источнике</h2>
                    </div>
                    <p>${escapeHtml(fighterLore.note)}</p>
                </section>`);
        }
        elements.pageContent.innerHTML = `
            <section class="lore-page">
                <header class="editorial-heading">
                    <span>Летопись бойца</span>
                    <h2>${escapeHtml(fighter.name)}</h2>
                    <p>Краткая история и персональный финал переведены с английского текста книги.</p>
                </header>
                ${historyHero}
                ${chapters.join("")}
            </section>`;
        elements.statusText.textContent = `${fighter.name} · история персонажа`;
    }

    function renderFacts(fighter, fighterLore) {
        hideMovePreview();
        elements.comboFocus.hidden = true;
        const facts = fighterLore.facts || [];
        elements.pageContent.innerHTML = `
            <section class="lore-page" id="category-lore-facts">
                <header class="editorial-heading">
                    <span>Проверено по книге</span>
                    <h2>Факты о ${escapeHtml(fighter.name)}</h2>
                    <p>Только сведения, которые прямо следуют из предоставленного руководства.</p>
                </header>
                <div class="fact-grid">
                    ${facts.map((fact, index) => `
                        <article class="fact-card">
                            <b>${String(index + 1).padStart(2, "0")}</b>
                            <p>${escapeHtml(fact)}</p>
                        </article>`).join("")}
                </div>
                <footer class="source-caption">
                    ${escapeHtml(lore.source.note)}
                    ${fighterLore.sources?.facts ? ` Источник: ${escapeHtml(fighterLore.sources.facts)}.` : ""}
                </footer>
            </section>`;
        elements.statusText.textContent = `${fighter.name} · факты из книги`;
    }

    function renderSecrets() {
        const codes = lore.codes;
        elements.comboFocus.hidden = true;
        elements.pageContent.innerHTML = `
            <section class="codes-page">
                <div class="source-warning">
                    <strong>Важно о совместимости</strong>
                    <p>${escapeHtml(codes.note)}</p>
                </div>

                <section class="codes-section" id="category-codes-how">
                    <div class="codes-heading">
                        <span>Инструкция</span>
                        <h2>Как вводить Kombat Kodes</h2>
                    </div>
                    <ol class="code-steps">
                        ${codes.versusHelp.map(item => `<li>${escapeHtml(item)}</li>`).join("")}
                    </ol>
                    <div class="code-example">
                        <div>
                            <span>Пример из книги</span>
                            ${renderKode("987-123")}
                        </div>
                        <p><b>Player 1:</b> LP ×9, BL ×8, LK ×7<br><b>Player 2:</b> LP ×1, BL ×2, LK ×3</p>
                    </div>
                </section>

                <section class="codes-section" id="category-codes-symbols">
                    <div class="codes-heading">
                        <span>Визуальная легенда</span>
                        <h2>Что означают символы 0–9</h2>
                    </div>
                    <div class="symbol-grid">
                        ${Array.from({ length: 10 }, (_, number) => `
                            <figure>
                                <img src="assets/codes/symbol-${number}.png" alt="Символ ${number}">
                                <figcaption>${number}</figcaption>
                            </figure>`).join("")}
                    </div>
                    <p class="codes-explanation">Игра показывает картинки, а в справочнике код записан цифрами. Цифра под каждой картинкой — её точное значение.</p>
                </section>

                <section class="codes-section" id="category-codes-game">
                    <div class="codes-heading">
                        <span>Game Altering Codes</span>
                        <h2>Изменение правил боя</h2>
                    </div>
                    <div class="code-list">${renderCodeRows(codes.game)}</div>
                </section>

                <section class="codes-section" id="category-codes-arenas">
                    <div class="codes-heading">
                        <span>Kombat Zone</span>
                        <h2>Выбор арены</h2>
                    </div>
                    ${["Новые арены", "Оригинальные арены", "Секретные арены"].map(group => `
                        <h3 class="code-subheading">${group}</h3>
                        <div class="code-list">${renderCodeRows(codes.arenas.filter(item => item.group === group))}</div>
                    `).join("")}
                </section>

                <section class="codes-section" id="category-codes-messages">
                    <div class="codes-heading">
                        <span>Text Message Kodes</span>
                        <h2>Текстовые сообщения</h2>
                    </div>
                    <div class="code-list">
                        ${codes.messages.map(item => `
                            <article class="code-row">
                                <div><strong>${escapeHtml(item.translation)}</strong><small>${escapeHtml(item.name)}</small></div>
                                ${renderKode(item.code)}
                            </article>`).join("")}
                    </div>
                </section>

                <section class="codes-section" id="category-codes-opponents">
                    <div class="codes-heading">
                        <span>Winner Fights & 2 vs 2</span>
                        <h2>Секретные соперники и режимы</h2>
                    </div>
                    <div class="code-list">${renderCodeRows(codes.opponents)}</div>
                </section>

                <section class="codes-section" id="category-codes-unlocks">
                    <div class="codes-heading">
                        <span>Ultimate Kombat Kodes</span>
                        <h2>Открытие секретных бойцов</h2>
                    </div>
                    <ol class="code-steps compact">
                        ${codes.unlockHelp.map(item => `<li>${escapeHtml(item)}</li>`).join("")}
                    </ol>
                    <div class="unlock-grid">
                        ${codes.unlocks.map(item => `
                            <article class="unlock-card">
                                <h3>${escapeHtml(item.name)}</h3>
                                ${renderKode(item.code)}
                                <p>${escapeHtml(item.explanation)}</p>
                            </article>`).join("")}
                    </div>
                </section>
            </section>`;
    }

    function renderCodeRows(items) {
        return items.map(item => `
            <article class="code-row">
                <div>
                    <strong>${escapeHtml(item.name)}</strong>
                    ${item.original && item.original !== item.name
                        ? `<small>${escapeHtml(item.original)}</small>`
                        : ""}
                </div>
                ${renderKode(item.code)}
            </article>`).join("");
    }

    function renderKode(code) {
        return `<div class="kode" aria-label="Код ${escapeHtml(code)}">` +
            [...code].map(character => character === "-"
                ? `<span class="kode-separator">—</span>`
                : `<span class="kode-digit">${character}</span>`
            ).join("") +
            `</div>`;
    }

    function renderSequence(notation) {
        const pretty = notation
            .replaceAll("_8", "↑")
            .replaceAll("_2", "↓")
            .replaceAll("_4", "←")
            .replaceAll("_6", "→")
            .replaceAll("_+", "+");
        const tokens = pretty.match(/↑|↓|←|→|HP|LP|HK|LK|BL|RUN|hold|x\d+|\+|,|[^\s,+]+/gi) || [];
        return tokens.map(token => {
            if (token === ",") {
                return `<span class="sequence-separator">›</span>`;
            }
            if (token === "+") {
                return `<span class="sequence-separator">+</span>`;
            }
            if (/^\[(?:HP|LP|HK|LK|BL|RUN)(?:\/(?:HP|LP|HK|LK|BL|RUN))+\]$/i.test(token)) {
                const actions = token.slice(1, -1).split("/").map(action => action.toUpperCase());
                const labels = actions.map(action =>
                    state.notation === "sega" ? segaLabels[action] : action
                );
                const title = state.notation === "sega"
                    ? `Варианты кнопки: ${actions.join("/")} → ${labels.join("/")}`
                    : "Варианты кнопки";
                return `<span class="sequence-text sequence-alternative" ` +
                    `title="${escapeHtml(title)}">[${labels.join("/")}]</span>`;
            }
            const normalized = token.toUpperCase();
            if (["HP", "LP", "HK", "LK", "BL", "RUN"].includes(normalized)) {
                const label = state.notation === "sega"
                    ? segaLabels[normalized]
                    : normalized;
                const title = state.notation === "sega"
                    ? `${actionNames[normalized]}: ${normalized} → ${label}`
                    : actionNames[normalized];
                return `<span class="keycap ${normalized.toLowerCase()}" ` +
                    `data-action="${normalized}" title="${escapeHtml(title)}">${label}</span>`;
            }
            if (["↑", "↓", "←", "→"].includes(token)) {
                return `<span class="keycap direction">${token}</span>`;
            }
            return `<span class="sequence-text">${escapeHtml(token)}</span>`;
        }).join("");
    }

    function selectMove(key) {
        const fighter = currentFighter();
        const [categoryIndex, moveIndex] = key.split(":").map(Number);
        const category = fighter.categories[categoryIndex];
        const move = category?.moves[moveIndex];
        if (!move) {
            return;
        }
        state.selectedMoveKey = key;
        elements.pageContent.querySelectorAll(".move-card").forEach(card => {
            card.classList.toggle("is-selected", card.dataset.move === key);
        });
        elements.comboFocus.innerHTML = `
            <div class="combo-focus-header">
                <strong>${escapeHtml(move.label || "Комбинация")}</strong>
                <span>${escapeHtml(category.name)}</span>
            </div>
            <div class="combo-keys">${renderSequence(move.notation)}</div>`;
        elements.comboFocus.hidden = false;
        showMovePreview(fighter, category, move);
        elements.statusText.textContent = `${fighter.name} · ${move.label}`;
    }

    function moveAnimationKey(fighter, category, move) {
        const baseKey = [fighter.id, category.name, move.label]
            .map(value => String(value).trim().toLowerCase().replace(/\s+/g, " "))
            .join("|");
        const notationKey = [baseKey, move.notation]
            .map(value => String(value).trim().toLowerCase().replace(/\s+/g, " "))
            .join("|");
        return moveAnimations[notationKey] ? notationKey : baseKey;
    }

    function showMovePreview(fighter, category, move) {
        const source = moveAnimations[moveAnimationKey(fighter, category, move)];
        if (!source) {
            hideMovePreview();
            return;
        }

        if (elements.movePreviewVideo.dataset.source !== source) {
            elements.movePreviewVideo.dataset.source = source;
            elements.movePreviewVideo.src = source;
            elements.movePreviewVideo.load();
        }
        elements.movePreview.hidden = false;
        elements.movePreviewVideo.muted = true;
        const playback = elements.movePreviewVideo.play();
        if (playback && typeof playback.catch === "function") {
            playback.catch(() => {
                // Muted autoplay normally works; clicking the video retries playback.
            });
        }
    }

    function hideMovePreview() {
        elements.movePreviewVideo.pause();
        elements.movePreview.hidden = true;
    }

    function render() {
        renderVersionSelect();
        renderRoster();
        updateNotationSwitch();
        elements.communityButton.setAttribute(
            "aria-pressed",
            String(state.tab === "community")
        );
        renderFighterPage();
    }

    function updateNotationSwitch() {
        const segaSelected = state.notation === "sega";
        elements.notationToggle.setAttribute(
            "aria-checked",
            String(segaSelected)
        );
        elements.notationToggle.title = segaSelected
            ? "Сейчас SEGA: переключить на обозначения UMK3"
            : "Сейчас UMK3: переключить на обозначения SEGA";
    }

    function escapeHtml(value) {
        return String(value)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll("\"", "&quot;")
            .replaceAll("'", "&#039;");
    }

    function updateBridge() {
        return window.chrome && window.chrome.webview
            ? window.chrome.webview
            : null;
    }

    function renderChangelog() {
        const entry = changelogEntries[0];
        if (!entry) {
            elements.updatePanelVersion.textContent = "";
            elements.updateNotesTitle.textContent = "История изменений пока пуста";
            elements.updateNotesList.innerHTML = "";
            return;
        }
        elements.updatePanelVersion.textContent =
            `Версия ${entry.version} · ${entry.date}`;
        elements.updateNotesTitle.textContent = entry.title;
        elements.updateNotesList.innerHTML = (entry.items || [])
            .map(item => `<li>${escapeHtml(item)}</li>`)
            .join("");
    }

    function rememberLatestChangelog() {
        const entry = changelogEntries[0];
        if (!entry) {
            return;
        }
        try {
            localStorage.setItem("movebook-changelog-seen", entry.version);
        } catch {
            // The panel still works when storage is unavailable.
        }
    }

    function setUpdatePanel(open, remember = false) {
        elements.updatePanel.hidden = !open;
        elements.updateButton.setAttribute("aria-expanded", String(open));
        if (!open && remember) {
            rememberLatestChangelog();
        }
    }

    function showNewChangelogOnce() {
        const entry = changelogEntries[0];
        if (!entry) {
            return;
        }
        let seenVersion = "";
        try {
            seenVersion = localStorage.getItem("movebook-changelog-seen") || "";
        } catch {
            // Show the notes when storage is unavailable.
        }
        if (seenVersion !== entry.version) {
            setUpdatePanel(true);
        }
    }

    function applyUpdateStatus(message) {
        if (!message || message.type !== "movebook-update-status") {
            return;
        }
        const label = elements.updateButton.querySelector("b");
        elements.updateButton.dataset.state = message.state || "idle";
        elements.updateButton.dataset.action = message.action || "";
        label.textContent = message.state === "current"
            ? "Нет обновлений"
            : (message.label || "Обновления");
        elements.updateButton.title = message.details || label.textContent;
        elements.updatePanelStatus.textContent =
            message.details || "Состояние обновлений неизвестно.";
        elements.updateActionButton.dataset.action = message.action || "";
        elements.updateActionButton.disabled = !message.action;
        let actionLabel = message.label || "Подождите…";
        if (message.action === "install") {
            actionLabel = "Установить обновление";
        } else if (message.action === "download") {
            actionLabel = "Скачать последнюю версию";
        } else if (message.action === "check") {
            actionLabel = "Проверить ещё раз";
        }
        elements.updateActionButton.textContent = actionLabel;
        if (message.details) {
            elements.statusText.textContent = message.details;
        }
    }

    function initializeUpdater() {
        renderChangelog();
        showNewChangelogOnce();
        const bridge = updateBridge();
        if (!bridge) {
            applyUpdateStatus({
                type: "movebook-update-status",
                state: "idle",
                action: "download",
                label: "Обновления",
                details:
                    "Обновления веб-версии устанавливаются вручную."
            });
            return;
        }
        bridge.addEventListener("message", event => {
            applyUpdateStatus(event.data);
        });
        elements.updateButton.dataset.action = "check";
        bridge.postMessage("check-updates");
    }

    elements.versionSelect.addEventListener("change", event => {
        const keepSecretsOpen = state.tab === "secrets";
        state.version = event.target.value;
        state.fighterId = currentVersion().fighters[0].id;
        state.tab = keepSecretsOpen ? "secrets" : "moves";
        state.query = "";
        elements.searchInput.value = "";
        setRoute();
    });

    elements.searchInput.addEventListener("input", event => {
        state.query = event.target.value.trim();
        renderRoster();
    });

    elements.rosterList.addEventListener("click", event => {
        const card = event.target.closest("[data-fighter]");
        if (!card) {
            return;
        }
        state.fighterId = card.dataset.fighter;
        state.tab = "moves";
        state.selectedMoveKey = "";
        setRoute();
    });

    elements.communityButton.addEventListener("click", () => {
        state.tab = "community";
        state.selectedMoveKey = "";
        setRoute();
    });

    elements.updateButton.addEventListener("click", () => {
        setUpdatePanel(elements.updatePanel.hidden);
    });

    elements.updatePanelClose.addEventListener("click", () => {
        setUpdatePanel(false, true);
    });

    elements.updateActionButton.addEventListener("click", () => {
        const bridge = updateBridge();
        if (!bridge) {
            window.open(
                latestReleaseUrl,
                "_blank",
                "noopener,noreferrer"
            );
            elements.statusText.textContent =
                "Открыта страница последней версии";
            return;
        }
        const action = elements.updateActionButton.dataset.action === "install"
            ? "install-update"
            : "check-updates";
        bridge.postMessage(action);
    });

    elements.notationToggle.addEventListener("click", () => {
        state.notation = state.notation === "umk3" ? "sega" : "umk3";
        try {
            localStorage.setItem("movebook-notation", state.notation);
        } catch {
            // The selected notation still works for the current session.
        }
        render();
    });

    elements.sectionTabs.addEventListener("click", event => {
        const tab = event.target.closest("[data-tab]");
        if (!tab) {
            return;
        }
        state.tab = tab.dataset.tab;
        state.selectedMoveKey = "";
        setRoute();
    });

    elements.categoryNav.addEventListener("click", event => {
        const link = event.target.closest("[data-category]");
        const target = link && document.querySelector(`#category-${link.dataset.category}`);
        if (target) {
            target.scrollIntoView({ behavior: "smooth", block: "start" });
        }
    });

    elements.pageContent.addEventListener("click", event => {
        const card = event.target.closest("[data-move]");
        if (card) {
            selectMove(card.dataset.move);
        }
    });

    elements.pageContent.addEventListener("keydown", event => {
        const card = event.target.closest("[data-move]");
        if (card && (event.key === "Enter" || event.key === " ")) {
            event.preventDefault();
            selectMove(card.dataset.move);
        }
    });

    elements.movePreviewVideo.addEventListener("click", () => {
        if (elements.movePreviewVideo.paused) {
            elements.movePreviewVideo.play().catch(() => {});
        } else {
            elements.movePreviewVideo.pause();
        }
    });

    elements.backButton.addEventListener("click", () => history.back());
    elements.forwardButton.addEventListener("click", () => history.forward());
    document.addEventListener("click", event => {
        if (!elements.updatePanel.hidden &&
            !event.target.closest(".update-control")) {
            setUpdatePanel(false, true);
        }
    });
    document.addEventListener("keydown", event => {
        if (event.key === "Escape" && !elements.updatePanel.hidden) {
            setUpdatePanel(false, true);
        }
    });
    window.addEventListener("hashchange", () => {
        readRoute();
        render();
    });

    initializeUpdater();
    readRoute();
    setRoute(true);
})();

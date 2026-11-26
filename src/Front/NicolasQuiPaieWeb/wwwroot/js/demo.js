// Presenter mode, on the model of IP Directions' and Black Coach's demos: /demo (redirected to /?demo by
// NicolasQuiPaieWeb.Host) opens the home page under a curtain of four scenes, then the curtain lifts and a guided tour
// presents the platform, page after page. Keyboard or presentation remote (→ / ← / Page Down / Page Up), or the mouse.
// Loaded at the top of <body>, outside #app: Blazor never renders over it. The state lives in sessionStorage for the
// tab's lifetime; with no demo under way, the script stops at once. Texts in French: the demo is made for its audience.
(() => {
    const storageKey = 'nqp-demo';
    const root = document.documentElement;
    const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)');
    const calm = () => reducedMotion.matches && root.dataset.motion !== 'demo';

    const read = () => {
        try {
            return JSON.parse(sessionStorage.getItem(storageKey) ?? 'null');
        } catch {
            return null;
        }
    };
    const write = (state) => {
        if (!state) delete root.dataset.motion;
        try {
            if (state) sessionStorage.setItem(storageKey, JSON.stringify(state));
            else sessionStorage.removeItem(storageKey);
        } catch {
            // No storage: the demo lasts for this page only.
        }
    };

    // /demo typed on a host that does not redirect: the same as /?demo.
    if (location.pathname.toLowerCase() === '/demo') history.replaceState(null, '', '/?demo');
    const requested = new URL(location.href).searchParams.has('demo');

    const wait = (ms) => new Promise((resolve) => setTimeout(resolve, calm() ? 0 : ms));
    const forward = new Set(['ArrowRight', 'PageDown']);
    const backward = new Set(['ArrowLeft', 'PageUp']);
    const plain = (event) => !event.ctrlKey && !event.metaKey && !event.altKey && !event.shiftKey;
    const isEditable = (element) => element instanceof Element && element.closest('input, textarea, select, [contenteditable="true"]') !== null;

    const element = (tag, className, text) => {
        const node = document.createElement(tag);
        if (className) node.className = className;
        if (text !== undefined) node.textContent = text;
        return node;
    };

    // ======================================================================
    // The curtain: opening, principle, one Nicolas one vote, reveal
    // ======================================================================
    const scenes = [
        { id: 'intro', label: 'Ouverture' },
        { id: 'principle', label: 'Le principe' },
        { id: 'vote', label: 'Une voix chacun' },
        { id: 'reveal', label: 'En ligne' },
    ];

    const pillars = [
        ['Proposer', 'Une idée sur les dépenses publiques ou la fiscalité, rangée par catégorie.'],
        ['Voter', 'Pour ou contre, sur chaque proposition. Le résultat se lit en direct.'],
        ['Débattre', 'Des commentaires sous chaque proposition, et les tendances du moment.'],
    ];

    const badges = [
        ['🥉', 'Petit Nicolas', 'Première participation'],
        ['🥈', 'Gros Nicolas', 'Contributeur actif'],
        ['🏆', 'Nicolas Suprême', 'Expert reconnu'],
    ];

    // The number of proposals, asked to the API the front itself uses; without an answer the line stays generic.
    const countProposals = async () => {
        try {
            const settings = await (await fetch('/appsettings.json', { cache: 'no-store' })).json();
            const api = settings?.ApiSettings?.BaseUrl;
            if (!api) return null;
            const controller = new AbortController();
            setTimeout(() => controller.abort(), 4000);
            const response = await fetch(`${api.replace(/\/$/, '')}/api/proposals?take=100`, { signal: controller.signal });
            if (!response.ok) return null;
            const body = await response.json();
            const list = Array.isArray(body) ? body : body?.items ?? body?.data ?? null;
            return Array.isArray(list) ? list.length : null;
        } catch {
            return null;
        }
    };

    const buildStage = () => {
        const stage = element('dialog', 'demo-stage');
        stage.id = 'demo-stage';
        stage.lang = 'fr';
        stage.open = true;
        stage.setAttribute('aria-roledescription', 'démonstration');
        stage.setAttribute('aria-labelledby', 'demo-title');

        const backdrop = element('div', 'demo-backdrop');
        backdrop.setAttribute('aria-hidden', 'true');
        backdrop.append(element('span', 'demo-flag demo-flag--blue'), element('span', 'demo-flag demo-flag--red'));

        const top = element('header', 'demo-top');
        const brand = element('span', 'demo-brand');
        brand.append(element('i', 'fas fa-coins'), ' Nicolas Qui Paie ', element('span', 'demo-brand-sub', 'la plateforme citoyenne'));
        const live = element('span', 'demo-live');
        const clock = element('time');
        live.append(element('span', 'demo-led'), element('span', 'demo-live-label', 'démo en direct · '), clock);
        const quit = element('button', 'demo-quit', '✕');
        quit.type = 'button';
        quit.setAttribute('aria-label', 'Quitter la démo et aller au site');
        quit.title = 'Quitter la démo';
        top.append(brand, live, quit);

        const holder = element('div', 'demo-scenes');

        // 00 · Opening
        const intro = element('section', 'demo-scene scene-intro');
        const boot = element('pre', 'demo-boot');
        boot.setAttribute('aria-hidden', 'true');
        const bootLines = [
            '$ nicolas --session demo',
            '> connexion à l\'API citoyenne .............. OK',
            '> propositions chargées .................... OK',
            '> 1 Nicolas = 1 voix ....................... OK',
            '> chargement de la plateforme .............. 100 %',
        ].map((line) => element('samp', null, line));
        boot.append(...bootLines);
        const hello = element('div', 'demo-hello');
        const title = element('h2', 'demo-display');
        title.id = 'demo-title';
        title.append('Nicolas ', element('mark', null, 'Qui Paie'));
        const hint = element('p', 'demo-hint');
        hint.append(element('kbd', null, '→'), ' pour commencer');
        hello.append(
            element('p', 'demo-eyebrow', 'La démocratie souveraine numérique'),
            title,
            element('p', 'demo-lead', 'Une démonstration en trois temps : le principe, une voix chacun, puis la visite de la plateforme.'),
            hint,
        );
        intro.append(boot, hello);

        // 01 · Principle
        const principle = element('section', 'demo-scene scene-principle');
        principle.append(head('01', 'Le principe', 'Les citoyens décident de ce que paie Nicolas'));
        const pillarList = element('ol', 'demo-cards');
        pillars.forEach(([name, text]) => {
            const item = element('li');
            item.append(element('strong', null, name), element('span', null, text));
            pillarList.append(item);
        });
        principle.append(pillarList);

        // 02 · One Nicolas, one vote
        const vote = element('section', 'demo-scene scene-vote');
        vote.append(head('02', 'Une voix chacun', '1 Nicolas = 1 voix, quel que soit son badge'));
        const badgeList = element('ul', 'demo-badges');
        badges.forEach(([icon, name, text]) => {
            const item = element('li');
            item.append(element('span', 'demo-badge-icon', icon), element('strong', null, name), element('span', null, text), element('em', null, '× 1 voix'));
            badgeList.append(item);
        });
        vote.append(badgeList, element('p', 'demo-lead', 'Les badges récompensent la participation. Ils ne changent jamais le poids d\'un vote.'));

        // 03 · Reveal
        const reveal = element('section', 'demo-scene scene-reveal');
        const stripes = element('div', 'demo-stripes');
        stripes.setAttribute('aria-hidden', 'true');
        stripes.append(element('span'), element('span'), element('span'));
        const name = element('h2', 'demo-name', 'NICOLAS QUI PAIE');
        name.dataset.text = 'NICOLAS QUI PAIE';
        reveal.append(element('p', 'demo-eyebrow', '03 · En ligne'), stripes, name, element('p', 'demo-lead', 'La plateforme, en ligne. Place à la visite.'));

        const sceneNodes = [intro, principle, vote, reveal];
        sceneNodes.forEach((node, index) => { node.dataset.scene = scenes[index].id; });
        holder.append(...sceneNodes);

        const controls = element('footer', 'demo-controls');
        const pager = element('div', 'demo-pager');
        const previous = element('button', 'demo-step', '←');
        previous.type = 'button';
        previous.setAttribute('aria-label', 'Scène précédente');
        const next = element('button', 'demo-step demo-step--next', '→');
        next.type = 'button';
        next.setAttribute('aria-label', 'Scène suivante');
        const progress = element('ol', 'demo-progress');
        const steps = scenes.map((scene, index) => {
            const button = element('button');
            button.type = 'button';
            button.append(element('span', 'demo-num', String(index).padStart(2, '0')), ' ', element('span', 'demo-progress-label', scene.label));
            const item = element('li');
            item.append(button);
            progress.append(item);
            return button;
        });
        pager.append(previous, progress, next);
        const keys = element('p', 'demo-keys');
        keys.setAttribute('aria-hidden', 'true');
        keys.append(element('kbd', null, '→'), ' suivant · ', element('kbd', null, '←'), ' précédent · ', element('kbd', null, 'Échap'), ' aller au site');
        controls.append(pager, keys);

        const edge = element('span', 'demo-curtain-edge');
        edge.setAttribute('aria-hidden', 'true');

        stage.append(backdrop, top, holder, controls, edge);
        document.body.prepend(stage);

        countProposals().then((count) => {
            if (count !== null) bootLines[2].textContent = `> ${count === 100 ? '100+' : count} propositions chargées ${'.'.repeat(Math.max(3, 24 - String(count).length))} OK`;
        });

        return { stage, sceneNodes, steps, previous, next, quit, clock };

        function head(number, eyebrow, text) {
            const group = element('hgroup', 'demo-head');
            const label = element('p', 'demo-eyebrow');
            label.append(element('span', 'demo-num', number), ` ${eyebrow}`);
            group.append(label, element('h2', null, text));
            return group;
        }
    };

    const runStage = () => {
        const { stage, sceneNodes, steps, previous, next, quit, clock } = buildStage();
        const ids = sceneNodes.map((scene) => scene.dataset.scene);
        let index = Math.max(0, ids.indexOf(new URL(location.href).searchParams.get('demo') ?? ''));
        let revealTimer = 0;
        let lifting = false;

        if (typeof stage.showModal === 'function') {
            stage.close();
            stage.showModal();
        }

        const format = new Intl.DateTimeFormat('fr-FR', { timeZone: 'Europe/Paris', hour: '2-digit', minute: '2-digit', second: '2-digit' });
        const tick = () => { clock.textContent = format.format(new Date()); };
        tick();
        setInterval(tick, 1000);

        const lift = (withTour) => {
            if (lifting) return;
            lifting = true;
            clearTimeout(revealTimer);
            const instant = calm() || typeof stage.animate !== 'function';
            stage.dataset.lifting = '';
            history.replaceState(null, '', location.pathname + location.hash);
            write(withTour ? { tour: 0 } : null);
            const done = () => {
                stage.close();
                stage.remove();
                document.removeEventListener('keydown', onKey);
                if (withTour) startTour(0);
            };
            if (instant) {
                done();
                return;
            }
            stage.animate(
                [{ transform: 'translateY(0)' }, { transform: 'translateY(-100%)' }],
                { duration: 1300, easing: 'cubic-bezier(.76, 0, .24, 1)', fill: 'forwards' },
            ).finished.then(done, done);
        };

        const show = (target) => {
            if (lifting) return;
            index = Math.min(Math.max(target, 0), ids.length - 1);
            const id = ids[index];
            clearTimeout(revealTimer);
            sceneNodes.forEach((scene, position) => {
                const active = position === index;
                scene.classList.toggle('is-active', active);
                scene.inert = !active;
            });
            steps.forEach((button, position) => {
                button.classList.toggle('is-done', position < index);
                if (position === index) button.setAttribute('aria-current', 'step');
                else button.removeAttribute('aria-current');
            });
            stage.dataset.scene = id;
            previous.disabled = index === 0;
            history.replaceState(null, '', `${location.pathname}?demo=${id}`);
            write({ scene: id });
            if (id === 'reveal') revealTimer = setTimeout(() => lift(true), calm() ? 1200 : 6000);
        };

        const advance = () => (index === ids.length - 1 ? lift(true) : show(index + 1));

        const onKey = (event) => {
            if (!stage.open || !plain(event)) return;
            if (forward.has(event.key)) advance();
            else if (backward.has(event.key)) show(index - 1);
            else if (event.key === 'Home') show(0);
            else if (event.key === 'End') show(ids.length - 1);
            else return;
            event.preventDefault();
        };

        document.addEventListener('keydown', onKey);
        stage.addEventListener('cancel', (event) => {
            event.preventDefault();
            lift(false);
        });
        next.addEventListener('click', advance);
        previous.addEventListener('click', () => show(index - 1));
        steps.forEach((button, position) => button.addEventListener('click', () => show(position)));
        quit.addEventListener('click', () => lift(false));

        show(index);
        next.focus({ preventScroll: true, focusVisible: false });
    };

    // ======================================================================
    // The guided tour: a console, a spotlight that waits for Blazor to render
    // ======================================================================
    const spot = () => document.querySelector('.demo-spot')
        ?? document.body.appendChild(Object.assign(document.createElement('div'), { className: 'demo-spot', ariaHidden: 'true' }));
    let follow = 0;
    let searching = 0;

    // Blazor renders the page after the WebAssembly runtime has started: the target is looked for for a few seconds.
    const highlight = (selector) => {
        cancelAnimationFrame(follow);
        clearInterval(searching);
        const light = spot();
        light.classList.remove('is-on');
        if (!selector) return;
        const started = Date.now();
        const attach = () => {
            const target = document.querySelector(selector);
            if (!target) {
                if (Date.now() - started > 20000) clearInterval(searching);
                return;
            }
            clearInterval(searching);
            target.scrollIntoView({ block: 'center', behavior: calm() ? 'auto' : 'smooth' });
            const margin = 8;
            const place = () => {
                const box = target.isConnected ? target.getBoundingClientRect() : null;
                if (!box) {
                    // Blazor replaced the element: find it again.
                    highlight(selector);
                    return;
                }
                light.style.setProperty('--top', `${box.top - margin}px`);
                light.style.setProperty('--left', `${box.left - margin}px`);
                light.style.setProperty('--width', `${box.width + margin * 2}px`);
                light.style.setProperty('--height', `${box.height + margin * 2}px`);
                follow = requestAnimationFrame(place);
            };
            place();
            light.classList.add('is-on');
        };
        searching = setInterval(attach, 250);
        attach();
    };

    const tour = [
        {
            path: '/', title: 'Voici Nicolas Qui Paie',
            text: 'La plateforme, en ligne : on y propose, on y vote et on y débat de ce que paie Nicolas. → pour suivre la visite.',
        },
        {
            path: '/', target: '.hero-section', title: 'La promesse',
            text: 'Dès le premier écran : voter sur les dépenses publiques, proposer ses idées, participer.',
        },
        {
            path: '/', target: '.hero-stats', title: 'Les chiffres, en direct',
            text: 'Nicolas inscrits, votes exprimés, propositions actives : l\'état de la plateforme au premier coup d\'œil.',
        },
        {
            path: '/', target: '.nicolas-navbar', title: 'Tout en un menu',
            text: 'Accueil, propositions, tendances, et l\'espace de chacun une fois connecté.',
        },
        {
            path: '/proposals', target: '.container .card', title: 'Les propositions',
            text: 'Filtrées par catégorie, par recherche, et triées : plus récentes, plus populaires ou plus débattues.',
        },
        {
            path: '/trending', target: '.trending-header', title: 'Ce qui fait réagir',
            text: 'Les tendances sur une heure, six heures, un jour ou une semaine, avec le podium des propositions qui buzzent.',
        },
        {
            path: '/help/voting-system', target: '.card', title: 'Un Nicolas, une voix',
            text: 'Le système de vote expliqué : les badges récompensent l\'engagement, jamais le poids du vote.',
        },
        {
            path: '/about', target: '.card', title: 'Le projet',
            text: 'La mission et les principes : égalité des votes, transparence, débat constructif.',
        },
        {
            path: '/login', target: '.card', title: 'Un compte pour voter',
            text: 'Connexion et inscription dans l\'application. Proposer, voter et commenter demandent un compte.',
        },
        {
            path: '/', title: 'Merci !',
            text: 'Fin de la visite.',
            links: [['Rejouer la démo', '/demo'], ['Aller au site', '/']],
        },
    ];

    let panel = null;
    let position = -1;

    const onStepPage = (step) => location.pathname.toLowerCase() === step.path.toLowerCase();

    const quitTour = () => {
        cancelAnimationFrame(follow);
        clearInterval(searching);
        document.querySelector('.demo-spot')?.remove();
        panel?.element.remove();
        panel = null;
        write(null);
        document.removeEventListener('keydown', onTourKey);
    };

    const buildPanel = () => {
        const aside = element('aside', 'demo-tour');
        aside.lang = 'fr';
        aside.setAttribute('aria-label', 'Visite guidée');

        const headLine = element('p', 'demo-tour-head');
        const counter = element('output');
        headLine.append(element('span', 'demo-led'), 'visite guidée', counter);

        const body = element('div', 'demo-tour-body');
        body.setAttribute('aria-live', 'polite');
        const title = element('h2');
        const text = element('p', 'demo-tour-text');
        const links = element('p', 'demo-tour-links');
        body.append(title, text, links);

        const nav = element('div', 'demo-tour-nav');
        const button = (name, label, glyph) => {
            const control = element('button', null, glyph);
            control.type = 'button';
            control.dataset[name] = '';
            control.setAttribute('aria-label', label);
            control.title = label;
            return control;
        };
        const previous = button('tourPrev', 'Étape précédente', '←');
        const next = button('tourNext', 'Étape suivante', '→');
        const quit = button('tourQuit', 'Terminer la visite', '✕');
        const bar = element('span', 'demo-tour-bar');
        bar.append(element('span'));
        nav.append(previous, bar, next, quit);

        previous.addEventListener('click', () => goTo(position - 1));
        next.addEventListener('click', () => goTo(position + 1));
        quit.addEventListener('click', quitTour);

        aside.append(headLine, body, nav);
        document.body.append(aside);
        return { element: aside, counter, body, title, text, links, previous, next, bar };
    };

    const render = (index) => {
        const step = tour[index];
        panel ??= buildPanel();
        panel.counter.textContent = `${String(index + 1).padStart(2, '0')}/${String(tour.length).padStart(2, '0')}`;
        panel.title.textContent = step.title;
        panel.text.textContent = step.text;
        panel.links.replaceChildren(...(step.links ?? []).map(([label, href]) => Object.assign(element('a', null, label), { href })));
        panel.links.hidden = !step.links;
        panel.previous.disabled = index === 0;
        panel.next.disabled = index === tour.length - 1;
        panel.bar.firstElementChild.style.width = `${((index + 1) / tour.length) * 100}%`;
        panel.body.classList.remove('is-changing');
        void panel.body.offsetWidth;
        panel.body.classList.add('is-changing');
    };

    const goTo = (index) => {
        if (index < 0 || index >= tour.length || index === position) return;
        position = index;
        write({ tour: index });
        const step = tour[index];
        // A real navigation: the WebAssembly app restarts on the step's page and the tour picks up there.
        if (!onStepPage(step)) {
            location.assign(step.path);
            return;
        }
        render(index);
        highlight(step.target);
    };

    const onTourKey = (event) => {
        if (!plain(event) || isEditable(event.target)) return;
        const step = forward.has(event.key) ? 1 : backward.has(event.key) ? -1 : 0;
        if (step === 0) return;
        event.preventDefault();
        goTo(position + step);
    };

    const startTour = (index) => {
        position = Math.min(Math.max(index, 0), tour.length - 1);
        document.addEventListener('keydown', onTourKey);
        render(position);
        if (onStepPage(tour[position])) highlight(tour[position].target);
    };

    const begin = () => {
        if (requested) {
            runStage();
            return;
        }
        const state = read();
        if (Number.isInteger(state?.tour)) startTour(state.tour);
    };

    if (document.body) begin();
    else document.addEventListener('DOMContentLoaded', begin, { once: true });
})();

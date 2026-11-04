(() => {
	'use strict';

	const themeToggle = document.getElementById('themeToggle');
	const applyTheme = theme => {
		document.documentElement.dataset.theme = theme;
		const dark = theme === 'dark';
		if (themeToggle) {
			themeToggle.setAttribute('aria-label', dark ? 'Ativar modo claro' : 'Ativar modo noturno');
			themeToggle.setAttribute('title', dark ? 'Ativar modo claro' : 'Ativar modo noturno');
			const label = themeToggle.querySelector('.theme-toggle-label');
			if (label) label.textContent = dark ? 'Modo claro' : 'Modo noturno';
		}
	};

	applyTheme(document.documentElement.dataset.theme || 'dark');
	themeToggle?.addEventListener('click', () => {
		const nextTheme = document.documentElement.dataset.theme === 'dark' ? 'light' : 'dark';
		applyTheme(nextTheme);
		try { localStorage.setItem('hermenos-theme', nextTheme); } catch { /* O tema continua ativo nesta página. */ }
	});

	document.querySelectorAll('[data-loading-form]').forEach(form => {
		form.addEventListener('submit', () => {
			const submit = form.querySelector('button[type="submit"]');
			if (!submit || submit.disabled) return;

			const label = submit.dataset.loadingLabel || 'Carregando...';
			submit.disabled = true;
			submit.setAttribute('aria-busy', 'true');
			submit.innerHTML = `<span class="spinner-border spinner-border-sm" aria-hidden="true"></span> ${label}`;
		});
	});

	document.querySelectorAll('[data-bs-toggle="collapse"]').forEach(trigger => {
		const targetSelector = trigger.getAttribute('data-bs-target');
		if (!targetSelector) return;

		const target = document.querySelector(targetSelector);
		if (!target) return;

		target.addEventListener('shown.bs.collapse', () => {
			trigger.textContent = 'Ocultar';
			trigger.setAttribute('aria-expanded', 'true');
		});

		target.addEventListener('hidden.bs.collapse', () => {
			trigger.textContent = trigger.dataset.originalLabel || (trigger.dataset.originalLabel = trigger.textContent);
			trigger.setAttribute('aria-expanded', 'false');
		});

		trigger.dataset.originalLabel = trigger.textContent;
	});

	// Duplo clique em uma linha executa a ação de detalhes já existente nela.
	// Elementos interativos mantêm seu comportamento normal de clique.
	const getRowAction = row => row.querySelector('button[data-box-modal-target]')
		|| row.querySelector('button[data-bs-toggle="collapse"]')
		|| row.querySelector('a.icon-link')
		|| row.querySelector('a.primary-cell')
		|| row.querySelector('a[href]');
	const interactiveSelector = 'a, button, input, select, textarea, label, [role="button"]';
	document.querySelectorAll('.data-table tbody tr:not(.comparison-detail)').forEach(row => {
		const action = getRowAction(row);
		const detailUrl = row.dataset.rowDetailUrl;
		if (!action && !detailUrl) return;

		row.classList.add('has-row-details');
		row.addEventListener('dblclick', event => {
			if (event.target.closest(interactiveSelector)) return;
			if (detailUrl) {
				window.location.assign(detailUrl);
				return;
			}
			if (action && !action.disabled) action.click();
		});
	});

	// Tela cheia somente nos painéis solicitados pelo usuário.
	const boxSelectors = '[data-fullscreen-box]';
	const expandIcon = '<svg viewBox="0 0 24 24" aria-hidden="true"><path d="M4 9V4h5v2H6v3H4Zm14-3h-3V4h5v5h-2V6ZM6 15v3h3v2H4v-5h2Zm14 0h2v5h-5v-2h3v-3Z"/></svg>';
	let activeBox = null;
	let activeBackdrop = null;
	let activePlaceholder = null;
	let activeTrigger = null;
	const fullscreenButton = () => {
		const button = document.createElement('button');
		button.type = 'button';
		button.className = 'box-fullscreen-toggle';
		button.setAttribute('aria-expanded', 'false');
		return button;
	};
	const closeBoxModal = () => {
		if (!activeBox || !activeBackdrop || !activePlaceholder?.parentNode) return;
		const box = activeBox;
		const trigger = activeTrigger;
		activePlaceholder.replaceWith(box);
		activeBackdrop.remove();
		box.classList.remove('box-modal-content');
		if (box.id === 'cadastro-empresa-dialog') {
			box.hidden = true;
		}
		activeBox = null;
		activeBackdrop = null;
		activePlaceholder = null;
		activeTrigger = null;
		document.body.classList.remove('has-modal-box');
		updateFullscreenButtons();
		trigger?.setAttribute('aria-expanded', 'false');
		trigger?.focus();
	};
	const updateFullscreenButtons = () => {
		document.querySelectorAll('.box-fullscreen-toggle').forEach(button => {
			const box = button.closest(boxSelectors);
			button.innerHTML = expandIcon;
			button.setAttribute('aria-label', 'Abrir este conteúdo em um box maior');
			button.title = 'Ampliar conteúdo';
			button.setAttribute('aria-expanded', String(Boolean(box && activeBox === box)));
		});
	};
	const openBoxModal = (box, trigger) => {
		if (!box) return;

		// Se já existe um painel expandido, não fecha
		// ao abrir a comparação.
		const previousBox = activeBox;
		const previousBackdrop = activeBackdrop;
		const previousPlaceholder = activePlaceholder;
		const previousTrigger = activeTrigger;

		const placeholder = document.createComment('box-position');
		box.before(placeholder);

		const backdrop = document.createElement('div');
		backdrop.className = 'box-modal-backdrop';
		backdrop.setAttribute('role', 'presentation');

		const frame = document.createElement('div');
		frame.className = 'box-modal-frame';
		frame.setAttribute('role', 'dialog');
		frame.setAttribute('aria-modal', 'true');
		frame.setAttribute(
			'aria-label',
			box.querySelector('h2, h3')?.textContent?.trim()
			|| 'Comparação de documentos'
		);

		const modalToolbar = document.createElement('div');
		modalToolbar.className = 'box-modal-toolbar';

		const closeButton = document.createElement('button');
		closeButton.type = 'button';
		closeButton.className = 'box-modal-close';
		closeButton.setAttribute('aria-label', 'Fechar comparação');
		closeButton.title = 'Fechar';
		closeButton.textContent = '×';

		closeButton.addEventListener('click', () => {
			placeholder.replaceWith(box);
			backdrop.remove();
			placeholder.replaceWith(box);
			backdrop.remove();
			box.classList.remove('box-modal-content');

			if (box.id === 'cadastro-empresa-dialog') {
				box.hidden = true;
			}
			box.classList.remove('box-modal-content');
			trigger?.setAttribute('aria-expanded', 'false');

			// Restaura o painel que já estava expandido.
			activeBox = previousBox;
			activeBackdrop = previousBackdrop;
			activePlaceholder = previousPlaceholder;
			activeTrigger = previousTrigger;

			document.body.classList.toggle(
				'has-modal-box',
				Boolean(activeBox)
			);

			updateFullscreenButtons();
			trigger?.focus();
		});

		modalToolbar.append(closeButton);
		frame.append(modalToolbar, box);
		backdrop.append(frame);

		backdrop.addEventListener('click', event => {
			if (event.target === backdrop) closeButton.click();
		});

		box.classList.add('box-modal-content');
		trigger?.setAttribute('aria-expanded', 'true');
		document.body.append(backdrop);

		// Mantém a referência do modal mais recente.
		activeBox = box;
		activeBackdrop = backdrop;
		activePlaceholder = placeholder;
		activeTrigger = trigger;

		document.body.classList.add('has-modal-box');
		closeButton.focus();
		updateFullscreenButtons();
	};
	document.querySelectorAll(boxSelectors).forEach(box => {
		if (box.querySelector(':scope > .box-fullscreen-toggle, :scope > .box-fullscreen-toolbar')) return;
		const button = fullscreenButton();
		const heading = box.querySelector(':scope > .surface-heading, :scope > .tab-pane-heading');
		if (heading) {
			heading.classList.add('has-fullscreen-control');
			heading.append(button);
		} else {
			const toolbar = document.createElement('div');
			toolbar.className = 'box-fullscreen-toolbar';
			toolbar.append(button);
			box.prepend(toolbar);
		}

		button.addEventListener('click', event => {
			event.preventDefault();
			event.stopPropagation();
			openBoxModal(box, button);
		});
	});
	document.querySelectorAll('[data-box-modal-target]').forEach(trigger => {
		trigger.addEventListener('click', event => {
			event.preventDefault();
			const target = document.querySelector(trigger.dataset.boxModalTarget);
			openBoxModal(target, trigger);
		});
	});
	document.addEventListener('keydown', event => {
		if (event.key === 'Escape' && activeBox) closeBoxModal();
	});
	updateFullscreenButtons();

	(() => {
		const botaoAbrir = document.getElementById('abrir-cadastro-empresa');
		const formulario = document.getElementById('cadastro-empresa-dialog');

		if (botaoAbrir && formulario) {
			botaoAbrir.addEventListener('click', () => {
				formulario.hidden = false;
				openBoxModal(formulario, botaoAbrir);
			});
		}
	})();
})();

// Abre e fecha o menu lateral no desktop e lembra a escolha.
(() => {
	const botao = document.getElementById('sidebarCollapse');
	if (!botao) return;
	const raiz = document.documentElement;
	const atualizar = () => {
		const fechado = raiz.classList.contains('sidebar-closed');
		const texto = fechado ? 'Abrir menu lateral' : 'Fechar menu lateral';
		botao.setAttribute('aria-expanded', String(!fechado));
		botao.setAttribute('aria-label', texto);
		botao.setAttribute('title', texto);
	};
	botao.addEventListener('click', () => {
		raiz.classList.toggle('sidebar-closed');
		try { localStorage.setItem('hermenos-sidebar', raiz.classList.contains('sidebar-closed') ? 'closed' : 'open'); } catch { /* vale só nesta página */ }
		atualizar();
	});
	atualizar();
})();
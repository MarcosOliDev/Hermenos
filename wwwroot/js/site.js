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

	applyTheme(document.documentElement.dataset.theme || 'light');
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
})();

(() => {
	'use strict';

	const reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
	const easeOut = progress => 1 - Math.pow(1 - progress, 3);

	const animateCounters = () => {
		const elements = new Set(document.querySelectorAll('[data-contador], .metric-card > strong, .metric-content > strong, .summary-strip > div > strong'));
		elements.forEach(element => {
			const rawValue = element.dataset.contador ?? element.textContent.trim();
			if (!/^\d+$/.test(rawValue)) return;
			const target = Number(rawValue);
			if (!Number.isFinite(target)) return;

			if (reducedMotion) {
				element.textContent = target.toLocaleString('pt-BR');
				return;
			}

			const duration = 800;
			const startedAt = performance.now();
			element.textContent = '0';

			const update = now => {
				const progress = Math.min((now - startedAt) / duration, 1);
				element.textContent = Math.round(target * easeOut(progress)).toLocaleString('pt-BR');
				if (progress < 1) requestAnimationFrame(update);
			};

			requestAnimationFrame(update);
		});
	};

	const animatePageSections = () => {
		if (reducedMotion) return;
		const sections = document.querySelectorAll('.page-heading, .summary-strip, .metric-card, .surface, .dashboard-grid, .company-hero, .detail-metric-grid');
		sections.forEach((element, index) => {
			element.dataset.entrada = 'true';
			element.style.setProperty('--entrada-atraso', `${Math.min(index * 35, 210)}ms`);
		});
	};

	const animateProgress = () => {
		document.querySelectorAll('[data-progresso]').forEach(svg => {
			const circle = svg.querySelector('.progress-ring-value');
			if (!circle) return;

			const value = Math.max(0, Math.min(100, Number(svg.dataset.progresso || 0)));
			const circumference = 2 * Math.PI * 23;
			circle.style.strokeDasharray = String(circumference);
			circle.style.strokeDashoffset = String(circumference);

			requestAnimationFrame(() => {
				circle.style.transition = reducedMotion ? 'none' : 'stroke-dashoffset 850ms cubic-bezier(.2,.75,.25,1)';
				circle.style.strokeDashoffset = String(circumference * (1 - value / 100));
			});
		});
	};

	const animateBars = () => {
		document.querySelectorAll('[data-barra]').forEach(bar => {
			const value = Math.max(0, Math.min(100, Number(bar.dataset.barra || 0)));
			bar.style.width = reducedMotion ? `${value}%` : '0%';

			if (!reducedMotion) {
				requestAnimationFrame(() => {
					bar.style.transition = 'width 700ms cubic-bezier(.2,.75,.25,1)';
					bar.style.width = `${value}%`;
				});
			}
		});
	};

	const initialise = () => {
		animatePageSections();
		animateCounters();
		animateProgress();
		animateBars();
	};

	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', initialise, { once: true });
	} else {
		initialise();
	}
})();

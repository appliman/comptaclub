const printer = (() => {
	const print = (selector, bootstrapStyle) => {
		const html = document.querySelector(selector).outerHTML;
		const iframe = document.createElement('iframe');
		iframe.style.display = 'none';
		iframe.src = 'about:blank';
		iframe.onload = () => {
		};

		document.body.appendChild(iframe);
		const doc = iframe.contentWindow.document;

		doc.body.innerHTML = html;

		//const bootstrapLink = doc.createElement('link');
		//bootstrapLink.rel = 'stylesheet';
		//bootstrapLink.href = 'https://cdn.jsdelivr.net/npm/bootstrap@5.3.2/dist/css/bootstrap.min.css';
		//bootstrapLink.type = 'text/css';
		//// bootstrapLink.media = 'print';
		//doc.head.appendChild(bootstrapLink);

		const style = doc.createElement('style');
		style.innerHTML = bootstrapStyle;
		doc.head.appendChild(style);

		iframe.contentWindow.focus();
		iframe.contentWindow.location.reload();
		iframe.contentWindow.print();
		document.body.removeChild(iframe);
	};

	return {
		print
	};
})();

window.printer = window.printer || printer;
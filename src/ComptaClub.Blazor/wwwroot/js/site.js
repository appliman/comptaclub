const printer = (() => {
	const print = (selector, bootstrapStyle) => {
		const html = document.querySelector(selector).outerHTML;
		const iframe = document.createElement('iframe');
		iframe.style.display = 'none';
		iframe.src = 'about:blank';
		
		document.body.appendChild(iframe);
		const doc = iframe.contentWindow.document;

		doc.body.innerHTML = html;

		const style = doc.createElement('style');
		style.innerHTML = bootstrapStyle;
		doc.head.appendChild(style);

		iframe.contentWindow.focus();
		iframe.contentWindow.print();
		document.body.removeChild(iframe);
	};

	return {
		print
	};
})();

window.printer = window.printer || printer;
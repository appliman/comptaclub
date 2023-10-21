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

const screenInfo = ((selector) => {
	try {
		const element = document.querySelector(selector);
		if (typeof (element) === null || element === undefined) {
			return null;
		}
		const dimension = element.getBoundingClientRect();
		const result = {
			top: parseInt(dimension.top),
			bottom: parseInt(dimension.bottom),
			windowHeight: window.innerHeight,
			documentHeight: document.documentElement.scrollHeight
		};
		return result;
	} catch (ex) {
		console.log(ex);
		return null;
	}
})();

window.printer = window.printer || printer;
window.screeInfo = window.screeInfo || screenInfo;
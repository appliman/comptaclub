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

const camera = (() => {
	const startCamera = async (videoSelector) => {

		if (!navigator?.mediaDevices?.getUserMedia) {
			console.error("getUserMedia is not supported");
			return;
		}

		const constraints = {
			video: {
				width: 1280,
				height: 720
			},
			audio: false
		};
		const stream = await navigator.mediaDevices.getUserMedia(constraints);

		const videoElement = document.getElementById(videoSelector);
		videoElement.srcObject = stream;
		videoElement.play();
	};

	const stopCamera = (videoSelector) => {
		const videoElement = document.getElementById(videoSelector);
		if (videoElement) {
			try {
				console.log(videoElement);
				videoElement.pause();
				videoElement.srcObject.getTracks().forEach(track => track.stop());
			} catch(e) {
				console.error(e);
			}
		}
	};

	const takePicture = (videoSelector, canvasSelector, dotNetHelper) => {
		const videoElement = document.getElementById(videoSelector);
		const canvasElement = document.getElementById(canvasSelector);

		const context = canvasElement.getContext('2d');
		context.drawImage(videoElement, 0, 0, canvasElement.width, canvasElement.height);

		const dataUrl = canvasElement.toDataURL('image/jpeg');
		dotNetHelper.invokeMethodAsync("ProcessImage", dataUrl);
	};

	return {
		startCamera,
		stopCamera,
		takePicture
	};
})();

window.printer = window.printer || printer;
window.camera = window.camera || camera;
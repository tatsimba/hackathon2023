const layer = document.getElementById('layer-capture');
const captureBtn = document.getElementById('btn-capture');

export const showCaptureLayer = () => {
    layer?.classList.remove("hide");
}

export const onCaptureButtonClick = (fn: Parameters<typeof addEventListener>[1]) => {
    captureBtn?.addEventListener("click", fn);
}
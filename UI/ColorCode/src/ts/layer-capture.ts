const layer = document.getElementById('layer-capture');
const captureBtn = document.getElementById('btn-capture');
const restartBtn = document.getElementById('btn-restart');

export const showCaptureLayer = () => {
    layer?.classList.remove("hide");
}

export const onCaptureButtonClick = (fn: Parameters<typeof addEventListener>[1]) => {
    captureBtn?.addEventListener("click", fn);
}

export const onRestartButtonClick = (fn: Parameters<typeof addEventListener>[1]) => {
    restartBtn?.addEventListener("click", fn);
}

export const toggleCaptureButton = () => {
    captureBtn?.classList.toggle("hide");
}

export const toggleRestartButton = () => {
    restartBtn?.classList.toggle("hide");
}
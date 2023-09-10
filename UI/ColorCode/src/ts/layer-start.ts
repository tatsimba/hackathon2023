const layer = document.getElementById('layer-start');
const startBtn = document.getElementById('btn-start');

export const hideStartLayer = () => {
    layer?.classList.add("hide");
}

export const onStartButtonClick = (fn: Parameters<typeof addEventListener>[1]) => {
    startBtn?.addEventListener("click", fn);
}
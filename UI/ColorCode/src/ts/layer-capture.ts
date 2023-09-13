const layer = document.getElementById('layer-capture');
const captureBtn = document.getElementById('btn-capture');
const restartBtn = document.getElementById('btn-restart');
const timerElement = document.getElementById('timer');
const errorMessage = document.getElementById('error-message');

export const timer = (start = 3) => {
    return new Promise(resolve => {
        let count = start;

        timerElement!.classList.remove("countdown-animation", "hide");
        timerElement!.style.setProperty("--countdown-value", String(count));
        timerElement!.classList.add("countdown-animation");
        
        const interval = setInterval(() => {
            if(count === 0) {
                clearInterval(interval);
                timerElement!.classList.add("hide");
                resolve(count);
            }

            timerElement!.classList.remove("countdown-animation");
            timerElement!.style.setProperty("--countdown-value", String(--count));
            timerElement!.classList.add("countdown-animation");
        }, 1000);
    });
}

export const toggleErrorMessage = () => {
    errorMessage?.classList.toggle("hide");
}

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
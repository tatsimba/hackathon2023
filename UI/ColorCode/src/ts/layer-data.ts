import {canvas} from "./layer-segmentation";
const layer = document.getElementById("layer-data");

const matchResponse = document.getElementById("match-response");
const toggleMatchResponseBtn = matchResponse?.querySelector("#toggle-match-response");
const matchToggleArea = matchResponse?.querySelector("#match-toggle-area");

toggleMatchResponseBtn?.addEventListener("click", () => {
    matchToggleArea?.classList.toggle("hide");
});

const posLabel = (x: number, y: number) => {
    const {width, height} = canvas;
    const rect = canvas.getBoundingClientRect();
    const xRatio = rect.width / width;
    const yRatio = rect.height / height;

    return {
        x: x * xRatio,
        y: y * yRatio
    }
}

export const createLabel = (key: string, label: string, x: number, y: number) => {
    const {x: rX, y: rY} = posLabel(x, y)
    const labelElement = document.createElement("div");
    labelElement.classList.add("data-label");
    labelElement.textContent = `${label} ${key}`;
    labelElement.style.left = `min(${rX}px, 100vw)`;
    labelElement.style.top = `min(${rY}px, 100vw)`;
    layer?.appendChild(labelElement);
}

const clearAllLabels = () => {
    const labels = [...document.getElementsByClassName("data-label")];
    labels.forEach(label => label.remove());
}

export const showDataLayer = () => {
    layer?.classList.remove("hide");
}

export const hideDataLayer = () => {
    layer?.classList.add("hide");
    clearAllLabels();
}

export const setMatchResponse = (isMatch: boolean, response: string) => {
    if(isMatch) {
        matchResponse?.classList.remove("swap-outfit");
        matchResponse?.classList.add("perfect-match");
    } else {
        matchResponse?.classList.remove("perfect-match");
        matchResponse?.classList.add("swap-outfit");
    }

    matchToggleArea!.textContent = response;
}


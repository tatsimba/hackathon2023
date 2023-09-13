import {canvas} from "./layer-segmentation";
const layer = document.getElementById("layer-data");
const matchColorResponse = document.getElementById("match-color-response");
const matchWeatherResponse = document.getElementById("match-weather-response");

[matchColorResponse, matchWeatherResponse].forEach(el => {
    const toggleMatchResponseBtn = el?.querySelector("#toggle-match-response");
    const matchToggleArea = el?.querySelector("#match-toggle-area");

    toggleMatchResponseBtn?.addEventListener("click", () => {
        matchToggleArea?.classList.toggle("hide");
    });
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

const setMatchResponse = (el: HTMLElement, isMatch: boolean, response: string) => {
    if(isMatch) {
        el?.classList.remove("swap-outfit");
        el?.classList.add("perfect-match");
    } else {
        el?.classList.remove("perfect-match");
        el?.classList.add("swap-outfit");
    }

    const matchToggleArea = el?.querySelector("#match-toggle-area");

    matchToggleArea!.textContent = response;
}

export const setColorMatchResponse = (isMatch: boolean, response: string) => {
    setMatchResponse(matchColorResponse!, isMatch, response);
}

export const setWeatherMatchResponse = (isMatch: boolean, response: string) => {
    setMatchResponse(matchWeatherResponse!, isMatch, response);
}



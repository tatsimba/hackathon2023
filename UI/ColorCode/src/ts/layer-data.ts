import {canvas} from "./layer-segmentation";
const layer = document.getElementById("layer-data");

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
    labelElement.textContent = `${key}: ${label}`;
    labelElement.style.left = `${rX}px`;
    labelElement.style.top = `${rY}px`;
    layer?.appendChild(labelElement);
}

export const clearAllLabels = () => {
    const labels = [...document.getElementsByClassName("data-label")];
    labels.forEach(label => label.remove());
}


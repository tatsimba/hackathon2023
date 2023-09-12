const layer = document.getElementById("layer-loading");

export const toggleLoadingLayer = () => {
    layer?.classList.toggle("hide");
}
import SegmentationWorker from "./worker-segmentation?worker";
export const canvas = document.getElementById('segmentation') as HTMLCanvasElement;
const ctx = canvas.getContext('2d', {alpha: true});

const worker = new SegmentationWorker();

worker.addEventListener("message", (e) => {
    e.data && requestAnimationFrame(() => {
        ctx?.clearRect(0, 0, canvas.width, canvas.height);

        if(!e.data?.clear) {
            ctx?.drawImage(e.data, 0, 0);
        }
    });
});

export const clearSegmentationLayer = () => {
    worker.postMessage({clear: true});
}

export const drawSegmentation = (labels: number[], segmentation: number[][]) => {
    return new Promise(resolve => {
        worker.postMessage({
            width: canvas.width,
            height: canvas.height,
            segmentation,
            labels,
        });

        worker.addEventListener("message", (e) => {
            setTimeout(() => resolve(Boolean(e.data)));
        });
    });
}

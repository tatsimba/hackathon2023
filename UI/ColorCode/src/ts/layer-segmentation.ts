import SegmentationWorker from "./worker-segmentation?worker";
const canvas = document.getElementById('segmentation') as HTMLCanvasElement;
const ctx = canvas.getContext('2d', {alpha: true});

const worker = new SegmentationWorker();

export const clearSegmentationLayer = () => {
    ctx?.clearRect(0, 0, canvas.width, canvas.height);
}

export const drawSegmentation = (segmentation: number[][]) => {
    return new Promise(resolve => {
        worker.postMessage({
            width: canvas.width,
            height: canvas.height,
            segmentation
        });

        worker.onmessage = (e) => {
            e.data && ctx?.drawImage(e.data, 0, 0);
            resolve(Boolean(e.data));
        };
    });
}

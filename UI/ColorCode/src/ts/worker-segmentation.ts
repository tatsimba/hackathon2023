import {Matrix} from "./matrix";

const colorMap: {[key: number]: [number, number, number]} = {
    0: [230, 184, 175],   // Background (Light Reddish)
    1: [255, 200, 128],   // Hat (Orange)
    2: [145, 211, 199],   // Hair (Turquoise)
    3: [105, 176, 231],   // Sunglasses (Light Blue)
    4: [255, 214, 102],   // Upper-clothes (Yellow)
    5: [255, 153, 204],   // Skirt (Pink)
    6: [153, 255, 204],   // Pants (Light Green)
    7: [192, 192, 192],   // Dress (Gray)
    8: [255, 102, 102],   // Belt (Red)
    9: [102, 102, 255],   // Left-shoe (Blue)
    10: [255, 204, 153],  // Right-shoe (Light Orange)
    11: [230, 128, 255],  // Face (Lavender)
    12: [102, 255, 102],  // Left-leg (Bright Green)
    13: [153, 102, 51],   // Right-leg (Brown)
    14: [102, 102, 102],  // Left-arm (Dark Gray)
    15: [204, 204, 0],    // Right-arm (Dark Yellow)
    16: [128, 128, 255],  // Bag (Light Blue)
    17: [204, 153, 255]   // Scarf (Light Purple)
};

const drawSegmentation = (width: number, height: number, segmentation: number[][]) => {
    const matrix = new Matrix(width, height);
    const matrixSize = matrix.getSize();
    const seg = segmentation.flat(2);

    for(let i = 0; i < matrixSize; i += 4) {
        if(seg[i / 4] !== 0) continue;

        const [r, g, b] = colorMap[Number(seg[i / 4])];
        matrix.setPixel(i, [r, g, b, 100]);
    }

    for(let i = 0; i < matrixSize; i += 4) {
        const leftIndex = matrix.getLeftPixelIndex(i);
        const left = matrix.getPixel(leftIndex);

        const rightIndex = matrix.getRightPixelIndex(i);
        const right = matrix.getPixel(rightIndex);

        const topIndex = matrix.getTopPixelIndex(i);
        const top = matrix.getPixel(topIndex);

        const bottomIndex = matrix.getBottomPixelIndex(i);
        const bottom = matrix.getPixel(bottomIndex);

        const segment = seg[i / 4];
        const x = left[3] === 100 && segment !== 0;
        const y = right[3] === 100 && segment !== 0;
        const z = top[3] === 100 && segment !== 0;
        const w = bottom[3] === 100 && segment !== 0;

        if(x || y || z || w) {
            const color: [number, number, number, number] = [...colorMap[segment], 255];

            matrix.setPixel(i, color);
            matrix.setPixel(leftIndex, color);
            matrix.setPixel(rightIndex, color);
            matrix.setPixel(topIndex, color);
            matrix.setPixel(bottomIndex, color);
        }
    }

    const offscreen = new OffscreenCanvas(width, height);
    const offscreenContext = offscreen.getContext('2d');

    const segImg = offscreenContext?.createImageData(width, height);
    segImg?.data.set(matrix.getMatrix());
    segImg && offscreenContext?.putImageData(segImg, 0, 0);

    return offscreen;
};

self.addEventListener("message", (e) => {
    const {width, height, segmentation} = e.data;
    const offscreen = drawSegmentation(width, height, segmentation);

    self.postMessage(offscreen.transferToImageBitmap());
});

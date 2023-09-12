const PATTERN_SIZE = 9;

const pattern = (pos = 0) => {
    const size = PATTERN_SIZE;
    const ptr = new OffscreenCanvas(size, size)!;
    const ctx = ptr.getContext('2d')!;
     
    ctx.fillStyle = "white";

    const y = size / 3; 
    const x = 3 + pos % size;
 
    ctx.fillRect(x, x, y, y); 

    return ctx.createPattern(ptr, 'repeat')!;
}

const drawPattern = (labels: number[], width: number, height: number, segmentation: number[][], pos: number) => {
    const ptr = pattern(pos);
    const canvas = new OffscreenCanvas(width, height)!;
    const ctx = canvas.getContext('2d')!;

    for(let i = 0; i < segmentation.length; i++) {
        for(let j = 0; j < segmentation[i].length; j++) {
            if(labels.includes(segmentation[i][j])) {                
                ctx.fillStyle = ptr;
                ctx.fillRect(j, i, 1, 1);
            }
        }
    }

    return canvas;
}

let animationFrameID: number;

self.addEventListener("message", (e) => {
    const {width, height, segmentation, labels, clear = false} = e.data;
    let pos = 0;

    if(clear) {
        cancelAnimationFrame(animationFrameID);
        self.postMessage({clear: true});
        return;
    }
    
    const draw = () => {
        const offscreen = drawPattern(labels, width, height, segmentation, pos);
        self.postMessage(offscreen.transferToImageBitmap());   
        pos = ++pos % PATTERN_SIZE;
        animationFrameID = requestAnimationFrame(draw);
    }

    animationFrameID = requestAnimationFrame(draw);
});

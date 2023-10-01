import { isMobile } from "./mobile";

const {VITE_IMAGE_TYPE} = import.meta.env;
const canvas = document.getElementById('video') as HTMLCanvasElement;
const ctx = canvas.getContext('2d', {alpha: false})!;
const video = document.createElement('video');
video.muted = true;
video.autoplay = true;
video.playsInline = true;
// document.body.appendChild(video);
// video.style.cssText = `position: absolute; top: 0; left: 0; z-index: 999; opacity: 0.5; transform: scale(-1, 1);` 

const getVideoPermission = async () => {
    try {
        const constraints = {
            audio: false,
            video: {
                width: isMobile ? undefined : window.outerWidth * window.devicePixelRatio,
                height: isMobile ? undefined : window.outerHeight * window.devicePixelRatio,
                facingMode: { ideal: 'user' }
            }
        };
    
        return await navigator.mediaDevices.getUserMedia(constraints);
    } catch (e) {
        throw new Error('No camera access');
    }
}

const rotate = new URLSearchParams(location.search).get("rotate") === "true";

const drawVideoFrame = () => {
    if(!video.paused && !video.ended) {

        if(isMobile) {
            const vW = video.videoWidth;
            const vH = video.videoHeight;

            ctx.save();
            ctx.scale(-1, 1);
            ctx.drawImage(video, -vW, 0, vW, vH);
            ctx.restore();

        } else if(!rotate) {
            ctx?.scale(-1, 1);
            ctx?.drawImage(video, -canvas.width, 0, canvas.width, canvas.height);

        } else {
            const r = video.videoWidth / video.videoHeight;
            const w = window.innerWidth * window.devicePixelRatio * r;
            const h = window.innerHeight * window.devicePixelRatio;

            ctx?.save();
            ctx?.translate(w / 2, h / 6);
            ctx?.rotate(90 * Math.PI / 180);
            ctx?.scale(-1, 1);
            ctx?.drawImage(video, -h / 2, -w / 2, h, w);
            ctx?.restore();
        }
    }
    
    requestAnimationFrame(drawVideoFrame);
}

const setCanvasesSize = (width: number, height: number) => {
    const canvases = document.querySelectorAll('canvas');

    canvases.forEach(canvas => {
        canvas.width = width;
        canvas.height = height;
    });
}

export const startVideo = async () => {
    const stream = await getVideoPermission();
    const {width = window.innerWidth, height = window.innerHeight} = stream.getVideoTracks()[0].getSettings();

    setCanvasesSize(width, height);

    video.srcObject = stream;
    video.play();
    drawVideoFrame();
}

export const pauseVideo = () => {
    video.pause();
}

export const playVideo = () => {
    video.play();
}

export const captureVideoFrame = () => {
    return new Promise<Blob | null>(resolve => {        
        canvas.toBlob(resolve, `image/${VITE_IMAGE_TYPE}`, 1);
    });
}

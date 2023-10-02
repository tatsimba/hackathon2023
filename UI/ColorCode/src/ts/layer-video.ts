import { isMobile } from "./mobile";

const {VITE_IMAGE_TYPE} = import.meta.env;
const canvas = document.getElementById('video') as HTMLCanvasElement;
const ctx = canvas.getContext('2d', {alpha: false})!;
const video = document.createElement('video');
video.muted = true;
video.autoplay = true;
video.playsInline = true;

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
        if(canvas.width != video.videoWidth || canvas.height != video.videoHeight) {
            setCanvasesSize(video.videoWidth, video.videoHeight);
        }

        ctx.save();
        ctx.scale(-1, 1);
        ctx.drawImage(video, -canvas.width, 0, canvas.width, canvas.height);
        ctx.restore();
    }
    
    requestAnimationFrame(drawVideoFrame);
}

const setCanvasesSize = (width: number, height: number) => {
    const canvases = document.querySelectorAll('canvas');

    canvases.forEach(canvas => {
        canvas.width = width;
        canvas.height = height;
        
        if(rotate) {
           canvas.classList.add("rotate");
        }
    });
}


export const startVideo = async () => {
    const stream = await getVideoPermission();
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

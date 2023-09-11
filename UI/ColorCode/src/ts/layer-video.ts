const {VITE_IMAGE_TYPE} = import.meta.env;
const canvas = document.getElementById('video') as HTMLCanvasElement;
const ctx = canvas.getContext('2d', {alpha: false});
const video = document.createElement('video');

const getVideoPermission = async () => {
    const constraints = {
        audio: false,
        video: {
            width: window.outerWidth,
            height: window.outerHeight,
            facingMode: { ideal: "user" }
        }
    };

    return await navigator.mediaDevices.getUserMedia(constraints);
}

const drawVideoFrame = () => {
    if(!video.paused && !video.ended) {
        ctx?.scale(-1, 1);
        ctx?.drawImage(video, -canvas.width, 0, canvas.width, canvas.height);
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
    const {width = window.outerWidth, height = window.outerHeight} = stream.getVideoTracks()[0].getSettings();

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

const {VITE_IMAGE_TYPE} = import.meta.env;
const canvas = document.getElementById('video') as HTMLCanvasElement;
const ctx = canvas.getContext('2d');
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
    ctx?.scale(-1, 1);
    ctx?.drawImage(video, -canvas.width, 0, canvas.width, canvas.height);
    requestAnimationFrame(drawVideoFrame);
}

export const startVideo = async () => {
    const stream = await getVideoPermission();
    const {width, height} = stream.getVideoTracks()[0].getSettings();

    if(width && height) {
        canvas.width = width * window.devicePixelRatio;
        canvas.height = height * window.devicePixelRatio;
    }

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
const {VITE_IMAGE_TYPE} = import.meta.env;
const video = document.getElementById('video') as HTMLVideoElement;

const getVideoPermission = async () => {
    const constraints = {
        audio: false,
        video: {
            facingMode: { ideal: "user" }
        }
    };
    const stream = await navigator.mediaDevices.getUserMedia(constraints);
    return stream;
}

export const startVideo = async () => {
    const stream = await getVideoPermission();
    video.srcObject = stream;
    video.play();
}

export const pauseVideo = () => {
    video.pause();
}

export const playVideo = () => {
    video.play();
}

export const captureVideoFrame = () => {
    return new Promise<Blob | null>(resolve => {
        const canvas = document.createElement("canvas");
        canvas.width = video.videoWidth;
        canvas.height = video.videoHeight;
        const ctx = canvas.getContext('2d');
        
        ctx?.drawImage(video, 0, 0, video.videoWidth, video.videoHeight);
        
        canvas.toBlob(resolve, `image/${VITE_IMAGE_TYPE}`, 1);
    });
}
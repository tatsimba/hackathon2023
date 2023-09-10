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
    if(video.paused || video.ended) return;
    
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

export const drawSegmentation = (segmentation: number[][]) => {
    const imageData = ctx?.getImageData(0, 0, canvas.width, canvas.height);
    const data = imageData?.data;
    const seg = segmentation.flat();
    console.log(seg.length, Number(data?.length) / 4);
    ctx?.clearRect(0, 0, canvas.width, canvas.height)

    if(data) {
        for(let i = 0; i < data.length; i += 4) {
            if(seg[i / 4] === 0) continue;

            data[i] = 255;
            // data[i + 1] = 0;
            // data[i + 2] = 0;
            // data[i + 3] = Math.floor(Math.random() * 255);
            data[i + 3] = 255/2;
        }
    }

    imageData && ctx?.putImageData(imageData, 0, 0);
}
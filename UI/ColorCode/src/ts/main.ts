import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame, drawSegmentation} from "./layer-video";
import {downloadImage, sendImage} from "./api";


onStartButtonClick(async () => {
    await Promise.all([
        startVideo(),
        document.documentElement.requestFullscreen()
    ]);

    hideStartLayer();
    showCaptureLayer();
});

onCaptureButtonClick(async () => {
    pauseVideo();
    const blob = await captureVideoFrame();
    // blob && downloadImage(blob);
    blob && sendImage(blob).then(async res => {
        const json = await res.json();
        drawSegmentation(json.imageSegmentationLabels);
    });
});
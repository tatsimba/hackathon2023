import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame} from "./layer-video";
import {sendImage} from "./api";

console.log(import.meta.env)

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
    blob && sendImage(blob);
});
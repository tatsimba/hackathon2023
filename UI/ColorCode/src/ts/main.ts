import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame} from "./layer-video";
import {drawSegmentation} from "./layer-segmentation";
import {sendImage} from "./api";


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

    blob && sendImage(blob).then(async res => {
        const json = await res.json();

        console.time("drawSegmentation");
        await drawSegmentation(json.imageSegmentationLabels);
        console.timeEnd("drawSegmentation");
    });
});
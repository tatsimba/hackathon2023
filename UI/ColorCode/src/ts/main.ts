import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame} from "./layer-video";
import {drawSegmentation} from "./layer-segmentation";
import {imageAnalyzeRequest, segmentationRequest} from "./api";


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

    blob && segmentationRequest(blob).then(async res => {
        const json = await res.json();

        const [analyze] = await Promise.all([
            imageAnalyzeRequest(blob),
            drawSegmentation(json.imageSegmentationLabels),
        ]);

        console.log(analyze)
    });
});
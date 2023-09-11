import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer, onRestartButtonClick, toggleCaptureButton, toggleRestartButton} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame, playVideo} from "./layer-video";
import {drawSegmentation, clearSegmentationLayer} from "./layer-segmentation";
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
    toggleCaptureButton();

    const blob = await captureVideoFrame();
    if(!blob) {
        alert("Failed to capture video frame");
        return;
    };

    await Promise.all([
        segmentationRequest(blob).then(res => res.json()).then(json => json.imageSegmentationLabels).then(drawSegmentation),
        imageAnalyzeRequest(blob),
    ]);
    
    toggleRestartButton();
});

onRestartButtonClick(() => {
    toggleRestartButton();
    clearSegmentationLayer();
    playVideo();
    toggleCaptureButton();
});
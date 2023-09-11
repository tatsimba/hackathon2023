import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer, onRestartButtonClick, toggleCaptureButton, toggleRestartButton} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame, playVideo} from "./layer-video";
import {drawSegmentation, clearSegmentationLayer} from "./layer-segmentation";
import {createLabel, clearAllLabels} from "./layer-data";
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

    const [segmentation, analyze] = await Promise.all([
        segmentationRequest(blob).then(res => res.json()),
        imageAnalyzeRequest(blob).then(res => res.json()),
    ]);

    drawSegmentation(segmentation.imageSegmentationLabels);

    const results = JSON.parse(analyze.result);
    const positions = segmentation.boxes;

    for(const pos of positions) {
        const value = results[pos.label];

        if(value && value !== "unknown") {
            createLabel(pos.label, value, pos.x, pos.y);
        }
    }

    toggleRestartButton();
});

onRestartButtonClick(() => {
    toggleRestartButton();
    clearAllLabels();
    clearSegmentationLayer();
    playVideo();
    toggleCaptureButton();
});
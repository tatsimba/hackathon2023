import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer, onRestartButtonClick, toggleCaptureButton, toggleRestartButton} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame, playVideo} from "./layer-video";
import {drawSegmentation, clearSegmentationLayer} from "./layer-segmentation";
import {createLabel, clearAllLabels} from "./layer-data";
import {imageAnalyzeRequest, segmentationRequest} from "./api";

const fullScreen = async () => {
    const query = new URLSearchParams(location.search);

    if(query.get("fullScreen") !== "false") {
        await document.documentElement.requestFullscreen();
    }
}

const onCapture = async () => {
    try {
        pauseVideo();
        toggleCaptureButton();
    
        const blob = await captureVideoFrame();
        if(!blob) {
            throw new Error("Failed to capture video frame");
        }
    
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
    } catch(e) {
        alert(e);
    }
}

const onRestart = () => {
    toggleRestartButton();
    clearAllLabels();
    clearSegmentationLayer();
    playVideo();
    toggleCaptureButton();
}

const onStart = async () => {
    try {
        await Promise.all([
            startVideo(),
            fullScreen(),
        ]);
    
        hideStartLayer();
        showCaptureLayer();
    } catch(e) {
        alert(e);
    }
}

window.addEventListener("keydown", (e) => {
    if(e.key === "Enter" || e.key === "c") {
        onCapture();
    
    } else if(e.key === "Escape" || e.key === "r") {
        onRestart();
    }
});

onCaptureButtonClick(onCapture);
onRestartButtonClick(onRestart);
onStartButtonClick(onStart);

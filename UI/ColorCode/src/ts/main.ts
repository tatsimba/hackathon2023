import JSConfetti from "js-confetti";
import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer, onRestartButtonClick, toggleCaptureButton, toggleRestartButton} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame, playVideo} from "./layer-video";
import {drawSegmentation, clearSegmentationLayer} from "./layer-segmentation";
import {createLabel, hideDataLayer, setMatchResponse, showDataLayer} from "./layer-data";
import {imageAnalyzeRequest, segmentationRequest} from "./api";

const jsConfetti = new JSConfetti()

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
    
        const [segmentation, analyze] = await Promise.all([
            segmentationRequest(blob!).then(res => res.json()),
            imageAnalyzeRequest(blob!).then(res => res.json()),
        ]);
    
        drawSegmentation(segmentation.imageSegmentationLabels);

        if(!analyze.success) {
            throw new Error("Failed to analyze image pls try again in 30s");
        }
    
        const garmentColorResult = JSON.parse(analyze.garmentColorResult);
        const matchingColorResult = JSON.parse(analyze.matchingColorResult);
        const positions = segmentation.boxes;

        const isMatching = matchingColorResult.matching === "matching";

        isMatching && jsConfetti.addConfetti();
    
        setMatchResponse(
            isMatching,
            matchingColorResult.result
        );

        for(const pos of positions) {
            const value = garmentColorResult[pos.label];
    
            if(value && value !== "unknown") {
                createLabel(pos.label, value, pos.x, pos.y);
            }
        }

        showDataLayer();
        toggleRestartButton();
    } catch(e) {
        alert(e);
    }
}

const onRestart = () => {
    toggleRestartButton();
    hideDataLayer();
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

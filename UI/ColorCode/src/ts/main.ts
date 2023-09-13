import JSConfetti from "js-confetti";
import {onStartButtonClick, hideStartLayer} from "./layer-start";
import {onCaptureButtonClick, showCaptureLayer, onRestartButtonClick, toggleCaptureButton, toggleRestartButton, timer, toggleErrorMessage} from "./layer-capture";
import {startVideo, pauseVideo, captureVideoFrame, playVideo} from "./layer-video";
import {drawSegmentation, clearSegmentationLayer} from "./layer-segmentation";
import {toggleLoadingLayer} from "./layer-loading";
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
        toggleCaptureButton();
        await timer(3);
        pauseVideo();
        toggleLoadingLayer();
        
        const blob = await captureVideoFrame();
    
        const [segmentation, analyze] = await Promise.all([
            segmentationRequest(blob!).then(res => res.json()),
            imageAnalyzeRequest(blob!).then(res => res.json()),
        ]);

        if(!analyze.success) {
            throw new Error("Failed to analyze image pls try again in 30s");
        }
    
        const {
            matchingWearing,
            // matchingWeather,
            nonMatchingGarmentsWearing,
            // nonMatchingGarmentsWeather,
            resultGarmentsColors,
            resultWearing,
            // resultWeather
         } = JSON.parse(analyze.result);

        const positions = segmentation.boxes;

        setMatchResponse(
            matchingWearing,
            resultWearing
        );

        const map: {[key: string]: string} = {
            pants: "pants",
            shorts: "pants",
            shirt: "upper clothing",
            shoes: "shoes",
            dress: "upper clothing",
            coat: "upper clothing",
            jacket: "upper clothing",
            scarf: "scarf"
        }

        for(const [key, val] of Object.entries(resultGarmentsColors)) {
            const pos = positions[key] || positions[map[key]];

            if(pos) {
                createLabel(key, String(val), pos.x, pos.y);
            }
        }

        const labels = nonMatchingGarmentsWearing?.map((label: string) => {
            const pos = positions[label] || positions[map[label]];
            return pos?.numerical_labels_values || [];
        }).flat();

        await drawSegmentation(labels, segmentation.imageSegmentationLabels);
        toggleLoadingLayer();

        matchingWearing && jsConfetti.addConfetti();


        showDataLayer();
        toggleRestartButton();
    } catch(e) {
        console.error(e);
        onRestart();
        toggleLoadingLayer();
        toggleCaptureButton();
        toggleErrorMessage();
        await timer(30).then(() => toggleErrorMessage());
        toggleCaptureButton(); 
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

onCaptureButtonClick(onCapture);
onRestartButtonClick(onRestart);
onStartButtonClick(onStart);

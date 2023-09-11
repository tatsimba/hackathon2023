const {VITE_IMAGE_TYPE, VITE_SEGMENTATION_API, VITE_IMAGE_ANALYZE_API} = import.meta.env;

export const segmentationRequest = (image: Blob, filename=`image.${VITE_IMAGE_TYPE}`) => {
    const formData = new FormData();
    formData.append("image", image, filename);
    
    return fetch(VITE_SEGMENTATION_API, {
        method: "POST",
        body: formData
    });
}

export const imageAnalyzeRequest = (image: Blob, filename=`image.${VITE_IMAGE_TYPE}`) => {
    const formData = new FormData();
    formData.append("image", image, filename);
    
    const headers = new Headers();
    headers.append("ApiKey", "ColorCodeHackKey");

    return fetch(VITE_IMAGE_ANALYZE_API, {
        method: "POST",
        body: formData,
        headers
    });
}


export const downloadImage = (image: Blob, filename=`image.${VITE_IMAGE_TYPE}`) => {
    const a = document.createElement("a");
    a.href = URL.createObjectURL(image);
    a.download = filename;
    a.click();
}
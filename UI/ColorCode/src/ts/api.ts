const {VITE_IMAGE_TYPE, VITE_SEGMENTATION_API} = import.meta.env;

export const sendImage = (image: Blob, filename=`image.${VITE_IMAGE_TYPE}`) => {
    const formData = new FormData();
    formData.append('image', image, filename);
    
    return fetch(VITE_SEGMENTATION_API, {
        method: 'POST',
        body: formData
    });
}
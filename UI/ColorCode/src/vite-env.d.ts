/// <reference types="vite/client" />

interface ImportMetaEnv {
    readonly VITE_IMAGE_TYPE: "png" | "jpeg";
    readonly VITE_SEGMENTATION_API: string;
    readonly VITE_IMAGE_ANALYZE_API: string;
}

interface ImportMeta {
    readonly env: ImportMetaEnv
}
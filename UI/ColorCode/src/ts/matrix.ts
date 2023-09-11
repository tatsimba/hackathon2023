export class Matrix {
    private matrix: Uint8ClampedArray;

    constructor(private width: number, private height: number) {
        this.matrix = new Uint8ClampedArray(this.width * this.height * 4);
    }

    getMatrix() {
        return this.matrix;
    }

    getSize() {
        return this.getMatrix().length;
    }

    getPixel(i: number) {
        return this.matrix.slice(i, i + 4);
    }

    setPixel(i: number, color: [number, number, number, number]) {
        this.matrix[i] = color[0];
        this.matrix[i + 1] = color[1];
        this.matrix[i + 2] = color[2];
        this.matrix[i + 3] = color[3];
    }

    getTopPixelIndex(i: number) {
        return i - this.width*4;
    }

    getBottomPixelIndex(i: number) {
        return i + this.width*4;
    }

    getLeftPixelIndex(i: number) {
        return i - 4;
    }

    getRightPixelIndex(i: number) {
        return i + 4;
    }
}
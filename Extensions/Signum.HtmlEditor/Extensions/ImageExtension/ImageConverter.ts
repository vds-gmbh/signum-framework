
export interface ImageConverter<T> {
  dataImageIdAttribute?: string;
  uploadData(blob: Blob): Promise<T>;
  renderImage(val: T): React.ReactElement;
  toElement(val: T): HTMLElement | undefined;
  fromElement(val: HTMLElement): T | undefined;
}

export interface ImageInfo {
  imageId?: string;
  binaryFile?: string;
  fileName?: string;
}

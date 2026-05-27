// Photo upload helpers for the create-listing wizard. Pure, React-free so the
// component layer can stay focused on UI state. The flow is two hops:
// 1. POST /api/products/photo-upload-url to mint a SAS upload URL.
// 2. PUT the file bytes directly to that SAS URL with x-ms-blob-type.
// The component then stores the returned blobUrl (NOT the SAS URL) in the form.

export type SasResponse = {
  uploadUrl: string;
  blobUrl: string;
  expiresAt: string;
};

export type UploadErrorCode =
  | "sas-rejected"
  | "put-failed"
  | "network"
  | "validation";

export class UploadError extends Error {
  readonly code: UploadErrorCode;
  constructor(message: string, code: UploadErrorCode) {
    super(message);
    this.name = "UploadError";
    this.code = code;
  }
}

const ALLOWED_MIME = new Set([
  "image/jpeg",
  "image/png",
  "image/webp",
  "image/heic",
]);
const MAX_BYTES = 8 * 1024 * 1024;
const MAX_DIMENSION = 6000;

export async function getUploadUrl(
  contentType: string,
  sizeBytes: number,
): Promise<SasResponse> {
  let response: Response;
  try {
    response = await fetch("/api/products/photo-upload-url", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ contentType, sizeBytes }),
    });
  } catch (err) {
    throw new UploadError(
      err instanceof Error ? err.message : "network error",
      "network",
    );
  }
  if (!response.ok) {
    throw new UploadError(
      `SAS request rejected (${response.status})`,
      "sas-rejected",
    );
  }
  return (await response.json()) as SasResponse;
}

export async function putFileToSas(
  file: File,
  uploadUrl: string,
): Promise<void> {
  let response: Response;
  try {
    response = await fetch(uploadUrl, {
      method: "PUT",
      headers: {
        "Content-Type": file.type,
        "x-ms-blob-type": "BlockBlob",
      },
      body: file,
    });
  } catch (err) {
    throw new UploadError(
      err instanceof Error ? err.message : "network error",
      "network",
    );
  }
  if (!response.ok) {
    throw new UploadError(`Blob PUT failed (${response.status})`, "put-failed");
  }
}

export type ValidationResult =
  | { ok: true }
  | { ok: false; reason: "mime" | "size" | "dimensions" };

export async function validateClientSide(
  file: File,
): Promise<ValidationResult> {
  if (!ALLOWED_MIME.has(file.type)) return { ok: false, reason: "mime" };
  if (file.size > MAX_BYTES) return { ok: false, reason: "size" };
  try {
    const bitmap = await createImageBitmap(file);
    const tooBig =
      bitmap.width > MAX_DIMENSION || bitmap.height > MAX_DIMENSION;
    // Free the underlying buffer; not all runtimes provide .close().
    if (typeof (bitmap as { close?: () => void }).close === "function") {
      (bitmap as { close: () => void }).close();
    }
    if (tooBig) return { ok: false, reason: "dimensions" };
  } catch {
    // Treat undecodable images as a dimension failure rather than crashing
    // the uploader; the user retries with a different file.
    return { ok: false, reason: "dimensions" };
  }
  return { ok: true };
}

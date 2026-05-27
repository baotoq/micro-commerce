import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import {
  getUploadUrl,
  putFileToSas,
  UploadError,
  validateClientSide,
} from "./upload";

const SAS_RESPONSE = {
  uploadUrl:
    "https://127.0.0.1:10000/devstoreaccount1/photos/products/abc.jpg?sv=2024&sig=zzz",
  blobUrl: "https://127.0.0.1:10000/devstoreaccount1/photos/products/abc.jpg",
  expiresAt: "2026-05-28T03:15:00.0000000+00:00",
};

const originalFetch = globalThis.fetch;
const originalCreateImageBitmap = (
  globalThis as { createImageBitmap?: unknown }
).createImageBitmap;

beforeEach(() => {
  globalThis.fetch = vi.fn() as unknown as typeof fetch;
});

afterEach(() => {
  globalThis.fetch = originalFetch;
  if (originalCreateImageBitmap === undefined) {
    delete (globalThis as { createImageBitmap?: unknown }).createImageBitmap;
  } else {
    (globalThis as { createImageBitmap?: unknown }).createImageBitmap =
      originalCreateImageBitmap;
  }
});

function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("getUploadUrl", () => {
  it("POSTs JSON to /api/products/photo-upload-url and returns the parsed body", async () => {
    const fetchMock = vi.mocked(globalThis.fetch);
    fetchMock.mockResolvedValueOnce(jsonResponse(SAS_RESPONSE));

    const result = await getUploadUrl("image/jpeg", 12345);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe("/api/products/photo-upload-url");
    expect(init?.method).toBe("POST");
    const headers = new Headers(init?.headers);
    expect(headers.get("Content-Type")).toBe("application/json");
    expect(init?.body).toBe(
      JSON.stringify({ contentType: "image/jpeg", sizeBytes: 12345 }),
    );
    expect(result).toEqual(SAS_RESPONSE);
  });

  it("throws an UploadError with code='sas-rejected' on a 400 response", async () => {
    const fetchMock = vi.mocked(globalThis.fetch);
    fetchMock.mockResolvedValueOnce(
      jsonResponse({ detail: "disallowed mime" }, 400),
    );

    await expect(getUploadUrl("image/gif", 1000)).rejects.toMatchObject({
      name: "UploadError",
      code: "sas-rejected",
    });
  });

  it("throws an UploadError with code='network' when fetch rejects", async () => {
    const fetchMock = vi.mocked(globalThis.fetch);
    fetchMock.mockRejectedValueOnce(new TypeError("network down"));

    await expect(getUploadUrl("image/jpeg", 1000)).rejects.toMatchObject({
      name: "UploadError",
      code: "network",
    });
  });
});

describe("putFileToSas", () => {
  it("PUTs the raw bytes with x-ms-blob-type=BlockBlob and Content-Type=file.type", async () => {
    const fetchMock = vi.mocked(globalThis.fetch);
    fetchMock.mockResolvedValueOnce(new Response(null, { status: 201 }));

    const file = new File(["bytes"], "a.jpg", { type: "image/jpeg" });
    await putFileToSas(file, SAS_RESPONSE.uploadUrl);

    expect(fetchMock).toHaveBeenCalledTimes(1);
    const [url, init] = fetchMock.mock.calls[0];
    expect(url).toBe(SAS_RESPONSE.uploadUrl);
    expect(init?.method).toBe("PUT");
    const headers = new Headers(init?.headers);
    expect(headers.get("Content-Type")).toBe("image/jpeg");
    expect(headers.get("x-ms-blob-type")).toBe("BlockBlob");
    expect(init?.body).toBe(file);
  });

  it("throws UploadError with code='put-failed' on non-2xx", async () => {
    const fetchMock = vi.mocked(globalThis.fetch);
    fetchMock.mockResolvedValueOnce(new Response("oops", { status: 403 }));

    const file = new File(["bytes"], "a.jpg", { type: "image/jpeg" });

    await expect(
      putFileToSas(file, SAS_RESPONSE.uploadUrl),
    ).rejects.toMatchObject({
      name: "UploadError",
      code: "put-failed",
    });
  });

  it("throws UploadError with code='network' when fetch rejects", async () => {
    const fetchMock = vi.mocked(globalThis.fetch);
    fetchMock.mockRejectedValueOnce(new TypeError("offline"));

    const file = new File(["bytes"], "a.jpg", { type: "image/jpeg" });

    await expect(
      putFileToSas(file, SAS_RESPONSE.uploadUrl),
    ).rejects.toMatchObject({
      name: "UploadError",
      code: "network",
    });
  });
});

describe("validateClientSide", () => {
  function stubImageBitmap(width: number, height: number) {
    (globalThis as { createImageBitmap?: unknown }).createImageBitmap = vi
      .fn()
      .mockResolvedValue({
        width,
        height,
        close: vi.fn(),
      });
  }

  it("accepts a small jpeg of reasonable dimensions", async () => {
    stubImageBitmap(800, 600);
    const file = new File(["x"], "a.jpg", { type: "image/jpeg" });
    Object.defineProperty(file, "size", { value: 100_000 });

    expect(await validateClientSide(file)).toEqual({ ok: true });
  });

  it("rejects an image/gif with reason='mime'", async () => {
    stubImageBitmap(100, 100);
    const file = new File(["x"], "a.gif", { type: "image/gif" });
    Object.defineProperty(file, "size", { value: 100 });

    expect(await validateClientSide(file)).toEqual({
      ok: false,
      reason: "mime",
    });
  });

  it("rejects a file larger than 8 MB with reason='size'", async () => {
    stubImageBitmap(100, 100);
    const file = new File(["x"], "a.jpg", { type: "image/jpeg" });
    Object.defineProperty(file, "size", { value: 9 * 1024 * 1024 });

    expect(await validateClientSide(file)).toEqual({
      ok: false,
      reason: "size",
    });
  });

  it("rejects a 7000x7000 image with reason='dimensions'", async () => {
    stubImageBitmap(7000, 7000);
    const file = new File(["x"], "a.jpg", { type: "image/jpeg" });
    Object.defineProperty(file, "size", { value: 100_000 });

    expect(await validateClientSide(file)).toEqual({
      ok: false,
      reason: "dimensions",
    });
  });

  it("accepts heic, png, webp MIME types", async () => {
    stubImageBitmap(500, 500);
    for (const type of [
      "image/png",
      "image/webp",
      "image/heic",
      "image/jpeg",
    ]) {
      const file = new File(["x"], "a", { type });
      Object.defineProperty(file, "size", { value: 100 });
      expect(await validateClientSide(file)).toEqual({ ok: true });
    }
  });
});

describe("UploadError", () => {
  it("exposes name='UploadError' and a code", () => {
    const err = new UploadError("boom", "network");
    expect(err).toBeInstanceOf(Error);
    expect(err.name).toBe("UploadError");
    expect(err.code).toBe("network");
    expect(err.message).toBe("boom");
  });
});

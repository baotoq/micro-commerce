import {
  act,
  fireEvent,
  render,
  screen,
  waitFor,
} from "@testing-library/react";
import { useState } from "react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { PhotoUploader } from "./photo-uploader";

const SAS = {
  uploadUrl: "https://blob.test/photos/products/abc.jpg?sv=2024&sig=zzz",
  blobUrl: "https://blob.test/photos/products/abc.jpg",
  expiresAt: "2026-05-28T03:15:00.0000000+00:00",
};

const originalFetch = globalThis.fetch;
const originalCreateImageBitmap = (
  globalThis as { createImageBitmap?: unknown }
).createImageBitmap;

beforeEach(() => {
  globalThis.fetch = vi.fn() as unknown as typeof fetch;
  (globalThis as { createImageBitmap?: unknown }).createImageBitmap = vi
    .fn()
    .mockResolvedValue({ width: 800, height: 600, close: vi.fn() });
});

afterEach(() => {
  globalThis.fetch = originalFetch;
  if (originalCreateImageBitmap === undefined) {
    delete (globalThis as { createImageBitmap?: unknown }).createImageBitmap;
  } else {
    (globalThis as { createImageBitmap?: unknown }).createImageBitmap =
      originalCreateImageBitmap;
  }
  vi.restoreAllMocks();
});

function mockSasOk(): void {
  vi.mocked(globalThis.fetch).mockImplementation(async (input) => {
    const url = typeof input === "string" ? input : (input as Request).url;
    if (url.includes("photo-upload-url")) {
      return new Response(JSON.stringify(SAS), {
        status: 200,
        headers: { "Content-Type": "application/json" },
      });
    }
    return new Response(null, { status: 201 });
  });
}

function Harness({
  initial = [] as string[],
  maxCount,
  onChange,
}: {
  initial?: string[];
  maxCount?: number;
  onChange?: (v: string[]) => void;
}) {
  const [value, setValue] = useState<string[]>(initial);
  return (
    <PhotoUploader
      value={value}
      onChange={(next) => {
        setValue(next);
        onChange?.(next);
      }}
      maxCount={maxCount}
    />
  );
}

function makeFile(name = "a.jpg", type = "image/jpeg", size = 100_000): File {
  const file = new File([new Uint8Array([0xff, 0xd8, 0xff])], name, { type });
  Object.defineProperty(file, "size", { value: size });
  return file;
}

function fileInput(): HTMLInputElement {
  const input = document.querySelector(
    'input[type="file"]',
  ) as HTMLInputElement | null;
  if (!input) throw new Error("No file input rendered");
  return input;
}

describe("PhotoUploader", () => {
  it("renders maxCount empty slots", () => {
    render(<Harness maxCount={6} />);
    const adders = screen.getAllByRole("button", { name: /add photo/i });
    expect(adders).toHaveLength(6);
  });

  it("uploads a selected file and calls onChange with the blobUrl", async () => {
    mockSasOk();
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);

    const file = makeFile();
    await act(async () => {
      fireEvent.change(fileInput(), { target: { files: [file] } });
    });

    await waitFor(() => {
      expect(onChange).toHaveBeenCalledWith([SAS.blobUrl]);
    });
  });

  it("shows a Primary pill on the first filled slot only", async () => {
    render(<Harness initial={[SAS.blobUrl, "https://blob.test/b.jpg"]} />);
    const pills = screen.getAllByText(/primary/i);
    expect(pills).toHaveLength(1);
  });

  it("removes a photo when × is clicked", () => {
    const onChange = vi.fn();
    render(
      <Harness
        initial={[SAS.blobUrl, "https://blob.test/b.jpg"]}
        onChange={onChange}
      />,
    );
    const removeBtn = screen.getAllByRole("button", {
      name: /remove photo/i,
    })[0];
    fireEvent.click(removeBtn);
    expect(onChange).toHaveBeenCalledWith(["https://blob.test/b.jpg"]);
  });

  it("rejects a non-whitelisted MIME and shows an inline error", async () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);

    const file = makeFile("a.gif", "image/gif");
    await act(async () => {
      fireEvent.change(fileInput(), { target: { files: [file] } });
    });

    await waitFor(() => {
      expect(screen.getByText(/file type/i)).toBeInTheDocument();
    });
    expect(onChange).not.toHaveBeenCalled();
  });

  it("rejects an oversize file (>8 MB) with an inline error", async () => {
    const onChange = vi.fn();
    render(<Harness onChange={onChange} />);

    const file = makeFile("a.jpg", "image/jpeg", 9 * 1024 * 1024);
    await act(async () => {
      fireEvent.change(fileInput(), { target: { files: [file] } });
    });

    await waitFor(() => {
      expect(screen.getByText(/too large/i)).toBeInTheDocument();
    });
    expect(onChange).not.toHaveBeenCalled();
  });

  it("does not render an extra + button past maxCount", () => {
    render(
      <Harness
        maxCount={2}
        initial={["https://blob.test/a.jpg", "https://blob.test/b.jpg"]}
      />,
    );
    expect(
      screen.queryByRole("button", { name: /add photo/i }),
    ).not.toBeInTheDocument();
  });

  it("disables the file picker while an upload is in flight", async () => {
    // Hold the SAS response open so we can assert in-flight UI state.
    let releaseSas: (value: Response) => void = () => {};
    vi.mocked(globalThis.fetch).mockImplementation(async (input) => {
      const url = typeof input === "string" ? input : (input as Request).url;
      if (url.includes("photo-upload-url")) {
        return new Promise<Response>((resolve) => {
          releaseSas = resolve;
        });
      }
      return new Response(null, { status: 201 });
    });

    render(<Harness />);
    const file = makeFile();
    await act(async () => {
      fireEvent.change(fileInput(), { target: { files: [file] } });
    });

    await waitFor(() => {
      expect(
        screen.getAllByRole("button", { name: /add photo/i })[0],
      ).toBeDisabled();
    });

    await act(async () => {
      releaseSas(
        new Response(JSON.stringify(SAS), {
          status: 200,
          headers: { "Content-Type": "application/json" },
        }),
      );
    });
  });

  it("renders the persisted blobUrl as an image preview", () => {
    render(<Harness initial={[SAS.blobUrl]} />);
    const img = screen.getByAltText(/photo 1/i) as HTMLImageElement;
    expect(img).toBeInTheDocument();
    // next/image rewrites src; the original URL must be discoverable for e2e.
    expect(img.getAttribute("data-photo-url")).toBe(SAS.blobUrl);
  });
});

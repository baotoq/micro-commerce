"use client";

import Image from "next/image";
import { useId, useRef, useState } from "react";
import {
  getUploadUrl,
  putFileToSas,
  UploadError,
  validateClientSide,
} from "@/lib/seller/listings/upload";
import { cn } from "@/lib/utils";

export type PhotoUploaderProps = {
  value: string[];
  onChange: (next: string[]) => void;
  maxCount?: number;
};

const ACCEPT = "image/jpeg,image/png,image/webp,image/heic";

function validationMessage(reason: "mime" | "size" | "dimensions"): string {
  if (reason === "mime")
    return "Unsupported file type. Use JPG, PNG, WEBP, or HEIC.";
  if (reason === "size") return "File too large. Max 8 MB.";
  return "Image dimensions too large. Max 6000 × 6000.";
}

function uploadErrorMessage(err: UploadError): string {
  if (err.code === "sas-rejected")
    return "Server rejected the upload. Try again.";
  if (err.code === "put-failed") return "Upload failed. Try again.";
  if (err.code === "network") return "Network error. Check your connection.";
  return "Upload failed.";
}

export function PhotoUploader({
  value,
  onChange,
  maxCount = 6,
}: PhotoUploaderProps) {
  const inputRef = useRef<HTMLInputElement>(null);
  const inputId = useId();
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const slotsRemaining = Math.max(maxCount - value.length, 0);
  const showAdder = slotsRemaining > 0;

  async function handleFile(file: File) {
    setError(null);
    const validation = await validateClientSide(file);
    if (!validation.ok) {
      setError(validationMessage(validation.reason));
      return;
    }
    setUploading(true);
    try {
      const sas = await getUploadUrl(file.type, file.size);
      await putFileToSas(file, sas.uploadUrl);
      onChange([...value, sas.blobUrl]);
    } catch (err) {
      if (err instanceof UploadError) {
        setError(uploadErrorMessage(err));
      } else {
        setError("Upload failed.");
      }
    } finally {
      setUploading(false);
    }
  }

  function onPick() {
    if (uploading) return;
    inputRef.current?.click();
  }

  function onInputChange(e: React.ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0];
    // Reset so picking the same file twice re-fires onChange.
    e.target.value = "";
    if (!file) return;
    void handleFile(file);
  }

  function onRemove(index: number) {
    onChange(value.filter((_, i) => i !== index));
    setError(null);
  }

  return (
    <div className="flex flex-col gap-2">
      <div className="grid grid-cols-3 gap-2 sm:grid-cols-6">
        {value.map((url, idx) => (
          <FilledSlot
            // biome-ignore lint/suspicious/noArrayIndexKey: blob URLs may repeat across slots in edge cases; the slot index is the stable identity.
            key={`${url}-${idx}`}
            url={url}
            index={idx}
            isPrimary={idx === 0}
            onRemove={() => onRemove(idx)}
          />
        ))}
        {showAdder &&
          Array.from({ length: slotsRemaining }).map((_, i) => (
            <EmptySlot
              // biome-ignore lint/suspicious/noArrayIndexKey: placeholder slots have no identity beyond their position.
              key={`empty-${i}`}
              onClick={onPick}
              disabled={uploading}
              busy={uploading && i === 0}
            />
          ))}
      </div>
      <input
        ref={inputRef}
        id={inputId}
        type="file"
        accept={ACCEPT}
        onChange={onInputChange}
        className="sr-only"
        aria-label="Choose a photo to upload"
      />
      {error && (
        <p className="text-[11px]" style={{ color: "var(--bad)" }} role="alert">
          {error}{" "}
          <button
            type="button"
            onClick={onPick}
            disabled={uploading}
            className="ml-1 underline"
          >
            Retry
          </button>
        </p>
      )}
    </div>
  );
}

function FilledSlot({
  url,
  index,
  isPrimary,
  onRemove,
}: {
  url: string;
  index: number;
  isPrimary: boolean;
  onRemove: () => void;
}) {
  return (
    <div className="relative aspect-square overflow-hidden rounded-md border border-border bg-muted">
      <Image
        src={url}
        alt={`Photo ${index + 1}`}
        fill
        sizes="(min-width: 640px) 120px, 33vw"
        unoptimized
        data-photo-url={url}
        className="object-cover"
      />
      {isPrimary && (
        <span className="absolute left-1 top-1 rounded-full bg-foreground/85 px-1.5 py-0.5 text-[10px] font-semibold uppercase tracking-wide text-background">
          Primary
        </span>
      )}
      <button
        type="button"
        aria-label={`Remove photo ${index + 1}`}
        onClick={onRemove}
        className="absolute right-1 top-1 inline-flex h-5 w-5 items-center justify-center rounded-full bg-foreground/85 text-[12px] leading-none text-background hover:bg-foreground"
      >
        &times;
      </button>
    </div>
  );
}

function EmptySlot({
  onClick,
  disabled,
  busy,
}: {
  onClick: () => void;
  disabled: boolean;
  busy: boolean;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      aria-label="Add photo"
      className={cn(
        "flex aspect-square items-center justify-center rounded-md border border-dashed border-black/25 bg-canvas-parchment text-[11px] text-muted-foreground hover:border-black/40 hover:text-foreground",
        disabled && "opacity-60",
      )}
    >
      {busy ? (
        <span className="animate-pulse">Uploading…</span>
      ) : (
        <span>+ Add photo</span>
      )}
    </button>
  );
}

"use client";

// STUB owned by task #3 (wizard-frontend). Task #5 (photo-vertical) replaces
// this with the real SAS-flow uploader. The prop contract here is the
// integration point — task #5 must keep the exact signature so step-media
// keeps compiling.

export type PhotoUploaderProps = {
  value: string[];
  onChange: (next: string[]) => void;
  maxCount?: number;
};

export function PhotoUploader({
  value,
  onChange: _onChange,
  maxCount = 6,
}: PhotoUploaderProps) {
  return (
    <div
      data-testid="photo-uploader-stub"
      className="rounded-md border border-dashed border-black/20 bg-canvas-parchment p-4 text-[12px] text-muted-foreground"
    >
      <p>Photo uploader (stub) — replaced by task #5.</p>
      <p className="mt-1 tabular-nums">
        {value.length} / {maxCount} photos
      </p>
    </div>
  );
}

"use client";

import { useRouter } from "next/navigation";
import { useEffect, useRef, useState, useTransition } from "react";
import { deleteListingAction } from "@/lib/seller/listings/actions";

export function DeleteListingButton({ sku }: { sku: string }) {
  const router = useRouter();
  const [open, setOpen] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isPending, startTransition] = useTransition();
  const confirmRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (!open) return;
    confirmRef.current?.focus();

    function onKeyDown(e: KeyboardEvent) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("keydown", onKeyDown);
    return () => document.removeEventListener("keydown", onKeyDown);
  }, [open]);

  function handleConfirm() {
    setError(null);
    startTransition(async () => {
      const result = await deleteListingAction(sku);
      if (result.ok) {
        router.push("/seller/listings");
      } else {
        setError(result.error);
      }
    });
  }

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="inline-flex h-9 items-center justify-center rounded-md border border-bad/30 px-3 text-sm font-medium text-bad hover:bg-bad/5 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-bad/50"
      >
        Delete listing
      </button>

      {open && (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
          {/* Backdrop */}
          <button
            type="button"
            aria-label="Close dialog"
            className="absolute inset-0 bg-black/40 cursor-default"
            onClick={() => setOpen(false)}
          />

          {/* Dialog */}
          <div
            role="alertdialog"
            aria-modal="true"
            aria-labelledby="delete-dialog-title"
            aria-describedby="delete-dialog-desc"
            className="relative z-10 w-full max-w-sm rounded-xl bg-white p-6 shadow-xl"
          >
            <h2
              id="delete-dialog-title"
              className="text-[17px] font-semibold text-foreground"
            >
              Delete listing?
            </h2>
            <p
              id="delete-dialog-desc"
              className="mt-1.5 text-sm text-foreground/60"
            >
              This will permanently remove the listing. This action cannot be
              undone.
            </p>

            {error && (
              <p className="mt-3 rounded-md bg-bad/10 px-3 py-2 text-sm text-bad">
                {error}
              </p>
            )}

            <div className="mt-5 flex justify-end gap-2">
              <button
                type="button"
                onClick={() => setOpen(false)}
                disabled={isPending}
                className="inline-flex h-9 items-center rounded-md px-3 text-sm font-medium text-foreground/70 hover:bg-canvas-parchment focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-black/20 disabled:opacity-50"
              >
                Cancel
              </button>
              <button
                ref={confirmRef}
                type="button"
                onClick={handleConfirm}
                disabled={isPending}
                className="inline-flex h-9 items-center rounded-md bg-bad px-3 text-sm font-medium text-white hover:bg-bad/90 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-bad/50 disabled:opacity-50"
              >
                {isPending ? "Deleting…" : "Delete"}
              </button>
            </div>
          </div>
        </div>
      )}
    </>
  );
}

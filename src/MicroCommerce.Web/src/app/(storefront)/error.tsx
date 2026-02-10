"use client";

import { useEffect } from "react";

export default function StorefrontError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("Storefront error:", error);
  }, [error]);

  return (
    <div className="flex min-h-[50vh] flex-col items-center justify-center px-4">
      <h2 className="mb-2 text-xl font-semibold text-zinc-900">
        Something went wrong
      </h2>
      <p className="mb-6 text-sm text-zinc-500">
        An unexpected error occurred. Please try again.
      </p>
      <button
        onClick={reset}
        className="rounded-lg bg-zinc-900 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-zinc-800"
      >
        Try again
      </button>
    </div>
  );
}

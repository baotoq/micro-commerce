"use client";

export function CopyShopLink({ domain }: { domain: string }) {
  return (
    <button
      type="button"
      onClick={() => navigator.clipboard.writeText(`https://${domain}`)}
      className="mt-5 rounded-lg bg-foreground px-5 py-2.5 text-sm font-medium text-white hover:bg-foreground/90"
    >
      Copy {domain}
    </button>
  );
}

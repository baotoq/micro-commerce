"use client";

export function CopyShopLink({ domain }: { domain: string }) {
  return (
    <button
      type="button"
      onClick={() => navigator.clipboard.writeText(`https://${domain}`)}
      className="mt-5 rounded-lg bg-[#1d1d1f] px-5 py-2.5 text-sm font-medium text-white hover:bg-[#1d1d1f]/90"
    >
      Copy {domain}
    </button>
  );
}

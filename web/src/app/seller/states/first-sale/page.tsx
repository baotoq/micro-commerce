import Link from "next/link";
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { money } from "@/lib/money";
import { BRAND } from "@/lib/seller/data";

export default function SellerStatesFirstSale() {
  return (
    <div className="relative flex min-h-screen flex-col overflow-hidden">
      <SellerTopbar
        title={`Welcome, ${BRAND.owner}`}
        subtitle="Day 4 · Friday, March 15"
      />

      {/* Blurred dashboard placeholder behind overlay */}
      <div
        className="flex-1 overflow-auto px-7 py-6"
        style={{ filter: "blur(2px)" }}
      >
        <div
          className="mb-5 grid gap-3"
          style={{ gridTemplateColumns: "repeat(4, 1fr)" }}
        >
          {[0, 1, 2, 3].map((i) => (
            <div
              key={i}
              className="rounded-xl border border-black/[0.06] bg-white p-[18px]"
            >
              <div
                className="mb-2 rounded"
                style={{ width: "40%", height: 10, background: "#f5f5f7" }}
              />
              <div
                className="rounded"
                style={{ width: "60%", height: 22, background: "#f5f5f7" }}
              />
            </div>
          ))}
        </div>

        <div
          className="rounded-xl border border-black/[0.06] bg-white p-5"
          style={{ height: 240 }}
        >
          <div
            className="mb-4 rounded"
            style={{ width: "20%", height: 14, background: "#f5f5f7" }}
          />
          <div
            className="rounded-lg"
            style={{ width: "100%", height: 180, background: "#f5f5f7" }}
          />
        </div>
      </div>

      {/* Modal overlay — always open, static visual */}
      <div
        className="absolute inset-0 flex items-center justify-center"
        style={{ background: "rgba(21,18,14,0.5)", zIndex: 50 }}
      >
        <div
          role="dialog"
          aria-modal="true"
          aria-labelledby="first-sale-heading"
          className="overflow-hidden rounded-2xl border border-black/[0.06] bg-white text-center"
          style={{ width: 480 }}
        >
          {/* Hero band */}
          <div
            className="relative overflow-hidden text-white"
            style={{
              background: "#0066cc",
              padding: "28px 28px 22px",
            }}
          >
            {/* Decorative circles */}
            <div
              className="absolute rounded-full"
              style={{
                right: -20,
                top: -30,
                width: 140,
                height: 140,
                background: "rgba(255,255,255,0.08)",
              }}
            />
            <div
              className="absolute rounded-full"
              style={{
                left: -10,
                bottom: -40,
                width: 90,
                height: 90,
                background: "rgba(255,255,255,0.06)",
              }}
            />
            <div className="relative" style={{ fontSize: 44, lineHeight: 1 }}>
              ★
            </div>
            <div
              className="relative mt-3 text-xs font-semibold uppercase tracking-wider"
              style={{ opacity: 0.85, letterSpacing: 0.1 }}
            >
              Your first sale
            </div>
            <h2
              id="first-sale-heading"
              className="relative mt-1.5 font-semibold text-white"
              style={{ fontSize: 36, lineHeight: 1 }}
            >
              It happened.
            </h2>
          </div>

          {/* Body */}
          <div style={{ padding: "24px 32px 28px" }}>
            {/* Order summary row */}
            <div
              className="flex items-center gap-3 rounded-[10px] text-left"
              style={{ padding: 14, background: "#f5f5f7" }}
            >
              {/* Clay-tone product image placeholder */}
              <div
                className="shrink-0 rounded-lg"
                style={{
                  width: 56,
                  height: 56,
                  background: "rgba(194,65,12,0.18)",
                }}
              />
              <div className="flex-1">
                <div className="text-sm font-semibold">Persimmon vase</div>
                <div className="text-xs text-[#1d1d1f]/50">
                  Sasha L. · San Francisco, CA
                </div>
              </div>
              <span
                className="font-semibold tabular-nums"
                style={{ fontSize: 22 }}
              >
                {money(86)}
              </span>
            </div>

            {/* Body copy */}
            <p
              className="text-[#1d1d1f]/60"
              style={{ marginTop: 18, fontSize: 14, lineHeight: 1.55 }}
            >
              {`You’ll receive ${money(82.56)} after Micro’s 4% fee. Pack & ship in the next 3 days and the rating will follow.`}
            </p>

            {/* Action buttons */}
            <div className="mt-5 flex gap-2">
              <button
                type="button"
                className="flex-1 rounded-xl border border-black/[0.15] px-4 py-2.5 text-sm font-semibold text-[#1d1d1f]"
              >
                Send a thank-you note
              </button>
              {/* biome-ignore lint/a11y/useSemanticElements: link styled as button to navigate while preserving e2e button-role assertion */}
              <Link
                href="/seller/orders/1001/pack"
                role="button"
                className="flex-1 rounded-xl bg-[#1d1d1f] px-4 py-2.5 text-sm font-semibold text-white text-center"
              >
                Pack &amp; ship →
              </Link>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

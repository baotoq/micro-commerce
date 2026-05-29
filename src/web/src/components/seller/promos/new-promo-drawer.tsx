"use client";

import { useActionState, useRef, useState } from "react";
import type { ActionResult } from "@/lib/seller/promos/actions";
import { createPromoAction } from "@/lib/seller/promos/actions";

type Kind = "percentage" | "fixed";

const WHO_OPTIONS = [
  {
    label: "Anyone with the code",
    sub: "Public · share on socials",
    on: false,
  },
  {
    label: "Followers only",
    sub: "Auto-applied · 184 buyers eligible",
    on: true,
  },
  { label: "Specific customers", sub: "Pick from your CRM", on: false },
] as const;

function CloseIcon() {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      aria-hidden="true"
    >
      <title>close</title>
      <path d="M4 4l12 12M16 4L4 16" />
    </svg>
  );
}

function RefreshIcon() {
  return (
    <svg
      width="11"
      height="11"
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.6"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden="true"
    >
      <title>refresh</title>
      <path d="M4 4a8 8 0 0 1 12 0M16 16a8 8 0 0 1-12 0M17 6v4h-4M3 14v-4h4" />
    </svg>
  );
}

export function NewPromoDrawer() {
  const formRef = useRef<HTMLFormElement>(null);
  const [code, setCode] = useState("");
  const [kind, setKind] = useState<Kind>("percentage");
  const [value, setValue] = useState("");
  const [startsAt, setStartsAt] = useState("");
  const [endsAt, setEndsAt] = useState("");
  const [minOrderAmount, setMinOrderAmount] = useState("");

  const [state, formAction, isPending] = useActionState<
    ActionResult | null,
    FormData
  >(async (_prev, formData) => {
    const result = await createPromoAction(formData);
    if (result.ok) {
      formRef.current?.reset();
      setCode("");
      setKind("percentage");
      setValue("");
      setStartsAt("");
      setEndsAt("");
      setMinOrderAmount("");
    }
    return result;
  }, null);

  function handleGenerate() {
    setCode(crypto.randomUUID().slice(0, 8).toUpperCase());
  }

  const windowDisplay =
    startsAt && endsAt
      ? `${new Date(startsAt).toLocaleDateString("en-US", { month: "short", day: "numeric" })} → ${new Date(endsAt).toLocaleDateString("en-US", { month: "short", day: "numeric" })}`
      : "—";

  const minOrderDisplay = minOrderAmount
    ? `$${Number(minOrderAmount).toFixed(2)}`
    : "—";

  return (
    <div
      className="absolute bottom-0 right-0 top-0 flex flex-col bg-white"
      style={{
        width: 460,
        borderLeft: "1px solid rgba(0,0,0,0.06)",
        boxShadow: "-12px 0 30px rgba(0,0,0,0.06)",
        zIndex: 30,
      }}
    >
      {/* Header */}
      <div
        className="flex items-center justify-between"
        style={{
          padding: "16px 22px",
          borderBottom: "1px solid rgba(0,0,0,0.06)",
        }}
      >
        <h2 className="text-[18px] font-semibold text-foreground">
          New promotion
        </h2>
        <button
          type="button"
          aria-label="Close"
          className="flex size-8 items-center justify-center rounded-full text-muted-foreground hover:bg-black/[0.04]"
        >
          <CloseIcon />
        </button>
      </div>

      {/* Form */}
      <form
        ref={formRef}
        action={formAction}
        className="flex min-h-0 flex-1 flex-col"
      >
        {/* Hidden kind field */}
        <input type="hidden" name="kind" value={kind} />
        {/* Hidden value fields — only submit the one matching kind */}
        {kind === "percentage" && (
          <input type="hidden" name="percentValue" value={value} />
        )}
        {kind === "fixed" && (
          <input type="hidden" name="fixedAmount" value={value} />
        )}

        {/* Body */}
        <div className="grow overflow-auto" style={{ padding: 22 }}>
          {/* Top-level feedback */}
          {state && !state.ok && (
            <div
              role="alert"
              className="mb-4 rounded-md border border-bad/30 bg-bad/10 px-4 py-3 text-sm text-bad"
            >
              {state.error}
            </div>
          )}

          {/* CODE section */}
          <div
            className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground"
            style={{ marginBottom: 8 }}
          >
            Code
          </div>
          <div
            className="flex items-center gap-2"
            style={{
              height: 44,
              padding: "0 14px",
              border: "1.5px solid var(--foreground)",
              borderRadius: 10,
            }}
          >
            <label htmlFor="code" className="sr-only">
              Promo code
            </label>
            <input
              id="code"
              name="code"
              type="text"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              placeholder="e.g. SUMMER20"
              className="flex-1 font-mono text-[15px] font-semibold text-foreground bg-transparent focus:outline-none"
              autoComplete="off"
            />
            <button
              type="button"
              onClick={handleGenerate}
              className="flex h-[26px] items-center gap-1 rounded-lg px-2 text-xs font-medium text-muted-foreground hover:bg-black/[0.04]"
            >
              <RefreshIcon />
              Generate
            </button>
          </div>
          <div className="mt-2 text-xs text-muted-foreground">
            Buyers will type or click this at checkout
          </div>

          {/* DISCOUNT section */}
          <div
            className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground"
            style={{ margin: "20px 0 8px" }}
          >
            Discount
          </div>
          {/* Kind toggle */}
          <div
            className="flex w-fit gap-0 rounded-full p-[3px]"
            style={{ background: "var(--canvas-parchment)" }}
          >
            <button
              type="button"
              onClick={() => setKind("percentage")}
              className="flex items-center justify-center rounded-full text-[12px]"
              style={{
                padding: "6px 14px",
                fontWeight: kind === "percentage" ? 600 : 400,
                background: kind === "percentage" ? "white" : "transparent",
                color:
                  kind === "percentage"
                    ? "var(--foreground)"
                    : "color-mix(in oklch, var(--foreground) 60%, transparent)",
                boxShadow:
                  kind === "percentage" ? "0 1px 2px rgba(0,0,0,0.06)" : "none",
              }}
            >
              % off
            </button>
            <button
              type="button"
              onClick={() => setKind("fixed")}
              className="flex items-center justify-center rounded-full text-[12px]"
              style={{
                padding: "6px 14px",
                fontWeight: kind === "fixed" ? 600 : 400,
                background: kind === "fixed" ? "white" : "transparent",
                color:
                  kind === "fixed"
                    ? "var(--foreground)"
                    : "color-mix(in oklch, var(--foreground) 60%, transparent)",
                boxShadow:
                  kind === "fixed" ? "0 1px 2px rgba(0,0,0,0.06)" : "none",
              }}
            >
              $ off
            </button>
            <span
              aria-disabled="true"
              tabIndex={-1}
              className="flex items-center justify-center rounded-full text-[12px]"
              style={{
                padding: "6px 14px",
                fontWeight: 400,
                background: "transparent",
                color:
                  "color-mix(in oklch, var(--foreground) 40%, transparent)",
                cursor: "not-allowed",
              }}
            >
              Free shipping
            </span>
            <span
              aria-disabled="true"
              tabIndex={-1}
              className="flex items-center justify-center rounded-full text-[12px]"
              style={{
                padding: "6px 14px",
                fontWeight: 400,
                background: "transparent",
                color:
                  "color-mix(in oklch, var(--foreground) 40%, transparent)",
                cursor: "not-allowed",
              }}
            >
              BOGO
            </span>
          </div>

          {/* Value input */}
          <div className="mt-3.5 flex items-center gap-2">
            <div
              className="flex items-center gap-1.5"
              style={{
                height: 56,
                padding: "0 18px",
                border: "1.5px solid var(--foreground)",
                borderRadius: 10,
              }}
            >
              <label htmlFor="discount-value" className="sr-only">
                Discount value
              </label>
              <input
                id="discount-value"
                type="number"
                value={value}
                onChange={(e) => setValue(e.target.value)}
                min={kind === "percentage" ? 1 : 0.01}
                max={kind === "percentage" ? 100 : undefined}
                step={kind === "percentage" ? 1 : 0.01}
                placeholder="0"
                className="w-[60px] font-semibold tabular-nums leading-none bg-transparent focus:outline-none"
                style={{ fontSize: 32 }}
              />
              <span className="text-[15px] font-medium text-muted-foreground">
                {kind === "percentage" ? "%" : "$"}
              </span>
            </div>
            <span className="text-sm text-muted-foreground">
              {kind === "percentage"
                ? "off the entire order"
                : "fixed amount off"}
            </span>
          </div>

          {/* WHO CAN USE IT section */}
          <div
            className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground"
            style={{ margin: "22px 0 8px" }}
          >
            Who can use it
          </div>
          <div className="flex flex-col gap-2">
            {WHO_OPTIONS.map((opt) => (
              <div
                key={opt.label}
                className="flex items-start gap-2.5"
                style={{
                  padding: 12,
                  borderRadius: 10,
                  border: opt.on
                    ? "1.5px solid var(--foreground)"
                    : "1px solid rgba(0,0,0,0.1)",
                  background: opt.on ? "var(--canvas-parchment)" : "white",
                }}
              >
                <span
                  className="mt-0.5 flex size-4 shrink-0 items-center justify-center rounded-full"
                  style={{ border: "1.5px solid var(--foreground)" }}
                >
                  {opt.on && (
                    <span
                      className="size-2 rounded-full"
                      style={{ background: "var(--foreground)" }}
                    />
                  )}
                </span>
                <div>
                  <div className="text-[13px] font-semibold text-foreground">
                    {opt.label}
                  </div>
                  <div className="text-xs text-muted-foreground">{opt.sub}</div>
                </div>
              </div>
            ))}
          </div>

          {/* LIMITS section */}
          <div
            className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground"
            style={{ margin: "22px 0 8px" }}
          >
            Limits
          </div>
          <div className="grid grid-cols-2 gap-2">
            {/* Min. order — from form state */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-3">
              <div className="text-xs text-muted-foreground">Min. order</div>
              <div className="mt-1">
                <label htmlFor="min-order" className="sr-only">
                  Minimum order amount
                </label>
                <input
                  id="min-order"
                  name="minOrderAmount"
                  type="number"
                  min="0"
                  step="0.01"
                  value={minOrderAmount}
                  onChange={(e) => setMinOrderAmount(e.target.value)}
                  placeholder="—"
                  className="w-full font-semibold tabular-nums bg-transparent focus:outline-none text-sm"
                />
              </div>
              {minOrderAmount && (
                <div className="text-xs text-muted-foreground">
                  {minOrderDisplay}
                </div>
              )}
            </div>
            {/* Per buyer — static placeholder */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-3">
              <div className="text-xs text-muted-foreground">Per buyer</div>
              <div className="mt-0.5 font-semibold tabular-nums">1 use</div>
            </div>
            {/* Total uses — static placeholder */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-3">
              <div className="text-xs text-muted-foreground">Total uses</div>
              <div className="mt-0.5 font-semibold tabular-nums">200</div>
            </div>
            {/* Window — from form state */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-3">
              <div className="text-xs text-muted-foreground">Window</div>
              <div className="mt-1 flex flex-col gap-1">
                <label htmlFor="starts-at" className="sr-only">
                  Starts at
                </label>
                <input
                  id="starts-at"
                  name="startsAt"
                  type="datetime-local"
                  value={startsAt}
                  onChange={(e) => setStartsAt(e.target.value)}
                  className="w-full text-xs bg-transparent focus:outline-none"
                />
                <label htmlFor="ends-at" className="sr-only">
                  Ends at
                </label>
                <input
                  id="ends-at"
                  name="endsAt"
                  type="datetime-local"
                  value={endsAt}
                  onChange={(e) => setEndsAt(e.target.value)}
                  className="w-full text-xs bg-transparent focus:outline-none"
                />
                {startsAt && endsAt && (
                  <div className="text-[12.5px] font-semibold tabular-nums">
                    {windowDisplay}
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Description */}
          <div style={{ margin: "22px 0 8px" }}>
            <label
              htmlFor="description"
              className="text-[10px] font-semibold uppercase tracking-wider text-muted-foreground"
            >
              Description
            </label>
            <textarea
              id="description"
              name="description"
              rows={2}
              maxLength={200}
              placeholder="e.g. sitewide, first order, followers only"
              className="mt-2 w-full rounded-xl border border-black/[0.06] bg-white p-3 text-sm text-foreground focus:outline-none focus:ring-1 focus:ring-foreground/30 resize-none"
            />
          </div>
        </div>

        {/* Footer */}
        <div
          className="flex items-center justify-between"
          style={{
            padding: "14px 22px",
            borderTop: "1px solid rgba(0,0,0,0.06)",
          }}
        >
          <button
            type="submit"
            name="activateImmediately"
            value="false"
            disabled={isPending}
            className="rounded-xl border border-black/[0.12] px-4 py-2 text-sm font-medium text-foreground hover:bg-black/[0.04] disabled:opacity-50"
          >
            Save draft
          </button>
          <button
            type="submit"
            name="activateImmediately"
            value="true"
            disabled={isPending}
            className="rounded-xl bg-foreground px-4 py-2 text-sm font-semibold text-white hover:bg-foreground/90 disabled:opacity-50"
          >
            Activate now
          </button>
        </div>
      </form>
    </div>
  );
}

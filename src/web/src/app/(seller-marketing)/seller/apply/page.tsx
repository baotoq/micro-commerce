import Link from "next/link";
import { MarketingTop } from "@/components/seller/marketing/marketing-top";
import { getApplication } from "@/lib/seller/application/data";

export default function ApplyPage() {
  const app = getApplication();

  return (
    <div className="flex min-h-screen flex-col bg-white text-foreground">
      <MarketingTop active="For makers" />
      <div className="flex flex-1 overflow-hidden">
        {/* Left — pitch */}
        <div
          className="flex flex-col justify-center"
          style={{ flex: "1.1", padding: "60px 56px 40px" }}
        >
          <div className="mb-3 text-xs font-semibold uppercase tracking-wider text-primary">
            For makers · 4% per sale, no monthly fee
          </div>
          <h1
            className="font-semibold tracking-tight leading-tight"
            style={{ fontSize: 56, letterSpacing: "-0.28px", maxWidth: 520 }}
          >
            Open a shop in <i>about ten minutes.</i>
          </h1>
          <p
            className="mt-3 text-foreground/60"
            style={{ maxWidth: 440, fontSize: 14.5, lineHeight: 1.55 }}
          >
            Bring your goods. We bring the storefront, payments, and a
            soft-spoken little community of buyers who want to know who made the
            thing.
          </p>
          <div className="mt-9 flex gap-6">
            {[
              { v: "4 800", l: "active makers" },
              { v: "$2.1M", l: "paid out · April" },
              { v: "12 min", l: "avg. setup" },
            ].map((s) => (
              <div
                key={s.l}
                className="border-t border-foreground pt-2"
                style={{ minWidth: 110 }}
              >
                <div
                  className="font-semibold tabular-nums"
                  style={{ fontSize: 28, lineHeight: 1 }}
                >
                  {s.v}
                </div>
                <div className="mt-0.5 text-[11px] text-foreground/60">
                  {s.l}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Right — claim card */}
        <div
          className="flex items-center border-l border-black/[0.06] bg-canvas-parchment"
          style={{ flex: "1", padding: "56px 56px 40px" }}
        >
          <div
            className="w-full rounded-lg border border-black/[0.06] bg-white p-7"
            style={{ maxWidth: 420 }}
          >
            <h2 className="text-xl font-semibold text-foreground">
              Claim your shop name
            </h2>
            <p className="mt-1 mb-4 text-[13px] text-foreground/60">
              You can change this later. We'll spin up a free .micro.shop URL
              too.
            </p>

            <p className="block text-[11px] font-medium uppercase tracking-wider text-foreground/60">
              Shop name
            </p>
            <div className="mt-1.5 flex h-11 items-center rounded-sm px-3.5 border-[1.5px] border-foreground">
              <span className="text-base font-semibold text-foreground">
                {app.shopName}
              </span>
              <span
                className="ml-0.5 inline-block bg-foreground"
                style={{
                  width: 1.5,
                  height: 18,
                  animation: "blink 1s steps(1) infinite",
                }}
              />
            </div>

            <div className="mt-2.5 flex items-center gap-2">
              <span className="flex h-3.5 w-3.5 items-center justify-center rounded-full bg-good">
                <svg
                  width="8"
                  height="8"
                  viewBox="0 0 8 8"
                  fill="none"
                  role="img"
                  aria-label="Available"
                >
                  <path
                    d="M1.5 4L3 5.5L6.5 2"
                    stroke="white"
                    strokeWidth="1.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                </svg>
              </span>
              <span className="text-[13px] font-medium text-good">
                {app.domain}
              </span>
              <span className="text-[11px] text-foreground/60">
                is available
              </span>
            </div>

            <p className="mt-4 block text-[11px] font-medium uppercase tracking-wider text-foreground/60">
              What you make
            </p>
            <div className="mt-2 flex flex-wrap gap-1">
              {app.categories.map((cat) => (
                <span
                  key={cat}
                  className="rounded-full px-3 py-1 text-[11.5px] font-medium"
                  style={
                    cat === app.category
                      ? { background: "var(--foreground)", color: "#fff" }
                      : {
                          background: "var(--canvas-parchment)",
                          color: "var(--foreground)",
                          border: "1px solid var(--border)",
                        }
                  }
                >
                  {cat}
                </span>
              ))}
            </div>

            {/* biome-ignore lint/a11y/useSemanticElements: link styled as button to navigate while preserving e2e button-role assertion */}
            <Link
              href="/seller/onboard"
              role="button"
              className="mt-5 block w-full rounded-full bg-primary py-3 text-sm font-medium text-white hover:bg-[#0055aa] text-center"
            >
              Continue · {app.stepsLeft} steps left
            </Link>
            <p className="mt-3 text-center text-[11px] text-foreground/60">
              By continuing you agree to our maker terms · No card needed
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

import { MarketingTop } from "@/components/seller/marketing-top";
import { getApplication } from "@/lib/seller/data";

export default function ApplyPage() {
  const app = getApplication();

  return (
    <div className="flex min-h-screen flex-col bg-white text-[#1d1d1f]">
      <MarketingTop active="For makers" />
      <div className="flex flex-1 overflow-hidden">
        {/* Left — pitch */}
        <div
          className="flex flex-col justify-center"
          style={{ flex: "1.1", padding: "60px 56px 40px" }}
        >
          <div
            className="mb-3 text-xs font-semibold uppercase tracking-wider"
            style={{ color: "#0066cc" }}
          >
            For makers · 4% per sale, no monthly fee
          </div>
          <h1
            className="font-semibold tracking-tight leading-tight"
            style={{ fontSize: 64, letterSpacing: "-0.02em", maxWidth: 520 }}
          >
            Open a shop in <i>about ten minutes.</i>
          </h1>
          <p
            className="mt-3 text-[#1d1d1f]/60"
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
                className="border-t border-[#1d1d1f] pt-2"
                style={{ minWidth: 110 }}
              >
                <div
                  className="font-semibold tabular-nums"
                  style={{ fontSize: 28, lineHeight: 1 }}
                >
                  {s.v}
                </div>
                <div className="mt-0.5 text-[11px] text-[#1d1d1f]/60">
                  {s.l}
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Right — claim card */}
        <div
          className="flex items-center border-l border-black/[0.06] bg-[#f5f5f7]"
          style={{ flex: "1", padding: "56px 56px 40px" }}
        >
          <div
            className="w-full rounded-lg border border-black/[0.06] bg-white p-7"
            style={{ maxWidth: 420 }}
          >
            <h2 className="text-xl font-semibold text-[#1d1d1f]">
              Claim your shop name
            </h2>
            <p className="mt-1 mb-4 text-[13px] text-[#1d1d1f]/60">
              You can change this later. We'll spin up a free .micro.shop URL
              too.
            </p>

            <p className="block text-[11px] font-medium uppercase tracking-wider text-[#1d1d1f]/60">
              Shop name
            </p>
            <div
              className="mt-1.5 flex h-11 items-center rounded-[10px] px-3.5"
              style={{ border: "1.5px solid #1d1d1f" }}
            >
              <span className="text-base font-semibold text-[#1d1d1f]">
                {app.shopName}
              </span>
              <span
                className="ml-0.5 inline-block"
                style={{
                  width: 1.5,
                  height: 18,
                  background: "#1d1d1f",
                  animation: "blink 1s steps(1) infinite",
                }}
              />
            </div>

            <div className="mt-2.5 flex items-center gap-2">
              <span
                className="flex h-3.5 w-3.5 items-center justify-center rounded-full"
                style={{ background: "#34c759" }}
              >
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
              <span
                className="text-[13px] font-medium"
                style={{ color: "#34c759" }}
              >
                {app.domain}
              </span>
              <span className="text-[11px] text-[#1d1d1f]/60">
                is available
              </span>
            </div>

            <p className="mt-4 block text-[11px] font-medium uppercase tracking-wider text-[#1d1d1f]/60">
              What you make
            </p>
            <div className="mt-2 flex flex-wrap gap-1">
              {app.categories.map((cat) => (
                <span
                  key={cat}
                  className="rounded-full px-3 py-1 text-[11.5px] font-medium"
                  style={
                    cat === app.category
                      ? { background: "#1d1d1f", color: "#fff" }
                      : {
                          background: "#f5f5f7",
                          color: "#1d1d1f",
                          border: "1px solid #e0e0e0",
                        }
                  }
                >
                  {cat}
                </span>
              ))}
            </div>

            <button
              type="button"
              className="mt-5 w-full rounded-full bg-[#0066cc] py-3 text-sm font-medium text-white hover:bg-[#0055aa]"
              style={{ border: "none", cursor: "pointer" }}
            >
              Continue · {app.stepsLeft} steps left
            </button>
            <p className="mt-3 text-center text-[11px] text-[#1d1d1f]/60">
              By continuing you agree to our maker terms · No card needed
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}

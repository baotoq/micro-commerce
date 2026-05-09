import { SellerTopbar } from "@/components/seller/seller-topbar";
import { Button } from "@/components/ui/button";
import { getDayOneStats, getLaunchChecklist } from "@/lib/seller/data";

export default function SellerWelcomePage() {
  const stats = getDayOneStats();
  const checklist = getLaunchChecklist();
  const doneCount = checklist.filter((c) => c.done).length;

  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar
        title="Welcome, Alex"
        subtitle="Day 1 · Tuesday, March 12"
        actions={
          <>
            <Button variant="outline">Share shop</Button>
            <Button>+ New listing</Button>
          </>
        }
      />

      <div className="flex-1 overflow-auto p-7">
        {/* Live-shop banner */}
        <div className="bg-[#1d1d1f] text-white rounded-xl p-6 mb-5">
          <div className="flex items-end justify-between">
            <div>
              <p className="text-[10px] uppercase tracking-widest text-white/60">
                Your shop is live
              </p>
              <h2 className="text-[32px] font-semibold leading-none mt-1.5 text-white">
                alex-studio.micro.shop
              </h2>
              <p className="text-sm text-white/70 mt-1">
                Tell people. The first sale is usually a friend.
              </p>
            </div>
            <div className="flex items-center gap-2 shrink-0">
              <Button
                variant="outline"
                className="border-white/30 text-white bg-transparent hover:bg-white/10 hover:text-white"
              >
                Copy link
              </Button>
              <Button className="bg-white text-[#1d1d1f] hover:bg-white/90">
                View shop →
              </Button>
            </div>
          </div>
        </div>

        {/* Two-column band */}
        <div
          className="grid gap-5"
          style={{ gridTemplateColumns: "1.4fr 1fr" }}
        >
          {/* Left column */}
          <div className="flex flex-col gap-5">
            {/* 3-up stat cards */}
            <div className="grid grid-cols-3 gap-5">
              {stats.map((s) => (
                <div
                  key={s.label}
                  className="bg-white rounded-xl border border-black/[0.06] p-4"
                >
                  <p className="text-[11px] text-[#1d1d1f]/50">{s.label}</p>
                  <p
                    className="text-[28px] font-semibold leading-none mt-1.5 tabular-nums"
                    style={{
                      color:
                        s.value === "0" || s.value === "$0.00"
                          ? "#1d1d1f99"
                          : "#1d1d1f",
                    }}
                  >
                    {s.value}
                  </p>
                  <p className="text-[11px] text-[#1d1d1f]/50 mt-1">
                    {s.subject}
                  </p>
                </div>
              ))}
            </div>

            {/* Empty inbox card */}
            <div
              className="bg-[#f5f5f7] rounded-xl flex flex-col items-center justify-center text-center p-6"
              style={{ minHeight: 240 }}
            >
              <div className="w-14 h-14 rounded-full bg-white border border-black/[0.08] flex items-center justify-center mb-3.5 text-[#1d1d1f]/40">
                <svg
                  aria-hidden="true"
                  width="22"
                  height="22"
                  viewBox="0 0 24 24"
                  fill="none"
                  stroke="currentColor"
                  strokeWidth="1.8"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                >
                  <path d="M22 12h-6l-2 3H10l-2-3H2" />
                  <path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z" />
                </svg>
              </div>
              <h3 className="text-[15px] font-semibold text-[#1d1d1f] mb-1">
                Your first order will land here
              </h3>
              <p className="text-sm text-[#1d1d1f]/50 max-w-[360px]">
                We'll email you the moment it does. Until then, the launch list
                on the right will keep you busy.
              </p>
            </div>
          </div>

          {/* Right column — launch checklist */}
          <div className="bg-white rounded-xl border border-black/[0.06] p-5">
            <div className="flex items-center justify-between mb-1.5">
              <h3 className="text-[15px] font-semibold text-[#1d1d1f]">
                Launch checklist
              </h3>
              <span className="text-[11px] text-[#1d1d1f]/50 tabular-nums">
                {doneCount} / {checklist.length}
              </span>
            </div>

            {/* Progress bar */}
            <div className="h-1.5 bg-black/[0.06] rounded-full mb-4 overflow-hidden">
              <div
                className="h-full bg-[#1d1d1f] rounded-full"
                style={{ width: `${(doneCount / checklist.length) * 100}%` }}
              />
            </div>

            <div className="flex flex-col gap-3">
              {checklist.map((item) => (
                <div key={item.label} className="flex items-start gap-3">
                  <span
                    className="w-[22px] h-[22px] rounded-full mt-px shrink-0 flex items-center justify-center"
                    style={{
                      background: item.done ? "#1d1d1f" : "white",
                      border: item.done ? "none" : "1.5px solid #1d1d1f40",
                      color: "white",
                    }}
                  >
                    {item.done && (
                      <svg
                        aria-hidden="true"
                        width="11"
                        height="11"
                        viewBox="0 0 12 12"
                        fill="none"
                        stroke="currentColor"
                        strokeWidth="2.4"
                        strokeLinecap="round"
                        strokeLinejoin="round"
                      >
                        <polyline points="2,6 5,9 10,3" />
                      </svg>
                    )}
                  </span>
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2">
                      <span
                        className="text-[13.5px] font-medium"
                        style={{
                          textDecoration: item.done ? "line-through" : "none",
                          color: item.done ? "#1d1d1f99" : "#1d1d1f",
                        }}
                      >
                        {item.label}
                      </span>
                      {item.hint && (
                        <span className="text-[10px] font-medium px-1.5 py-0.5 rounded bg-amber-100 text-amber-700">
                          {item.hint}
                        </span>
                      )}
                    </div>
                    <p className="text-[11px] text-[#1d1d1f]/50 mt-0.5">
                      {item.subject}
                    </p>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

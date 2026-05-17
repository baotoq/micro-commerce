// web/src/app/seller/states/loading/page.tsx

function Sk({
  w = "60%",
  h = 14,
  r = 4,
}: {
  w?: number | string;
  h?: number;
  r?: number;
}) {
  return (
    <div
      data-testid="skeleton"
      style={{
        width: typeof w === "number" ? w : w,
        height: h,
        borderRadius: r,
        background:
          "linear-gradient(90deg, rgba(21,18,14,0.06) 0%, rgba(21,18,14,0.12) 50%, rgba(21,18,14,0.06) 100%)",
        backgroundSize: "200% 100%",
        animation: "hf-shimmer 1.4s linear infinite",
        flexShrink: 0,
      }}
    />
  );
}

export default function SellerStatesLoading() {
  return (
    <div
      aria-busy="true"
      aria-live="polite"
      className="flex flex-col min-h-screen"
    >
      {/* Top-bar skeleton */}
      <div className="flex items-center justify-between px-7 py-5 border-b border-black/[0.06]">
        <div className="flex flex-col gap-2">
          <Sk w={140} h={10} />
          <Sk w={220} h={26} />
        </div>
        <div className="flex gap-2">
          <Sk w={94} h={32} r={999} />
          <Sk w={120} h={32} r={999} />
        </div>
      </div>

      {/* Main content */}
      <div className="flex-1 overflow-auto px-7 py-6">
        {/* KPI grid: 4 columns */}
        <div
          className="grid gap-3 mb-5"
          style={{ gridTemplateColumns: "repeat(4, 1fr)" }}
        >
          {[0, 1, 2, 3].map((i) => (
            <div
              key={i}
              className="flex flex-col gap-2 bg-white border border-black/[0.06] rounded-xl p-[18px]"
            >
              <Sk w="40%" h={10} />
              <Sk w="65%" h={28} />
              <Sk w="100%" h={36} />
            </div>
          ))}
        </div>

        {/* Lower grid: 1.6fr / 1fr */}
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "1.6fr 1fr" }}
        >
          {/* Chart card */}
          <div className="flex flex-col gap-3 bg-white border border-black/[0.06] rounded-xl p-5">
            <Sk w="20%" h={10} />
            <Sk w="35%" h={28} />
            <div style={{ height: 200, marginTop: 8 }}>
              <Sk w="100%" h={200} r={8} />
            </div>
          </div>

          {/* Activity list card */}
          <div className="flex flex-col gap-3 bg-white border border-black/[0.06] rounded-xl p-5">
            <Sk w="40%" h={14} />
            {[0, 1, 2, 3].map((i) => (
              <div key={i} className="flex items-start gap-3 mt-1">
                <Sk w={10} h={10} r={999} />
                <div className="flex flex-col gap-2 flex-1">
                  <Sk w="80%" h={12} />
                  <Sk w="50%" h={9} />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}

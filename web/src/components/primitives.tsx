import Image from "next/image";

export type ImageTone =
  | "terra"
  | "sage"
  | "clay"
  | "bone"
  | "shadow"
  | "rose"
  | "moss"
  | "cobalt"
  | "cream"
  | "rust"
  | "violet"
  | "coal";

export function Stars({
  n = 5,
  of = 5,
  size = 11,
}: {
  n?: number;
  of?: number;
  size?: number;
}) {
  return (
    <span className="hf-rating" style={{ fontSize: size }}>
      {Array.from({ length: of }).map((_, i) => (
        <span
          // biome-ignore lint/suspicious/noArrayIndexKey: positional star slots never reorder
          key={i}
          className="hf-star"
          style={{
            color: i < n ? "var(--sun)" : "var(--ink-5)",
            width: size,
            height: size,
          }}
        />
      ))}
    </span>
  );
}

export function Avatar({
  name = "A",
  size = "md",
  src,
}: {
  name?: string;
  size?: "sm" | "md" | "lg" | "xl";
  src?: string;
}) {
  const cls =
    size === "sm"
      ? "hf-avatar hf-avatar-sm"
      : size === "lg"
        ? "hf-avatar hf-avatar-lg"
        : size === "xl"
          ? "hf-avatar hf-avatar-xl"
          : "hf-avatar";
  const colors = [
    "#E8C2B5",
    "#C9D4BC",
    "#E5C0A6",
    "#F0E8D7",
    "#DCAE9F",
    "#B6C4AB",
    "#D9B49A",
    "#F0E5D0",
    "#C7754F",
    "#8D759F",
  ];
  const c = colors[name.charCodeAt(0) % colors.length];
  if (src) {
    return (
      <div
        className={cls}
        style={{
          background: c,
          color: "#15120E",
          borderColor: "rgba(0,0,0,0.08)",
          overflow: "hidden",
        }}
      >
        <Image src={src} alt={name} fill style={{ objectFit: "cover" }} />
      </div>
    );
  }
  return (
    <div
      className={cls}
      style={{
        background: c,
        color: "#15120E",
        borderColor: "rgba(0,0,0,0.08)",
      }}
    >
      {name
        .split(" ")
        .slice(0, 2)
        .map((s) => s[0])
        .join("")
        .toUpperCase()}
    </div>
  );
}

export function ProductImage({
  tone,
  badge,
  label,
  h,
  r,
}: {
  tone: ImageTone;
  badge?: string;
  label?: string;
  h: number;
  r?: string;
}) {
  return (
    <div
      className={`hf-img hf-img-${tone}`}
      style={{
        width: "100%",
        height: h,
        borderRadius: r ?? "var(--r-md)",
        position: "relative",
      }}
    >
      {badge && (
        <div
          style={{
            position: "absolute",
            top: 8,
            left: 8,
            padding: "3px 8px",
            borderRadius: 999,
            background: "rgba(255,255,255,0.92)",
            fontSize: 10,
            fontWeight: 600,
            letterSpacing: 0.04,
            color: "var(--ink)",
          }}
        >
          {badge}
        </div>
      )}
      {label && (
        <div
          style={{
            position: "absolute",
            bottom: 8,
            right: 8,
            padding: "2px 6px",
            borderRadius: 4,
            background: "rgba(0,0,0,0.5)",
            color: "white",
            fontSize: 9,
            fontWeight: 500,
            letterSpacing: 0.04,
          }}
        >
          {label}
        </div>
      )}
    </div>
  );
}

export function Spark({
  data,
  color = "currentColor",
  fill = "transparent",
  sw = 1.5,
}: {
  data: number[];
  color?: string;
  fill?: string;
  sw?: number;
}) {
  const w = 100,
    h = 32;
  const pts = data
    .map((v, i) => {
      const x = (i / (data.length - 1)) * w;
      const y = h - 2 - v * (h - 4);
      return `${i ? "L" : "M"}${x.toFixed(1)} ${y.toFixed(1)}`;
    })
    .join(" ");
  const fillP = fill !== "transparent" ? `${pts} L${w} ${h} L0 ${h} Z` : null;
  return (
    <svg
      aria-hidden="true"
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      className="hf-spark"
    >
      {fillP && <path d={fillP} fill={fill} />}
      <path
        d={pts}
        fill="none"
        stroke={color}
        strokeWidth={sw}
        strokeLinejoin="round"
        strokeLinecap="round"
        vectorEffect="non-scaling-stroke"
      />
    </svg>
  );
}

export function Bars({
  data,
  color = "var(--ink)",
  max,
}: {
  data: number[];
  color?: string;
  max?: number;
}) {
  const m = max ?? Math.max(...data);
  return (
    <div className="hf-row hf-items-end" style={{ gap: 4, height: "100%" }}>
      {data.map((v, i) => (
        <div
          // biome-ignore lint/suspicious/noArrayIndexKey: bar order matches data order
          key={i}
          className="hf-grow"
          style={{
            height: `${(v / m) * 100}%`,
            background: color,
            borderRadius: "2px 2px 0 0",
            minHeight: 2,
          }}
        />
      ))}
    </div>
  );
}

export function AreaChart({
  data,
  color = "var(--ink)",
  fill = "rgba(21,18,14,0.06)",
  height = 160,
  gridY = 4,
}: {
  data: number[];
  color?: string;
  fill?: string;
  height?: number;
  gridY?: number;
}) {
  const w = 600,
    h = height;
  const max = Math.max(...data) * 1.1,
    min = 0;
  const pts = data.map((v, i) => {
    const x = (i / (data.length - 1)) * w;
    const y = h - ((v - min) / (max - min)) * h;
    return [x, y] as [number, number];
  });
  const path = pts
    .map((p, i) => `${i ? "L" : "M"}${p[0].toFixed(1)} ${p[1].toFixed(1)}`)
    .join(" ");
  const fillPath = `${path} L${w} ${h} L0 ${h} Z`;
  return (
    <svg
      aria-hidden="true"
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      style={{ width: "100%", height: "100%", display: "block" }}
    >
      {Array.from({ length: gridY + 1 }).map((_, i) => {
        const y = (i / gridY) * h;
        return (
          <line
            // biome-ignore lint/suspicious/noArrayIndexKey: grid lines are positional
            key={i}
            x1={0}
            y1={y}
            x2={w}
            y2={y}
            stroke="var(--line)"
            strokeWidth="0.5"
            vectorEffect="non-scaling-stroke"
            strokeDasharray="2 3"
          />
        );
      })}
      <path d={fillPath} fill={fill} />
      <path
        d={path}
        fill="none"
        stroke={color}
        strokeWidth="2"
        vectorEffect="non-scaling-stroke"
        strokeLinejoin="round"
        strokeLinecap="round"
      />
    </svg>
  );
}

export function Donut({
  segments,
  size = 100,
  thickness = 14,
}: {
  segments: { value: number; color: string }[];
  size?: number;
  thickness?: number;
}) {
  const r = size / 2 - thickness / 2;
  const c = 2 * Math.PI * r;
  const total = segments.reduce((s, x) => s + x.value, 0);
  let off = 0;
  return (
    <svg
      aria-hidden="true"
      width={size}
      height={size}
      viewBox={`0 0 ${size} ${size}`}
      style={{ transform: "rotate(-90deg)" }}
    >
      <circle
        cx={size / 2}
        cy={size / 2}
        r={r}
        fill="none"
        stroke="var(--paper-2)"
        strokeWidth={thickness}
      />
      {segments.map((seg, i) => {
        const len = (seg.value / total) * c;
        const dash = `${len} ${c - len}`;
        const el = (
          <circle
            // biome-ignore lint/suspicious/noArrayIndexKey: donut segments are positional
            key={i}
            cx={size / 2}
            cy={size / 2}
            r={r}
            fill="none"
            stroke={seg.color}
            strokeWidth={thickness}
            strokeDasharray={dash}
            strokeDashoffset={-off}
            strokeLinecap="butt"
          />
        );
        off += len;
        return el;
      })}
    </svg>
  );
}

export function PhoneStatus({ time = "9:41" }: { time?: string }) {
  return (
    <div className="hf-phone-status">
      <span style={{ fontVariantNumeric: "tabular-nums" }}>{time}</span>
      <div className="hf-phone-status-icons">
        <svg
          aria-hidden="true"
          width="17"
          height="11"
          viewBox="0 0 17 11"
          fill="currentColor"
        >
          <rect x="0" y="7" width="3" height="4" rx="0.5" />
          <rect x="4" y="5" width="3" height="6" rx="0.5" />
          <rect x="8" y="3" width="3" height="8" rx="0.5" />
          <rect x="12" y="0" width="3" height="11" rx="0.5" />
        </svg>
        <svg
          aria-hidden="true"
          width="15"
          height="11"
          viewBox="0 0 15 11"
          fill="currentColor"
        >
          <path d="M7.5 0C4.6 0 1.9 1.1 0 2.9l1.4 1.4A8.5 8.5 0 0 1 7.5 2c2.3 0 4.4.9 6.1 2.4L15 2.9C13.1 1.1 10.4 0 7.5 0zm0 4C5.7 4 3.9 4.7 2.5 5.9l1.4 1.4A5.5 5.5 0 0 1 7.5 6c1.4 0 2.7.5 3.6 1.3l1.4-1.4A7.4 7.4 0 0 0 7.5 4zm0 4a3 3 0 0 0-2.1.9l1.5 1.5a1.5 1.5 0 0 1 1.2 0l1.5-1.5A3 3 0 0 0 7.5 8z" />
        </svg>
        <svg
          aria-hidden="true"
          width="25"
          height="11"
          viewBox="0 0 25 11"
          fill="none"
        >
          <rect
            x="0.5"
            y="0.5"
            width="21"
            height="10"
            rx="2.5"
            stroke="currentColor"
          />
          <rect x="2" y="2" width="18" height="7" rx="1" fill="currentColor" />
          <rect
            x="22.5"
            y="3.5"
            width="1.5"
            height="4"
            rx="0.5"
            fill="currentColor"
          />
        </svg>
      </div>
    </div>
  );
}

export function PhoneHome() {
  return <div className="hf-phone-home" />;
}

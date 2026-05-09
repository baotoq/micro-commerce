export function Sparkline({
  points,
  className = "h-8 w-full text-[#0066cc]",
}: {
  points: number[];
  className?: string;
}) {
  const w = 100;
  const h = 32;
  const max = Math.max(...points, 0.0001);
  const step = points.length > 1 ? w / (points.length - 1) : w;
  const path = points
    .map(
      (v, i) =>
        `${i === 0 ? "M" : "L"}${(i * step).toFixed(2)},${(h - (v / max) * h).toFixed(2)}`,
    )
    .join(" ");
  const area = `${path} L${w},${h} L0,${h} Z`;
  return (
    <svg
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      className={className}
      role="img"
      aria-label="Sparkline chart"
    >
      <path d={area} fill="currentColor" fillOpacity="0.12" />
      <path d={path} fill="none" stroke="currentColor" strokeWidth="1.5" />
    </svg>
  );
}

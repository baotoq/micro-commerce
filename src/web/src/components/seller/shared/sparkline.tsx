type SparklineProps = {
  /**
   * Data points (ordered, evenly spaced). Empty arrays render an empty SVG.
   */
  points: number[];
  /**
   * Required, meaningful accessible label — surfaced as both `aria-label` and
   * the SVG `<title>` so screen readers, tooltips, and headless inspections
   * carry the same summary instead of a generic "Sparkline chart" placeholder.
   */
  label: string;
  className?: string;
};

export function Sparkline({
  points,
  label,
  className = "h-8 w-full text-primary",
}: SparklineProps) {
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

  // Synthesize a delta summary so the accessible title carries data, not just
  // a label. Skip when the series is too short to be meaningful.
  const first = points[0];
  const last = points[points.length - 1];
  const summary =
    points.length >= 2 && first !== undefined && last !== undefined && first > 0
      ? `${label} — start ${first}, end ${last} (${last >= first ? "+" : ""}${(((last - first) / first) * 100).toFixed(0)}%)`
      : label;

  return (
    <svg
      viewBox={`0 0 ${w} ${h}`}
      preserveAspectRatio="none"
      className={className}
      role="img"
      aria-label={label}
    >
      <title>{summary}</title>
      <path d={area} fill="currentColor" fillOpacity="0.12" />
      <path d={path} fill="none" stroke="currentColor" strokeWidth="1.5" />
    </svg>
  );
}

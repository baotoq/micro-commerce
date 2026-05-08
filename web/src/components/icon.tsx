const paths: Record<string, string> = {
  cart: "M3 4h2l1.5 8.5a2 2 0 0 0 2 1.5h6a2 2 0 0 0 2-1.5L18 7H6",
  bag: "M5 7h10l-1 9.5a1.5 1.5 0 0 1-1.5 1.5h-5A1.5 1.5 0 0 1 6 16.5L5 7zm2 0a3 3 0 0 1 6 0",
  search: "M9 3a6 6 0 1 1 0 12A6 6 0 0 1 9 3zm5 10l3.5 3.5",
  heart:
    "M10 16s-6-3.5-6-8a3.5 3.5 0 0 1 6-2.5A3.5 3.5 0 0 1 16 8c0 4.5-6 8-6 8z",
  user: "M10 10a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm-6 8a6 6 0 0 1 12 0",
  menu: "M3 6h14M3 10h14M3 14h14",
  plus: "M10 4v12M4 10h12",
  minus: "M4 10h12",
  x: "M5 5l10 10M15 5L5 15",
  check: "M4 10l4 4 8-8",
  chevR: "M8 5l5 5-5 5",
  chevL: "M12 5l-5 5 5 5",
  chevD: "M5 8l5 5 5-5",
  chevU: "M5 12l5-5 5 5",
  arrowR: "M4 10h12M12 6l4 4-4 4",
  arrowL: "M16 10H4M8 6l-4 4 4 4",
  star: "M10 2l2.5 5 5.5.8-4 3.9.9 5.5L10 14.5 5.1 17.2 6 11.7 2 7.8l5.5-.8z",
  home: "M3 10l7-7 7 7M5 9v8h10V9",
  grid: "M3 3h6v6H3zM11 3h6v6h-6zM3 11h6v6H3zM11 11h6v6h-6z",
  list: "M6 5h12M6 10h12M6 15h12M3 5h0M3 10h0M3 15h0",
  chart: "M3 17V3M3 17h14M6 13l3-4 3 2 4-6",
  pkg: "M3 6l7-3 7 3v8l-7 3-7-3V6zM3 6l7 3 7-3M10 9v8",
  truck: "M2 14V5h10v9m0-5h3l3 3v2h-1m-15 0h2m12 0h-3m-7 0h7",
  card: "M2 6h16v9H2zM2 9h16",
  pin: "M10 17s-5-5-5-9a5 5 0 0 1 10 0c0 4-5 9-5 9zM10 8a1 1 0 1 0 0 2 1 1 0 0 0 0-2z",
  bell: "M5 13V9a5 5 0 0 1 10 0v4l1.5 2h-13L5 13zm3 4h4",
  shop: "M3 7l1-3h12l1 3M3 7v10h14V7M3 7c0 2 2 2 2 0M5 7c0 2 2 2 2 0M7 7c0 2 2 2 2 0M9 7c0 2 2 2 2 0M11 7c0 2 2 2 2 0M13 7c0 2 2 2 2 0M15 7c0 2 2 2 2 0",
  sett: "M10 7a3 3 0 1 0 0 6 3 3 0 0 0 0-6zM10 1v3m0 12v3M3 10H1m18 0h-2m-3-7l-2 2m-6 6l-2 2m0-10l2 2m6 6l2 2",
  wallet:
    "M2 6h13a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6zm0 0V5a1 1 0 0 1 1-1h11M14 11h2",
  box: "M3 6h14v11H3zM3 6l2-3h10l2 3M10 6v11M3 11h14",
  play: "M6 4l10 6-10 6V4z",
  cam: "M2 6h3l1.5-2h7L15 6h3v10H2zM10 14a3 3 0 1 0 0-6 3 3 0 0 0 0 6z",
  img: "M3 4h14v12H3zM3 13l4-4 5 5 2-2 3 3M7 8a1 1 0 1 0 0-2 1 1 0 0 0 0 2z",
  tag: "M3 3h7l7 7-7 7-7-7V3zM6 6h0",
  eye: "M2 10s3-5 8-5 8 5 8 5-3 5-8 5-8-5-8-5zm8 2.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z",
  edit: "M3 17l1-4 9-9 3 3-9 9-4 1zM12 5l3 3",
  trash: "M4 6h12m-9 0V4h6v2m-6 0v10h6V6",
  upload: "M10 14V3m-4 4l4-4 4 4M3 16h14",
  dl: "M10 3v11m-4-4l4 4 4-4M3 16h14",
  filter: "M3 5h14M5 10h10M8 15h4",
  sort: "M6 4v12m0 0l-3-3m3 3l3-3M14 16V4m0 0l-3 3m3-3l3 3",
  sun: "M10 6a4 4 0 1 0 0 8 4 4 0 0 0 0-8zM10 1v2M10 17v2M3 10H1m18 0h-2m-13 6L3 17m13-13L17 3M4 4L3 3m13 13l1 1",
  moon: "M16 11A6 6 0 1 1 9 4a5 5 0 0 0 7 7z",
  refresh: "M3 4v4h4M17 16v-4h-4M5 12a6 6 0 0 0 11 2M15 8A6 6 0 0 0 4 6",
  info: "M10 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16zM10 9v5m0-7v0",
  ext: "M5 5h4M5 5v4M5 5l6 6M11 3h6v6",
  palette:
    "M10 2a8 8 0 0 0 0 16h2a1 1 0 0 0 1-1 1 1 0 0 1 1-1h2a3 3 0 0 0 3-3v-3a8 8 0 0 0-9-8zM6 9a1 1 0 1 0 0 2 1 1 0 0 0 0-2zM10 6a1 1 0 1 0 0 2 1 1 0 0 0 0-2zM14 9a1 1 0 1 0 0 2 1 1 0 0 0 0-2z",
  palette2:
    "M3 10a7 7 0 1 0 14 0c0-2-2-2-2-4s2-3 0-3a7 7 0 0 0-12 7zM6 8h0M9 5h0M13 7h0M14 11h0",
  bolt: "M11 2l-6 9h4l-1 7 6-9h-4z",
  tasks: "M3 4h14M3 10h14M3 16h14M0 4h0M0 10h0M0 16h0",
  inbox: "M3 11l3-7h8l3 7v6H3zM3 11h4l1 2h4l1-2h4",
  flag: "M4 17V3M4 3h11l-2 4 2 4H4",
  chat: "M3 4h14v9H8l-4 4v-4H3z",
  pencil: "M3 17l1-4 9-9 3 3-9 9-4 1z",
  zap: "M11 1l-7 11h5l-1 7 7-11h-5z",
  apple:
    "M11 4c0-1 1-2 2-2 0 1-1 2-2 2zm-1 0a3 3 0 0 0-3 3c0 4 2 9 4 9 1 0 1-1 2-1s1 1 2 1c2 0 4-5 4-9a3 3 0 0 0-3-3c-1 0-2 1-3 1s-2-1-3-1z",
  google:
    "M17 10c0 4-3 7-7 7s-7-3-7-7 3-7 7-7c2 0 4 1 5 2l-2 2c-1-1-2-1-3-1a4 4 0 0 0 0 8c2 0 3-1 4-3h-4v-2h6c0 0 1 1 1 0z",
};

export type IconName = keyof typeof paths;

export function Icon({
  n,
  s = 14,
  sw = 1.6,
  className,
}: {
  n: IconName;
  s?: number;
  sw?: number;
  className?: string;
}) {
  return (
    <svg
      aria-hidden="true"
      width={s}
      height={s}
      viewBox="0 0 20 20"
      fill="none"
      stroke="currentColor"
      strokeWidth={sw}
      strokeLinecap="round"
      strokeLinejoin="round"
      style={{ flexShrink: 0 }}
      className={className}
    >
      <path d={paths[n] ?? ""} />
    </svg>
  );
}

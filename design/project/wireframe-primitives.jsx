// Wireframe primitives — sketchy boxes, phone/desktop frames, annotations.
// Loaded as Babel script; exports to window for sibling scripts.

const { useState, useEffect, useRef, useMemo } = React;

// Deterministic small-rotate for hand-drawn wobble per-element.
// Hashing the key into a stable -1.2deg..+1.2deg keeps it consistent across renders.
function wobble(seed) {
  let h = 0;
  const s = String(seed);
  for (let i = 0; i < s.length; i++) h = (h * 31 + s.charCodeAt(i)) | 0;
  const r = ((h % 100) - 50) / 42; // ~-1.2..1.2
  return `${r.toFixed(2)}deg`;
}

// Section divider scribble
const Scribble = ({ w = 60, h = 14 }) => (
  <svg width={w} height={h} viewBox="0 0 60 14" style={{ display: 'block' }}>
    <path d="M2 8 Q 8 2, 14 8 T 28 8 T 42 8 T 58 8" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" />
  </svg>
);

// An annotation pill that points at something. anchor is {top|bottom|left|right} on the parent
const Anno = ({ children, x, y, dir = 'right', length = 36 }) => {
  const arrow = {
    right: <svg width={length} height="14" viewBox={`0 0 ${length} 14`}><path d={`M2 7 Q ${length / 2} 1, ${length - 4} 7`} fill="none" stroke="currentColor" strokeWidth="1.4" /><path d={`M${length - 8} 3 L${length - 2} 7 L${length - 8} 11`} fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" /></svg>,
    left: <svg width={length} height="14" viewBox={`0 0 ${length} 14`}><path d={`M${length - 2} 7 Q ${length / 2} 1, 4 7`} fill="none" stroke="currentColor" strokeWidth="1.4" /><path d={`M8 3 L2 7 L8 11`} fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" /></svg>,
    down: <svg width="14" height={length} viewBox={`0 0 14 ${length}`}><path d={`M7 2 Q 13 ${length / 2}, 7 ${length - 4}`} fill="none" stroke="currentColor" strokeWidth="1.4" /><path d={`M3 ${length - 8} L7 ${length - 2} L11 ${length - 8}`} fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" /></svg>,
    up: <svg width="14" height={length} viewBox={`0 0 14 ${length}`}><path d={`M7 ${length - 2} Q 13 ${length / 2}, 7 4`} fill="none" stroke="currentColor" strokeWidth="1.4" /><path d={`M3 8 L7 2 L11 8`} fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" /></svg>,
  }[dir];
  const flexDir = dir === 'right' ? 'row' : dir === 'left' ? 'row-reverse' : dir === 'down' ? 'column' : 'column-reverse';
  return (
    <div className="wf-anno" style={{ left: x, top: y, flexDirection: flexDir }}>
      {arrow}
      <div className="wf-anno-text">{children}</div>
    </div>
  );
};

// Placeholder image with optional caption
const ImgBox = ({ w, h, label, dark, plain, style, children }) => (
  <div className={`wf-img${dark ? ' wf-img-dark' : ''}${plain ? ' wf-img-plain' : ''}`}
    style={{ width: w, height: h, ...style }}>
    {children}
    {label && (
      <div style={{
        position: 'absolute', inset: 0, display: 'flex', alignItems: 'center', justifyContent: 'center',
        fontFamily: 'var(--hand-display)', fontWeight: 600, fontSize: 16,
        color: dark ? '#bbb' : 'var(--ink-soft)', textAlign: 'center', padding: 8, zIndex: 2,
      }}>{label}</div>
    )}
  </div>
);

// Text placeholder lines — fills width, n lines, last line shorter
const Lines = ({ n = 3, last = 0.7, gap = 6, h = 8, style }) => (
  <div style={{ display: 'flex', flexDirection: 'column', gap, ...style }}>
    {Array.from({ length: n }).map((_, i) => (
      <div key={i} className="wf-line" style={{
        height: h, margin: 0,
        width: i === n - 1 ? `${last * 100}%` : '100%',
      }} />
    ))}
  </div>
);

// A handwritten "title" — actual readable text
const Heading = ({ children, size = 'h2', style }) => {
  const cls = size === 'h1' ? 'wf-h1' : size === 'h3' ? 'wf-h3' : 'wf-h2';
  return <div className={cls} style={style}>{children}</div>;
};

// Sketchy button
const Btn = ({ children, fill, accent, ghost, sm, lg, block, style, className = '' }) => (
  <div className={`wf-btn${fill ? ' wf-btn-fill' : ''}${accent ? ' wf-btn-accent' : ''}${ghost ? ' wf-btn-ghost' : ''}${sm ? ' wf-btn-sm' : ''}${lg ? ' wf-btn-lg' : ''}${block ? ' wf-btn-block' : ''} ${className}`}
    style={style}>{children}</div>
);

// Chip
const Chip = ({ children, on, accent, style }) => (
  <div className={`wf-chip${on ? ' wf-chip-on' : ''}${accent ? ' wf-chip-accent' : ''}`} style={style}>{children}</div>
);

// Phone frame — 360x720 inner
const Phone = ({ children, statusTitle, dark }) => (
  <div className={`wf ${dark ? 'wf-dark' : ''}`}>
    <div className="wf-phone">
      <div className="wf-phone-status">
        <span style={{ fontWeight: 600 }}>9:41</span>
        <span style={{ fontFamily: 'var(--hand-display)', fontWeight: 600 }}>{statusTitle}</span>
        <span>•••</span>
      </div>
      <div style={{ flex: 1, position: 'relative', overflow: 'hidden' }}>{children}</div>
      <div className="wf-phone-home" />
    </div>
  </div>
);

// Desktop browser frame
const Desktop = ({ children, url = 'micro.commerce' }) => (
  <div className="wf">
    <div className="wf-desktop-chrome">
      <div className="wf-tl" />
      <div className="wf-tl" />
      <div className="wf-tl" />
      <div className="wf-url">{url}</div>
    </div>
    <div style={{ height: 'calc(100% - 30px)', overflow: 'hidden', position: 'relative' }}>{children}</div>
  </div>
);

// Tiny icon glyphs (sketchy SVG) — minimal, sized 16
const Icon = ({ name, size = 16 }) => {
  const stroke = 'currentColor';
  const sw = 1.4;
  const paths = {
    bag: <path d="M5 6 L5 14 H15 L15 6 M7 6 V4.5 Q10 2 13 4.5 V6" fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />,
    search: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><circle cx="9" cy="9" r="4.5" /><path d="M12.5 12.5 L15 15" /></g>,
    user: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><circle cx="10" cy="7" r="3" /><path d="M4 16 Q 10 11 16 16" /></g>,
    menu: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><path d="M3 6 H17 M3 10 H17 M3 14 H17" /></g>,
    cart: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><path d="M3 4 H5 L7 13 H15 L17 6 H6" /><circle cx="8" cy="16" r="0.8" fill={stroke} /><circle cx="14" cy="16" r="0.8" fill={stroke} /></g>,
    plus: <path d="M10 4 V16 M4 10 H16" stroke={stroke} strokeWidth={sw} strokeLinecap="round" />,
    chev_r: <path d="M7 4 L13 10 L7 16" fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />,
    chev_d: <path d="M4 7 L10 13 L16 7" fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />,
    chev_l: <path d="M13 4 L7 10 L13 16" fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />,
    x: <path d="M5 5 L15 15 M15 5 L5 15" stroke={stroke} strokeWidth={sw} strokeLinecap="round" />,
    heart: <path d="M10 16 Q 3 11 3 7 Q 3 3 6.5 3 Q 9 3 10 6 Q 11 3 13.5 3 Q 17 3 17 7 Q 17 11 10 16Z" fill="none" stroke={stroke} strokeWidth={sw} strokeLinejoin="round" />,
    check: <path d="M4 10 L8.5 14 L16 5" fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />,
    star: <path d="M10 3 L12 8 L17 8.5 L13 12 L14.5 17 L10 14 L5.5 17 L7 12 L3 8.5 L8 8 Z" fill="none" stroke={stroke} strokeWidth={sw} strokeLinejoin="round" />,
    circle: <circle cx="10" cy="10" r="6" fill="none" stroke={stroke} strokeWidth={sw} />,
    dot: <circle cx="10" cy="10" r="2" fill={stroke} />,
    arrow_r: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round"><path d="M3 10 H16" /><path d="M12 6 L16 10 L12 14" /></g>,
    arrow_u: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round"><path d="M10 16 V4" /><path d="M5 8 L10 4 L15 8" /></g>,
    arrow_d: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round"><path d="M10 4 V16" /><path d="M5 12 L10 16 L15 12" /></g>,
    home: <path d="M3 10 L10 4 L17 10 V16 H12 V12 H8 V16 H3 Z" fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" />,
    grid: <g fill="none" stroke={stroke} strokeWidth={sw}><rect x="3" y="3" width="6" height="6" /><rect x="11" y="3" width="6" height="6" /><rect x="3" y="11" width="6" height="6" /><rect x="11" y="11" width="6" height="6" /></g>,
    list: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><circle cx="4" cy="5" r="0.9" fill={stroke}/><circle cx="4" cy="10" r="0.9" fill={stroke}/><circle cx="4" cy="15" r="0.9" fill={stroke}/><path d="M7 5 H17 M7 10 H17 M7 15 H17" /></g>,
    pencil: <path d="M3 17 L4 13 L13 4 L16 7 L7 16 Z" fill="none" stroke={stroke} strokeWidth={sw} strokeLinejoin="round" />,
    eye: <g fill="none" stroke={stroke} strokeWidth={sw}><path d="M2 10 Q 10 3 18 10 Q 10 17 2 10 Z" /><circle cx="10" cy="10" r="2" /></g>,
    chart: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><path d="M3 16 L8 9 L12 13 L17 4" /><path d="M14 4 H17 V7" /></g>,
    box: <path d="M3 6 L10 3 L17 6 L17 14 L10 17 L3 14 Z M3 6 L10 9 L17 6 M10 9 V17" fill="none" stroke={stroke} strokeWidth={sw} strokeLinejoin="round" strokeLinecap="round" />,
    bell: <path d="M5 13 Q 5 6 10 6 Q 15 6 15 13 H17 H3 H5 Z M8 14 Q 10 17 12 14" fill="none" stroke={stroke} strokeWidth={sw} strokeLinejoin="round" strokeLinecap="round" />,
    settings: <g fill="none" stroke={stroke} strokeWidth={sw}><circle cx="10" cy="10" r="2.5" /><path d="M10 2 V4 M10 16 V18 M2 10 H4 M16 10 H18 M4.3 4.3 L5.7 5.7 M14.3 14.3 L15.7 15.7 M4.3 15.7 L5.7 14.3 M14.3 5.7 L15.7 4.3" strokeLinecap="round" /></g>,
    upload: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round"><path d="M3 14 V16 H17 V14" /><path d="M10 12 V3" /><path d="M6 7 L10 3 L14 7" /></g>,
    filter: <path d="M3 5 H17 L13 11 V16 L7 14 V11 L3 5 Z" fill="none" stroke={stroke} strokeWidth={sw} strokeLinejoin="round" />,
    sliders: <g fill="none" stroke={stroke} strokeWidth={sw} strokeLinecap="round"><path d="M3 6 H17" /><circle cx="7" cy="6" r="1.6" fill="white" /><path d="M3 14 H17" /><circle cx="13" cy="14" r="1.6" fill="white" /></g>,
  };
  return (
    <svg width={size} height={size} viewBox="0 0 20 20" style={{ flex: '0 0 auto', display: 'inline-block', verticalAlign: 'middle' }}>
      {paths[name] || paths.circle}
    </svg>
  );
};

// Tiny line/bar charts for sketchy data viz
const LineChart = ({ w = 200, h = 60, points, accent }) => {
  const max = Math.max(...points), min = Math.min(...points);
  const range = max - min || 1;
  const xs = points.map((_, i) => 4 + (i * (w - 8)) / (points.length - 1));
  const ys = points.map((p) => h - 4 - ((p - min) / range) * (h - 12));
  const d = xs.map((x, i) => `${i === 0 ? 'M' : 'L'} ${x.toFixed(1)} ${ys[i].toFixed(1)}`).join(' ');
  return (
    <svg width={w} height={h} viewBox={`0 0 ${w} ${h}`} className="wf-chart">
      <path d={`${d} L ${xs[xs.length - 1]} ${h - 2} L ${xs[0]} ${h - 2} Z`} fill={accent ? 'var(--accent)' : 'var(--ink)'} fillOpacity="0.08" />
      <path d={d} fill="none" stroke={accent ? 'var(--accent)' : 'var(--ink)'} strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" />
      {xs.map((x, i) => i === xs.length - 1 && (
        <circle key={i} cx={x} cy={ys[i]} r="2.5" fill={accent ? 'var(--accent)' : 'var(--ink)'} />
      ))}
    </svg>
  );
};

const BarChart = ({ w = 200, h = 60, values, accent }) => {
  const max = Math.max(...values) || 1;
  const bw = (w - 4) / values.length - 4;
  return (
    <svg width={w} height={h} viewBox={`0 0 ${w} ${h}`} className="wf-chart">
      {values.map((v, i) => {
        const bh = (v / max) * (h - 4);
        return <rect key={i} x={2 + i * (bw + 4)} y={h - bh - 2} width={bw} height={bh}
          fill={accent ? 'var(--accent)' : 'var(--ink)'} fillOpacity={i === values.length - 1 ? 0.85 : 0.45} rx="1" />;
      })}
    </svg>
  );
};

// Tab bar (mobile bottom)
const TabBar = ({ active = 0, items = ['Shop', 'Search', 'Bag', 'You'] }) => {
  const icons = ['home', 'search', 'bag', 'user'];
  return (
    <div style={{
      borderTop: '1.2px solid var(--ink-faint)', padding: '8px 0 4px',
      display: 'flex', justifyContent: 'space-around', background: 'var(--paper)',
    }}>
      {items.map((it, i) => (
        <div key={it} style={{
          display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2,
          color: i === active ? 'var(--ink)' : 'var(--ink-soft)',
          fontSize: 11, fontFamily: 'var(--hand)',
        }}>
          <Icon name={icons[i]} size={18} />
          <span>{it}</span>
        </div>
      ))}
    </div>
  );
};

Object.assign(window, { Scribble, Anno, ImgBox, Lines, Heading, Btn, Chip, Phone, Desktop, Icon, LineChart, BarChart, TabBar, wobble });

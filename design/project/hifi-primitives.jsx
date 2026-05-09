// hifi-primitives.jsx — shared icons + small UI bits

// Tiny icon set — strokes only, 16x16 viewBox, single-color via currentColor.
function Ico({ n, s = 14, sw = 1.6 }) {
  const p = {
    cart:   'M3 4h2l1.5 8.5a2 2 0 0 0 2 1.5h6a2 2 0 0 0 2-1.5L18 7H6',
    bag:    'M5 7h10l-1 9.5a1.5 1.5 0 0 1-1.5 1.5h-5A1.5 1.5 0 0 1 6 16.5L5 7zm2 0a3 3 0 0 1 6 0',
    search: 'M9 3a6 6 0 1 1 0 12A6 6 0 0 1 9 3zm5 10l3.5 3.5',
    heart:  'M10 16s-6-3.5-6-8a3.5 3.5 0 0 1 6-2.5A3.5 3.5 0 0 1 16 8c0 4.5-6 8-6 8z',
    user:   'M10 10a3 3 0 1 0 0-6 3 3 0 0 0 0 6zm-6 8a6 6 0 0 1 12 0',
    menu:   'M3 6h14M3 10h14M3 14h14',
    plus:   'M10 4v12M4 10h12',
    minus:  'M4 10h12',
    x:      'M5 5l10 10M15 5L5 15',
    check:  'M4 10l4 4 8-8',
    chevR:  'M8 5l5 5-5 5',
    chevL:  'M12 5l-5 5 5 5',
    chevD:  'M5 8l5 5 5-5',
    chevU:  'M5 12l5-5 5 5',
    arrowR: 'M4 10h12M12 6l4 4-4 4',
    arrowL: 'M16 10H4M8 6l-4 4 4 4',
    star:   'M10 2l2.5 5 5.5.8-4 3.9.9 5.5L10 14.5 5.1 17.2 6 11.7 2 7.8l5.5-.8z',
    home:   'M3 10l7-7 7 7M5 9v8h10V9',
    grid:   'M3 3h6v6H3zM11 3h6v6h-6zM3 11h6v6H3zM11 11h6v6h-6z',
    list:   'M6 5h12M6 10h12M6 15h12M3 5h0M3 10h0M3 15h0',
    chart:  'M3 17V3M3 17h14M6 13l3-4 3 2 4-6',
    pkg:    'M3 6l7-3 7 3v8l-7 3-7-3V6zM3 6l7 3 7-3M10 9v8',
    truck:  'M2 14V5h10v9m0-5h3l3 3v2h-1m-15 0h2m12 0h-3m-7 0h7',
    card:   'M2 6h16v9H2zM2 9h16',
    pin:    'M10 17s-5-5-5-9a5 5 0 0 1 10 0c0 4-5 9-5 9zM10 8a1 1 0 1 0 0 2 1 1 0 0 0 0-2z',
    bell:   'M5 13V9a5 5 0 0 1 10 0v4l1.5 2h-13L5 13zm3 4h4',
    shop:   'M3 7l1-3h12l1 3M3 7v10h14V7M3 7c0 2 2 2 2 0M5 7c0 2 2 2 2 0M7 7c0 2 2 2 2 0M9 7c0 2 2 2 2 0M11 7c0 2 2 2 2 0M13 7c0 2 2 2 2 0M15 7c0 2 2 2 2 0',
    sett:   'M10 7a3 3 0 1 0 0 6 3 3 0 0 0 0-6zM10 1v3m0 12v3M3 10H1m18 0h-2m-3-7l-2 2m-6 6l-2 2m0-10l2 2m6 6l2 2',
    wallet: 'M2 6h13a2 2 0 0 1 2 2v6a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V6zm0 0V5a1 1 0 0 1 1-1h11M14 11h2',
    box:    'M3 6h14v11H3zM3 6l2-3h10l2 3M10 6v11M3 11h14',
    play:   'M6 4l10 6-10 6V4z',
    cam:    'M2 6h3l1.5-2h7L15 6h3v10H2zM10 14a3 3 0 1 0 0-6 3 3 0 0 0 0 6z',
    img:    'M3 4h14v12H3zM3 13l4-4 5 5 2-2 3 3M7 8a1 1 0 1 0 0-2 1 1 0 0 0 0 2z',
    tag:    'M3 3h7l7 7-7 7-7-7V3zM6 6h0',
    eye:    'M2 10s3-5 8-5 8 5 8 5-3 5-8 5-8-5-8-5zm8 2.5a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5z',
    edit:   'M3 17l1-4 9-9 3 3-9 9-4 1zM12 5l3 3',
    trash:  'M4 6h12m-9 0V4h6v2m-6 0v10h6V6',
    upload: 'M10 14V3m-4 4l4-4 4 4M3 16h14',
    dl:     'M10 3v11m-4-4l4 4 4-4M3 16h14',
    filter: 'M3 5h14M5 10h10M8 15h4',
    sort:   'M6 4v12m0 0l-3-3m3 3l3-3M14 16V4m0 0l-3 3m3-3l3 3',
    sun:    'M10 6a4 4 0 1 0 0 8 4 4 0 0 0 0-8zM10 1v2M10 17v2M3 10H1m18 0h-2m-13 6L3 17m13-13L17 3M4 4L3 3m13 13l1 1',
    moon:   'M16 11A6 6 0 1 1 9 4a5 5 0 0 0 7 7z',
    refresh:'M3 4v4h4M17 16v-4h-4M5 12a6 6 0 0 0 11 2M15 8A6 6 0 0 0 4 6',
    info:   'M10 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16zM10 9v5m0-7v0',
    ext:    'M5 5h4M5 5v4M5 5l6 6M11 3h6v6',
    palette:'M10 2a8 8 0 0 0 0 16h2a1 1 0 0 0 1-1 1 1 0 0 1 1-1h2a3 3 0 0 0 3-3v-3a8 8 0 0 0-9-8zM6 9a1 1 0 1 0 0 2 1 1 0 0 0 0-2zM10 6a1 1 0 1 0 0 2 1 1 0 0 0 0-2zM14 9a1 1 0 1 0 0 2 1 1 0 0 0 0-2z',
    palette2:'M3 10a7 7 0 1 0 14 0c0-2-2-2-2-4s2-3 0-3a7 7 0 0 0-12 7zM6 8h0M9 5h0M13 7h0M14 11h0',
    bolt:   'M11 2l-6 9h4l-1 7 6-9h-4z',
    tasks:  'M3 4h14M3 10h14M3 16h14M0 4h0M0 10h0M0 16h0',
    inbox:  'M3 11l3-7h8l3 7v6H3zM3 11h4l1 2h4l1-2h4',
    flag:   'M4 17V3M4 3h11l-2 4 2 4H4',
    chat:   'M3 4h14v9H8l-4 4v-4H3z',
    pencil: 'M3 17l1-4 9-9 3 3-9 9-4 1z',
    zap:    'M11 1l-7 11h5l-1 7 7-11h-5z',
    apple:  'M11 4c0-1 1-2 2-2 0 1-1 2-2 2zm-1 0a3 3 0 0 0-3 3c0 4 2 9 4 9 1 0 1-1 2-1s1 1 2 1c2 0 4-5 4-9a3 3 0 0 0-3-3c-1 0-2 1-3 1s-2-1-3-1z',
    google: 'M17 10c0 4-3 7-7 7s-7-3-7-7 3-7 7-7c2 0 4 1 5 2l-2 2c-1-1-2-1-3-1a4 4 0 0 0 0 8c2 0 3-1 4-3h-4v-2h6c0 0 1 1 1 0z',
    alert:  'M10 2l8 14H2L10 2zM10 8v4m0 2v0',
    close:  'M5 5l10 10M15 5L5 15',
    share:  'M10 2v10M10 2L7 5M10 2l3 3M4 11v5h12v-5',
    lock:   'M5 9V7a5 5 0 0 1 10 0v2M4 9h12v8H4zM10 12v2',
    shield: 'M10 2l7 3v5c0 4-3 7-7 8-4-1-7-4-7-8V5l7-3z',
    clock:  'M10 2a8 8 0 1 0 0 16 8 8 0 0 0 0-16zM10 6v4l3 2',
  }[n] || '';
  return (
    <svg width={s} height={s} viewBox="0 0 20 20" fill="none" stroke="currentColor"
         strokeWidth={sw} strokeLinecap="round" strokeLinejoin="round" style={{ flexShrink: 0 }}>
      <path d={p} />
    </svg>
  );
}

// Phone status bar — modern iOS look (notchless, mobile-first)
function PhoneStatus({ time = '9:41' }) {
  return (
    <div className="hf-phone-status">
      <span style={{ fontVariantNumeric: 'tabular-nums' }}>{time}</span>
      <div className="hf-phone-status-icons">
        {/* signal */}
        <svg width="17" height="11" viewBox="0 0 17 11" fill="currentColor"><rect x="0" y="7" width="3" height="4" rx="0.5"/><rect x="4" y="5" width="3" height="6" rx="0.5"/><rect x="8" y="3" width="3" height="8" rx="0.5"/><rect x="12" y="0" width="3" height="11" rx="0.5"/></svg>
        {/* wifi */}
        <svg width="15" height="11" viewBox="0 0 15 11" fill="currentColor"><path d="M7.5 0C4.6 0 1.9 1.1 0 2.9l1.4 1.4A8.5 8.5 0 0 1 7.5 2c2.3 0 4.4.9 6.1 2.4L15 2.9C13.1 1.1 10.4 0 7.5 0zm0 4C5.7 4 3.9 4.7 2.5 5.9l1.4 1.4A5.5 5.5 0 0 1 7.5 6c1.4 0 2.7.5 3.6 1.3l1.4-1.4A7.4 7.4 0 0 0 7.5 4zm0 4a3 3 0 0 0-2.1.9l1.5 1.5a1.5 1.5 0 0 1 1.2 0l1.5-1.5A3 3 0 0 0 7.5 8z"/></svg>
        {/* battery */}
        <svg width="25" height="11" viewBox="0 0 25 11" fill="none"><rect x="0.5" y="0.5" width="21" height="10" rx="2.5" stroke="currentColor"/><rect x="2" y="2" width="18" height="7" rx="1" fill="currentColor"/><rect x="22.5" y="3.5" width="1.5" height="4" rx="0.5" fill="currentColor"/></svg>
      </div>
    </div>
  );
}

function PhoneHome() { return <div className="hf-phone-home" />; }

// Mini sparkline (svg path), `data` 0–1
function Spark({ data, color = 'currentColor', fill = 'transparent', sw = 1.5 }) {
  const w = 100, h = 32;
  const pts = data.map((v, i) => {
    const x = (i / (data.length - 1)) * w;
    const y = h - 2 - v * (h - 4);
    return `${i ? 'L' : 'M'}${x.toFixed(1)} ${y.toFixed(1)}`;
  }).join(' ');
  const fillP = fill !== 'transparent' ? `${pts} L${w} ${h} L0 ${h} Z` : null;
  return (
    <svg viewBox={`0 0 ${w} ${h}`} preserveAspectRatio="none" className="hf-spark">
      {fillP && <path d={fillP} fill={fill} />}
      <path d={pts} fill="none" stroke={color} strokeWidth={sw} strokeLinejoin="round" strokeLinecap="round" vectorEffect="non-scaling-stroke" />
    </svg>
  );
}

// Bar chart
function Bars({ data, color = 'var(--ink)', max }) {
  const m = max ?? Math.max(...data);
  return (
    <div className="hf-row hf-items-end" style={{ gap: 4, height: '100%' }}>
      {data.map((v, i) => (
        <div key={i} className="hf-grow" style={{ height: `${(v / m) * 100}%`, background: color, borderRadius: '2px 2px 0 0', minHeight: 2 }} />
      ))}
    </div>
  );
}

// Donut chart (from segments [{value, color}])
function Donut({ segments, size = 100, thickness = 14 }) {
  const r = size / 2 - thickness / 2;
  const c = 2 * Math.PI * r;
  const total = segments.reduce((s, x) => s + x.value, 0);
  let off = 0;
  return (
    <svg width={size} height={size} viewBox={`0 0 ${size} ${size}`} style={{ transform: 'rotate(-90deg)' }}>
      <circle cx={size/2} cy={size/2} r={r} fill="none" stroke="var(--paper-2)" strokeWidth={thickness} />
      {segments.map((seg, i) => {
        const len = (seg.value / total) * c;
        const dash = `${len} ${c - len}`;
        const out = (
          <circle key={i} cx={size/2} cy={size/2} r={r} fill="none"
            stroke={seg.color} strokeWidth={thickness}
            strokeDasharray={dash} strokeDashoffset={-off} strokeLinecap="butt" />
        );
        off += len;
        return out;
      })}
    </svg>
  );
}

// Smooth area chart with axis-grid + optional fill
function AreaChart({ data, color = 'var(--ink)', fill = 'rgba(21,18,14,0.06)', height = 160, gridY = 4 }) {
  const w = 600, h = height;
  const max = Math.max(...data) * 1.1, min = 0;
  const pts = data.map((v, i) => {
    const x = (i / (data.length - 1)) * w;
    const y = h - ((v - min) / (max - min)) * h;
    return [x, y];
  });
  const path = pts.map((p, i) => `${i ? 'L' : 'M'}${p[0].toFixed(1)} ${p[1].toFixed(1)}`).join(' ');
  const fillPath = `${path} L${w} ${h} L0 ${h} Z`;
  return (
    <svg viewBox={`0 0 ${w} ${h}`} preserveAspectRatio="none" style={{ width: '100%', height: '100%', display: 'block' }}>
      {Array.from({length: gridY + 1}).map((_, i) => {
        const y = (i / gridY) * h;
        return <line key={i} x1={0} y1={y} x2={w} y2={y} stroke="var(--line)" strokeWidth="0.5" vectorEffect="non-scaling-stroke" strokeDasharray="2 3" />;
      })}
      <path d={fillPath} fill={fill} />
      <path d={path} fill="none" stroke={color} strokeWidth="2" vectorEffect="non-scaling-stroke" strokeLinejoin="round" strokeLinecap="round" />
    </svg>
  );
}

// Small annotation pill — title says where it is
function Anno({ children, top, left, right, bottom, ...rest }) {
  return (
    <div className="hf-anno" style={{ top, left, right, bottom }} {...rest}>
      <span className="hf-anno-text">{children}</span>
    </div>
  );
}

// Annotated, drawn-arrow hint (used in onboarding etc)
function ArrowAnno({ children, top, left, right, bottom, dir = 'left' }) {
  return (
    <div className="hf-anno" style={{ top, left, right, bottom }}>
      <span className="hf-anno-text">{children}</span>
      <svg width="22" height="14" viewBox="0 0 22 14" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round" style={{ transform: dir === 'right' ? 'scaleX(-1)' : '' }}>
        <path d="M2 7c5 0 12-4 18-4M2 7l4-3M2 7l4 3" />
      </svg>
    </div>
  );
}

// Star rating
function Stars({ n = 5, of = 5, size = 11 }) {
  return (
    <span className="hf-rating" style={{ fontSize: size }}>
      {Array.from({ length: of }).map((_, i) => (
        <span key={i} className="hf-star" style={{ color: i < n ? 'var(--sun)' : 'var(--ink-5)', width: size, height: size }} />
      ))}
    </span>
  );
}

// Avatar with initials & deterministic color
function Avatar({ name = 'A', size = 'md', src }) {
  const cls = size === 'sm' ? 'hf-avatar hf-avatar-sm' : size === 'lg' ? 'hf-avatar hf-avatar-lg' : size === 'xl' ? 'hf-avatar hf-avatar-xl' : 'hf-avatar';
  const colors = ['#E8C2B5','#C9D4BC','#E5C0A6','#F0E8D7','#DCAE9F','#B6C4AB','#D9B49A','#F0E5D0','#C7754F','#8D759F'];
  const c = colors[name.charCodeAt(0) % colors.length];
  return (
    <div className={cls} style={{ background: c, color: '#15120E', borderColor: 'rgba(0,0,0,0.08)' }}>
      {name.split(' ').slice(0, 2).map(s => s[0]).join('').toUpperCase()}
    </div>
  );
}

// Money formatting helper
function money(n, c = '$') { return c + n.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'); }

// Product image with optional badge
function ProdImg({ tone = 'terra', badge, label, h = 180, r = 'var(--r-md)' }) {
  return (
    <div className={`hf-img hf-img-${tone}`} style={{ width: '100%', height: h, borderRadius: r, position: 'relative' }}>
      {badge && (
        <div style={{ position: 'absolute', top: 8, left: 8, padding: '3px 8px', borderRadius: 999, background: 'rgba(255,255,255,0.92)', fontSize: 10, fontWeight: 600, letterSpacing: 0.04, color: 'var(--ink)' }}>{badge}</div>
      )}
      {label && (
        <div style={{ position: 'absolute', bottom: 8, right: 8, padding: '2px 6px', borderRadius: 4, background: 'rgba(0,0,0,0.5)', color: 'white', fontSize: 9, fontWeight: 500, letterSpacing: 0.04 }}>{label}</div>
      )}
    </div>
  );
}

Object.assign(window, { Ico, PhoneStatus, PhoneHome, Spark, Bars, Donut, AreaChart, Anno, ArrowAnno, Stars, Avatar, money, ProdImg });

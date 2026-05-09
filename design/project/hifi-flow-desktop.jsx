// hifi-flow-desktop.jsx — End-to-end desktop web shopper journey
// Same Mira → Mira Studio narrative, but in browser-window context.
// 8 steps + 4 in-flow states. Each artboard is 1280x800.

// Reusable browser chrome
function BrowserChrome({ url = 'mira.studio', title = 'Mira Studio — Hand-thrown ceramics' }) {
  return (
    <div style={{ height: 38, background: '#E8E3D8', borderBottom: '1px solid var(--line)', display: 'flex', alignItems: 'center', padding: '0 14px', gap: 12, flexShrink: 0 }}>
      <div className="hf-flex hf-gap-1">
        {['#FF6058','#FFBD2E','#28C941'].map(c => <span key={c} style={{ width: 11, height: 11, borderRadius: 999, background: c }} />)}
      </div>
      <div className="hf-flex hf-gap-1" style={{ marginLeft: 6, color: 'var(--ink-3)' }}>
        <Ico n="chevL" s={13} />
        <Ico n="chevR" s={13} />
        <Ico n="refresh" s={13} />
      </div>
      <div className="hf-flex hf-items-center hf-gap-2" style={{ flex: 1, height: 22, background: 'var(--paper)', borderRadius: 6, padding: '0 10px', border: '1px solid var(--line)' }}>
        <Ico n="lock" s={11} />
        <span style={{ fontSize: 11, color: 'var(--ink-2)', fontFamily: 'var(--sans)' }}>{url}</span>
      </div>
      <div className="hf-flex hf-gap-2" style={{ color: 'var(--ink-3)' }}>
        <Ico n="user" s={13} />
        <Ico n="menu" s={13} />
      </div>
    </div>
  );
}

// Desktop "user action" caption — sits at bottom-left of viewport
function DesktopAction({ children }) {
  return (
    <div style={{
      position: 'absolute', left: 14, bottom: 8, zIndex: 5,
      display: 'flex', alignItems: 'center', gap: 6,
      fontFamily: 'var(--sans)', fontSize: 11, pointerEvents: 'none',
    }}>
      <svg width="16" height="10" viewBox="0 0 16 10" fill="none" stroke="var(--terra)" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
        <path d="M1 5h13M11 1l4 4-4 4" />
      </svg>
      <span style={{ background: 'white', border: '1px solid var(--terra)', color: 'var(--terra)', padding: '2px 8px', borderRadius: 999, whiteSpace: 'nowrap' }}>{children}</span>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 01 · DISCOVER — Mira lands from a search result / instagram link
// (Marketplace-style splash before entering a specific shop)
// ──────────────────────────────────────────────────────────────────────
function DFlow_01_Landing() {
  const grid = [
    { tone: 'clay',   t: 'Persimmon vase',  m: 'Mira Studio',   p: 86 },
    { tone: 'sage',   t: 'Forest bowl',     m: 'Atelier Hana',  p: 64 },
    { tone: 'rust',   t: 'Rust mug Nº 04',  m: 'Mira Studio',   p: 32 },
    { tone: 'cream',  t: 'Ribbon plate',    m: 'Quiet Goods',   p: 48 },
    { tone: 'cobalt', t: 'Indigo carafe',   m: 'Mira Studio',   p: 110 },
    { tone: 'moss',   t: 'Moss saucer',     m: 'Quiet Goods',   p: 44 },
  ];
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="micro.shop/discover · ceramics" title="Micro · Discover" />
      <div className="hf-flex hf-items-center" style={{ padding: '14px 32px', borderBottom: '1px solid var(--line)', gap: 28, background: 'var(--paper)' }}>
        <span className="hf-logo" style={{ fontSize: 20 }}>micro.</span>
        <div className="hf-flex hf-gap-5">
          {['Discover', 'Shops', 'Journal', 'For makers'].map((l, i) => (
            <a key={l} style={{ fontSize: 12.5, color: i === 0 ? 'var(--ink)' : 'var(--ink-3)', fontWeight: 500 }}>{l}</a>
          ))}
        </div>
        <div className="hf-grow" />
        <div className="hf-flex hf-items-center hf-gap-2" style={{ height: 32, padding: '0 12px', background: 'var(--paper-2)', borderRadius: 999, width: 280 }}>
          <Ico n="search" s={13} />
          <span style={{ fontSize: 12.5, color: 'var(--ink-3)' }}>Search makers, products, places…</span>
        </div>
        <button className="hf-btn hf-btn-ghost hf-btn-sm">Sign in</button>
        <button className="hf-btn hf-btn-primary hf-btn-sm">Sell on Micro</button>
      </div>

      <div className="hf-grow" style={{ overflow: 'auto', padding: '28px 32px 0' }}>
        <div className="hf-flex hf-items-end hf-between" style={{ marginBottom: 18 }}>
          <div>
            <div className="hf-eyebrow" style={{ color: 'var(--terra)' }}>Editor's pick · Spring</div>
            <h1 className="hf-display" style={{ fontSize: 48, lineHeight: 0.95, marginTop: 6 }}>Small shops, <i>real people.</i></h1>
            <p className="hf-body hf-muted" style={{ marginTop: 8, maxWidth: 460 }}>
              48 makers near you. Browse hand-made ceramics, baked goods, and small-batch goods — buy direct.
            </p>
          </div>
          <div className="hf-flex hf-gap-2">
            <span className="hf-chip hf-chip-on">Ceramics</span>
            <span className="hf-chip">Bakery</span>
            <span className="hf-chip">Vintage</span>
            <span className="hf-chip">Within 5mi</span>
          </div>
        </div>

        <div className="hf-grid" style={{ gridTemplateColumns: 'repeat(3, 1fr)', gap: 16 }}>
          {grid.map((p, i) => (
            <div key={i} className="hf-card" style={{ padding: 0, overflow: 'hidden', background: 'var(--paper)' }}>
              <div className={`hf-img hf-img-${p.tone}`} style={{ height: 180, borderRadius: 0, position: 'relative' }}>
                {i === 0 && <span style={{ position: 'absolute', top: 10, left: 10, padding: '3px 8px', borderRadius: 4, background: 'rgba(255,255,255,0.92)', fontSize: 10, fontWeight: 600 }}>FEATURED</span>}
              </div>
              <div style={{ padding: '12px 14px 14px' }}>
                <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 4 }}>
                  <span className="hf-tiny hf-muted">{p.m}</span>
                  <span className="hf-num hf-h4">{money(p.p)}</span>
                </div>
                <div className="hf-h4" style={{ fontSize: 14 }}>{p.t}</div>
              </div>
            </div>
          ))}
        </div>
      </div>
      <DesktopAction>clicks Persimmon vase · enters Mira Studio</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · HOME — Mira Studio's storefront (reuses Home_Rails — strongest)
// ──────────────────────────────────────────────────────────────────────
function DFlow_02_Home() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Home_Rails />
      <DesktopAction>clicks "Best loved" rail · opens Persimmon vase</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · CATEGORY — filtered listing of "Vessels"
// ──────────────────────────────────────────────────────────────────────
function DFlow_03_Category() {
  const products = [
    { tone: 'clay',   t: 'Persimmon vase',  p: 86,  badge: 'New' },
    { tone: 'sage',   t: 'Forest bowl, lg', p: 64 },
    { tone: 'rose',   t: 'Soft hand vessel',p: 92, badge: '2 left' },
    { tone: 'cobalt', t: 'Indigo carafe',   p: 110 },
    { tone: 'moss',   t: 'Moss saucer',     p: 44 },
    { tone: 'rust',   t: 'Rust mug Nº 04',  p: 32 },
    { tone: 'bone',   t: 'Cream tumbler',   p: 48 },
    { tone: 'cream',  t: 'Ribbon plate',    p: 48 },
  ];
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="mira.studio/shop/vessels" />
      <ShopperTopbar />
      <div className="hf-grow" style={{ overflow: 'auto' }}>
        {/* Crumb + title */}
        <div style={{ padding: '20px 32px 12px' }}>
          <div className="hf-tiny hf-muted" style={{ marginBottom: 8 }}>Shop · <span style={{ color: 'var(--ink)' }}>Vessels</span></div>
          <div className="hf-flex hf-between hf-items-end">
            <h1 className="hf-display" style={{ fontSize: 36, lineHeight: 1 }}>Vessels</h1>
            <span className="hf-small hf-muted">42 pieces · sorted Newest</span>
          </div>
        </div>

        {/* Layout: left filter rail + product grid */}
        <div style={{ padding: '0 32px 24px', display: 'grid', gridTemplateColumns: '220px 1fr', gap: 28 }}>
          {/* Filters */}
          <div>
            {[
              { l: 'Type', items: [{ n: 'Vase', on: true, c: 18 }, { n: 'Bowl', c: 12 }, { n: 'Mug', c: 22 }, { n: 'Plate', c: 9 }] },
              { l: 'Color', items: [{ n: 'Persimmon', on: true, c: 6 }, { n: 'Sage', c: 9 }, { n: 'Coal', c: 7 }, { n: 'Cream', c: 11 }] },
              { l: 'Price', items: [{ n: 'Under $50', c: 14 }, { n: '$50–$100', on: true, c: 19 }, { n: '$100+', c: 9 }] },
            ].map((g, i) => (
              <div key={i} style={{ marginBottom: 22, paddingBottom: 18, borderBottom: '1px solid var(--line)' }}>
                <div className="hf-h4" style={{ fontSize: 12.5, marginBottom: 10 }}>{g.l}</div>
                <div className="hf-col hf-gap-2">
                  {g.items.map((it, j) => (
                    <label key={j} className="hf-flex hf-items-center hf-gap-2" style={{ fontSize: 12.5, color: it.on ? 'var(--ink)' : 'var(--ink-2)' }}>
                      <span style={{ width: 14, height: 14, borderRadius: 3, border: it.on ? 'none' : '1.5px solid var(--ink-4)', background: it.on ? 'var(--ink)' : 'transparent', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--paper)' }}>
                        {it.on && <Ico n="check" s={9} sw={2.6} />}
                      </span>
                      <span style={{ flex: 1, fontWeight: it.on ? 500 : 400 }}>{it.n}</span>
                      <span className="hf-tiny hf-muted">{it.c}</span>
                    </label>
                  ))}
                </div>
              </div>
            ))}
          </div>

          {/* Grid */}
          <div>
            <div className="hf-flex hf-gap-2" style={{ marginBottom: 14 }}>
              <span className="hf-chip hf-chip-on">Vase ×</span>
              <span className="hf-chip hf-chip-on">Persimmon ×</span>
              <span className="hf-chip hf-chip-on">$50–$100 ×</span>
              <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ height: 26 }}>Clear</button>
            </div>

            <div className="hf-grid" style={{ gridTemplateColumns: 'repeat(4, 1fr)', gap: 14 }}>
              {products.map((p, i) => (
                <div key={i}>
                  <div className="hf-relative" style={{ marginBottom: 8 }}>
                    <ProdImg tone={p.tone} h={170} badge={p.badge} />
                    {i === 0 && <div style={{ position: 'absolute', inset: '-3px', border: '2px solid var(--ink)', borderRadius: 'var(--r-md)', pointerEvents: 'none' }} />}
                  </div>
                  <div className="hf-flex hf-between">
                    <span className="hf-h4" style={{ fontSize: 13 }}>{p.t}</span>
                    <span className="hf-h4 hf-num" style={{ fontSize: 13 }}>{money(p.p)}</span>
                  </div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>Stoneware · 4 colors</div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>filters by Persimmon · clicks the vase</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · PRODUCT — desktop product detail
// ──────────────────────────────────────────────────────────────────────
function DFlow_04_Product() {
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="mira.studio/p/persimmon-vase" />
      <ShopperTopbar />
      <div className="hf-grow" style={{ overflow: 'auto', padding: '20px 32px 28px' }}>
        <div className="hf-tiny hf-muted" style={{ marginBottom: 16 }}>Shop · Vessels · <span style={{ color: 'var(--ink)' }}>Persimmon vase</span></div>

        <div className="hf-grid" style={{ gridTemplateColumns: '64px 1fr 380px', gap: 20 }}>
          {/* thumbnails */}
          <div className="hf-col hf-gap-2">
            {['clay','rose','rust','sage'].map((t, i) => (
              <div key={i} className={`hf-img hf-img-${t}`} style={{ width: 64, height: 64, borderRadius: 8, border: i === 0 ? '2px solid var(--ink)' : '1px solid var(--line)' }} />
            ))}
          </div>
          {/* hero image */}
          <div className="hf-img hf-img-clay" style={{ height: 540, borderRadius: 'var(--r-lg)' }} />
          {/* buy box */}
          <div>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 10 }}>
              <Avatar name="Mira" size="sm" />
              <div className="hf-grow" style={{ minWidth: 0 }}>
                <div className="hf-tiny hf-muted">Mira Studio</div>
                <div className="hf-flex hf-items-center hf-gap-1"><Stars n={5} size={10} /><span className="hf-tiny hf-muted">· 184 sales</span></div>
              </div>
              <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ border: '1px solid var(--line-2)' }}>Follow</button>
            </div>

            <h1 className="hf-display" style={{ fontSize: 36, lineHeight: 0.98, marginBottom: 6 }}>Persimmon <i>vase</i></h1>
            <div className="hf-flex hf-items-baseline hf-gap-2" style={{ marginBottom: 14 }}>
              <span className="hf-display hf-num" style={{ fontSize: 26 }}>{money(86)}</span>
              <span className="hf-tiny hf-muted">free shipping over $80</span>
            </div>
            <p className="hf-body hf-muted" style={{ fontSize: 13, marginBottom: 18 }}>
              Wheel-thrown stoneware, glazed in our house persimmon. One-of-one — no two are alike.
            </p>

            <div style={{ marginBottom: 14 }}>
              <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 8 }}>
                <span className="hf-label" style={{ marginBottom: 0 }}>Size</span>
                <span className="hf-tiny hf-muted">Medium · 6.5"</span>
              </div>
              <div className="hf-flex hf-gap-2">
                {[{ l: 'S', s: '4.5"' },{ l: 'M', s: '6.5"', on: true },{ l: 'L', s: '9"' }].map((o, i) => (
                  <div key={i} className="hf-center" style={{ flex: 1, padding: 10, borderRadius: 10, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: o.on ? 'var(--paper-2)' : 'transparent', flexDirection: 'column', gap: 2 }}>
                    <span className="hf-h4" style={{ fontSize: 13 }}>{o.l}</span>
                    <span className="hf-tiny hf-muted" style={{ fontSize: 10 }}>{o.s}</span>
                  </div>
                ))}
              </div>
            </div>

            <div style={{ marginBottom: 18 }}>
              <span className="hf-label">Glaze</span>
              <div className="hf-flex hf-gap-2">
                {[{ c: '#C7754F', on: true, l: 'Persimmon' },{ c: '#92A487', l: 'Sage' },{ c: '#1F1D1B', l: 'Coal' }].map((g, i) => (
                  <div key={i} className="hf-flex hf-items-center hf-gap-2" style={{ padding: '6px 10px 6px 6px', borderRadius: 999, border: g.on ? '1.5px solid var(--ink)' : '1px solid var(--line)' }}>
                    <span style={{ width: 18, height: 18, borderRadius: 999, background: g.c, border: '1px solid rgba(0,0,0,0.08)' }} />
                    <span className="hf-tiny" style={{ fontWeight: 500 }}>{g.l}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="hf-flex hf-gap-2" style={{ marginBottom: 14 }}>
              <button className="hf-btn hf-btn-outline" style={{ width: 48 }}><Ico n="heart" s={14} /></button>
              <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg" style={{ flex: 1 }}>Add to cart · <span className="hf-num">{money(86)}</span></button>
            </div>

            <div className="hf-card" style={{ padding: 12, background: 'var(--paper-2)', borderColor: 'transparent' }}>
              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 6 }}><Ico n="truck" s={13} /> <span className="hf-h4" style={{ fontSize: 12.5 }}>Arrives Wed–Thu</span></div>
              <div className="hf-tiny hf-muted">Free 30-day returns · Carbon-neutral shipping</div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>picks Medium · Persimmon · clicks Add to cart</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05 · ADDED — slide-in cart drawer (desktop interstitial)
// ──────────────────────────────────────────────────────────────────────
function DFlow_05_Added() {
  return (
    <div className="hf hf-desktop" style={{ position: 'relative' }}>
      <BrowserChrome url="mira.studio/p/persimmon-vase" />
      <ShopperTopbar cartCount={1} />
      {/* dimmed page behind */}
      <div className="hf-grow" style={{ overflow: 'hidden', position: 'relative' }}>
        <div style={{ padding: '20px 32px', opacity: 0.4 }}>
          <div className="hf-grid" style={{ gridTemplateColumns: '1fr 380px', gap: 20 }}>
            <div className="hf-img hf-img-clay" style={{ height: 540, borderRadius: 'var(--r-lg)' }} />
            <div>
              <div className="hf-display" style={{ fontSize: 36, marginBottom: 12 }}>Persimmon vase</div>
              <div className="hf-num hf-display" style={{ fontSize: 26 }}>{money(86)}</div>
            </div>
          </div>
        </div>
        <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.25)', pointerEvents: 'none' }} />

        {/* Drawer */}
        <div style={{ position: 'absolute', top: 0, right: 0, bottom: 0, width: 420, background: 'var(--paper)', boxShadow: '-12px 0 40px rgba(0,0,0,0.18)', display: 'flex', flexDirection: 'column' }}>
          <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
            <div className="hf-flex hf-items-center hf-gap-2">
              <div className="hf-center" style={{ width: 28, height: 28, borderRadius: 999, background: 'var(--forest-2)', color: 'var(--good)' }}>
                <Ico n="check" s={14} sw={2.4} />
              </div>
              <span className="hf-h4">Added to bag</span>
            </div>
            <button className="hf-icon-btn"><Ico n="x" s={14} /></button>
          </div>

          <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <div className="hf-img hf-img-clay" style={{ width: 64, height: 64, borderRadius: 8 }} />
              <div className="hf-grow">
                <div className="hf-h4">Persimmon vase</div>
                <div className="hf-tiny hf-muted">Medium · Persimmon</div>
                <div className="hf-num" style={{ fontWeight: 600, marginTop: 4 }}>{money(86)}</div>
              </div>
              <div className="hf-flex hf-items-center" style={{ border: '1px solid var(--line-2)', borderRadius: 999, padding: 2 }}>
                <button className="hf-icon-btn" style={{ width: 22, height: 22 }}><Ico n="minus" s={11} /></button>
                <span className="hf-num" style={{ width: 22, textAlign: 'center', fontSize: 12, fontWeight: 600 }}>1</span>
                <button className="hf-icon-btn" style={{ width: 22, height: 22 }}><Ico n="plus" s={11} /></button>
              </div>
            </div>
          </div>

          <div style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 10 }}>
              <span className="hf-eyebrow">Pairs well with</span>
              <span className="hf-tiny hf-muted">free ship at $80 · you're $0 away</span>
            </div>
            <div className="hf-progress" style={{ marginBottom: 14, height: 4 }}><i style={{ width: '100%', background: 'var(--good)' }} /></div>
            <div className="hf-flex hf-gap-2">
              {[
                { tone: 'sage',  t: 'Forest bowl', p: 64 },
                { tone: 'rust',  t: 'Rust mug',    p: 32 },
                { tone: 'cream', t: 'Ribbon plate',p: 48 },
              ].map((p, i) => (
                <div key={i} className="hf-card" style={{ padding: 0, overflow: 'hidden', flex: 1 }}>
                  <div className={`hf-img hf-img-${p.tone}`} style={{ height: 70, borderRadius: 0 }} />
                  <div style={{ padding: '6px 8px 8px' }}>
                    <div className="hf-h4" style={{ fontSize: 11, lineHeight: 1.2 }}>{p.t}</div>
                    <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 4 }}>
                      <span className="hf-num" style={{ fontWeight: 600, fontSize: 11 }}>{money(p.p)}</span>
                      <span className="hf-icon-btn" style={{ width: 18, height: 18, background: 'var(--ink)', color: 'var(--paper)' }}><Ico n="plus" s={10} sw={2.4} /></span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="hf-grow" />

          <div style={{ padding: '14px 20px', borderTop: '1px solid var(--line)' }}>
            <div className="hf-flex hf-between" style={{ marginBottom: 10 }}>
              <span className="hf-h4">Subtotal</span>
              <span className="hf-display hf-num" style={{ fontSize: 22 }}>{money(86)}</span>
            </div>
            <div className="hf-flex hf-gap-2">
              <button className="hf-btn hf-btn-outline hf-btn-block" style={{ flex: 1 }}>View cart</button>
              <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg" style={{ flex: 1.4 }}>Checkout · <span className="hf-num">{money(86)}</span></button>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>clicks View cart in the drawer</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 06 · CART (reuses Cart_Desktop)
// ──────────────────────────────────────────────────────────────────────
function DFlow_06_Cart() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Cart_Desktop />
      <DesktopAction>reviews · clicks Checkout</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 07 · CHECKOUT (reuses Checkout_Desktop — 1200x1240, scaled into 800)
// ──────────────────────────────────────────────────────────────────────
function DFlow_07_Checkout() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%', overflow: 'hidden' }}>
      <Checkout_Desktop />
      <DesktopAction>fills info · places order</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 08 · CONFIRMED — desktop receipt page
// ──────────────────────────────────────────────────────────────────────
function DFlow_08_Confirmed() {
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="mira.studio/orders/MS-3142" />
      <div className="hf-flex hf-items-center" style={{ padding: '14px 32px', borderBottom: '1px solid var(--line)', background: 'var(--paper)' }}>
        <span className="hf-logo" style={{ fontSize: 18 }}>Mira Studio</span>
        <span className="hf-tiny hf-muted" style={{ marginLeft: 12 }}>/ Order MS-3142</span>
        <div className="hf-grow" />
        <span className="hf-tiny hf-muted">mira@studio.co</span>
      </div>

      <div className="hf-grow" style={{ overflow: 'auto', padding: '32px 0', background: 'var(--paper-2)' }}>
        <div style={{ maxWidth: 1040, margin: '0 auto', padding: '0 32px' }}>
          {/* hero */}
          <div className="hf-flex hf-items-start hf-gap-4" style={{ marginBottom: 28 }}>
            <div className="hf-center" style={{ width: 52, height: 52, borderRadius: 999, background: 'var(--forest-2)', color: 'var(--good)', flexShrink: 0 }}>
              <Ico n="check" s={26} sw={2.4} />
            </div>
            <div className="hf-grow">
              <div className="hf-eyebrow" style={{ color: 'var(--good)' }}>Order placed</div>
              <h1 className="hf-display" style={{ fontSize: 40, lineHeight: 1.05, marginTop: 4 }}>Thanks, <i>Mira.</i></h1>
              <p className="hf-body hf-muted" style={{ fontSize: 14, marginTop: 8 }}>
                Order <span className="hf-num" style={{ color: 'var(--ink)' }}>#MS-3142</span> · receipt sent to mira@studio.co · arrives Wed–Thu
              </p>
            </div>
            <div className="hf-flex hf-gap-2">
              <button className="hf-btn hf-btn-outline">Print receipt</button>
              <button className="hf-btn hf-btn-primary">Track order</button>
            </div>
          </div>

          {/* Two-col: tracking + order */}
          <div className="hf-grid" style={{ gridTemplateColumns: '1fr 380px', gap: 20 }}>
            <div className="hf-card" style={{ padding: 20, background: 'var(--paper)' }}>
              <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 18 }}>
                <span className="hf-h3">Delivery progress</span>
                <span className="hf-tiny hf-muted">Express · UPS · 1Z 9999 ABC</span>
              </div>
              <div className="hf-flex hf-items-center" style={{ gap: 0, marginBottom: 24 }}>
                {[
                  { l: 'Confirmed', d: 'Mon, May 4', on: true, done: true },
                  { l: 'Packed',    d: 'expected Wed', on: true },
                  { l: 'Shipped' },
                  { l: 'Delivered' },
                ].map((s, i, a) => (
                  <React.Fragment key={i}>
                    <div className="hf-col hf-items-center hf-gap-1" style={{ flexShrink: 0 }}>
                      <div className="hf-center" style={{ width: 28, height: 28, borderRadius: 999,
                        background: s.done ? 'var(--good)' : s.on ? 'var(--paper-2)' : 'transparent',
                        border: s.on || s.done ? 'none' : '1px solid var(--line-2)',
                        color: s.done ? 'var(--paper)' : 'var(--ink-4)',
                      }}>
                        {s.done ? <Ico n="check" s={13} sw={2.4} /> : <span style={{ width: 7, height: 7, borderRadius: 999, background: s.on ? 'var(--ink)' : 'var(--ink-5)' }} />}
                      </div>
                      <span className="hf-tiny" style={{ fontWeight: s.on || s.done ? 600 : 400, color: s.on || s.done ? 'var(--ink)' : 'var(--ink-4)' }}>{s.l}</span>
                      {s.d && <span className="hf-tiny hf-muted" style={{ fontSize: 10 }}>{s.d}</span>}
                    </div>
                    {i < a.length - 1 && <div style={{ flex: 1, height: 2, background: i === 0 ? 'var(--good)' : 'var(--line)', margin: '0 6px', marginBottom: 24 }} />}
                  </React.Fragment>
                ))}
              </div>

              {/* Maker note */}
              <div style={{ padding: 14, borderRadius: 'var(--r-md)', background: 'var(--paper-2)' }}>
                <div className="hf-flex hf-items-start hf-gap-3">
                  <Avatar name="Mira" size="sm" />
                  <div className="hf-grow">
                    <div className="hf-h4">A note from Mira</div>
                    <p className="hf-body hf-muted" style={{ fontSize: 13, marginTop: 4 }}>"Thank you so much. I'll wrap these by hand on Friday — keep an eye out for a small surprise."</p>
                  </div>
                </div>
              </div>
            </div>

            {/* Order summary */}
            <div className="hf-card" style={{ padding: 0, background: 'var(--paper)', overflow: 'hidden' }}>
              <div style={{ padding: '14px 18px', borderBottom: '1px solid var(--line)' }}>
                <span className="hf-h4">Your order</span>
              </div>
              <div style={{ padding: '6px 18px' }}>
                {[
                  { tone: 'clay', t: 'Persimmon vase',  v: 'Medium · Persimmon', p: 86, q: 1 },
                  { tone: 'sage', t: 'Forest bowl',     v: 'Large · Sage',       p: 64, q: 2 },
                  { tone: 'rust', t: 'Rust mug Nº 04',  v: 'Set of 2',           p: 32, q: 1 },
                ].map((it, i, a) => (
                  <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '12px 0', borderBottom: i < a.length - 1 ? '1px solid var(--line)' : 'none' }}>
                    <div className={`hf-img hf-img-${it.tone}`} style={{ width: 48, height: 48, borderRadius: 8, flexShrink: 0 }} />
                    <div className="hf-grow" style={{ minWidth: 0 }}>
                      <div className="hf-h4" style={{ fontSize: 12.5 }}>{it.t} <span className="hf-muted" style={{ fontWeight: 400 }}>×{it.q}</span></div>
                      <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{it.v}</div>
                    </div>
                    <span className="hf-num hf-small" style={{ fontWeight: 600 }}>{money(it.p * it.q)}</span>
                  </div>
                ))}
              </div>
              <div style={{ padding: '14px 18px', background: 'var(--paper-2)' }}>
                <div className="hf-flex hf-between hf-items-baseline">
                  <span className="hf-h4">Total paid</span>
                  <span className="hf-display hf-num" style={{ fontSize: 22 }}>{money(204)}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>opens email · shares with a friend</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// IN-FLOW STATES — desktop loading / empty / error / success
// ──────────────────────────────────────────────────────────────────────

function DState_Loading() {
  const Skel = ({ w = '100%', h = 12, r = 4, style, ...rest }) => (
    <div {...rest} style={{ width: w, height: h, borderRadius: r, background: 'linear-gradient(90deg, var(--paper-2) 0%, var(--line) 50%, var(--paper-2) 100%)', backgroundSize: '200% 100%', animation: 'hf-shimmer 1.4s linear infinite', ...style }} />
  );
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="mira.studio" />
      <style>{`@keyframes hf-shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }`}</style>
      <ShopperTopbar />
      <div className="hf-grow" style={{ overflow: 'hidden' }}>
        <div style={{ padding: '24px 32px', display: 'grid', gridTemplateColumns: '1.1fr 1fr', gap: 20 }}>
          <div style={{ paddingTop: 20 }}>
            <Skel w={120} h={11} style={{ marginBottom: 18 }} />
            <Skel w="80%" h={48} style={{ marginBottom: 10 }} />
            <Skel w="60%" h={48} style={{ marginBottom: 22 }} />
            <Skel w="90%" h={12} style={{ marginBottom: 6 }} />
            <Skel w="70%" h={12} style={{ marginBottom: 22 }} />
            <div className="hf-flex hf-gap-2">
              <Skel w={140} h={42} r={8} />
              <Skel w={120} h={42} r={8} />
            </div>
          </div>
          <Skel h={300} r={16} />
        </div>
        <div style={{ padding: '0 32px' }}>
          <Skel w={160} h={20} style={{ marginBottom: 14 }} />
          <div className="hf-grid" style={{ gridTemplateColumns: 'repeat(5, 1fr)', gap: 14 }}>
            {[0,1,2,3,4].map(i => (
              <div key={i}>
                <Skel h={130} r={8} style={{ marginBottom: 8 }} />
                <Skel w="80%" h={11} style={{ marginBottom: 6 }} />
                <Skel w="40%" h={11} />
              </div>
            ))}
          </div>
        </div>
      </div>
      <DesktopAction>cold cache · slow Wi-Fi</DesktopAction>
    </div>
  );
}

function DState_EmptyCart() {
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="mira.studio/cart" />
      <ShopperTopbar cartCount={0} />
      <div className="hf-grow" style={{ padding: '32px 32px 0', overflow: 'auto' }}>
        <h1 className="hf-display" style={{ fontSize: 36, marginBottom: 28 }}>Your bag</h1>
        <div className="hf-grid" style={{ gridTemplateColumns: '1fr 380px', gap: 20 }}>
          <div className="hf-card" style={{ padding: 60, background: 'var(--paper)', textAlign: 'center' }}>
            <div className="hf-center" style={{ width: 110, height: 110, borderRadius: 999, background: 'var(--paper-2)', margin: '0 auto 18px', position: 'relative' }}>
              <Ico n="bag" s={50} sw={1.4} />
              <span style={{ position: 'absolute', bottom: 10, right: 6, width: 28, height: 28, borderRadius: 999, background: 'var(--paper)', border: '1.5px solid var(--ink)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 16, fontWeight: 700 }}>0</span>
            </div>
            <h2 className="hf-display" style={{ fontSize: 28, lineHeight: 1.05, marginBottom: 8 }}>Your bag is <i>empty.</i></h2>
            <p className="hf-body hf-muted" style={{ fontSize: 13, maxWidth: 320, margin: '0 auto 18px' }}>
              Save things you love and they'll wait here for you.
            </p>
            <button className="hf-btn hf-btn-primary hf-btn-lg">Browse the shop</button>
          </div>

          <div className="hf-card" style={{ padding: 18, background: 'var(--paper)' }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 12 }}>
              <span className="hf-h4">You were looking at</span>
              <a className="hf-tiny" style={{ color: 'var(--ink-2)', textDecoration: 'underline' }}>See all</a>
            </div>
            <div className="hf-col hf-gap-3">
              {[
                { tone: 'clay', t: 'Persimmon vase', p: 86 },
                { tone: 'sage', t: 'Forest bowl',    p: 64 },
                { tone: 'rust', t: 'Rust mug Nº 04', p: 32 },
              ].map((p, i) => (
                <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '8px 0', borderBottom: i < 2 ? '1px solid var(--line)' : 'none' }}>
                  <div className={`hf-img hf-img-${p.tone}`} style={{ width: 48, height: 48, borderRadius: 8 }} />
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 12.5 }}>{p.t}</div>
                    <div className="hf-num hf-tiny" style={{ fontWeight: 600, marginTop: 2 }}>{money(p.p)}</div>
                  </div>
                  <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ border: '1px solid var(--line-2)' }}>Add</button>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>landed on cart with nothing in it</DesktopAction>
    </div>
  );
}

function DState_FormError() {
  return (
    <div className="hf hf-desktop" style={{ background: 'var(--paper-2)' }}>
      <BrowserChrome url="mira.studio/checkout" />
      <div className="hf-flex hf-items-center hf-between" style={{ padding: '14px 32px', background: 'var(--paper)', borderBottom: '1px solid var(--line)' }}>
        <div className="hf-flex hf-items-center hf-gap-3">
          <span className="hf-logo">Mira Studio</span>
          <span className="hf-tiny hf-muted">/ Checkout</span>
        </div>
        <span className="hf-tiny hf-muted hf-flex hf-items-center hf-gap-1"><Ico n="lock" s={11} /> Secure SSL</span>
      </div>

      <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 0' }}>
        <div style={{ maxWidth: 1040, margin: '0 auto', padding: '0 24px', display: 'grid', gridTemplateColumns: '1fr 380px', gap: 36 }}>
          <div>
            {/* Top error banner */}
            <div className="hf-flex hf-items-start hf-gap-3" style={{ padding: '14px 18px', borderRadius: 10, background: 'var(--rose, #F8DDDA)', border: '1px solid #DD6660', color: '#8A2C26', marginBottom: 22 }}>
              <Ico n="info" s={16} />
              <div style={{ flex: 1, fontSize: 13 }}>
                <div style={{ fontWeight: 600, marginBottom: 2 }}>We couldn't place your order</div>
                <div style={{ opacity: 0.85 }}>2 fields need attention — your card was declined and ZIP looks incomplete.</div>
              </div>
              <button className="hf-icon-btn" style={{ color: '#8A2C26' }}><Ico n="x" s={13} /></button>
            </div>

            <h3 className="hf-h3" style={{ marginBottom: 14 }}>Shipping address</h3>
            <div className="hf-col hf-gap-3" style={{ marginBottom: 24 }}>
              <div>
                <span className="hf-label">Email</span>
                <input className="hf-input" defaultValue="mira@studio.co" />
              </div>
              <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr 1fr' }}>
                <div><span className="hf-label">City</span><input className="hf-input hf-input-sm" defaultValue="Oakland" /></div>
                <div><span className="hf-label">State</span><input className="hf-input hf-input-sm" defaultValue="CA" /></div>
                <div>
                  <span className="hf-label" style={{ color: '#A93A33' }}>ZIP</span>
                  <input className="hf-input hf-input-sm" defaultValue="94" style={{ borderColor: '#DD6660', background: 'rgba(221,102,96,0.06)' }} />
                  <div className="hf-tiny" style={{ color: '#A93A33', marginTop: 4, display: 'flex', alignItems: 'center', gap: 4 }}>
                    <Ico n="info" s={11} /> ZIP must be 5 digits
                  </div>
                </div>
              </div>
            </div>

            <h3 className="hf-h3" style={{ marginBottom: 14 }}>Payment</h3>
            <div className="hf-card" style={{ padding: 0, background: 'var(--paper)', borderColor: '#DD6660' }}>
              <div style={{ padding: '14px 16px', borderBottom: '1px solid var(--line)' }}>
                <span className="hf-label" style={{ color: '#A93A33' }}>Card number</span>
                <input className="hf-input" defaultValue="4242 4242 4242 0000" style={{ borderColor: '#DD6660', background: 'rgba(221,102,96,0.06)' }} />
                <div className="hf-tiny" style={{ color: '#A93A33', marginTop: 6, display: 'flex', alignItems: 'center', gap: 4 }}>
                  <Ico n="info" s={11} /> This card was declined by your bank. Try another card or contact your bank.
                </div>
              </div>
              <div className="hf-grid" style={{ gridTemplateColumns: '1fr 1fr' }}>
                <div style={{ padding: '14px 16px', borderRight: '1px solid var(--line)' }}>
                  <span className="hf-label">Expiry</span>
                  <input className="hf-input hf-input-sm" defaultValue="08 / 28" />
                </div>
                <div style={{ padding: '14px 16px' }}>
                  <span className="hf-label">CVC</span>
                  <input className="hf-input hf-input-sm" defaultValue="•••" />
                </div>
              </div>
            </div>

            <div className="hf-flex hf-between" style={{ marginTop: 22 }}>
              <a className="hf-flex hf-items-center hf-gap-1 hf-small" style={{ color: 'var(--ink-2)' }}><Ico n="arrowL" s={12} /> Return to cart</a>
              <button className="hf-btn hf-btn-block hf-btn-lg" style={{ minWidth: 220, background: 'var(--ink-5)', color: 'var(--ink-3)', cursor: 'not-allowed' }}>Fix errors to place order</button>
            </div>
          </div>

          <div>
            <div className="hf-card" style={{ padding: 18, background: 'var(--paper)' }}>
              <div className="hf-eyebrow" style={{ marginBottom: 12 }}>Order summary</div>
              <div className="hf-flex hf-between" style={{ marginBottom: 6, fontSize: 13 }}><span className="hf-muted">Subtotal · 4 items</span><span className="hf-num">{money(214)}</span></div>
              <div className="hf-flex hf-between" style={{ marginBottom: 6, fontSize: 13 }}><span className="hf-muted">Shipping</span><span className="hf-num">{money(22)}</span></div>
              <div className="hf-flex hf-between" style={{ marginBottom: 12, fontSize: 13 }}><span className="hf-muted">Tax</span><span className="hf-num">{money(18.19)}</span></div>
              <div className="hf-divider" />
              <div className="hf-flex hf-between hf-items-baseline" style={{ marginTop: 12 }}>
                <span className="hf-h4">Total</span>
                <span className="hf-display hf-num" style={{ fontSize: 22 }}>{money(254.19)}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>card declined · ZIP typo</DesktopAction>
    </div>
  );
}

function DState_Success() {
  return (
    <div className="hf hf-desktop" style={{ position: 'relative' }}>
      <BrowserChrome url="mira.studio/p/persimmon-vase" />
      <ShopperTopbar cartCount={1} />
      <div className="hf-grow" style={{ overflow: 'hidden', position: 'relative', padding: '20px 32px' }}>
        {/* faded product behind */}
        <div className="hf-grid" style={{ gridTemplateColumns: '1fr 380px', gap: 20, opacity: 0.55 }}>
          <div className="hf-img hf-img-clay" style={{ height: 540, borderRadius: 'var(--r-lg)' }} />
          <div>
            <div className="hf-display" style={{ fontSize: 36 }}>Persimmon vase</div>
          </div>
        </div>

        {/* Floating success toast — top right under topbar */}
        <div style={{ position: 'absolute', top: 16, right: 32, width: 360, zIndex: 3 }}>
          <div className="hf-card" style={{ padding: 14, background: 'var(--paper)', borderRadius: 12, boxShadow: 'var(--shadow-3)', borderColor: 'transparent' }}>
            <div className="hf-flex hf-items-start hf-gap-3">
              <div className="hf-center" style={{ width: 36, height: 36, borderRadius: 999, background: 'var(--forest-2)', color: 'var(--good)', flexShrink: 0 }}>
                <Ico n="check" s={18} sw={2.4} />
              </div>
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13.5 }}>Added to bag</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>Persimmon vase · Medium · Persimmon</div>
                <div className="hf-flex hf-gap-2" style={{ marginTop: 10 }}>
                  <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ border: '1px solid var(--line-2)' }}>Keep browsing</button>
                  <button className="hf-btn hf-btn-primary hf-btn-sm">View bag (1)</button>
                </div>
              </div>
              <button className="hf-icon-btn"><Ico n="x" s={12} /></button>
            </div>
            {/* free-ship progress */}
            <div style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--line)' }}>
              <div className="hf-flex hf-between" style={{ marginBottom: 6 }}>
                <span className="hf-tiny hf-muted">Free shipping unlocked</span>
                <span className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600 }}>$86 / $80 ✓</span>
              </div>
              <div className="hf-progress" style={{ height: 4 }}><i style={{ width: '100%', background: 'var(--good)' }} /></div>
            </div>
          </div>
        </div>

        {/* small celebratory confetti */}
        <svg style={{ position: 'absolute', top: 60, right: 360, width: 120, height: 120, pointerEvents: 'none', zIndex: 2 }} viewBox="0 0 120 120">
          {[
            ['M 20 30 l 4 -8', '#C2410C'],
            ['M 30 60 l -6 -3', '#1B5E3F'],
            ['M 60 20 l 3 -8', '#E8A33C'],
            ['M 80 50 l 6 4',  '#5B2A6E'],
            ['M 95 80 l -3 7', '#C2410C'],
            ['M 50 90 l 8 -2', '#1B5E3F'],
          ].map((c, i) => <path key={i} d={c[0]} stroke={c[1]} strokeWidth="2.5" strokeLinecap="round" fill="none" />)}
          {[[28, 22], [70, 38], [100, 70], [40, 80], [88, 22]].map((p, i) => (
            <circle key={i} cx={p[0]} cy={p[1]} r="2.5" fill={['#C2410C','#1B5E3F','#E8A33C','#5B2A6E','#C2410C'][i]} />
          ))}
        </svg>
      </div>
      <DesktopAction>added to bag · free shipping unlocked</DesktopAction>
    </div>
  );
}

Object.assign(window, {
  BrowserChrome,
  DFlow_01_Landing, DFlow_02_Home, DFlow_03_Category, DFlow_04_Product,
  DFlow_05_Added, DFlow_06_Cart, DFlow_07_Checkout, DFlow_08_Confirmed,
  DState_Loading, DState_EmptyCart, DState_FormError, DState_Success,
});

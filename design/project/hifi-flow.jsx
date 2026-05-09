// hifi-flow.jsx — End-to-end shopper journey, one cohesive mobile flow
// Mira (a buyer) discovers Mira Studio → onboards → browses → buys.
// 8 happy-path steps + 4 in-flow states (loading / empty / error / success).

// ── Utility: a small "user action" caption that sits under the artboard label.
// Rendered inside the phone, anchored to the bottom of the inner container so
// it reads like a director's note: "what the user just did before this screen".
function FlowAction({ children }) {
  return (
    <div style={{
      position: 'absolute', left: 14, right: 14, bottom: 6, zIndex: 5,
      display: 'flex', alignItems: 'center', gap: 6,
      fontFamily: 'var(--sans)', fontSize: 10, color: 'var(--terra)',
      pointerEvents: 'none',
    }}>
      <svg width="14" height="10" viewBox="0 0 14 10" fill="none" stroke="currentColor" strokeWidth="1.4" strokeLinecap="round" strokeLinejoin="round">
        <path d="M1 5h11M9 1l4 4-4 4" />
      </svg>
      <span style={{ background: 'white', border: '1px solid var(--terra)', padding: '2px 7px', borderRadius: 999, whiteSpace: 'nowrap' }}>{children}</span>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 01 · WELCOME — first launch, brand splash
// ──────────────────────────────────────────────────────────────────────
function Flow_01_Welcome() {
  return (
    <div className="hf hf-phone" style={{ background: 'var(--ink)', color: 'var(--paper)' }}>
      <PhoneStatus />
      <div className="hf-phone-body" style={{ background: 'var(--ink)', color: 'var(--paper)', position: 'relative' }}>
        {/* subtle clay gradient peeking from behind */}
        <div className="hf-img hf-img-clay" style={{ position: 'absolute', inset: '40% -10% -20% -10%', filter: 'blur(60px)', opacity: 0.35, borderRadius: 0 }} />

        <div style={{ position: 'relative', zIndex: 1, padding: '40px 28px 0' }}>
          <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 8 }}>
            <span style={{ width: 28, height: 28, borderRadius: 8, background: 'var(--paper)', color: 'var(--ink)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 700, fontFamily: 'var(--serif)', fontSize: 16 }}>m.</span>
            <span className="hf-tiny" style={{ opacity: 0.7, letterSpacing: 0.08, textTransform: 'uppercase' }}>Micro Commerce</span>
          </div>
        </div>

        <div className="hf-grow" style={{ position: 'relative', zIndex: 1, padding: '0 28px', display: 'flex', flexDirection: 'column', justifyContent: 'flex-end', paddingBottom: 24 }}>
          <h1 className="hf-display" style={{ fontSize: 52, lineHeight: 0.95, color: 'var(--paper)', marginBottom: 12 }}>
            Small shops,<br /><i style={{ color: 'var(--sun)' }}>real people.</i>
          </h1>
          <p className="hf-body" style={{ opacity: 0.75, marginBottom: 24, maxWidth: 260 }}>
            Discover ceramicists, bakers, framers — buy direct from makers near you.
          </p>

          <div className="hf-col hf-gap-2">
            <button className="hf-btn hf-btn-block hf-btn-lg" style={{ background: 'var(--paper)', color: 'var(--ink)' }}>Get started</button>
            <button className="hf-btn hf-btn-block" style={{ background: 'transparent', color: 'var(--paper)', border: '1px solid rgba(255,255,255,0.3)' }}>I have an account</button>
          </div>

          <div className="hf-center" style={{ marginTop: 18, gap: 6, fontSize: 11, opacity: 0.55 }}>
            <Ico n="lock" s={11} /> <span>Browse without signing up</span>
          </div>
        </div>
      </div>
      <PhoneHome />
      <FlowAction>opens app for the first time</FlowAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · SETUP — goal-first onboarding (reuses existing variant C)
// Wrapped to add the flow action caption.
// ──────────────────────────────────────────────────────────────────────
function Flow_02_Setup() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Onboarding_GoalFirst />
      <FlowAction>tells us what she wants — in her own words</FlowAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · HOME — mobile storefront, tailored to her stated goal
// ──────────────────────────────────────────────────────────────────────
function Flow_03_Home() {
  const tiles = [
    { tone: 'clay',   t: 'Persimmon vase',  m: 'Mira Studio',   p: 86 },
    { tone: 'sage',   t: 'Forest bowl',     m: 'Atelier Hana',  p: 64 },
    { tone: 'rust',   t: 'Rust mug Nº 04',  m: 'Mira Studio',   p: 32 },
    { tone: 'cream',  t: 'Ribbon plate',    m: 'Quiet Goods',   p: 48 },
  ];
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'hidden' }}>
        {/* Top bar */}
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 12 }}>
          <div>
            <div className="hf-tiny hf-muted">Good morning</div>
            <div className="hf-h4">Mira</div>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-icon-btn" style={{ width: 34, height: 34, border: '1px solid var(--line)' }}><Ico n="search" s={15} /></button>
            <button className="hf-icon-btn" style={{ width: 34, height: 34, border: '1px solid var(--line)', position: 'relative' }}>
              <Ico n="bag" s={15} />
              <span style={{ position: 'absolute', top: -3, right: -3, width: 14, height: 14, fontSize: 9, fontWeight: 700, background: 'var(--terra)', color: 'white', borderRadius: 999, display: 'flex', alignItems: 'center', justifyContent: 'center', border: '2px solid var(--paper)' }}>0</span>
            </button>
          </div>
        </div>

        {/* Hero card */}
        <div className="hf-px-4" style={{ marginBottom: 14 }}>
          <div className="hf-relative hf-overflow-hidden" style={{ borderRadius: 14, height: 170 }}>
            <div className="hf-img hf-img-clay" style={{ position: 'absolute', inset: 0, borderRadius: 0 }} />
            <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(180deg, rgba(0,0,0,0) 50%, rgba(0,0,0,0.55))' }} />
            <div style={{ position: 'absolute', left: 14, right: 14, bottom: 12, color: 'white' }}>
              <div className="hf-eyebrow" style={{ color: 'rgba(255,255,255,0.8)' }}>Featured · ceramics</div>
              <div className="hf-display" style={{ fontSize: 22, lineHeight: 1.05, marginTop: 2 }}>Persimmon &amp; <i>peach</i> — fall drop from Mira Studio</div>
            </div>
          </div>
        </div>

        {/* Chips */}
        <div className="hf-px-4 hf-no-scrollbar" style={{ overflowX: 'auto', marginBottom: 10 }}>
          <div className="hf-flex hf-gap-2" style={{ paddingBottom: 4 }}>
            {[
              { l: 'For you', on: true },
              { l: 'Ceramics' },
              { l: 'New' },
              { l: 'Under $50' },
              { l: 'Local' },
            ].map((c, i) => <span key={i} className={`hf-chip ${c.on ? 'hf-chip-on' : ''}`} style={{ flexShrink: 0 }}>{c.l}</span>)}
          </div>
        </div>

        {/* Grid */}
        <div className="hf-grow hf-px-4" style={{ overflow: 'auto' }}>
          <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 8 }}>
            <span className="hf-h4">Picked for you</span>
            <span className="hf-tiny hf-muted">based on your setup</span>
          </div>
          <div className="hf-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: 10, paddingBottom: 12 }}>
            {tiles.map((t, i) => (
              <div key={i} className="hf-card hf-overflow-hidden" style={{ padding: 0 }}>
                <div className={`hf-img hf-img-${t.tone}`} style={{ height: 110, borderRadius: 0, position: 'relative' }}>
                  {i === 0 && <span style={{ position: 'absolute', top: 6, left: 6, padding: '2px 6px', borderRadius: 4, background: 'rgba(255,255,255,0.92)', fontSize: 9, fontWeight: 600 }}>NEW</span>}
                  <button className="hf-icon-btn" style={{ position: 'absolute', top: 6, right: 6, width: 24, height: 24, background: 'rgba(255,255,255,0.92)' }}><Ico n="heart" s={11} /></button>
                </div>
                <div style={{ padding: '8px 10px 10px' }}>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 1 }}>{t.m}</div>
                  <div className="hf-h4" style={{ fontSize: 12.5, lineHeight: 1.2 }}>{t.t}</div>
                  <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 4 }}>
                    <span className="hf-num" style={{ fontWeight: 600, fontSize: 12 }}>{money(t.p)}</span>
                    <Stars n={5} size={9} />
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Tab bar */}
        <div className="hf-flex hf-between" style={{ borderTop: '1px solid var(--line)', padding: '8px 24px 4px', background: 'var(--paper)' }}>
          {[
            { ic: 'home', l: 'Home', on: true },
            { ic: 'search', l: 'Search' },
            { ic: 'heart', l: 'Saved' },
            { ic: 'bag', l: 'Cart' },
            { ic: 'user', l: 'You' },
          ].map((t, i) => (
            <div key={i} className="hf-col hf-items-center hf-gap-1" style={{ color: t.on ? 'var(--ink)' : 'var(--ink-4)' }}>
              <Ico n={t.ic} s={18} />
              <span style={{ fontSize: 9.5, fontWeight: t.on ? 600 : 400 }}>{t.l}</span>
            </div>
          ))}
        </div>
      </div>
      <PhoneHome />
      <FlowAction>scrolls — taps the persimmon vase</FlowAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · PRODUCT — mobile detail
// ──────────────────────────────────────────────────────────────────────
function Flow_04_Product() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'hidden', position: 'relative' }}>
        {/* Hero image */}
        <div className="hf-img hf-img-clay hf-relative" style={{ height: 320, borderRadius: 0 }}>
          {/* Top controls */}
          <div className="hf-flex hf-between" style={{ position: 'absolute', top: 12, left: 14, right: 14 }}>
            <button className="hf-icon-btn" style={{ width: 34, height: 34, background: 'rgba(255,255,255,0.92)' }}><Ico n="chevL" s={15} /></button>
            <div className="hf-flex hf-gap-2">
              <button className="hf-icon-btn" style={{ width: 34, height: 34, background: 'rgba(255,255,255,0.92)' }}><Ico n="heart" s={15} /></button>
              <button className="hf-icon-btn" style={{ width: 34, height: 34, background: 'rgba(255,255,255,0.92)' }}><Ico n="ext" s={14} /></button>
            </div>
          </div>
          {/* Image dots */}
          <div className="hf-center" style={{ position: 'absolute', bottom: 12, left: 0, right: 0, gap: 4 }}>
            {[0,1,2,3].map(i => (
              <span key={i} style={{ width: i === 0 ? 18 : 5, height: 5, borderRadius: 999, background: i === 0 ? 'white' : 'rgba(255,255,255,0.55)' }} />
            ))}
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '14px 18px 8px' }}>
          {/* Maker row */}
          <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 8 }}>
            <Avatar name="Mira" size="sm" />
            <div className="hf-grow" style={{ minWidth: 0 }}>
              <div className="hf-tiny hf-muted">Mira Studio · Oakland CA</div>
              <div className="hf-flex hf-items-center hf-gap-1"><Stars n={5} size={10} /><span className="hf-tiny hf-muted">· 184 sales</span></div>
            </div>
            <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ border: '1px solid var(--line-2)' }}>Follow</button>
          </div>

          <h1 className="hf-display" style={{ fontSize: 30, lineHeight: 1, marginBottom: 4 }}>Persimmon <i>vase</i></h1>
          <div className="hf-flex hf-items-baseline hf-gap-2" style={{ marginBottom: 12 }}>
            <span className="hf-display hf-num" style={{ fontSize: 22 }}>{money(86)}</span>
            <span className="hf-tiny hf-muted">free shipping over $80</span>
          </div>

          <p className="hf-body hf-muted" style={{ fontSize: 13, marginBottom: 14 }}>
            Wheel-thrown stoneware, glazed in our house persimmon. One-of-one — no two are alike.
          </p>

          {/* Variant: size */}
          <div style={{ marginBottom: 12 }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 6 }}>
              <span className="hf-label" style={{ marginBottom: 0 }}>Size</span>
              <span className="hf-tiny hf-muted">Medium · 6.5"</span>
            </div>
            <div className="hf-flex hf-gap-2">
              {[
                { l: 'S', s: '4.5"' },
                { l: 'M', s: '6.5"', on: true },
                { l: 'L', s: '9"' },
              ].map((o, i) => (
                <div key={i} className="hf-center" style={{ flex: 1, padding: '8px', borderRadius: 10, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: o.on ? 'var(--paper-2)' : 'transparent', flexDirection: 'column', gap: 2 }}>
                  <span className="hf-h4" style={{ fontSize: 13 }}>{o.l}</span>
                  <span className="hf-tiny hf-muted" style={{ fontSize: 10 }}>{o.s}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Variant: glaze */}
          <div style={{ marginBottom: 14 }}>
            <span className="hf-label">Glaze</span>
            <div className="hf-flex hf-gap-2">
              {[
                { c: '#C7754F', on: true, l: 'Persimmon' },
                { c: '#92A487', l: 'Sage' },
                { c: '#1F1D1B', l: 'Coal' },
              ].map((g, i) => (
                <div key={i} className="hf-flex hf-items-center hf-gap-2" style={{ padding: '6px 10px 6px 6px', borderRadius: 999, border: g.on ? '1.5px solid var(--ink)' : '1px solid var(--line)' }}>
                  <span style={{ width: 18, height: 18, borderRadius: 999, background: g.c, border: '1px solid rgba(0,0,0,0.08)' }} />
                  <span className="hf-tiny" style={{ fontWeight: 500 }}>{g.l}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Trust row */}
          <div className="hf-flex hf-gap-3" style={{ paddingTop: 8, borderTop: '1px solid var(--line)' }}>
            <div className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"><Ico n="truck" s={12} /> Ships in 3 days</div>
            <div className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"><Ico n="refresh" s={12} /> 30-day returns</div>
          </div>
        </div>

        {/* Sticky CTA */}
        <div className="hf-flex hf-gap-2" style={{ padding: '10px 18px 12px', borderTop: '1px solid var(--line)', background: 'var(--paper)' }}>
          <button className="hf-btn hf-btn-outline" style={{ width: 44 }}><Ico n="chat" s={14} /></button>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg" style={{ flex: 1 }}>Add to cart · <span className="hf-num">{money(86)}</span></button>
        </div>
      </div>
      <PhoneHome />
      <FlowAction>picks Medium · Persimmon · adds to cart</FlowAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05–07 — reuse existing strongest mobile variants
// ──────────────────────────────────────────────────────────────────────
function Flow_05_Cart() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Cart_List />
      <FlowAction>reviews items · taps Checkout</FlowAction>
    </div>
  );
}

function Flow_06_Shipping() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Checkout_Steps />
      <FlowAction>fills shipping · continues to pay</FlowAction>
    </div>
  );
}

function Flow_07_Pay() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Checkout_Express />
      <FlowAction>double-clicks side button to pay</FlowAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 08 · CONFIRMED — order placed, what's next
// ──────────────────────────────────────────────────────────────────────
function Flow_08_Confirmed() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        {/* Hero confirmation */}
        <div style={{ padding: '32px 24px 18px', textAlign: 'center' }}>
          <div className="hf-center" style={{ width: 64, height: 64, borderRadius: 999, background: 'var(--forest-2)', color: 'var(--good)', margin: '0 auto 14px' }}>
            <Ico n="check" s={28} sw={2.4} />
          </div>
          <div className="hf-eyebrow" style={{ color: 'var(--good)', marginBottom: 6 }}>Order placed</div>
          <h1 className="hf-display" style={{ fontSize: 30, lineHeight: 1.05, marginBottom: 6 }}>Thanks, <i>Mira.</i></h1>
          <p className="hf-body hf-muted" style={{ fontSize: 13 }}>
            Order <span className="hf-num" style={{ color: 'var(--ink)' }}>#MS-3142</span> · receipt sent to mira@studio.co
          </p>
        </div>

        <div className="hf-px-4" style={{ paddingBottom: 12 }}>
          {/* Tracking timeline */}
          <div className="hf-card" style={{ padding: 14, marginBottom: 10 }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 12 }}>
              <span className="hf-h4">Arrives Wed–Thu</span>
              <span className="hf-tiny hf-muted">Express · UPS</span>
            </div>
            <div className="hf-flex hf-items-center" style={{ gap: 0 }}>
              {[
                { l: 'Confirmed', on: true, done: true },
                { l: 'Packed', on: true },
                { l: 'Shipped' },
                { l: 'Delivered' },
              ].map((s, i, a) => (
                <React.Fragment key={i}>
                  <div className="hf-col hf-items-center hf-gap-1" style={{ flexShrink: 0 }}>
                    <div className="hf-center" style={{
                      width: 22, height: 22, borderRadius: 999,
                      background: s.done ? 'var(--good)' : s.on ? 'var(--paper-2)' : 'transparent',
                      border: s.on || s.done ? 'none' : '1px solid var(--line-2)',
                      color: s.done ? 'var(--paper)' : 'var(--ink-4)',
                    }}>
                      {s.done ? <Ico n="check" s={11} sw={2.5} /> : <span style={{ width: 6, height: 6, borderRadius: 999, background: s.on ? 'var(--ink)' : 'var(--ink-5)' }} />}
                    </div>
                    <span className="hf-tiny" style={{ fontSize: 9.5, fontWeight: s.on || s.done ? 600 : 400, color: s.on || s.done ? 'var(--ink)' : 'var(--ink-4)' }}>{s.l}</span>
                  </div>
                  {i < a.length - 1 && <div style={{ flex: 1, height: 1.5, background: i === 0 ? 'var(--good)' : 'var(--line)', margin: '0 4px', marginBottom: 14 }} />}
                </React.Fragment>
              ))}
            </div>
          </div>

          {/* Items mini */}
          <div className="hf-card" style={{ padding: '12px 14px', marginBottom: 10 }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 10 }}>
              <span className="hf-h4">3 items · {money(204)}</span>
              <a className="hf-tiny" style={{ color: 'var(--ink-2)', textDecoration: 'underline' }}>Receipt</a>
            </div>
            <div className="hf-flex hf-gap-2">
              {[
                { tone: 'clay', q: 1 },
                { tone: 'sage', q: 2 },
                { tone: 'rust', q: 1 },
              ].map((it, i) => (
                <div key={i} className="hf-relative" style={{ flexShrink: 0 }}>
                  <div className={`hf-img hf-img-${it.tone}`} style={{ width: 52, height: 52, borderRadius: 8 }} />
                  <span style={{ position: 'absolute', top: -6, right: -6, minWidth: 18, height: 18, padding: '0 5px', background: 'var(--ink)', color: 'var(--paper)', borderRadius: 999, fontSize: 10, fontWeight: 700, display: 'flex', alignItems: 'center', justifyContent: 'center', border: '2px solid var(--paper)' }}>{it.q}</span>
                </div>
              ))}
            </div>
          </div>

          {/* Maker note card */}
          <div className="hf-card" style={{ padding: 14, background: 'var(--paper-2)', marginBottom: 12, borderColor: 'var(--paper-2)' }}>
            <div className="hf-flex hf-items-start hf-gap-3">
              <Avatar name="Mira" size="sm" />
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 12.5 }}>A note from Mira</div>
                <p className="hf-tiny hf-muted" style={{ marginTop: 4, fontSize: 11.5, lineHeight: 1.45 }}>"Thank you so much. I'll wrap these by hand on Friday — keep an eye out for a small surprise."</p>
              </div>
            </div>
          </div>
        </div>

        <div className="hf-px-4" style={{ paddingTop: 12, paddingBottom: 14, borderTop: '1px solid var(--line)' }}>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-outline hf-btn-block" style={{ flex: 1 }}>Track order</button>
            <button className="hf-btn hf-btn-primary hf-btn-block" style={{ flex: 1 }}>Keep shopping</button>
          </div>
        </div>
      </div>
      <PhoneHome />
      <FlowAction>screen-shots the receipt · texts a friend</FlowAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// IN-FLOW STATES — loading · empty · error · success
// Each maps onto a step it interrupts.
// ──────────────────────────────────────────────────────────────────────

// State A · Loading skeleton (during step 03 · home)
function State_Loading() {
  const Skel = ({ w = '100%', h = 12, r = 4, style, ...rest }) => (
    <div {...rest} style={{ width: w, height: h, borderRadius: r, background: 'linear-gradient(90deg, var(--paper-2) 0%, var(--line) 50%, var(--paper-2) 100%)', backgroundSize: '200% 100%', animation: 'hf-shimmer 1.4s linear infinite', ...style }} />
  );
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <style>{`@keyframes hf-shimmer { 0% { background-position: 200% 0; } 100% { background-position: -200% 0; } }`}</style>
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 12 }}>
          <div>
            <Skel w={70} h={9} style={{ marginBottom: 6 }} />
            <Skel w={50} h={14} />
          </div>
          <div className="hf-flex hf-gap-2">
            <Skel w={34} h={34} r={999} />
            <Skel w={34} h={34} r={999} />
          </div>
        </div>
        <div className="hf-px-4" style={{ marginBottom: 14 }}>
          <Skel h={170} r={14} />
        </div>
        <div className="hf-px-4 hf-flex hf-gap-2" style={{ marginBottom: 12 }}>
          {[60, 80, 70, 90, 60].map((w, i) => <Skel key={i} w={w} h={26} r={999} />)}
        </div>
        <div className="hf-px-4 hf-grid" style={{ gridTemplateColumns: '1fr 1fr', gap: 10 }}>
          {[0,1,2,3].map(i => (
            <div key={i} className="hf-card" style={{ padding: 0, overflow: 'hidden' }}>
              <Skel h={110} r={0} />
              <div style={{ padding: 10 }}>
                <Skel w="50%" h={9} style={{ marginBottom: 6 }} />
                <Skel w="80%" h={11} style={{ marginBottom: 8 }} />
                <Skel w="35%" h={11} />
              </div>
            </div>
          ))}
        </div>
      </div>
      <PhoneHome />
      <FlowAction>first paint — slow connection</FlowAction>
    </div>
  );
}

// State B · Empty cart (during step 05 · cart)
function State_EmptyCart() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 12 }}>
          <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
          <span className="hf-h4">Cart</span>
          <span style={{ width: 30 }} />
        </div>

        <div className="hf-grow hf-center hf-px-5" style={{ flexDirection: 'column', textAlign: 'center', gap: 0 }}>
          {/* big bag icon */}
          <div className="hf-center" style={{ width: 96, height: 96, borderRadius: 999, background: 'var(--paper-2)', marginBottom: 16, position: 'relative' }}>
            <Ico n="bag" s={42} sw={1.4} />
            <span style={{ position: 'absolute', bottom: 8, right: 6, width: 24, height: 24, borderRadius: 999, background: 'var(--paper)', border: '1.5px solid var(--ink)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 14, fontWeight: 700 }}>0</span>
          </div>
          <h2 className="hf-display" style={{ fontSize: 26, lineHeight: 1.05, marginBottom: 6 }}>Your bag is <i>empty.</i></h2>
          <p className="hf-body hf-muted" style={{ fontSize: 13, maxWidth: 240, marginBottom: 18 }}>
            Save things you love and they'll wait here for you.
          </p>

          {/* "last viewed" mini rail */}
          <div className="hf-px-4" style={{ alignSelf: 'stretch', marginTop: 8 }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 8 }}>
              <span className="hf-tiny hf-muted" style={{ fontWeight: 500 }}>You were looking at</span>
              <a className="hf-tiny" style={{ color: 'var(--ink-2)', textDecoration: 'underline' }}>See all</a>
            </div>
            <div className="hf-flex hf-gap-2 hf-no-scrollbar" style={{ overflowX: 'auto' }}>
              {[
                { tone: 'clay', t: 'Persimmon vase', p: 86 },
                { tone: 'sage', t: 'Forest bowl',    p: 64 },
                { tone: 'rust', t: 'Rust mug',       p: 32 },
              ].map((p, i) => (
                <div key={i} className="hf-card" style={{ padding: 0, overflow: 'hidden', flexShrink: 0, width: 110, textAlign: 'left' }}>
                  <div className={`hf-img hf-img-${p.tone}`} style={{ height: 80, borderRadius: 0 }} />
                  <div style={{ padding: '6px 8px 8px' }}>
                    <div className="hf-h4" style={{ fontSize: 11, lineHeight: 1.2 }}>{p.t}</div>
                    <div className="hf-num" style={{ fontWeight: 600, fontSize: 11, marginTop: 2 }}>{money(p.p)}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="hf-px-5" style={{ paddingTop: 12, paddingBottom: 14, borderTop: '1px solid var(--line)' }}>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">Browse shops</button>
        </div>
      </div>
      <PhoneHome />
      <FlowAction>opens cart with nothing in it</FlowAction>
    </div>
  );
}

// State C · Form error (during step 06 · shipping)
function State_FormError() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 14 }}>
          <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
          <span className="hf-h4">Checkout</span>
          <span style={{ width: 30 }} />
        </div>

        {/* Inline error banner */}
        <div className="hf-px-4" style={{ marginBottom: 12 }}>
          <div className="hf-flex hf-items-start hf-gap-2" style={{ padding: '10px 12px', borderRadius: 10, background: 'var(--rose, #F8DDDA)', border: '1px solid #DD6660', color: '#8A2C26' }}>
            <Ico n="info" s={14} />
            <div style={{ fontSize: 12, lineHeight: 1.35 }}>
              <div style={{ fontWeight: 600 }}>2 fields need attention</div>
              <div style={{ opacity: 0.8 }}>Check ZIP and card number to continue.</div>
            </div>
          </div>
        </div>

        <div className="hf-grow hf-px-5" style={{ overflow: 'auto' }}>
          <div className="hf-eyebrow" style={{ marginBottom: 8 }}>Where to?</div>
          <h2 className="hf-display" style={{ fontSize: 26, lineHeight: 1, marginBottom: 14 }}>Shipping <i>address</i></h2>

          <div className="hf-col hf-gap-3">
            <div>
              <span className="hf-label">Email</span>
              <input className="hf-input" defaultValue="mira@studio.co" />
            </div>
            <div>
              <span className="hf-label">Street address</span>
              <input className="hf-input" defaultValue="241 Telegraph Ave" />
            </div>
            <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr 0.9fr' }}>
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
            <div>
              <span className="hf-label" style={{ color: '#A93A33' }}>Card number</span>
              <input className="hf-input" defaultValue="4242 4242 4242 0000" style={{ borderColor: '#DD6660', background: 'rgba(221,102,96,0.06)' }} />
              <div className="hf-tiny" style={{ color: '#A93A33', marginTop: 4, display: 'flex', alignItems: 'center', gap: 4 }}>
                <Ico n="info" s={11} /> This card was declined. Try another.
              </div>
            </div>
          </div>
        </div>

        <div className="hf-px-5" style={{ paddingTop: 12, paddingBottom: 14, borderTop: '1px solid var(--line)' }}>
          <button className="hf-btn hf-btn-block hf-btn-lg" style={{ background: 'var(--ink-5)', color: 'var(--ink-3)', cursor: 'not-allowed' }}>Fix errors to continue</button>
        </div>
      </div>
      <PhoneHome />
      <FlowAction>typo in ZIP · card declined</FlowAction>
    </div>
  );
}

// State D · Success toast (interstitial after step 04 · added to cart)
function State_Success() {
  return (
    <div className="hf hf-phone" style={{ position: 'relative' }}>
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'hidden', position: 'relative' }}>
        {/* Faded product behind */}
        <div className="hf-img hf-img-clay" style={{ position: 'absolute', inset: 0, opacity: 0.5, borderRadius: 0, filter: 'blur(2px)' }} />
        <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.18)' }} />

        {/* Floating toast */}
        <div style={{ position: 'absolute', top: 64, left: 16, right: 16, zIndex: 3 }}>
          <div className="hf-card" style={{ padding: 12, background: 'var(--paper)', borderRadius: 14, boxShadow: 'var(--shadow-3)' }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <div className="hf-center" style={{ width: 36, height: 36, borderRadius: 999, background: 'var(--forest-2)', color: 'var(--good)' }}>
                <Ico n="check" s={18} sw={2.4} />
              </div>
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13 }}>Added to bag</div>
                <div className="hf-tiny hf-muted">Persimmon vase · Medium</div>
              </div>
              <button className="hf-icon-btn"><Ico n="x" s={12} /></button>
            </div>
          </div>
        </div>

        {/* Bottom celebration sheet */}
        <div style={{ marginTop: 'auto', background: 'var(--paper)', borderRadius: '20px 20px 0 0', boxShadow: '0 -8px 30px rgba(0,0,0,0.2)', padding: '14px 18px 0', position: 'relative', zIndex: 2 }}>
          <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--ink-5)', margin: '0 auto 14px' }} />

          <div className="hf-flex hf-items-center hf-gap-3" style={{ marginBottom: 14 }}>
            <div className="hf-img hf-img-clay" style={{ width: 56, height: 56, borderRadius: 10 }} />
            <div className="hf-grow">
              <div className="hf-h4">Persimmon vase</div>
              <div className="hf-tiny hf-muted">Medium · Persimmon</div>
              <div className="hf-num" style={{ fontWeight: 600, marginTop: 2, fontSize: 13 }}>{money(86)}</div>
            </div>
          </div>

          {/* Smart upsell */}
          <div style={{ marginBottom: 12 }}>
            <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 8 }}>
              <span className="hf-tiny hf-muted" style={{ fontWeight: 600, letterSpacing: 0.04, textTransform: 'uppercase' }}>Pairs well with</span>
              <span className="hf-tiny hf-muted">Free ship at $80</span>
            </div>
            <div className="hf-flex hf-gap-2 hf-no-scrollbar" style={{ overflowX: 'auto' }}>
              {[
                { tone: 'sage',  t: 'Forest bowl',    p: 64 },
                { tone: 'rust',  t: 'Rust mug Nº 04', p: 32 },
                { tone: 'cream', t: 'Ribbon plate',   p: 48 },
              ].map((p, i) => (
                <div key={i} className="hf-card" style={{ padding: 0, overflow: 'hidden', flexShrink: 0, width: 100 }}>
                  <div className={`hf-img hf-img-${p.tone}`} style={{ height: 70, borderRadius: 0 }} />
                  <div style={{ padding: '6px 8px 8px' }}>
                    <div className="hf-h4" style={{ fontSize: 10.5, lineHeight: 1.2 }}>{p.t}</div>
                    <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 4 }}>
                      <span className="hf-num" style={{ fontWeight: 600, fontSize: 10.5 }}>{money(p.p)}</span>
                      <span className="hf-icon-btn" style={{ width: 18, height: 18, background: 'var(--ink)', color: 'var(--paper)' }}><Ico n="plus" s={10} sw={2.4} /></span>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>

          <div className="hf-flex hf-gap-2" style={{ paddingBottom: 14 }}>
            <button className="hf-btn hf-btn-outline hf-btn-block" style={{ flex: 1 }}>Keep browsing</button>
            <button className="hf-btn hf-btn-primary hf-btn-block" style={{ flex: 1 }}>View bag (1)</button>
          </div>
        </div>
      </div>
      <PhoneHome />
      <FlowAction>just tapped Add to cart</FlowAction>
    </div>
  );
}

Object.assign(window, {
  Flow_01_Welcome, Flow_02_Setup, Flow_03_Home, Flow_04_Product,
  Flow_05_Cart, Flow_06_Shipping, Flow_07_Pay, Flow_08_Confirmed,
  State_Loading, State_EmptyCart, State_FormError, State_Success,
});

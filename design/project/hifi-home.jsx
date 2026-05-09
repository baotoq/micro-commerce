// hifi-home.jsx — 3 desktop storefront homes (1100x720)

// Shared mini header for shopper storefront
function ShopperTopbar({ shop = 'Mira Studio', cartCount = 2, dark = false }) {
  return (
    <div className="hf-flex hf-items-center" style={{ padding: '14px 28px', borderBottom: '1px solid var(--line)', gap: 28, background: dark ? 'transparent' : 'var(--paper)' }}>
      <span className="hf-logo" style={{ color: dark ? 'white' : undefined }}>{shop}</span>
      <div className="hf-flex hf-gap-5" style={{ marginLeft: 8 }}>
        {['Shop', 'Collections', 'Journal', 'About'].map((l, i) => (
          <a key={l} style={{ fontSize: 12.5, color: dark ? 'rgba(255,255,255,0.85)' : (i === 0 ? 'var(--ink)' : 'var(--ink-3)'), fontWeight: 500, textDecoration: 'none' }}>{l}</a>
        ))}
      </div>
      <div className="hf-grow" />
      <div className="hf-flex hf-items-center hf-gap-2">
        <div className="hf-flex hf-items-center hf-gap-2" style={{ height: 30, padding: '0 10px', background: dark ? 'rgba(255,255,255,0.1)' : 'var(--paper-2)', borderRadius: 999, color: dark ? 'rgba(255,255,255,0.7)' : 'var(--ink-3)', width: 200 }}>
          <Ico n="search" s={13} />
          <span style={{ fontSize: 12 }}>Search products…</span>
        </div>
        <button className="hf-icon-btn" style={{ color: dark ? 'white' : undefined }}><Ico n="user" s={15} /></button>
        <button className="hf-icon-btn hf-relative" style={{ color: dark ? 'white' : undefined }}>
          <Ico n="bag" s={15} />
          <span style={{ position: 'absolute', top: 1, right: 1, width: 14, height: 14, background: 'var(--terra)', color: 'white', fontSize: 9, fontWeight: 700, borderRadius: 999, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>{cartCount}</span>
        </button>
      </div>
    </div>
  );
}

// ── A · Edge-to-edge tiles — magazine grid, photography-led
function Home_Tiles() {
  const products = [
    { tone: 'clay',   title: 'Persimmon vase',  price: 86,  badge: 'New' },
    { tone: 'sage',   title: 'Forest bowl, lg.', price: 64 },
    { tone: 'bone',   title: 'Cream tumbler — set of 2', price: 48 },
    { tone: 'rose',   title: 'Soft hand vessel', price: 92, badge: '2 left' },
    { tone: 'cobalt', title: 'Indigo carafe', price: 110 },
    { tone: 'cream',  title: 'Bone dinner plate', price: 38 },
    { tone: 'rust',   title: 'Rust mug, Nº 04', price: 32 },
    { tone: 'moss',   title: 'Moss saucer set', price: 44 },
  ];
  return (
    <div className="hf hf-desktop">
      <ShopperTopbar />
      <div className="hf-grow hf-overflow-hidden" style={{ overflow: 'hidden' }}>
        {/* Hero strip */}
        <div className="hf-relative" style={{ height: 200, overflow: 'hidden' }}>
          <div className="hf-img hf-img-clay" style={{ height: '100%', borderRadius: 0 }} />
          <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(90deg, rgba(0,0,0,0.45), rgba(0,0,0,0.05) 60%)' }} />
          <div style={{ position: 'absolute', left: 28, top: 38, color: 'white', maxWidth: 460 }}>
            <div className="hf-eyebrow" style={{ color: 'rgba(255,255,255,0.8)' }}>Spring '26 · Vessels</div>
            <h1 className="hf-display" style={{ fontSize: 44, color: 'white', marginTop: 8, lineHeight: 1 }}>Hand-thrown for slow<br /><i>mornings.</i></h1>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 14 }}>
              <button className="hf-btn hf-btn-lg" style={{ background: 'white', color: 'var(--ink)' }}>Shop the drop<Ico n="arrowR" s={13} /></button>
              <button className="hf-btn hf-btn-lg" style={{ background: 'transparent', border: '1px solid rgba(255,255,255,0.5)', color: 'white' }}>The studio</button>
            </div>
          </div>
        </div>

        {/* Filter bar */}
        <div className="hf-flex hf-items-center hf-between" style={{ padding: '14px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-gap-2">
            {['All', 'Vessels', 'Tableware', 'Drinkware', 'Limited'].map((c, i) => (
              <span key={c} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`}>{c}</span>
            ))}
          </div>
          <div className="hf-flex hf-items-center hf-gap-3 hf-small">
            <span className="hf-muted">42 pieces</span>
            <span className="hf-flex hf-items-center hf-gap-1" style={{ color: 'var(--ink-2)', fontWeight: 500 }}>Sort: Newest <Ico n="chevD" s={11} /></span>
          </div>
        </div>

        {/* 4-up grid */}
        <div style={{ padding: '20px 28px', display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: 20 }}>
          {products.slice(0, 4).map((p, i) => (
            <div key={i}>
              <ProdImg tone={p.tone} h={170} badge={p.badge} />
              <div className="hf-flex hf-between" style={{ marginTop: 10 }}>
                <span className="hf-h4">{p.title}</span>
                <span className="hf-h4 hf-num">{money(p.price)}</span>
              </div>
              <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>Stoneware · 4 colors</div>
            </div>
          ))}
        </div>
      </div>
      <Anno top={20} right={28}>full-bleed photography hero</Anno>
      <Anno top={272} left={28}>chip filters · 4-up grid</Anno>
    </div>
  );
}

// ── B · Editorial mosaic — content-led, story+products mixed
function Home_Editorial() {
  return (
    <div className="hf hf-desktop">
      <ShopperTopbar />
      <div className="hf-grow" style={{ padding: 24, overflow: 'hidden' }}>
        {/* Editorial mosaic grid */}
        <div style={{ display: 'grid', gridTemplateColumns: '1.4fr 1fr 1fr', gridTemplateRows: '180px 180px 110px', gap: 12 }}>
          {/* Big editorial card */}
          <div className="hf-relative" style={{ gridRow: '1 / span 2', borderRadius: 'var(--r-lg)', overflow: 'hidden', background: 'var(--paper-2)' }}>
            <div className="hf-img hf-img-clay" style={{ height: '100%', borderRadius: 0 }} />
            <div style={{ position: 'absolute', inset: 0, padding: 24, display: 'flex', flexDirection: 'column', justifyContent: 'flex-end', background: 'linear-gradient(180deg, rgba(0,0,0,0.05) 40%, rgba(0,0,0,0.55))', color: 'white' }}>
              <div className="hf-eyebrow" style={{ color: 'rgba(255,255,255,0.85)' }}>Journal · Issue 04</div>
              <h2 className="hf-display" style={{ fontSize: 36, color: 'white', marginTop: 6, lineHeight: 1 }}>The fire log:<br /><i>a season at the kiln</i></h2>
              <p style={{ fontSize: 12, marginTop: 8, color: 'rgba(255,255,255,0.85)', maxWidth: 280 }}>Field notes from spring firings — every piece in the new drop and how it came to be.</p>
              <span style={{ fontSize: 11, marginTop: 10, fontWeight: 500 }}>Read essay →</span>
            </div>
          </div>
          {/* product cells */}
          <div className="hf-relative hf-overflow-hidden" style={{ borderRadius: 'var(--r-lg)' }}>
            <ProdImg tone="rose" h={180} r={'var(--r-lg)'} />
            <div style={{ position: 'absolute', bottom: 10, left: 12, color: 'white', textShadow: '0 1px 4px rgba(0,0,0,0.4)' }}>
              <div style={{ fontSize: 13, fontWeight: 600 }}>Soft hand vessel</div>
              <div className="hf-num" style={{ fontSize: 11.5 }}>{money(92)}</div>
            </div>
          </div>
          <div className="hf-card hf-flex hf-col" style={{ padding: 16, justifyContent: 'space-between', background: 'var(--ink)', color: 'var(--paper)', borderColor: 'transparent' }}>
            <div>
              <div className="hf-eyebrow" style={{ color: 'rgba(255,255,255,0.6)' }}>Limited</div>
              <div className="hf-display" style={{ fontSize: 22, color: 'var(--paper)', marginTop: 4 }}>Indigo carafe</div>
              <p style={{ fontSize: 11.5, color: 'rgba(255,255,255,0.7)', marginTop: 6 }}>Run of 12. Hand-glazed in cobalt.</p>
            </div>
            <div className="hf-flex hf-between hf-items-end">
              <span className="hf-num hf-display" style={{ fontSize: 24, color: 'var(--paper)' }}>{money(110)}</span>
              <button className="hf-btn hf-btn-sm" style={{ background: 'var(--paper)', color: 'var(--ink)' }}>Add</button>
            </div>
          </div>
          <div className="hf-relative hf-overflow-hidden" style={{ borderRadius: 'var(--r-lg)' }}>
            <ProdImg tone="moss" h={180} r={'var(--r-lg)'} />
            <div style={{ position: 'absolute', bottom: 10, left: 12, color: 'white', textShadow: '0 1px 4px rgba(0,0,0,0.4)' }}>
              <div style={{ fontSize: 13, fontWeight: 600 }}>Forest bowl</div>
              <div className="hf-num" style={{ fontSize: 11.5 }}>{money(64)}</div>
            </div>
          </div>
          <div className="hf-card hf-flex hf-items-center hf-gap-3" style={{ padding: '0 18px', background: 'var(--paper-2)', borderColor: 'transparent' }}>
            <div className="hf-flex hf-gap-1">
              {['rose','sage','clay','cream','rust'].map(t => (
                <div key={t} className={`hf-img hf-img-${t}`} style={{ width: 28, height: 28, borderRadius: 999 }} />
              ))}
            </div>
            <div className="hf-grow">
              <div className="hf-h4">5 colorways</div>
              <div className="hf-tiny hf-muted">Pick the season</div>
            </div>
            <Ico n="arrowR" s={14} />
          </div>
          <div className="hf-card hf-flex hf-col" style={{ padding: 16, justifyContent: 'center', borderColor: 'transparent', background: 'var(--terra-2)' }}>
            <div className="hf-display" style={{ fontSize: 24, color: 'var(--terra)', lineHeight: 1.05 }}>Free local<br /><i>delivery</i></div>
            <div className="hf-tiny hf-muted" style={{ marginTop: 6, color: 'var(--terra)' }}>SF · Oakland · Berkeley</div>
          </div>
          <div className="hf-relative hf-overflow-hidden" style={{ borderRadius: 'var(--r-lg)' }}>
            <ProdImg tone="bone" h={110} r={'var(--r-lg)'} />
            <div style={{ position: 'absolute', bottom: 8, left: 10, color: 'var(--ink)' }}>
              <div style={{ fontSize: 12, fontWeight: 600 }}>Cream tumbler</div>
              <div className="hf-num" style={{ fontSize: 11 }}>{money(48)}</div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={140} left={28}>editorial-led mosaic</Anno>
    </div>
  );
}

// ── C · Hero + product rails — classic e-comm
function Home_Rails() {
  const rail = [
    { tone: 'clay',  title: 'Persimmon vase',     price: 86,  rating: 5 },
    { tone: 'sage',  title: 'Forest bowl',         price: 64,  rating: 5 },
    { tone: 'bone',  title: 'Cream tumbler set',   price: 48,  rating: 4 },
    { tone: 'rose',  title: 'Soft hand vessel',    price: 92,  rating: 5 },
    { tone: 'rust',  title: 'Rust mug Nº 04',      price: 32,  rating: 5 },
  ];
  return (
    <div className="hf hf-desktop">
      <ShopperTopbar />
      <div className="hf-grow" style={{ overflow: 'hidden' }}>
        {/* Hero — 2-col split */}
        <div style={{ padding: '24px 28px', display: 'grid', gridTemplateColumns: '1.1fr 1fr', gap: 20 }}>
          <div className="hf-flex hf-col hf-items-start" style={{ paddingTop: 18 }}>
            <span className="hf-eyebrow" style={{ color: 'var(--terra)' }}>· Now shipping ·</span>
            <h1 className="hf-display" style={{ fontSize: 56, lineHeight: 0.95, marginTop: 14 }}>The slow<br /><i>kitchen</i> drop.</h1>
            <p className="hf-body" style={{ fontSize: 14, marginTop: 14, color: 'var(--ink-2)', maxWidth: 380 }}>
              Twelve new pieces in clay, glaze, and fire — made one at a time in a small studio in Oakland.
            </p>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 18 }}>
              <button className="hf-btn hf-btn-primary hf-btn-lg">Shop the drop<Ico n="arrowR" s={13} /></button>
              <button className="hf-btn hf-btn-outline hf-btn-lg">Lookbook</button>
            </div>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 20 }}>
              <div className="hf-flex" style={{ marginLeft: 0 }}>
                {['MR','JT','AS'].map((n, i) => (
                  <div key={i} className="hf-avatar hf-avatar-sm" style={{ marginLeft: i ? -8 : 0, background: ['#E8C2B5','#C9D4BC','#E5C0A6'][i] }}>{n}</div>
                ))}
              </div>
              <Stars n={5} />
              <span className="hf-small">"the only mug I drink from now" · <span className="hf-muted">— Maya R.</span></span>
            </div>
          </div>
          <div className="hf-relative hf-overflow-hidden" style={{ borderRadius: 'var(--r-xl)' }}>
            <ProdImg tone="clay" h={260} r={'var(--r-xl)'} />
            <div style={{ position: 'absolute', bottom: 14, left: 14, right: 14, padding: 12, background: 'rgba(255,255,255,0.92)', borderRadius: 'var(--r-md)', display: 'flex', alignItems: 'center', gap: 10 }}>
              <div className="hf-img hf-img-clay" style={{ width: 36, height: 36, borderRadius: 8 }} />
              <div className="hf-grow">
                <div className="hf-h4">Persimmon vase</div>
                <div className="hf-tiny hf-muted">Stoneware · 26 cm</div>
              </div>
              <span className="hf-num hf-h4">{money(86)}</span>
              <button className="hf-btn hf-btn-primary hf-btn-sm">Add</button>
            </div>
          </div>
        </div>

        {/* Product rail */}
        <div style={{ padding: '0 28px 12px' }}>
          <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 12 }}>
            <h2 className="hf-h2"><i className="hf-display-2" style={{ fontStyle: 'italic' }}>Best loved</i></h2>
            <span className="hf-flex hf-items-center hf-gap-1 hf-small hf-muted">See all <Ico n="arrowR" s={11} /></span>
          </div>
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(5, 1fr)', gap: 14 }}>
            {rail.map((p, i) => (
              <div key={i}>
                <ProdImg tone={p.tone} h={130} />
                <div className="hf-h4" style={{ marginTop: 8 }}>{p.title}</div>
                <div className="hf-flex hf-between" style={{ marginTop: 2 }}>
                  <Stars n={p.rating} size={9} />
                  <span className="hf-num hf-small" style={{ fontWeight: 600 }}>{money(p.price)}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
      <Anno top={140} left={28}>narrative hero · product CTA pinned</Anno>
      <Anno top={420} left={28}>scrollable rail</Anno>
    </div>
  );
}

Object.assign(window, { Home_Tiles, Home_Editorial, Home_Rails, ShopperTopbar });

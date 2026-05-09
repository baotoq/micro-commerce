// hifi-product.jsx — 3 product detail layouts (1100x620)

// ── A · Photo · info classic — image left, info right
function Product_Classic() {
  return (
    <div className="hf hf-desktop">
      <ShopperTopbar />
      <div className="hf-grow" style={{ padding: '18px 28px', overflow: 'hidden' }}>
        <div className="hf-crumb" style={{ marginBottom: 14 }}>
          <a>Shop</a><span className="hf-crumb-sep">/</span>
          <a>Vessels</a><span className="hf-crumb-sep">/</span>
          <span style={{ color: 'var(--ink)' }}>Persimmon vase</span>
        </div>

        <div style={{ display: 'grid', gridTemplateColumns: '60px 1fr 380px', gap: 18, height: 460 }}>
          {/* Thumbs rail */}
          <div className="hf-col hf-gap-2">
            {['clay','rose','rust','bone'].map((t, i) => (
              <div key={t} className={`hf-img hf-img-${t}`} style={{ height: 60, borderRadius: 8, border: i === 0 ? '1.5px solid var(--ink)' : '1px solid var(--line)' }} />
            ))}
          </div>
          {/* Hero image */}
          <div className="hf-relative hf-overflow-hidden" style={{ borderRadius: 'var(--r-lg)' }}>
            <ProdImg tone="clay" h={'100%'} r={'var(--r-lg)'} />
            <div style={{ position: 'absolute', top: 14, left: 14, padding: '4px 10px', borderRadius: 999, background: 'rgba(255,255,255,0.92)', fontSize: 10.5, fontWeight: 600, color: 'var(--ink)' }}>NEW · Spring '26</div>
            <div style={{ position: 'absolute', bottom: 14, left: 14, display: 'flex', gap: 6 }}>
              {[1,2,3,4,5].map(i => <span key={i} style={{ width: 6, height: 6, borderRadius: 999, background: i === 1 ? 'var(--ink)' : 'rgba(0,0,0,0.25)' }} />)}
            </div>
            <button className="hf-icon-btn" style={{ position: 'absolute', top: 14, right: 14, background: 'rgba(255,255,255,0.92)' }}><Ico n="heart" s={14} /></button>
          </div>
          {/* Info column */}
          <div className="hf-col" style={{ paddingLeft: 8 }}>
            <span className="hf-eyebrow">Mira Studio · Oakland</span>
            <h1 className="hf-display" style={{ fontSize: 36, marginTop: 6, lineHeight: 1 }}>Persimmon <i>vase</i></h1>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 8 }}>
              <Stars n={5} size={12} />
              <span className="hf-small hf-muted">4.9 · 38 reviews</span>
            </div>
            <div className="hf-flex hf-items-baseline hf-gap-2" style={{ marginTop: 14 }}>
              <span className="hf-display hf-num" style={{ fontSize: 26 }}>{money(86)}</span>
              <span className="hf-small hf-muted">+ tax</span>
            </div>

            <div style={{ marginTop: 18 }}>
              <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 8 }}>
                <span className="hf-tiny hf-muted">COLOR · <span style={{ color: 'var(--ink)' }}>Persimmon</span></span>
                <span className="hf-tiny hf-muted">4 of 4</span>
              </div>
              <div className="hf-flex hf-gap-2">
                {['clay','rose','rust','bone'].map((t, i) => (
                  <div key={t} className={`hf-img hf-img-${t}`} style={{ width: 30, height: 30, borderRadius: 999, border: i === 0 ? '2px solid var(--ink)' : '1px solid var(--line-2)', boxShadow: i === 0 ? '0 0 0 2px var(--paper)' : 'none' }} />
                ))}
              </div>
            </div>

            <div style={{ marginTop: 14 }}>
              <div className="hf-tiny hf-muted" style={{ marginBottom: 8 }}>SIZE</div>
              <div className="hf-flex hf-gap-2">
                {[{l: 'Small', s: '18cm'}, {l: 'Medium', s: '26cm', on: true}, {l: 'Large', s: '34cm', dis: true}].map((o, i) => (
                  <div key={i} className={`hf-flex hf-col hf-items-center`} style={{ flex: 1, padding: '8px 0', borderRadius: 8, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line-2)', opacity: o.dis ? 0.4 : 1, background: o.on ? 'var(--paper-2)' : 'transparent', position: 'relative' }}>
                    <div style={{ fontSize: 12, fontWeight: 600 }}>{o.l}</div>
                    <div className="hf-tiny hf-muted">{o.s}</div>
                    {o.dis && <div className="hf-tiny" style={{ position: 'absolute', top: 4, right: 6, color: 'var(--ink-4)' }}>—</div>}
                  </div>
                ))}
              </div>
            </div>

            <div className="hf-flex hf-gap-2" style={{ marginTop: 18 }}>
              <div className="hf-flex hf-items-center" style={{ height: 44, padding: '0 6px', border: '1px solid var(--line-2)', borderRadius: 999 }}>
                <button className="hf-icon-btn"><Ico n="minus" s={13} /></button>
                <span className="hf-num" style={{ width: 18, textAlign: 'center', fontSize: 13, fontWeight: 600 }}>1</span>
                <button className="hf-icon-btn"><Ico n="plus" s={13} /></button>
              </div>
              <button className="hf-btn hf-btn-primary hf-btn-lg" style={{ flex: 1 }}>Add to bag · {money(86)}</button>
            </div>

            <div className="hf-flex hf-items-center hf-gap-2 hf-small hf-muted" style={{ marginTop: 12 }}>
              <Ico n="truck" s={13} />
              <span>Free local delivery · ships in 3–5 days</span>
            </div>
            <div className="hf-flex hf-items-center hf-gap-2 hf-small hf-muted" style={{ marginTop: 4 }}>
              <Ico n="refresh" s={13} />
              <span>14-day returns · made one at a time</span>
            </div>
          </div>
        </div>
      </div>
      <Anno top={170} right={400}>thumb rail · 5 photos</Anno>
      <Anno top={400} right={28}>variant · color &amp; size</Anno>
    </div>
  );
}

// ── B · Full-bleed + pinned card
function Product_FullBleed() {
  return (
    <div className="hf hf-desktop hf-relative">
      {/* Full-bleed image */}
      <div className="hf-img hf-img-shadow" style={{ position: 'absolute', inset: 0, borderRadius: 0 }} />
      <div style={{ position: 'absolute', inset: 0, background: 'linear-gradient(180deg, rgba(0,0,0,0.4), rgba(0,0,0,0.1) 30%, rgba(0,0,0,0.55))' }} />

      {/* Top nav (transparent) */}
      <div className="hf-relative" style={{ zIndex: 2 }}>
        <ShopperTopbar dark />
      </div>

      {/* Content layer */}
      <div className="hf-relative hf-grow hf-flex" style={{ zIndex: 2, padding: '28px 28px 22px', alignItems: 'flex-end' }}>
        <div className="hf-grow" style={{ paddingBottom: 8 }}>
          <span className="hf-eyebrow" style={{ color: 'rgba(255,255,255,0.7)' }}>Mira Studio</span>
          <h1 className="hf-display" style={{ fontSize: 60, color: 'white', lineHeight: 0.95, marginTop: 12 }}>Persimmon<br /><i>vase, Nº 04</i></h1>
          <p style={{ color: 'rgba(255,255,255,0.78)', fontSize: 14, marginTop: 14, maxWidth: 380 }}>
            Stoneware. Wood-fired in a small kiln in West Oakland. Each one is a slightly different orange — that's the whole point.
          </p>
          <div className="hf-flex hf-gap-1 hf-items-center" style={{ marginTop: 18 }}>
            {[1,2,3,4,5].map(i => <span key={i} style={{ width: 24, height: 4, borderRadius: 2, background: i === 1 ? 'white' : 'rgba(255,255,255,0.3)' }} />)}
            <span className="hf-tiny hf-num" style={{ color: 'rgba(255,255,255,0.7)', marginLeft: 8 }}>01 / 05</span>
          </div>
        </div>

        {/* Pinned product card */}
        <div className="hf-card" style={{ width: 360, padding: 22, borderRadius: 'var(--r-lg)', boxShadow: 'var(--shadow-3)', background: 'rgba(251,250,246,0.96)', backdropFilter: 'blur(20px)' }}>
          <div className="hf-flex hf-between hf-items-baseline">
            <span className="hf-tiny hf-muted">In stock · 4 left</span>
            <Stars n={5} size={11} />
          </div>
          <div className="hf-flex hf-items-baseline hf-between" style={{ marginTop: 10 }}>
            <span className="hf-h2">Persimmon vase</span>
            <span className="hf-display hf-num" style={{ fontSize: 26 }}>{money(86)}</span>
          </div>

          <div style={{ marginTop: 14 }}>
            <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>COLOR</div>
            <div className="hf-flex hf-gap-2">
              {['clay','rose','rust','bone'].map((t, i) => (
                <div key={t} className={`hf-img hf-img-${t}`} style={{ width: 28, height: 28, borderRadius: 999, border: i === 0 ? '2px solid var(--ink)' : '1px solid var(--line-2)' }} />
              ))}
            </div>
          </div>

          <div style={{ marginTop: 12 }}>
            <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>SIZE</div>
            <div className="hf-flex hf-gap-1">
              {['S', 'M', 'L'].map((s, i) => (
                <div key={s} style={{ flex: 1, height: 30, borderRadius: 8, border: i === 1 ? '1.5px solid var(--ink)' : '1px solid var(--line-2)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 12, fontWeight: 600, background: i === 1 ? 'var(--paper-2)' : 'transparent' }}>{s}</div>
              ))}
            </div>
          </div>

          <div className="hf-flex hf-gap-2" style={{ marginTop: 14 }}>
            <button className="hf-btn hf-btn-outline" style={{ width: 38 }}><Ico n="heart" s={14} /></button>
            <button className="hf-btn hf-btn-primary hf-btn-lg" style={{ flex: 1 }}>Add to bag</button>
          </div>
          <div className="hf-tiny hf-muted hf-flex hf-items-center hf-gap-2" style={{ marginTop: 10 }}>
            <Ico n="truck" s={11} /> Free delivery in SF/Oakland · 3–5 days
          </div>
        </div>
      </div>
      <Anno top={64} right={400} ><span style={{ color: 'white', background: 'var(--terra)', borderColor: 'var(--terra)' }} className="hf-anno-text">image as page</span></Anno>
      <Anno top={250} right={420}>pinned spec card</Anno>
    </div>
  );
}

// ── C · Vertical story — long-form scrolling product page
function Product_Story() {
  return (
    <div className="hf hf-desktop">
      <ShopperTopbar />
      <div className="hf-grow" style={{ overflow: 'hidden' }}>
        {/* Story header */}
        <div style={{ padding: '32px 28px 22px', textAlign: 'center', borderBottom: '1px solid var(--line)' }}>
          <span className="hf-eyebrow">Story · Field log Nº 12</span>
          <h1 className="hf-display" style={{ fontSize: 56, lineHeight: 1, marginTop: 14 }}>The making of a <i>persimmon vase.</i></h1>
          <p className="hf-body" style={{ fontSize: 14, marginTop: 14, maxWidth: 480, margin: '14px auto 0', color: 'var(--ink-2)' }}>
            From a sack of clay in February to a glazed vessel on your shelf — twelve weeks, four firings, one warm orange.
          </p>
        </div>

        {/* Three-up image story */}
        <div style={{ padding: '20px 28px', display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: 14 }}>
          {[
            { tone: 'rose', n: '01', t: 'Wedging the clay', s: 'Stoneware blend, 2.4kg' },
            { tone: 'clay', n: '02', t: 'On the wheel', s: '4 minutes per piece' },
            { tone: 'shadow', n: '03', t: 'Wood-firing', s: '8 hours, 1280°C' },
          ].map((s, i) => (
            <div key={i}>
              <div className={`hf-img hf-img-${s.tone}`} style={{ height: 160, borderRadius: 'var(--r-md)' }} />
              <div className="hf-flex hf-items-baseline hf-gap-2" style={{ marginTop: 10 }}>
                <span className="hf-display hf-num" style={{ fontSize: 28, color: 'var(--terra)' }}>{s.n}</span>
                <div>
                  <div className="hf-h4">{s.t}</div>
                  <div className="hf-tiny hf-muted">{s.s}</div>
                </div>
              </div>
            </div>
          ))}
        </div>

        {/* Sticky-feel bottom CTA bar */}
        <div className="hf-flex hf-items-center hf-between" style={{ margin: '0 28px', padding: '14px 18px', background: 'var(--ink)', color: 'var(--paper)', borderRadius: 'var(--r-lg)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <div className="hf-img hf-img-clay" style={{ width: 44, height: 44, borderRadius: 8 }} />
            <div>
              <div className="hf-h4" style={{ color: 'var(--paper)' }}>Persimmon vase · Medium</div>
              <div className="hf-tiny" style={{ color: 'rgba(255,255,255,0.6)' }}>Made one at a time · 4 left</div>
            </div>
          </div>
          <div className="hf-flex hf-items-center hf-gap-3">
            <span className="hf-display hf-num" style={{ fontSize: 24, color: 'var(--paper)' }}>{money(86)}</span>
            <button className="hf-btn hf-btn-lg" style={{ background: 'var(--paper)', color: 'var(--ink)' }}>Add to bag</button>
          </div>
        </div>
        <div className="hf-flex hf-between hf-px-6" style={{ marginTop: 14, paddingBottom: 14 }}>
          <span className="hf-small hf-muted">Scroll for the full essay →</span>
          <span className="hf-small hf-muted">14 min read</span>
        </div>
      </div>
      <Anno top={86} left={28}>centered editorial header</Anno>
      <Anno top={400} right={28}>persistent buy bar</Anno>
    </div>
  );
}

Object.assign(window, { Product_Classic, Product_FullBleed, Product_Story });

// hifi-flow-seller.jsx — End-to-end seller journey for Mira Studio
// 8 steps + 4 in-flow states. Each artboard 1280×800 (one taller for the editor).

// ──────────────────────────────────────────────────────────────────────
// Shared marketing chrome (admin area uses SellerSidebar + SellerTopbar
// from hifi-seller.jsx; the marketing-side pages use this top nav.)
// ──────────────────────────────────────────────────────────────────────
function MarketingTop({ active = '' }) {
  return (
    <div className="hf-flex hf-items-center" style={{ padding: '14px 32px', borderBottom: '1px solid var(--line)', gap: 28, background: 'var(--paper)' }}>
      <span className="hf-logo" style={{ fontSize: 20 }}>micro.</span>
      <div className="hf-flex hf-gap-5">
        {['Discover', 'Shops', 'Journal', 'For makers'].map((l) => (
          <a key={l} style={{ fontSize: 12.5, color: l === active ? 'var(--ink)' : 'var(--ink-3)', fontWeight: 500 }}>{l}</a>
        ))}
      </div>
      <div className="hf-grow" />
      <button className="hf-btn hf-btn-ghost hf-btn-sm">Sign in</button>
      <button className="hf-btn hf-btn-primary hf-btn-sm">Sell on Micro</button>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 01 · APPLY — Mira lands on the "Sell on Micro" page and starts claiming
// ──────────────────────────────────────────────────────────────────────
function SFlow_01_Apply() {
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="micro.shop/sell" title="Sell on Micro" />
      <MarketingTop active="For makers" />
      <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
        {/* Left — pitch */}
        <div style={{ flex: '1.1', padding: '60px 56px 40px', display: 'flex', flexDirection: 'column', justifyContent: 'center' }}>
          <div className="hf-eyebrow" style={{ color: 'var(--terra)', marginBottom: 12 }}>For makers · 4% per sale, no monthly fee</div>
          <h1 className="hf-display" style={{ fontSize: 64, lineHeight: 0.95, letterSpacing: '-0.02em', maxWidth: 520 }}>
            Open a shop in <i>about ten minutes.</i>
          </h1>
          <p className="hf-body hf-muted" style={{ marginTop: 14, maxWidth: 440, fontSize: 14.5, lineHeight: 1.55 }}>
            Bring your goods. We bring the storefront, payments, and a soft-spoken little community of buyers who want to know who made the thing.
          </p>
          <div className="hf-flex hf-gap-6" style={{ marginTop: 36 }}>
            {[
              { v: '4 800', l: 'active makers' },
              { v: '$2.1M', l: 'paid out · April' },
              { v: '12 min', l: 'avg. setup' },
            ].map((s) => (
              <div key={s.l} style={{ borderTop: '1px solid var(--ink)', paddingTop: 8, minWidth: 110 }}>
                <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1 }}>{s.v}</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{s.l}</div>
              </div>
            ))}
          </div>
        </div>

        {/* Right — claim card */}
        <div style={{ flex: '1', background: 'var(--paper-2)', padding: '56px 56px 40px', display: 'flex', alignItems: 'center', borderLeft: '1px solid var(--line)' }}>
          <div className="hf-card" style={{ padding: 28, width: '100%', maxWidth: 420, background: 'var(--paper)' }}>
            <div className="hf-h3" style={{ marginBottom: 4 }}>Claim your shop name</div>
            <div className="hf-small hf-muted" style={{ marginBottom: 18 }}>You can change this later. We'll spin up a free .micro.shop URL too.</div>

            <label className="hf-tiny hf-muted" style={{ letterSpacing: 0.04, textTransform: 'uppercase' }}>Shop name</label>
            <div className="hf-flex hf-items-center" style={{ height: 44, padding: '0 14px', border: '1.5px solid var(--ink)', borderRadius: 10, marginTop: 6 }}>
              <span className="hf-h4" style={{ fontSize: 16 }}>Mira Studio</span>
              <span style={{ width: 1.5, height: 18, background: 'var(--ink)', marginLeft: 2, animation: 'hf-blink 1s steps(1) infinite' }} />
            </div>

            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 10 }}>
              <span style={{ width: 14, height: 14, borderRadius: 999, background: 'var(--good)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white' }}>
                <Ico n="check" s={9} sw={2.4} />
              </span>
              <span className="hf-small" style={{ color: 'var(--good)', fontWeight: 500 }}>mira-studio.micro.shop</span>
              <span className="hf-tiny hf-muted">is available</span>
            </div>

            <label className="hf-tiny hf-muted" style={{ letterSpacing: 0.04, textTransform: 'uppercase', display: 'block', marginTop: 18 }}>What you make</label>
            <div className="hf-flex hf-gap-1" style={{ marginTop: 8, flexWrap: 'wrap' }}>
              {['Ceramics', 'Bakery', 'Textiles', 'Jewelry', 'Vintage', 'Other'].map((c, i) => (
                <span key={c} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11.5 }}>{c}</span>
              ))}
            </div>

            <button className="hf-btn hf-btn-primary" style={{ width: '100%', marginTop: 22, height: 44, fontSize: 14 }}>
              Continue · 6 steps left <Ico n="arrowR" s={12} />
            </button>
            <div className="hf-tiny hf-muted" style={{ textAlign: 'center', marginTop: 12 }}>
              By continuing you agree to our maker terms · No card needed
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>continues to shop setup</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · SETUP — Onboarding, step 2 of 6 (location + payouts)
// ──────────────────────────────────────────────────────────────────────
function SFlow_02_Setup() {
  const steps = ['Shop name', 'Location & payouts', 'Brand', 'First listing', 'Shipping', 'Review'];
  return (
    <div className="hf hf-desktop">
      <BrowserChrome url="micro.shop/sell/onboard?step=2" />
      <div className="hf-grow hf-flex" style={{ overflow: 'hidden', background: 'var(--paper-2)' }}>
        {/* Left rail · steps */}
        <div style={{ width: 280, padding: '40px 28px', borderRight: '1px solid var(--line)', background: 'var(--paper)' }}>
          <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 28 }}>
            <span className="hf-logo" style={{ fontSize: 18 }}>micro.</span>
            <span className="hf-tiny hf-muted" style={{ marginLeft: 4 }}>Open a shop</span>
          </div>
          <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Step 2 of 6</div>
          <div className="hf-progress" style={{ marginBottom: 22 }}><i style={{ width: '33%' }} /></div>
          <div className="hf-col hf-gap-3">
            {steps.map((s, i) => (
              <div key={s} className="hf-flex hf-items-center hf-gap-2">
                <span className="hf-center" style={{
                  width: 22, height: 22, borderRadius: 999, fontSize: 11, fontWeight: 600,
                  background: i < 1 ? 'var(--good)' : i === 1 ? 'var(--ink)' : 'var(--paper-2)',
                  color: i <= 1 ? 'white' : 'var(--ink-3)',
                  border: i > 1 ? '1px solid var(--line)' : 'none',
                }}>
                  {i < 1 ? <Ico n="check" s={11} sw={2.4} /> : i + 1}
                </span>
                <span className="hf-small" style={{ color: i === 1 ? 'var(--ink)' : 'var(--ink-3)', fontWeight: i === 1 ? 600 : 400 }}>{s}</span>
              </div>
            ))}
          </div>
          <div className="hf-card" style={{ padding: 14, marginTop: 32, background: 'var(--paper-2)', border: 'none' }}>
            <div className="hf-tiny" style={{ color: 'var(--terra)', fontWeight: 600, letterSpacing: 0.06, textTransform: 'uppercase', marginBottom: 4 }}>Tip</div>
            <div className="hf-small hf-muted">Add payouts last if you'd like — you can launch in draft and finish this when an order comes in.</div>
          </div>
        </div>

        {/* Center · form */}
        <div className="hf-grow" style={{ overflow: 'auto', padding: '60px 56px' }}>
          <div style={{ maxWidth: 620 }}>
            <div className="hf-eyebrow" style={{ marginBottom: 8 }}>Step 2 · Location &amp; payouts</div>
            <h1 className="hf-display" style={{ fontSize: 40, lineHeight: 1, letterSpacing: '-0.02em' }}>Where are you shipping from, and where should we send the money?</h1>

            <div className="hf-card" style={{ padding: 24, marginTop: 28, background: 'var(--paper)' }}>
              <div className="hf-h4" style={{ marginBottom: 4 }}>Studio location</div>
              <div className="hf-small hf-muted" style={{ marginBottom: 14 }}>Customers see only your city &amp; state.</div>
              <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '2fr 1fr' }}>
                <div>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Address line 1</div>
                  <div style={{ height: 38, border: '1px solid var(--line)', borderRadius: 8, padding: '0 12px', display: 'flex', alignItems: 'center' }}>
                    <span className="hf-small">410 Linden St</span>
                  </div>
                </div>
                <div>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>City</div>
                  <div style={{ height: 38, border: '1px solid var(--line)', borderRadius: 8, padding: '0 12px', display: 'flex', alignItems: 'center' }}>
                    <span className="hf-small">Oakland</span>
                  </div>
                </div>
              </div>
            </div>

            <div className="hf-card" style={{ padding: 24, marginTop: 18, background: 'var(--paper)' }}>
              <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-h4" style={{ marginBottom: 2 }}>Where to send your payouts</div>
                  <div className="hf-small hf-muted">Pick one — you can add more later.</div>
                </div>
                <span className="hf-chip hf-chip-soft" style={{ fontSize: 11 }}><Ico n="lock" s={10} /> 256-bit · Stripe</span>
              </div>
              <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr 1fr' }}>
                {[
                  { l: 'Bank account', s: 'ACH · 1–2 days', on: true },
                  { l: 'Debit card', s: 'Instant · 1.5%', on: false },
                  { l: 'Add later', s: 'Launch in draft', on: false },
                ].map((o) => (
                  <div key={o.l} className="hf-card" style={{
                    padding: 14, borderColor: o.on ? 'var(--ink)' : 'var(--line)', borderWidth: o.on ? 1.5 : 1,
                    background: o.on ? 'var(--paper-2)' : 'var(--paper)',
                  }}>
                    <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 6 }}>
                      <Ico n={o.l === 'Debit card' ? 'card' : o.l === 'Bank account' ? 'shield' : 'clock'} s={14} />
                      {o.on && <span className="hf-center" style={{ width: 18, height: 18, borderRadius: 999, background: 'var(--ink)', color: 'white' }}><Ico n="check" s={10} sw={2.4} /></span>}
                    </div>
                    <div className="hf-h4" style={{ fontSize: 13 }}>{o.l}</div>
                    <div className="hf-tiny hf-muted">{o.s}</div>
                  </div>
                ))}
              </div>
            </div>

            <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 28 }}>
              <button className="hf-btn hf-btn-ghost">← Back</button>
              <button className="hf-btn hf-btn-primary" style={{ height: 44, padding: '0 22px' }}>Continue · Brand <Ico n="arrowR" s={12} /></button>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>completes setup → opens listings editor</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · FIRST LISTING — Editor with one product, photos uploading
// ──────────────────────────────────────────────────────────────────────
function SFlow_03_FirstListing() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="New listing"
          subtitle="Listings · Drafts · 1 of 1"
          actions={<><button className="hf-btn hf-btn-outline">Save draft</button><button className="hf-btn hf-btn-primary"><Ico n="check" s={12} /> Publish</button></>}
        />
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr' }}>
            {/* Left · photos + basics */}
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 12 }}>Photos · 3 of 6</div>
                <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                  <ProdImg tone="clay"  h={120} r={8} />
                  <ProdImg tone="rose"  h={120} r={8} />
                  <ProdImg tone="cream" h={120} r={8} label="3 / 6" />
                  {/* uploading placeholder */}
                  <div style={{ height: 120, borderRadius: 8, border: '1.5px dashed var(--ink-4)', background: 'var(--paper-2)', display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 6 }}>
                    <div className="hf-progress" style={{ width: '60%' }}><i style={{ width: '72%' }} /></div>
                    <span className="hf-tiny hf-muted">vase-04.heic · 1.4mb</span>
                  </div>
                  <div style={{ height: 120, borderRadius: 8, border: '1.5px dashed var(--ink-4)', background: 'var(--paper-2)', display: 'flex', alignItems: 'center', justifyContent: 'center', flexDirection: 'column', gap: 6, color: 'var(--ink-3)' }}>
                    <Ico n="upload" s={18} />
                    <span className="hf-tiny">Drag to add</span>
                  </div>
                  <div style={{ height: 120, borderRadius: 8, background: 'var(--paper-2)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--ink-4)' }}>
                    <span className="hf-tiny">+</span>
                  </div>
                </div>
              </div>

              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 12 }}>Title &amp; description</div>
                <div style={{ height: 44, border: '1.5px solid var(--ink)', borderRadius: 8, padding: '0 14px', display: 'flex', alignItems: 'center', marginBottom: 10 }}>
                  <span className="hf-h4" style={{ fontSize: 15 }}>Persimmon vase</span>
                </div>
                <div style={{ minHeight: 110, border: '1px solid var(--line)', borderRadius: 8, padding: 14 }}>
                  <p className="hf-small" style={{ lineHeight: 1.55 }}>
                    Hand-thrown stoneware with a soft persimmon glaze. Each piece is a little different — the rim<br />
                    has a slight asymmetry I happen to like. Best for short-stem flowers or
                  </p>
                  <span className="hf-tiny hf-muted">|</span>
                </div>
                <div className="hf-flex hf-between" style={{ marginTop: 8 }}>
                  <div className="hf-flex hf-gap-1 hf-tiny hf-muted">
                    <Ico n="info" s={11} /> Markdown OK
                  </div>
                  <span className="hf-tiny hf-muted">142 / 800</span>
                </div>
              </div>
            </div>

            {/* Right · price, inventory, category */}
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 12 }}>Price &amp; stock</div>
                <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr' }}>
                  <div>
                    <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Price</div>
                    <div style={{ height: 44, border: '1px solid var(--line)', borderRadius: 8, padding: '0 12px', display: 'flex', alignItems: 'center', gap: 6 }}>
                      <span className="hf-muted hf-num">$</span>
                      <span className="hf-h4 hf-num" style={{ fontSize: 16 }}>86.00</span>
                    </div>
                  </div>
                  <div>
                    <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Stock</div>
                    <div style={{ height: 44, border: '1px solid var(--line)', borderRadius: 8, padding: '0 12px', display: 'flex', alignItems: 'center' }}>
                      <span className="hf-h4 hf-num" style={{ fontSize: 16 }}>12</span>
                    </div>
                  </div>
                </div>
                <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 14, padding: '8px 10px', background: 'var(--paper-2)', borderRadius: 8 }}>
                  <span className="hf-dot hf-dot-good" />
                  <span className="hf-small">Suggested: $78–$94 based on 6 similar shops</span>
                </div>
              </div>

              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 12 }}>Category &amp; tags</div>
                <div className="hf-tiny hf-muted">Category</div>
                <div className="hf-flex hf-items-center hf-between" style={{ height: 38, border: '1px solid var(--line)', borderRadius: 8, padding: '0 12px', marginTop: 4 }}>
                  <span className="hf-small">Ceramics · <span className="hf-muted">Vessels</span></span>
                  <Ico n="chevD" s={11} />
                </div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 12 }}>Tags</div>
                <div className="hf-flex hf-gap-1" style={{ marginTop: 6, flexWrap: 'wrap' }}>
                  {['hand-thrown', 'stoneware', 'persimmon', 'small batch'].map(t => (
                    <span key={t} className="hf-chip hf-chip-soft" style={{ fontSize: 11 }}>{t} <span style={{ marginLeft: 4, opacity: 0.5 }}>×</span></span>
                  ))}
                  <span className="hf-chip" style={{ fontSize: 11, color: 'var(--ink-3)' }}>+ add</span>
                </div>
              </div>

              <div className="hf-card" style={{ padding: 18, background: 'var(--paper-2)', border: 'none' }}>
                <div className="hf-flex hf-between" style={{ marginBottom: 8 }}>
                  <span className="hf-h4">Listing health</span>
                  <span className="hf-num hf-h4" style={{ color: 'var(--good)' }}>92</span>
                </div>
                <div className="hf-progress" style={{ marginBottom: 10 }}><i style={{ width: '92%', background: 'var(--good)' }} /></div>
                <div className="hf-tiny hf-muted">3 photos · clear title · price set · description over 100 chars. Add 1 more photo to reach 100.</div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>publishes Persimmon vase · returns to dashboard</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · DAY ONE — Empty dashboard with launch checklist
// ──────────────────────────────────────────────────────────────────────
function SFlow_04_DayOne() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Welcome, Mira"
          subtitle="Day 1 · Tuesday, March 12"
          actions={<><button className="hf-btn hf-btn-outline"><Ico n="share" s={12} /> Share shop</button><button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New listing</button></>}
        />
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* hero strip · launch banner */}
          <div className="hf-card" style={{ padding: 24, background: 'var(--ink)', color: 'var(--paper)', border: 'none', marginBottom: 20 }}>
            <div className="hf-flex hf-between hf-items-end">
              <div>
                <div className="hf-tiny" style={{ opacity: 0.6, letterSpacing: 0.08, textTransform: 'uppercase' }}>Your shop is live</div>
                <h2 className="hf-display" style={{ fontSize: 32, lineHeight: 1, marginTop: 6, color: 'var(--paper)' }}>mira-studio.micro.shop</h2>
                <div className="hf-small" style={{ opacity: 0.7, marginTop: 4 }}>Tell people. The first sale is usually a friend.</div>
              </div>
              <div className="hf-flex hf-gap-2">
                <button className="hf-btn hf-btn-outline" style={{ borderColor: 'rgba(255,255,255,0.3)', color: 'var(--paper)' }}>Copy link</button>
                <button className="hf-btn" style={{ background: 'var(--paper)', color: 'var(--ink)' }}>View shop →</button>
              </div>
            </div>
          </div>

          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr' }}>
            {/* Empty stat row */}
            <div className="hf-col hf-gap-3">
              <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                {[
                  { l: 'Sales', v: money(0) },
                  { l: 'Orders', v: '0' },
                  { l: 'Visits', v: '14', s: 'mostly you 😉' },
                ].map((s) => (
                  <div key={s.l} className="hf-card" style={{ padding: 16 }}>
                    <div className="hf-tiny hf-muted">{s.l} · today</div>
                    <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1, marginTop: 6, color: s.v === '0' || s.v === '$0.00' ? 'var(--ink-3)' : 'var(--ink)' }}>{s.v}</div>
                    <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>{s.s || 'no activity yet'}</div>
                  </div>
                ))}
              </div>

              <div className="hf-card" style={{ padding: 24, background: 'var(--paper-2)', border: 'none', minHeight: 240, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', textAlign: 'center' }}>
                <div className="hf-center" style={{ width: 56, height: 56, borderRadius: 999, background: 'var(--paper)', border: '1px solid var(--line)', marginBottom: 14, color: 'var(--ink-3)' }}>
                  <Ico n="inbox" s={22} />
                </div>
                <div className="hf-h3" style={{ marginBottom: 4 }}>Your first order will land here</div>
                <div className="hf-small hf-muted" style={{ maxWidth: 360 }}>We'll email you the moment it does. Until then, the launch list on the right will keep you busy.</div>
              </div>
            </div>

            {/* Launch checklist */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-flex hf-between" style={{ marginBottom: 6 }}>
                <span className="hf-h3">Launch checklist</span>
                <span className="hf-tiny hf-muted hf-num">3 / 6</span>
              </div>
              <div className="hf-progress" style={{ marginBottom: 16 }}><i style={{ width: '50%' }} /></div>
              <div className="hf-col hf-gap-3">
                {[
                  { l: 'Claim shop name', s: 'mira-studio · 2 days ago', done: true },
                  { l: 'Add payout method', s: 'Bank · ending 4421', done: true },
                  { l: 'Publish first listing', s: 'Persimmon vase · just now', done: true },
                  { l: 'Add 2 more listings', s: 'Most shops launch with 5+', done: false, hint: 'Recommended' },
                  { l: 'Set shipping rates', s: 'US · Intl · Local pickup', done: false },
                  { l: 'Share with 3 friends', s: 'Average shop gets first sale in 4 days', done: false },
                ].map((c, i) => (
                  <div key={i} className="hf-flex hf-items-start hf-gap-3">
                    <span className="hf-center" style={{
                      width: 22, height: 22, borderRadius: 999, marginTop: 1, flexShrink: 0,
                      background: c.done ? 'var(--good)' : 'var(--paper)',
                      border: c.done ? 'none' : '1.5px solid var(--ink-4)',
                      color: 'white',
                    }}>
                      {c.done && <Ico n="check" s={11} sw={2.4} />}
                    </span>
                    <div className="hf-grow">
                      <div className="hf-flex hf-items-center hf-gap-2">
                        <span className="hf-h4" style={{ fontSize: 13.5, textDecoration: c.done ? 'line-through' : 'none', color: c.done ? 'var(--ink-3)' : 'var(--ink)' }}>{c.l}</span>
                        {c.hint && <span className="hf-chip hf-chip-warn" style={{ fontSize: 10 }}>{c.hint}</span>}
                      </div>
                      <div className="hf-tiny hf-muted">{c.s}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>shares shop link · waits</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05 · FIRST ORDER — Celebration moment, banner + new order row
// ──────────────────────────────────────────────────────────────────────
function SFlow_05_FirstOrder() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden', position: 'relative' }}>
        <SellerTopbar
          title="Good afternoon, Mira"
          subtitle="Day 4 · Friday, March 15"
          actions={<><button className="hf-icon-btn" style={{ position: 'relative' }}><Ico n="bell" s={15} /><span style={{ position: 'absolute', top: 6, right: 7, width: 8, height: 8, borderRadius: 999, background: 'var(--terra)', border: '2px solid var(--paper)' }} /></button><Avatar name="Mira" size="sm" /></>}
        />
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* celebration banner */}
          <div className="hf-card" style={{ padding: 22, background: 'var(--primary)', color: 'white', border: 'none', marginBottom: 20, position: 'relative', overflow: 'hidden' }}>
            <div style={{ position: 'absolute', right: -20, top: -20, width: 180, height: 180, borderRadius: '50%', background: 'rgba(255,255,255,0.08)' }} />
            <div style={{ position: 'absolute', right: 80, bottom: -40, width: 110, height: 110, borderRadius: '50%', background: 'rgba(255,255,255,0.06)' }} />
            <div className="hf-flex hf-between hf-items-end" style={{ position: 'relative' }}>
              <div>
                <div className="hf-tiny" style={{ opacity: 0.85, letterSpacing: 0.08, textTransform: 'uppercase' }}>★ Your first order</div>
                <h2 className="hf-display" style={{ fontSize: 36, lineHeight: 1, marginTop: 6, color: 'white' }}>Sasha bought a Persimmon vase.</h2>
                <div className="hf-body" style={{ opacity: 0.9, marginTop: 6 }}>$86.00 · placed 12 minutes ago · we held it for you to confirm.</div>
              </div>
              <div className="hf-flex hf-gap-2">
                <button className="hf-btn" style={{ background: 'rgba(255,255,255,0.18)', color: 'white', border: '1px solid rgba(255,255,255,0.3)' }}>Send a thank-you</button>
                <button className="hf-btn" style={{ background: 'white', color: 'var(--terra)' }}>Open order →</button>
              </div>
            </div>
          </div>

          {/* stat row */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 20 }}>
            {[
              { l: 'Sales · today', v: money(86), d: 'first sale!', t: 'up', spark: [0,0,0,0,0,0,1] },
              { l: 'Orders · today', v: '1', d: 'new', t: 'up', spark: [0,0,0,0,0,0,1] },
              { l: 'Visits · today', v: '142', d: '8× yesterday', t: 'up', spark: [0.1,0.15,0.1,0.2,0.3,0.6,0.95] },
              { l: 'Followers', v: '14', d: '+5 today', t: 'up', spark: [0.1,0.2,0.2,0.4,0.45,0.7,0.95] },
            ].map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>{s.l}</div>
                <div className="hf-flex hf-between hf-items-end">
                  <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1 }}>{s.v}</div>
                  <span className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600 }}>↑ {s.d}</span>
                </div>
                <div style={{ height: 32, marginTop: 10, color: 'var(--primary)' }}>
                  <Spark data={s.spark} color="var(--primary)" fill="rgba(0,102,204,0.1)" />
                </div>
              </div>
            ))}
          </div>

          {/* Orders table — single new order */}
          <div className="hf-card" style={{ padding: 0 }}>
            <div className="hf-flex hf-between" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
              <div className="hf-h3">Orders</div>
              <a className="hf-small" style={{ color: 'var(--ink-2)', fontWeight: 500 }}>All orders →</a>
            </div>
            <table className="hf-table">
              <thead><tr><th>Order</th><th>Customer</th><th>Items</th><th>Total</th><th>Status</th><th></th></tr></thead>
              <tbody>
                <tr style={{ background: 'rgba(194,65,12,0.05)' }}>
                  <td className="hf-mono" style={{ color: 'var(--ink)', fontWeight: 600 }}>
                    <span className="hf-flex hf-items-center hf-gap-2">
                      <span className="hf-dot hf-dot-warn" style={{ background: 'var(--terra)' }} />#1001
                    </span>
                  </td>
                  <td><div className="hf-flex hf-items-center hf-gap-2"><Avatar name="Sasha L" size="sm" /><span>Sasha L.</span></div></td>
                  <td className="hf-muted">Persimmon vase</td>
                  <td className="hf-num" style={{ color: 'var(--ink)', fontWeight: 600 }}>{money(86)}</td>
                  <td><span className="hf-chip hf-chip-warn" style={{ fontSize: 11 }}>New · pack today</span></td>
                  <td><Ico n="chevR" s={13} /></td>
                </tr>
              </tbody>
            </table>
            <div className="hf-flex hf-items-center hf-gap-2 hf-tiny hf-muted" style={{ padding: '14px 20px', borderTop: '1px solid var(--line)' }}>
              <Ico n="info" s={11} /> Funds are released to your bank 2 days after the order ships.
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>opens order → starts packing</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 06 · PACK & SHIP — Order detail with print-label modal
// ──────────────────────────────────────────────────────────────────────
function SFlow_06_PackShip() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row', position: 'relative' }}>
      <SellerSidebar active="Orders" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 24px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <a className="hf-small hf-muted">Orders /</a>
            <span className="hf-mono" style={{ color: 'var(--ink-3)' }}>#1001</span>
            <span className="hf-chip hf-chip-warn" style={{ fontSize: 11 }}><span className="hf-dot hf-dot-warn" /> Needs shipping</span>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="chat" s={11} /> Message Sasha</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm">Print packing slip</button>
          </div>
        </div>
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px', filter: 'blur(0)' }}>
          <h2 className="hf-display" style={{ fontSize: 26, marginBottom: 14 }}>Sasha Leblanc · Persimmon vase</h2>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.6fr 1fr' }}>
            <div className="hf-card" style={{ padding: 0 }}>
              <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: '14px 18px' }}>
                <ProdImg tone="clay" h={60} r={8} />
                <div className="hf-grow">
                  <div className="hf-h4">Persimmon vase</div>
                  <div className="hf-tiny hf-muted">Glazed terra · qty 1</div>
                </div>
                <span className="hf-num hf-h4">{money(86)}</span>
              </div>
              <div className="hf-divider" />
              <div className="hf-col hf-gap-2" style={{ padding: '14px 18px' }}>
                <div className="hf-flex hf-between hf-small"><span className="hf-muted">Subtotal</span><span className="hf-num">{money(86)}</span></div>
                <div className="hf-flex hf-between hf-small"><span className="hf-muted">Shipping · USPS Ground</span><span className="hf-num">{money(0)}</span></div>
                <div className="hf-flex hf-between"><span className="hf-h4">Customer paid</span><span className="hf-display-2 hf-num" style={{ fontSize: 18 }}>{money(86)}</span></div>
                <div className="hf-flex hf-between hf-small" style={{ marginTop: 6, paddingTop: 8, borderTop: '1px dashed var(--line)' }}>
                  <span className="hf-muted">Micro fee · 4%</span><span className="hf-num">−{money(3.44)}</span>
                </div>
                <div className="hf-flex hf-between hf-small">
                  <span className="hf-muted">You'll receive</span><span className="hf-num" style={{ color: 'var(--good)', fontWeight: 600 }}>{money(82.56)}</span>
                </div>
              </div>
            </div>
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 16 }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Ship to</div>
                <div className="hf-h4">Sasha Leblanc</div>
                <div className="hf-small hf-muted" style={{ marginTop: 2 }}>820 Sutter St · #4B<br />San Francisco, CA 94109</div>
              </div>
              <div className="hf-card" style={{ padding: 16, background: 'var(--paper-2)', border: 'none' }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Customer note</div>
                <div className="hf-small">"So excited — please pack carefully, this is for my mom."</div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Modal · choose shipping label */}
      <div style={{ position: 'absolute', inset: 0, background: 'rgba(21,18,14,0.42)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <div className="hf-card" style={{ padding: 28, width: 540, background: 'var(--paper)' }}>
          <div className="hf-flex hf-between hf-items-start" style={{ marginBottom: 18 }}>
            <div>
              <div className="hf-eyebrow" style={{ marginBottom: 4 }}>Step 2 of 2</div>
              <div className="hf-h2" style={{ fontSize: 22 }}>Buy your shipping label</div>
            </div>
            <button className="hf-icon-btn"><Ico n="close" s={14} /></button>
          </div>
          <div className="hf-col hf-gap-2">
            {[
              { l: 'USPS Priority · 1–3 days', s: 'Tracked · $50 insured', p: 9.84, on: true },
              { l: 'USPS Ground Advantage', s: '2–5 days · tracked', p: 6.52, on: false },
              { l: 'UPS Ground', s: '3–4 days · pickup avail.', p: 11.20, on: false },
            ].map((o) => (
              <div key={o.l} className="hf-flex hf-items-center" style={{ padding: 14, borderRadius: 10, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: o.on ? 'var(--paper-2)' : 'var(--paper)', gap: 12 }}>
                <span className="hf-center" style={{ width: 18, height: 18, borderRadius: 999, border: '1.5px solid var(--ink)' }}>
                  {o.on && <span style={{ width: 9, height: 9, borderRadius: 999, background: 'var(--ink)' }} />}
                </span>
                <div className="hf-grow">
                  <div className="hf-h4" style={{ fontSize: 13.5 }}>{o.l}</div>
                  <div className="hf-tiny hf-muted">{o.s}</div>
                </div>
                <span className="hf-h4 hf-num">{money(o.p)}</span>
              </div>
            ))}
          </div>
          <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 18, padding: '12px 14px', background: 'var(--paper-2)', borderRadius: 8 }}>
            <span className="hf-small">Buy label · charge to payouts</span>
            <span className="hf-h4 hf-num">{money(9.84)}</span>
          </div>
          <button className="hf-btn hf-btn-primary" style={{ width: '100%', marginTop: 14, height: 44 }}>Buy &amp; print label →</button>
          <div className="hf-tiny hf-muted" style={{ textAlign: 'center', marginTop: 10 }}>Marks order shipped automatically when scanned</div>
        </div>
      </div>
      <Anno top={20} right={20}>label · print modal</Anno>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 07 · ANALYTICS — A month in. Story-mode insight.
// ──────────────────────────────────────────────────────────────────────
function SFlow_07_Analytics() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Your first month"
          subtitle="Analytics · March 12 → April 11"
          actions={<><button className="hf-btn hf-btn-outline hf-btn-sm">Compare</button><button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="upload" s={11} /> Export</button></>}
        />
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 20 }}>
            {[
              { l: 'Revenue', v: money(2148), d: '+ first month', spark: [0.05,0.1,0.2,0.25,0.4,0.55,0.7,0.85] },
              { l: 'Orders', v: '23', d: 'avg $93.39', spark: [0.05,0.1,0.2,0.3,0.45,0.6,0.7,0.9] },
              { l: 'Conversion', v: '3.1%', d: 'vs 2.4% benchmark', spark: [0.3,0.4,0.5,0.55,0.6,0.7,0.75,0.85] },
              { l: 'Repeat buyers', v: '4', d: '17% of orders', spark: [0,0,0.1,0.2,0.3,0.4,0.5,0.7] },
            ].map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>{s.l}</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1 }}>{s.v}</div>
                <div className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600, marginTop: 4 }}>↑ {s.d}</div>
                <div style={{ height: 30, marginTop: 8, color: 'var(--ink-3)' }}>
                  <Spark data={s.spark} color="var(--ink-3)" fill="rgba(21,18,14,0.05)" />
                </div>
              </div>
            ))}
          </div>

          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.6fr 1fr' }}>
            {/* Big chart */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-flex hf-between hf-items-start" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-tiny hf-muted">Daily revenue</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 24, marginTop: 2 }}>{money(2148)}</div>
                </div>
                <div className="hf-flex hf-gap-1">
                  {['Day', 'Week', 'Month'].map((p, i) => (
                    <span key={p} className={`hf-chip ${i === 2 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11 }}>{p}</span>
                  ))}
                </div>
              </div>
              <div style={{ height: 220, position: 'relative' }}>
                <AreaChart data={[0, 0, 0, 86, 0, 0, 0, 152, 78, 110, 0, 64, 230, 86, 110, 0, 178, 220, 64, 152, 86, 320, 110, 152, 64, 86, 230, 110, 320, 86]} color="var(--terra)" fill="rgba(194,65,12,0.1)" />
                {/* annotation pin on day 4 */}
                <div style={{ position: 'absolute', left: '11%', top: 16, padding: '6px 10px', background: 'var(--ink)', color: 'var(--paper)', borderRadius: 6, fontSize: 11, fontFamily: 'var(--sans)' }}>
                  Day 4 · first sale
                  <span style={{ position: 'absolute', left: 14, bottom: -4, width: 8, height: 8, background: 'var(--ink)', transform: 'rotate(45deg)' }} />
                </div>
              </div>
              <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
                <span>Mar 12</span><span>Mar 19</span><span>Mar 26</span><span>Apr 2</span><span>Apr 11</span>
              </div>
            </div>

            {/* Insight panel */}
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny" style={{ color: 'var(--terra)', fontWeight: 600, letterSpacing: 0.08, textTransform: 'uppercase', marginBottom: 8 }}>★ Insight</div>
                <div className="hf-h3" style={{ marginBottom: 6, lineHeight: 1.25 }}>Friday afternoons sell 2.3× more than the rest of the week.</div>
                <div className="hf-small hf-muted">Try posting new listings Thursday evening — they tend to land in feeds before the Friday rush.</div>
              </div>

              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 12 }}>Top sellers</div>
                <div className="hf-col hf-gap-3">
                  {[
                    { t: 'Persimmon vase', q: '14 sold', r: 1204, tone: 'clay' },
                    { t: 'Forest bowl, lg.', q: '6 sold', r: 384, tone: 'sage' },
                    { t: 'Cream tumbler set', q: '4 sold', r: 192, tone: 'cream' },
                  ].map((p) => (
                    <div key={p.t} className="hf-flex hf-items-center hf-gap-2">
                      <ProdImg tone={p.tone} h={36} r={6} />
                      <div className="hf-grow">
                        <div className="hf-h4" style={{ fontSize: 13 }}>{p.t}</div>
                        <div className="hf-tiny hf-muted">{p.q}</div>
                      </div>
                      <span className="hf-num hf-small" style={{ fontWeight: 500 }}>{money(p.r)}</span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>opens payouts page</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 08 · PAYOUT — money lands in the bank
// ──────────────────────────────────────────────────────────────────────
function SFlow_08_Payout() {
  const rows = [
    { d: 'Apr 8',  l: 'Payout · weekly',   s: 'Sent · ending 4421', amt: 1284.62, tone: 'good',  type: 'out' },
    { d: 'Apr 7',  l: 'Order #1042 · Sasha L.', s: 'Net of $3.44 fee', amt: 82.56, tone: 'mute', type: 'in' },
    { d: 'Apr 7',  l: 'Order #1041 · Devon T.', s: 'Net of $2.56 fee', amt: 61.44, tone: 'mute', type: 'in' },
    { d: 'Apr 6',  l: 'Shipping label · USPS', s: '#1041 · 1lb 4oz', amt: -6.52, tone: 'mute', type: 'out' },
    { d: 'Apr 5',  l: 'Order #1040 · Ari K.',   s: 'Net of $1.92 fee', amt: 46.08, tone: 'mute', type: 'in' },
    { d: 'Apr 1',  l: 'Payout · weekly',   s: 'Sent · ending 4421', amt: 624.18, tone: 'good',  type: 'out' },
  ];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Payouts"
          subtitle="Finance · all time"
          actions={<button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="upload" s={11} /> Statements</button>}
        />
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* big number — last payout */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr', marginBottom: 20 }}>
            <div className="hf-card" style={{ padding: 28 }}>
              <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Last payout · sent today</div>
              <div className="hf-flex hf-items-end hf-gap-3">
                <div className="hf-display hf-num" style={{ fontSize: 64, lineHeight: 0.95, letterSpacing: '-0.02em' }}>{money(1284.62)}</div>
                <span className="hf-chip hf-chip-good" style={{ fontSize: 11, marginBottom: 12 }}><span className="hf-dot hf-dot-good" /> Sent · arriving Wed</span>
              </div>
              <div className="hf-small hf-muted" style={{ marginTop: 14, maxWidth: 460 }}>
                Nine orders, less Micro's 4% and three shipping labels. We send payouts every Tuesday — you can switch to instant from Settings.
              </div>
              <div className="hf-flex hf-gap-2" style={{ marginTop: 18 }}>
                <button className="hf-btn hf-btn-outline hf-btn-sm">View receipt</button>
                <button className="hf-btn hf-btn-ghost hf-btn-sm">Switch to instant payouts</button>
              </div>
            </div>

            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted">Available · next payout</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1, marginTop: 6 }}>{money(184.08)}</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 6 }}>From 3 orders · sends Tue, Apr 15</div>
              </div>
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted">Lifetime earned</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1, marginTop: 6 }}>{money(2148.36)}</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 6 }}>23 orders · Mar 12 → Apr 11</div>
              </div>
            </div>
          </div>

          {/* Ledger */}
          <div className="hf-card" style={{ padding: 0 }}>
            <div className="hf-flex hf-between" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
              <div className="hf-h3">Activity</div>
              <div className="hf-flex hf-gap-1">
                {['All', 'Payouts', 'Sales', 'Fees'].map((c, i) => (
                  <span key={c} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11 }}>{c}</span>
                ))}
              </div>
            </div>
            <table className="hf-table">
              <thead><tr><th>Date</th><th>Description</th><th></th><th style={{ textAlign: 'right' }}>Amount</th></tr></thead>
              <tbody>
                {rows.map((r, i) => (
                  <tr key={i}>
                    <td className="hf-mono hf-muted">{r.d}</td>
                    <td>
                      <div className="hf-h4" style={{ fontSize: 13 }}>{r.l}</div>
                      <div className="hf-tiny hf-muted">{r.s}</div>
                    </td>
                    <td>{r.type === 'out' && r.amt > 0 ? <span className="hf-chip hf-chip-good" style={{ fontSize: 11 }}>Payout</span> : r.amt < 0 ? <span className="hf-chip hf-chip-soft" style={{ fontSize: 11 }}>Fee</span> : <span className="hf-chip hf-chip-soft" style={{ fontSize: 11 }}>Sale</span>}</td>
                    <td className="hf-num" style={{ textAlign: 'right', color: r.amt < 0 ? 'var(--ink-3)' : r.type === 'out' ? 'var(--ink)' : 'var(--good)', fontWeight: 600 }}>
                      {r.amt < 0 ? `−${money(Math.abs(r.amt))}` : r.type === 'out' ? `−${money(r.amt)}` : `+${money(r.amt)}`}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>finance · payouts</Anno>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// IN-FLOW STATES (4) — loading, empty, error, success
// ──────────────────────────────────────────────────────────────────────

// Loading: dashboard skeleton
function SState_Loading() {
  const Sk = ({ w = '60%', h = 14, r = 4 }) => <div style={{ width: w, height: h, borderRadius: r, background: 'linear-gradient(90deg, rgba(21,18,14,0.06) 0%, rgba(21,18,14,0.12) 50%, rgba(21,18,14,0.06) 100%)', backgroundSize: '200% 100%', animation: 'hf-shimmer 1.4s linear infinite' }} />;
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '20px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-col hf-gap-2">
            <Sk w={140} h={10} />
            <Sk w={220} h={26} />
          </div>
          <div className="hf-flex hf-gap-2"><Sk w={94} h={32} r={999} /><Sk w={120} h={32} r={999} /></div>
        </div>
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 20 }}>
            {[0, 1, 2, 3].map((i) => (
              <div key={i} className="hf-card hf-col hf-gap-2" style={{ padding: 18 }}>
                <Sk w="40%" h={10} />
                <Sk w="65%" h={28} />
                <Sk w="100%" h={36} />
              </div>
            ))}
          </div>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.6fr 1fr' }}>
            <div className="hf-card hf-col hf-gap-3" style={{ padding: 20 }}>
              <Sk w="20%" h={10} />
              <Sk w="35%" h={28} />
              <div style={{ height: 200, marginTop: 8 }}><Sk w="100%" h={200} r={8} /></div>
            </div>
            <div className="hf-card hf-col hf-gap-3" style={{ padding: 20 }}>
              <Sk w="40%" h={14} />
              {[0,1,2,3].map((i) => (
                <div key={i} className="hf-flex hf-items-start hf-gap-3" style={{ marginTop: 4 }}>
                  <Sk w={10} h={10} r={999} />
                  <div className="hf-col hf-gap-2 hf-grow">
                    <Sk w="80%" h={12} />
                    <Sk w="50%" h={9} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>loading · skeleton</Anno>
    </div>
  );
}

// Empty: orders inbox, no orders yet
function SState_EmptyOrders() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Orders" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 24px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <h2 className="hf-h2" style={{ fontSize: 18 }}>Orders</h2>
            <div className="hf-flex hf-gap-1">
              {['All · 0', 'New', 'Pack', 'Ship', 'Done'].map((c, i) => (
                <span key={c} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ color: i === 0 ? 'white' : 'var(--ink-3)' }}>{c}</span>
              ))}
            </div>
          </div>
          <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="filter" s={11} /> Filter</button>
        </div>
        <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
          <div style={{ width: 340, borderRight: '1px solid var(--line)', flexShrink: 0, padding: '40px 24px', textAlign: 'center', color: 'var(--ink-3)' }}>
            <Ico n="inbox" s={32} />
            <div className="hf-h4" style={{ marginTop: 14, color: 'var(--ink)' }}>No orders yet</div>
            <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>They'll show up here as soon as someone buys.</div>
          </div>
          <div className="hf-grow" style={{ overflow: 'auto', padding: '40px', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <div style={{ maxWidth: 460, textAlign: 'center' }}>
              <div className="hf-center" style={{ width: 84, height: 84, borderRadius: 999, background: 'var(--paper-2)', margin: '0 auto 18px', color: 'var(--ink-3)' }}>
                <Ico n="inbox" s={36} />
              </div>
              <h2 className="hf-display" style={{ fontSize: 32, lineHeight: 1, marginBottom: 8 }}>Quiet, isn't it.</h2>
              <p className="hf-body hf-muted" style={{ marginBottom: 22, fontSize: 14, maxWidth: 380, marginInline: 'auto' }}>
                Most shops get their first order within a week of sharing the link. While you wait, two things tend to help.
              </p>
              <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr', textAlign: 'left' }}>
                <div className="hf-card" style={{ padding: 14 }}>
                  <div className="hf-h4" style={{ fontSize: 13, marginBottom: 4 }}>Add 2 more listings</div>
                  <div className="hf-tiny hf-muted">Shops with 5+ items get found 3× more.</div>
                </div>
                <div className="hf-card" style={{ padding: 14 }}>
                  <div className="hf-h4" style={{ fontSize: 13, marginBottom: 4 }}>Share your link</div>
                  <div className="hf-tiny hf-muted">A short note to friends does most of the lifting.</div>
                </div>
              </div>
              <button className="hf-btn hf-btn-primary" style={{ marginTop: 20 }}>Copy mira-studio.micro.shop</button>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>empty · no orders</Anno>
    </div>
  );
}

// Error: payout connection failed banner + recovery
function SState_PayoutError() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar title="Good morning, Mira" subtitle="Tuesday · April 8" actions={<><button className="hf-icon-btn"><Ico n="bell" s={15} /></button><Avatar name="Mira" size="sm" /></>} />

        {/* Error banner */}
        <div style={{ background: '#FBE9E5', borderBottom: '1px solid #E8B5AB', padding: '14px 28px' }}>
          <div className="hf-flex hf-items-start hf-gap-3">
            <span className="hf-center" style={{ width: 28, height: 28, borderRadius: 999, background: 'var(--bad)', color: 'white', flexShrink: 0 }}>
              <Ico n="alert" s={14} sw={2.2} />
            </span>
            <div className="hf-grow">
              <div className="hf-h4" style={{ marginBottom: 2 }}>We couldn't send your Tuesday payout · {money(1284.62)} held</div>
              <div className="hf-small hf-muted">Your bank rejected the transfer (account ending 4421). This sometimes happens after an address change. Update the account and we'll retry within an hour.</div>
            </div>
            <div className="hf-flex hf-gap-2">
              <button className="hf-btn hf-btn-outline hf-btn-sm">View details</button>
              <button className="hf-btn hf-btn-primary hf-btn-sm">Update bank →</button>
            </div>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* held payout card */}
          <div className="hf-card" style={{ padding: 24, marginBottom: 20, borderColor: 'var(--bad)' }}>
            <div className="hf-flex hf-between hf-items-end">
              <div>
                <div className="hf-tiny" style={{ color: 'var(--bad)', fontWeight: 600, letterSpacing: 0.08, textTransform: 'uppercase', marginBottom: 6 }}>● Payout held</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 36, lineHeight: 1 }}>{money(1284.62)}</div>
                <div className="hf-small hf-muted" style={{ marginTop: 6 }}>9 orders · weekly batch · would have arrived Wed</div>
              </div>
              <div className="hf-flex hf-gap-2">
                <button className="hf-btn hf-btn-outline">Switch payout method</button>
                <button className="hf-btn hf-btn-primary">Retry payout</button>
              </div>
            </div>
            <div className="hf-divider" style={{ margin: '18px 0' }} />
            <div className="hf-flex hf-items-start hf-gap-3">
              <Ico n="info" s={14} />
              <div>
                <div className="hf-h4" style={{ fontSize: 13, marginBottom: 2 }}>What's happening</div>
                <div className="hf-small hf-muted">Stripe returned <span className="hf-mono" style={{ color: 'var(--ink)' }}>R03 · No account / unable to locate</span>. The funds are safe with us; nothing left your shop. Once you update the routing or account number, we'll auto-retry.</div>
              </div>
            </div>
          </div>

          {/* dim normal stat row, faded */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(3, 1fr)', opacity: 0.4 }}>
            {[
              { l: 'Revenue · 7 days', v: money(4280) },
              { l: 'Orders · 7 days',  v: '38' },
              { l: 'Avg. order',       v: money(112) },
            ].map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted">{s.l}</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 28, marginTop: 6 }}>{s.v}</div>
                <div style={{ height: 32, marginTop: 8 }}><Spark data={[0.4,0.5,0.45,0.6,0.55,0.7,0.85]} color="var(--ink-3)" fill="rgba(21,18,14,0.05)" /></div>
              </div>
            ))}
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>error · payout failed</Anno>
    </div>
  );
}

// Success: first sale celebration modal overlay
function SState_FirstSaleSuccess() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row', position: 'relative' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar title="Welcome, Mira" subtitle="Day 4 · Friday, March 15" actions={<Avatar name="Mira" size="sm" />} />
        {/* dim background dashboard */}
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px', filter: 'blur(2px)' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 20 }}>
            {[1, 2, 3, 4].map(i => (
              <div key={i} className="hf-card" style={{ padding: 18 }}>
                <div style={{ width: '40%', height: 10, background: 'var(--paper-2)', borderRadius: 4, marginBottom: 8 }} />
                <div style={{ width: '60%', height: 22, background: 'var(--paper-2)', borderRadius: 4 }} />
              </div>
            ))}
          </div>
          <div className="hf-card" style={{ padding: 20, height: 240 }}>
            <div style={{ width: '20%', height: 14, background: 'var(--paper-2)', borderRadius: 4, marginBottom: 16 }} />
            <div style={{ width: '100%', height: 180, background: 'var(--paper-2)', borderRadius: 8 }} />
          </div>
        </div>
      </div>

      {/* overlay */}
      <div style={{ position: 'absolute', inset: 0, background: 'rgba(21,18,14,0.5)', zIndex: 50, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
        <div className="hf-card" style={{ padding: 0, width: 480, background: 'var(--paper)', overflow: 'hidden', textAlign: 'center' }}>
          {/* hero band */}
          <div style={{ background: 'var(--primary)', color: 'white', padding: '28px 28px 22px', position: 'relative', overflow: 'hidden' }}>
            <div style={{ position: 'absolute', right: -20, top: -30, width: 140, height: 140, borderRadius: '50%', background: 'rgba(255,255,255,0.08)' }} />
            <div style={{ position: 'absolute', left: -10, bottom: -40, width: 90, height: 90, borderRadius: '50%', background: 'rgba(255,255,255,0.06)' }} />
            <div style={{ fontSize: 44, lineHeight: 1, position: 'relative' }}>★</div>
            <div className="hf-tiny" style={{ opacity: 0.85, letterSpacing: 0.1, textTransform: 'uppercase', marginTop: 12, position: 'relative' }}>Your first sale</div>
            <div className="hf-display" style={{ fontSize: 36, lineHeight: 1, marginTop: 6, color: 'white', position: 'relative' }}>It happened.</div>
          </div>

          <div style={{ padding: '24px 32px 28px' }}>
            <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: 14, background: 'var(--paper-2)', borderRadius: 10, textAlign: 'left' }}>
              <ProdImg tone="clay" h={56} r={8} />
              <div className="hf-grow">
                <div className="hf-h4">Persimmon vase</div>
                <div className="hf-tiny hf-muted">Sasha L. · San Francisco, CA</div>
              </div>
              <span className="hf-display-2 hf-num" style={{ fontSize: 22 }}>{money(86)}</span>
            </div>
            <p className="hf-body hf-muted" style={{ marginTop: 18, fontSize: 14, lineHeight: 1.55 }}>
              You'll receive {money(82.56)} after Micro's 4% fee. Pack &amp; ship in the next 3 days and the rating will follow.
            </p>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 20 }}>
              <button className="hf-btn hf-btn-outline" style={{ flex: 1 }}>Send a thank-you note</button>
              <button className="hf-btn hf-btn-primary" style={{ flex: 1 }}>Pack &amp; ship →</button>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>success · first sale</Anno>
    </div>
  );
}

Object.assign(window, {
  SFlow_01_Apply, SFlow_02_Setup, SFlow_03_FirstListing, SFlow_04_DayOne,
  SFlow_05_FirstOrder, SFlow_06_PackShip, SFlow_07_Analytics, SFlow_08_Payout,
  SState_Loading, SState_EmptyOrders, SState_PayoutError, SState_FirstSaleSuccess,
});

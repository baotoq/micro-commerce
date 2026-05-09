// hifi-seller-config.jsx — Seller config + finance detail + mobile seller
// Shop settings · Finance · Mobile seller (orders, order detail, listings, dashboard)

// ──────────────────────────────────────────────────────────────────────
// 01 · SHOP SETTINGS — shipping zones, taxes, policies, brand
// ──────────────────────────────────────────────────────────────────────
function SCfg_Settings() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Storefront" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Shop settings"
          subtitle="Storefront · last saved 2 minutes ago"
          actions={<><button className="hf-btn hf-btn-ghost hf-btn-sm">Discard</button><button className="hf-btn hf-btn-primary hf-btn-sm">Save changes</button></>}
        />

        <div className="hf-grow" style={{ overflow: 'hidden', display: 'flex' }}>
          {/* Sub-nav */}
          <div style={{ width: 220, padding: '20px 0', borderRight: '1px solid var(--line)', flexShrink: 0 }}>
            {[
              { l: 'Brand & profile', i: 'shop' },
              { l: 'Shipping zones',  i: 'truck', on: true },
              { l: 'Taxes',           i: 'card' },
              { l: 'Policies',        i: 'shield' },
              { l: 'Domain & SEO',    i: 'globe' },
              { l: 'Notifications',   i: 'bell' },
              { l: 'Team',            i: 'user' },
              { l: 'Apps & API',      i: 'cog' },
            ].map((s) => (
              <div key={s.l} className="hf-flex hf-items-center hf-gap-3" style={{
                padding: '10px 24px',
                background: s.on ? 'var(--paper-2)' : 'transparent',
                borderLeft: s.on ? '2px solid var(--ink)' : '2px solid transparent',
                color: s.on ? 'var(--ink)' : 'var(--ink-3)',
              }}>
                <Ico n={s.i} s={14} />
                <span className="hf-small" style={{ fontWeight: s.on ? 600 : 400 }}>{s.l}</span>
              </div>
            ))}
          </div>

          {/* Form */}
          <div className="hf-grow" style={{ overflow: 'auto', padding: '28px 32px' }}>
            <div style={{ maxWidth: 760 }}>
              <div className="hf-eyebrow" style={{ marginBottom: 6 }}>Storefront · Shipping</div>
              <h1 className="hf-display" style={{ fontSize: 36, lineHeight: 1, letterSpacing: '-0.02em' }}>Where do you ship, and how much?</h1>
              <p className="hf-small hf-muted" style={{ marginTop: 8, maxWidth: 540 }}>
                Group countries into zones with their own rates. We'll show buyers the right shipping price at checkout based on where they live.
              </p>

              {/* Zones */}
              <div className="hf-card" style={{ padding: 0, marginTop: 24, overflow: 'hidden' }}>
                {[
                  {
                    n: 'United States',
                    sub: '50 states · USPS, UPS, FedEx',
                    on: true,
                    rates: [
                      { l: 'Standard · 3–5 days',  p: 6.50, w: 'all weights' },
                      { l: 'Priority · 1–3 days',  p: 9.84, w: 'up to 5 lb' },
                      { l: 'Free shipping',        p: 0,    w: 'orders $80+', tag: 'promo' },
                    ],
                  },
                  {
                    n: 'Canada & Mexico',
                    sub: '2 countries · USPS Intl',
                    on: true,
                    rates: [
                      { l: 'USPS Intl · 6–10 days', p: 18.40, w: 'up to 4 lb' },
                    ],
                  },
                  {
                    n: 'Rest of world',
                    sub: 'Off — turn on if you ship elsewhere',
                    on: false,
                    rates: [],
                  },
                ].map((z, zi) => (
                  <div key={z.n} style={{ padding: 20, borderBottom: zi < 2 ? '1px solid var(--line)' : 'none', background: z.on ? 'var(--paper)' : 'var(--paper-2)' }}>
                    <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 10 }}>
                      <div>
                        <div className="hf-flex hf-items-center hf-gap-2">
                          <span className="hf-h3" style={{ fontSize: 16 }}>{z.n}</span>
                          {!z.on && <span className="hf-chip hf-chip-soft" style={{ fontSize: 10 }}>Off</span>}
                        </div>
                        <div className="hf-tiny hf-muted">{z.sub}</div>
                      </div>
                      <div className="hf-flex hf-gap-2">
                        {z.on && <button className="hf-btn hf-btn-ghost hf-btn-sm">Edit countries</button>}
                        <span className={`hf-switch ${z.on ? 'hf-switch-on' : ''}`} />
                      </div>
                    </div>
                    {z.on && (
                      <div className="hf-col hf-gap-2">
                        {z.rates.map((r, i) => (
                          <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '10px 12px', background: 'var(--paper-2)', borderRadius: 8 }}>
                            <Ico n="truck" s={13} />
                            <div className="hf-grow">
                              <div className="hf-flex hf-items-center hf-gap-2">
                                <span className="hf-h4" style={{ fontSize: 13 }}>{r.l}</span>
                                {r.tag === 'promo' && <span className="hf-chip hf-chip-good" style={{ fontSize: 10 }}>Promo</span>}
                              </div>
                              <span className="hf-tiny hf-muted">{r.w}</span>
                            </div>
                            <span className="hf-num hf-h4" style={{ fontSize: 13 }}>{r.p === 0 ? 'Free' : money(r.p)}</span>
                            <button className="hf-icon-btn" style={{ width: 24, height: 24 }}><Ico n="edit" s={11} /></button>
                          </div>
                        ))}
                        <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ alignSelf: 'flex-start' }}><Ico n="plus" s={11} /> Add rate</button>
                      </div>
                    )}
                  </div>
                ))}
              </div>

              {/* Tax */}
              <div className="hf-h3" style={{ marginTop: 36, marginBottom: 12 }}>Taxes</div>
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-flex hf-items-center hf-gap-3" style={{ marginBottom: 14 }}>
                  <span className="hf-switch hf-switch-on" />
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 13.5 }}>Auto-calculate US sales tax</div>
                    <div className="hf-tiny hf-muted">Based on buyer's address · maintained by Micro</div>
                  </div>
                  <span className="hf-chip hf-chip-good" style={{ fontSize: 10 }}>Active in 12 states</span>
                </div>
                <div className="hf-divider" style={{ marginBottom: 14 }} />
                <div className="hf-flex hf-items-center hf-gap-3">
                  <span className="hf-switch" />
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 13.5 }}>Charge tax on shipping</div>
                    <div className="hf-tiny hf-muted">Off · most makers leave this off for hand-made goods</div>
                  </div>
                </div>
              </div>

              {/* Policies */}
              <div className="hf-h3" style={{ marginTop: 36, marginBottom: 12 }}>Policies</div>
              <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr' }}>
                {[
                  { l: 'Returns',     s: 'Within 14 days · seller pays return shipping for damaged items', edited: 'Apr 6' },
                  { l: 'Cancellations', s: 'Until packed · automatic refund', edited: 'Mar 30' },
                  { l: 'Privacy',     s: 'Standard · drafted by Micro', edited: 'Default' },
                  { l: 'Terms of sale', s: 'Standard · drafted by Micro', edited: 'Default' },
                ].map((p) => (
                  <div key={p.l} className="hf-card" style={{ padding: 16 }}>
                    <div className="hf-flex hf-between" style={{ marginBottom: 6 }}>
                      <span className="hf-h4">{p.l}</span>
                      <span className="hf-tiny hf-muted">{p.edited}</span>
                    </div>
                    <p className="hf-small hf-muted" style={{ lineHeight: 1.5 }}>{p.s}</p>
                    <a className="hf-tiny" style={{ color: 'var(--primary)', marginTop: 8, display: 'inline-block' }}>Edit policy →</a>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>settings · shipping zones</Anno>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · FINANCE DETAIL — statements, fees, taxes
// ──────────────────────────────────────────────────────────────────────
function SCfg_Finance() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Finance"
          subtitle="Statements · taxes · fees"
          actions={<><button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="upload" s={11} /> Download · April 1099</button><button className="hf-btn hf-btn-outline hf-btn-sm">Settings</button></>}
        />

        {/* sub-tabs */}
        <div className="hf-flex hf-items-end" style={{ padding: '0 28px', borderBottom: '1px solid var(--line)', gap: 22 }}>
          {['Payouts', 'Statements', 'Fees', 'Taxes'].map((t, i) => (
            <div key={t} className={`hf-tab ${i === 1 ? 'hf-tab-on' : ''}`}>{t}</div>
          ))}
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* monthly statements */}
          <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 14 }}>
            <div>
              <div className="hf-h3">Monthly statements</div>
              <div className="hf-tiny hf-muted">Generated on the 1st · downloaded as PDF or CSV</div>
            </div>
            <div className="hf-flex hf-gap-1">
              <span className="hf-chip hf-chip-on" style={{ fontSize: 11 }}>2026</span>
              <span className="hf-chip" style={{ fontSize: 11 }}>2025</span>
            </div>
          </div>

          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(3, 1fr)', marginBottom: 22 }}>
            {[
              { m: 'April · in progress',  s: 'Mar 12 → today',     gross: 2148.36, fees: 85.94, ship: 24.36, tax: 168.42, net: 1869.64, status: 'open',     bars: [0.4,0.6,0.5,0.7,0.8,0.65,0.9,0.7,0.85] },
              { m: 'March',                 s: 'Mar 1 → Mar 31',     gross: 0,        fees: 0,    ship: 0,    tax: 0,      net: 0,        status: 'pre' },
              { m: 'February',              s: 'Pre-launch',         gross: 0,        fees: 0,    ship: 0,    tax: 0,      net: 0,        status: 'pre' },
            ].map((s, i) => (
              <div key={i} className="hf-card" style={{ padding: 18, opacity: s.status === 'pre' ? 0.55 : 1 }}>
                <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 6 }}>
                  <span className="hf-h4">{s.m}</span>
                  <span className={`hf-chip ${s.status === 'open' ? 'hf-chip-warn' : 'hf-chip-soft'}`} style={{ fontSize: 10 }}>{s.status === 'open' ? 'Open' : 'No activity'}</span>
                </div>
                <div className="hf-tiny hf-muted">{s.s}</div>
                {s.status === 'open' ? (
                  <>
                    <div className="hf-display-2 hf-num" style={{ fontSize: 24, marginTop: 14, lineHeight: 1 }}>{money(s.net)}</div>
                    <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>Net deposited · 23 orders</div>
                    <div style={{ height: 32, marginTop: 12, color: 'var(--good)' }}>
                      <Bars data={s.bars} color="var(--good)" />
                    </div>
                    <div className="hf-flex hf-gap-2" style={{ marginTop: 12 }}>
                      <button className="hf-btn hf-btn-outline hf-btn-sm hf-grow">Preview</button>
                      <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ width: 32, padding: 0 }}><Ico n="upload" s={11} /></button>
                    </div>
                  </>
                ) : (
                  <div className="hf-tiny hf-muted" style={{ marginTop: 22 }}>—</div>
                )}
              </div>
            ))}
          </div>

          {/* April detail breakdown */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr' }}>
            <div className="hf-card" style={{ padding: 22 }}>
              <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 18 }}>
                <div>
                  <div className="hf-h3">April · breakdown</div>
                  <div className="hf-tiny hf-muted">Gross to net · Mar 12 → today</div>
                </div>
                <span className="hf-tiny hf-muted">23 orders · 4 refunds</span>
              </div>

              {/* waterfall rows */}
              {[
                { l: 'Gross sales',           v: 2148.36, type: 'pos', big: true,  desc: '23 orders · avg $93.39' },
                { l: 'Refunds',                v: -88.00,  type: 'neg',             desc: '2 partial · 2 full' },
                { l: 'Discount codes used',    v: -42.00,  type: 'neg',             desc: 'STUDIO15 · 3 redemptions' },
                { l: 'Micro fee · 4%',         v: -85.94,  type: 'neg',             desc: 'On order subtotal · no fee on shipping' },
                { l: 'Payment processing',     v: -62.30,  type: 'neg',             desc: '2.9% + 30¢ · Stripe' },
                { l: 'Shipping labels',        v: -24.36,  type: 'neg',             desc: '4 USPS Priority · charged at print time' },
                { l: 'Sales tax collected',    v: 168.42,  type: 'hold',            desc: 'Held by Micro · remitted on your behalf' },
                { l: 'Net to bank',            v: 1869.64, type: 'pos', big: true,  desc: 'Sent in 4 weekly payouts · 1 pending' },
              ].map((r, i, a) => {
                const sign = r.v < 0 ? '−' : r.type === 'pos' ? '' : '';
                const c = r.type === 'pos' ? (r.big ? 'var(--good)' : 'var(--ink)') : r.type === 'neg' ? 'var(--ink-3)' : 'var(--ink-2)';
                const sep = i === a.length - 2;
                return (
                  <div key={r.l} className="hf-flex hf-between hf-items-center" style={{ padding: '10px 0', borderTop: i === 0 ? 'none' : sep ? '1.5px solid var(--ink)' : '1px solid var(--line)' }}>
                    <div>
                      <div className="hf-h4" style={{ fontSize: r.big ? 14 : 13 }}>{r.l}</div>
                      <div className="hf-tiny hf-muted">{r.desc}</div>
                    </div>
                    <span className="hf-num" style={{ fontSize: r.big ? 18 : 14, fontWeight: r.big ? 600 : 500, color: c, fontFamily: 'var(--mono)' }}>
                      {sign}{money(Math.abs(r.v))}
                    </span>
                  </div>
                );
              })}
            </div>

            {/* Tax + 1099 */}
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted" style={{ letterSpacing: 0.06, textTransform: 'uppercase', marginBottom: 8 }}>Taxes ytd</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 26, lineHeight: 1 }}>{money(168.42)}</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>Sales tax collected · 12 states · remitted by Micro</div>
                <div className="hf-divider" style={{ margin: '14px 0' }} />
                <div className="hf-col hf-gap-2">
                  {[
                    { s: 'California', a: 84.20, n: 13 },
                    { s: 'New York',   a: 38.40, n: 5 },
                    { s: 'Texas',      a: 24.10, n: 3 },
                    { s: '9 others',   a: 21.72, n: 2 },
                  ].map((t) => (
                    <div key={t.s} className="hf-flex hf-between hf-items-center">
                      <span className="hf-small">{t.s}</span>
                      <span className="hf-tiny hf-muted">{t.n} orders</span>
                      <span className="hf-num hf-small" style={{ width: 60, textAlign: 'right' }}>{money(t.a)}</span>
                    </div>
                  ))}
                </div>
              </div>

              <div className="hf-card" style={{ padding: 18, background: 'var(--paper-2)', border: 'none' }}>
                <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 4 }}>
                  <span style={{ color: 'var(--terra)' }}><Ico n="info" s={14} /></span>
                  <span className="hf-h4" style={{ fontSize: 13 }}>1099-K threshold</span>
                </div>
                <div className="hf-small hf-muted" style={{ lineHeight: 1.5 }}>
                  You're at <b style={{ color: 'var(--ink)' }}>$2,148 · 23 orders</b>. Federal threshold is $5,000 for 2026. We'll automatically generate your 1099-K when you cross it.
                </div>
                <div className="hf-progress" style={{ marginTop: 12 }}><i style={{ width: '43%' }} /></div>
                <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 6 }}>
                  <span>$2,148</span><span>$5,000</span>
                </div>
              </div>

              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 10 }}>Tax forms</div>
                <div className="hf-col hf-gap-2">
                  {[
                    { l: 'W-9',              s: 'On file · expires 2029',    tone: 'good' },
                    { l: 'State resale cert', s: 'Not added · optional',      tone: 'mute' },
                    { l: '1099-K · 2026',    s: 'Pending threshold',          tone: 'mute' },
                  ].map((f) => (
                    <div key={f.l} className="hf-flex hf-between hf-items-center" style={{ padding: '8px 0', borderBottom: '1px solid var(--line)' }}>
                      <div>
                        <div className="hf-h4" style={{ fontSize: 12.5 }}>{f.l}</div>
                        <div className="hf-tiny hf-muted">{f.s}</div>
                      </div>
                      <span className={`hf-chip hf-chip-${f.tone === 'good' ? 'good' : 'soft'}`} style={{ fontSize: 10 }}>
                        <span className={`hf-dot hf-dot-${f.tone}`} />{f.tone === 'good' ? 'Filed' : '—'}
                      </span>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>finance · statements</Anno>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · MOBILE SELLER — Dashboard (one-screen morning check-in)
// ──────────────────────────────────────────────────────────────────────
function MSeller_Dashboard() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80, background: 'var(--paper-2)' }}>
        {/* greeting + avatar */}
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 14, paddingBottom: 18, background: 'var(--paper)' }}>
          <div>
            <div className="hf-tiny hf-muted">Tuesday morning</div>
            <div className="hf-h3" style={{ fontSize: 18 }}>Hi, Mira</div>
          </div>
          <div className="hf-flex hf-items-center hf-gap-2">
            <button className="hf-icon-btn" style={{ position: 'relative' }}>
              <Ico n="bell" s={15} />
              <span style={{ position: 'absolute', top: 6, right: 7, width: 7, height: 7, borderRadius: 999, background: 'var(--terra)' }} />
            </button>
            <Avatar name="Mira K" size="md" />
          </div>
        </div>

        {/* big needs-action card */}
        <div className="hf-px-5" style={{ paddingBottom: 18 }}>
          <div className="hf-card" style={{ padding: 18, background: 'var(--ink)', color: 'var(--paper)', border: 'none' }}>
            <div className="hf-tiny" style={{ opacity: 0.7, letterSpacing: 0.06, textTransform: 'uppercase' }}>Today's tasks</div>
            <div className="hf-display-2" style={{ color: 'var(--paper)', fontSize: 26, lineHeight: 1.1, marginTop: 6, letterSpacing: '-0.01em' }}>3 orders to pack &amp; ship</div>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 14 }}>
              <button className="hf-btn" style={{ background: 'var(--paper)', color: 'var(--ink)', height: 36, padding: '0 16px' }}>Open inbox →</button>
              <button className="hf-btn" style={{ background: 'rgba(255,255,255,0.14)', color: 'var(--paper)', border: '1px solid rgba(255,255,255,0.25)', height: 36, padding: '0 16px' }}>Print labels</button>
            </div>
          </div>
        </div>

        {/* stats */}
        <div className="hf-px-5" style={{ paddingBottom: 14 }}>
          <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 10 }}>
            <span className="hf-h4">Today</span>
            <span className="hf-tiny hf-muted">vs avg</span>
          </div>
          <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr' }}>
            {[
              { l: 'Sales',   v: money(248), d: '↑ 2.1×', spark: [0.2,0.3,0.45,0.5,0.65,0.8,0.95] },
              { l: 'Orders',  v: '4',         d: '↑ 1.8×', spark: [0.1,0.2,0.3,0.4,0.5,0.7,0.85] },
              { l: 'Visits',  v: '142',       d: '+8',     spark: [0.3,0.4,0.4,0.5,0.6,0.7,0.8] },
              { l: 'Convers.', v: '3.1%',      d: '+0.4',   spark: [0.4,0.5,0.5,0.55,0.6,0.65,0.7] },
            ].map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 12 }}>
                <div className="hf-tiny hf-muted">{s.l}</div>
                <div className="hf-flex hf-between hf-items-end" style={{ marginTop: 4 }}>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 20, lineHeight: 1 }}>{s.v}</div>
                  <span className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600 }}>{s.d}</span>
                </div>
                <div style={{ height: 20, marginTop: 6, color: 'var(--primary)' }}>
                  <Spark data={s.spark} color="var(--primary)" fill="rgba(0,102,204,0.08)" />
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* needs action list */}
        <div className="hf-px-5" style={{ paddingTop: 8 }}>
          <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 10 }}>
            <span className="hf-h4">Needs you</span>
            <a className="hf-tiny" style={{ color: 'var(--primary)' }}>All 3 →</a>
          </div>
          <div className="hf-card" style={{ padding: 0, overflow: 'hidden' }}>
            {[
              { i: 'box',    c: 'var(--terra)',   t: 'Pack #1042 · Sasha L.', s: 'Persimmon vase · paid 2h ago',  cta: 'Pack' },
              { i: 'chat',   c: 'var(--primary)', t: 'Question from Devon',    s: '"Is the bowl food-safe?"',     cta: 'Reply' },
              { i: 'box',    c: 'var(--terra)',   t: 'Pack #1041 · Devon T.',  s: 'Forest bowl · paid 5h ago',     cta: 'Pack' },
            ].map((n, i, a) => (
              <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '14px 16px', borderBottom: i < a.length - 1 ? '1px solid var(--line)' : 'none' }}>
                <span className="hf-center" style={{ width: 36, height: 36, borderRadius: 10, background: 'var(--paper-2)', color: n.c, flexShrink: 0 }}><Ico n={n.i} s={15} /></span>
                <div className="hf-grow">
                  <div className="hf-h4" style={{ fontSize: 13 }}>{n.t}</div>
                  <div className="hf-tiny hf-muted">{n.s}</div>
                </div>
                <button className="hf-btn hf-btn-outline hf-btn-sm" style={{ height: 28, padding: '0 10px', fontSize: 11 }}>{n.cta}</button>
              </div>
            ))}
          </div>
        </div>

        {/* low stock */}
        <div className="hf-px-5" style={{ paddingTop: 18 }}>
          <div className="hf-h4" style={{ marginBottom: 10 }}>Low stock</div>
          <div className="hf-card" style={{ padding: 14 }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <ProdImg tone="rust" h={48} r={8} />
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13 }}>Ash budstem</div>
                <div className="hf-tiny hf-muted">2 left · 4 in pre-order</div>
              </div>
              <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ padding: '0 0' }}>Restock</button>
            </div>
          </div>
        </div>
      </div>
      {/* mobile seller tab bar */}
      <div className="hf-flex" style={{ borderTop: '1px solid var(--line)', padding: '8px 0 14px', background: 'var(--paper)', position: 'absolute', bottom: 0, left: 0, right: 0 }}>
        {[
          { l: 'Home', i: 'home', on: true },
          { l: 'Orders', i: 'inbox', n: 4 },
          { l: 'Listings', i: 'pkg' },
          { l: 'Stats', i: 'chart' },
          { l: 'More', i: 'cog' },
        ].map((t) => (
          <div key={t.l} className="hf-col hf-items-center hf-grow hf-gap-1" style={{ color: t.on ? 'var(--ink)' : 'var(--ink-3)', position: 'relative' }}>
            <Ico n={t.i} s={18} />
            <span style={{ fontSize: 10, fontWeight: t.on ? 600 : 400 }}>{t.l}</span>
            {t.n && <span style={{ position: 'absolute', top: -2, right: '30%', width: 16, height: 16, borderRadius: 999, background: 'var(--terra)', color: 'white', fontSize: 9, fontWeight: 600, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>{t.n}</span>}
          </div>
        ))}
      </div>
      <PhoneHome />
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · MOBILE SELLER — Orders inbox (swipe-friendly)
// ──────────────────────────────────────────────────────────────────────
function MSeller_Orders() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80 }}>
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 14, paddingBottom: 12 }}>
          <div className="hf-h3" style={{ fontSize: 20 }}>Orders</div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-icon-btn"><Ico n="search" s={15} /></button>
            <button className="hf-icon-btn"><Ico n="filter" s={15} /></button>
          </div>
        </div>

        {/* tab pills */}
        <div className="hf-flex hf-px-5 hf-gap-1" style={{ paddingBottom: 14 }}>
          {[
            { l: 'Needs action', n: 4, on: true },
            { l: 'Shipped', n: 18 },
            { l: 'Done', n: 21 },
          ].map((t) => (
            <span key={t.l} className={`hf-chip ${t.on ? 'hf-chip-on' : ''}`} style={{ fontSize: 11.5, height: 30 }}>
              {t.l} <span className="hf-num" style={{ marginLeft: 4, fontSize: 10, padding: '1px 5px', borderRadius: 999, background: t.on ? 'rgba(255,255,255,0.18)' : 'var(--paper-2)' }}>{t.n}</span>
            </span>
          ))}
        </div>

        {/* swipe-revealed row */}
        <div style={{ position: 'relative', overflow: 'hidden', borderBottom: '1px solid var(--line)' }}>
          <div style={{ position: 'absolute', right: 0, top: 0, bottom: 0, display: 'flex' }}>
            <div className="hf-center" style={{ width: 64, background: 'var(--good)', color: 'white', flexDirection: 'column', gap: 2 }}>
              <Ico n="box" s={16} />
              <span style={{ fontSize: 10 }}>Pack</span>
            </div>
            <div className="hf-center" style={{ width: 64, background: 'var(--terra)', color: 'white', flexDirection: 'column', gap: 2 }}>
              <Ico n="truck" s={16} />
              <span style={{ fontSize: 10 }}>Ship</span>
            </div>
          </div>
          <div className="hf-flex hf-items-center hf-gap-3 hf-px-5" style={{ padding: '14px 20px', background: 'var(--paper)', transform: 'translateX(-128px)' }}>
            <ProdImg tone="clay" h={48} r={8} />
            <div className="hf-grow">
              <div className="hf-flex hf-items-center hf-gap-2">
                <span className="hf-mono" style={{ fontSize: 11, color: 'var(--ink)', fontWeight: 600 }}>#1042</span>
                <span className="hf-chip hf-chip-warn" style={{ fontSize: 10, padding: '1px 6px' }}>New</span>
              </div>
              <div className="hf-h4" style={{ fontSize: 13, marginTop: 2 }}>Sasha L. · Persimmon vase</div>
              <div className="hf-tiny hf-muted">2h ago · {money(86)}</div>
            </div>
          </div>
        </div>

        {/* normal rows */}
        {[
          { id: '#1041', n: 'Devon T.',  t: 'Forest bowl, lg.',         tone: 'sage', age: '5h',  amt: 64,  status: 'New',    chip: 'warn' },
          { id: '#1040', n: 'Ari K.',    t: 'Cream tumbler set',        tone: 'cream', age: '7h',  amt: 48,  status: 'New',    chip: 'warn' },
          { id: '#1039', n: 'June P.',   t: 'Indigo carafe',            tone: 'indigo', age: '1d', amt: 110, status: 'Packed', chip: 'soft' },
          { id: '#1038', n: 'Theo R.',   t: 'Soft hand vessel +2',      tone: 'rust', age: '2d',  amt: 218, status: 'Shipped',chip: 'soft' },
          { id: '#1037', n: 'Liu W.',    t: 'Ceremony bowl',            tone: 'clay', age: '3d',  amt: 142, status: 'Shipped',chip: 'soft' },
          { id: '#1036', n: 'Marisol G.', t: 'Field cup × 4',           tone: 'cream', age: '4d', amt: 88,  status: 'Refund', chip: 'bad' },
        ].map((o, i) => (
          <div key={i} className="hf-flex hf-items-center hf-gap-3 hf-px-5" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
            <ProdImg tone={o.tone} h={48} r={8} />
            <div className="hf-grow">
              <div className="hf-flex hf-items-center hf-gap-2">
                <span className="hf-mono" style={{ fontSize: 11, color: 'var(--ink-2)', fontWeight: 600 }}>{o.id}</span>
                <span className={`hf-chip hf-chip-${o.chip}`} style={{ fontSize: 10, padding: '1px 6px' }}>{o.status}</span>
              </div>
              <div className="hf-h4" style={{ fontSize: 13, marginTop: 2 }}>{o.n} · {o.t}</div>
              <div className="hf-tiny hf-muted">{o.age} · {money(o.amt)}</div>
            </div>
            <Ico n="chevR" s={13} />
          </div>
        ))}
      </div>
      <Anno top={120} right={20}>swipe · pack / ship</Anno>
      <PhoneHome />
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05 · MOBILE SELLER — Order detail (pack & ship on the go)
// ──────────────────────────────────────────────────────────────────────
function MSeller_OrderDetail() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 110 }}>
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 12 }}>
          <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
          <div className="hf-h4" style={{ fontSize: 14 }}>#1042</div>
          <button className="hf-icon-btn"><Ico n="chat" s={15} /></button>
        </div>

        {/* Status hero */}
        <div className="hf-px-5" style={{ paddingBottom: 18 }}>
          <div className="hf-eyebrow" style={{ color: 'var(--terra)', marginBottom: 4 }}>★ Needs shipping</div>
          <div className="hf-display-2" style={{ fontSize: 24, lineHeight: 1.1, letterSpacing: '-0.01em' }}>Pack &amp; ship the persimmon vase.</div>
          <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>Sasha paid 2 hours ago · note attached</div>
        </div>

        {/* Buyer */}
        <div className="hf-px-5">
          <div className="hf-card" style={{ padding: 14 }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <Avatar name="Sasha L" size="md" />
              <div className="hf-grow">
                <div className="hf-h4">Sasha Leblanc</div>
                <div className="hf-tiny hf-muted">3rd order · $284 lifetime</div>
              </div>
              <button className="hf-icon-btn"><Ico n="chat" s={14} /></button>
            </div>
            <div className="hf-divider" style={{ margin: '12px 0' }} />
            <div className="hf-tiny hf-muted">Ship to</div>
            <div className="hf-small">820 Sutter St · #4B<br />San Francisco, CA 94109</div>
          </div>
        </div>

        {/* Items */}
        <div className="hf-px-5" style={{ paddingTop: 16 }}>
          <div className="hf-h4" style={{ marginBottom: 10 }}>1 item to pack</div>
          <div className="hf-card" style={{ padding: 14 }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <ProdImg tone="clay" h={56} r={8} />
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13.5 }}>Persimmon vase</div>
                <div className="hf-tiny hf-muted">SKU PV-08 · qty 1</div>
              </div>
              <span className="hf-num hf-h4">{money(86)}</span>
            </div>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 12, padding: 10, background: 'var(--paper-2)', borderRadius: 8 }}>
              <span className="hf-center" style={{ width: 18, height: 18, borderRadius: 4, background: 'var(--ink)', color: 'white' }}><Ico n="check" s={11} sw={2.4} /></span>
              <span className="hf-small">Picked from shelf</span>
              <span className="hf-grow" />
              <span className="hf-tiny hf-muted">just now</span>
            </div>
          </div>
        </div>

        {/* Shipping label picker */}
        <div className="hf-px-5" style={{ paddingTop: 16 }}>
          <div className="hf-h4" style={{ marginBottom: 10 }}>Buy a label</div>
          <div className="hf-col hf-gap-2">
            {[
              { l: 'USPS Priority',     s: '1–3 days · $50 insured', p: 9.84, on: true },
              { l: 'USPS Ground',        s: '2–5 days · tracked',     p: 6.52, on: false },
              { l: 'UPS Ground',         s: '3–4 days · pickup',      p: 11.20, on: false },
            ].map((o) => (
              <div key={o.l} className="hf-flex hf-items-center" style={{ padding: 12, borderRadius: 10, gap: 10, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: o.on ? 'var(--paper-2)' : 'var(--paper)' }}>
                <span className="hf-center" style={{ width: 16, height: 16, borderRadius: 999, border: '1.5px solid var(--ink)' }}>{o.on && <span style={{ width: 7, height: 7, borderRadius: 999, background: 'var(--ink)' }} />}</span>
                <div className="hf-grow">
                  <div className="hf-h4" style={{ fontSize: 13 }}>{o.l}</div>
                  <div className="hf-tiny hf-muted">{o.s}</div>
                </div>
                <span className="hf-num hf-h4">{money(o.p)}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Note */}
        <div className="hf-px-5" style={{ paddingTop: 16 }}>
          <div className="hf-card" style={{ padding: 14, background: 'var(--paper-2)', border: 'none' }}>
            <div className="hf-tiny hf-muted">Note from Sasha</div>
            <div className="hf-small" style={{ marginTop: 4, fontStyle: 'italic' }}>"So excited — please pack carefully, this is for my mom."</div>
          </div>
        </div>
      </div>
      {/* sticky cta */}
      <div style={{ position: 'absolute', bottom: 0, left: 0, right: 0, padding: '12px 20px 24px', background: 'var(--paper)', borderTop: '1px solid var(--line)' }}>
        <div className="hf-flex hf-between" style={{ marginBottom: 8 }}>
          <span className="hf-tiny hf-muted">Charged to payouts</span>
          <span className="hf-num hf-h4">{money(9.84)}</span>
        </div>
        <button className="hf-btn hf-btn-primary" style={{ width: '100%', height: 44 }}>Buy &amp; print label →</button>
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 06 · MOBILE SELLER — Listings (light catalog editor)
// ──────────────────────────────────────────────────────────────────────
function MSeller_Listings() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80 }}>
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 14, paddingBottom: 12 }}>
          <div>
            <div className="hf-tiny hf-muted">14 listings · 3 drafts</div>
            <div className="hf-h3" style={{ fontSize: 20 }}>Listings</div>
          </div>
          <button className="hf-btn hf-btn-primary hf-btn-sm" style={{ height: 32, padding: '0 12px' }}><Ico n="plus" s={11} /> New</button>
        </div>

        {/* search */}
        <div className="hf-px-5" style={{ paddingBottom: 12 }}>
          <div className="hf-flex hf-items-center hf-gap-2" style={{ height: 36, padding: '0 12px', background: 'var(--paper-2)', borderRadius: 9999 }}>
            <Ico n="search" s={13} />
            <span className="hf-small hf-muted">Search listings…</span>
          </div>
        </div>

        {/* filter chips */}
        <div className="hf-flex hf-gap-1 hf-px-5" style={{ paddingBottom: 14 }}>
          {[
            { l: 'All', n: 14, on: true },
            { l: 'Active', n: 11 },
            { l: 'Drafts', n: 3 },
            { l: 'Out of stock', n: 1 },
          ].map((t) => (
            <span key={t.l} className={`hf-chip ${t.on ? 'hf-chip-on' : ''}`} style={{ fontSize: 11, height: 28 }}>{t.l} · {t.n}</span>
          ))}
        </div>

        {/* listing rows */}
        {[
          { t: 'Persimmon vase',     p: 86,  stk: 12, sold: 14, tone: 'clay',   status: 'Active', chip: 'good' },
          { t: 'Forest bowl, lg.',   p: 64,  stk: 4,  sold: 6,  tone: 'sage',   status: 'Active', chip: 'good' },
          { t: 'Ash budstem',        p: 66,  stk: 2,  sold: 8,  tone: 'rust',   status: 'Low stock', chip: 'warn' },
          { t: 'Cream tumbler set',  p: 48,  stk: 6,  sold: 4,  tone: 'cream',  status: 'Active', chip: 'good' },
          { t: 'Indigo carafe',      p: 110, stk: 0,  sold: 3,  tone: 'indigo', status: 'Out',    chip: 'bad' },
          { t: 'Storm bowl · sm',    p: 0,   stk: 0,  sold: 0,  tone: 'sage',   status: 'Draft',  chip: 'soft' },
        ].map((l, i) => (
          <div key={i} className="hf-flex hf-items-center hf-gap-3 hf-px-5" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
            <div style={{ position: 'relative' }}>
              <ProdImg tone={l.tone} h={56} r={8} />
              {l.stk === 0 && l.status === 'Out' && (
                <div style={{ position: 'absolute', inset: 0, background: 'rgba(0,0,0,0.35)', borderRadius: 8, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                  <span className="hf-tiny" style={{ color: 'white', fontWeight: 600 }}>OUT</span>
                </div>
              )}
            </div>
            <div className="hf-grow">
              <div className="hf-h4" style={{ fontSize: 13 }}>{l.t}</div>
              <div className="hf-flex hf-gap-2 hf-items-center" style={{ marginTop: 3 }}>
                {l.p > 0 ? <span className="hf-num hf-tiny" style={{ color: 'var(--ink)', fontWeight: 600 }}>{money(l.p)}</span> : <span className="hf-tiny hf-muted">—</span>}
                <span className="hf-tiny hf-muted">·</span>
                <span className="hf-tiny hf-muted">{l.stk} in stock</span>
                <span className="hf-tiny hf-muted">·</span>
                <span className="hf-tiny hf-muted">{l.sold} sold</span>
              </div>
            </div>
            <span className={`hf-chip hf-chip-${l.chip}`} style={{ fontSize: 10, padding: '1px 6px' }}>{l.status}</span>
          </div>
        ))}
      </div>
      <PhoneHome />
    </div>
  );
}

Object.assign(window, {
  SCfg_Settings, SCfg_Finance,
  MSeller_Dashboard, MSeller_Orders, MSeller_OrderDetail, MSeller_Listings,
});

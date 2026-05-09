// hifi-buyer-extras.jsx — Buyer-side gap fills (mobile, 360×720)
// Search · Account home · Order tracking · Addresses · Saved items
// Returns request · Leave review · Notifications

// shared chrome — top bar with back + title
function MTop({ title, back = true, right }) {
  return (
    <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 12 }}>
      {back ? <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button> : <span style={{ width: 32 }} />}
      <div className="hf-h4" style={{ fontSize: 14 }}>{title}</div>
      {right || <span style={{ width: 32 }} />}
    </div>
  );
}

// bottom tab bar
function MTabBar({ active = 'Account' }) {
  const tabs = [
    { l: 'Shop', i: 'home' },
    { l: 'Search', i: 'search' },
    { l: 'Saved', i: 'heart' },
    { l: 'Account', i: 'user' },
  ];
  return (
    <div className="hf-flex" style={{ borderTop: '1px solid var(--line)', padding: '8px 0 14px', background: 'var(--paper)', position: 'absolute', bottom: 0, left: 0, right: 0 }}>
      {tabs.map(t => (
        <div key={t.l} className="hf-col hf-items-center hf-grow hf-gap-1" style={{ color: t.l === active ? 'var(--ink)' : 'var(--ink-3)' }}>
          <Ico n={t.i} s={18} />
          <span style={{ fontSize: 10, fontWeight: t.l === active ? 600 : 400 }}>{t.l}</span>
        </div>
      ))}
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 01 · SEARCH RESULTS — query "vase", filters open
// ──────────────────────────────────────────────────────────────────────
function MFlow_Search() {
  const results = [
    { t: 'Persimmon vase', s: 'Mira Studio · Oakland', p: 86, tone: 'clay' },
    { t: 'Storm bud vase', s: 'Field & Hand · Detroit', p: 64, tone: 'sage' },
    { t: 'Cream carafe',   s: 'Soft Goods · Toronto', p: 110, tone: 'cream' },
    { t: 'Ash budstem',    s: 'Mira Studio · Oakland', p: 66, tone: 'rust' },
    { t: 'Indigo vessel',  s: 'Pier Ceramics · Maine', p: 142, tone: 'indigo' },
  ];
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'hidden' }}>
        {/* search field */}
        <div className="hf-flex hf-items-center hf-gap-2 hf-px-5" style={{ paddingTop: 12, paddingBottom: 10 }}>
          <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
          <div className="hf-flex hf-items-center hf-gap-2 hf-grow" style={{ height: 36, padding: '0 12px', background: 'var(--paper-2)', borderRadius: 9999 }}>
            <Ico n="search" s={13} />
            <span className="hf-h4" style={{ fontSize: 13.5 }}>vase</span>
            <span style={{ width: 1.5, height: 14, background: 'var(--ink)', animation: 'hf-blink 1s steps(1) infinite' }} />
            <span className="hf-grow" />
            <Ico n="close" s={13} />
          </div>
        </div>

        {/* result count + sort */}
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingBottom: 8 }}>
          <span className="hf-tiny hf-muted">28 vases · across 11 makers</span>
          <span className="hf-tiny hf-flex hf-items-center hf-gap-1" style={{ color: 'var(--ink-2)' }}>Most loved <Ico n="chevD" s={10} /></span>
        </div>

        {/* filter chips */}
        <div className="hf-flex hf-gap-1 hf-px-5" style={{ paddingBottom: 12, overflow: 'hidden' }}>
          {[
            { l: 'All filters', icon: 'filter', on: false },
            { l: 'Under $100',  on: true },
            { l: 'Hand-thrown', on: true },
            { l: 'Local pickup', on: false },
            { l: 'In stock', on: false },
          ].map((c, i) => (
            <span key={c.l} className={`hf-chip ${c.on ? 'hf-chip-on' : ''}`} style={{ fontSize: 11, height: 28, padding: '0 10px', flexShrink: 0 }}>
              {c.icon && <Ico n={c.icon} s={11} />}{c.l}
            </span>
          ))}
        </div>

        {/* applied filter rail */}
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingBottom: 8 }}>
          <span className="hf-tiny" style={{ color: 'var(--primary)', fontWeight: 600 }}>2 filters applied</span>
          <span className="hf-tiny hf-muted" style={{ textDecoration: 'underline' }}>Clear</span>
        </div>

        {/* grid */}
        <div className="hf-grid hf-gap-2 hf-px-5" style={{ gridTemplateColumns: '1fr 1fr', overflow: 'auto', paddingBottom: 80 }}>
          {results.map((r, i) => (
            <div key={i}>
              <ProdImg tone={r.tone} h={140} r={10} badge={i === 0 ? '★' : undefined} />
              <div className="hf-h4" style={{ fontSize: 12.5, marginTop: 8 }}>{r.t}</div>
              <div className="hf-tiny hf-muted">{r.s}</div>
              <div className="hf-num" style={{ fontSize: 13, fontWeight: 600, marginTop: 2 }}>{money(r.p)}</div>
            </div>
          ))}
        </div>
      </div>
      <MTabBar active="Search" />
      <PhoneHome />
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · ACCOUNT HOME
// ──────────────────────────────────────────────────────────────────────
function MFlow_Account() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80 }}>
        <MTop title="Account" back={false} right={<button className="hf-icon-btn"><Ico n="cog" s={15} /></button>} />
        {/* identity card */}
        <div className="hf-flex hf-items-center hf-gap-3 hf-px-5" style={{ paddingBottom: 18 }}>
          <Avatar name="Mira K" size="xl" />
          <div className="hf-grow">
            <div className="hf-display-2" style={{ fontSize: 22, lineHeight: 1.05, letterSpacing: '-0.01em' }}>Mira Kim</div>
            <div className="hf-tiny hf-muted">mira.k@gmail.com · since Mar '26</div>
          </div>
        </div>

        {/* stat strip */}
        <div className="hf-flex hf-px-5 hf-gap-2" style={{ paddingBottom: 18 }}>
          {[
            { l: 'Orders',   v: '6' },
            { l: 'Saved',    v: '14' },
            { l: 'Followed', v: '8' },
          ].map(s => (
            <div key={s.l} className="hf-card hf-grow" style={{ padding: 12 }}>
              <div className="hf-display-2 hf-num" style={{ fontSize: 22, lineHeight: 1 }}>{s.v}</div>
              <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{s.l}</div>
            </div>
          ))}
        </div>

        {/* in-flight order */}
        <div className="hf-px-5">
          <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 10 }}>
            <span className="hf-h4">Active order</span>
            <a className="hf-tiny" style={{ color: 'var(--primary)' }}>All orders →</a>
          </div>
          <div className="hf-card" style={{ padding: 14 }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <ProdImg tone="clay" h={60} r={8} />
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13 }}>Persimmon vase</div>
                <div className="hf-tiny hf-muted">#1042 · Mira Studio</div>
              </div>
              <span className="hf-num hf-h4" style={{ fontSize: 13 }}>{money(86)}</span>
            </div>
            {/* progress beam */}
            <div className="hf-flex hf-items-center" style={{ marginTop: 14, gap: 0 }}>
              {['Placed', 'Packed', 'Shipped', 'Out', 'Delivered'].map((s, i) => (
                <React.Fragment key={s}>
                  <div className="hf-col hf-items-center" style={{ flexShrink: 0 }}>
                    <span className="hf-center" style={{ width: 18, height: 18, borderRadius: 999, background: i <= 2 ? 'var(--ink)' : 'var(--paper-2)', color: i <= 2 ? 'white' : 'var(--ink-4)', border: i <= 2 ? 'none' : '1.5px solid var(--ink-4)' }}>
                      {i <= 2 ? <Ico n="check" s={9} sw={2.4} /> : <span style={{ width: 5, height: 5, borderRadius: 999, background: i === 3 ? 'var(--ink)' : 'transparent' }} />}
                    </span>
                    <span className="hf-tiny hf-muted" style={{ fontSize: 9, marginTop: 4, fontWeight: i === 3 ? 600 : 400, color: i === 3 ? 'var(--ink)' : undefined }}>{s}</span>
                  </div>
                  {i < 4 && <span style={{ flex: 1, height: 2, background: i < 2 ? 'var(--ink)' : 'var(--line)', margin: '0 2px', marginBottom: 14 }} />}
                </React.Fragment>
              ))}
            </div>
            <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 12, paddingTop: 12, borderTop: '1px solid var(--line)' }}>
              <div>
                <div className="hf-tiny hf-muted">Out for delivery</div>
                <div className="hf-h4" style={{ fontSize: 13 }}>Today by 8:00 PM</div>
              </div>
              <button className="hf-btn hf-btn-outline hf-btn-sm">Track →</button>
            </div>
          </div>
        </div>

        {/* menu */}
        <div className="hf-px-5" style={{ marginTop: 22 }}>
          {[
            { i: 'inbox',  l: 'Order history', s: '6 orders · 1 in flight', n: null },
            { i: 'truck',  l: 'Returns',       s: '1 in progress · #1036',  n: 1 },
            { i: 'heart',  l: 'Saved items',   s: '14 across 6 shops' },
            { i: 'pin',    l: 'Addresses',     s: '2 saved' },
            { i: 'card',   l: 'Payment',       s: 'Visa · 4421' },
            { i: 'bell',   l: 'Notifications', s: '3 unread', n: 3 },
            { i: 'cog',    l: 'Settings',      s: 'Privacy · language · region' },
          ].map((m) => (
            <div key={m.l} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '14px 0', borderBottom: '1px solid var(--line)' }}>
              <span className="hf-center" style={{ width: 36, height: 36, borderRadius: 10, background: 'var(--paper-2)' }}><Ico n={m.i} s={15} /></span>
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13.5 }}>{m.l}</div>
                <div className="hf-tiny hf-muted">{m.s}</div>
              </div>
              {m.n && <span className="hf-chip hf-chip-warn" style={{ fontSize: 10, padding: '2px 7px' }}>{m.n}</span>}
              <Ico n="chevR" s={13} />
            </div>
          ))}
        </div>

        <div className="hf-px-5" style={{ marginTop: 18 }}>
          <button className="hf-btn hf-btn-ghost" style={{ width: '100%', color: 'var(--ink-3)' }}>Sign out</button>
          <div className="hf-tiny hf-muted" style={{ textAlign: 'center', marginTop: 6 }}>v 4.2 · made in Oakland</div>
        </div>
      </div>
      <MTabBar active="Account" />
      <PhoneHome />
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · ORDER TRACKING — single order detail w/ map
// ──────────────────────────────────────────────────────────────────────
function MFlow_Tracking() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80 }}>
        <MTop title="Order #1042" right={<button className="hf-icon-btn"><Ico n="chat" s={15} /></button>} />

        {/* map placeholder */}
        <div style={{ position: 'relative', height: 220, background: 'var(--paper-2)', overflow: 'hidden' }}>
          <svg viewBox="0 0 360 220" width="100%" height="100%" style={{ position: 'absolute' }}>
            {/* abstract street grid */}
            <g stroke="var(--line)" strokeWidth="1">
              {[20, 60, 100, 140, 180, 220, 260, 300, 340].map(x => <line key={'v'+x} x1={x} y1={0} x2={x} y2={220} />)}
              {[20, 60, 100, 140, 180].map(y => <line key={'h'+y} x1={0} y1={y} x2={360} y2={y} />)}
            </g>
            {/* dashed route */}
            <path d="M 60 180 C 130 160 160 100 220 90 S 290 60 320 40" fill="none" stroke="var(--ink)" strokeWidth="2.5" strokeDasharray="6 4" />
          </svg>
          {/* origin pin */}
          <div style={{ position: 'absolute', left: 50, bottom: 32 }}>
            <span className="hf-center" style={{ width: 22, height: 22, borderRadius: 999, background: 'var(--paper)', border: '2px solid var(--ink)' }}><span style={{ width: 8, height: 8, borderRadius: 999, background: 'var(--ink)' }} /></span>
          </div>
          {/* truck */}
          <div style={{ position: 'absolute', left: 200, top: 70 }}>
            <span className="hf-center" style={{ width: 32, height: 32, borderRadius: 999, background: 'var(--ink)', color: 'white', boxShadow: '0 4px 12px rgba(0,0,0,0.15)' }}><Ico n="truck" s={15} /></span>
          </div>
          {/* dest pin */}
          <div style={{ position: 'absolute', right: 28, top: 24 }}>
            <span className="hf-center" style={{ width: 22, height: 22, borderRadius: 999, background: 'var(--terra)', color: 'white' }}><Ico n="pin" s={11} /></span>
          </div>
          {/* eta pill */}
          <div style={{ position: 'absolute', left: '50%', top: 14, transform: 'translateX(-50%)', padding: '8px 14px', background: 'var(--ink)', color: 'white', borderRadius: 9999, fontSize: 12, fontWeight: 500 }}>
            Out for delivery · arrives by 8:00 PM
          </div>
        </div>

        {/* status copy */}
        <div className="hf-px-5" style={{ paddingTop: 18, paddingBottom: 14 }}>
          <div className="hf-eyebrow" style={{ color: 'var(--good)', marginBottom: 4 }}>★ On the way</div>
          <div className="hf-display-2" style={{ fontSize: 24, lineHeight: 1.1, letterSpacing: '-0.01em' }}>3 stops away. Sit tight.</div>
          <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>USPS · 9405 5036 9930 0124 2317</div>
        </div>

        {/* timeline */}
        <div className="hf-px-5" style={{ paddingBottom: 18 }}>
          {[
            { t: 'Out for delivery',   s: 'San Francisco · 12:14 PM',  on: true,  active: true },
            { t: 'In transit',          s: 'San Francisco hub · 8:02 AM', on: true },
            { t: 'Shipped',             s: 'Oakland · Mar 15, 6:48 PM',  on: true },
            { t: 'Packed',              s: 'Mira Studio · Mar 15',       on: true },
            { t: 'Order placed',        s: 'Mar 14, 7:14 PM',            on: true },
          ].map((e, i, a) => (
            <div key={i} className="hf-flex hf-items-start hf-gap-3" style={{ paddingBottom: i < a.length - 1 ? 14 : 0 }}>
              <div className="hf-col hf-items-center" style={{ width: 18, flexShrink: 0 }}>
                <span className="hf-center" style={{ width: 14, height: 14, borderRadius: 999, background: e.active ? 'var(--terra)' : e.on ? 'var(--ink)' : 'var(--paper-2)', color: 'white' }}>
                  {e.active && <span style={{ width: 6, height: 6, borderRadius: 999, background: 'white' }} />}
                </span>
                {i < a.length - 1 && <span style={{ width: 1.5, flex: 1, background: 'var(--line)', minHeight: 16, marginTop: 2 }} />}
              </div>
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13, color: e.active ? 'var(--ink)' : 'var(--ink-2)' }}>{e.t}</div>
                <div className="hf-tiny hf-muted">{e.s}</div>
              </div>
            </div>
          ))}
        </div>

        {/* item card */}
        <div className="hf-px-5" style={{ paddingBottom: 18 }}>
          <div className="hf-card" style={{ padding: 14 }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <ProdImg tone="clay" h={56} r={8} />
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13.5 }}>Persimmon vase</div>
                <div className="hf-tiny hf-muted">Mira Studio · qty 1</div>
              </div>
              <span className="hf-num hf-h4">{money(86)}</span>
            </div>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 12 }}>
              <button className="hf-btn hf-btn-outline hf-btn-sm hf-grow">Message Mira</button>
              <button className="hf-btn hf-btn-outline hf-btn-sm hf-grow">Need to return?</button>
            </div>
          </div>
        </div>
      </div>
      <PhoneHome />
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · ADDRESSES
// ──────────────────────────────────────────────────────────────────────
function MFlow_Addresses() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80 }}>
        <MTop title="Addresses" right={<button className="hf-icon-btn"><Ico n="plus" s={15} /></button>} />
        <div className="hf-px-5">
          {[
            { l: 'Home',  n: 'Mira Kim', a1: '820 Sutter St · #4B', a2: 'San Francisco, CA 94109', def: true },
            { l: 'Work',  n: 'Mira Kim', a1: '120 Mission St · 14F', a2: 'San Francisco, CA 94105', def: false },
          ].map((a, i) => (
            <div key={i} className="hf-card" style={{ padding: 16, marginBottom: 12, position: 'relative', border: a.def ? '1.5px solid var(--ink)' : '1px solid var(--line)' }}>
              <div className="hf-flex hf-between" style={{ marginBottom: 6 }}>
                <span className="hf-eyebrow">{a.l}</span>
                {a.def && <span className="hf-chip hf-chip-soft" style={{ fontSize: 10 }}>Default</span>}
              </div>
              <div className="hf-h4">{a.n}</div>
              <div className="hf-small hf-muted" style={{ marginTop: 2 }}>{a.a1}<br />{a.a2}</div>
              <div className="hf-flex hf-gap-2" style={{ marginTop: 12 }}>
                <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ padding: '0 0' }}>Edit</button>
                {!a.def && <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ padding: '0 0' }}>Make default</button>}
              </div>
            </div>
          ))}

          <button className="hf-card hf-flex hf-items-center hf-center hf-gap-2" style={{ width: '100%', padding: 16, border: '1.5px dashed var(--ink-4)', background: 'transparent', color: 'var(--ink-2)' }}>
            <Ico n="plus" s={14} /> <span className="hf-h4" style={{ fontSize: 13 }}>Add address</span>
          </button>
        </div>

        <div className="hf-px-5" style={{ marginTop: 22 }}>
          <div className="hf-h4" style={{ marginBottom: 10 }}>Saved items · 14</div>
          <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr 1fr' }}>
            {['clay', 'sage', 'rust', 'cream', 'indigo', 'rose'].map((tone, i) => (
              <div key={i} style={{ position: 'relative' }}>
                <ProdImg tone={tone} h={88} r={8} />
                <span style={{ position: 'absolute', top: 6, right: 6, width: 22, height: 22, borderRadius: 999, background: 'rgba(255,255,255,0.92)', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--terra)' }}><Ico n="heart" s={11} /></span>
              </div>
            ))}
          </div>
          <a className="hf-tiny" style={{ color: 'var(--primary)', display: 'block', textAlign: 'center', marginTop: 14 }}>See all 14 →</a>
        </div>
      </div>
      <MTabBar active="Account" />
      <PhoneHome />
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05 · RETURN REQUEST
// ──────────────────────────────────────────────────────────────────────
function MFlow_Return() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 110 }}>
        <MTop title="Return · #1036" />
        <div className="hf-px-5">
          <div className="hf-eyebrow" style={{ color: 'var(--terra)', marginBottom: 6 }}>Step 2 of 3</div>
          <div className="hf-display-2" style={{ fontSize: 24, lineHeight: 1.1, letterSpacing: '-0.01em' }}>What went wrong with the Field cup?</div>
          <div className="hf-small hf-muted" style={{ marginTop: 6 }}>Mira will see this. Be kind — most things get sorted with a quick message.</div>

          {/* product */}
          <div className="hf-card" style={{ padding: 14, marginTop: 18 }}>
            <div className="hf-flex hf-items-center hf-gap-3">
              <ProdImg tone="cream" h={64} r={8} />
              <div className="hf-grow">
                <div className="hf-h4" style={{ fontSize: 13.5 }}>Field cup × 4</div>
                <div className="hf-tiny hf-muted">Marisol G. · arrived Apr 1</div>
              </div>
              <span className="hf-num hf-h4">{money(88)}</span>
            </div>
          </div>

          {/* reason */}
          <div className="hf-eyebrow" style={{ marginTop: 22, marginBottom: 8 }}>Reason</div>
          <div className="hf-col hf-gap-2">
            {[
              { l: 'Arrived damaged',           s: 'Chip, crack, or breakage', on: true },
              { l: 'Wrong item',                 s: 'Different from listing',  on: false },
              { l: 'Changed my mind',            s: 'Eligible within 14 days', on: false },
              { l: 'Quality not as expected',    s: 'Color or finish differs', on: false },
              { l: 'Never arrived',              s: 'Lost in shipping',        on: false },
            ].map((r) => (
              <div key={r.l} className="hf-flex hf-items-start hf-gap-3" style={{ padding: 12, borderRadius: 10, border: r.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: r.on ? 'var(--paper-2)' : 'var(--paper)' }}>
                <span className="hf-center" style={{ width: 16, height: 16, borderRadius: 999, border: '1.5px solid var(--ink)', flexShrink: 0, marginTop: 2 }}>
                  {r.on && <span style={{ width: 8, height: 8, borderRadius: 999, background: 'var(--ink)' }} />}
                </span>
                <div>
                  <div className="hf-h4" style={{ fontSize: 13 }}>{r.l}</div>
                  <div className="hf-tiny hf-muted">{r.s}</div>
                </div>
              </div>
            ))}
          </div>

          {/* photos */}
          <div className="hf-eyebrow" style={{ marginTop: 22, marginBottom: 8 }}>Add photos · 2 of 4</div>
          <div className="hf-flex hf-gap-2">
            <ProdImg tone="cream" h={72} r={8} />
            <ProdImg tone="cream" h={72} r={8} />
            <button className="hf-card hf-center" style={{ width: 72, height: 72, padding: 0, border: '1.5px dashed var(--ink-4)', background: 'transparent', color: 'var(--ink-3)' }}>
              <Ico n="plus" s={18} />
            </button>
          </div>

          {/* note */}
          <div className="hf-eyebrow" style={{ marginTop: 22, marginBottom: 8 }}>Note to Mira (optional)</div>
          <div style={{ minHeight: 90, border: '1px solid var(--line)', borderRadius: 10, padding: 12 }}>
            <p className="hf-small">Two of the four arrived chipped along the rim — looks like the box was crushed. The other two are perfect, no rush, happy to keep them.</p>
            <span style={{ width: 1.5, height: 14, background: 'var(--ink)', display: 'inline-block', verticalAlign: 'middle', animation: 'hf-blink 1s steps(1) infinite' }} />
          </div>

          {/* resolution preview */}
          <div className="hf-card" style={{ padding: 14, marginTop: 18, background: 'var(--paper-2)', border: 'none' }}>
            <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 6 }}>
              <span className="hf-h4" style={{ fontSize: 13 }}>Likely resolution</span>
              <span className="hf-chip hf-chip-good" style={{ fontSize: 10 }}>★ Most return same-day</span>
            </div>
            <div className="hf-tiny hf-muted">Refund <b style={{ color: 'var(--ink)' }}>{money(44)}</b> for 2 of 4 cups · prepaid label included · expect a reply within 24h.</div>
          </div>
        </div>
      </div>
      {/* footer cta */}
      <div style={{ position: 'absolute', bottom: 0, left: 0, right: 0, padding: '12px 20px 24px', background: 'var(--paper)', borderTop: '1px solid var(--line)' }}>
        <button className="hf-btn hf-btn-primary" style={{ width: '100%', height: 44 }}>Send return request →</button>
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 06 · LEAVE A REVIEW
// ──────────────────────────────────────────────────────────────────────
function MFlow_Review() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 110 }}>
        <MTop title="Review your order" />
        <div className="hf-px-5">
          {/* delivered banner */}
          <div className="hf-card" style={{ padding: 14, background: 'var(--ink)', color: 'var(--paper)', border: 'none', marginBottom: 18 }}>
            <div className="hf-tiny" style={{ opacity: 0.7, letterSpacing: 0.06, textTransform: 'uppercase' }}>Delivered Apr 7</div>
            <div className="hf-h3" style={{ color: 'var(--paper)', marginTop: 4 }}>How was the Persimmon vase?</div>
          </div>

          {/* product */}
          <div className="hf-flex hf-items-center hf-gap-3" style={{ marginBottom: 18 }}>
            <ProdImg tone="clay" h={64} r={8} />
            <div className="hf-grow">
              <div className="hf-h4" style={{ fontSize: 13.5 }}>Persimmon vase</div>
              <div className="hf-tiny hf-muted">Mira Studio · Oakland</div>
            </div>
          </div>

          {/* rating stars */}
          <div className="hf-flex hf-center hf-gap-2" style={{ marginBottom: 6 }}>
            {[1,2,3,4,5].map((s, i) => (
              <span key={s} className="hf-center" style={{ width: 40, height: 40, color: i < 5 ? '#E8A53B' : 'var(--ink-4)' }}>
                <svg width="32" height="32" viewBox="0 0 24 24" fill={i < 5 ? '#E8A53B' : 'none'} stroke={i < 5 ? '#E8A53B' : 'var(--ink-4)'} strokeWidth="1.5"><polygon points="12 2 15 9 22 9.3 16.5 13.8 18.5 21 12 17 5.5 21 7.5 13.8 2 9.3 9 9" /></svg>
              </span>
            ))}
          </div>
          <div className="hf-tiny hf-muted" style={{ textAlign: 'center', marginBottom: 18 }}>5 of 5 · Loved it</div>

          {/* tag chips */}
          <div className="hf-eyebrow" style={{ marginBottom: 8 }}>What stood out?</div>
          <div className="hf-flex hf-gap-1" style={{ flexWrap: 'wrap', marginBottom: 18 }}>
            {[
              { l: 'Beautiful in person', on: true },
              { l: 'Well-packaged', on: true },
              { l: 'Fast shipping', on: false },
              { l: 'As described', on: true },
              { l: 'Heavier than expected', on: false },
              { l: 'Color slightly different', on: false },
            ].map(c => (
              <span key={c.l} className={`hf-chip ${c.on ? 'hf-chip-on' : ''}`} style={{ fontSize: 11 }}>{c.l}</span>
            ))}
          </div>

          {/* review body */}
          <div className="hf-eyebrow" style={{ marginBottom: 8 }}>Tell us more (optional)</div>
          <div style={{ minHeight: 110, border: '1px solid var(--line)', borderRadius: 10, padding: 12 }}>
            <p className="hf-small" style={{ lineHeight: 1.55 }}>The asymmetry on the rim is even better in person. Sat on my desk for a week before I dared put a flower in it.</p>
            <span style={{ width: 1.5, height: 14, background: 'var(--ink)', display: 'inline-block', verticalAlign: 'middle', animation: 'hf-blink 1s steps(1) infinite' }} />
          </div>

          {/* photo */}
          <div className="hf-eyebrow" style={{ marginTop: 18, marginBottom: 8 }}>Add a photo</div>
          <div className="hf-flex hf-gap-2">
            <ProdImg tone="clay" h={88} r={8} />
            <button className="hf-card hf-center" style={{ width: 88, height: 88, padding: 0, border: '1.5px dashed var(--ink-4)', background: 'transparent', color: 'var(--ink-3)', flexDirection: 'column', gap: 4 }}>
              <Ico n="plus" s={16} />
              <span className="hf-tiny">Add</span>
            </button>
          </div>

          {/* publish toggle */}
          <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: 14, border: '1px solid var(--line)', borderRadius: 10, marginTop: 18 }}>
            <span className="hf-switch hf-switch-on" />
            <div className="hf-grow">
              <div className="hf-h4" style={{ fontSize: 13 }}>Show on Mira's shop</div>
              <div className="hf-tiny hf-muted">Posted as "Mira K." · helps small shops</div>
            </div>
          </div>
        </div>
      </div>
      <div style={{ position: 'absolute', bottom: 0, left: 0, right: 0, padding: '12px 20px 24px', background: 'var(--paper)', borderTop: '1px solid var(--line)' }}>
        <button className="hf-btn hf-btn-primary" style={{ width: '100%', height: 44 }}>Post review</button>
        <div className="hf-tiny hf-muted" style={{ textAlign: 'center', marginTop: 6 }}>Skip for now</div>
      </div>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 07 · NOTIFICATIONS
// ──────────────────────────────────────────────────────────────────────
function MFlow_Notifs() {
  const groups = [
    { d: 'Today', items: [
      { i: 'truck',  c: 'var(--terra)',  t: 'Out for delivery',     s: 'Persimmon vase · arriving by 8 PM',           tm: '12m', un: true, tone: 'warn' },
      { i: 'tag',    c: 'var(--primary)', t: 'New code · STUDIO15', s: 'Mira Studio is offering 15% off this week',   tm: '2h',  un: true },
      { i: 'star',   c: 'var(--ink)',    t: 'Field & Hand restocked', s: 'The storm bowl is back · 4 in stock',       tm: '6h',  un: true },
    ]},
    { d: 'Yesterday', items: [
      { i: 'check',  c: 'var(--good)',   t: 'Delivered',            s: 'Indigo carafe · enjoy',                       tm: '1d' },
      { i: 'chat',   c: 'var(--ink-2)',  t: 'Mira sent a thank-you', s: '"This means a lot — your second order…"',    tm: '1d' },
    ]},
    { d: 'This week', items: [
      { i: 'box',    c: 'var(--ink-2)',  t: 'Order packed',         s: '#1042 · Persimmon vase',                      tm: '3d' },
      { i: 'heart',  c: 'var(--ink-2)',  t: 'Saved item back in stock', s: 'Cream tumbler set · 6 in stock',          tm: '4d' },
    ]},
  ];
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body" style={{ overflow: 'auto', paddingBottom: 80 }}>
        <MTop title="Notifications" right={<button className="hf-icon-btn"><Ico n="cog" s={15} /></button>} />

        {/* tabs */}
        <div className="hf-flex hf-gap-1 hf-px-5" style={{ paddingBottom: 14 }}>
          {[
            { l: 'All', n: 8, on: true },
            { l: 'Orders', n: 4 },
            { l: 'Shops', n: 3 },
            { l: 'Promos', n: 1 },
          ].map(t => (
            <span key={t.l} className={`hf-chip ${t.on ? 'hf-chip-on' : ''}`} style={{ fontSize: 11.5, height: 30 }}>
              {t.l} <span className="hf-num" style={{ marginLeft: 4, fontSize: 10, padding: '1px 5px', borderRadius: 999, background: t.on ? 'rgba(255,255,255,0.18)' : 'var(--paper-2)' }}>{t.n}</span>
            </span>
          ))}
        </div>

        {groups.map((g) => (
          <div key={g.d}>
            <div className="hf-eyebrow hf-px-5" style={{ paddingBottom: 8 }}>{g.d}</div>
            {g.items.map((n, i) => (
              <div key={i} className="hf-flex hf-items-start hf-gap-3 hf-px-5" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)', background: n.un ? 'rgba(0,102,204,0.04)' : 'transparent' }}>
                <span className="hf-center" style={{ width: 36, height: 36, borderRadius: 10, background: 'var(--paper-2)', color: n.c, flexShrink: 0 }}>
                  <Ico n={n.i} s={16} />
                </span>
                <div className="hf-grow">
                  <div className="hf-flex hf-between">
                    <span className="hf-h4" style={{ fontSize: 13 }}>{n.t}</span>
                    <span className="hf-tiny hf-muted">{n.tm}</span>
                  </div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 2, lineHeight: 1.4 }}>{n.s}</div>
                  {n.tone === 'warn' && (
                    <div className="hf-flex hf-gap-2" style={{ marginTop: 8 }}>
                      <button className="hf-btn hf-btn-outline hf-btn-sm" style={{ height: 26, padding: '0 10px', fontSize: 11 }}>Track →</button>
                    </div>
                  )}
                </div>
                {n.un && <span style={{ width: 7, height: 7, borderRadius: 999, background: 'var(--primary)', flexShrink: 0, marginTop: 6 }} />}
              </div>
            ))}
          </div>
        ))}

        {/* settings link */}
        <div className="hf-px-5" style={{ marginTop: 18 }}>
          <a className="hf-tiny" style={{ color: 'var(--primary)' }}>Manage notifications →</a>
        </div>
      </div>
      <PhoneHome />
    </div>
  );
}

Object.assign(window, {
  MFlow_Search, MFlow_Account, MFlow_Tracking, MFlow_Addresses, MFlow_Return, MFlow_Review, MFlow_Notifs,
});

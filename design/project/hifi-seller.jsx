// hifi-seller.jsx — 3 desktop seller dashboards (1280x800)

// ── Shared seller chrome
function SellerSidebar({ active = 'Overview' }) {
  const items = [
  { l: 'Overview', i: 'home' },
  { l: 'Orders', i: 'inbox', n: 4 },
  { l: 'Listings', i: 'pkg' },
  { l: 'Analytics', i: 'chart' },
  { l: 'Customers', i: 'user' },
  { l: 'Discounts', i: 'tag' },
  { l: 'Storefront', i: 'shop' }];

  return (
    <div className="hf-sidebar" style={{ width: 220, padding: '20px 14px' }}>
      <div className="hf-flex hf-items-center hf-gap-2" style={{ padding: '0 6px 18px' }}>
        <div className="hf-center" style={{ width: 28, height: 28, borderRadius: 8, background: 'var(--ink)', color: 'var(--paper)' }}>
          <span className="hf-display" style={{ fontSize: 18, lineHeight: 1 }}>m</span>
        </div>
        <div>
          <div className="hf-h4" style={{ fontSize: 13 }}>Mira Studio</div>
          <div className="hf-tiny hf-muted">Plan · Maker</div>
        </div>
      </div>
      {items.map((it) =>
      <div key={it.l} className={`hf-nav-item ${it.l === active ? 'hf-nav-item-on' : ''}`}>
          <Ico n={it.i} s={14} />
          <span className="hf-grow">{it.l}</span>
          {it.n && <span className="hf-num" style={{ fontSize: 11, padding: '1px 6px', borderRadius: 999, background: it.l === active ? 'rgba(255,255,255,0.18)' : 'var(--terra)', color: it.l === active ? 'white' : 'white', fontWeight: 600 }}>{it.n}</span>}
        </div>
      )}
      <div className="hf-grow" />
      <div className="hf-card" style={{ padding: 12, background: 'var(--paper-2)', border: 'none' }}>
        <div className="hf-h4" style={{ marginBottom: 4 }}>Setup · 4 of 6</div>
        <div className="hf-progress" style={{ marginBottom: 8 }}><i style={{ width: '66%' }} /></div>
        <div className="hf-tiny hf-muted">Add payouts &amp; ship rates</div>
      </div>
    </div>);

}

function SellerTopbar({ title, subtitle, actions }) {
  return (
    <div className="hf-flex hf-between hf-items-center" style={{ padding: '20px 28px', borderBottom: '1px solid var(--line)', background: 'var(--paper)' }}>
      <div>
        <div className="hf-eyebrow" style={{ marginBottom: 4 }}>{subtitle}</div>
        <h1 className="hf-display" style={{ fontSize: 30, lineHeight: 1 }}>{title}</h1>
      </div>
      <div className="hf-flex hf-items-center hf-gap-2">
        {actions}
        <button className="hf-icon-btn"><Ico n="bell" s={15} /></button>
        <Avatar name="Mira" size="sm" />
      </div>
    </div>);

}

// ── A · Overview · stat-driven (default seller home)
function Seller_Overview() {
  const orders = [
  { id: '#1042', name: 'Sasha L.', items: 'Persimmon vase + 1', total: 152, status: 'New', tone: 'warn' },
  { id: '#1041', name: 'Devon T.', items: 'Forest bowl, lg.', total: 64, status: 'Paid', tone: 'good' },
  { id: '#1040', name: 'Ari K.', items: 'Cream tumbler set', total: 48, status: 'Paid', tone: 'good' },
  { id: '#1039', name: 'June P.', items: 'Indigo carafe', total: 110, status: 'Shipped', tone: 'mute' },
  { id: '#1038', name: 'Theo R.', items: 'Soft hand vessel + 2', total: 218, status: 'Shipped', tone: 'mute' }];

  const Stat = ({ l, v, d, t = 'up', spark }) =>
  <div className="hf-card" style={{ padding: 18 }}>
      <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>{l}</div>
      <div className="hf-flex hf-between hf-items-end">
        <div className="hf-display-2 hf-num" style={{ fontSize: 30, lineHeight: 1 }}>{v}</div>
        <span className={`hf-tiny`} style={{ color: t === 'up' ? 'var(--good)' : 'var(--bad)', fontWeight: 600, letterSpacing: 0 }}>
          {t === 'up' ? '↑' : '↓'} {d}
        </span>
      </div>
      <div style={{ height: 36, marginTop: 10, color: 'var(--ink-3)' }}>
        <Spark data={spark} color="var(--ink-3)" fill="rgba(21,18,14,0.05)" />
      </div>
    </div>;


  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Good morning, Mira"
          subtitle="Tuesday · April 8"
          actions={<><button className="hf-btn hf-btn-outline"><Ico n="upload" s={12} /> Export</button><button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New listing</button></>} />
        
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* stat row */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 20 }} data-comment-anchor="2ccfe8643d-div-93-11">
            <Stat l="Revenue · 7 days" v={money(4280)} d="12%" spark={[0.4, 0.5, 0.45, 0.6, 0.55, 0.7, 0.85]} />
            <Stat l="Orders · 7 days" v="38" d="7%" spark={[0.5, 0.6, 0.55, 0.5, 0.7, 0.65, 0.8]} />
            <Stat l="Avg. order" v={money(112)} d="2%" t="down" spark={[0.6, 0.55, 0.7, 0.5, 0.4, 0.55, 0.5]} />
            <Stat l="Storefront views" v="2.1k" d="24%" spark={[0.3, 0.45, 0.4, 0.55, 0.6, 0.75, 0.9]} />
          </div>

          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.6fr 1fr' }}>
            {/* Revenue chart */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-flex hf-between hf-items-start" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-tiny hf-muted">Revenue</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 26, marginTop: 2 }}>{money(4280)}</div>
                </div>
                <div className="hf-flex hf-gap-1">
                  {['7d', '30d', '90d', 'Year'].map((p, i) =>
                  <span key={p} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11 }}>{p}</span>
                  )}
                </div>
              </div>
              <div style={{ height: 200 }}>
                <AreaChart data={[120, 280, 220, 340, 410, 380, 520, 480, 540, 620, 590, 720, 680, 780]} color="var(--ink)" fill="rgba(21,18,14,0.06)" />
              </div>
              <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
                <span>Apr 1</span><span>Apr 4</span><span>Apr 7</span><span>Apr 10</span><span>Apr 14</span>
              </div>
            </div>

            {/* Tasks */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 12 }}>Today</div>
              <div className="hf-col hf-gap-3">
                {[
                { l: 'Pack 2 orders ready to ship', s: '#1042 · #1041', t: 'warn' },
                { l: 'Reply to 3 messages', s: 'Sasha asked about glaze', t: 'good' },
                { l: 'Restock: Persimmon vase', s: 'Out of stock — 4 in pre-order', t: 'bad' },
                { l: 'Review draft: Forest bowl Sm.', s: 'Last edited 2 days ago', t: 'mute' }].
                map((t, i) =>
                <div key={i} className="hf-flex hf-items-start hf-gap-3">
                    <span className={`hf-dot hf-dot-${t.t}`} style={{ marginTop: 7, flexShrink: 0 }} />
                    <div className="hf-grow">
                      <div className="hf-h4">{t.l}</div>
                      <div className="hf-tiny hf-muted">{t.s}</div>
                    </div>
                    <Ico n="chevR" s={13} />
                  </div>
                )}
              </div>
            </div>
          </div>

          {/* Recent orders */}
          <div className="hf-card" style={{ marginTop: 20, padding: 0 }}>
            <div className="hf-flex hf-between" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
              <div className="hf-h3">Recent orders</div>
              <a className="hf-small" style={{ color: 'var(--ink-2)', fontWeight: 500 }}>All orders →</a>
            </div>
            <table className="hf-table">
              <thead><tr><th>Order</th><th>Customer</th><th>Items</th><th>Total</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {orders.map((o) =>
                <tr key={o.id}>
                    <td className="hf-mono" style={{ color: 'var(--ink)', fontWeight: 500 }}>{o.id}</td>
                    <td><div className="hf-flex hf-items-center hf-gap-2"><Avatar name={o.name} size="sm" /><span>{o.name}</span></div></td>
                    <td className="hf-muted">{o.items}</td>
                    <td className="hf-num" style={{ color: 'var(--ink)', fontWeight: 500 }}>{money(o.total)}</td>
                    <td><span className={`hf-chip hf-chip-${o.tone === 'good' ? 'good' : o.tone === 'warn' ? 'warn' : 'soft'}`} style={{ fontSize: 11 }}>{o.status}</span></td>
                    <td><Ico n="chevR" s={13} /></td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>data-led overview</Anno>
    </div>);

}

// ── B · Inbox-style — orders front and center
function Seller_Inbox() {
  const list = [
  { id: '#1042', name: 'Sasha L.', sub: 'Persimmon vase + 1 · $152', t: '2m', unread: true, on: true },
  { id: '#1041', name: 'Devon T.', sub: 'Forest bowl, lg. · $64', t: '14m', unread: true },
  { id: '#1040', name: 'Ari K.', sub: 'Cream tumbler set · $48', t: '1h' },
  { id: '#1039', name: 'June P.', sub: 'Indigo carafe · $110', t: '3h' },
  { id: '#1038', name: 'Theo R.', sub: 'Soft hand vessel + 2 · $218', t: 'Mon' },
  { id: '#1037', name: 'Lou H.', sub: 'Bone dinner plate × 4 · $152', t: 'Mon' },
  { id: '#1036', name: 'Ines M.', sub: 'Rust mug Nº 04 × 2 · $64', t: 'Sun' }];

  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Orders" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 24px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <h2 className="hf-h2" style={{ fontSize: 18 }}>Orders</h2>
            <div className="hf-flex hf-gap-1">
              {['All · 38', 'New · 4', 'Pack', 'Ship', 'Done'].map((c, i) =>
              <span key={c} className={`hf-chip ${i === 1 ? 'hf-chip-on' : ''}`}>{c}</span>
              )}
            </div>
          </div>
          <div className="hf-flex hf-items-center hf-gap-2">
            <div className="hf-flex hf-items-center hf-gap-2 hf-px-3" style={{ height: 30, background: 'var(--paper-2)', borderRadius: 999, color: 'var(--ink-3)' }}>
              <Ico n="search" s={12} /><span className="hf-small">Search orders…</span>
            </div>
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="filter" s={11} /> Filter</button>
          </div>
        </div>

        {/* split: list + detail */}
        <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
          {/* list */}
          <div style={{ width: 340, borderRight: '1px solid var(--line)', overflow: 'auto', flexShrink: 0 }}>
            {list.map((o) =>
            <div key={o.id} className="hf-flex hf-gap-3" style={{ padding: '12px 16px', borderBottom: '1px solid var(--line)', background: o.on ? 'var(--paper-2)' : 'transparent', borderLeft: o.on ? '2px solid var(--ink)' : '2px solid transparent' }}>
                <Avatar name={o.name} />
                <div className="hf-grow" style={{ minWidth: 0 }}>
                  <div className="hf-flex hf-between hf-items-center">
                    <span className="hf-h4">{o.name}</span>
                    <span className="hf-tiny hf-muted">{o.t}</span>
                  </div>
                  <div className="hf-flex hf-items-center hf-gap-1">
                    <span className="hf-mono" style={{ color: 'var(--ink-3)' }}>{o.id}</span>
                    {o.unread && <span className="hf-dot hf-dot-warn" style={{ marginLeft: 2 }} />}
                  </div>
                  <div className="hf-tiny hf-muted" style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{o.sub}</div>
                </div>
              </div>
            )}
          </div>

          {/* detail */}
          <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
            <div className="hf-flex hf-between hf-items-start" style={{ marginBottom: 18 }}>
              <div>
                <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 6 }}>
                  <span className="hf-mono" style={{ color: 'var(--ink-3)' }}>#1042</span>
                  <span className="hf-chip hf-chip-warn" style={{ fontSize: 11 }}><span className="hf-dot hf-dot-warn" /> Needs packing</span>
                </div>
                <h2 className="hf-display" style={{ fontSize: 26, lineHeight: 1 }}>Sasha Leblanc · 2 items</h2>
                <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>Placed 12 min ago · Express shipping</div>
              </div>
              <div className="hf-flex hf-gap-2">
                <button className="hf-btn hf-btn-outline"><Ico n="chat" s={12} /> Message</button>
                <button className="hf-btn hf-btn-primary"><Ico n="check" s={12} /> Mark packed</button>
              </div>
            </div>

            <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.6fr 1fr' }}>
              <div className="hf-card" style={{ padding: 0 }}>
                <div className="hf-flex hf-between" style={{ padding: '12px 18px', borderBottom: '1px solid var(--line)' }}>
                  <span className="hf-h4">Items</span>
                  <span className="hf-tiny hf-muted">Subtotal {money(140)}</span>
                </div>
                {[
                { t: 'Persimmon vase', s: 'Glazed terra · qty 1', p: 86, tone: 'clay' },
                { t: 'Forest bowl, lg.', s: 'Matte sage · qty 1', p: 64, tone: 'sage' }].
                map((it, i) =>
                <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '14px 18px', borderBottom: i === 0 ? '1px solid var(--line)' : 'none' }}>
                    <ProdImg tone={it.tone} h={48} r={8} />
                    <div className="hf-grow">
                      <div className="hf-h4">{it.t}</div>
                      <div className="hf-tiny hf-muted">{it.s}</div>
                    </div>
                    <span className="hf-num hf-h4">{money(it.p)}</span>
                  </div>
                )}
                <div className="hf-col hf-gap-2" style={{ padding: '14px 18px' }}>
                  <div className="hf-flex hf-between hf-small"><span className="hf-muted">Shipping · Express</span><span className="hf-num">{money(12)}</span></div>
                  <div className="hf-flex hf-between hf-small"><span className="hf-muted">Tax</span><span className="hf-num">{money(0)}</span></div>
                  <div className="hf-divider" style={{ margin: '6px 0' }} />
                  <div className="hf-flex hf-between"><span className="hf-h4">Total</span><span className="hf-display-2 hf-num" style={{ fontSize: 18 }}>{money(152)}</span></div>
                </div>
              </div>

              <div className="hf-col hf-gap-3">
                <div className="hf-card" style={{ padding: 16 }}>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Ship to</div>
                  <div className="hf-h4">Sasha Leblanc</div>
                  <div className="hf-small hf-muted" style={{ marginTop: 2 }}>820 Sutter St · #4B<br />San Francisco, CA 94109</div>
                </div>
                <div className="hf-card" style={{ padding: 16 }}>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 8 }}>Timeline</div>
                  <div className="hf-col hf-gap-2">
                    {[
                    { t: 'Order placed', s: '12 min ago', on: true },
                    { t: 'Payment captured', s: '12 min ago', on: true },
                    { t: 'Packed', s: '—' },
                    { t: 'Shipped', s: '—' }].
                    map((s, i) =>
                    <div key={i} className="hf-flex hf-items-center hf-gap-2">
                        <span className="hf-dot" style={{ background: s.on ? 'var(--good)' : 'var(--ink-5)', width: 8, height: 8 }} />
                        <span className="hf-small" style={{ color: s.on ? 'var(--ink)' : 'var(--ink-4)', fontWeight: s.on ? 500 : 400 }}>{s.t}</span>
                        <span className="hf-grow" />
                        <span className="hf-tiny hf-muted">{s.s}</span>
                      </div>
                    )}
                  </div>
                </div>
                <div className="hf-card" style={{ padding: 16, background: 'var(--paper-2)', border: 'none' }}>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Customer note</div>
                  <div className="hf-small">"Gift — could you wrap in twine and skip the invoice slip? Thanks!"</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>orders as inbox</Anno>
    </div>);

}

// ── C · Editorial — story-led, less-is-more
function Seller_Editorial() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Overview" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden', background: 'var(--paper)' }}>
        <div className="hf-flex hf-between" style={{ padding: '24px 40px 8px', alignItems: 'baseline' }}>
          <div className="hf-eyebrow">Tuesday, April 8</div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-ghost hf-btn-sm">Today</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm">This week</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ color: 'var(--ink)', fontWeight: 600 }}>This month</button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '0 40px 40px' }}>
          <h1 className="hf-display" style={{ fontSize: 96, lineHeight: 0.95, letterSpacing: '-0.03em', margin: '8px 0 28px', maxWidth: 900 }}>
            Things have been <i>moving</i>.
          </h1>

          <div className="hf-grid hf-gap-6" style={{ gridTemplateColumns: '1fr 1fr 1fr', marginBottom: 36 }}>
            {[
            { l: 'In sales', v: money(4280), s: '38 orders · up 12%' },
            { l: 'Pieces shipped', v: '46', s: '6 awaiting pickup' },
            { l: 'New followers', v: '128', s: 'across 5 cities' }].
            map((s, i) =>
            <div key={i} style={{ borderTop: '1px solid var(--ink)', paddingTop: 14 }}>
                <div className="hf-tiny hf-muted">{s.l}</div>
                <div className="hf-display-2 hf-num" style={{ fontSize: 44, lineHeight: 1, marginTop: 6 }}>{s.v}</div>
                <div className="hf-small hf-muted" style={{ marginTop: 4 }}>{s.s}</div>
              </div>
            )}
          </div>

          {/* Two-column: chart + best seller */}
          <div className="hf-grid hf-gap-6" style={{ gridTemplateColumns: '1.5fr 1fr', alignItems: 'start' }}>
            <div>
              <div className="hf-flex hf-between" style={{ marginBottom: 12, alignItems: 'baseline' }}>
                <h3 className="hf-display" style={{ fontSize: 28 }}>The shape of the month</h3>
                <span className="hf-small hf-muted">Daily revenue, $</span>
              </div>
              <div style={{ height: 180 }}>
                <AreaChart data={[80, 150, 90, 200, 260, 180, 320, 260, 340, 420, 360, 500, 420, 560, 500, 640, 580, 720, 640, 800]} color="var(--terra)" fill="rgba(194,65,12,0.08)" />
              </div>
              <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
                <span>Apr 1</span><span>Apr 8</span><span>Apr 15</span><span>Apr 22</span><span>Apr 30</span>
              </div>
            </div>

            <div>
              <div className="hf-eyebrow" style={{ marginBottom: 8 }}>Bestseller this month</div>
              <ProdImg tone="clay" h={220} />
              <h3 className="hf-display" style={{ fontSize: 28, marginTop: 14 }}>Persimmon vase</h3>
              <div className="hf-flex hf-gap-3 hf-small hf-muted" style={{ marginTop: 4 }}>
                <span>14 sold</span>
                <span className="hf-bullet" style={{ marginTop: 7 }} />
                <span className="hf-num" style={{ color: 'var(--good)', fontWeight: 600 }}>{money(1204)}</span>
                <span className="hf-bullet" style={{ marginTop: 7 }} />
                <span>4 in pre-order</span>
              </div>
            </div>
          </div>

          {/* Footnote: inventory alerts */}
          <div style={{ marginTop: 36, paddingTop: 18, borderTop: '1px solid var(--line)' }}>
            <div className="hf-flex hf-between" style={{ marginBottom: 14 }}>
              <h3 className="hf-h3">Three things to know</h3>
              <a className="hf-small" style={{ fontWeight: 500 }}>See all 7 →</a>
            </div>
            <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr 1fr' }}>
              {[
              { l: 'Persimmon vase', s: 'Out of stock — 4 in pre-order. Restock?', t: 'Inventory', tone: 'warn' },
              { l: '#1042 · Sasha L.', s: 'Express order placed 12 min ago. Pack today.', t: 'Order', tone: 'good' },
              { l: 'Spring \'26 collection', s: '3 of 8 listings still in draft.', t: 'Listings', tone: 'mute' }].
              map((c, i) =>
              <div key={i} className="hf-card" style={{ padding: 16 }}>
                  <div className={`hf-tiny`} style={{ color: c.tone === 'warn' ? 'var(--warn)' : c.tone === 'good' ? 'var(--good)' : 'var(--ink-3)', fontWeight: 600, letterSpacing: 0.08, textTransform: 'uppercase', marginBottom: 6 }}>{c.t}</div>
                  <div className="hf-h4" style={{ marginBottom: 4 }}>{c.l}</div>
                  <div className="hf-small hf-muted">{c.s}</div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>editorial · less-is-more</Anno>
    </div>);

}

Object.assign(window, { Seller_Overview, Seller_Inbox, Seller_Editorial, SellerSidebar, SellerTopbar });
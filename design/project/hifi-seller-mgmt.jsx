// hifi-seller-mgmt.jsx — Seller operations gap fills
// Orders inbox · Order detail w/ refund + partial fulfillment · Promotions · Marketing email composer
// All 1280×800 desktop. Reuses SellerSidebar / SellerTopbar from hifi-seller.jsx.

// ──────────────────────────────────────────────────────────────────────
// 01 · ORDERS INBOX — full list, filters, bulk actions
// ──────────────────────────────────────────────────────────────────────
function SMgmt_OrdersInbox() {
  const orders = [
    { id: '#1042', d: 'Today · 2:14 PM', name: 'Sasha L.',  city: 'San Francisco, CA', items: 'Persimmon vase, Ash budstem', qty: 2, total: 152, ship: 'USPS Priority', status: 'New',          tone: 'warn',  age: '2h',  starred: true },
    { id: '#1041', d: 'Today · 11:08 AM', name: 'Devon T.', city: 'Brooklyn, NY',     items: 'Forest bowl, lg.',              qty: 1, total: 64,  ship: 'USPS Ground',   status: 'New',          tone: 'warn',  age: '5h' },
    { id: '#1040', d: 'Today · 9:41 AM',  name: 'Ari K.',   city: 'Portland, OR',     items: 'Cream tumbler set',             qty: 1, total: 48,  ship: 'USPS Ground',   status: 'New',          tone: 'warn',  age: '7h' },
    { id: '#1039', d: 'Yesterday',        name: 'June P.',  city: 'Seattle, WA',      items: 'Indigo carafe',                 qty: 1, total: 110, ship: 'UPS Ground',    status: 'Packed',       tone: 'mute',  age: '1d' },
    { id: '#1038', d: '2 days ago',       name: 'Theo R.',  city: 'Austin, TX',       items: 'Soft hand vessel +2',           qty: 3, total: 218, ship: 'USPS Priority', status: 'Shipped',      tone: 'mute',  age: '2d' },
    { id: '#1037', d: '3 days ago',       name: 'Liu W.',   city: 'Vancouver, BC',    items: 'Ceremony bowl',                 qty: 1, total: 142, ship: 'USPS Intl',     status: 'Shipped',      tone: 'mute',  age: '3d' },
    { id: '#1036', d: '4 days ago',       name: 'Marisol G.', city: 'Mexico City, MX', items: 'Field cup × 4',                 qty: 4, total: 88,  ship: 'USPS Intl',     status: 'Refund req.',  tone: 'bad',   age: '4d' },
    { id: '#1035', d: '5 days ago',       name: 'Sam D.',   city: 'Chicago, IL',      items: 'Storm bowl',                    qty: 1, total: 58,  ship: 'USPS Ground',   status: 'Delivered',    tone: 'good',  age: '5d' },
    { id: '#1034', d: '6 days ago',       name: 'Hana R.',  city: 'Oakland, CA',      items: 'Earth tumbler ×2',              qty: 2, total: 56,  ship: 'Local pickup',  status: 'Delivered',    tone: 'good',  age: '6d' },
    { id: '#1033', d: '1 week ago',       name: 'Paul N.',  city: 'Los Angeles, CA',  items: 'Linen vase wrap',               qty: 1, total: 18,  ship: 'USPS First',    status: 'Cancelled',    tone: 'mute',  age: '7d' },
  ];
  const tabs = [
    { l: 'All', n: 47 },
    { l: 'Needs action', n: 4, on: true },
    { l: 'Packed', n: 2 },
    { l: 'Shipped', n: 18 },
    { l: 'Delivered', n: 21 },
    { l: 'Refund / cancel', n: 2 },
  ];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Orders" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Orders"
          subtitle="47 lifetime · 4 need action"
          actions={<><button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="upload" s={11} /> Export CSV</button><button className="hf-btn hf-btn-primary hf-btn-sm"><Ico n="plus" s={12} /> Manual order</button></>}
        />

        {/* tabs */}
        <div className="hf-flex hf-items-end" style={{ padding: '0 28px', borderBottom: '1px solid var(--line)', gap: 22 }}>
          {tabs.map((t) => (
            <div key={t.l} className={`hf-tab ${t.on ? 'hf-tab-on' : ''}`}>
              <span className="hf-flex hf-items-center hf-gap-2">
                {t.l} <span className={`hf-num`} style={{ fontSize: 11, padding: '1px 6px', borderRadius: 999, background: t.on ? 'var(--ink)' : 'var(--paper-2)', color: t.on ? 'white' : 'var(--ink-3)', fontWeight: 600 }}>{t.n}</span>
              </span>
            </div>
          ))}
        </div>

        {/* search + filter row */}
        <div className="hf-flex hf-items-center" style={{ padding: '14px 28px', gap: 10, borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-2" style={{ flex: 1, maxWidth: 360, height: 32, padding: '0 12px', background: 'var(--paper-2)', borderRadius: 9999 }}>
            <Ico n="search" s={13} />
            <span className="hf-small hf-muted">Search by order, customer, SKU…</span>
          </div>
          {['Status', 'Ship method', 'Date · last 30d'].map((l) => (
            <span key={l} className="hf-chip" style={{ fontSize: 11.5, height: 30, padding: '0 12px' }}>{l} <Ico n="chevD" s={10} /></span>
          ))}
          <span className="hf-grow" />
          <button className="hf-btn hf-btn-ghost hf-btn-sm"><Ico n="filter" s={11} /> More filters</button>
        </div>

        {/* bulk action bar (selected) */}
        <div className="hf-flex hf-items-center hf-between" style={{ padding: '10px 28px', borderBottom: '1px solid var(--line)', background: 'rgba(0,102,204,0.06)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <span className="hf-h4" style={{ fontSize: 13, color: 'var(--primary)' }}>3 orders selected</span>
            <span className="hf-tiny hf-muted">— $264 total</span>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-outline hf-btn-sm">Print labels</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm">Mark packed</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm">Bulk message</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ color: 'var(--ink-3)' }}>Cancel</button>
          </div>
        </div>

        {/* table */}
        <div className="hf-grow" style={{ overflow: 'auto' }}>
          <table className="hf-table">
            <thead>
              <tr>
                <th style={{ width: 36, paddingLeft: 28 }}>
                  <span className="hf-center" style={{ width: 14, height: 14, borderRadius: 3, border: '1.5px solid var(--ink-4)', display: 'inline-flex' }} />
                </th>
                <th>Order</th>
                <th>Customer</th>
                <th>Items</th>
                <th>Ship</th>
                <th>Total</th>
                <th>Status</th>
                <th>Age</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {orders.map((o, i) => {
                const sel = i < 3;
                return (
                  <tr key={o.id} style={{ background: sel ? 'rgba(0,102,204,0.04)' : 'transparent' }}>
                    <td style={{ paddingLeft: 28 }}>
                      <span className="hf-center" style={{ width: 14, height: 14, borderRadius: 3, background: sel ? 'var(--ink)' : 'var(--paper)', border: sel ? 'none' : '1.5px solid var(--ink-4)', color: 'white', display: 'inline-flex' }}>
                        {sel && <Ico n="check" s={9} sw={2.6} />}
                      </span>
                    </td>
                    <td>
                      <div className="hf-flex hf-items-center hf-gap-2">
                        {o.starred && <span style={{ color: 'var(--ink)' }}><Ico n="star" s={11} sw={2} /></span>}
                        <div>
                          <div className="hf-mono" style={{ fontWeight: 600, color: 'var(--ink)' }}>{o.id}</div>
                          <div className="hf-tiny hf-muted">{o.d}</div>
                        </div>
                      </div>
                    </td>
                    <td>
                      <div className="hf-flex hf-items-center hf-gap-2">
                        <Avatar name={o.name} size="sm" />
                        <div>
                          <div className="hf-h4" style={{ fontSize: 12.5 }}>{o.name}</div>
                          <div className="hf-tiny hf-muted">{o.city}</div>
                        </div>
                      </div>
                    </td>
                    <td className="hf-muted" style={{ maxWidth: 220 }}>
                      <div className="hf-small" style={{ color: 'var(--ink-2)', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{o.items}</div>
                      <div className="hf-tiny hf-muted">{o.qty} item{o.qty > 1 ? 's' : ''}</div>
                    </td>
                    <td className="hf-tiny hf-muted">{o.ship}</td>
                    <td className="hf-num" style={{ color: 'var(--ink)', fontWeight: 600 }}>{money(o.total)}</td>
                    <td>
                      <span className={`hf-chip hf-chip-${o.tone === 'bad' ? 'bad' : o.tone === 'warn' ? 'warn' : o.tone === 'good' ? 'good' : 'soft'}`} style={{ fontSize: 11 }}>
                        <span className={`hf-dot hf-dot-${o.tone}`} />{o.status}
                      </span>
                    </td>
                    <td className="hf-tiny hf-muted hf-num">{o.age}</td>
                    <td><Ico n="chevR" s={13} /></td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        {/* footer pagination */}
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '12px 28px', borderTop: '1px solid var(--line)' }}>
          <span className="hf-tiny hf-muted">Showing 1 – 10 of 47</span>
          <div className="hf-flex hf-gap-1">
            <button className="hf-icon-btn"><Ico n="chevL" s={12} /></button>
            {['1','2','3','4','5'].map((n, i) => (
              <span key={n} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11, minWidth: 26, justifyContent: 'center' }}>{n}</span>
            ))}
            <button className="hf-icon-btn"><Ico n="chevR" s={12} /></button>
          </div>
        </div>
      </div>
      <DesktopAction>opens #1042 · partial fulfillment</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · ORDER DETAIL — refund + partial fulfillment + cancel
// ──────────────────────────────────────────────────────────────────────
function SMgmt_OrderDetail() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Orders" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        {/* breadcrumb header */}
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <button className="hf-icon-btn"><Ico n="chevL" s={14} /></button>
            <a className="hf-small hf-muted">Orders /</a>
            <span className="hf-mono" style={{ color: 'var(--ink)', fontWeight: 600 }}>#1042</span>
            <span className="hf-chip hf-chip-warn" style={{ fontSize: 11 }}><span className="hf-dot hf-dot-warn" /> Partially fulfilled</span>
            <span className="hf-tiny hf-muted">· Sasha L. · 2 hours ago</span>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="chat" s={11} /> Message</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm">Print slip</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ color: 'var(--bad)' }}><Ico n="x" s={11} /> Cancel order</button>
            <button className="hf-btn hf-btn-primary hf-btn-sm">Buy label · Ash budstem</button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '20px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.6fr 1fr', alignItems: 'start' }}>
            {/* LEFT — items, fulfillment, refund history, timeline */}
            <div className="hf-col hf-gap-3">
              {/* Fulfillments */}
              <div className="hf-card" style={{ padding: 0, overflow: 'hidden' }}>
                {/* fulfilled box 1 */}
                <div style={{ padding: '14px 18px', background: 'var(--paper-2)', borderBottom: '1px solid var(--line)' }}>
                  <div className="hf-flex hf-between hf-items-center">
                    <div className="hf-flex hf-items-center hf-gap-2">
                      <span className="hf-center" style={{ width: 22, height: 22, borderRadius: 999, background: 'var(--good)', color: 'white' }}><Ico n="check" s={11} sw={2.4} /></span>
                      <span className="hf-h4">Fulfillment 1 of 2 · shipped</span>
                    </div>
                    <span className="hf-tiny hf-muted hf-mono">USPS · 9405 5036 9930 0124 2317</span>
                  </div>
                </div>
                <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: '14px 18px' }}>
                  <ProdImg tone="clay" h={56} r={8} />
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 13.5 }}>Persimmon vase</div>
                    <div className="hf-tiny hf-muted">SKU PV-08 · qty 1 · $86.00</div>
                  </div>
                  <span className="hf-num hf-h4">{money(86)}</span>
                </div>

                {/* unfulfilled box 2 */}
                <div style={{ padding: '14px 18px', background: 'rgba(194,65,12,0.05)', borderTop: '1px solid var(--line)', borderBottom: '1px solid var(--line)' }}>
                  <div className="hf-flex hf-between hf-items-center">
                    <div className="hf-flex hf-items-center hf-gap-2">
                      <span className="hf-center" style={{ width: 22, height: 22, borderRadius: 999, background: 'var(--paper)', border: '1.5px solid var(--ink-4)' }} />
                      <span className="hf-h4">Fulfillment 2 of 2 · awaiting restock</span>
                    </div>
                    <button className="hf-btn hf-btn-outline hf-btn-sm">Buy label</button>
                  </div>
                </div>
                <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: '14px 18px' }}>
                  <ProdImg tone="rust" h={56} r={8} />
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 13.5 }}>Ash budstem</div>
                    <div className="hf-flex hf-gap-2 hf-items-center">
                      <span className="hf-tiny hf-muted">SKU AB-02 · qty 1 · $66.00</span>
                      <span className="hf-chip hf-chip-warn" style={{ fontSize: 10 }}>back in stock Tue</span>
                    </div>
                  </div>
                  <span className="hf-num hf-h4">{money(66)}</span>
                </div>
              </div>

              {/* Refund / partial refund block */}
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-flex hf-between" style={{ marginBottom: 12 }}>
                  <span className="hf-h3">Issue refund</span>
                  <span className="hf-tiny hf-muted">refundable: {money(152)} · across 2 items</span>
                </div>
                <div className="hf-col hf-gap-2">
                  {[
                    { l: 'Persimmon vase', q: 1, p: 86, on: false },
                    { l: 'Ash budstem',     q: 1, p: 66, on: true, partial: 30 },
                  ].map((r) => (
                    <div key={r.l} className="hf-flex hf-items-center" style={{ padding: 12, borderRadius: 10, gap: 12, border: r.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: r.on ? 'var(--paper-2)' : 'var(--paper)' }}>
                      <span className="hf-center" style={{ width: 16, height: 16, borderRadius: 4, background: r.on ? 'var(--ink)' : 'var(--paper)', border: r.on ? 'none' : '1.5px solid var(--ink-4)', color: 'white' }}>
                        {r.on && <Ico n="check" s={10} sw={2.6} />}
                      </span>
                      <div className="hf-grow">
                        <div className="hf-h4" style={{ fontSize: 13 }}>{r.l}</div>
                        <div className="hf-tiny hf-muted">qty {r.q} · paid {money(r.p)}</div>
                      </div>
                      {r.on ? (
                        <div className="hf-flex hf-items-center hf-gap-2">
                          <span className="hf-tiny hf-muted">refund</span>
                          <div style={{ height: 32, padding: '0 10px', border: '1.5px solid var(--ink)', borderRadius: 8, display: 'flex', alignItems: 'center', gap: 4 }}>
                            <span className="hf-muted hf-num">$</span><span className="hf-num hf-h4" style={{ fontSize: 13 }}>{r.partial.toFixed(2)}</span>
                          </div>
                          <span className="hf-tiny hf-muted">of {money(r.p)}</span>
                        </div>
                      ) : (
                        <span className="hf-tiny hf-muted">—</span>
                      )}
                    </div>
                  ))}
                </div>
                <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr', marginTop: 14 }}>
                  <div>
                    <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Reason</div>
                    <div className="hf-flex hf-between hf-items-center" style={{ height: 36, padding: '0 12px', border: '1px solid var(--line)', borderRadius: 8 }}>
                      <span className="hf-small">Item arrived chipped</span>
                      <Ico n="chevD" s={11} />
                    </div>
                  </div>
                  <div>
                    <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Restock?</div>
                    <div className="hf-flex hf-gap-1">
                      {['Yes', 'No', 'Damage'].map((o, i) => (
                        <span key={o} className={`hf-chip ${i === 2 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11.5 }}>{o}</span>
                      ))}
                    </div>
                  </div>
                </div>
                <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 14, padding: '12px 14px', background: 'var(--paper-2)', borderRadius: 8 }}>
                  <div>
                    <div className="hf-tiny hf-muted">Refund total · to Visa · 4421</div>
                    <div className="hf-h3 hf-num" style={{ marginTop: 2 }}>{money(30)}</div>
                  </div>
                  <button className="hf-btn hf-btn-primary">Issue refund</button>
                </div>
              </div>

              {/* Timeline */}
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h3" style={{ marginBottom: 14 }}>Timeline</div>
                {[
                  { ico: 'check', t: 'Order placed', s: '2 items · $152.00 paid via Visa · 4421', d: '2h ago', on: true },
                  { ico: 'box',   t: 'Persimmon vase packed', s: 'Box S · 1lb 4oz', d: '1h ago' },
                  { ico: 'truck', t: 'Persimmon vase shipped', s: 'USPS Priority · 1–3 days', d: '52m ago' },
                  { ico: 'chat',  t: 'Note from Sasha',   s: '"No rush on the budstem — ship together if it\u2019s faster!"', d: '14m ago' },
                  { ico: 'info',  t: 'Ash budstem oversold',  s: 'Restock arrives Tue · auto-fulfill on', d: '8m ago', tone: 'warn' },
                ].map((e, i, a) => (
                  <div key={i} className="hf-flex hf-items-start hf-gap-3" style={{ paddingBottom: i < a.length - 1 ? 14 : 0, position: 'relative' }}>
                    <div className="hf-col hf-items-center" style={{ width: 24, flexShrink: 0 }}>
                      <span className="hf-center" style={{ width: 24, height: 24, borderRadius: 999, background: e.tone === 'warn' ? 'var(--paper-2)' : e.on ? 'var(--ink)' : 'var(--paper-2)', color: e.on ? 'white' : 'var(--ink-2)' }}>
                        <Ico n={e.ico} s={12} />
                      </span>
                      {i < a.length - 1 && <span style={{ width: 1.5, flex: 1, background: 'var(--line)', minHeight: 18 }} />}
                    </div>
                    <div className="hf-grow" style={{ paddingTop: 1 }}>
                      <div className="hf-flex hf-between">
                        <span className="hf-h4" style={{ fontSize: 13 }}>{e.t}</span>
                        <span className="hf-tiny hf-muted">{e.d}</span>
                      </div>
                      <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{e.s}</div>
                    </div>
                  </div>
                ))}
              </div>
            </div>

            {/* RIGHT — customer + financial summary + actions */}
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 16 }}>
                <div className="hf-flex hf-items-center hf-gap-3" style={{ marginBottom: 12 }}>
                  <Avatar name="Sasha L" size="lg" />
                  <div>
                    <div className="hf-h4">Sasha Leblanc</div>
                    <div className="hf-tiny hf-muted">3rd order · $284 lifetime</div>
                  </div>
                  <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ marginLeft: 'auto' }}>Profile →</button>
                </div>
                <div className="hf-divider" style={{ marginBottom: 12 }} />
                <div className="hf-tiny hf-muted">Ship to</div>
                <div className="hf-small" style={{ marginTop: 2 }}>820 Sutter St · #4B<br />San Francisco, CA 94109</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 12 }}>Bill to · same as ship</div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 12 }}>Email</div>
                <div className="hf-small">sasha.l@gmail.com</div>
              </div>

              <div className="hf-card" style={{ padding: 16 }}>
                <div className="hf-h3" style={{ marginBottom: 12 }}>Summary</div>
                <div className="hf-col hf-gap-2 hf-small">
                  <div className="hf-flex hf-between"><span className="hf-muted">Subtotal · 2 items</span><span className="hf-num">{money(152)}</span></div>
                  <div className="hf-flex hf-between"><span className="hf-muted">Shipping</span><span className="hf-num">{money(0)}</span></div>
                  <div className="hf-flex hf-between"><span className="hf-muted">Tax</span><span className="hf-num">{money(0)}</span></div>
                  <div className="hf-divider" style={{ margin: '4px 0' }} />
                  <div className="hf-flex hf-between"><span className="hf-h4">Customer paid</span><span className="hf-display-2 hf-num" style={{ fontSize: 18 }}>{money(152)}</span></div>
                  <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}><span>Micro fee · 4%</span><span className="hf-num">−{money(6.08)}</span></div>
                  <div className="hf-flex hf-between hf-tiny hf-muted"><span>Shipping label · USPS</span><span className="hf-num">−{money(9.84)}</span></div>
                  <div className="hf-flex hf-between"><span className="hf-small">You'll receive</span><span className="hf-num hf-h4" style={{ color: 'var(--good)' }}>{money(136.08)}</span></div>
                </div>
              </div>

              <div className="hf-card" style={{ padding: 16 }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 8, letterSpacing: 0.06, textTransform: 'uppercase' }}>Internal note</div>
                <div className="hf-card-flat" style={{ padding: 10, fontSize: 12 }}>
                  Held until budstem restocks Tue. Sasha OK with split.
                </div>
                <div className="hf-flex hf-gap-1" style={{ marginTop: 10, flexWrap: 'wrap' }}>
                  {['VIP', 'Repeat buyer', 'Gift'].map((t, i) => (
                    <span key={t} className={`hf-chip ${i < 2 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11 }}>{t}</span>
                  ))}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>refund · partial selected</Anno>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · PROMOTIONS — Discount codes list + creation drawer
// ──────────────────────────────────────────────────────────────────────
function SMgmt_Promos() {
  const codes = [
    { c: 'SPRING20',    t: '20% off · sitewide',          v: 'Apr 1 → May 15', n: 142, r: 1842, status: 'Active',    tone: 'good' },
    { c: 'WELCOME10',   t: '$10 off · first order $40+',  v: 'Always · 1 per buyer', n: 38, r: 612, status: 'Active', tone: 'good' },
    { c: 'STUDIO15',    t: '15% off · followers only',    v: 'Apr 22 → May 06', n: 17, r: 286, status: 'Active', tone: 'good' },
    { c: 'BLOOM',       t: 'Free ship · $80+',            v: 'Mar 1 → Apr 12',  n: 84, r: 0,    status: 'Ended', tone: 'mute' },
    { c: 'FRIENDS',     t: '15% off · sitewide',          v: 'Drafted',         n: 0,  r: 0,    status: 'Draft', tone: 'mute' },
  ];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row', position: 'relative' }}>
      <SellerSidebar active="Discounts" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Discounts &amp; promotions"
          subtitle="3 active · $2,740 driven · 281 redemptions"
          actions={<button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New promotion</button>}
        />

        {/* Stat row */}
        <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', padding: '20px 28px 0' }}>
          {[
            { l: 'Driven revenue',  v: money(2740), s: 'Last 30 days', spark: [0.1,0.2,0.3,0.45,0.6,0.55,0.7,0.85] },
            { l: 'Redemptions',     v: '281',       s: '14% of orders', spark: [0.2,0.25,0.4,0.45,0.55,0.6,0.7,0.8] },
            { l: 'Avg. discount',   v: '$9.74',     s: 'per redemption', spark: [0.3,0.32,0.36,0.4,0.42,0.45,0.5,0.52] },
            { l: 'New buyers',      v: '38',        s: 'from WELCOME10', spark: [0,0.1,0.15,0.2,0.3,0.4,0.5,0.7] },
          ].map((s) => (
            <div key={s.l} className="hf-card" style={{ padding: 16 }}>
              <div className="hf-tiny hf-muted">{s.l}</div>
              <div className="hf-flex hf-between hf-items-end">
                <div className="hf-display-2 hf-num" style={{ fontSize: 26, lineHeight: 1, marginTop: 6 }}>{s.v}</div>
              </div>
              <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>{s.s}</div>
              <div style={{ height: 24, marginTop: 8, color: 'var(--primary)' }}>
                <Spark data={s.spark} color="var(--primary)" fill="rgba(0,102,204,0.08)" />
              </div>
            </div>
          ))}
        </div>

        {/* Tabs + table */}
        <div className="hf-flex hf-items-end" style={{ padding: '20px 28px 0', gap: 22 }}>
          {[{ l: 'Promotions', n: 5, on: true }, { l: 'Automatic', n: 1 }, { l: 'Gift cards', n: 0 }].map((t) => (
            <div key={t.l} className={`hf-tab ${t.on ? 'hf-tab-on' : ''}`}>
              <span className="hf-flex hf-items-center hf-gap-2">{t.l} <span className="hf-num" style={{ fontSize: 11, padding: '1px 6px', borderRadius: 999, background: t.on ? 'var(--ink)' : 'var(--paper-2)', color: t.on ? 'white' : 'var(--ink-3)', fontWeight: 600 }}>{t.n}</span></span>
            </div>
          ))}
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '0 28px 20px' }}>
          <div className="hf-card" style={{ padding: 0, marginTop: -1, borderTopLeftRadius: 0, borderTopRightRadius: 0 }}>
            <table className="hf-table">
              <thead><tr><th>Code</th><th>What it does</th><th>Redemptions</th><th>Driven revenue</th><th>Window</th><th>Status</th><th></th></tr></thead>
              <tbody>
                {codes.map((c, i) => (
                  <tr key={c.c} style={{ background: i === 2 ? 'rgba(0,102,204,0.04)' : 'transparent' }}>
                    <td>
                      <div className="hf-flex hf-items-center hf-gap-2">
                        <span className="hf-mono" style={{ padding: '4px 8px', background: 'var(--paper-2)', borderRadius: 6, fontWeight: 600, color: 'var(--ink)' }}>{c.c}</span>
                        <button className="hf-icon-btn" style={{ width: 24, height: 24 }}><Ico n="share" s={11} /></button>
                      </div>
                    </td>
                    <td>
                      <div className="hf-h4" style={{ fontSize: 12.5 }}>{c.t}</div>
                    </td>
                    <td className="hf-num">{c.n}</td>
                    <td className="hf-num" style={{ color: c.r > 0 ? 'var(--ink)' : 'var(--ink-4)', fontWeight: 600 }}>{c.r > 0 ? money(c.r) : '—'}</td>
                    <td className="hf-tiny hf-muted">{c.v}</td>
                    <td><span className={`hf-chip hf-chip-${c.tone === 'good' ? 'good' : 'soft'}`} style={{ fontSize: 11 }}><span className={`hf-dot hf-dot-${c.tone}`} />{c.status}</span></td>
                    <td>
                      <div className="hf-flex hf-gap-1">
                        <button className="hf-icon-btn" style={{ width: 26, height: 26 }}><Ico n="edit" s={11} /></button>
                        <button className="hf-icon-btn" style={{ width: 26, height: 26 }}><Ico n="chevR" s={12} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>

      {/* Drawer · new promotion */}
      <div style={{ position: 'absolute', top: 0, right: 0, bottom: 0, width: 460, background: 'var(--paper)', borderLeft: '1px solid var(--line)', boxShadow: '-12px 0 30px rgba(0,0,0,0.06)', zIndex: 30, display: 'flex', flexDirection: 'column' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '16px 22px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-h3">New promotion</div>
          <button className="hf-icon-btn"><Ico n="close" s={14} /></button>
        </div>
        <div className="hf-grow" style={{ overflow: 'auto', padding: 22 }}>
          <div className="hf-tiny hf-muted" style={{ letterSpacing: 0.06, textTransform: 'uppercase', marginBottom: 8 }}>Code</div>
          <div style={{ height: 44, padding: '0 14px', border: '1.5px solid var(--ink)', borderRadius: 10, display: 'flex', alignItems: 'center', gap: 8 }}>
            <span className="hf-mono hf-h4" style={{ fontSize: 15 }}>STUDIO15</span>
            <span className="hf-grow" />
            <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ height: 26, padding: '0 8px' }}><Ico n="refresh" s={11} /> Generate</button>
          </div>
          <div className="hf-tiny hf-muted" style={{ marginTop: 8 }}>Buyers will type or click this at checkout</div>

          <div className="hf-tiny hf-muted" style={{ letterSpacing: 0.06, textTransform: 'uppercase', margin: '20px 0 8px' }}>Discount</div>
          <div className="hf-flex" style={{ background: 'var(--paper-2)', borderRadius: 9999, padding: 3, gap: 0, width: 'fit-content' }}>
            {['% off', '$ off', 'Free shipping', 'BOGO'].map((t, i) => (
              <span key={t} className="hf-flex hf-items-center hf-center" style={{ padding: '6px 14px', borderRadius: 9999, fontSize: 12, fontWeight: i === 0 ? 600 : 400, background: i === 0 ? 'var(--paper)' : 'transparent', color: i === 0 ? 'var(--ink)' : 'var(--ink-3)', boxShadow: i === 0 ? '0 1px 2px rgba(0,0,0,0.06)' : 'none' }}>{t}</span>
            ))}
          </div>
          <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 14 }}>
            <div className="hf-flex hf-items-center" style={{ height: 56, padding: '0 18px', border: '1.5px solid var(--ink)', borderRadius: 10, gap: 6 }}>
              <span className="hf-display hf-num" style={{ fontSize: 32, lineHeight: 1 }}>15</span>
              <span className="hf-h4 hf-muted">%</span>
            </div>
            <span className="hf-small hf-muted">off the entire order</span>
          </div>

          <div className="hf-tiny hf-muted" style={{ letterSpacing: 0.06, textTransform: 'uppercase', margin: '22px 0 8px' }}>Who can use it</div>
          <div className="hf-col hf-gap-2">
            {[
              { l: 'Anyone with the code', s: 'Public · share on socials', on: false },
              { l: 'Followers only',        s: 'Auto-applied · 184 buyers eligible', on: true },
              { l: 'Specific customers',    s: 'Pick from your CRM', on: false },
            ].map((o) => (
              <div key={o.l} className="hf-flex hf-items-start" style={{ padding: 12, borderRadius: 10, gap: 10, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', background: o.on ? 'var(--paper-2)' : 'var(--paper)' }}>
                <span className="hf-center" style={{ width: 16, height: 16, borderRadius: 999, border: '1.5px solid var(--ink)', flexShrink: 0, marginTop: 2 }}>
                  {o.on && <span style={{ width: 8, height: 8, borderRadius: 999, background: 'var(--ink)' }} />}
                </span>
                <div>
                  <div className="hf-h4" style={{ fontSize: 13 }}>{o.l}</div>
                  <div className="hf-tiny hf-muted">{o.s}</div>
                </div>
              </div>
            ))}
          </div>

          <div className="hf-tiny hf-muted" style={{ letterSpacing: 0.06, textTransform: 'uppercase', margin: '22px 0 8px' }}>Limits</div>
          <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr' }}>
            <div className="hf-card" style={{ padding: 12 }}>
              <div className="hf-tiny hf-muted">Min. order</div>
              <div className="hf-h4 hf-num" style={{ marginTop: 2 }}>$40.00</div>
            </div>
            <div className="hf-card" style={{ padding: 12 }}>
              <div className="hf-tiny hf-muted">Per buyer</div>
              <div className="hf-h4 hf-num" style={{ marginTop: 2 }}>1 use</div>
            </div>
            <div className="hf-card" style={{ padding: 12 }}>
              <div className="hf-tiny hf-muted">Total uses</div>
              <div className="hf-h4 hf-num" style={{ marginTop: 2 }}>200</div>
            </div>
            <div className="hf-card" style={{ padding: 12 }}>
              <div className="hf-tiny hf-muted">Window</div>
              <div className="hf-h4" style={{ marginTop: 2, fontSize: 12.5 }}>Apr 22 → May 06</div>
            </div>
          </div>

          <div className="hf-card" style={{ padding: 14, marginTop: 18, background: 'var(--paper-2)', border: 'none' }}>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 4 }}>
              <span className="hf-dot hf-dot-good" />
              <span className="hf-h4" style={{ fontSize: 12.5 }}>Forecast</span>
            </div>
            <div className="hf-small hf-muted">At your follower count, expect <b style={{ color: 'var(--ink)' }}>~24 redemptions</b> driving <b style={{ color: 'var(--ink)' }}>$420–$640</b> in incremental revenue. Margin impact: <b style={{ color: 'var(--ink)' }}>-$72</b>.</div>
          </div>
        </div>
        <div className="hf-flex hf-between" style={{ padding: '14px 22px', borderTop: '1px solid var(--line)' }}>
          <button className="hf-btn hf-btn-ghost">Save draft</button>
          <button className="hf-btn hf-btn-primary">Activate · Tue 12:00 AM</button>
        </div>
      </div>
      <Anno top={20} right={480}>drawer · new promo</Anno>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · MARKETING — Email recent buyers (composer + preview)
// ──────────────────────────────────────────────────────────────────────
function SMgmt_Marketing() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Customers" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Email recent buyers"
          subtitle="Marketing · drafted Tuesday"
          actions={<><button className="hf-btn hf-btn-outline hf-btn-sm">Save draft</button><button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="eye" s={11} /> Send test</button><button className="hf-btn hf-btn-primary hf-btn-sm">Schedule send</button></>}
        />
        <div className="hf-grow" style={{ overflow: 'hidden', display: 'flex' }}>
          {/* LEFT — composer */}
          <div style={{ flex: 1, padding: '24px 28px', overflow: 'auto', borderRight: '1px solid var(--line)' }}>
            <div className="hf-eyebrow" style={{ marginBottom: 8 }}>Step 1 of 3 · Audience</div>
            <div className="hf-card" style={{ padding: 18 }}>
              <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 12 }}>
                <span className="hf-h4">Recipients</span>
                <span className="hf-tiny hf-muted">184 buyers · 96% deliverable</span>
              </div>
              <div className="hf-col hf-gap-2">
                {[
                  { l: 'Buyers · last 30 days',  s: '47 buyers · avg $74 spend',    on: true,  count: 47 },
                  { l: 'Repeat buyers',           s: '23 buyers · 2+ orders',         on: true,  count: 23 },
                  { l: 'Followers without an order', s: '114 followers',                on: true,  count: 114 },
                  { l: 'All-time buyers',         s: '142 buyers · since Mar',        on: false, count: 142 },
                ].map((s, i) => (
                  <div key={s.l} className="hf-flex hf-items-center hf-gap-3" style={{ padding: 12, borderRadius: 10, border: '1px solid var(--line)', background: s.on ? 'rgba(0,102,204,0.04)' : 'var(--paper)' }}>
                    <span className={`hf-switch ${s.on ? 'hf-switch-on' : ''}`} />
                    <div className="hf-grow">
                      <div className="hf-h4" style={{ fontSize: 13 }}>{s.l}</div>
                      <div className="hf-tiny hf-muted">{s.s}</div>
                    </div>
                    <span className="hf-num hf-h4" style={{ fontSize: 13 }}>{s.count}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="hf-eyebrow" style={{ margin: '24px 0 8px' }}>Step 2 of 3 · Content</div>
            <div className="hf-card" style={{ padding: 18 }}>
              <div className="hf-tiny hf-muted">Template</div>
              <div className="hf-flex hf-gap-2" style={{ marginTop: 8, flexWrap: 'wrap' }}>
                {[
                  { l: 'Restock', on: true },
                  { l: 'New drop', on: false },
                  { l: 'Behind the scenes', on: false },
                  { l: 'Discount code', on: false },
                  { l: 'Plain text', on: false },
                ].map((t) => (
                  <span key={t.l} className={`hf-chip ${t.on ? 'hf-chip-on' : ''}`} style={{ fontSize: 11.5 }}>{t.l}</span>
                ))}
              </div>

              <div className="hf-tiny hf-muted" style={{ marginTop: 18, marginBottom: 6 }}>Subject</div>
              <div style={{ height: 44, padding: '0 14px', border: '1.5px solid var(--ink)', borderRadius: 10, display: 'flex', alignItems: 'center' }}>
                <span className="hf-h4" style={{ fontSize: 14 }}>The persimmon vase is back · just 8 this batch</span>
                <span style={{ width: 1.5, height: 18, background: 'var(--ink)', marginLeft: 2, animation: 'hf-blink 1s steps(1) infinite' }} />
              </div>
              <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 6 }}>
                <span>Open-rate forecast: <b style={{ color: 'var(--ink-2)' }}>32%</b> (above your avg)</span>
                <span>52 / 80</span>
              </div>

              <div className="hf-tiny hf-muted" style={{ marginTop: 18, marginBottom: 6 }}>Preview text</div>
              <div style={{ height: 38, padding: '0 12px', border: '1px solid var(--line)', borderRadius: 8, display: 'flex', alignItems: 'center' }}>
                <span className="hf-small">A small restock — three glaze variations this round.</span>
              </div>

              <div className="hf-tiny hf-muted" style={{ marginTop: 18, marginBottom: 8 }}>Featured product</div>
              <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: 12, border: '1px solid var(--line)', borderRadius: 10 }}>
                <ProdImg tone="clay" h={56} r={8} />
                <div className="hf-grow">
                  <div className="hf-h4">Persimmon vase</div>
                  <div className="hf-tiny hf-muted">8 in stock · $86</div>
                </div>
                <button className="hf-btn hf-btn-ghost hf-btn-sm">Change</button>
              </div>
            </div>

            <div className="hf-eyebrow" style={{ margin: '24px 0 8px' }}>Step 3 of 3 · Schedule</div>
            <div className="hf-card" style={{ padding: 18 }}>
              <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr' }}>
                <div>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Send</div>
                  <div className="hf-col hf-gap-2">
                    {[
                      { l: 'Now',                s: 'Sends within 5 min', on: false },
                      { l: 'Best time · Thu 6 PM', s: 'Highest opens for your list', on: true },
                      { l: 'Pick a time',         s: '— select date & time —', on: false },
                    ].map((o) => (
                      <div key={o.l} className="hf-flex hf-items-center hf-gap-2" style={{ padding: 8, borderRadius: 8, border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)' }}>
                        <span className="hf-center" style={{ width: 14, height: 14, borderRadius: 999, border: '1.5px solid var(--ink)' }}>{o.on && <span style={{ width: 7, height: 7, borderRadius: 999, background: 'var(--ink)' }} />}</span>
                        <div>
                          <div className="hf-small" style={{ fontWeight: 500 }}>{o.l}</div>
                          <div className="hf-tiny hf-muted">{o.s}</div>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
                <div>
                  <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Follow-ups</div>
                  <div className="hf-col hf-gap-2">
                    <div className="hf-flex hf-items-center hf-gap-2" style={{ padding: 10, borderRadius: 8, border: '1px solid var(--line)' }}>
                      <span className="hf-switch hf-switch-on" />
                      <span className="hf-small hf-grow">Re-send to non-openers · 3 days later</span>
                    </div>
                    <div className="hf-flex hf-items-center hf-gap-2" style={{ padding: 10, borderRadius: 8, border: '1px solid var(--line)' }}>
                      <span className="hf-switch" />
                      <span className="hf-small hf-grow">Auto-pause if &gt; 0.5% spam complaint</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* RIGHT — preview */}
          <div style={{ width: 480, padding: '24px 28px', overflow: 'auto', background: 'var(--paper-2)' }}>
            <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 14 }}>
              <span className="hf-h4">Preview</span>
              <div className="hf-flex" style={{ background: 'var(--paper)', borderRadius: 9999, padding: 2 }}>
                {[{ i: 'sun', on: true }, { i: 'moon' }].map((m, i) => (
                  <span key={i} className="hf-center" style={{ width: 28, height: 28, borderRadius: 9999, background: m.on ? 'var(--ink)' : 'transparent', color: m.on ? 'white' : 'var(--ink-3)' }}>
                    <Ico n={m.i} s={12} />
                  </span>
                ))}
              </div>
            </div>
            {/* email mock */}
            <div className="hf-card" style={{ padding: 0, overflow: 'hidden' }}>
              <div style={{ padding: '14px 18px', borderBottom: '1px solid var(--line)', background: 'var(--paper)' }}>
                <div className="hf-flex hf-items-center hf-gap-2">
                  <Avatar name="Mira Studio" size="sm" />
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 12.5 }}>Mira Studio</div>
                    <div className="hf-tiny hf-muted">to you · Tue 6:00 PM</div>
                  </div>
                </div>
                <div className="hf-h3" style={{ marginTop: 10 }}>The persimmon vase is back · just 8 this batch</div>
                <div className="hf-tiny hf-muted">A small restock — three glaze variations this round.</div>
              </div>
              <ProdImg tone="clay" h={220} r={0} />
              <div style={{ padding: '20px 22px' }}>
                <div className="hf-display-2" style={{ fontSize: 22, lineHeight: 1.1, letterSpacing: '-0.02em' }}>Hi Sasha,</div>
                <p className="hf-body" style={{ marginTop: 10, fontSize: 13.5, lineHeight: 1.55 }}>
                  Pulled eight persimmon vases out of the kiln Sunday — the warm batch, with the soft asymmetry on the rim you asked about last time.
                </p>
                <p className="hf-body" style={{ marginTop: 8, fontSize: 13.5, lineHeight: 1.55 }}>
                  They tend to go in a day. If you'd like one, the link is below — followers get $5 off through Friday.
                </p>
                <button className="hf-btn hf-btn-primary" style={{ marginTop: 18, height: 44, padding: '0 26px' }}>Shop the restock →</button>
                <div className="hf-tiny hf-muted" style={{ marginTop: 16 }}>— Mira</div>
              </div>
              <div style={{ padding: '12px 22px', background: 'var(--paper-2)', borderTop: '1px solid var(--line)' }}>
                <div className="hf-tiny hf-muted">You're getting this because you bought from Mira Studio · <a style={{ color: 'var(--primary)' }}>Unsubscribe</a></div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>marketing · email composer</Anno>
    </div>
  );
}

Object.assign(window, {
  SMgmt_OrdersInbox, SMgmt_OrderDetail, SMgmt_Promos, SMgmt_Marketing,
});

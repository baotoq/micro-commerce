// hifi-flow-listings.jsx — End-to-end listings management flow
// 5 desktop steps · 1280×800 · Mira tunes her catalog before a restock drop.

// ──────────────────────────────────────────────────────────────────────
// 01 · CATALOG — full table view, all 42 listings (reuses Listings_Table)
// ──────────────────────────────────────────────────────────────────────
function LFlow_01_Catalog() {
  return (
    <div style={{ position: 'relative', width: '100%', height: '100%' }}>
      <Listings_Table />
      <DesktopAction>filters → "Low &amp; Out of stock"</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · BULK · Select low-stock rows, bulk-edit price up 10%
// ──────────────────────────────────────────────────────────────────────
function LFlow_02_Bulk() {
  const rows = [
    { t: 'Persimmon vase',   sku: 'MS-VS-001', tone: 'clay',   price: 86,  stock: 0, status: 'Out',  sel: true },
    { t: 'Soft hand vessel', sku: 'MS-VS-019', tone: 'rose',   price: 92,  stock: 2, status: 'Low',  sel: true },
    { t: 'Indigo carafe',    sku: 'MS-CR-003', tone: 'cobalt', price: 110, stock: 5, status: 'Low',  sel: true },
    { t: 'Shadow vase, tall',sku: 'MS-VS-031', tone: 'shadow', price: 124, stock: 3, status: 'Low',  sel: false },
  ];
  const tone = (s) => s === 'Active' ? 'good' : s === 'Low' ? 'warn' : s === 'Out' ? 'bad' : 'soft';
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Listings"
          subtitle="Filtered · low &amp; out of stock"
          actions={<button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New listing</button>}
        />
        <div className="hf-flex hf-between" style={{ padding: '14px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-2">
            {['All · 42', 'Active · 38', 'Low · 3', 'Out · 1', 'Drafts · 4'].map((c, i) => (
              <span key={c} className={`hf-chip ${i === 2 || i === 3 ? 'hf-chip-on' : ''}`}>{c}</span>
            ))}
          </div>
        </div>

        {/* Bulk action bar */}
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '12px 28px', background: 'var(--ink)', color: 'var(--paper)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <span className="hf-center" style={{ width: 18, height: 18, borderRadius: 3, background: 'var(--paper)', color: 'var(--ink)' }}><Ico n="check" s={11} sw={2.4} /></span>
            <span className="hf-h4" style={{ color: 'var(--paper)' }}>3 of 4 selected</span>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-sm" style={{ background: 'rgba(255,255,255,0.12)', color: 'var(--paper)', border: '1px solid rgba(255,255,255,0.22)' }}>Edit price</button>
            <button className="hf-btn hf-btn-sm" style={{ background: 'rgba(255,255,255,0.12)', color: 'var(--paper)', border: '1px solid rgba(255,255,255,0.22)' }}>Adjust stock</button>
            <button className="hf-btn hf-btn-sm" style={{ background: 'rgba(255,255,255,0.12)', color: 'var(--paper)', border: '1px solid rgba(255,255,255,0.22)' }}>Move to draft</button>
            <button className="hf-btn hf-btn-sm" style={{ background: 'var(--paper)', color: 'var(--ink)' }}>Apply →</button>
          </div>
        </div>

        <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
          <div className="hf-grow" style={{ overflow: 'auto' }}>
            <table className="hf-table">
              <thead>
                <tr>
                  <th style={{ width: 32, paddingRight: 0 }}><span className="hf-center" style={{ width: 14, height: 14, borderRadius: 3, background: 'var(--ink)', color: 'white', display: 'inline-flex' }}><Ico n="check" s={9} sw={2.4} /></span></th>
                  <th style={{ width: 320 }}>Product</th>
                  <th>SKU</th>
                  <th>Status</th>
                  <th>Stock</th>
                  <th>Price</th>
                  <th>New price</th>
                </tr>
              </thead>
              <tbody>
                {rows.map((r, i) => (
                  <tr key={i} style={{ background: r.sel ? 'rgba(194,65,12,0.04)' : 'transparent' }}>
                    <td style={{ paddingRight: 0 }}>
                      <span className="hf-center" style={{ width: 14, height: 14, borderRadius: 3, background: r.sel ? 'var(--ink)' : 'transparent', border: r.sel ? 'none' : '1.5px solid var(--ink-4)', color: 'white', display: 'inline-flex' }}>
                        {r.sel && <Ico n="check" s={9} sw={2.4} />}
                      </span>
                    </td>
                    <td>
                      <div className="hf-flex hf-items-center hf-gap-3">
                        <ProdImg tone={r.tone} h={36} r={6} />
                        <div className="hf-h4" style={{ color: 'var(--ink)' }}>{r.t}</div>
                      </div>
                    </td>
                    <td className="hf-mono" style={{ color: 'var(--ink-3)' }}>{r.sku}</td>
                    <td><span className={`hf-chip hf-chip-${tone(r.status)}`} style={{ fontSize: 11 }}>{r.status}</span></td>
                    <td className="hf-num" style={{ color: r.stock === 0 ? 'var(--bad)' : 'var(--warn)', fontWeight: 500 }}>{r.stock}</td>
                    <td className="hf-num hf-muted" style={{ textDecoration: r.sel ? 'line-through' : 'none' }}>{money(r.price)}</td>
                    <td className="hf-num" style={{ color: r.sel ? 'var(--good)' : 'var(--ink-4)', fontWeight: 600 }}>
                      {r.sel ? money(r.price * 1.1) : '—'}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Right · bulk-edit drawer */}
          <div style={{ width: 340, borderLeft: '1px solid var(--line)', padding: 22, background: 'var(--paper)', flexShrink: 0 }}>
            <div className="hf-eyebrow" style={{ marginBottom: 6 }}>Bulk edit · 3 items</div>
            <div className="hf-h2" style={{ fontSize: 20, marginBottom: 18 }}>Adjust price</div>

            <div className="hf-flex hf-gap-1" style={{ background: 'var(--paper-2)', padding: 3, borderRadius: 8, marginBottom: 16 }}>
              {['Set to', 'Increase', 'Decrease'].map((s, i) => (
                <button key={s} className="hf-btn hf-btn-sm hf-grow" style={{ background: i === 1 ? 'var(--paper)' : 'transparent', boxShadow: i === 1 ? 'var(--shadow-1)' : 'none', color: 'var(--ink)' }}>{s}</button>
              ))}
            </div>

            <div className="hf-flex hf-gap-2" style={{ marginBottom: 16 }}>
              <div className="hf-grow">
                <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Amount</div>
                <div style={{ height: 44, border: '1.5px solid var(--ink)', borderRadius: 8, padding: '0 14px', display: 'flex', alignItems: 'center' }}>
                  <span className="hf-h4 hf-num" style={{ fontSize: 16 }}>10</span>
                </div>
              </div>
              <div style={{ width: 90 }}>
                <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Unit</div>
                <div className="hf-flex hf-items-center hf-between" style={{ height: 44, border: '1px solid var(--line)', borderRadius: 8, padding: '0 12px' }}>
                  <span className="hf-h4">%</span>
                  <Ico n="chevD" s={11} />
                </div>
              </div>
            </div>

            <div className="hf-card" style={{ padding: 14, background: 'var(--paper-2)', border: 'none', marginBottom: 18 }}>
              <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>Preview · 3 items</div>
              <div className="hf-col hf-gap-2 hf-small">
                {rows.filter(r => r.sel).map(r => (
                  <div key={r.sku} className="hf-flex hf-between">
                    <span className="hf-muted" style={{ overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{r.t}</span>
                    <span className="hf-num"><span className="hf-muted" style={{ textDecoration: 'line-through' }}>{money(r.price)}</span> <span style={{ color: 'var(--good)', fontWeight: 600 }}>{money(r.price * 1.1)}</span></span>
                  </div>
                ))}
              </div>
            </div>

            <button className="hf-btn hf-btn-primary" style={{ width: '100%', height: 42 }}>Apply to 3 items</button>
            <button className="hf-btn hf-btn-ghost" style={{ width: '100%', marginTop: 6 }}>Cancel</button>
          </div>
        </div>
      </div>
      <DesktopAction>applies · opens Persimmon vase to fix variants</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · EDIT — single product editor focused on variants
// ──────────────────────────────────────────────────────────────────────
function LFlow_03_Edit() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '16px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
            <div>
              <div className="hf-tiny hf-muted">Listings · Vessels</div>
              <h1 className="hf-display" style={{ fontSize: 24, lineHeight: 1, marginTop: 2 }}>Persimmon vase</h1>
            </div>
            <span className="hf-chip hf-chip-warn" style={{ marginLeft: 8 }}><span className="hf-dot hf-dot-warn" /> Unsaved changes</span>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-ghost">Discard</button>
            <button className="hf-btn hf-btn-outline">Save draft</button>
            <button className="hf-btn hf-btn-primary">Publish →</button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.5fr 1fr' }}>
            {/* Variant matrix */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-h3">Variant matrix</div>
                  <div className="hf-tiny hf-muted">Size × Glaze · 6 combinations</div>
                </div>
                <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="plus" s={11} /> Add option</button>
              </div>
              <table className="hf-table">
                <thead>
                  <tr>
                    <th>Variant</th>
                    <th>SKU</th>
                    <th>Price</th>
                    <th>Stock</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {[
                    { v: 'Small · Persimmon',  sku: 'MS-VS-001-S',  p: 70,  s: 6, st: 'Active', changed: true },
                    { v: 'Medium · Persimmon', sku: 'MS-VS-001-M',  p: 95,  s: 4, st: 'Active', changed: true },
                    { v: 'Large · Persimmon',  sku: 'MS-VS-001-L',  p: 136, s: 0, st: 'Out' },
                    { v: 'Small · Cream',      sku: 'MS-VS-001-SC', p: 70,  s: 8, st: 'Active' },
                    { v: 'Medium · Cream',     sku: 'MS-VS-001-MC', p: 95,  s: 5, st: 'Active' },
                    { v: 'Large · Cream',      sku: 'MS-VS-001-LC', p: 136, s: 2, st: 'Low' },
                  ].map((v, i) => (
                    <tr key={i} style={{ background: v.changed ? 'rgba(27,94,63,0.04)' : 'transparent' }}>
                      <td style={{ color: 'var(--ink)', fontWeight: 500 }}>
                        <span className="hf-flex hf-items-center hf-gap-2">
                          {v.changed && <span className="hf-dot hf-dot-good" />}
                          {v.v}
                        </span>
                      </td>
                      <td className="hf-mono" style={{ color: 'var(--ink-3)' }}>{v.sku}</td>
                      <td className="hf-num" style={{ fontWeight: v.changed ? 600 : 400, color: v.changed ? 'var(--good)' : 'var(--ink)' }}>{money(v.p)}</td>
                      <td className="hf-num" style={{ color: v.s === 0 ? 'var(--bad)' : v.s < 5 ? 'var(--warn)' : 'var(--ink)', fontWeight: 500 }}>{v.s}</td>
                      <td><span className={`hf-chip hf-chip-${v.st === 'Active' ? 'good' : v.st === 'Low' ? 'warn' : 'bad'}`} style={{ fontSize: 11 }}>{v.st}</span></td>
                    </tr>
                  ))}
                </tbody>
              </table>
              <div className="hf-tiny hf-muted" style={{ marginTop: 14 }}>● 2 variants updated · prices +10%</div>
            </div>

            {/* Right · meta + photos */}
            <div className="hf-col hf-gap-3">
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 12 }}>Photos · 4 of 8</div>
                <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: 'repeat(3, 1fr)' }}>
                  <ProdImg tone="clay" h={88} r={6} label="cover" />
                  <ProdImg tone="bone" h={88} r={6} />
                  <ProdImg tone="shadow" h={88} r={6} />
                  <ProdImg tone="terra" h={88} r={6} />
                  <div className="hf-center" style={{ height: 88, borderRadius: 6, border: '1.5px dashed var(--ink-4)', color: 'var(--ink-3)' }}>
                    <Ico n="plus" s={16} />
                  </div>
                </div>
              </div>
              <div className="hf-card" style={{ padding: 18 }}>
                <div className="hf-h4" style={{ marginBottom: 10 }}>Status</div>
                <div className="hf-flex hf-gap-1" style={{ background: 'var(--paper-2)', padding: 3, borderRadius: 8 }}>
                  {['Active', 'Draft', 'Archived'].map((s, i) => (
                    <button key={s} className="hf-btn hf-btn-sm hf-grow" style={{ background: i === 0 ? 'var(--paper)' : 'transparent', boxShadow: i === 0 ? 'var(--shadow-1)' : 'none', color: 'var(--ink)' }}>{s}</button>
                  ))}
                </div>
                <div className="hf-tiny hf-muted" style={{ marginTop: 10 }}>Visible at <span className="hf-mono" style={{ color: 'var(--ink)' }}>/persimmon-vase</span></div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>publish → preview</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · PREVIEW — what shoppers see (mini split: phone + desktop)
// ──────────────────────────────────────────────────────────────────────
function LFlow_04_Preview() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '16px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
            <div>
              <div className="hf-tiny hf-muted">Preview · Persimmon vase</div>
              <h1 className="hf-display" style={{ fontSize: 22, lineHeight: 1, marginTop: 2 }}>How shoppers will see it</h1>
            </div>
          </div>
          <div className="hf-flex hf-gap-2">
            <div className="hf-flex hf-gap-1" style={{ background: 'var(--paper-2)', padding: 3, borderRadius: 8 }}>
              <button className="hf-btn hf-btn-sm" style={{ background: 'var(--paper)', boxShadow: 'var(--shadow-1)' }}>Desktop</button>
              <button className="hf-btn hf-btn-sm hf-btn-ghost">Mobile</button>
            </div>
            <button className="hf-btn hf-btn-outline">Back to edit</button>
            <button className="hf-btn hf-btn-primary">Publish now →</button>
          </div>
        </div>

        <div className="hf-grow hf-flex" style={{ overflow: 'hidden', background: 'var(--paper-2)' }}>
          {/* desktop browser preview */}
          <div className="hf-grow hf-center" style={{ padding: 24 }}>
            <div className="hf-card" style={{ width: '100%', maxWidth: 720, height: 560, padding: 0, overflow: 'hidden', boxShadow: 'var(--shadow-3)' }}>
              <div style={{ height: 30, background: '#E8E3D8', borderBottom: '1px solid var(--line)', display: 'flex', alignItems: 'center', padding: '0 12px', gap: 8 }}>
                {['#FF6058','#FFBD2E','#28C941'].map(c => <span key={c} style={{ width: 9, height: 9, borderRadius: 999, background: c }} />)}
                <div className="hf-flex hf-items-center hf-gap-1" style={{ flex: 1, height: 18, background: 'var(--paper)', borderRadius: 4, padding: '0 8px', marginLeft: 8 }}>
                  <Ico n="lock" s={10} />
                  <span className="hf-tiny" style={{ color: 'var(--ink-3)' }}>mira-studio.micro.shop/persimmon-vase</span>
                </div>
              </div>
              <div className="hf-grid" style={{ gridTemplateColumns: '1.4fr 1fr', height: 'calc(100% - 30px)' }}>
                <ProdImg tone="clay" h="100%" r={0} />
                <div style={{ padding: 28, display: 'flex', flexDirection: 'column' }}>
                  <div className="hf-eyebrow" style={{ marginBottom: 6 }}>Mira Studio · Vessels</div>
                  <h2 className="hf-display" style={{ fontSize: 32, lineHeight: 1, letterSpacing: '-0.01em' }}>Persimmon vase</h2>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 22, marginTop: 10 }}>{money(95)}</div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>Medium · Persimmon · 4 in stock</div>
                  <div className="hf-tiny" style={{ marginTop: 14, marginBottom: 6, fontWeight: 500 }}>SIZE</div>
                  <div className="hf-flex hf-gap-1">
                    {['Small', 'Medium', 'Large'].map((s, i) => (
                      <span key={s} className={`hf-chip ${i === 1 ? 'hf-chip-on' : ''}`} style={{ opacity: s === 'Large' ? 0.5 : 1 }}>{s}{s === 'Large' && ' · out'}</span>
                    ))}
                  </div>
                  <div className="hf-tiny" style={{ marginTop: 14, marginBottom: 6, fontWeight: 500 }}>GLAZE</div>
                  <div className="hf-flex hf-gap-2">
                    <span style={{ width: 24, height: 24, borderRadius: 999, background: 'var(--terra)', border: '2px solid var(--ink)' }} />
                    <span style={{ width: 24, height: 24, borderRadius: 999, background: '#F0E8D7', border: '1px solid var(--line)' }} />
                  </div>
                  <div className="hf-grow" />
                  <button className="hf-btn hf-btn-primary" style={{ width: '100%', height: 44 }}>Add to bag · {money(95)}</button>
                </div>
              </div>
            </div>
          </div>

          {/* Right · linter / SEO check */}
          <div style={{ width: 320, borderLeft: '1px solid var(--line)', padding: 22, background: 'var(--paper)', flexShrink: 0, overflow: 'auto' }}>
            <div className="hf-flex hf-between" style={{ marginBottom: 8 }}>
              <span className="hf-h4">Listing health</span>
              <span className="hf-num hf-h4" style={{ color: 'var(--good)' }}>96</span>
            </div>
            <div className="hf-progress" style={{ marginBottom: 16 }}><i style={{ width: '96%', background: 'var(--good)' }} /></div>
            <div className="hf-col hf-gap-3">
              {[
                { l: 'Title under 60 chars', s: '14 / 60', t: 'good' },
                { l: 'Description over 100 chars', s: '208 / 800', t: 'good' },
                { l: '4 photos', s: 'recommend 6+', t: 'warn' },
                { l: 'Variants in stock', s: '5 of 6 active', t: 'good' },
                { l: 'Tagged & categorized', s: '4 tags · Vessels', t: 'good' },
              ].map((c, i) => (
                <div key={i} className="hf-flex hf-items-start hf-gap-2">
                  <span className="hf-center" style={{ width: 18, height: 18, borderRadius: 999, marginTop: 1, flexShrink: 0, background: c.t === 'good' ? 'var(--good)' : 'var(--warn)', color: 'white' }}>
                    <Ico n={c.t === 'good' ? 'check' : 'info'} s={10} sw={2.4} />
                  </span>
                  <div className="hf-grow">
                    <div className="hf-h4" style={{ fontSize: 12.5 }}>{c.l}</div>
                    <div className="hf-tiny hf-muted">{c.s}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>publishes · scheduled drop confirmation</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05 · PUBLISHED — Confirmation + back to catalog with diff
// ──────────────────────────────────────────────────────────────────────
function LFlow_05_Published() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row', position: 'relative' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Listings"
          subtitle="42 products · 39 active"
          actions={<button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New listing</button>}
        />
        {/* success banner */}
        <div style={{ background: '#DDEDE1', borderBottom: '1px solid var(--line)', padding: '14px 28px' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <span className="hf-center" style={{ width: 26, height: 26, borderRadius: 999, background: 'var(--good)', color: 'white', flexShrink: 0 }}>
              <Ico n="check" s={13} sw={2.4} />
            </span>
            <div className="hf-grow">
              <span className="hf-h4">Persimmon vase published · 2 variants updated, 1 went live.</span>
            </div>
            <button className="hf-btn hf-btn-ghost hf-btn-sm">View shop →</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm">Undo</button>
          </div>
        </div>

        {/* Diff summary */}
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(3, 1fr)', marginBottom: 20 }}>
            {[
              { l: 'Active listings', v: '39', d: '+1', t: 'up' },
              { l: 'Variants in stock', v: '128', d: '+2', t: 'up' },
              { l: 'Out-of-stock items', v: '1', d: '−2', t: 'up' },
            ].map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted">{s.l}</div>
                <div className="hf-flex hf-items-end hf-gap-2" style={{ marginTop: 6 }}>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1 }}>{s.v}</div>
                  <span className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600, marginBottom: 4 }}>↑ {s.d}</span>
                </div>
              </div>
            ))}
          </div>

          <div className="hf-card" style={{ padding: 0 }}>
            <div className="hf-flex hf-between" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
              <div className="hf-h3">What just changed</div>
              <a className="hf-small" style={{ color: 'var(--ink-2)', fontWeight: 500 }}>Activity log →</a>
            </div>
            <table className="hf-table">
              <thead><tr><th>When</th><th>Item</th><th>Change</th><th>By</th></tr></thead>
              <tbody>
                {[
                  { d: 'just now',  l: 'Persimmon vase · Medium', c: 'Price · $86 → $95', a: 'You' },
                  { d: 'just now',  l: 'Persimmon vase · Small',  c: 'Price · $64 → $70', a: 'You' },
                  { d: 'just now',  l: 'Persimmon vase · Large',  c: 'Status · Out → still out (no stock)', a: 'You' },
                  { d: '12 min ago', l: 'Soft hand vessel',        c: 'Price · $92 → $101', a: 'You · bulk' },
                  { d: '12 min ago', l: 'Indigo carafe',           c: 'Price · $110 → $121', a: 'You · bulk' },
                ].map((r, i) => (
                  <tr key={i}>
                    <td className="hf-tiny hf-muted">{r.d}</td>
                    <td style={{ color: 'var(--ink)', fontWeight: 500 }}>{r.l}</td>
                    <td className="hf-muted">{r.c}</td>
                    <td><span className="hf-flex hf-items-center hf-gap-2"><Avatar name="Mira" size="sm" /><span className="hf-small">{r.a}</span></span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>published · diff log</Anno>
    </div>
  );
}

Object.assign(window, { LFlow_01_Catalog, LFlow_02_Bulk, LFlow_03_Edit, LFlow_04_Preview, LFlow_05_Published });

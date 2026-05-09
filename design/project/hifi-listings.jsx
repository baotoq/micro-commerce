// hifi-listings.jsx — 3 desktop seller listings views (1280x800)

// ── A · Table view — dense, spreadsheet-like
function Listings_Table() {
  const rows = [
    { t: 'Persimmon vase', sku: 'MS-VS-001', tone: 'clay', price: 86, stock: 0,  status: 'Out',  views: 412, sales: 14 },
    { t: 'Forest bowl, lg.', sku: 'MS-BW-014', tone: 'sage', price: 64, stock: 8,  status: 'Active', views: 308, sales: 9 },
    { t: 'Cream tumbler — set of 2', sku: 'MS-TB-007', tone: 'bone', price: 48, stock: 22, status: 'Active', views: 256, sales: 12 },
    { t: 'Soft hand vessel', sku: 'MS-VS-019', tone: 'rose', price: 92, stock: 2,  status: 'Low',  views: 180, sales: 6 },
    { t: 'Indigo carafe', sku: 'MS-CR-003', tone: 'cobalt', price: 110, stock: 5, status: 'Active', views: 148, sales: 4 },
    { t: 'Bone dinner plate', sku: 'MS-PL-022', tone: 'cream', price: 38, stock: 36, status: 'Active', views: 142, sales: 8 },
    { t: 'Rust mug, Nº 04', sku: 'MS-MG-041', tone: 'rust', price: 32, stock: 18, status: 'Active', views: 132, sales: 7 },
    { t: 'Moss saucer, set of 4', sku: 'MS-SC-008', tone: 'moss', price: 44, stock: 0,  status: 'Draft', views: 0, sales: 0 },
    { t: 'Shadow vase, tall', sku: 'MS-VS-031', tone: 'shadow', price: 124, stock: 3, status: 'Low', views: 92, sales: 3 },
  ];
  const tone = (s) => s === 'Active' ? 'good' : s === 'Low' ? 'warn' : s === 'Out' ? 'bad' : 'soft';

  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Listings"
          subtitle="42 products · 38 active"
          actions={<><button className="hf-btn hf-btn-outline"><Ico n="upload" s={12} /> Import CSV</button><button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New listing</button></>}
        />
        <div className="hf-flex hf-between" style={{ padding: '14px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-2">
            {['All · 42', 'Active · 38', 'Low · 3', 'Out · 1', 'Drafts · 4'].map((c, i) => (
              <span key={c} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`}>{c}</span>
            ))}
          </div>
          <div className="hf-flex hf-items-center hf-gap-2">
            <div className="hf-flex hf-items-center hf-gap-2 hf-px-3" style={{ height: 30, background: 'var(--paper-2)', borderRadius: 999, color: 'var(--ink-3)', width: 220 }}>
              <Ico n="search" s={12} /><span className="hf-small">Search products…</span>
            </div>
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="filter" s={11} /> Collection</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="sort" s={11} /> Best-selling</button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto' }}>
          <table className="hf-table" style={{ minWidth: '100%' }}>
            <thead>
              <tr>
                <th style={{ width: 32, paddingRight: 0 }}><div style={{ width: 14, height: 14, border: '1.5px solid var(--ink-4)', borderRadius: 3 }} /></th>
                <th style={{ width: 320 }}>Product</th>
                <th>SKU</th>
                <th>Status</th>
                <th>Stock</th>
                <th>Price</th>
                <th>Views · 30d</th>
                <th>Sales · 30d</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {rows.map((r, i) => (
                <tr key={i}>
                  <td style={{ paddingRight: 0 }}><div style={{ width: 14, height: 14, border: '1.5px solid var(--ink-4)', borderRadius: 3 }} /></td>
                  <td>
                    <div className="hf-flex hf-items-center hf-gap-3">
                      <ProdImg tone={r.tone} h={36} r={6} />
                      <div>
                        <div className="hf-h4" style={{ color: 'var(--ink)' }}>{r.t}</div>
                        <div className="hf-tiny hf-muted">Vessels · 4 photos</div>
                      </div>
                    </div>
                  </td>
                  <td className="hf-mono" style={{ color: 'var(--ink-3)' }}>{r.sku}</td>
                  <td><span className={`hf-chip hf-chip-${tone(r.status)}`} style={{ fontSize: 11 }}>{r.status}</span></td>
                  <td className="hf-num" style={{ color: r.stock === 0 ? 'var(--bad)' : r.stock < 5 ? 'var(--warn)' : 'var(--ink)', fontWeight: 500 }}>{r.stock}</td>
                  <td className="hf-num" style={{ color: 'var(--ink)', fontWeight: 500 }}>{money(r.price)}</td>
                  <td className="hf-num">{r.views.toLocaleString()}</td>
                  <td className="hf-num">{r.sales}</td>
                  <td><Ico n="chevR" s={13} /></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="hf-flex hf-between" style={{ padding: '12px 28px', borderTop: '1px solid var(--line)', background: 'var(--paper)' }}>
          <span className="hf-small hf-muted">9 of 42 shown</span>
          <div className="hf-flex hf-gap-1">
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="chevL" s={11} /></button>
            <button className="hf-btn hf-btn-sm" style={{ background: 'var(--ink)', color: 'var(--paper)' }}>1</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm">2</button>
            <button className="hf-btn hf-btn-ghost hf-btn-sm">3</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="chevR" s={11} /></button>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>dense table</Anno>
    </div>
  );
}

// ── B · Card grid — visual-led
function Listings_Cards() {
  const items = [
    { t: 'Persimmon vase', tone: 'clay', price: 86, stock: 0, status: 'Out', sales: 14 },
    { t: 'Forest bowl, lg.', tone: 'sage', price: 64, stock: 8, status: 'Active', sales: 9 },
    { t: 'Cream tumbler set', tone: 'bone', price: 48, stock: 22, status: 'Active', sales: 12 },
    { t: 'Soft hand vessel', tone: 'rose', price: 92, stock: 2, status: 'Low', sales: 6 },
    { t: 'Indigo carafe', tone: 'cobalt', price: 110, stock: 5, status: 'Active', sales: 4 },
    { t: 'Bone dinner plate', tone: 'cream', price: 38, stock: 36, status: 'Active', sales: 8 },
    { t: 'Rust mug, Nº 04', tone: 'rust', price: 32, stock: 18, status: 'Active', sales: 7 },
    { t: 'Moss saucer set', tone: 'moss', price: 44, stock: 0, status: 'Draft', sales: 0 },
  ];
  const tone = (s) => s === 'Active' ? 'good' : s === 'Low' ? 'warn' : s === 'Out' ? 'bad' : 'soft';
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Listings"
          subtitle="The shop · arranged by you"
          actions={<><button className="hf-btn hf-btn-outline"><Ico n="grid" s={12} /> Reorder</button><button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New listing</button></>}
        />
        <div className="hf-flex hf-items-center hf-between" style={{ padding: '14px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <div className="hf-flex hf-gap-1">
              {['All · 42', 'Vessels · 14', 'Tableware · 18', 'Drinkware · 10'].map((c, i) => (
                <span key={c} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`}>{c}</span>
              ))}
            </div>
          </div>
          <div className="hf-flex hf-items-center hf-gap-2">
            <button className="hf-icon-btn"><Ico n="grid" s={14} /></button>
            <button className="hf-icon-btn" style={{ color: 'var(--ink-4)' }}><Ico n="list" s={14} /></button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-4" style={{ gridTemplateColumns: 'repeat(4, 1fr)' }}>
            {items.map((p, i) => (
              <div key={i} className="hf-card" style={{ padding: 0, overflow: 'hidden' }}>
                <div style={{ position: 'relative' }}>
                  <ProdImg tone={p.tone} h={170} r={0} />
                  <div style={{ position: 'absolute', top: 10, left: 10 }}>
                    <span className={`hf-chip hf-chip-${tone(p.status)}`} style={{ fontSize: 11, background: 'rgba(255,255,255,0.95)', color: p.status === 'Active' ? 'var(--good)' : p.status === 'Low' ? 'var(--warn)' : p.status === 'Out' ? 'var(--bad)' : 'var(--ink-3)' }}><span className={`hf-dot hf-dot-${tone(p.status)}`} /> {p.status}</span>
                  </div>
                  <button className="hf-icon-btn" style={{ position: 'absolute', top: 8, right: 8, background: 'rgba(255,255,255,0.92)', width: 26, height: 26 }}><Ico n="edit" s={12} /></button>
                </div>
                <div style={{ padding: 14 }}>
                  <div className="hf-flex hf-between hf-items-baseline">
                    <span className="hf-h4" style={{ flexGrow: 1, paddingRight: 8 }}>{p.t}</span>
                    <span className="hf-display-2 hf-num" style={{ fontSize: 16 }}>{money(p.price)}</span>
                  </div>
                  <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
                    <span>{p.stock === 0 ? 'No stock' : p.stock + ' in stock'}</span>
                    <span>{p.sales} sold · 30d</span>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>visual catalog</Anno>
    </div>
  );
}

// ── C · Editor — single-listing detail / create flow
function Listings_Editor() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Listings" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '16px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
            <div>
              <div className="hf-crumb"><a>Listings</a> <span className="hf-crumb-sep">/</span> <a>Vessels</a> <span className="hf-crumb-sep">/</span> <span style={{ color: 'var(--ink)' }}>Persimmon vase</span></div>
              <h1 className="hf-display" style={{ fontSize: 24, lineHeight: 1, marginTop: 4 }}>Persimmon vase</h1>
            </div>
          </div>
          <div className="hf-flex hf-items-center hf-gap-2">
            <span className="hf-chip hf-chip-soft"><span className="hf-dot hf-dot-mute" /> Saved 2 min ago</span>
            <button className="hf-btn hf-btn-outline">Preview</button>
            <button className="hf-btn hf-btn-primary">Publish changes</button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto' }}>
          <div className="hf-grid" style={{ gridTemplateColumns: '1fr 320px', gap: 0 }}>
            {/* Main column */}
            <div style={{ padding: '24px 28px' }}>
              {/* Photos */}
              <div className="hf-card" style={{ padding: 18, marginBottom: 16 }}>
                <div className="hf-flex hf-between" style={{ marginBottom: 12 }}>
                  <span className="hf-h3">Photos</span>
                  <span className="hf-tiny hf-muted">4 of 8 · drag to reorder</span>
                </div>
                <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: 'repeat(5, 1fr)' }}>
                  <ProdImg tone="clay" h={110} r={8} label="cover" />
                  <ProdImg tone="bone" h={110} r={8} />
                  <ProdImg tone="shadow" h={110} r={8} />
                  <ProdImg tone="terra" h={110} r={8} />
                  <div className="hf-center" style={{ height: 110, borderRadius: 8, border: '1.5px dashed var(--line-2)', color: 'var(--ink-3)' }}>
                    <div className="hf-col hf-items-center hf-gap-1"><Ico n="plus" s={16} /><span className="hf-tiny">Add</span></div>
                  </div>
                </div>
              </div>

              {/* Details */}
              <div className="hf-card" style={{ padding: 20, marginBottom: 16 }}>
                <div className="hf-h3" style={{ marginBottom: 14 }}>Details</div>
                <div className="hf-col hf-gap-3">
                  <div>
                    <span className="hf-label">Title</span>
                    <input className="hf-input" defaultValue="Persimmon vase" />
                  </div>
                  <div>
                    <span className="hf-label">Description</span>
                    <textarea className="hf-input hf-textarea" rows={4} defaultValue="A round-bellied stoneware vase, glazed in a deep persimmon. Each piece is wheel-thrown and fired in our Oakland studio — small variations in form and color are part of the work." />
                  </div>
                  <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr' }}>
                    <div><span className="hf-label">Category</span>
                      <div className="hf-input hf-flex hf-between hf-items-center" style={{ paddingRight: 10 }}>Vessels<Ico n="chevD" s={12} /></div>
                    </div>
                    <div><span className="hf-label">Material</span><input className="hf-input" defaultValue="Stoneware" /></div>
                  </div>
                </div>
              </div>

              {/* Variants */}
              <div className="hf-card" style={{ padding: 20 }}>
                <div className="hf-flex hf-between" style={{ marginBottom: 12 }}>
                  <span className="hf-h3">Variants</span>
                  <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="plus" s={11} /> Add option</button>
                </div>
                <div className="hf-flex hf-gap-2" style={{ marginBottom: 14 }}>
                  <span className="hf-chip">Size: Small · Medium · Large</span>
                  <span className="hf-chip">Glaze: Persimmon</span>
                </div>
                <table className="hf-table" style={{ marginTop: 4 }}>
                  <thead><tr><th>Variant</th><th>SKU</th><th>Price</th><th>Stock</th><th>Status</th></tr></thead>
                  <tbody>
                    {[
                      { v: 'Small · Persimmon', sku: 'MS-VS-001-S', p: 64, s: 4, st: 'Active' },
                      { v: 'Medium · Persimmon', sku: 'MS-VS-001-M', p: 86, s: 0, st: 'Out' },
                      { v: 'Large · Persimmon', sku: 'MS-VS-001-L', p: 124, s: 2, st: 'Low' },
                    ].map((v, i) => (
                      <tr key={i}>
                        <td style={{ color: 'var(--ink)', fontWeight: 500 }}>{v.v}</td>
                        <td className="hf-mono" style={{ color: 'var(--ink-3)' }}>{v.sku}</td>
                        <td className="hf-num">{money(v.p)}</td>
                        <td className="hf-num" style={{ color: v.s === 0 ? 'var(--bad)' : v.s < 5 ? 'var(--warn)' : 'var(--ink)', fontWeight: 500 }}>{v.s}</td>
                        <td><span className={`hf-chip hf-chip-${v.st === 'Active' ? 'good' : v.st === 'Low' ? 'warn' : 'bad'}`} style={{ fontSize: 11 }}>{v.st}</span></td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Right rail */}
            <div style={{ borderLeft: '1px solid var(--line)', padding: '24px 22px', background: 'var(--paper)' }}>
              <div style={{ marginBottom: 22 }}>
                <span className="hf-label">Status</span>
                <div className="hf-flex hf-gap-1" style={{ background: 'var(--paper-2)', padding: 3, borderRadius: 8 }}>
                  {['Active', 'Draft', 'Archived'].map((s, i) => (
                    <button key={s} className="hf-btn hf-btn-sm hf-grow" style={{ background: i === 0 ? 'var(--card)' : 'transparent', boxShadow: i === 0 ? 'var(--shadow-1)' : 'none', color: 'var(--ink)' }}>{s}</button>
                  ))}
                </div>
              </div>

              <div style={{ marginBottom: 22 }}>
                <span className="hf-label">Pricing</span>
                <div className="hf-grid hf-gap-2" style={{ gridTemplateColumns: '1fr 1fr' }}>
                  <div>
                    <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Price</div>
                    <input className="hf-input hf-input-sm" defaultValue="$86.00" />
                  </div>
                  <div>
                    <div className="hf-tiny hf-muted" style={{ marginBottom: 4 }}>Compare-at</div>
                    <input className="hf-input hf-input-sm" placeholder="—" />
                  </div>
                </div>
              </div>

              <div style={{ marginBottom: 22 }}>
                <span className="hf-label">Inventory</span>
                <div className="hf-flex hf-between hf-items-center" style={{ padding: '8px 12px', background: 'var(--paper-2)', borderRadius: 8, marginBottom: 8 }}>
                  <span className="hf-small">Total in stock</span>
                  <span className="hf-num hf-h4">6</span>
                </div>
                <div className="hf-flex hf-items-center hf-gap-2">
                  <div className="hf-switch hf-switch-on" /><span className="hf-small">Allow pre-orders</span>
                </div>
              </div>

              <div style={{ marginBottom: 22 }}>
                <span className="hf-label">Shipping</span>
                <div className="hf-col hf-gap-1 hf-small hf-muted">
                  <div className="hf-flex hf-between"><span>Weight</span><span style={{ color: 'var(--ink)' }} className="hf-num">1.4 kg</span></div>
                  <div className="hf-flex hf-between"><span>Origin</span><span style={{ color: 'var(--ink)' }}>Oakland, CA</span></div>
                  <div className="hf-flex hf-between"><span>Class</span><span style={{ color: 'var(--ink)' }}>Fragile · standard</span></div>
                </div>
              </div>

              <div>
                <span className="hf-label">SEO &amp; tags</span>
                <div className="hf-flex hf-gap-1" style={{ flexWrap: 'wrap' }}>
                  {['vessels', 'stoneware', 'wheel-thrown', 'persimmon'].map(t => (
                    <span key={t} className="hf-chip hf-chip-soft" style={{ fontSize: 11 }}>{t}</span>
                  ))}
                  <span className="hf-chip" style={{ fontSize: 11, borderStyle: 'dashed', color: 'var(--ink-3)' }}>+ Add</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>focused editor</Anno>
    </div>
  );
}

Object.assign(window, { Listings_Table, Listings_Cards, Listings_Editor });

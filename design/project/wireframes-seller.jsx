// Seller wireframes — dashboard, listings (create/edit), analytics
// Three variations per screen.

// ─── SELLER DASHBOARD (desktop, 3 variants) ──────────────────────────────────

const SellerNav = ({ active = 'overview' }) => (
  <div className="wf-col" style={{ width: 160, padding: '14px 12px', borderRight: '1px solid var(--ink-faint)', gap: 12, background: 'var(--paper-2)' }}>
    <div className="wf-between">
      <span className="wf-display" style={{ fontWeight: 700, fontSize: 16 }}>micro</span>
      <Icon name="bell" size={14} />
    </div>
    <div className="wf-tiny wf-muted">North Hide Co.</div>
    <div className="wf-col wf-gap-2 wf-mt-2">
      {[
        ['overview', 'Overview', 'home'],
        ['orders', 'Orders', 'box'],
        ['listings', 'Products', 'grid'],
        ['analytics', 'Analytics', 'chart'],
        ['settings', 'Settings', 'settings'],
      ].map(([k, l, ic]) => (
        <div key={k} className="wf-row wf-gap-2"
          style={{
            padding: '6px 8px', borderRadius: 6,
            background: k === active ? 'var(--ink)' : 'transparent',
            color: k === active ? 'var(--paper)' : 'var(--ink-2)',
            fontSize: 13, alignItems: 'center',
          }}>
          <Icon name={ic} size={14} /><span>{l}</span>
        </div>
      ))}
    </div>
  </div>
);

const Dashboard_KPI = () => (
  <Desktop url="micro.shop/dashboard">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="overview" />
      <div className="wf-grow" style={{ padding: '20px 24px', overflow: 'hidden' }}>
        <div className="wf-between">
          <Heading size="h2">Today</Heading>
          <div className="wf-row wf-gap-2"><Chip>Today</Chip><Chip>7d</Chip><Chip>30d</Chip></div>
        </div>
        {/* KPI tiles */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4,1fr)', gap: 12, marginTop: 14 }}>
          {[
            { l: 'Sales', v: '$842', d: '+18%', a: true },
            { l: 'Orders', v: '14', d: '+3' },
            { l: 'Visitors', v: '512', d: '+9%' },
            { l: 'Conv.', v: '2.7%', d: '−0.2' },
          ].map((k) => (
            <div key={k.l} className="wf-box wf-p-3" style={{ borderRadius: 8 }}>
              <div className="wf-tiny wf-muted">{k.l}</div>
              <div className="wf-stat-num wf-mt-2">{k.v}</div>
              <div className={`wf-tiny ${k.a ? 'wf-accent-text' : 'wf-muted'} wf-mt-2`}>{k.d}</div>
            </div>
          ))}
        </div>
        {/* charts row */}
        <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 12, marginTop: 14 }}>
          <div className="wf-box wf-p-3" style={{ borderRadius: 8 }}>
            <div className="wf-between"><span className="wf-small" style={{ fontWeight: 600 }}>Sales · 30d</span><Icon name="chev_r" size={12} /></div>
            <div style={{ marginTop: 8 }}>
              <LineChart w={520} h={120} accent points={[12, 20, 18, 28, 24, 32, 30, 34, 28, 36, 40, 38, 44, 50]} />
            </div>
          </div>
          <div className="wf-box wf-p-3" style={{ borderRadius: 8 }}>
            <div className="wf-small" style={{ fontWeight: 600 }}>Top sellers</div>
            <div className="wf-col wf-gap-2 wf-mt-2">
              {['Cardholder №3', 'Long Wallet', 'Keychain', 'Belt'].map((p, i) => (
                <div key={p} className="wf-between">
                  <div className="wf-row wf-gap-2" style={{ alignItems: 'center' }}>
                    <ImgBox w={22} h={22} plain />
                    <span className="wf-tiny">{p}</span>
                  </div>
                  <span className="wf-tiny wf-muted">{[12, 8, 5, 3][i]}</span>
                </div>
              ))}
            </div>
          </div>
        </div>
        {/* activity */}
        <div className="wf-box wf-p-3" style={{ borderRadius: 8, marginTop: 12 }}>
          <div className="wf-small" style={{ fontWeight: 600 }}>Recent activity</div>
          <div className="wf-col wf-gap-2 wf-mt-2">
            {[
              ['Order #1042 — $58 · Cardholder №3', '2 min ago'],
              ['New review · Long Wallet · ★★★★★', '14 min'],
              ['Inventory low · Keychain (3 left)', '1h'],
              ['Order #1041 shipped', '3h'],
            ].map(([t, m]) => (
              <div key={t} className="wf-between" style={{ padding: '4px 0', borderBottom: '1px dashed var(--ink-faint)' }}>
                <span className="wf-tiny">{t}</span>
                <span className="wf-tiny wf-muted">{m}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
    <Anno x={200} y={70} dir="down">KPI grid · charts · feed</Anno>
  </Desktop>
);

const Dashboard_TodayFocus = () => (
  <Desktop url="micro.shop/dashboard">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="overview" />
      <div className="wf-grow wf-col" style={{ padding: '24px 32px', gap: 18, overflow: 'hidden' }}>
        <div>
          <span className="wf-eyebrow">Tuesday, May 8</span>
          <Heading size="h1" style={{ fontSize: 56, lineHeight: 1, marginTop: 6 }}>$842</Heading>
          <div className="wf-muted wf-small">14 orders · 6 unfulfilled</div>
        </div>
        {/* needs attention */}
        <div>
          <Heading size="h3">Needs you</Heading>
          <div className="wf-col wf-gap-2 wf-mt-2">
            {[
              { i: 'box', t: '6 orders to ship', s: '2 are late', urgent: true },
              { i: 'bell', t: '3 messages', s: 'Customer questions' },
              { i: 'chart', t: 'Keychain almost out', s: '3 left in stock' },
            ].map((row) => (
              <div key={row.t} className="wf-box" style={{ padding: '12px 14px', borderRadius: 8, borderColor: row.urgent ? 'var(--accent)' : 'var(--ink-faint)', borderWidth: row.urgent ? 1.8 : 1.2, display: 'flex', alignItems: 'center', gap: 12 }}>
                <Icon name={row.i} size={18} />
                <div className="wf-grow">
                  <div style={{ fontSize: 14, fontWeight: 600 }}>{row.t}</div>
                  <div className="wf-tiny wf-muted">{row.s}</div>
                </div>
                <Btn sm accent={row.urgent} fill={!row.urgent}>Open</Btn>
              </div>
            ))}
          </div>
        </div>
        {/* gentle trend */}
        <div className="wf-box wf-p-3" style={{ borderRadius: 8 }}>
          <div className="wf-between"><span className="wf-tiny wf-muted">7 days</span><span className="wf-tiny">+18% week / week</span></div>
          <LineChart w={520} h={70} points={[14, 18, 16, 22, 20, 28, 32]} accent />
        </div>
        <div style={{ flex: 1 }} />
      </div>
    </div>
    <Anno x={210} y={56} dir="down">One number · what to do next</Anno>
  </Desktop>
);

const Dashboard_DataDense = () => (
  <Desktop url="micro.shop/dashboard">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="overview" />
      <div className="wf-grow" style={{ padding: '14px 18px', overflow: 'hidden' }}>
        <div className="wf-between">
          <div className="wf-row wf-gap-2 wf-tiny" style={{ alignItems: 'center' }}>
            <span style={{ fontWeight: 600 }}>Overview</span>
            <span className="wf-muted">·</span>
            <span className="wf-muted">May 1 – 8</span>
          </div>
          <div className="wf-row wf-gap-2"><Icon name="filter" size={14} /><Icon name="upload" size={14} /></div>
        </div>
        {/* data dense grid */}
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(6,1fr)', gap: 8, marginTop: 10 }}>
          {[
            ['Revenue', '$5,820', [10, 12, 18, 14, 22, 28, 26, 32]],
            ['Orders', '94', [4, 6, 5, 8, 7, 10, 12, 14]],
            ['AOV', '$62', [60, 58, 64, 62, 65, 60, 64, 66]],
            ['Visitors', '3.4k', [200, 240, 280, 260, 320, 380, 340, 420]],
            ['Conv.', '2.7%', [2.1, 2.3, 2.5, 2.4, 2.7, 2.6, 2.8, 2.7]],
            ['Returns', '2', [0, 1, 0, 1, 0, 0, 1, 0]],
          ].map(([l, v, pts]) => (
            <div key={l} className="wf-box" style={{ padding: 10, borderRadius: 6 }}>
              <div className="wf-tiny wf-muted" style={{ fontSize: 11 }}>{l}</div>
              <div style={{ fontFamily: 'var(--hand-display)', fontWeight: 700, fontSize: 22, marginTop: 2 }}>{v}</div>
              <div style={{ marginTop: 4 }}><LineChart w={120} h={24} points={pts} /></div>
            </div>
          ))}
        </div>
        {/* table */}
        <div className="wf-box" style={{ marginTop: 12, borderRadius: 6, overflow: 'hidden' }}>
          <div style={{ display: 'grid', gridTemplateColumns: '60px 1fr 70px 70px 80px 80px 70px', padding: '8px 12px', background: 'var(--paper-2)', fontSize: 11, fontWeight: 600, color: 'var(--ink-soft)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
            <span>SKU</span><span>Product</span><span>Sold</span><span>Stock</span><span>Revenue</span><span>Conv.</span><span>Trend</span>
          </div>
          {[
            ['CH3', 'Cardholder №3', 24, 18, '$1,392', '4.2%'],
            ['LW1', 'Long Wallet', 12, 9, '$1,536', '2.8%'],
            ['KC1', 'Keychain Loop', 31, 3, '$589', '5.1%'],
            ['BL2', 'Belt — Bridle', 7, 22, '$686', '1.9%'],
            ['CH2', 'Cardholder №2', 5, 14, '$245', '1.2%'],
          ].map((row, i) => (
            <div key={i} style={{ display: 'grid', gridTemplateColumns: '60px 1fr 70px 70px 80px 80px 70px', padding: '7px 12px', fontSize: 12, borderTop: '1px solid var(--ink-faint)', alignItems: 'center' }}>
              <span className="wf-muted">{row[0]}</span>
              <span>{row[1]}</span>
              <span>{row[2]}</span>
              <span style={{ color: row[3] < 5 ? 'var(--accent)' : 'inherit' }}>{row[3]}</span>
              <span>{row[4]}</span>
              <span>{row[5]}</span>
              <span><LineChart w={50} h={14} points={[row[2] - 4, row[2] - 2, row[2] - 3, row[2], row[2] - 1, row[2] + 1]} /></span>
            </div>
          ))}
        </div>
      </div>
    </div>
    <Anno x={210} y={50} dir="down">Charts everywhere · table-led</Anno>
  </Desktop>
);

// ─── SELLER LISTINGS / Create-Edit (desktop, 3 variants) ─────────────────────

const Listings_Form = () => (
  <Desktop url="micro.shop/products/new">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="listings" />
      <div className="wf-grow" style={{ padding: '20px 32px', overflow: 'hidden' }}>
        <div className="wf-between">
          <div>
            <span className="wf-tiny wf-muted">Products / New</span>
            <Heading size="h2" style={{ marginTop: 2 }}>Add a product</Heading>
          </div>
          <div className="wf-row wf-gap-2"><Btn ghost>Save draft</Btn><Btn accent>Publish</Btn></div>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr', gap: 16, marginTop: 16 }}>
          <div className="wf-col wf-gap-3">
            <div className="wf-box wf-p-4" style={{ borderRadius: 8 }}>
              <span className="wf-eyebrow">Basics</span>
              <div className="wf-mt-2">
                <div className="wf-tiny wf-muted">Name</div>
                <div className="wf-box" style={{ height: 32, marginTop: 4, padding: '0 10px', display: 'flex', alignItems: 'center', borderRadius: 6 }}>Cardholder №3</div>
              </div>
              <div className="wf-mt-3">
                <div className="wf-tiny wf-muted">Tagline</div>
                <div className="wf-box" style={{ height: 32, marginTop: 4, padding: '0 10px', display: 'flex', alignItems: 'center', borderRadius: 6 }}>Cut from a single piece of leather.</div>
              </div>
              <div className="wf-mt-3">
                <div className="wf-tiny wf-muted">Description</div>
                <div className="wf-box" style={{ minHeight: 80, marginTop: 4, padding: 10, borderRadius: 6 }}>
                  <Lines n={3} h={6} />
                </div>
              </div>
            </div>
            <div className="wf-box wf-p-4" style={{ borderRadius: 8 }}>
              <span className="wf-eyebrow">Photos</span>
              <div className="wf-row wf-gap-2 wf-mt-2">
                {[1, 2, 3].map((i) => <ImgBox key={i} w={100} h={100} />)}
                <div className="wf-box wf-center" style={{ width: 100, height: 100, borderRadius: 6, borderStyle: 'dashed', flexDirection: 'column', gap: 4 }}>
                  <Icon name="plus" />
                  <span className="wf-tiny wf-muted">Add</span>
                </div>
              </div>
            </div>
            <div className="wf-box wf-p-4" style={{ borderRadius: 8 }}>
              <span className="wf-eyebrow">Variants</span>
              <div className="wf-row wf-gap-2 wf-mt-2">
                <div className="wf-box wf-p-2" style={{ borderRadius: 6, fontSize: 12 }}>Color: 3</div>
                <div className="wf-box wf-p-2" style={{ borderRadius: 6, fontSize: 12 }}>+ Add option</div>
              </div>
            </div>
          </div>
          <div className="wf-col wf-gap-3">
            <div className="wf-box wf-p-4" style={{ borderRadius: 8 }}>
              <span className="wf-eyebrow">Price</span>
              <div className="wf-row wf-gap-2 wf-mt-2">
                <div className="wf-box wf-grow" style={{ height: 32, padding: '0 10px', display: 'flex', alignItems: 'center', borderRadius: 6 }}>$58.00</div>
              </div>
              <div className="wf-tiny wf-muted wf-mt-2">Compare at <span className="wf-ink">$65</span></div>
            </div>
            <div className="wf-box wf-p-4" style={{ borderRadius: 8 }}>
              <span className="wf-eyebrow">Inventory</span>
              <div className="wf-mt-2 wf-tiny">SKU: <b>CH3-BLK</b></div>
              <div className="wf-tiny">In stock: <b>18</b></div>
              <div className="wf-tiny wf-muted">Track inventory ✓</div>
            </div>
            <div className="wf-box wf-p-4" style={{ borderRadius: 8 }}>
              <span className="wf-eyebrow">Visibility</span>
              <div className="wf-row wf-gap-2 wf-mt-2"><Chip on>Active</Chip><Chip>Draft</Chip></div>
            </div>
          </div>
        </div>
      </div>
    </div>
    <Anno x={210} y={62} dir="down">Two-column form · canonical</Anno>
  </Desktop>
);

const Listings_LivePreview = () => (
  <Desktop url="micro.shop/products/cardholder-3/edit">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="listings" />
      <div className="wf-grow wf-row" style={{ overflow: 'hidden' }}>
        {/* left: form */}
        <div style={{ width: 380, padding: '18px 18px', borderRight: '1px solid var(--ink-faint)', overflow: 'hidden' }}>
          <div className="wf-between">
            <Heading size="h3">Edit product</Heading>
            <Btn sm accent>Save</Btn>
          </div>
          <div className="wf-col wf-gap-3 wf-mt-3">
            <div>
              <div className="wf-tiny wf-muted">Name</div>
              <div className="wf-box wf-mt-2 wf-p-2" style={{ borderRadius: 6 }}>Cardholder №3</div>
            </div>
            <div>
              <div className="wf-tiny wf-muted">Tagline</div>
              <div className="wf-box wf-mt-2 wf-p-2 wf-accent-border" style={{ borderRadius: 6, borderWidth: 1.8 }}>Cut from a single piece of leather.</div>
            </div>
            <div>
              <div className="wf-tiny wf-muted">Photos</div>
              <div className="wf-row wf-gap-2 wf-mt-2">
                <ImgBox w={56} h={56} /><ImgBox w={56} h={56} /><ImgBox w={56} h={56} />
                <div className="wf-box wf-center" style={{ width: 56, height: 56, borderStyle: 'dashed', borderRadius: 6 }}><Icon name="plus" size={14} /></div>
              </div>
            </div>
            <div>
              <div className="wf-tiny wf-muted">Price</div>
              <div className="wf-box wf-mt-2 wf-p-2" style={{ borderRadius: 6 }}>$58</div>
            </div>
            <div>
              <div className="wf-tiny wf-muted">Color variants</div>
              <div className="wf-row wf-gap-2 wf-mt-2">
                <Chip on>Black</Chip><Chip on>Tan</Chip><Chip on>Cognac</Chip>
                <Chip>+ add</Chip>
              </div>
            </div>
          </div>
        </div>
        {/* right: preview */}
        <div style={{ flex: 1, background: 'var(--paper-2)', padding: 20, position: 'relative', overflow: 'hidden' }}>
          <div className="wf-between" style={{ marginBottom: 12 }}>
            <span className="wf-eyebrow">Preview · live</span>
            <div className="wf-row wf-gap-2"><Chip on>Desktop</Chip><Chip>Mobile</Chip></div>
          </div>
          <div className="wf-box" style={{ background: 'var(--paper)', padding: 20, borderRadius: 8 }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1.2fr 1fr', gap: 18, alignItems: 'start' }}>
              <ImgBox h={180} />
              <div>
                <Heading size="h2">Cardholder №3</Heading>
                <div className="wf-muted wf-small">Cut from a single piece of leather.</div>
                <div style={{ fontSize: 22, fontFamily: 'var(--hand-display)', fontWeight: 700, marginTop: 8 }}>$58</div>
                <div className="wf-row wf-gap-2 wf-mt-2">
                  {['#1d1d1f', '#5a3a26', '#a87a4d'].map((c) => (
                    <div key={c} style={{ width: 22, height: 22, borderRadius: '50%', background: c, border: '1px solid var(--ink-faint)' }} />
                  ))}
                </div>
                <Btn accent style={{ marginTop: 12 }}>Add to bag</Btn>
              </div>
            </div>
          </div>
          <div className="wf-tiny wf-muted wf-mt-2">Updates as you type</div>
        </div>
      </div>
    </div>
    <Anno x={420} y={50} dir="down">Edit · preview side-by-side</Anno>
  </Desktop>
);

const Listings_Wizard = () => {
  const Step = ({ n, t, done, active }) => (
    <div className="wf-row wf-gap-2" style={{ alignItems: 'center', opacity: done || active ? 1 : 0.5 }}>
      <div style={{
        width: 26, height: 26, borderRadius: 13, border: `1.4px solid ${active ? 'var(--accent)' : done ? 'var(--ink)' : 'var(--ink-faint)'}`,
        background: done ? 'var(--ink)' : active ? 'var(--accent)' : 'transparent', color: done || active ? 'white' : 'var(--ink-2)',
        display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 12, fontWeight: 700,
      }}>
        {done ? '✓' : n}
      </div>
      <span style={{ fontWeight: active ? 600 : 400 }}>{t}</span>
    </div>
  );
  return (
    <Desktop url="micro.shop/products/new">
      <div className="wf-row" style={{ height: '100%' }}>
        <SellerNav active="listings" />
        <div className="wf-grow wf-col" style={{ padding: '24px 60px', gap: 24, alignItems: 'stretch', overflow: 'hidden' }}>
          {/* stepper */}
          <div className="wf-row" style={{ justifyContent: 'space-between', maxWidth: 580, margin: '0 auto', width: '100%' }}>
            <Step n={1} t="Photos" done />
            <div style={{ flex: 1, height: 1, background: 'var(--ink)', alignSelf: 'center', margin: '0 8px' }} />
            <Step n={2} t="Details" active />
            <div style={{ flex: 1, height: 1, background: 'var(--ink-faint)', alignSelf: 'center', margin: '0 8px' }} />
            <Step n={3} t="Pricing" />
            <div style={{ flex: 1, height: 1, background: 'var(--ink-faint)', alignSelf: 'center', margin: '0 8px' }} />
            <Step n={4} t="Review" />
          </div>
          {/* step content */}
          <div style={{ maxWidth: 580, width: '100%', margin: '0 auto' }}>
            <Heading size="h1" style={{ fontSize: 30 }}>Tell us about it.</Heading>
            <div className="wf-muted wf-small wf-mt-2">Three quick fields. You can polish later.</div>
            <div className="wf-col wf-gap-4 wf-mt-6">
              <div>
                <div className="wf-eyebrow">Name</div>
                <div className="wf-box wf-mt-2" style={{ height: 44, padding: '0 12px', display: 'flex', alignItems: 'center', borderRadius: 8, fontSize: 16 }}>Cardholder №3</div>
              </div>
              <div>
                <div className="wf-eyebrow">One-line tagline</div>
                <div className="wf-box wf-mt-2 wf-accent-border" style={{ height: 44, padding: '0 12px', display: 'flex', alignItems: 'center', borderRadius: 8, fontSize: 16, borderWidth: 2 }}>Cut from a single piece of leather.</div>
                <div className="wf-tiny wf-muted wf-mt-2">Tip: keep it under 6 words.</div>
              </div>
              <div>
                <div className="wf-eyebrow">Category</div>
                <div className="wf-row wf-gap-2 wf-mt-2">
                  <Chip on>Wallets</Chip><Chip>Belts</Chip><Chip>Bags</Chip><Chip>Other</Chip>
                </div>
              </div>
            </div>
          </div>
          <div style={{ flex: 1 }} />
          <div className="wf-row wf-between" style={{ borderTop: '1px solid var(--ink-faint)', paddingTop: 16 }}>
            <Btn ghost>Back</Btn>
            <span className="wf-tiny wf-muted">Step 2 of 4</span>
            <Btn accent>Next: pricing</Btn>
          </div>
        </div>
      </div>
      <Anno x={290} y={56} dir="down">Wizard · one thing at a time</Anno>
    </Desktop>
  );
};

// ─── SELLER ANALYTICS (desktop, 3 variants) ──────────────────────────────────

const Analytics_Grid = () => (
  <Desktop url="micro.shop/analytics">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="analytics" />
      <div className="wf-grow" style={{ padding: '18px 24px', overflow: 'hidden' }}>
        <div className="wf-between">
          <Heading size="h2">Analytics</Heading>
          <div className="wf-row wf-gap-2"><Chip>7d</Chip><Chip on>30d</Chip><Chip>90d</Chip><Chip>YTD</Chip></div>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2,1fr)', gap: 12, marginTop: 16 }}>
          {[
            { t: 'Revenue', v: '$5,820', d: '+18%', c: 'line', pts: [10, 14, 18, 16, 22, 28, 26, 32, 30, 38, 42, 48] },
            { t: 'Sessions', v: '3,412', d: '+9%', c: 'bar', pts: [12, 18, 14, 22, 16, 24, 28, 20, 26, 30, 24, 32] },
            { t: 'Conversion', v: '2.7%', d: '+0.3', c: 'line', pts: [2.0, 2.2, 2.1, 2.4, 2.5, 2.7, 2.6, 2.8, 2.7, 2.9, 2.7, 2.8] },
            { t: 'AOV', v: '$62', d: '−$2', c: 'line', pts: [60, 58, 64, 62, 66, 60, 64, 62, 65, 60, 62, 62] },
          ].map((m) => (
            <div key={m.t} className="wf-box wf-p-3" style={{ borderRadius: 8 }}>
              <div className="wf-between">
                <div>
                  <div className="wf-tiny wf-muted">{m.t}</div>
                  <div className="wf-stat-num">{m.v}</div>
                </div>
                <span className="wf-tiny wf-accent-text">{m.d}</span>
              </div>
              <div style={{ marginTop: 8 }}>
                {m.c === 'line'
                  ? <LineChart w={400} h={90} points={m.pts} accent />
                  : <BarChart w={400} h={90} values={m.pts} />}
              </div>
            </div>
          ))}
        </div>
        <div className="wf-row wf-gap-3 wf-mt-3">
          <div className="wf-box wf-p-3 wf-grow" style={{ borderRadius: 8 }}>
            <div className="wf-small" style={{ fontWeight: 600 }}>Traffic by source</div>
            <div className="wf-col wf-gap-2 wf-mt-2 wf-tiny">
              {[['Direct', 42], ['Instagram', 28], ['Search', 18], ['Email', 12]].map(([k, v]) => (
                <div key={k}>
                  <div className="wf-between"><span>{k}</span><span className="wf-muted">{v}%</span></div>
                  <div style={{ height: 4, borderRadius: 2, background: 'var(--ink-faint)', marginTop: 2 }}>
                    <div className="wf-accent-bg" style={{ width: `${v}%`, height: '100%', borderRadius: 2 }} />
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
    <Anno x={210} y={50} dir="down">Charts grid · classic</Anno>
  </Desktop>
);

const Analytics_BigChart = () => (
  <Desktop url="micro.shop/analytics">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="analytics" />
      <div className="wf-grow" style={{ padding: '18px 24px', overflow: 'hidden' }}>
        <div className="wf-between">
          <div>
            <span className="wf-tiny wf-muted">Apr 8 – May 8</span>
            <Heading size="h1" style={{ fontSize: 36, marginTop: 2 }}>$5,820</Heading>
            <div className="wf-tiny wf-accent-text">+18% vs prior 30d</div>
          </div>
          <div className="wf-row wf-gap-2">
            <Chip on>Revenue</Chip><Chip>Orders</Chip><Chip>Visitors</Chip><Chip>Conv.</Chip>
          </div>
        </div>
        <div className="wf-box wf-p-3" style={{ borderRadius: 8, marginTop: 14 }}>
          <LineChart w={760} h={220} points={[40, 52, 48, 62, 58, 70, 68, 74, 80, 78, 84, 92, 88, 96, 102, 110, 108, 116, 124, 130, 128, 138, 142, 150, 158, 162, 170, 174, 182, 196]} accent />
        </div>
        {/* breakdown */}
        <div style={{ marginTop: 14 }}>
          <div className="wf-between" style={{ marginBottom: 8 }}>
            <span className="wf-small" style={{ fontWeight: 600 }}>By product</span>
            <Icon name="filter" size={14} />
          </div>
          <div className="wf-box" style={{ borderRadius: 8, overflow: 'hidden' }}>
            {[
              ['Cardholder №3', 1392, 24, '+22%'],
              ['Long Wallet', 1536, 12, '+8%'],
              ['Keychain Loop', 589, 31, '+34%'],
              ['Belt — Bridle', 686, 7, '−4%'],
            ].map(([n, r, q, d], i) => (
              <div key={n} style={{ display: 'grid', gridTemplateColumns: '1.6fr 1fr 100px 80px 80px', gap: 8, padding: '10px 14px', borderTop: i ? '1px solid var(--ink-faint)' : 'none', alignItems: 'center', fontSize: 13 }}>
                <span style={{ fontWeight: 500 }}>{n}</span>
                <div style={{ height: 6, borderRadius: 3, background: 'var(--ink-faint)' }}>
                  <div className="wf-accent-bg" style={{ width: `${(r / 1600) * 100}%`, height: '100%', borderRadius: 3 }} />
                </div>
                <span style={{ textAlign: 'right' }}>${r}</span>
                <span className="wf-muted" style={{ textAlign: 'right' }}>{q} sold</span>
                <span className="wf-tiny wf-accent-text" style={{ textAlign: 'right' }}>{d}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
    <Anno x={300} y={56} dir="down">One big chart · breakdown table</Anno>
  </Desktop>
);

const Analytics_Story = () => (
  <Desktop url="micro.shop/analytics">
    <div className="wf-row" style={{ height: '100%' }}>
      <SellerNav active="analytics" />
      <div className="wf-grow" style={{ padding: '24px 40px', overflow: 'hidden' }}>
        <span className="wf-eyebrow">Week of May 1</span>
        <Heading size="h1" style={{ fontSize: 30, marginTop: 6 }}>3 things to know</Heading>
        <div className="wf-col wf-gap-3 wf-mt-4">
          {/* insight 1 */}
          <div className="wf-box wf-p-4" style={{ borderRadius: 10, display: 'grid', gridTemplateColumns: '36px 1fr 200px', gap: 16, alignItems: 'center' }}>
            <div className="wf-accent-text wf-h2" style={{ fontSize: 28 }}>1</div>
            <div>
              <div style={{ fontFamily: 'var(--hand-display)', fontSize: 20, fontWeight: 600, lineHeight: 1.15 }}>
                Keychain sales are up <span className="wf-accent-text">34%</span>.
              </div>
              <div className="wf-muted wf-small wf-mt-2">
                Mostly from Instagram. Three customers bought more than one. Stock is at 3 — consider restocking.
              </div>
            </div>
            <BarChart w={180} h={50} values={[6, 8, 10, 12, 14, 18, 22]} accent />
          </div>
          <div className="wf-box wf-p-4" style={{ borderRadius: 10, display: 'grid', gridTemplateColumns: '36px 1fr 200px', gap: 16, alignItems: 'center' }}>
            <div className="wf-accent-text wf-h2" style={{ fontSize: 28 }}>2</div>
            <div>
              <div style={{ fontFamily: 'var(--hand-display)', fontSize: 20, fontWeight: 600, lineHeight: 1.15 }}>
                Belt page traffic is high but conv. is <span className="wf-accent-text">low</span>.
              </div>
              <div className="wf-muted wf-small wf-mt-2">
                412 visits, 7 sales. Try clearer sizing photos — that's the question 4 customers asked.
              </div>
            </div>
            <LineChart w={180} h={50} points={[14, 12, 10, 8, 6, 5, 4]} />
          </div>
          <div className="wf-box wf-p-4" style={{ borderRadius: 10, display: 'grid', gridTemplateColumns: '36px 1fr 200px', gap: 16, alignItems: 'center' }}>
            <div className="wf-accent-text wf-h2" style={{ fontSize: 28 }}>3</div>
            <div>
              <div style={{ fontFamily: 'var(--hand-display)', fontSize: 20, fontWeight: 600, lineHeight: 1.15 }}>
                Returning customers up to <span className="wf-accent-text">22%</span>.
              </div>
              <div className="wf-muted wf-small wf-mt-2">
                People who bought a Cardholder are coming back for the Long Wallet. Your bundles email is working.
              </div>
            </div>
            <LineChart w={180} h={50} points={[8, 10, 12, 14, 16, 20, 22]} accent />
          </div>
        </div>
        <div className="wf-row wf-gap-2 wf-mt-4">
          <Btn ghost>See all metrics</Btn>
          <Btn accent>Email this digest</Btn>
        </div>
      </div>
    </div>
    <Anno x={250} y={50} dir="down">Insights · narrated cards</Anno>
  </Desktop>
);

Object.assign(window, {
  Dashboard_KPI, Dashboard_TodayFocus, Dashboard_DataDense,
  Listings_Form, Listings_LivePreview, Listings_Wizard,
  Analytics_Grid, Analytics_BigChart, Analytics_Story,
});

// hifi-analytics.jsx — 3 desktop seller analytics views (1280x800)

// ── A · Standard dashboard — KPIs + charts
function Analytics_Standard() {
  const Stat = ({ l, v, d, t = 'up', spark, sparkColor }) => (
    <div className="hf-card" style={{ padding: 18 }}>
      <div className="hf-tiny hf-muted" style={{ marginBottom: 6 }}>{l}</div>
      <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1 }}>{v}</div>
      <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 6 }}>
        <span className="hf-tiny" style={{ color: t === 'up' ? 'var(--good)' : 'var(--bad)', fontWeight: 600 }}>
          {t === 'up' ? '↑' : '↓'} {d}
        </span>
        <span className="hf-tiny hf-muted">vs prev. period</span>
      </div>
      <div style={{ height: 28, marginTop: 10, color: sparkColor || 'var(--ink-3)' }}>
        <Spark data={spark} color="currentColor" fill="rgba(21,18,14,0.05)" sw={1.5} />
      </div>
    </div>
  );
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Analytics"
          subtitle="Apr 1 – Apr 30 · vs Mar 1 – Mar 30"
          actions={<>
            <div className="hf-flex hf-gap-1" style={{ background: 'var(--paper-2)', padding: 3, borderRadius: 8 }}>
              {['7d', '30d', '90d', 'Year'].map((p, i) => (
                <button key={p} className="hf-btn hf-btn-sm" style={{ background: i === 1 ? 'var(--card)' : 'transparent', boxShadow: i === 1 ? 'var(--shadow-1)' : 'none' }}>{p}</button>
              ))}
            </div>
            <button className="hf-btn hf-btn-outline"><Ico n="dl" s={12} /> Export</button>
          </>}
        />
        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 20 }}>
            <Stat l="Revenue" v={money(12480)} d="14%" spark={[0.3,0.4,0.45,0.55,0.5,0.7,0.65,0.85]} />
            <Stat l="Orders"  v="124" d="9%" spark={[0.4,0.5,0.45,0.6,0.55,0.7,0.75,0.8]} />
            <Stat l="Conversion" v="3.4%" d="0.6 pp" spark={[0.5,0.55,0.5,0.6,0.7,0.65,0.75,0.8]} />
            <Stat l="Avg. order" v={money(112)} d="3%" t="down" spark={[0.7,0.65,0.6,0.55,0.5,0.55,0.6,0.5]} sparkColor="var(--bad)" />
          </div>

          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '2fr 1fr' }}>
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-flex hf-between" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-h3">Revenue over time</div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>Daily, by source</div>
                </div>
                <div className="hf-flex hf-gap-3 hf-tiny">
                  <span className="hf-flex hf-items-center hf-gap-1"><span className="hf-dot" style={{ background: 'var(--ink)' }} /> Organic</span>
                  <span className="hf-flex hf-items-center hf-gap-1"><span className="hf-dot" style={{ background: 'var(--terra)' }} /> Social</span>
                  <span className="hf-flex hf-items-center hf-gap-1"><span className="hf-dot" style={{ background: 'var(--ink-4)' }} /> Direct</span>
                </div>
              </div>
              <div style={{ height: 220, position: 'relative' }}>
                <AreaChart data={[120,180,160,220,260,200,290,260,310,360,330,420,380,460,500,540,520,580,620,640,680,720,700,780,820,860,840,920,950,1020]} color="var(--ink)" fill="rgba(21,18,14,0.06)" />
                <div style={{ position: 'absolute', inset: 0, opacity: 0.5 }}>
                  <AreaChart data={[40,60,55,70,90,80,110,100,130,150,140,170,160,200,220,240,230,260,280,290,300,330,320,350,380,400,390,420,440,470]} color="var(--terra)" fill="rgba(194,65,12,0.05)" />
                </div>
              </div>
              <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
                <span>Apr 1</span><span>Apr 8</span><span>Apr 15</span><span>Apr 22</span><span>Apr 30</span>
              </div>
            </div>

            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 14 }}>Sources</div>
              <div className="hf-center" style={{ marginBottom: 14 }}>
                <div style={{ position: 'relative' }}>
                  <Donut size={140} thickness={20} segments={[
                    { value: 48, color: 'var(--ink)' },
                    { value: 28, color: 'var(--terra)' },
                    { value: 14, color: 'var(--forest)' },
                    { value: 10, color: 'var(--ink-4)' },
                  ]} />
                  <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center' }}>
                    <div className="hf-tiny hf-muted">Sessions</div>
                    <div className="hf-display-2 hf-num" style={{ fontSize: 22 }}>4.8k</div>
                  </div>
                </div>
              </div>
              <div className="hf-col hf-gap-2">
                {[
                  { l: 'Organic search', v: '48%', n: '2,304', c: 'var(--ink)' },
                  { l: 'Social', v: '28%', n: '1,344', c: 'var(--terra)' },
                  { l: 'Direct', v: '14%', n: '672', c: 'var(--forest)' },
                  { l: 'Email', v: '10%', n: '480', c: 'var(--ink-4)' },
                ].map(s => (
                  <div key={s.l} className="hf-flex hf-items-center hf-gap-2 hf-small">
                    <span className="hf-dot" style={{ background: s.c }} />
                    <span className="hf-grow">{s.l}</span>
                    <span className="hf-num hf-muted">{s.n}</span>
                    <span className="hf-num" style={{ width: 40, textAlign: 'right', fontWeight: 500 }}>{s.v}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr', marginTop: 20 }}>
            {/* top products */}
            <div className="hf-card" style={{ padding: 0 }}>
              <div className="hf-flex hf-between" style={{ padding: '14px 18px', borderBottom: '1px solid var(--line)' }}>
                <span className="hf-h3">Top products</span>
                <a className="hf-small" style={{ fontWeight: 500 }}>All →</a>
              </div>
              {[
                { t: 'Persimmon vase', tone: 'clay', units: 14, rev: 1204, share: 0.85 },
                { t: 'Cream tumbler set', tone: 'bone', units: 12, rev: 576, share: 0.62 },
                { t: 'Forest bowl, lg.', tone: 'sage', units: 9, rev: 576, share: 0.58 },
                { t: 'Bone dinner plate', tone: 'cream', units: 8, rev: 304, share: 0.46 },
              ].map((p, i) => (
                <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '12px 18px', borderBottom: i < 3 ? '1px solid var(--line)' : 'none' }}>
                  <ProdImg tone={p.tone} h={36} r={6} />
                  <div className="hf-grow">
                    <div className="hf-h4">{p.t}</div>
                    <div className="hf-tiny hf-muted">{p.units} sold</div>
                  </div>
                  <div style={{ width: 140 }}>
                    <div className="hf-progress"><i style={{ width: `${p.share * 100}%` }} /></div>
                  </div>
                  <span className="hf-num hf-h4" style={{ width: 66, textAlign: 'right' }}>{money(p.rev)}</span>
                </div>
              ))}
            </div>

            {/* funnel */}
            <div className="hf-card" style={{ padding: 20 }}>
              <div className="hf-h3" style={{ marginBottom: 14 }}>Conversion funnel</div>
              <div className="hf-col hf-gap-3">
                {[
                  { l: 'Storefront views', v: 4830, p: 1.0 },
                  { l: 'Product views', v: 2104, p: 0.44 },
                  { l: 'Added to cart', v: 412, p: 0.085 },
                  { l: 'Checkout started', v: 218, p: 0.045 },
                  { l: 'Purchased', v: 124, p: 0.026 },
                ].map((s, i) => (
                  <div key={i}>
                    <div className="hf-flex hf-between" style={{ marginBottom: 4 }}>
                      <span className="hf-small" style={{ fontWeight: 500 }}>{s.l}</span>
                      <span className="hf-flex hf-gap-2 hf-tiny">
                        <span className="hf-num hf-muted">{(s.p * 100).toFixed(1)}%</span>
                        <span className="hf-num" style={{ color: 'var(--ink)', fontWeight: 600 }}>{s.v.toLocaleString()}</span>
                      </span>
                    </div>
                    <div style={{ height: 22, borderRadius: 4, background: 'var(--paper-2)', position: 'relative', overflow: 'hidden' }}>
                      <div style={{ width: `${s.p * 100}%`, height: '100%', background: i === 4 ? 'var(--terra)' : 'var(--ink)', transition: 'width .3s' }} />
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>standard KPIs</Anno>
    </div>
  );
}

// ── B · Cohort/explorer — wide table with inline sparklines, breakdown by category
function Analytics_Explorer() {
  const cats = [
    { c: 'Vessels',    rev: 4820, d: 18, units: 38, conv: 4.2, spark: [0.3,0.4,0.5,0.45,0.55,0.6,0.7,0.65,0.8,0.85] },
    { c: 'Tableware',  rev: 3210, d: 12, units: 64, conv: 3.6, spark: [0.4,0.45,0.5,0.55,0.5,0.6,0.65,0.6,0.7,0.75] },
    { c: 'Drinkware',  rev: 1820, d: -4, units: 38, conv: 2.8, spark: [0.6,0.55,0.5,0.45,0.5,0.55,0.4,0.45,0.5,0.4] },
    { c: 'Limited',    rev: 1640, d: 42, units: 12, conv: 5.4, spark: [0.2,0.3,0.4,0.5,0.55,0.7,0.85,0.9,0.95,1.0] },
    { c: 'Gifts',      rev: 990,  d: 8,  units: 22, conv: 3.2, spark: [0.5,0.55,0.5,0.6,0.55,0.65,0.6,0.7,0.65,0.75] },
  ];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Explorer"
          subtitle="Slice by category, source, time"
          actions={<><button className="hf-btn hf-btn-outline"><Ico n="plus" s={12} /> Saved view</button><button className="hf-btn hf-btn-outline"><Ico n="dl" s={12} /> Export</button></>}
        />

        <div className="hf-flex hf-items-center" style={{ padding: '12px 28px', borderBottom: '1px solid var(--line)', gap: 8 }}>
          <span className="hf-tiny hf-muted" style={{ marginRight: 4 }}>FILTERS</span>
          {[
            { l: 'Apr 1 – Apr 30', i: 'sett' },
            { l: 'Compare: prev. month', i: 'refresh' },
            { l: 'Source: All', i: 'filter' },
            { l: 'Channel: Online', i: 'filter' },
          ].map(f => (
            <span key={f.l} className="hf-chip"><Ico n={f.i} s={11} /> {f.l} <Ico n="x" s={10} /></span>
          ))}
          <span className="hf-chip" style={{ borderStyle: 'dashed', color: 'var(--ink-3)' }}><Ico n="plus" s={11} /> Add filter</span>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '20px 28px' }}>
          {/* big number + comparator */}
          <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 20 }}>
            <div>
              <div className="hf-eyebrow">Revenue · April</div>
              <div className="hf-display-2 hf-num" style={{ fontSize: 56, lineHeight: 1, marginTop: 4 }}>{money(12480)}</div>
              <div className="hf-flex hf-items-center hf-gap-2 hf-small" style={{ marginTop: 6 }}>
                <span className="hf-chip hf-chip-good" style={{ fontSize: 11 }}>↑ 14% MoM</span>
                <span className="hf-muted">+ {money(1530)} vs March</span>
              </div>
            </div>
            <div style={{ flexGrow: 1, marginLeft: 28, height: 90 }}>
              <AreaChart data={[40,80,60,120,100,160,140,200,180,240,220,280,260,320,300,360,340,400,420,460,440,500,520,560,580,620,640,680,720,760]} color="var(--ink)" fill="rgba(21,18,14,0.05)" />
            </div>
          </div>

          <div className="hf-card" style={{ padding: 0 }}>
            <div className="hf-flex hf-between" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
              <div className="hf-h3">By category</div>
              <div className="hf-flex hf-gap-1">
                {['Category', 'Product', 'Source', 'Customer'].map((t, i) => (
                  <span key={t} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ fontSize: 11 }}>{t}</span>
                ))}
              </div>
            </div>
            <table className="hf-table">
              <thead>
                <tr>
                  <th style={{ width: 220 }}>Category</th>
                  <th>Trend · 30d</th>
                  <th>Revenue</th>
                  <th>Δ</th>
                  <th>Units</th>
                  <th>Conv. rate</th>
                  <th>Share</th>
                </tr>
              </thead>
              <tbody>
                {cats.map(r => {
                  const total = cats.reduce((s, x) => s + x.rev, 0);
                  const share = r.rev / total;
                  return (
                    <tr key={r.c}>
                      <td style={{ color: 'var(--ink)', fontWeight: 500 }}>{r.c}</td>
                      <td><div style={{ width: 140, height: 28, color: r.d >= 0 ? 'var(--good)' : 'var(--bad)' }}><Spark data={r.spark} color="currentColor" fill="transparent" /></div></td>
                      <td className="hf-num" style={{ color: 'var(--ink)', fontWeight: 500 }}>{money(r.rev)}</td>
                      <td><span style={{ color: r.d >= 0 ? 'var(--good)' : 'var(--bad)', fontWeight: 600, fontSize: 11.5 }}>{r.d >= 0 ? '↑' : '↓'} {Math.abs(r.d)}%</span></td>
                      <td className="hf-num">{r.units}</td>
                      <td className="hf-num">{r.conv.toFixed(1)}%</td>
                      <td>
                        <div className="hf-flex hf-items-center hf-gap-2" style={{ width: 160 }}>
                          <div className="hf-progress hf-grow"><i style={{ width: `${share * 100}%` }} /></div>
                          <span className="hf-num hf-tiny" style={{ width: 30, textAlign: 'right' }}>{(share * 100).toFixed(0)}%</span>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>

          {/* heatmap — sales by hour/day */}
          <div className="hf-card" style={{ marginTop: 20, padding: 20 }}>
            <div className="hf-flex hf-between" style={{ marginBottom: 14 }}>
              <div className="hf-h3">When people shop</div>
              <span className="hf-tiny hf-muted">Sales by day &amp; hour · Apr</span>
            </div>
            <div className="hf-flex" style={{ gap: 8 }}>
              <div className="hf-col" style={{ paddingTop: 18, gap: 4, alignItems: 'flex-end' }}>
                {['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'].map(d => (
                  <div key={d} className="hf-tiny hf-muted" style={{ height: 18, lineHeight: '18px' }}>{d}</div>
                ))}
              </div>
              <div className="hf-grow">
                <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginBottom: 4 }}>
                  <span>0</span><span>4</span><span>8</span><span>12</span><span>16</span><span>20</span>
                </div>
                <div className="hf-grid hf-gap-1" style={{ gridTemplateColumns: 'repeat(24, 1fr)', gridTemplateRows: 'repeat(7, 18px)' }}>
                  {Array.from({ length: 7 * 24 }).map((_, i) => {
                    const day = Math.floor(i / 24);
                    const hr = i % 24;
                    const peak = (hr >= 9 && hr <= 21) ? 1 : 0.2;
                    const weekend = day >= 5 ? 1.4 : 1;
                    const noise = ((Math.sin(i * 1.3) + 1) / 2) * 0.6 + 0.2;
                    const v = Math.min(1, peak * weekend * noise);
                    return <div key={i} style={{ height: 18, background: `rgba(194,65,12,${v.toFixed(2)})`, borderRadius: 2 }} />;
                  })}
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>multi-dimensional explorer</Anno>
    </div>
  );
}

// ── C · Story-mode — 1 insight per "card", scroll through narratives
function Analytics_Story() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '20px 32px', borderBottom: '1px solid var(--line)' }}>
          <div>
            <div className="hf-eyebrow">Insights · April</div>
            <h1 className="hf-display" style={{ fontSize: 28, lineHeight: 1, marginTop: 4 }}>Five things to know this month</h1>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-outline">Send to my email</button>
            <button className="hf-btn hf-btn-primary">View raw data</button>
          </div>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 32px 60px' }}>
          {/* story 1 — biggest mover */}
          <div className="hf-grid hf-gap-6" style={{ gridTemplateColumns: '1fr 1.3fr', marginBottom: 36, alignItems: 'center' }}>
            <div>
              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 10 }}>
                <div className="hf-center hf-display-2 hf-num" style={{ width: 32, height: 32, borderRadius: 999, background: 'var(--ink)', color: 'var(--paper)', fontSize: 14 }}>1</div>
                <div className="hf-eyebrow">Biggest mover</div>
              </div>
              <h2 className="hf-display" style={{ fontSize: 36, lineHeight: 1.05 }}>The <i>Persimmon vase</i> drove a third of revenue this month.</h2>
              <p className="hf-body" style={{ marginTop: 12, maxWidth: 460 }}>14 sold at full price, sold out by April 22. Pre-orders are now open with a 4-week lead. Consider raising the price by 10% on next batch.</p>
              <div className="hf-flex hf-gap-3" style={{ marginTop: 16 }}>
                <div>
                  <div className="hf-tiny hf-muted">Revenue</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 22 }}>{money(1204)}</div>
                </div>
                <div>
                  <div className="hf-tiny hf-muted">Share of total</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 22 }}>33%</div>
                </div>
                <div>
                  <div className="hf-tiny hf-muted">Repeat buyers</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 22 }}>4</div>
                </div>
              </div>
            </div>
            <ProdImg tone="clay" h={300} r="var(--r-lg)" />
          </div>

          {/* story 2 — mobile/desktop */}
          <div className="hf-card" style={{ padding: 28, marginBottom: 28, background: 'var(--paper-2)', border: 'none' }}>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 8 }}>
              <div className="hf-center hf-display-2 hf-num" style={{ width: 28, height: 28, borderRadius: 999, background: 'var(--ink)', color: 'var(--paper)', fontSize: 13 }}>2</div>
              <div className="hf-eyebrow">Where buyers come from</div>
            </div>
            <h2 className="hf-display" style={{ fontSize: 28, marginBottom: 14 }}>Instagram brought in <i>twice</i> the customers of search.</h2>
            <div className="hf-grid hf-gap-4" style={{ gridTemplateColumns: '1fr 1fr 1fr 1fr' }}>
              {[
                { l: 'Instagram', v: '52', s: '42% conv' },
                { l: 'Organic search', v: '24', s: '18% conv' },
                { l: 'Direct', v: '16', s: '24% conv' },
                { l: 'Email', v: '12', s: '38% conv' },
              ].map(s => (
                <div key={s.l} style={{ borderTop: '2px solid var(--ink)', paddingTop: 10 }}>
                  <div className="hf-tiny hf-muted">{s.l}</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1, marginTop: 4 }}>{s.v}</div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{s.s}</div>
                </div>
              ))}
            </div>
          </div>

          {/* story 3 — repeat customers */}
          <div className="hf-grid hf-gap-6" style={{ gridTemplateColumns: '1.3fr 1fr', marginBottom: 28, alignItems: 'center' }}>
            <div>
              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 8 }}>
                <div className="hf-center hf-display-2 hf-num" style={{ width: 28, height: 28, borderRadius: 999, background: 'var(--ink)', color: 'var(--paper)', fontSize: 13 }}>3</div>
                <div className="hf-eyebrow">Repeat customers</div>
              </div>
              <h2 className="hf-display" style={{ fontSize: 28, marginBottom: 12 }}>One in five buyers came back to buy more.</h2>
              <p className="hf-body" style={{ maxWidth: 480 }}>22 customers placed a second order this month. They spent on average {money(168)} per order — 50% more than first-time buyers.</p>
            </div>
            <div className="hf-card" style={{ padding: 18 }}>
              <div className="hf-flex hf-items-center hf-gap-3" style={{ marginBottom: 14 }}>
                <Donut size={80} thickness={14} segments={[
                  { value: 22, color: 'var(--terra)' },
                  { value: 102, color: 'var(--paper-2)' },
                ]} />
                <div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 22, lineHeight: 1 }}>22 / 124</div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>17.7% repeat rate</div>
                </div>
              </div>
              <div className="hf-divider" style={{ marginBottom: 14 }} />
              <div className="hf-col hf-gap-2 hf-small">
                {[
                  ['Avg. first order', money(112)],
                  ['Avg. repeat order', money(168)],
                  ['Most repeat-bought', 'Forest bowl, lg.'],
                ].map(([k, v]) => (
                  <div key={k} className="hf-flex hf-between"><span className="hf-muted">{k}</span><span style={{ color: 'var(--ink)', fontWeight: 500 }}>{v}</span></div>
                ))}
              </div>
            </div>
          </div>

          {/* story 4 — soft suggestion */}
          <div className="hf-card" style={{ padding: 24, border: '1px dashed var(--terra)', background: 'var(--terra-2)' }}>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 8 }}>
              <div className="hf-center" style={{ width: 28, height: 28, borderRadius: 999, background: 'var(--terra)', color: 'white', fontSize: 13, fontWeight: 600 }}>4</div>
              <div className="hf-eyebrow" style={{ color: 'var(--terra)' }}>Suggestion</div>
            </div>
            <h2 className="hf-display" style={{ fontSize: 24, marginBottom: 8, color: 'var(--ink)' }}>Drinkware is dropping. Worth a look?</h2>
            <p className="hf-body" style={{ maxWidth: 540 }}>Drinkware revenue is down 4% MoM, while every other category is up. The Cobalt mug Nº 02 has 3 listings stuck in draft from March — could that be the gap?</p>
            <div className="hf-flex hf-gap-2" style={{ marginTop: 14 }}>
              <button className="hf-btn hf-btn-primary">Open drafts</button>
              <button className="hf-btn hf-btn-ghost">Dismiss</button>
            </div>
          </div>
        </div>
      </div>
      <Anno top={20} right={20}>insight-led story</Anno>
    </div>
  );
}

Object.assign(window, { Analytics_Standard, Analytics_Explorer, Analytics_Story });

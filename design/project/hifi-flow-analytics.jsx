// hifi-flow-analytics.jsx — End-to-end analytics flow
// 5 desktop steps · 1280×800 · Mira investigates a Tuesday-traffic spike
//   then schedules a recurring report.

// shared period selector chip group
function _PeriodTabs({ active = 'Last 30 days' }) {
  return (
    <div className="hf-flex hf-gap-1" style={{ background: 'var(--paper-2)', padding: 3, borderRadius: 8 }}>
      {['7d', '30d', '90d', 'YTD'].map((p, i) => (
        <button key={p} className="hf-btn hf-btn-sm" style={{ background: i === 1 ? 'var(--paper)' : 'transparent', boxShadow: i === 1 ? 'var(--shadow-1)' : 'none', color: 'var(--ink)', minWidth: 44 }}>{p}</button>
      ))}
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 01 · OVERVIEW — KPIs, trend chart, anomaly badge calling out Tuesday
// ──────────────────────────────────────────────────────────────────────
function AFlow_01_Overview() {
  const trend = [220,260,240,300,340,320,380,400,360,420,460,440,510,560,540,620,720,680,740,820,1240,860,780,820,840,920,880,950,1020,990];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Analytics"
          subtitle="Last 30 days · Mar 12 → Apr 11"
          actions={<><_PeriodTabs /><button className="hf-btn hf-btn-outline hf-btn-sm">Export</button><Avatar name="Mira" size="sm" /></>}
        />

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* KPI row */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 18 }}>
            {[
              { l: 'Revenue',     v: '$8,420', d: '+24%', spark: [0.3,0.4,0.5,0.6,0.55,0.7,0.85,0.9], t: 'good' },
              { l: 'Orders',      v: '94',     d: '+18%', spark: [0.4,0.5,0.45,0.6,0.7,0.75,0.85,0.9], t: 'good' },
              { l: 'Conversion',  v: '3.4%',   d: '+0.6pt', spark: [0.5,0.55,0.5,0.6,0.7,0.75,0.8,0.85], t: 'good' },
              { l: 'Visitors',    v: '2,768',  d: '+9%',  spark: [0.4,0.5,0.55,0.5,0.6,0.65,0.7,0.78], t: 'good' },
            ].map((s) => (
              <div key={s.l} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-tiny hf-muted">{s.l}</div>
                <div className="hf-flex hf-items-end hf-between" style={{ marginTop: 6 }}>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 28, lineHeight: 1 }}>{s.v}</div>
                  <span className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600 }}>↑ {s.d}</span>
                </div>
                <div style={{ marginTop: 10, color: 'var(--good)' }}><Spark data={s.spark} /></div>
              </div>
            ))}
          </div>

          {/* Trend chart with anomaly */}
          <div className="hf-card" style={{ padding: 22, position: 'relative' }}>
            <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 14 }}>
              <div>
                <div className="hf-h3">Revenue · daily</div>
                <div className="hf-tiny hf-muted">Mar 12 → Apr 11 · vs prior period</div>
              </div>
              <div className="hf-flex hf-gap-3 hf-tiny">
                <span className="hf-flex hf-items-center hf-gap-1"><span style={{ width: 10, height: 2, background: 'var(--ink)' }} /> This period</span>
                <span className="hf-flex hf-items-center hf-gap-1"><span style={{ width: 10, height: 2, background: 'var(--ink-4)', borderTop: '1px dashed var(--ink-4)' }} /> Prior period</span>
              </div>
            </div>
            <AreaChart data={trend} height={200} />
            {/* anomaly callout */}
            <div className="hf-card" style={{ position: 'absolute', top: 90, right: 100, padding: '10px 14px', background: 'var(--paper)', border: '1.5px solid var(--warn)', boxShadow: 'var(--shadow-2)', maxWidth: 220 }}>
              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 4 }}>
                <span className="hf-dot hf-dot-warn" />
                <span className="hf-h4" style={{ fontSize: 12 }}>Anomaly · Apr 1</span>
              </div>
              <div className="hf-tiny hf-muted">$1,240 · <span style={{ color: 'var(--good)' }}>3.6× usual</span></div>
              <div className="hf-tiny" style={{ color: 'var(--ink-2)', fontWeight: 500, marginTop: 4 }}>Investigate →</div>
            </div>
          </div>

          {/* Insight cards row */}
          <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(3, 1fr)', marginTop: 18 }}>
            {[
              { e: 'Top product', t: 'Persimmon vase', v: '$2,180 · 23 sold', s: [0.3,0.4,0.5,0.6,0.7,0.75,0.85,0.9] },
              { e: 'Top channel', t: 'Instagram',     v: '38% of orders', s: [0.5,0.55,0.6,0.65,0.7,0.78,0.82,0.88] },
              { e: 'New customers', t: '64 of 94',     v: '68% first-time', s: [0.4,0.5,0.55,0.6,0.7,0.72,0.8,0.85] },
            ].map((c) => (
              <div key={c.e} className="hf-card" style={{ padding: 18 }}>
                <div className="hf-eyebrow" style={{ marginBottom: 6 }}>{c.e}</div>
                <div className="hf-h3" style={{ fontSize: 18 }}>{c.t}</div>
                <div className="hf-small hf-muted" style={{ marginTop: 4 }}>{c.v}</div>
                <div style={{ marginTop: 10, color: 'var(--ink-3)' }}><Spark data={c.s} /></div>
              </div>
            ))}
          </div>
        </div>
      </div>
      <DesktopAction>clicks Apr 1 anomaly · drills into the day</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 02 · DRILL — focus on Apr 1, hour-by-hour + traffic source
// ──────────────────────────────────────────────────────────────────────
function AFlow_02_Drill() {
  const hours = [4,6,8,12,18,24,38,52,86,142,168,164,148,124,98,82,68,54,46,38,30,24,18,12];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <div className="hf-flex hf-between hf-items-center" style={{ padding: '16px 28px', borderBottom: '1px solid var(--line)' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
            <div>
              <div className="hf-tiny hf-muted">Analytics · Anomaly drill-down</div>
              <h1 className="hf-display" style={{ fontSize: 24, lineHeight: 1, marginTop: 2 }}>Tuesday, Apr 1</h1>
            </div>
            <span className="hf-chip hf-chip-warn" style={{ marginLeft: 8 }}><span className="hf-dot hf-dot-warn" /> 3.6× usual</span>
          </div>
          <div className="hf-flex hf-gap-2">
            <button className="hf-btn hf-btn-outline hf-btn-sm"><Ico n="chevL" s={11} /> Apr 1</button>
            <button className="hf-btn hf-btn-outline hf-btn-sm">Apr 1 <Ico n="chevR" s={11} /></button>
          </div>
        </div>

        <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
          <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
            {/* day-of stats */}
            <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(4, 1fr)', marginBottom: 18 }}>
              {[
                { l: 'Revenue', v: '$1,240' },
                { l: 'Orders',  v: '14' },
                { l: 'Visitors', v: '486' },
                { l: 'Conversion', v: '2.9%' },
              ].map(s => (
                <div key={s.l} className="hf-card" style={{ padding: 16 }}>
                  <div className="hf-tiny hf-muted">{s.l}</div>
                  <div className="hf-display-2 hf-num" style={{ fontSize: 24, marginTop: 4 }}>{s.v}</div>
                </div>
              ))}
            </div>

            {/* hour-by-hour */}
            <div className="hf-card" style={{ padding: 22, marginBottom: 18 }}>
              <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 14 }}>
                <div>
                  <div className="hf-h3">Visitors · by hour</div>
                  <div className="hf-tiny hf-muted">Spike at 11am · likely external referrer</div>
                </div>
              </div>
              <div style={{ height: 140, position: 'relative' }}>
                <Bars data={hours} />
                <div style={{ position: 'absolute', top: 0, left: '40%', width: 2, height: '100%', background: 'var(--warn)' }} />
                <div style={{ position: 'absolute', top: -4, left: 'calc(40% + 6px)', background: 'var(--warn)', color: 'white', padding: '2px 6px', fontSize: 10, fontWeight: 600, borderRadius: 3 }}>11AM SPIKE</div>
              </div>
              <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
                <span>12am</span><span>6am</span><span>12pm</span><span>6pm</span><span>11pm</span>
              </div>
            </div>

            {/* traffic source breakdown */}
            <div className="hf-card" style={{ padding: 0 }}>
              <div className="hf-flex hf-between" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
                <div className="hf-h3">Traffic by source</div>
                <span className="hf-tiny hf-muted">Apr 1 · 486 visitors</span>
              </div>
              <table className="hf-table">
                <thead><tr><th>Source</th><th>Visitors</th><th>vs avg</th><th>Orders</th><th>Conv.</th></tr></thead>
                <tbody>
                  {[
                    { s: 'Design Sponge · feature article', v: 312, d: '+∞', o: 8, c: '2.6%', hot: true },
                    { s: 'Instagram',  v: 78,  d: '+12%', o: 3, c: '3.8%' },
                    { s: 'Direct',     v: 56,  d: '−4%',  o: 2, c: '3.6%' },
                    { s: 'Google',     v: 32,  d: '−8%',  o: 1, c: '3.1%' },
                    { s: 'Other',      v: 8,   d: '—',    o: 0, c: '0%' },
                  ].map((r, i) => (
                    <tr key={i} style={{ background: r.hot ? 'rgba(194,65,12,0.06)' : 'transparent' }}>
                      <td style={{ color: 'var(--ink)', fontWeight: r.hot ? 600 : 400 }}>
                        <span className="hf-flex hf-items-center hf-gap-2">
                          {r.hot && <span className="hf-dot hf-dot-warn" />}
                          {r.s}
                        </span>
                      </td>
                      <td className="hf-num" style={{ fontWeight: r.hot ? 600 : 400 }}>{r.v}</td>
                      <td><span className="hf-tiny" style={{ color: r.hot ? 'var(--good)' : 'var(--ink-3)', fontWeight: 600 }}>{r.d}</span></td>
                      <td className="hf-num">{r.o}</td>
                      <td className="hf-num">{r.c}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Right · narrative panel */}
          <div style={{ width: 320, borderLeft: '1px solid var(--line)', padding: 22, background: 'var(--paper-2)', flexShrink: 0, overflow: 'auto' }}>
            <div className="hf-eyebrow" style={{ marginBottom: 8 }}>What we think happened</div>
            <div className="hf-h2" style={{ fontSize: 18, marginBottom: 14, lineHeight: 1.3 }}>A blog feature drove most of Tuesday's lift.</div>
            <div className="hf-col hf-gap-3 hf-small">
              <div>312 of 486 visitors arrived from <span style={{ color: 'var(--ink)', fontWeight: 600 }}>designsponge.com</span> between 11am–1pm.</div>
              <div>That source has never appeared before — likely a new article or roundup mention.</div>
              <div>Conversion was lower than your usual social traffic (2.6% vs 3.8%) but the volume more than made up for it.</div>
            </div>
            <div className="hf-card" style={{ padding: 14, marginTop: 16, background: 'var(--paper)' }}>
              <div className="hf-tiny hf-muted">REFERRER URL</div>
              <div className="hf-mono hf-small" style={{ color: 'var(--ink)', marginTop: 4, wordBreak: 'break-all' }}>designsponge.com/2026/04/01/six-emerging-ceramicists</div>
              <button className="hf-btn hf-btn-outline hf-btn-sm" style={{ marginTop: 10, width: '100%' }}>Open article →</button>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>compares this week to last · expands view</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 03 · COMPARE — period-over-period side-by-side
// ──────────────────────────────────────────────────────────────────────
function AFlow_03_Compare() {
  const A = [220,260,240,300,340,320,380,400];
  const B = [380,420,1240,440,460,520,540,580];
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Compare periods"
          subtitle="This week vs last week"
          actions={<><_PeriodTabs active="7d" /><Avatar name="Mira" size="sm" /></>}
        />

        <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
          {/* date range pickers */}
          <div className="hf-flex hf-gap-3" style={{ marginBottom: 18 }}>
            <div className="hf-card hf-grow" style={{ padding: 14 }}>
              <div className="hf-tiny hf-muted">Period A</div>
              <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 4 }}>
                <span className="hf-h4">Mar 25 → Mar 31</span>
                <Ico n="chevD" s={11} />
              </div>
            </div>
            <div className="hf-card hf-grow" style={{ padding: 14, border: '1.5px solid var(--ink)' }}>
              <div className="hf-tiny" style={{ color: 'var(--ink)' }}>Period B</div>
              <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 4 }}>
                <span className="hf-h4">Apr 1 → Apr 7</span>
                <Ico n="chevD" s={11} />
              </div>
            </div>
          </div>

          {/* compare KPI rows */}
          <div className="hf-card" style={{ padding: 0, marginBottom: 18 }}>
            <table className="hf-table">
              <thead><tr><th>Metric</th><th>Period A</th><th>Period B</th><th>Δ</th><th>%</th></tr></thead>
              <tbody>
                {[
                  { l: 'Revenue',     a: '$1,840', b: '$3,240', d: '+$1,400', p: '+76%', t: 'good' },
                  { l: 'Orders',      a: '21',     b: '36',     d: '+15',     p: '+71%', t: 'good' },
                  { l: 'Conversion',  a: '3.1%',   b: '3.6%',   d: '+0.5pt',  p: '+16%', t: 'good' },
                  { l: 'AOV',         a: '$87.62', b: '$90.00', d: '+$2.38',  p: '+3%',  t: 'good' },
                  { l: 'Returns',     a: '0',      b: '1',      d: '+1',      p: '—',    t: 'soft' },
                ].map((r) => (
                  <tr key={r.l}>
                    <td style={{ color: 'var(--ink)', fontWeight: 500 }}>{r.l}</td>
                    <td className="hf-num">{r.a}</td>
                    <td className="hf-num" style={{ fontWeight: 600 }}>{r.b}</td>
                    <td className="hf-num">{r.d}</td>
                    <td><span className={`hf-chip hf-chip-${r.t}`} style={{ fontSize: 11 }}>{r.p}</span></td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* overlay chart */}
          <div className="hf-card" style={{ padding: 22 }}>
            <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 14 }}>
              <div className="hf-h3">Revenue · overlay</div>
              <div className="hf-flex hf-gap-3 hf-tiny">
                <span className="hf-flex hf-items-center hf-gap-1"><span style={{ width: 10, height: 2, background: 'var(--ink-4)' }} /> Period A</span>
                <span className="hf-flex hf-items-center hf-gap-1"><span style={{ width: 10, height: 2, background: 'var(--ink)' }} /> Period B</span>
              </div>
            </div>
            <div style={{ position: 'relative' }}>
              <div style={{ position: 'absolute', inset: 0, opacity: 0.4 }}><AreaChart data={A} color="var(--ink-4)" fill="rgba(21,18,14,0.04)" height={180} /></div>
              <AreaChart data={B} height={180} />
            </div>
            <div className="hf-flex hf-between hf-tiny hf-muted" style={{ marginTop: 8 }}>
              {['Mon','Tue','Wed','Thu','Fri','Sat','Sun'].map(d => <span key={d}>{d}</span>)}
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>"got it — what should I do about it?" → opens Insights</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 04 · INSIGHT — story-mode recommendation card with action
// ──────────────────────────────────────────────────────────────────────
function AFlow_04_Insight() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Insights"
          subtitle="3 recommendations for your shop"
          actions={<Avatar name="Mira" size="sm" />}
        />

        <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
          {/* Insight list */}
          <div style={{ width: 320, borderRight: '1px solid var(--line)', padding: 16, background: 'var(--paper-2)', flexShrink: 0, overflow: 'auto' }}>
            {[
              { e: 'Featured', t: 'Capture the Design Sponge halo', d: 'Apr 1', tone: 'good', active: true },
              { e: 'Inventory', t: 'Restock Persimmon vase, large', d: 'Apr 9', tone: 'warn' },
              { e: 'Pricing',   t: 'Cream carafe is underpriced',    d: 'Apr 11', tone: 'soft' },
            ].map((c, i) => (
              <div key={i} className="hf-card" style={{ padding: 14, marginBottom: 10, background: c.active ? 'var(--ink)' : 'var(--paper)', color: c.active ? 'var(--paper)' : 'var(--ink)', border: c.active ? 'none' : undefined, cursor: 'pointer' }}>
                <div className="hf-flex hf-between hf-items-start">
                  <div className="hf-eyebrow" style={{ color: c.active ? 'rgba(255,255,255,0.6)' : undefined }}>{c.e}</div>
                  <span className={`hf-dot hf-dot-${c.tone}`} />
                </div>
                <div className="hf-h4" style={{ marginTop: 6, color: c.active ? 'var(--paper)' : 'var(--ink)' }}>{c.t}</div>
                <div className="hf-tiny" style={{ color: c.active ? 'rgba(255,255,255,0.5)' : 'var(--ink-3)', marginTop: 4 }}>Updated {c.d}</div>
              </div>
            ))}
          </div>

          {/* Story-mode insight detail */}
          <div className="hf-grow" style={{ overflow: 'auto', padding: '40px 56px', background: 'var(--paper)' }}>
            <div style={{ maxWidth: 640 }}>
              <div className="hf-eyebrow" style={{ marginBottom: 10 }}>Featured · 3 min read</div>
              <h1 className="hf-display" style={{ fontSize: 38, letterSpacing: '-0.01em', lineHeight: 1.05, marginBottom: 16 }}>
                Tuesday's traffic was a Design Sponge feature.<br />
                <span style={{ color: 'var(--ink-3)' }}>Here's how to keep some of it.</span>
              </h1>
              <p className="hf-body" style={{ fontSize: 16, lineHeight: 1.55, color: 'var(--ink-2)', marginBottom: 20 }}>
                312 visitors landed on your shop from <span style={{ color: 'var(--ink)', fontWeight: 500 }}>designsponge.com</span> on April 1, driving $1,240 in same-day revenue. New traffic from a feature decays fast — usually 60–80% within the first week. A few moves while attention is high can compound into long-term followers.
              </p>

              {/* Recommendation cards */}
              <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: 'repeat(2, 1fr)' }}>
                {[
                  { i: 'mail', t: 'Email recent buyers', d: 'Thank Apr 1 customers · 8 people · suggest IG follow', primary: true, when: '~5 min' },
                  { i: 'tag',  t: 'Run a thank-you code', d: '15% off for new visitors · ends Apr 14', when: '~3 min' },
                  { i: 'star', t: 'Pin Persimmon vase', d: 'It was the article\'s hero shot · feature on home', when: '~1 min' },
                  { i: 'image', t: 'Add Design Sponge press badge', d: 'Builds trust on PDPs · 1 click', when: '~30 sec' },
                ].map((r, i) => (
                  <div key={i} className="hf-card" style={{ padding: 16 }}>
                    <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 8 }}>
                      <span className="hf-center" style={{ width: 28, height: 28, borderRadius: 6, background: r.primary ? 'var(--ink)' : 'var(--paper-2)', color: r.primary ? 'var(--paper)' : 'var(--ink)' }}><Ico n={r.i} s={14} /></span>
                      <div className="hf-h4">{r.t}</div>
                    </div>
                    <div className="hf-small hf-muted" style={{ marginBottom: 12 }}>{r.d}</div>
                    <div className="hf-flex hf-between hf-items-center">
                      <span className="hf-tiny hf-muted">{r.when}</span>
                      <button className={`hf-btn hf-btn-sm ${r.primary ? 'hf-btn-primary' : 'hf-btn-outline'}`}>Do it →</button>
                    </div>
                  </div>
                ))}
              </div>

              {/* Schedule report nudge */}
              <div className="hf-card" style={{ padding: 18, marginTop: 22, background: 'var(--paper-2)', border: 'none', display: 'flex', alignItems: 'center', gap: 14 }}>
                <span className="hf-center" style={{ width: 36, height: 36, borderRadius: 999, background: 'var(--ink)', color: 'var(--paper)', flexShrink: 0 }}><Ico n="bell" s={16} /></span>
                <div className="hf-grow">
                  <div className="hf-h4">Get a weekly digest like this</div>
                  <div className="hf-tiny hf-muted">3 insights every Monday · email + dashboard</div>
                </div>
                <button className="hf-btn hf-btn-outline">Set up →</button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <DesktopAction>schedules a weekly digest report</DesktopAction>
    </div>
  );
}

// ──────────────────────────────────────────────────────────────────────
// 05 · SCHEDULED — confirmation that the digest is set up; share with team
// ──────────────────────────────────────────────────────────────────────
function AFlow_05_Scheduled() {
  return (
    <div className="hf hf-desktop" style={{ flexDirection: 'row' }}>
      <SellerSidebar active="Analytics" />
      <div className="hf-grow hf-col" style={{ overflow: 'hidden' }}>
        <SellerTopbar
          title="Reports"
          subtitle="Scheduled · 1 active"
          actions={<button className="hf-btn hf-btn-primary"><Ico n="plus" s={12} /> New report</button>}
        />

        {/* Success banner */}
        <div style={{ background: '#DDEDE1', borderBottom: '1px solid var(--line)', padding: '14px 28px' }}>
          <div className="hf-flex hf-items-center hf-gap-3">
            <span className="hf-center" style={{ width: 26, height: 26, borderRadius: 999, background: 'var(--good)', color: 'white', flexShrink: 0 }}><Ico n="check" s={13} sw={2.4} /></span>
            <div className="hf-grow">
              <span className="hf-h4">Weekly digest scheduled — first email Monday, Apr 14 at 8am.</span>
            </div>
            <button className="hf-btn hf-btn-ghost hf-btn-sm">Send a test now</button>
          </div>
        </div>

        <div className="hf-grow hf-flex" style={{ overflow: 'hidden' }}>
          <div className="hf-grow" style={{ overflow: 'auto', padding: '24px 28px' }}>
            <div className="hf-card" style={{ padding: 0, marginBottom: 18 }}>
              <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
                <div className="hf-h3">Active schedules</div>
                <a className="hf-small" style={{ color: 'var(--ink-2)', fontWeight: 500 }}>History →</a>
              </div>
              <table className="hf-table">
                <thead><tr><th>Name</th><th>Cadence</th><th>Recipients</th><th>Next send</th><th></th></tr></thead>
                <tbody>
                  <tr style={{ background: 'rgba(27,94,63,0.04)' }}>
                    <td style={{ color: 'var(--ink)', fontWeight: 600 }}>
                      <span className="hf-flex hf-items-center hf-gap-2"><span className="hf-dot hf-dot-good" /> Weekly insights digest</span>
                    </td>
                    <td>Mondays · 8:00am</td>
                    <td>
                      <span className="hf-flex hf-items-center hf-gap-1">
                        <Avatar name="Mira" size="sm" />
                        <span className="hf-small hf-muted">mira@studio.example</span>
                      </span>
                    </td>
                    <td className="hf-num">Apr 14</td>
                    <td><button className="hf-btn hf-btn-ghost hf-btn-sm">Edit</button></td>
                  </tr>
                  <tr>
                    <td className="hf-muted">Monthly summary <span className="hf-chip" style={{ fontSize: 10, marginLeft: 6 }}>suggested</span></td>
                    <td className="hf-muted">1st of month · 9:00am</td>
                    <td className="hf-muted">—</td>
                    <td className="hf-muted">—</td>
                    <td><button className="hf-btn hf-btn-outline hf-btn-sm">Enable</button></td>
                  </tr>
                </tbody>
              </table>
            </div>

            {/* Email preview */}
            <div className="hf-card" style={{ padding: 0, overflow: 'hidden' }}>
              <div className="hf-flex hf-between hf-items-center" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
                <div className="hf-h3">Preview · what Monday's email looks like</div>
                <button className="hf-btn hf-btn-ghost hf-btn-sm">Edit template</button>
              </div>
              <div style={{ background: 'var(--paper-2)', padding: 24 }}>
                <div className="hf-card" style={{ maxWidth: 560, margin: '0 auto', padding: 28, boxShadow: 'var(--shadow-2)' }}>
                  <div className="hf-eyebrow" style={{ marginBottom: 6 }}>Mira Studio · Week of Apr 7–13</div>
                  <h2 className="hf-display" style={{ fontSize: 24, lineHeight: 1.1, marginBottom: 14 }}>You had your best week yet.</h2>
                  <div className="hf-flex hf-gap-3" style={{ marginBottom: 16 }}>
                    {[{l:'Revenue',v:'$3,240'},{l:'Orders',v:'36'},{l:'New buyers',v:'24'}].map(s => (
                      <div key={s.l} className="hf-grow">
                        <div className="hf-tiny hf-muted">{s.l}</div>
                        <div className="hf-display-2 hf-num" style={{ fontSize: 20, marginTop: 2 }}>{s.v}</div>
                      </div>
                    ))}
                  </div>
                  <div className="hf-h4" style={{ marginBottom: 8 }}>3 things to look at</div>
                  <ul className="hf-col hf-gap-2 hf-small" style={{ paddingLeft: 18, color: 'var(--ink-2)', margin: 0 }}>
                    <li>Persimmon vase · large is sold out — restock in time for next weekend</li>
                    <li>Cream carafe converts 2× the average — consider raising price</li>
                    <li>You haven't posted to IG since Mar 28 — your traffic dipped 14%</li>
                  </ul>
                  <button className="hf-btn hf-btn-primary" style={{ width: '100%', marginTop: 18 }}>Open dashboard →</button>
                </div>
              </div>
            </div>
          </div>

          {/* Right · share */}
          <div style={{ width: 300, borderLeft: '1px solid var(--line)', padding: 22, background: 'var(--paper)', flexShrink: 0 }}>
            <div className="hf-h4" style={{ marginBottom: 4 }}>Share with your team</div>
            <div className="hf-tiny hf-muted" style={{ marginBottom: 14 }}>Add up to 3 collaborators on the Free plan.</div>
            <div className="hf-card" style={{ padding: 12, marginBottom: 8, background: 'var(--paper-2)', border: 'none' }}>
              <div className="hf-flex hf-items-center hf-gap-2">
                <Avatar name="Mira" size="sm" />
                <div className="hf-grow">
                  <div className="hf-h4" style={{ fontSize: 12 }}>Mira (you)</div>
                  <div className="hf-tiny hf-muted">Owner</div>
                </div>
              </div>
            </div>
            <div className="hf-flex hf-gap-2" style={{ marginBottom: 14 }}>
              <input className="hf-input hf-grow" placeholder="teammate@example.com" />
              <button className="hf-btn hf-btn-outline">Invite</button>
            </div>
            <div className="hf-tiny hf-muted">They'll get the digest too, plus dashboard access.</div>
          </div>
        </div>
      </div>
      <DesktopAction>done · Mira closes the tab</DesktopAction>
    </div>
  );
}

Object.assign(window, { AFlow_01_Overview, AFlow_02_Drill, AFlow_03_Compare, AFlow_04_Insight, AFlow_05_Scheduled });

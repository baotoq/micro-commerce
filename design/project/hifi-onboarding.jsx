// hifi-onboarding.jsx — 3 mobile onboarding flows

const OB_PHONE = { width: 360, height: 720 };

// ── A · Linear stepper — three crisp steps with progress
function Onboarding_Stepper() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 14, paddingBottom: 22 }}>
          <span className="hf-tiny hf-num">Step 2 of 3</span>
          <button className="hf-icon-btn" style={{ width: 24, height: 24 }}><Ico n="x" s={14} /></button>
        </div>

        <div className="hf-px-5" style={{ marginBottom: 26 }}>
          <div className="hf-progress" style={{ height: 3 }}>
            <i style={{ width: '66%' }} />
          </div>
        </div>

        <div className="hf-px-5">
          <div className="hf-eyebrow" style={{ marginBottom: 10 }}>Tell us about your shop</div>
          <h1 className="hf-display" style={{ fontSize: 38, marginBottom: 10 }}>What will you<br /><i>make &amp; sell?</i></h1>
          <p className="hf-body hf-muted" style={{ marginTop: 8 }}>Pick what fits best. We'll set up the right templates, units &amp; tax.</p>
        </div>

        <div className="hf-col hf-gap-2 hf-px-5" style={{ marginTop: 22 }}>
          {[
            { ic: 'palette', l: 'Original art &amp; prints', s: '24 templates · pricing presets' },
            { ic: 'pkg', l: 'Handmade goods', s: 'shipping + materials log', on: true },
            { ic: 'tag', l: 'Vintage &amp; resale', s: 'condition + provenance' },
            { ic: 'cam', l: 'Digital products', s: 'auto-deliver downloads' },
            { ic: 'bolt', l: 'Services &amp; bookings', s: 'calendar + holds' },
          ].map((o, i) => (
            <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '12px 14px', border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', borderRadius: 12, background: o.on ? 'var(--paper)' : 'var(--card)' }}>
              <div className="hf-center" style={{ width: 34, height: 34, borderRadius: 10, background: o.on ? 'var(--ink)' : 'var(--paper-2)', color: o.on ? 'var(--paper)' : 'var(--ink-2)' }}>
                <Ico n={o.ic} s={16} />
              </div>
              <div className="hf-grow">
                <div className="hf-h4" dangerouslySetInnerHTML={{ __html: o.l }} />
                <div className="hf-tiny hf-muted">{o.s}</div>
              </div>
              {o.on && <div className="hf-center" style={{ width: 20, height: 20, borderRadius: 999, background: 'var(--ink)', color: 'var(--paper)' }}><Ico n="check" s={11} sw={2.4} /></div>}
            </div>
          ))}
        </div>
      </div>

      <div className="hf-px-5" style={{ paddingTop: 14, paddingBottom: 18, borderTop: '1px solid var(--line)' }}>
        <div className="hf-flex hf-gap-2">
          <button className="hf-btn hf-btn-outline" style={{ flex: '0 0 auto' }}><Ico n="arrowL" s={13} /></button>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg" style={{ flex: 1 }}>Continue<Ico n="arrowR" s={13} /></button>
        </div>
      </div>
      <PhoneHome />
      <Anno top={88} left={222}>1 of 3 steps · clear progress</Anno>
    </div>
  );
}

// ── B · Card stack — swipeable cards "What do you sell?"
function Onboarding_Cards() {
  return (
    <div className="hf hf-phone hf-stripes">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-px-5" style={{ paddingTop: 14, paddingBottom: 18, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span className="hf-logo">micro.</span>
          <button className="hf-btn hf-btn-ghost hf-btn-sm">Skip</button>
        </div>

        <div className="hf-px-5" style={{ marginBottom: 18 }}>
          <h1 className="hf-display" style={{ fontSize: 32, color: 'var(--ink)', lineHeight: 1.05 }}>Sell anything,<br /><i>beautifully.</i></h1>
          <p className="hf-body hf-muted" style={{ marginTop: 10 }}>Take a tour, then we'll tailor the setup.</p>
        </div>

        {/* Card stack */}
        <div className="hf-relative hf-grow" style={{ padding: '18px 28px 0' }}>
          {/* back card */}
          <div className="hf-card" style={{ position: 'absolute', left: 36, right: 36, top: 36, height: 360, transform: 'rotate(3deg)', boxShadow: 'var(--shadow-2)', overflow: 'hidden' }} />
          {/* mid card */}
          <div className="hf-card" style={{ position: 'absolute', left: 30, right: 30, top: 26, height: 360, transform: 'rotate(-2deg)', boxShadow: 'var(--shadow-2)', overflow: 'hidden' }} />
          {/* front card */}
          <div className="hf-card hf-overflow-hidden" style={{ position: 'absolute', left: 18, right: 18, top: 12, height: 360, boxShadow: 'var(--shadow-3)' }}>
            <div className="hf-img hf-img-clay" style={{ height: 200, borderRadius: 0 }} />
            <div className="hf-p-4">
              <div className="hf-eyebrow" style={{ color: 'var(--terra)' }}>Step 01</div>
              <h2 className="hf-h2" style={{ marginTop: 6 }}>Make a beautiful storefront</h2>
              <p className="hf-body hf-muted" style={{ marginTop: 6 }}>Pick a theme, drop in your photos. Goes live in minutes — no code, no fuss.</p>
              <div className="hf-flex hf-gap-1" style={{ marginTop: 14 }}>
                <span style={{ width: 18, height: 4, borderRadius: 2, background: 'var(--ink)' }} />
                <span style={{ width: 4, height: 4, borderRadius: 2, background: 'var(--ink-5)' }} />
                <span style={{ width: 4, height: 4, borderRadius: 2, background: 'var(--ink-5)' }} />
                <span style={{ width: 4, height: 4, borderRadius: 2, background: 'var(--ink-5)' }} />
              </div>
            </div>
          </div>
        </div>

        <div className="hf-col hf-gap-2 hf-px-5" style={{ paddingBottom: 14, marginTop: 'auto' }}>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">Start setup</button>
          <button className="hf-btn hf-btn-ghost hf-btn-block">I have a Micro account</button>
        </div>
      </div>
      <PhoneHome />
      <Anno top={290} left={28}>swipe — 4 cards</Anno>
    </div>
  );
}

// ── C · Goal-first / inline — conversational fill-the-blanks
function Onboarding_GoalFirst() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-px-5" style={{ paddingTop: 14, paddingBottom: 8, display: 'flex', alignItems: 'center', justifyContent: 'space-between' }}>
          <button className="hf-icon-btn" style={{ width: 28, height: 28 }}><Ico n="chevL" s={14} /></button>
          <div className="hf-flex hf-gap-1">
            <span style={{ width: 6, height: 6, borderRadius: 999, background: 'var(--ink)' }} />
            <span style={{ width: 6, height: 6, borderRadius: 999, background: 'var(--ink)' }} />
            <span style={{ width: 6, height: 6, borderRadius: 999, background: 'var(--ink-5)' }} />
          </div>
          <span style={{ width: 28 }} />
        </div>

        <div className="hf-px-5 hf-grow" style={{ marginTop: 32 }}>
          <div className="hf-display" style={{ fontSize: 30, lineHeight: 1.25, color: 'var(--ink)' }}>
            Hi <span style={{ background: 'var(--sun-2)', padding: '0 6px', borderRadius: 4 }}>Mira</span>, I run a
            <span style={{ display: 'inline-block', borderBottom: '1.5px dashed var(--ink-3)', padding: '0 8px', margin: '0 4px', color: 'var(--terra)' }}>ceramics studio</span>
            and want to sell to about
            <span style={{ display: 'inline-block', borderBottom: '1.5px dashed var(--ink-3)', padding: '0 8px', margin: '0 4px' }}>30</span>
            customers a month, mostly through my <span className="hf-display" style={{ color: 'var(--ink-4)' }}>Instagram</span>.
          </div>
        </div>

        {/* keyboard inline suggestions */}
        <div className="hf-px-4" style={{ paddingTop: 12, paddingBottom: 8 }}>
          <div className="hf-flex hf-gap-2 hf-no-scrollbar" style={{ overflowX: 'auto' }}>
            {['Instagram', 'TikTok', 'Etsy', 'Word of mouth', 'Markets'].map((s, i) => (
              <span key={i} className={`hf-chip ${i === 0 ? 'hf-chip-on' : ''}`} style={{ flexShrink: 0 }}>{s}</span>
            ))}
          </div>
        </div>
        <div className="hf-px-5" style={{ paddingBottom: 14 }}>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">Looks right →</button>
        </div>
        {/* Fake keyboard */}
        <div style={{ background: '#D1CFC8', padding: '6px 4px 6px', borderTop: '1px solid var(--line)' }}>
          <div className="hf-col hf-gap-1">
            {['QWERTYUIOP', 'ASDFGHJKL', 'ZXCVBNM'].map((row, i) => (
              <div key={i} className="hf-flex hf-gap-1" style={{ justifyContent: 'center', padding: i === 1 ? '0 14px' : 0 }}>
                {row.split('').map(k => (
                  <div key={k} style={{ flex: 1, height: 30, background: 'white', borderRadius: 4, fontSize: 14, display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'var(--ink)', fontWeight: 500, boxShadow: '0 1px 0 rgba(0,0,0,0.2)' }}>{k}</div>
                ))}
              </div>
            ))}
            <div className="hf-flex hf-gap-1" style={{ marginTop: 4 }}>
              <div style={{ width: 50, height: 30, background: '#A8A29E', borderRadius: 4, fontSize: 11, color: 'var(--ink)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>123</div>
              <div style={{ flex: 1, height: 30, background: 'white', borderRadius: 4, fontSize: 12, color: 'var(--ink-3)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>space</div>
              <div style={{ width: 64, height: 30, background: 'var(--ink)', color: 'white', borderRadius: 4, fontSize: 11, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>return</div>
            </div>
          </div>
        </div>
      </div>
      <PhoneHome />
      <Anno top={210} left={16}>fill-in-the-blanks</Anno>
    </div>
  );
}

Object.assign(window, { Onboarding_Stepper, Onboarding_Cards, Onboarding_GoalFirst });

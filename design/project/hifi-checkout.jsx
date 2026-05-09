// hifi-checkout.jsx — 3 mobile checkout layouts (360x720)

// ── A · Multi-step (step 2 of 3 · shipping)
function Checkout_Steps() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 14 }}>
          <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
          <span className="hf-h4">Checkout</span>
          <span style={{ width: 30 }} />
        </div>

        {/* Step rail */}
        <div className="hf-flex hf-items-center hf-px-5" style={{ paddingBottom: 18, gap: 8 }}>
          {[{l: 'Cart', on: 1}, {l: 'Ship', on: 2}, {l: 'Pay', on: 0}].map((s, i, a) => (
            <React.Fragment key={s.l}>
              <div className="hf-flex hf-items-center hf-gap-2">
                <div className="hf-center" style={{ width: 22, height: 22, borderRadius: 999,
                  background: s.on === 1 ? 'var(--paper-2)' : s.on === 2 ? 'var(--ink)' : 'transparent',
                  border: s.on === 0 ? '1px solid var(--line-2)' : 'none',
                  color: s.on === 2 ? 'var(--paper)' : s.on === 1 ? 'var(--ink-2)' : 'var(--ink-4)',
                  fontSize: 10.5, fontWeight: 600,
                }}>{s.on === 1 ? <Ico n="check" s={11} sw={2.5} /> : i + 1}</div>
                <span className="hf-tiny" style={{ fontWeight: 500, color: s.on === 0 ? 'var(--ink-4)' : 'var(--ink)' }}>{s.l}</span>
              </div>
              {i < a.length - 1 && <div className="hf-grow" style={{ height: 1, background: 'var(--line)' }} />}
            </React.Fragment>
          ))}
        </div>

        <div className="hf-grow hf-px-5" style={{ overflow: 'auto' }}>
          <div className="hf-eyebrow" style={{ marginBottom: 8 }}>Where to?</div>
          <h2 className="hf-display" style={{ fontSize: 28, lineHeight: 1, marginBottom: 16 }}>Shipping <i>address</i></h2>

          <div className="hf-col hf-gap-3">
            <div>
              <span className="hf-label">Email</span>
              <input className="hf-input" defaultValue="mira@studio.co" />
            </div>
            <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr' }}>
              <div><span className="hf-label">First name</span><input className="hf-input" defaultValue="Mira" /></div>
              <div><span className="hf-label">Last name</span><input className="hf-input" defaultValue="Castellanos" /></div>
            </div>
            <div>
              <span className="hf-label">Street address</span>
              <input className="hf-input" defaultValue="241 Telegraph Ave" />
            </div>
            <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr 0.9fr' }}>
              <div><span className="hf-label">City</span><input className="hf-input hf-input-sm" defaultValue="Oakland" /></div>
              <div><span className="hf-label">State</span><input className="hf-input hf-input-sm" defaultValue="CA" /></div>
              <div><span className="hf-label">ZIP</span><input className="hf-input hf-input-sm" defaultValue="94612" /></div>
            </div>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 4 }}>
              <div className="hf-switch hf-switch-on" />
              <span className="hf-small">Save for next time</span>
            </div>
          </div>
        </div>

        <div className="hf-px-5" style={{ paddingTop: 12, paddingBottom: 14, borderTop: '1px solid var(--line)', background: 'var(--paper)' }}>
          <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 8 }}>
            <span className="hf-small hf-muted">Total · 3 items</span>
            <span className="hf-display hf-num" style={{ fontSize: 22 }}>{money(190)}</span>
          </div>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">Continue to payment<Ico n="arrowR" s={13} /></button>
        </div>
      </div>
      <PhoneHome />
      <Anno top={70} left={130}>3 steps · clear breadcrumb</Anno>
    </div>
  );
}

// ── B · Single-page accordion
function Checkout_Accordion() {
  const Section = ({ n, label, value, open, done, children }) => (
    <div className="hf-card" style={{ padding: 0, borderColor: open ? 'var(--ink)' : 'var(--line)' }}>
      <div className="hf-flex hf-between hf-items-center" style={{ padding: '12px 14px' }}>
        <div className="hf-flex hf-items-center hf-gap-2">
          <div className="hf-center hf-num" style={{ width: 20, height: 20, borderRadius: 999, background: done ? 'var(--good)' : open ? 'var(--ink)' : 'var(--paper-2)', color: done || open ? 'var(--paper)' : 'var(--ink-2)', fontSize: 10, fontWeight: 700 }}>
            {done ? <Ico n="check" s={11} sw={2.5} /> : n}
          </div>
          <span className="hf-h4">{label}</span>
        </div>
        {value && <span className="hf-small hf-muted" style={{ maxWidth: 160, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{value}</span>}
        {!value && open && <Ico n="chevU" s={14} />}
      </div>
      {open && <div style={{ padding: '0 14px 14px' }}>{children}</div>}
    </div>
  );

  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 14 }}>
          <button className="hf-icon-btn"><Ico n="x" s={15} /></button>
          <span className="hf-h4">Checkout</span>
          <button className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"><Ico n="lock" s={11} /> Secure</button>
        </div>

        <div className="hf-grow hf-px-4 hf-col hf-gap-2" style={{ overflow: 'auto', paddingBottom: 12 }}>
          <Section n={1} label="Account" value="mira@studio.co" done />
          <Section n={2} label="Shipping" value="241 Telegraph Ave, Oakland CA 94612" done />
          <Section n={3} label="Delivery" open>
            <div className="hf-col hf-gap-2" style={{ marginTop: 4 }}>
              {[
                { l: 'Standard', s: '5–7 days', p: '$8' },
                { l: 'Express',  s: '2–3 days', p: '$22', on: true },
                { l: 'Local pick-up', s: 'Oakland · ready Fri', p: 'Free' },
              ].map((o, i) => (
                <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '10px 12px', border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', borderRadius: 10, background: o.on ? 'var(--paper-2)' : 'transparent' }}>
                  <div style={{ width: 16, height: 16, borderRadius: 999, border: '1.5px solid ' + (o.on ? 'var(--ink)' : 'var(--ink-4)'), display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
                    {o.on && <span style={{ width: 8, height: 8, borderRadius: 999, background: 'var(--ink)' }} />}
                  </div>
                  <div className="hf-grow">
                    <div className="hf-h4">{o.l}</div>
                    <div className="hf-tiny hf-muted">{o.s}</div>
                  </div>
                  <span className="hf-num hf-h4">{o.p}</span>
                </div>
              ))}
            </div>
          </Section>
          <Section n={4} label="Payment" />
          <Section n={5} label="Review &amp; place order" />
        </div>

        <div style={{ padding: '12px 20px 14px', borderTop: '1px solid var(--line)', background: 'var(--paper)' }}>
          <div className="hf-flex hf-between" style={{ marginBottom: 8 }}>
            <span className="hf-small hf-muted">Order total</span>
            <span className="hf-display hf-num" style={{ fontSize: 20 }}>{money(204)}</span>
          </div>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">Continue · Payment</button>
        </div>
      </div>
      <PhoneHome />
      <Anno top={250} left={16}>everything on one page</Anno>
    </div>
  );
}

// ── C · Express 3-tap — Apple Pay sheet style
function Checkout_Express() {
  return (
    <div className="hf hf-phone" style={{ background: 'rgba(0,0,0,0.5)' }}>
      <PhoneStatus />
      <div className="hf-phone-body" style={{ background: 'rgba(0,0,0,0.45)', position: 'relative' }}>
        {/* faded shop behind */}
        <div className="hf-img hf-img-clay" style={{ position: 'absolute', inset: 0, opacity: 0.35, borderRadius: 0 }} />

        {/* Bottom sheet */}
        <div style={{ marginTop: 'auto', background: 'var(--paper)', borderRadius: '20px 20px 0 0', boxShadow: '0 -8px 30px rgba(0,0,0,0.2)', padding: '8px 18px 0', position: 'relative', zIndex: 2 }}>
          <div style={{ width: 36, height: 4, borderRadius: 2, background: 'var(--ink-5)', margin: '0 auto 12px' }} />

          <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 12 }}>
            <span className="hf-h4">Pay Mira Studio</span>
            <button className="hf-icon-btn"><Ico n="x" s={14} /></button>
          </div>

          <div className="hf-flex hf-items-center hf-gap-3" style={{ padding: '10px 0', borderTop: '1px solid var(--line)', borderBottom: '1px solid var(--line)' }}>
            <div className="hf-img hf-img-clay" style={{ width: 44, height: 44, borderRadius: 8 }} />
            <div className="hf-grow">
              <div className="hf-h4">3 items · Persimmon vase + 2</div>
              <div className="hf-tiny hf-muted">Mira Studio · ships in 3–5 days</div>
            </div>
            <Ico n="chevR" s={14} />
          </div>

          {[
            { l: 'Pay', v: <span className="hf-num hf-display" style={{ fontSize: 18 }}>{money(204)}</span> },
            { l: 'Pay with', v: <span className="hf-flex hf-items-center hf-gap-1"><Ico n="apple" s={13} /> Apple Cash · 1234</span> },
            { l: 'Ship to', v: '241 Telegraph Ave' },
            { l: 'Contact', v: 'mira@studio.co' },
          ].map((r, i) => (
            <div key={i} className="hf-flex hf-between hf-items-center" style={{ padding: '12px 0', borderBottom: i < 3 ? '1px solid var(--line)' : 'none' }}>
              <span className="hf-tiny hf-muted" style={{ fontWeight: 500 }}>{r.l.toUpperCase()}</span>
              <span style={{ fontSize: 13, fontWeight: 500, color: 'var(--ink)' }}>{r.v}</span>
            </div>
          ))}

          <div style={{ padding: '14px 0 18px' }}>
            <button className="hf-btn hf-btn-primary hf-btn-block" style={{ height: 50, borderRadius: 12, fontSize: 14, fontWeight: 600 }}>
              <Ico n="apple" s={16} />
              <span>Pay {money(204)}</span>
            </button>
            <div className="hf-center" style={{ marginTop: 12 }}>
              <span className="hf-tiny hf-muted hf-flex hf-items-center hf-gap-1">Double-click to confirm <span style={{ width: 12, height: 12, borderRadius: 4, background: 'var(--ink-5)' }} /></span>
            </div>
          </div>
        </div>
      </div>
      <PhoneHome />
      <Anno top={250} right={16}>3-tap sheet</Anno>
    </div>
  );
}

// ── D · Desktop — 2-column: form + order summary
function Checkout_Desktop() {
  const items = [
    { tone: 'clay', t: 'Persimmon vase',  v: 'Medium · Persimmon', p: 86, q: 1 },
    { tone: 'sage', t: 'Forest bowl',     v: 'Large · Sage',       p: 64, q: 2 },
    { tone: 'rust', t: 'Rust mug Nº 04',  v: 'Set of 2',           p: 32, q: 1 },
  ];
  const sub = items.reduce((s, i) => s + i.p * i.q, 0);
  const ship = 8;
  const tax = Math.round(sub * 0.085);
  const total = sub + ship + tax - 25;

  const Field = ({ label, value, sm, type = 'text', placeholder }) => (
    <div>
      <span className="hf-label">{label}</span>
      <input className={sm ? 'hf-input hf-input-sm' : 'hf-input'} defaultValue={value} placeholder={placeholder} type={type} />
    </div>
  );

  return (
    <div className="hf hf-desktop" style={{ background: 'var(--paper-2)' }}>
      {/* Slim checkout topbar — no nav, just brand + secure badge */}
      <div className="hf-flex hf-items-center hf-between" style={{ padding: '14px 40px', background: 'var(--paper)', borderBottom: '1px solid var(--line)' }}>
        <div className="hf-flex hf-items-center hf-gap-3">
          <span className="hf-logo">Mira Studio</span>
          <span className="hf-tiny hf-muted">/ Checkout</span>
        </div>
        <div className="hf-flex hf-items-center hf-gap-4 hf-tiny hf-muted">
          <span className="hf-flex hf-items-center hf-gap-1"><Ico n="lock" s={11} /> Secure SSL</span>
          <span>Need help? <a style={{ color: 'var(--ink)', fontWeight: 500, textDecoration: 'underline' }}>chat</a></span>
        </div>
      </div>

      <div className="hf-grow" style={{ overflow: 'auto' }}>
        <div style={{ maxWidth: 1040, margin: '0 auto', padding: '24px 24px 40px', display: 'grid', gridTemplateColumns: '1fr 380px', gap: 36, alignItems: 'start' }}>
          {/* LEFT — form */}
          <div>
            {/* Step rail */}
            <div className="hf-flex hf-items-center" style={{ marginBottom: 22, gap: 10 }}>
              {[{l: 'Cart', on: 1}, {l: 'Information', on: 2}, {l: 'Shipping', on: 0}, {l: 'Payment', on: 0}].map((s, i, a) => (
                <React.Fragment key={s.l}>
                  <div className="hf-flex hf-items-center hf-gap-2">
                    <div className="hf-center" style={{ width: 22, height: 22, borderRadius: 999,
                      background: s.on === 1 ? 'var(--paper-2)' : s.on === 2 ? 'var(--ink)' : 'transparent',
                      border: s.on === 0 ? '1px solid var(--line-2)' : 'none',
                      color: s.on === 2 ? 'var(--paper)' : s.on === 1 ? 'var(--ink-2)' : 'var(--ink-4)',
                      fontSize: 10.5, fontWeight: 600,
                    }}>{s.on === 1 ? <Ico n="check" s={11} sw={2.5} /> : i + 1}</div>
                    <span className="hf-small" style={{ fontWeight: 500, color: s.on === 0 ? 'var(--ink-4)' : 'var(--ink)' }}>{s.l}</span>
                  </div>
                  {i < a.length - 1 && <div style={{ width: 28, height: 1, background: 'var(--line-2)' }} />}
                </React.Fragment>
              ))}
            </div>

            {/* Account */}
            <section style={{ marginBottom: 28 }}>
              <div className="hf-flex hf-between hf-items-baseline" style={{ marginBottom: 12 }}>
                <h3 className="hf-h3">Contact</h3>
                <span className="hf-tiny hf-muted">Have an account? <a style={{ color: 'var(--ink)', fontWeight: 500, textDecoration: 'underline' }}>Sign in</a></span>
              </div>
              <Field label="Email" value="mira@studio.co" type="email" />
              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 10 }}>
                <div className="hf-switch hf-switch-on" />
                <span className="hf-small">Email me with news and offers</span>
              </div>
            </section>

            {/* Shipping address */}
            <section style={{ marginBottom: 28 }}>
              <h3 className="hf-h3" style={{ marginBottom: 12 }}>Shipping address</h3>
              <div className="hf-col hf-gap-3">
                <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1fr 1fr' }}>
                  <Field label="First name" value="Mira" />
                  <Field label="Last name" value="Castellanos" />
                </div>
                <Field label="Street address" value="241 Telegraph Ave" />
                <Field label="Apt, suite (optional)" value="" placeholder="—" />
                <div className="hf-grid hf-gap-3" style={{ gridTemplateColumns: '1.4fr 1fr 1fr' }}>
                  <Field label="City" value="Oakland" sm />
                  <Field label="State" value="CA" sm />
                  <Field label="ZIP" value="94612" sm />
                </div>
                <Field label="Phone (for delivery)" value="(415) 555-0142" type="tel" />
              </div>
            </section>

            {/* Delivery */}
            <section style={{ marginBottom: 28 }}>
              <h3 className="hf-h3" style={{ marginBottom: 12 }}>Delivery method</h3>
              <div className="hf-col hf-gap-2">
                {[
                  { l: 'Standard',     s: '5–7 business days · USPS Ground',   p: '$8',  on: false },
                  { l: 'Express',      s: '2–3 business days · UPS Saver',     p: '$22', on: true },
                  { l: 'Local pickup', s: 'Oakland studio · ready Fri 4–7pm',  p: 'Free', on: false },
                ].map((o, i) => (
                  <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '12px 16px', border: o.on ? '1.5px solid var(--ink)' : '1px solid var(--line)', borderRadius: 10, background: o.on ? 'var(--paper-2)' : 'var(--paper)' }}>
                    <div style={{ width: 16, height: 16, borderRadius: 999, border: '1.5px solid ' + (o.on ? 'var(--ink)' : 'var(--ink-4)'), display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
                      {o.on && <span style={{ width: 8, height: 8, borderRadius: 999, background: 'var(--ink)' }} />}
                    </div>
                    <div className="hf-grow">
                      <div className="hf-h4">{o.l}</div>
                      <div className="hf-tiny hf-muted">{o.s}</div>
                    </div>
                    <span className="hf-num hf-h4">{o.p}</span>
                  </div>
                ))}
              </div>
            </section>

            {/* Payment */}
            <section style={{ marginBottom: 28 }}>
              <h3 className="hf-h3" style={{ marginBottom: 12 }}>Payment</h3>

              {/* Express options */}
              <div className="hf-flex hf-gap-2" style={{ marginBottom: 14 }}>
                <button className="hf-btn hf-btn-block" style={{ background: 'var(--ink)', color: 'var(--paper)' }}><Ico n="apple" s={14} /> Pay</button>
                <button className="hf-btn hf-btn-block" style={{ background: '#5A31F4', color: 'white' }}>Shop&nbsp;Pay</button>
                <button className="hf-btn hf-btn-block" style={{ background: '#FFC439', color: 'var(--ink)' }}>PayPal</button>
              </div>

              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginBottom: 14 }}>
                <div className="hf-grow" style={{ height: 1, background: 'var(--line)' }} />
                <span className="hf-tiny hf-muted">or pay with card</span>
                <div className="hf-grow" style={{ height: 1, background: 'var(--line)' }} />
              </div>

              <div className="hf-card" style={{ padding: 0, background: 'var(--paper)' }}>
                <div style={{ padding: '14px 16px', borderBottom: '1px solid var(--line)' }}>
                  <span className="hf-label">Card number</span>
                  <div className="hf-flex hf-items-center hf-gap-2">
                    <input className="hf-input" defaultValue="4242 4242 4242 4242" style={{ flex: 1 }} />
                    <div className="hf-flex hf-gap-1">
                      <span style={{ height: 18, padding: '0 6px', borderRadius: 4, background: '#1A1F71', color: 'white', fontSize: 9, fontWeight: 700, display: 'flex', alignItems: 'center' }}>VISA</span>
                      <span style={{ height: 18, width: 26, borderRadius: 4, background: 'linear-gradient(90deg,#EB001B 50%,#F79E1B 50%)' }} />
                    </div>
                  </div>
                </div>
                <div className="hf-grid" style={{ gridTemplateColumns: '1fr 1fr', borderBottom: '1px solid var(--line)' }}>
                  <div style={{ padding: '14px 16px', borderRight: '1px solid var(--line)' }}>
                    <span className="hf-label">Expiry</span>
                    <input className="hf-input hf-input-sm" defaultValue="08 / 28" />
                  </div>
                  <div style={{ padding: '14px 16px' }}>
                    <span className="hf-label">CVC</span>
                    <input className="hf-input hf-input-sm" defaultValue="•••" />
                  </div>
                </div>
                <div style={{ padding: '14px 16px' }}>
                  <span className="hf-label">Name on card</span>
                  <input className="hf-input hf-input-sm" defaultValue="Mira Castellanos" />
                </div>
              </div>

              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 12 }}>
                <div className="hf-switch hf-switch-on" />
                <span className="hf-small">Billing address same as shipping</span>
              </div>
            </section>

            <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 8 }}>
              <a className="hf-flex hf-items-center hf-gap-1 hf-small" style={{ color: 'var(--ink-2)' }}><Ico n="arrowL" s={12} /> Return to cart</a>
              <button className="hf-btn hf-btn-primary hf-btn-lg" style={{ minWidth: 220 }}>
                Place order · <span className="hf-num">{money(total)}</span>
              </button>
            </div>
          </div>

          {/* RIGHT — order summary */}
          <div style={{ position: 'sticky', top: 14 }}>
            <div className="hf-card" style={{ padding: 0, background: 'var(--paper)', overflow: 'hidden' }}>
              <div style={{ padding: '16px 20px', borderBottom: '1px solid var(--line)' }}>
                <div className="hf-flex hf-between hf-items-center">
                  <span className="hf-eyebrow">Order summary</span>
                  <span className="hf-tiny hf-muted">{items.reduce((s,i)=>s+i.q,0)} items</span>
                </div>
              </div>

              <div style={{ padding: '8px 20px' }}>
                {items.map((it, i) => (
                  <div key={i} className="hf-flex hf-items-center hf-gap-3" style={{ padding: '12px 0', borderBottom: i < items.length - 1 ? '1px solid var(--line)' : 'none' }}>
                    <div className="hf-relative" style={{ flexShrink: 0 }}>
                      <div className={`hf-img hf-img-${it.tone}`} style={{ width: 52, height: 52, borderRadius: 8 }} />
                      <span style={{ position: 'absolute', top: -6, right: -6, minWidth: 18, height: 18, padding: '0 5px', background: 'var(--ink)', color: 'var(--paper)', borderRadius: 999, fontSize: 10, fontWeight: 700, display: 'flex', alignItems: 'center', justifyContent: 'center', border: '2px solid var(--paper)' }}>{it.q}</span>
                    </div>
                    <div className="hf-grow" style={{ minWidth: 0 }}>
                      <div className="hf-h4" style={{ fontSize: 13 }}>{it.t}</div>
                      <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{it.v}</div>
                    </div>
                    <span className="hf-num hf-small" style={{ fontWeight: 600 }}>{money(it.p * it.q)}</span>
                  </div>
                ))}
              </div>

              <div style={{ padding: '14px 20px', borderTop: '1px solid var(--line)' }}>
                <div className="hf-flex hf-items-center hf-gap-2">
                  <Ico n="tag" s={13} />
                  <input className="hf-input" style={{ height: 32, padding: '0 8px', flex: 1 }} placeholder="Promo code" />
                  <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ border: '1px solid var(--line-2)' }}>Apply</button>
                </div>
              </div>

              <div style={{ padding: '14px 20px', background: 'var(--paper-2)' }}>
                <div className="hf-col hf-gap-2" style={{ fontSize: 13 }}>
                  <div className="hf-flex hf-between"><span className="hf-muted">Subtotal</span><span className="hf-num">{money(sub)}</span></div>
                  <div className="hf-flex hf-between"><span className="hf-muted">Shipping · Express</span><span className="hf-num">{money(22)}</span></div>
                  <div className="hf-flex hf-between"><span className="hf-muted">Estimated tax</span><span className="hf-num">{money(tax)}</span></div>
                  <div className="hf-flex hf-between" style={{ color: 'var(--good)' }}><span>Promo · WELCOME10</span><span className="hf-num">−$24.60</span></div>
                </div>

                <div className="hf-divider" style={{ margin: '12px 0' }} />

                <div className="hf-flex hf-between hf-items-baseline">
                  <span className="hf-h4">Total</span>
                  <div className="hf-flex hf-items-baseline hf-gap-1">
                    <span className="hf-tiny hf-muted">USD</span>
                    <span className="hf-display hf-num" style={{ fontSize: 26 }}>{money(sub + 22 + tax - 25)}</span>
                  </div>
                </div>
              </div>
            </div>

            {/* Trust card */}
            <div className="hf-card" style={{ padding: 14, marginTop: 12, background: 'var(--paper)' }}>
              <div className="hf-flex hf-items-start hf-gap-2">
                <Ico n="truck" s={14} />
                <div>
                  <div className="hf-h4" style={{ fontSize: 12.5 }}>Arrives Wed–Thu</div>
                  <div className="hf-tiny hf-muted" style={{ marginTop: 2 }}>Free 30-day returns. Carbon-neutral shipping.</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={62} left={40}>2-col · everything on one page</Anno>
      <Anno top={62} right={40}>order rail follows scroll</Anno>
    </div>
  );
}

Object.assign(window, { Checkout_Steps, Checkout_Accordion, Checkout_Express, Checkout_Desktop });

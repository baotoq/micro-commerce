// hifi-cart.jsx — 3 mobile cart layouts (360x720)

const cartItems = [
  { tone: 'clay', t: 'Persimmon vase', v: 'Medium · Persimmon', p: 86, q: 1 },
  { tone: 'sage', t: 'Forest bowl',     v: 'Large · Sage',       p: 64, q: 2 },
  { tone: 'rust', t: 'Rust mug Nº 04',  v: 'Set of 2',           p: 32, q: 1 },
];
const subtotal = cartItems.reduce((s, i) => s + i.p * i.q, 0);

// ── A · List — classic compact list, sticky checkout
function Cart_List() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-flex hf-between hf-items-center hf-px-5" style={{ paddingTop: 12, paddingBottom: 14 }}>
          <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
          <div className="hf-flex hf-col hf-items-center">
            <span className="hf-h3">Your bag</span>
            <span className="hf-tiny hf-muted">3 items · Mira Studio</span>
          </div>
          <button className="hf-icon-btn"><Ico n="search" s={14} /></button>
        </div>

        <div className="hf-grow" style={{ overflow: 'auto' }}>
          {cartItems.map((it, i) => (
            <div key={i} className="hf-flex hf-gap-3" style={{ padding: '14px 20px', borderBottom: '1px solid var(--line)' }}>
              <div className={`hf-img hf-img-${it.tone}`} style={{ width: 70, height: 70, borderRadius: 10, flexShrink: 0 }} />
              <div className="hf-grow hf-col">
                <div className="hf-flex hf-between">
                  <span className="hf-h4">{it.t}</span>
                  <span className="hf-num hf-h4">{money(it.p * it.q)}</span>
                </div>
                <span className="hf-tiny hf-muted" style={{ marginTop: 2 }}>{it.v}</span>
                <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 10 }}>
                  <div className="hf-flex hf-items-center hf-gap-1" style={{ height: 26, padding: '0 4px', border: '1px solid var(--line-2)', borderRadius: 999 }}>
                    <button className="hf-icon-btn" style={{ width: 22, height: 22 }}><Ico n="minus" s={11} /></button>
                    <span className="hf-num" style={{ width: 16, textAlign: 'center', fontSize: 12, fontWeight: 600 }}>{it.q}</span>
                    <button className="hf-icon-btn" style={{ width: 22, height: 22 }}><Ico n="plus" s={11} /></button>
                  </div>
                  <button className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"><Ico n="trash" s={11} /> Remove</button>
                </div>
              </div>
            </div>
          ))}
          {/* Promo */}
          <div className="hf-px-5" style={{ paddingTop: 14 }}>
            <div className="hf-flex hf-items-center hf-gap-2" style={{ padding: '10px 12px', border: '1px dashed var(--line-2)', borderRadius: 10 }}>
              <Ico n="tag" s={13} />
              <input className="hf-input" style={{ height: 26, border: 'none', padding: 0, background: 'transparent', flex: 1 }} placeholder="Promo code" />
              <button className="hf-btn hf-btn-ghost hf-btn-sm">Apply</button>
            </div>
          </div>
          {/* Free shipping */}
          <div className="hf-px-5" style={{ paddingTop: 14, paddingBottom: 14 }}>
            <div className="hf-flex hf-between hf-tiny" style={{ marginBottom: 6 }}>
              <span style={{ color: 'var(--terra)', fontWeight: 600 }}><Ico n="truck" s={11} /> $24 to free shipping</span>
              <span className="hf-num hf-muted">{money(subtotal)} / $200</span>
            </div>
            <div className="hf-progress"><i style={{ width: '88%', background: 'var(--terra)' }} /></div>
          </div>
        </div>

        {/* Sticky checkout bar */}
        <div style={{ borderTop: '1px solid var(--line)', padding: '14px 20px', background: 'var(--paper)' }}>
          <div className="hf-flex hf-between" style={{ marginBottom: 4 }}>
            <span className="hf-small hf-muted">Subtotal</span>
            <span className="hf-num hf-small">{money(subtotal)}</span>
          </div>
          <div className="hf-flex hf-between" style={{ marginBottom: 10 }}>
            <span className="hf-small hf-muted">Shipping</span>
            <span className="hf-num hf-small">$8.00</span>
          </div>
          <div className="hf-divider" style={{ marginBottom: 12 }} />
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">
            <span className="hf-flex hf-items-center hf-gap-2">Checkout · <span className="hf-num">{money(subtotal + 8)}</span></span>
            <Ico n="arrowR" s={13} />
          </button>
        </div>
      </div>
      <PhoneHome />
      <Anno top={460} left={16}>free-ship progress</Anno>
    </div>
  );
}

// ── B · Stacked cards — each item is its own card, swipe-to-delete hint
function Cart_Stacked() {
  return (
    <div className="hf hf-phone" style={{ background: 'var(--paper-2)' }}>
      <PhoneStatus />
      <div className="hf-phone-body">
        <div className="hf-px-5" style={{ paddingTop: 12, paddingBottom: 16 }}>
          <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 14 }}>
            <button className="hf-icon-btn"><Ico n="chevL" s={15} /></button>
            <span className="hf-tiny hf-muted">3 items</span>
          </div>
          <h1 className="hf-display" style={{ fontSize: 38, lineHeight: 1, marginTop: 4 }}>Your <i>bag</i>.</h1>
        </div>

        <div className="hf-grow hf-col hf-gap-3 hf-px-5" style={{ overflow: 'auto', paddingBottom: 12 }}>
          {cartItems.map((it, i) => (
            <div key={i} className="hf-card hf-relative" style={{ padding: 14, boxShadow: 'var(--shadow-1)' }}>
              {/* swipe hint */}
              {i === 0 && (
                <div style={{ position: 'absolute', right: -2, top: 0, bottom: 0, width: 44, background: 'var(--bad)', borderRadius: '0 var(--r-lg) var(--r-lg) 0', display: 'flex', alignItems: 'center', justifyContent: 'center', color: 'white', transform: 'translateX(8px)' }}>
                  <Ico n="trash" s={14} />
                </div>
              )}
              <div className="hf-flex hf-gap-3" style={{ transform: i === 0 ? 'translateX(-22px)' : 'none' }}>
                <div className={`hf-img hf-img-${it.tone}`} style={{ width: 84, height: 84, borderRadius: 10, flexShrink: 0 }} />
                <div className="hf-grow hf-col">
                  <span className="hf-h4">{it.t}</span>
                  <span className="hf-tiny hf-muted">{it.v}</span>
                  <div className="hf-flex hf-between hf-items-center" style={{ marginTop: 10 }}>
                    <span className="hf-display hf-num" style={{ fontSize: 18 }}>{money(it.p * it.q)}</span>
                    <div className="hf-flex hf-items-center hf-gap-2" style={{ height: 26 }}>
                      <button className="hf-icon-btn" style={{ width: 26, height: 26, background: 'var(--paper-2)' }}><Ico n="minus" s={11} /></button>
                      <span className="hf-num" style={{ fontSize: 13, fontWeight: 600, minWidth: 14, textAlign: 'center' }}>{it.q}</span>
                      <button className="hf-icon-btn" style={{ width: 26, height: 26, background: 'var(--paper-2)' }}><Ico n="plus" s={11} /></button>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          ))}

          {/* Recommended add */}
          <div className="hf-card" style={{ padding: 14, borderColor: 'transparent', background: 'var(--terra-2)' }}>
            <div className="hf-flex hf-gap-3 hf-items-center">
              <div className="hf-img hf-img-bone" style={{ width: 56, height: 56, borderRadius: 10 }} />
              <div className="hf-grow">
                <div className="hf-tiny" style={{ color: 'var(--terra)', fontWeight: 600 }}>OFTEN PAIRED</div>
                <div className="hf-h4" style={{ marginTop: 2 }}>Cream tumbler · 2 pk</div>
                <div className="hf-num hf-small" style={{ marginTop: 2 }}>{money(48)}</div>
              </div>
              <button className="hf-icon-btn" style={{ background: 'white', border: '1px solid var(--line-2)' }}><Ico n="plus" s={14} /></button>
            </div>
          </div>
        </div>

        <div style={{ padding: '12px 20px 16px', background: 'var(--paper)', borderTop: '1px solid var(--line)' }}>
          <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">
            <span>Checkout</span>
            <span className="hf-num" style={{ marginLeft: 'auto' }}>{money(subtotal + 8)}</span>
          </button>
        </div>
      </div>
      <PhoneHome />
      <Anno top={232} right={16}>swipe to delete</Anno>
    </div>
  );
}

// ── C · Total-first minimal — receipt-feel, big total
function Cart_Minimal() {
  return (
    <div className="hf hf-phone">
      <PhoneStatus />
      <div className="hf-phone-body hf-px-5" style={{ paddingTop: 12 }}>
        <div className="hf-flex hf-between hf-items-center" style={{ marginBottom: 24 }}>
          <button className="hf-icon-btn"><Ico n="x" s={14} /></button>
          <span className="hf-tiny hf-muted">Bag · 3</span>
          <button className="hf-btn hf-btn-ghost hf-btn-sm">Edit</button>
        </div>

        {/* Big total up top */}
        <div className="hf-col hf-items-center" style={{ marginBottom: 28, textAlign: 'center' }}>
          <span className="hf-eyebrow">Total today</span>
          <div className="hf-display hf-num" style={{ fontSize: 64, lineHeight: 1, marginTop: 8 }}>{money(subtotal + 8)}</div>
          <span className="hf-small hf-muted" style={{ marginTop: 4 }}>{money(subtotal)} + $8 shipping</span>
        </div>

        {/* Tiny item rail */}
        <div className="hf-flex hf-gap-2" style={{ marginBottom: 18 }}>
          {cartItems.map((it, i) => (
            <div key={i} className="hf-relative">
              <div className={`hf-img hf-img-${it.tone}`} style={{ width: 72, height: 72, borderRadius: 10 }} />
              {it.q > 1 && <div style={{ position: 'absolute', top: -4, right: -4, width: 18, height: 18, background: 'var(--ink)', color: 'var(--paper)', borderRadius: 999, fontSize: 10, fontWeight: 700, display: 'flex', alignItems: 'center', justifyContent: 'center', border: '2px solid var(--paper)' }}>×{it.q}</div>}
            </div>
          ))}
          <div className="hf-center" style={{ width: 72, height: 72, borderRadius: 10, border: '1px dashed var(--line-2)', color: 'var(--ink-3)' }}>
            <Ico n="plus" s={14} />
          </div>
        </div>

        {/* Receipt lines */}
        <div className="hf-card-flat" style={{ padding: 16, borderRadius: 14 }}>
          {cartItems.map((it, i) => (
            <div key={i} className="hf-flex hf-between" style={{ padding: '6px 0', fontSize: 12.5, color: 'var(--ink-2)' }}>
              <span>{it.t}{it.q > 1 ? ` · ×${it.q}` : ''}</span>
              <span className="hf-num">{money(it.p * it.q)}</span>
            </div>
          ))}
          <div style={{ borderTop: '1px dashed var(--line-2)', margin: '8px 0' }} />
          <div className="hf-flex hf-between" style={{ padding: '4px 0', fontSize: 12, color: 'var(--ink-3)' }}><span>Shipping</span><span className="hf-num">$8.00</span></div>
          <div className="hf-flex hf-between" style={{ padding: '4px 0', fontSize: 12, color: 'var(--ink-3)' }}><span>Tax</span><span className="hf-num">included</span></div>
        </div>

        <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 16, padding: '10px 12px', background: 'var(--forest-2)', borderRadius: 10, color: 'var(--good)', fontSize: 11.5 }}>
          <Ico n="check" s={13} sw={2.4} />
          <span>Free shipping unlocked</span>
        </div>
      </div>

      <div style={{ padding: '12px 20px 14px' }}>
        <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg">
          <Ico n="apple" s={14} />
          <span>Pay with Apple Pay</span>
        </button>
        <button className="hf-btn hf-btn-ghost hf-btn-block" style={{ marginTop: 6 }}>Other ways to pay</button>
      </div>
      <PhoneHome />
      <Anno top={142} right={16}>total-first</Anno>
    </div>
  );
}

// ── D · Desktop — 2-column: items + sticky order summary
function Cart_Desktop() {
  const desktopItems = [
    { tone: 'clay', t: 'Persimmon vase', v: 'Medium · Persimmon', sku: 'MS-PV-MD', p: 86, q: 1, ship: 'Ships in 3–5 days' },
    { tone: 'sage', t: 'Forest bowl',     v: 'Large · Sage',       sku: 'MS-FB-LG', p: 64, q: 2, ship: 'Ships in 3–5 days' },
    { tone: 'rust', t: 'Rust mug Nº 04',  v: 'Set of 2',           sku: 'MS-RM-04', p: 32, q: 1, ship: 'In stock · ships tomorrow' },
  ];
  const sub = desktopItems.reduce((s, i) => s + i.p * i.q, 0);
  const ship = 8;
  const tax = Math.round(sub * 0.085);

  return (
    <div className="hf hf-desktop">
      <ShopperTopbar cartCount={4} />
      <div className="hf-grow" style={{ overflow: 'auto', background: 'var(--paper-2)' }}>
        {/* breadcrumb */}
        <div className="hf-flex hf-items-center hf-gap-2" style={{ padding: '14px 40px', fontSize: 11.5, color: 'var(--ink-3)' }}>
          <a style={{ color: 'inherit' }}>Shop</a>
          <Ico n="chevR" s={10} />
          <span style={{ color: 'var(--ink)', fontWeight: 500 }}>Bag</span>
        </div>

        <div style={{ padding: '4px 40px 32px', display: 'grid', gridTemplateColumns: '1fr 360px', gap: 32, alignItems: 'start' }}>
          {/* LEFT — items */}
          <div>
            <div className="hf-flex hf-between hf-items-end" style={{ marginBottom: 18 }}>
              <h1 className="hf-display" style={{ fontSize: 40, lineHeight: 1 }}>Your <i>bag</i></h1>
              <span className="hf-small hf-muted">{desktopItems.length} items · saved at 9:41 am</span>
            </div>

            {/* free shipping bar */}
            <div className="hf-card" style={{ padding: '12px 16px', marginBottom: 14, background: 'var(--paper)' }}>
              <div className="hf-flex hf-between hf-items-center hf-tiny" style={{ marginBottom: 6 }}>
                <span style={{ color: 'var(--terra)', fontWeight: 600 }}><Ico n="truck" s={11} /> $24 to free shipping</span>
                <span className="hf-num hf-muted">{money(sub)} / $200</span>
              </div>
              <div className="hf-progress"><i style={{ width: '88%', background: 'var(--terra)' }} /></div>
            </div>

            {/* table-like items */}
            <div className="hf-card" style={{ padding: 0, background: 'var(--paper)' }}>
              <div className="hf-flex" style={{ padding: '12px 20px', borderBottom: '1px solid var(--line)', fontSize: 10.5, color: 'var(--ink-3)', fontWeight: 600, letterSpacing: '0.06em', textTransform: 'uppercase' }}>
                <span style={{ flex: 1 }}>Item</span>
                <span style={{ width: 110, textAlign: 'center' }}>Quantity</span>
                <span style={{ width: 90, textAlign: 'right' }}>Price</span>
              </div>
              {desktopItems.map((it, i) => (
                <div key={i} className="hf-flex hf-items-center" style={{ padding: '18px 20px', borderBottom: i < desktopItems.length - 1 ? '1px solid var(--line)' : 'none', gap: 16 }}>
                  <div className={`hf-img hf-img-${it.tone}`} style={{ width: 84, height: 84, borderRadius: 10, flexShrink: 0 }} />
                  <div style={{ flex: 1, minWidth: 0 }}>
                    <div className="hf-h4">{it.t}</div>
                    <div className="hf-tiny hf-muted" style={{ marginTop: 3 }}>{it.v} · SKU {it.sku}</div>
                    <div className="hf-flex hf-items-center hf-gap-3" style={{ marginTop: 10 }}>
                      <span className="hf-tiny" style={{ color: 'var(--good)', fontWeight: 600 }}><Ico n="check" s={11} sw={2.4} /> {it.ship}</span>
                      <button className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"><Ico n="heart" s={11} /> Save for later</button>
                      <button className="hf-flex hf-items-center hf-gap-1 hf-tiny hf-muted"><Ico n="trash" s={11} /> Remove</button>
                    </div>
                  </div>
                  <div className="hf-flex hf-items-center hf-gap-1" style={{ width: 110, justifyContent: 'center', height: 30, padding: '0 6px', border: '1px solid var(--line-2)', borderRadius: 999 }}>
                    <button className="hf-icon-btn" style={{ width: 22, height: 22 }}><Ico n="minus" s={11} /></button>
                    <span className="hf-num" style={{ width: 22, textAlign: 'center', fontSize: 13, fontWeight: 600 }}>{it.q}</span>
                    <button className="hf-icon-btn" style={{ width: 22, height: 22 }}><Ico n="plus" s={11} /></button>
                  </div>
                  <div style={{ width: 90, textAlign: 'right' }}>
                    <div className="hf-num hf-h4">{money(it.p * it.q)}</div>
                    {it.q > 1 && <div className="hf-tiny hf-muted hf-num" style={{ marginTop: 2 }}>{money(it.p)} ea</div>}
                  </div>
                </div>
              ))}
            </div>

            {/* Often paired */}
            <div style={{ marginTop: 22 }}>
              <h3 className="hf-h3" style={{ marginBottom: 12 }}>Often paired with these</h3>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: 14 }}>
                {[
                  { tone: 'bone',   t: 'Cream tumbler · 2 pk', p: 48 },
                  { tone: 'cobalt', t: 'Indigo carafe',         p: 110 },
                  { tone: 'cream',  t: 'Bone dinner plate',     p: 38 },
                ].map((p, i) => (
                  <div key={i} className="hf-card" style={{ padding: 12, background: 'var(--paper)' }}>
                    <div className="hf-flex hf-gap-3 hf-items-center">
                      <div className={`hf-img hf-img-${p.tone}`} style={{ width: 56, height: 56, borderRadius: 8, flexShrink: 0 }} />
                      <div className="hf-grow" style={{ minWidth: 0 }}>
                        <div className="hf-h4" style={{ fontSize: 13 }}>{p.t}</div>
                        <div className="hf-num hf-tiny" style={{ marginTop: 2, fontWeight: 600 }}>{money(p.p)}</div>
                      </div>
                      <button className="hf-btn hf-btn-ghost hf-btn-sm" style={{ border: '1px solid var(--line-2)' }}>Add</button>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* RIGHT — order summary (sticky feel) */}
          <div style={{ position: 'sticky', top: 14 }}>
            <div className="hf-card" style={{ padding: 22, background: 'var(--paper)' }}>
              <div className="hf-eyebrow" style={{ marginBottom: 10 }}>Order summary</div>

              <div className="hf-col hf-gap-2" style={{ fontSize: 13 }}>
                <div className="hf-flex hf-between"><span className="hf-muted">Subtotal · {desktopItems.length} items</span><span className="hf-num">{money(sub)}</span></div>
                <div className="hf-flex hf-between"><span className="hf-muted">Shipping · Standard</span><span className="hf-num">{money(ship)}</span></div>
                <div className="hf-flex hf-between"><span className="hf-muted">Estimated tax</span><span className="hf-num">{money(tax)}</span></div>
                <div className="hf-flex hf-between" style={{ color: 'var(--good)' }}><span><Ico n="tag" s={11} /> Promo · WELCOME10</span><span className="hf-num">−$24.60</span></div>
              </div>

              <div className="hf-divider" style={{ margin: '14px 0' }} />

              <div className="hf-flex hf-between hf-items-baseline">
                <span className="hf-h4">Total</span>
                <span className="hf-display hf-num" style={{ fontSize: 26 }}>{money(sub + ship + tax - 25)}</span>
              </div>
              <div className="hf-tiny hf-muted" style={{ marginTop: 4 }}>USD · taxes calculated at checkout</div>

              <button className="hf-btn hf-btn-primary hf-btn-block hf-btn-lg" style={{ marginTop: 16 }}>
                Checkout <Ico n="arrowR" s={13} />
              </button>
              <div className="hf-flex hf-items-center hf-gap-2" style={{ margin: '10px 0' }}>
                <div className="hf-grow" style={{ height: 1, background: 'var(--line)' }} />
                <span className="hf-tiny hf-muted">or pay express</span>
                <div className="hf-grow" style={{ height: 1, background: 'var(--line)' }} />
              </div>
              <div className="hf-flex hf-gap-2">
                <button className="hf-btn hf-btn-block" style={{ background: 'var(--ink)', color: 'var(--paper)' }}><Ico n="apple" s={14} /> Pay</button>
                <button className="hf-btn hf-btn-block" style={{ background: '#5A31F4', color: 'white' }}>Shop&nbsp;Pay</button>
              </div>

              <div className="hf-flex hf-items-center hf-gap-2" style={{ marginTop: 14, padding: '10px 12px', background: 'var(--paper-2)', borderRadius: 8 }}>
                <Ico n="lock" s={12} />
                <span className="hf-tiny hf-muted">Secure checkout · 30-day returns</span>
              </div>
            </div>

            {/* Promo input */}
            <div className="hf-card" style={{ padding: 14, marginTop: 12, background: 'var(--paper)' }}>
              <div className="hf-flex hf-items-center hf-gap-2">
                <Ico n="tag" s={13} />
                <input className="hf-input" style={{ height: 32, padding: '0 8px', flex: 1 }} placeholder="Add another promo code" />
                <button className="hf-btn hf-btn-ghost hf-btn-sm">Apply</button>
              </div>
            </div>
          </div>
        </div>
      </div>
      <Anno top={62} left={40}>2-col · sticky summary</Anno>
      <Anno top={300} right={40}>express pay rail</Anno>
    </div>
  );
}

// Tiny lock icon — re-use Ico but lock isn't in the set, alias
const _ensureLock = (() => {
  // no-op: lock glyph approximated via "pin" or via inline svg below where used
  return null;
})();

Object.assign(window, { Cart_List, Cart_Stacked, Cart_Minimal, Cart_Desktop });

// Storefront wireframes — onboarding, home, product, cart, checkout
// Three variations per screen.

// ─── ONBOARDING (mobile, 3 variants) ─────────────────────────────────────────

const Onboarding_Stepper = () => (
  <Phone statusTitle="">
    <div className="wf-col wf-p-4" style={{ height: '100%', gap: 16 }}>
      <div className="wf-between" style={{ marginTop: 6 }}>
        <span className="wf-eyebrow">Step 2 of 4</span>
        <span className="wf-tiny wf-muted">Skip</span>
      </div>
      <div className="wf-row wf-gap-2">
        {[1, 1, 0, 0].map((on, i) => (
          <div key={i} style={{
            flex: 1, height: 4, borderRadius: 2,
            background: on ? 'var(--ink)' : 'var(--ink-faint)',
          }} />
        ))}
      </div>
      <div style={{ marginTop: 8 }}>
        <Heading size="h1">What do you sell?</Heading>
        <div className="wf-muted wf-small wf-mt-2">Pick one. You can add more later.</div>
      </div>
      <div className="wf-col wf-gap-3" style={{ marginTop: 8 }}>
        {['Apparel', 'Home goods', 'Art & prints', 'Food & drink', 'Something else'].map((label, i) => (
          <div key={label} className={`wf-box ${i === 1 ? 'wf-accent-border' : ''}`}
            style={{ padding: '14px 16px', display: 'flex', justifyContent: 'space-between', alignItems: 'center', borderWidth: i === 1 ? 2 : 1.4 }}>
            <span style={{ fontSize: 16 }}>{label}</span>
            {i === 1 && <span className="wf-accent-text"><Icon name="check" /></span>}
          </div>
        ))}
      </div>
      <div style={{ flex: 1 }} />
      <Btn accent lg block>Continue <span style={{ marginLeft: 6 }}><Icon name="arrow_r" size={16} /></span></Btn>
    </div>
    <Anno x={26} y={70} dir="down">Linear progress · classic stepper</Anno>
  </Phone>
);

const Onboarding_Cards = () => (
  <Phone statusTitle="">
    <div className="wf-col" style={{ height: '100%', position: 'relative', padding: 16 }}>
      <div className="wf-between" style={{ marginBottom: 12 }}>
        <Icon name="x" size={18} />
        <span className="wf-tiny wf-muted">2 / 5</span>
      </div>
      <div style={{ position: 'relative', flex: 1 }}>
        {/* back card */}
        <div className="wf-box" style={{ position: 'absolute', left: 14, right: 14, top: 16, bottom: 60, background: 'var(--paper-2)', transform: 'rotate(-2deg)' }} />
        {/* mid card */}
        <div className="wf-box" style={{ position: 'absolute', left: 8, right: 8, top: 8, bottom: 50, background: 'var(--paper)', transform: 'rotate(1.2deg)' }} />
        {/* front card */}
        <div className="wf-box" style={{ position: 'absolute', inset: 0, bottom: 40, padding: 18, display: 'flex', flexDirection: 'column', gap: 12, background: 'var(--paper)' }}>
          <div style={{ borderRadius: 999, width: 56, height: 56, background: 'var(--paper-2)', border: '1.4px solid var(--ink-soft)', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
            <Icon name="box" size={28} />
          </div>
          <Heading size="h2">Your first product</Heading>
          <div className="wf-small wf-muted">Snap a photo, add a name and price. We'll handle the rest.</div>
          <div className="wf-mt-3" style={{ display: 'flex', flexDirection: 'column', gap: 8 }}>
            <div className="wf-between wf-tiny"><span>Photo</span><Icon name="upload" size={14} /></div>
            <ImgBox h={120} label="Tap to add" />
            <div className="wf-line thin" style={{ width: '60%' }} />
            <div className="wf-line thin" style={{ width: '90%' }} />
          </div>
        </div>
      </div>
      <div className="wf-row wf-gap-2" style={{ marginTop: 12 }}>
        <Btn ghost style={{ flex: 1 }}>Back</Btn>
        <Btn accent style={{ flex: 2 }}>Add product</Btn>
      </div>
    </div>
    <Anno x={20} y={52} dir="down">Stack · swipe-able cards</Anno>
  </Phone>
);

const Onboarding_GoalFirst = () => (
  <Phone statusTitle="">
    <div className="wf-col wf-p-4" style={{ height: '100%' }}>
      <div className="wf-between" style={{ marginBottom: 16 }}>
        <span className="wf-display" style={{ fontWeight: 700, fontSize: 17 }}>micro</span>
        <span className="wf-tiny wf-muted">Sign in</span>
      </div>
      <div style={{ marginTop: 12 }}>
        <Heading size="h1">A store,<br/>by Friday.</Heading>
        <div className="wf-muted wf-mt-2" style={{ fontSize: 14 }}>
          Tell us what you make. We'll set up everything else.
        </div>
      </div>
      {/* the entire setup happens inline */}
      <div className="wf-col wf-gap-4 wf-mt-6">
        <div>
          <span className="wf-eyebrow">I sell</span>
          <div className="wf-box wf-mt-2 wf-p-3" style={{ padding: '12px 14px', borderWidth: 1.6 }}>
            <span className="wf-small">handmade leather wallets &nbsp;·&nbsp; <span className="wf-muted">edit</span></span>
          </div>
        </div>
        <div>
          <span className="wf-eyebrow">My shop is called</span>
          <div className="wf-box wf-mt-2 wf-p-3 wf-accent-border" style={{ padding: '12px 14px', borderWidth: 2 }}>
            <span style={{ fontSize: 16 }}>North Hide Co.</span>
            <span className="wf-tiny wf-muted" style={{ marginLeft: 8 }}>↗ northhide.micro.shop</span>
          </div>
        </div>
        <div>
          <span className="wf-eyebrow">First product</span>
          <div className="wf-row wf-gap-2 wf-mt-2">
            <ImgBox w={64} h={64} />
            <div className="wf-col wf-grow wf-gap-2" style={{ justifyContent: 'center' }}>
              <div className="wf-line thin" style={{ width: '70%' }} />
              <div className="wf-line thin" style={{ width: '40%' }} />
            </div>
          </div>
        </div>
      </div>
      <div style={{ flex: 1 }} />
      <Btn accent lg block>Open my store</Btn>
      <div className="wf-tiny wf-muted" style={{ textAlign: 'center', marginTop: 10 }}>3 minutes · no card needed</div>
    </div>
    <Anno x={120} y={300} dir="right">Single screen · everything inline</Anno>
  </Phone>
);

// ─── STOREFRONT HOME (desktop, 3 variants) ───────────────────────────────────

const Home_Tiles = () => (
  <Desktop url="northhide.micro.shop">
    <div style={{ height: '100%', overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
      {/* nav */}
      <div className="wf-between" style={{ padding: '14px 32px', borderBottom: '1px solid var(--ink-faint)' }}>
        <span className="wf-display" style={{ fontWeight: 700, fontSize: 18 }}>North Hide Co.</span>
        <div className="wf-row wf-gap-6" style={{ fontSize: 14 }}>
          <span>Shop</span><span>Story</span><span>Journal</span><span>Contact</span>
        </div>
        <div className="wf-row wf-gap-3"><Icon name="search" /><Icon name="bag" /></div>
      </div>
      {/* tile 1 — light hero */}
      <div style={{ padding: '40px 60px 30px', textAlign: 'center', background: 'var(--paper)' }}>
        <Heading size="h1" style={{ fontSize: 38, marginBottom: 6 }}>Cardholder №3.</Heading>
        <div className="wf-muted" style={{ fontSize: 16 }}>Cut from a single piece of bridle leather.</div>
        <div className="wf-row wf-gap-3" style={{ justifyContent: 'center', marginTop: 14, fontSize: 13 }}>
          <span className="wf-accent-text wf-sk-underline">Learn more</span>
          <span className="wf-accent-text wf-sk-underline">Buy</span>
        </div>
        <ImgBox h={150} style={{ marginTop: 18, maxWidth: 480, marginLeft: 'auto', marginRight: 'auto' }} label="full-bleed product render" />
      </div>
      {/* tile 2 — dark */}
      <div className="wf-img-dark" style={{ padding: '32px 60px', display: 'flex', alignItems: 'center', gap: 32, color: '#eee' }}>
        <div style={{ flex: 1 }}>
          <Heading size="h2" style={{ color: '#eee', fontSize: 28 }}>The Long Wallet.</Heading>
          <div style={{ color: '#bbb', fontSize: 14, marginTop: 4 }}>Eight cards. Bills flat. No fold.</div>
          <div className="wf-row wf-gap-3 wf-mt-2" style={{ fontSize: 13, color: '#9bcfff' }}>
            <span className="wf-sk-underline" style={{ borderColor: '#9bcfff' }}>Learn more</span>
            <span className="wf-sk-underline" style={{ borderColor: '#9bcfff' }}>Buy</span>
          </div>
        </div>
        <ImgBox w={260} h={130} dark />
      </div>
      {/* tile row — 2 light */}
      <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', background: 'var(--paper-2)' }}>
        <div style={{ padding: 24 }}>
          <Heading size="h3">Belts</Heading>
          <div className="wf-muted wf-tiny">From $48</div>
          <ImgBox h={110} style={{ marginTop: 12 }} />
        </div>
        <div style={{ padding: 24, borderLeft: '1px solid var(--ink-faint)' }}>
          <Heading size="h3">Keychains</Heading>
          <div className="wf-muted wf-tiny">From $19</div>
          <ImgBox h={110} style={{ marginTop: 12 }} />
        </div>
      </div>
      <div className="wf-fade-bottom" />
    </div>
    <Anno x={400} y={120} dir="down">Edge-to-edge tiles · 0 gap · alt light/dark</Anno>
    <Anno x={26} y={200} dir="right">No drop shadow on cards — only on product imagery</Anno>
  </Desktop>
);

const Home_Editorial = () => (
  <Desktop url="northhide.micro.shop">
    <div style={{ height: '100%', overflow: 'hidden' }}>
      <div className="wf-between" style={{ padding: '14px 32px' }}>
        <Icon name="menu" />
        <span className="wf-display" style={{ fontWeight: 700, fontSize: 20 }}>NORTH HIDE</span>
        <div className="wf-row wf-gap-3"><Icon name="search" /><Icon name="bag" /></div>
      </div>
      {/* asymmetric mosaic */}
      <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr', gridTemplateRows: '180px 130px 130px', gap: 4, padding: '0 16px' }}>
        <div className="wf-img" style={{ gridRow: '1 / span 2', position: 'relative' }}>
          <div style={{ position: 'absolute', left: 16, bottom: 16, zIndex: 3 }}>
            <span className="wf-eyebrow" style={{ color: 'var(--paper)' }}>Issue 04</span>
            <Heading size="h1" style={{ fontSize: 30, color: 'var(--paper)' }}>Made by hand.</Heading>
          </div>
        </div>
        <div className="wf-img wf-img-dark" />
        <div className="wf-img" />
        <div className="wf-img" />
        <div className="wf-img" />
        <div className="wf-img" style={{ gridColumn: '1 / span 2' }} />
        <div className="wf-img wf-img-dark" />
      </div>
      <div style={{ padding: '20px 32px' }}>
        <Heading size="h3">Read</Heading>
        <div className="wf-row wf-gap-4 wf-mt-2">
          {['On bridle leather', 'A wallet, by Friday', 'The case for cards'].map((t) => (
            <div key={t} style={{ flex: 1 }}>
              <ImgBox h={70} />
              <div className="wf-eyebrow wf-mt-2">Journal · 5 min</div>
              <div className="wf-h3" style={{ fontSize: 15, marginTop: 4 }}>{t}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
    <Anno x={420} y={50} dir="down">Magazine · asymmetric grid · editorial</Anno>
  </Desktop>
);

const Home_Rails = () => (
  <Desktop url="northhide.micro.shop">
    <div style={{ height: '100%', overflow: 'hidden', display: 'flex', flexDirection: 'column' }}>
      <div className="wf-between" style={{ padding: '12px 32px', borderBottom: '1px solid var(--ink-faint)' }}>
        <span className="wf-display" style={{ fontWeight: 700, fontSize: 18 }}>North Hide Co.</span>
        <div className="wf-row wf-gap-4 wf-tiny"><span>Wallets</span><span>Belts</span><span>Bags</span><span>Sale</span></div>
        <div className="wf-row wf-gap-3"><Icon name="search" /><Icon name="user" /><Icon name="bag" /></div>
      </div>
      {/* big single hero */}
      <div style={{ padding: 0, position: 'relative' }}>
        <ImgBox h={260} plain style={{ borderRadius: 0, borderLeft: 0, borderRight: 0 }}>
          <div style={{ position: 'absolute', inset: 0, display: 'flex', flexDirection: 'column', justifyContent: 'flex-end', padding: 32, zIndex: 3 }}>
            <Heading size="h1" style={{ fontSize: 36 }}>The Cardholder.</Heading>
            <div className="wf-muted" style={{ marginTop: 4 }}>Three pockets. One piece of leather.</div>
            <div className="wf-row wf-gap-3 wf-mt-3"><Btn fill>Buy · $58</Btn><Btn ghost>Learn more</Btn></div>
          </div>
        </ImgBox>
      </div>
      {/* horizontal product rail */}
      <div style={{ padding: '20px 32px 0' }}>
        <div className="wf-between">
          <Heading size="h3">Best sellers</Heading>
          <span className="wf-tiny wf-accent-text">See all →</span>
        </div>
        <div className="wf-row wf-gap-3 wf-mt-3" style={{ overflow: 'hidden' }}>
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} style={{ width: 150, flexShrink: 0 }}>
              <ImgBox h={130} />
              <div className="wf-tiny wf-mt-2">Product {i}</div>
              <div className="wf-tiny wf-muted">$58</div>
            </div>
          ))}
        </div>
      </div>
      <div style={{ padding: '16px 32px' }}>
        <Heading size="h3">New this week</Heading>
        <div className="wf-row wf-gap-3 wf-mt-2" style={{ overflow: 'hidden' }}>
          {[1, 2, 3, 4, 5].map((i) => (
            <div key={i} style={{ width: 130, flexShrink: 0 }}>
              <ImgBox h={100} />
              <div className="wf-tiny wf-mt-2">New {i}</div>
            </div>
          ))}
        </div>
      </div>
    </div>
    <Anno x={300} y={50} dir="down">Hero + horizontal rails · familiar</Anno>
  </Desktop>
);

// ─── PRODUCT DETAIL (desktop, 3 variants) ────────────────────────────────────

const Product_Classic = () => (
  <Desktop url="northhide.micro.shop/cardholder-3">
    <div style={{ height: '100%', overflow: 'hidden' }}>
      <div className="wf-between" style={{ padding: '12px 32px', borderBottom: '1px solid var(--ink-faint)' }}>
        <span className="wf-display" style={{ fontWeight: 700 }}>North Hide</span>
        <div className="wf-row wf-gap-3"><Icon name="search" /><Icon name="bag" /></div>
      </div>
      <div className="wf-tiny wf-muted" style={{ padding: '10px 32px' }}>Shop / Wallets / <span className="wf-ink">Cardholder №3</span></div>
      <div style={{ display: 'grid', gridTemplateColumns: '60px 1fr 360px', gap: 16, padding: '0 32px 16px' }}>
        {/* thumbs */}
        <div className="wf-col wf-gap-2">
          {[1, 2, 3, 4].map((i) => (
            <ImgBox key={i} h={56} />
          ))}
        </div>
        {/* main image */}
        <ImgBox h={360} />
        {/* info */}
        <div className="wf-col wf-gap-3">
          <Heading size="h1" style={{ fontSize: 30 }}>Cardholder №3</Heading>
          <div className="wf-muted">Bridle leather · Made in Porto</div>
          <div style={{ fontSize: 22, fontFamily: 'var(--hand-display)', fontWeight: 700 }}>$58</div>
          <div>
            <div className="wf-eyebrow">Color</div>
            <div className="wf-row wf-gap-2 wf-mt-2">
              {['#1d1d1f', '#5a3a26', '#a87a4d'].map((c, i) => (
                <div key={c} style={{ width: 28, height: 28, borderRadius: '50%', background: c, border: i === 0 ? '2px solid var(--ink)' : '1px solid var(--ink-faint)', boxShadow: i === 0 ? '0 0 0 2px var(--paper), 0 0 0 3px var(--ink)' : 'none' }} />
              ))}
            </div>
          </div>
          <Btn accent block lg>Add to bag</Btn>
          <Btn ghost block>Save for later <span style={{ marginLeft: 6 }}><Icon name="heart" size={14} /></span></Btn>
          <div className="wf-mt-2" style={{ borderTop: '1px solid var(--ink-faint)', paddingTop: 12 }}>
            <Lines n={3} h={6} />
          </div>
          <div className="wf-tiny wf-muted">Free shipping over $80 · Ships in 2 days</div>
        </div>
      </div>
    </div>
    <Anno x={26} y={70} dir="right">Classic 3-col: thumbs · image · info</Anno>
  </Desktop>
);

const Product_FullBleed = () => (
  <Desktop url="northhide.micro.shop/cardholder-3">
    <div style={{ height: '100%', overflow: 'hidden', position: 'relative' }}>
      {/* floating top nav (frosted) */}
      <div style={{
        position: 'absolute', top: 12, left: 32, right: 32, height: 40,
        borderRadius: 20, background: 'rgba(253,252,248,0.85)', border: '1px solid var(--ink-faint)',
        display: 'flex', alignItems: 'center', justifyContent: 'space-between', padding: '0 18px', zIndex: 5,
      }}>
        <Icon name="chev_l" />
        <span className="wf-tiny">Cardholder №3</span>
        <Icon name="bag" />
      </div>
      {/* full bleed image */}
      <ImgBox h={360} plain style={{ borderRadius: 0, borderLeft: 0, borderRight: 0, borderTop: 0 }} />
      {/* dot pager */}
      <div style={{ position: 'absolute', top: 320, left: '50%', transform: 'translateX(-50%)', display: 'flex', gap: 6, zIndex: 4 }}>
        {[1, 2, 3, 4].map((i) => (
          <div key={i} style={{ width: 6, height: 6, borderRadius: 3, background: i === 1 ? 'var(--ink)' : 'var(--ink-faint)' }} />
        ))}
      </div>
      {/* pinned bottom card */}
      <div className="wf-box" style={{
        position: 'absolute', bottom: 16, left: 24, right: 24, padding: '20px 24px',
        background: 'var(--paper)', display: 'grid', gridTemplateColumns: '1fr auto', gap: 16, alignItems: 'center',
        borderWidth: 1.6, borderRadius: 12,
      }}>
        <div>
          <Heading size="h2">Cardholder №3</Heading>
          <div className="wf-muted wf-small">Bridle · 3 pockets · Porto</div>
          <div className="wf-row wf-gap-2 wf-mt-2">
            <Chip on>Black</Chip><Chip>Tan</Chip><Chip>Cognac</Chip>
          </div>
        </div>
        <div style={{ textAlign: 'right' }}>
          <div style={{ fontSize: 26, fontFamily: 'var(--hand-display)', fontWeight: 700 }}>$58</div>
          <Btn accent style={{ marginTop: 6 }}>Add to bag</Btn>
        </div>
      </div>
    </div>
    <Anno x={20} y={250} dir="right">Photography first · info pinned to bottom</Anno>
  </Desktop>
);

const Product_Story = () => (
  <Desktop url="northhide.micro.shop/cardholder-3">
    <div style={{ height: '100%', overflow: 'hidden', position: 'relative' }}>
      <div className="wf-between" style={{ padding: '12px 32px' }}>
        <Icon name="chev_l" />
        <span className="wf-tiny wf-muted">Cardholder №3</span>
        <Icon name="bag" />
      </div>
      <div style={{ overflow: 'hidden', height: 'calc(100% - 56px)' }}>
        {/* alternating image / fact rows */}
        <div className="wf-center" style={{ flexDirection: 'column', padding: '24px 80px 12px' }}>
          <Heading size="h1" style={{ fontSize: 32, textAlign: 'center' }}>Cardholder №3.<br/>Cut from one piece.</Heading>
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 0 }}>
          <div className="wf-center wf-p-6" style={{ flexDirection: 'column', textAlign: 'center', gap: 6 }}>
            <span className="wf-eyebrow">Material</span>
            <div className="wf-h2">Bridle leather, Tärnsjö.</div>
            <div className="wf-small wf-muted">Tanned slowly, finished by hand.</div>
          </div>
          <ImgBox h={170} />
        </div>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 0 }}>
          <ImgBox h={170} dark />
          <div className="wf-center wf-p-6" style={{ flexDirection: 'column', textAlign: 'center', gap: 6, background: 'var(--paper-2)' }}>
            <span className="wf-eyebrow">Detail</span>
            <div className="wf-h2">Three pockets. One fold.</div>
            <div className="wf-small wf-muted">Sized for cards, folded bills.</div>
          </div>
        </div>
        <div className="wf-center" style={{ padding: 24, gap: 12 }}>
          <span style={{ fontSize: 22, fontFamily: 'var(--hand-display)', fontWeight: 700 }}>$58</span>
          <Btn accent>Buy</Btn>
        </div>
      </div>
    </div>
    <Anno x={24} y={120} dir="right">Vertical story · alternating image/text</Anno>
  </Desktop>
);

// ─── CART (mobile, 3 variants) ───────────────────────────────────────────────

const Cart_List = () => (
  <Phone statusTitle="Bag">
    <div className="wf-col" style={{ height: '100%', padding: '8px 16px' }}>
      <div className="wf-between" style={{ marginBottom: 8 }}>
        <span className="wf-tiny wf-muted">3 items</span>
        <span className="wf-tiny wf-accent-text">Edit</span>
      </div>
      {[
        { name: 'Cardholder №3', meta: 'Black · Bridle', price: 58 },
        { name: 'Long Wallet', meta: 'Cognac', price: 128 },
        { name: 'Keychain Loop', meta: 'Black', price: 19 },
      ].map((item) => (
        <div key={item.name} className="wf-row wf-gap-3" style={{ padding: '12px 0', borderBottom: '1px solid var(--ink-faint)' }}>
          <ImgBox w={56} h={56} />
          <div className="wf-grow">
            <div style={{ fontSize: 14, fontWeight: 600 }}>{item.name}</div>
            <div className="wf-tiny wf-muted">{item.meta}</div>
            <div className="wf-row wf-gap-2 wf-mt-2 wf-tiny">
              <div className="wf-box wf-center" style={{ width: 22, height: 22, borderRadius: 4 }}>−</div>
              <span style={{ minWidth: 18, textAlign: 'center' }}>1</span>
              <div className="wf-box wf-center" style={{ width: 22, height: 22, borderRadius: 4 }}>+</div>
            </div>
          </div>
          <div style={{ fontSize: 14, fontWeight: 600 }}>${item.price}</div>
        </div>
      ))}
      <div style={{ flex: 1 }} />
      <div className="wf-col wf-gap-2 wf-mt-3">
        <div className="wf-between"><span className="wf-small">Subtotal</span><span className="wf-small">$205</span></div>
        <div className="wf-between"><span className="wf-small">Shipping</span><span className="wf-small">$0</span></div>
        <div className="wf-between" style={{ paddingTop: 8, borderTop: '1px solid var(--ink-faint)' }}>
          <span style={{ fontSize: 16, fontWeight: 600 }}>Total</span>
          <span style={{ fontSize: 16, fontWeight: 700 }}>$205</span>
        </div>
      </div>
      <Btn accent lg block style={{ marginTop: 12 }}>Checkout</Btn>
    </div>
    <Anno x={20} y={80} dir="down">Classic list · per-row qty</Anno>
  </Phone>
);

const Cart_Stacked = () => (
  <Phone statusTitle="Bag">
    <div className="wf-col" style={{ height: '100%', padding: 12, gap: 10, position: 'relative' }}>
      {[
        { name: 'Cardholder №3', meta: 'Black', price: 58 },
        { name: 'Long Wallet', meta: 'Cognac', price: 128 },
        { name: 'Keychain Loop', meta: 'Black', price: 19 },
      ].map((item) => (
        <div key={item.name} className="wf-box" style={{ padding: 12, display: 'flex', gap: 12, borderRadius: 12, borderWidth: 1.4 }}>
          <ImgBox w={70} h={70} />
          <div className="wf-grow wf-col wf-gap-2">
            <div className="wf-between">
              <div>
                <div style={{ fontSize: 14, fontWeight: 600 }}>{item.name}</div>
                <div className="wf-tiny wf-muted">{item.meta}</div>
              </div>
              <Icon name="x" size={14} />
            </div>
            <div className="wf-between">
              <div className="wf-row wf-gap-2 wf-tiny" style={{ alignItems: 'center' }}>
                <div className="wf-box wf-center" style={{ width: 20, height: 20, borderRadius: 4 }}>−</div>
                <span>1</span>
                <div className="wf-box wf-center" style={{ width: 20, height: 20, borderRadius: 4 }}>+</div>
              </div>
              <span style={{ fontWeight: 600 }}>${item.price}</span>
            </div>
          </div>
        </div>
      ))}
      <div className="wf-tiny wf-accent-text" style={{ textAlign: 'center', marginTop: 4 }}>+ Add a gift note</div>
      <div style={{ flex: 1 }} />
      {/* bottom sheet summary — rounded top */}
      <div style={{
        margin: '0 -12px -12px', padding: '14px 16px 16px', borderTop: '1.4px solid var(--ink)',
        background: 'var(--paper-2)', borderRadius: '14px 14px 0 0',
      }}>
        <div className="wf-between"><span className="wf-tiny wf-muted">3 items · free ship</span><span className="wf-tiny wf-muted">Subtotal $205</span></div>
        <Btn accent lg block style={{ marginTop: 10 }}>Checkout · $205</Btn>
      </div>
    </div>
    <Anno x={140} y={48} dir="up">Cards · summary as sheet</Anno>
  </Phone>
);

const Cart_Minimal = () => (
  <Phone statusTitle="Bag">
    <div className="wf-col" style={{ height: '100%', padding: 18, gap: 16 }}>
      <div className="wf-mt-4">
        <span className="wf-eyebrow">Your bag</span>
        <Heading size="h1" style={{ fontSize: 44, marginTop: 4 }}>$205</Heading>
        <div className="wf-muted wf-small">3 items · free shipping</div>
      </div>
      {/* tiny stacked thumbs */}
      <div className="wf-row" style={{ gap: -6 }}>
        {[1, 2, 3].map((i) => (
          <ImgBox key={i} w={48} h={48} style={{ marginLeft: i > 1 ? -10 : 0, borderRadius: 8, border: '2px solid var(--paper)' }} />
        ))}
        <div className="wf-tiny wf-muted" style={{ marginLeft: 12, alignSelf: 'center' }}>Cardholder, Wallet, Keychain</div>
      </div>
      <div className="wf-box wf-p-3" style={{ borderRadius: 10 }}>
        <div className="wf-between">
          <span className="wf-small">Show items</span>
          <Icon name="chev_d" />
        </div>
      </div>
      <div className="wf-box wf-p-3" style={{ borderRadius: 10 }}>
        <div className="wf-between">
          <span className="wf-small">Shipping to <b>Porto, PT</b></span>
          <Icon name="chev_r" />
        </div>
      </div>
      <div className="wf-box wf-p-3" style={{ borderRadius: 10 }}>
        <div className="wf-between">
          <span className="wf-small">Pay with <b>Apple Pay</b></span>
          <Icon name="chev_r" />
        </div>
      </div>
      <div style={{ flex: 1 }} />
      <Btn accent lg block>Place order · $205</Btn>
      <div className="wf-tiny wf-muted" style={{ textAlign: 'center' }}>Tap and hold to confirm</div>
    </div>
    <Anno x={150} y={70} dir="left">Total-first · everything collapsed</Anno>
  </Phone>
);

// ─── CHECKOUT (mobile, 3 variants) ───────────────────────────────────────────

const Checkout_Steps = () => (
  <Phone statusTitle="Checkout">
    <div className="wf-col" style={{ height: '100%', padding: 16, gap: 14 }}>
      <div className="wf-between">
        <Icon name="chev_l" />
        <div className="wf-row wf-gap-2 wf-tiny">
          <span className="wf-accent-text"><b>Ship</b></span>
          <span className="wf-muted">→ Pay</span>
          <span className="wf-muted">→ Review</span>
        </div>
        <span style={{ width: 16 }} />
      </div>
      <Heading size="h2">Where to?</Heading>
      <div className="wf-col wf-gap-3">
        {['Full name', 'Email', 'Street', 'City', 'Postcode'].map((label, i) => (
          <div key={label}>
            <div className="wf-tiny wf-muted">{label}</div>
            <div className="wf-box" style={{ height: 38, borderRadius: 8, marginTop: 4, background: i < 2 ? 'var(--paper-2)' : 'transparent', display: 'flex', alignItems: 'center', padding: '0 10px', fontSize: 13 }}>
              {i === 0 && <span>Maria Santos</span>}
              {i === 1 && <span>maria@…</span>}
            </div>
          </div>
        ))}
      </div>
      <div className="wf-row wf-gap-2 wf-tiny wf-muted">
        <Icon name="check" size={14} />
        <span>Save this address for next time</span>
      </div>
      <div style={{ flex: 1 }} />
      <Btn accent lg block>Continue to payment</Btn>
    </div>
    <Anno x={140} y={56} dir="up">Linear · 3 steps</Anno>
  </Phone>
);

const Checkout_Accordion = () => {
  const Section = ({ n, title, open, summary, children }) => (
    <div className="wf-box" style={{ padding: 12, borderRadius: 10, borderWidth: open ? 1.8 : 1.2, borderColor: open ? 'var(--ink)' : 'var(--ink-faint)' }}>
      <div className="wf-between">
        <div className="wf-row wf-gap-2" style={{ alignItems: 'center' }}>
          <div style={{ width: 22, height: 22, borderRadius: 11, border: `1.4px solid ${open ? 'var(--accent)' : 'var(--ink-soft)'}`, background: open ? 'var(--accent)' : 'transparent', color: open ? 'white' : 'var(--ink-2)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: 11, fontWeight: 700 }}>
            {n}
          </div>
          <span style={{ fontWeight: 600 }}>{title}</span>
        </div>
        {!open && summary && <span className="wf-tiny wf-muted">{summary}</span>}
        {open || <Icon name="chev_d" size={14} />}
      </div>
      {open && <div style={{ marginTop: 10 }}>{children}</div>}
    </div>
  );
  return (
    <Phone statusTitle="Checkout">
      <div className="wf-col" style={{ height: '100%', padding: 14, gap: 10 }}>
        <Section n={1} title="Contact" summary="maria@…" />
        <Section n={2} title="Shipping" open>
          <Lines n={2} h={6} />
          <div className="wf-row wf-gap-2 wf-mt-2">
            <Chip on>Standard · free</Chip>
            <Chip>Express · $9</Chip>
          </div>
        </Section>
        <Section n={3} title="Payment" summary="Apple Pay" />
        <div style={{ flex: 1 }} />
        <div className="wf-box wf-p-3" style={{ borderRadius: 10, background: 'var(--paper-2)' }}>
          <div className="wf-between"><span className="wf-tiny">Subtotal</span><span className="wf-tiny">$205</span></div>
          <div className="wf-between"><span className="wf-tiny">Shipping</span><span className="wf-tiny">free</span></div>
          <div className="wf-between" style={{ marginTop: 4 }}>
            <span style={{ fontWeight: 600 }}>Total</span>
            <span style={{ fontWeight: 700 }}>$205</span>
          </div>
        </div>
        <Btn accent lg block>Place order</Btn>
      </div>
      <Anno x={150} y={50} dir="up">Single screen · accordion</Anno>
    </Phone>
  );
};

const Checkout_Express = () => (
  <Phone statusTitle="Checkout">
    <div className="wf-col" style={{ height: '100%', padding: 18, gap: 14 }}>
      <div className="wf-between">
        <Icon name="chev_l" />
        <span className="wf-tiny wf-muted">Express</span>
        <span style={{ width: 16 }} />
      </div>
      <div className="wf-mt-2">
        <span className="wf-eyebrow">Total</span>
        <div style={{ fontSize: 44, fontFamily: 'var(--hand-display)', fontWeight: 700, lineHeight: 1 }}>$205.00</div>
      </div>
      {/* three taps */}
      <div className="wf-col wf-gap-3 wf-mt-2">
        <div className="wf-box wf-p-3" style={{ borderRadius: 10 }}>
          <div className="wf-tiny wf-muted">Ship to</div>
          <div className="wf-between">
            <span style={{ fontSize: 15 }}>Maria · Rua do Almada 42, Porto</span>
            <Icon name="chev_r" size={14} />
          </div>
        </div>
        <div className="wf-box wf-p-3" style={{ borderRadius: 10 }}>
          <div className="wf-tiny wf-muted">Pay with</div>
          <div className="wf-between">
            <span style={{ fontSize: 15 }}>•••• 4221 · Visa</span>
            <Icon name="chev_r" size={14} />
          </div>
        </div>
        <div className="wf-box wf-p-3" style={{ borderRadius: 10 }}>
          <div className="wf-tiny wf-muted">Bag</div>
          <div className="wf-between">
            <span style={{ fontSize: 15 }}>3 items · free ship</span>
            <Icon name="chev_r" size={14} />
          </div>
        </div>
      </div>
      <div style={{ flex: 1 }} />
      <Btn accent lg block style={{ height: 52, fontSize: 17 }}>
        <span style={{ marginRight: 6 }}>Hold to pay</span>
        <Icon name="check" size={16} />
      </Btn>
      <div className="wf-tiny wf-muted" style={{ textAlign: 'center' }}>Face ID will confirm</div>
    </div>
    <Anno x={148} y={150} dir="left">Returning shopper · 3-tap</Anno>
  </Phone>
);

Object.assign(window, {
  Onboarding_Stepper, Onboarding_Cards, Onboarding_GoalFirst,
  Home_Tiles, Home_Editorial, Home_Rails,
  Product_Classic, Product_FullBleed, Product_Story,
  Cart_List, Cart_Stacked, Cart_Minimal,
  Checkout_Steps, Checkout_Accordion, Checkout_Express,
});

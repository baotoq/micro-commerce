// Main app — composes the design canvas with all wireframe sections.

const TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "annotations": true,
  "accent": true,
  "wobble": false
}/*EDITMODE-END*/;

function App() {
  const [t, setTweak] = useTweaks(TWEAK_DEFAULTS);

  // Apply body-class flags so CSS can react globally.
  React.useEffect(() => {
    document.body.classList.toggle('wf-no-anno', !t.annotations);
    document.body.classList.toggle('wf-no-accent', !t.accent);
    document.body.classList.toggle('wf-wobble', !!t.wobble);
  }, [t.annotations, t.accent, t.wobble]);

  return (
    <>
      <DesignCanvas>
        <DCSection id="onboarding" title="Onboarding" subtitle="First-run flow — pick what to sell, set up shop">
          <DCArtboard id="ob-stepper" label="A · Linear stepper" width={360} height={720}><Onboarding_Stepper /></DCArtboard>
          <DCArtboard id="ob-cards" label="B · Card stack" width={360} height={720}><Onboarding_Cards /></DCArtboard>
          <DCArtboard id="ob-goal" label="C · Goal-first / inline" width={360} height={720}><Onboarding_GoalFirst /></DCArtboard>
        </DCSection>

        <DCSection id="home" title="Storefront home" subtitle="Public shop landing — desktop">
          <DCArtboard id="home-tiles" label="A · Edge-to-edge tiles" width={1100} height={720}><Home_Tiles /></DCArtboard>
          <DCArtboard id="home-editorial" label="B · Editorial mosaic" width={1100} height={720}><Home_Editorial /></DCArtboard>
          <DCArtboard id="home-rails" label="C · Hero + product rails" width={1100} height={720}><Home_Rails /></DCArtboard>
        </DCSection>

        <DCSection id="product" title="Product detail" subtitle="The page that does the selling">
          <DCArtboard id="prod-classic" label="A · Photo · info classic" width={1100} height={620}><Product_Classic /></DCArtboard>
          <DCArtboard id="prod-bleed" label="B · Full-bleed + pinned card" width={1100} height={620}><Product_FullBleed /></DCArtboard>
          <DCArtboard id="prod-story" label="C · Vertical story" width={1100} height={620}><Product_Story /></DCArtboard>
        </DCSection>

        <DCSection id="cart" title="Cart" subtitle="Mobile · what people see before checkout">
          <DCArtboard id="cart-list" label="A · List" width={360} height={720}><Cart_List /></DCArtboard>
          <DCArtboard id="cart-stacked" label="B · Stacked cards" width={360} height={720}><Cart_Stacked /></DCArtboard>
          <DCArtboard id="cart-min" label="C · Total-first minimal" width={360} height={720}><Cart_Minimal /></DCArtboard>
        </DCSection>

        <DCSection id="checkout" title="Checkout" subtitle="Mobile · pay & confirm">
          <DCArtboard id="co-steps" label="A · Multi-step" width={360} height={720}><Checkout_Steps /></DCArtboard>
          <DCArtboard id="co-acc" label="B · Single-page accordion" width={360} height={720}><Checkout_Accordion /></DCArtboard>
          <DCArtboard id="co-express" label="C · Express 3-tap" width={360} height={720}><Checkout_Express /></DCArtboard>
        </DCSection>

        <DCSection id="seller-dash" title="Seller dashboard" subtitle="The seller's home — what's happening today">
          <DCArtboard id="dash-kpi" label="A · KPI grid + activity" width={1100} height={680}><Dashboard_KPI /></DCArtboard>
          <DCArtboard id="dash-today" label="B · Today-focused" width={1100} height={680}><Dashboard_TodayFocus /></DCArtboard>
          <DCArtboard id="dash-dense" label="C · Data-dense" width={1100} height={680}><Dashboard_DataDense /></DCArtboard>
        </DCSection>

        <DCSection id="seller-listings" title="Seller — create / edit product" subtitle="The listings flow">
          <DCArtboard id="list-form" label="A · Two-column form" width={1100} height={680}><Listings_Form /></DCArtboard>
          <DCArtboard id="list-live" label="B · Form + live preview" width={1100} height={680}><Listings_LivePreview /></DCArtboard>
          <DCArtboard id="list-wizard" label="C · Wizard" width={1100} height={680}><Listings_Wizard /></DCArtboard>
        </DCSection>

        <DCSection id="seller-analytics" title="Seller analytics" subtitle="How the shop is doing">
          <DCArtboard id="ana-grid" label="A · Charts grid" width={1100} height={680}><Analytics_Grid /></DCArtboard>
          <DCArtboard id="ana-big" label="B · One big chart" width={1100} height={680}><Analytics_BigChart /></DCArtboard>
          <DCArtboard id="ana-story" label="C · Insight cards" width={1100} height={680}><Analytics_Story /></DCArtboard>
        </DCSection>

        <DCPostIt top={20} left={60} rotate={-3} width={220}>
          Sketchy wireframes — handwritten but readable. Toggle annotations / accent / wobble in <b>Tweaks</b>.
        </DCPostIt>
      </DesignCanvas>

      <TweaksPanel title="Wireframe controls">
        <TweakSection label="Display">
          <TweakToggle label="Annotations" value={t.annotations} onChange={(v) => setTweak('annotations', v)} />
          <TweakToggle label="Accent (Action Blue)" value={t.accent} onChange={(v) => setTweak('accent', v)} />
          <TweakToggle label="Hand-drawn wobble" value={t.wobble} onChange={(v) => setTweak('wobble', v)} />
        </TweakSection>
      </TweaksPanel>
    </>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App />);

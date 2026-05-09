// hifi-app.jsx — Hi-fi designs assembled into a Design Canvas

const TWEAK_DEFAULTS = /*EDITMODE-BEGIN*/{
  "annotations": false,
  "density": "comfortable",
  "accent": "#C2410C"
}/*EDITMODE-END*/;

function App() {
  const [t, setTweak] = useTweaks(TWEAK_DEFAULTS);

  React.useEffect(() => {
    document.body.classList.toggle('hf-no-anno', !t.annotations);
    document.body.classList.toggle('hf-density-compact', t.density === 'compact');
    const root = document.documentElement;
    const accents = {
      '#C2410C': '#FBE6D6',
      '#1B5E3F': '#DAE9DD',
      '#5B2A6E': '#ECE0F0',
      '#1E3A8A': '#DCE4F4',
    };
    root.style.setProperty('--terra', t.accent);
    root.style.setProperty('--terra-2', accents[t.accent] || '#FBE6D6');
  }, [t.annotations, t.density, t.accent]);

  return (
    <>
      <DesignCanvas>
        <DCSection id="flow" title="End-to-end shopper journey" subtitle="Hi-fi · 8 steps · Mira buys her first pieces from Mira Studio">
          <DCArtboard id="fl-01" label="01 · Welcome"          width={360} height={720}><Flow_01_Welcome   /></DCArtboard>
          <DCArtboard id="fl-02" label="02 · Goal-first setup" width={360} height={720}><Flow_02_Setup     /></DCArtboard>
          <DCArtboard id="fl-03" label="03 · Storefront home"  width={360} height={720}><Flow_03_Home      /></DCArtboard>
          <DCArtboard id="fl-04" label="04 · Product detail"   width={360} height={720}><Flow_04_Product   /></DCArtboard>
          <DCArtboard id="fl-05" label="05 · Cart"             width={360} height={720}><Flow_05_Cart      /></DCArtboard>
          <DCArtboard id="fl-06" label="06 · Shipping"         width={360} height={720}><Flow_06_Shipping  /></DCArtboard>
          <DCArtboard id="fl-07" label="07 · Pay"              width={360} height={720}><Flow_07_Pay       /></DCArtboard>
          <DCArtboard id="fl-08" label="08 · Order placed"     width={360} height={720}><Flow_08_Confirmed /></DCArtboard>
        </DCSection>

        <DCSection id="states" title="In-flow states · mobile" subtitle="Hi-fi · loading, empty, error, success — branches off the happy path">
          <DCArtboard id="st-loading" label="Loading · home skeleton"     width={360} height={720}><State_Loading   /></DCArtboard>
          <DCArtboard id="st-empty"   label="Empty · cart"                width={360} height={720}><State_EmptyCart /></DCArtboard>
          <DCArtboard id="st-error"   label="Error · form &amp; payment"  width={360} height={720}><State_FormError /></DCArtboard>
          <DCArtboard id="st-success" label="Success · added to bag"      width={360} height={720}><State_Success   /></DCArtboard>
        </DCSection>

        <DCSection id="flow-desktop" title="End-to-end shopper journey · desktop web" subtitle="Hi-fi · 8 steps, 1280×800 · same Mira → Mira Studio narrative in browser">
          <DCArtboard id="dfl-01" label="01 · Discover (marketplace)" width={1280} height={800}><DFlow_01_Landing   /></DCArtboard>
          <DCArtboard id="dfl-02" label="02 · Storefront home"        width={1280} height={800}><DFlow_02_Home      /></DCArtboard>
          <DCArtboard id="dfl-03" label="03 · Category · Vessels"     width={1280} height={800}><DFlow_03_Category  /></DCArtboard>
          <DCArtboard id="dfl-04" label="04 · Product detail"         width={1280} height={800}><DFlow_04_Product   /></DCArtboard>
          <DCArtboard id="dfl-05" label="05 · Added · cart drawer"    width={1280} height={800}><DFlow_05_Added     /></DCArtboard>
          <DCArtboard id="dfl-06" label="06 · Cart"                   width={1280} height={820}><DFlow_06_Cart      /></DCArtboard>
          <DCArtboard id="dfl-07" label="07 · Checkout"               width={1280} height={1240}><DFlow_07_Checkout /></DCArtboard>
          <DCArtboard id="dfl-08" label="08 · Order placed"           width={1280} height={800}><DFlow_08_Confirmed /></DCArtboard>
        </DCSection>

        <DCSection id="states-desktop" title="In-flow states · desktop web" subtitle="Hi-fi · same four states, browser-sized">
          <DCArtboard id="dst-loading" label="Loading · home skeleton"    width={1280} height={800}><DState_Loading   /></DCArtboard>
          <DCArtboard id="dst-empty"   label="Empty · cart"               width={1280} height={800}><DState_EmptyCart /></DCArtboard>
          <DCArtboard id="dst-error"   label="Error · form &amp; payment" width={1280} height={800}><DState_FormError /></DCArtboard>
          <DCArtboard id="dst-success" label="Success · added to bag"     width={1280} height={800}><DState_Success   /></DCArtboard>
        </DCSection>

        <DCSection id="seller-flow" title="End-to-end seller journey" subtitle="Hi-fi · 8 steps · Mira opens her shop, ships her first order, gets paid">
          <DCArtboard id="sfl-01" label="01 · Apply"            width={1280} height={800}><SFlow_01_Apply        /></DCArtboard>
          <DCArtboard id="sfl-02" label="02 · Setup · payouts"  width={1280} height={800}><SFlow_02_Setup        /></DCArtboard>
          <DCArtboard id="sfl-03" label="03 · First listing"    width={1280} height={800}><SFlow_03_FirstListing /></DCArtboard>
          <DCArtboard id="sfl-04" label="04 · Day one · empty"  width={1280} height={800}><SFlow_04_DayOne       /></DCArtboard>
          <DCArtboard id="sfl-05" label="05 · First order"      width={1280} height={800}><SFlow_05_FirstOrder   /></DCArtboard>
          <DCArtboard id="sfl-06" label="06 · Pack &amp; ship"  width={1280} height={800}><SFlow_06_PackShip     /></DCArtboard>
          <DCArtboard id="sfl-07" label="07 · Analytics · 1 mo" width={1280} height={800}><SFlow_07_Analytics    /></DCArtboard>
          <DCArtboard id="sfl-08" label="08 · Payout sent"      width={1280} height={800}><SFlow_08_Payout       /></DCArtboard>
        </DCSection>

        <DCSection id="seller-states" title="In-flow states · seller" subtitle="Hi-fi · loading, empty, error, success — branches off the seller path">
          <DCArtboard id="sst-loading" label="Loading · dashboard skeleton" width={1280} height={800}><SState_Loading           /></DCArtboard>
          <DCArtboard id="sst-empty"   label="Empty · no orders yet"        width={1280} height={800}><SState_EmptyOrders       /></DCArtboard>
          <DCArtboard id="sst-error"   label="Error · payout failed"        width={1280} height={800}><SState_PayoutError       /></DCArtboard>
          <DCArtboard id="sst-success" label="Success · first sale"         width={1280} height={800}><SState_FirstSaleSuccess  /></DCArtboard>
        </DCSection>

        <DCSection id="listings-flow" title="End-to-end listings management" subtitle="Hi-fi · 5 steps · Mira fixes prices &amp; restocks before a Friday drop">
          <DCArtboard id="lfl-01" label="01 · Catalog"        width={1280} height={800}><LFlow_01_Catalog   /></DCArtboard>
          <DCArtboard id="lfl-02" label="02 · Bulk edit"      width={1280} height={800}><LFlow_02_Bulk      /></DCArtboard>
          <DCArtboard id="lfl-03" label="03 · Edit · variants" width={1280} height={800}><LFlow_03_Edit     /></DCArtboard>
          <DCArtboard id="lfl-04" label="04 · Preview"        width={1280} height={800}><LFlow_04_Preview   /></DCArtboard>
          <DCArtboard id="lfl-05" label="05 · Published · diff" width={1280} height={800}><LFlow_05_Published /></DCArtboard>
        </DCSection>

        <DCSection id="buyer-extras" title="Buyer · account, search, returns, reviews, notifications" subtitle="Hi-fi · mobile · the rest of the shopper surface beyond the happy path">
          <DCArtboard id="be-search"   label="01 · Search results · filters" width={360} height={720}><MFlow_Search    /></DCArtboard>
          <DCArtboard id="be-account"  label="02 · Account home"             width={360} height={720}><MFlow_Account   /></DCArtboard>
          <DCArtboard id="be-tracking" label="03 · Order tracking"           width={360} height={720}><MFlow_Tracking  /></DCArtboard>
          <DCArtboard id="be-addr"     label="04 · Addresses"                width={360} height={720}><MFlow_Addresses /></DCArtboard>
          <DCArtboard id="be-return"   label="05 · Return request"           width={360} height={720}><MFlow_Return    /></DCArtboard>
          <DCArtboard id="be-review"   label="06 · Leave a review"           width={360} height={720}><MFlow_Review    /></DCArtboard>
          <DCArtboard id="be-notifs"   label="07 · Notifications"            width={360} height={720}><MFlow_Notifs    /></DCArtboard>
        </DCSection>

        <DCSection id="seller-mgmt" title="Seller · orders, promos, marketing" subtitle="Hi-fi · the management surfaces around the single Pack &amp; Ship screen">
          <DCArtboard id="sm-inbox"     label="01 · Orders inbox"            width={1280} height={800}><SMgmt_OrdersInbox /></DCArtboard>
          <DCArtboard id="sm-detail"    label="02 · Order detail · refund &amp; partial" width={1280} height={1040}><SMgmt_OrderDetail /></DCArtboard>
          <DCArtboard id="sm-promos"    label="03 · Promotions · discount codes" width={1280} height={800}><SMgmt_Promos /></DCArtboard>
          <DCArtboard id="sm-marketing" label="04 · Marketing · email recent buyers" width={1280} height={800}><SMgmt_Marketing /></DCArtboard>
        </DCSection>

        <DCSection id="seller-config" title="Seller · shop settings &amp; finance" subtitle="Hi-fi · the back-office configuration and finance detail">
          <DCArtboard id="sc-settings" label="01 · Shop settings · shipping zones" width={1280} height={1040}><SCfg_Settings /></DCArtboard>
          <DCArtboard id="sc-finance"  label="02 · Finance · statements &amp; taxes" width={1280} height={1040}><SCfg_Finance  /></DCArtboard>
        </DCSection>

        <DCSection id="seller-mobile" title="Seller · mobile companion" subtitle="Hi-fi · iPhone · check-in, pack, restock from the road">
          <DCArtboard id="sm-m-home"     label="01 · Dashboard"      width={360} height={720}><MSeller_Dashboard   /></DCArtboard>
          <DCArtboard id="sm-m-orders"   label="02 · Orders inbox"   width={360} height={720}><MSeller_Orders      /></DCArtboard>
          <DCArtboard id="sm-m-detail"   label="03 · Order · pack &amp; ship" width={360} height={720}><MSeller_OrderDetail /></DCArtboard>
          <DCArtboard id="sm-m-listings" label="04 · Listings"       width={360} height={720}><MSeller_Listings    /></DCArtboard>
        </DCSection>

        <DCSection id="analytics-flow" title="End-to-end analytics" subtitle="Hi-fi · 5 steps · Mira investigates a Tuesday spike, schedules a digest">
          <DCArtboard id="afl-01" label="01 · Overview · anomaly"     width={1280} height={800}><AFlow_01_Overview  /></DCArtboard>
          <DCArtboard id="afl-02" label="02 · Drill · Apr 1"           width={1280} height={800}><AFlow_02_Drill     /></DCArtboard>
          <DCArtboard id="afl-03" label="03 · Compare periods"         width={1280} height={800}><AFlow_03_Compare   /></DCArtboard>
          <DCArtboard id="afl-04" label="04 · Insight · recommendations" width={1280} height={800}><AFlow_04_Insight /></DCArtboard>
          <DCArtboard id="afl-05" label="05 · Digest scheduled"        width={1280} height={800}><AFlow_05_Scheduled /></DCArtboard>
        </DCSection>

        <DCPostIt top={20} left={60} rotate={-2} width={260}>
          <b>End-to-end flow</b> at the top — 8 steps + 4 states. Below: 3 variants per section for comparison. <b>Mira Studio</b> ceramics. Use Tweaks to swap accent / density / annotations.
        </DCPostIt>
      </DesignCanvas>

      <TweaksPanel title="Hi-fi controls">
        <TweakSection label="Display">
          <TweakToggle label="Show annotations" value={t.annotations} onChange={(v) => setTweak('annotations', v)} />
          <TweakRadio  label="Density" value={t.density} options={['comfortable', 'compact']} onChange={(v) => setTweak('density', v)} />
        </TweakSection>
        <TweakSection label="Accent">
          <TweakColor label="Brand accent" value={t.accent} options={['#C2410C', '#1B5E3F', '#5B2A6E', '#1E3A8A']} onChange={(v) => setTweak('accent', v)} />
        </TweakSection>
      </TweaksPanel>
    </>
  );
}

ReactDOM.createRoot(document.getElementById('root')).render(<App />);

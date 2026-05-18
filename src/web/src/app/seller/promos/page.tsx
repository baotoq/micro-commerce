import { NewPromoDrawer } from "@/components/seller/new-promo-drawer";
import { PromoStatCard } from "@/components/seller/promo-stat-card";
import { PromosTable } from "@/components/seller/promos-table";
import { PromosTabs } from "@/components/seller/promos-tabs";
import { SellerTopbar } from "@/components/seller/seller-topbar";
import { getPromoStats, getPromos, getPromoTabs } from "@/lib/seller/promos/data";

export default function PromosPage() {
  const stats = getPromoStats();
  const tabs = getPromoTabs();
  const promos = getPromos();

  return (
    <div className="relative flex h-screen min-w-0 flex-col overflow-hidden">
      <SellerTopbar
        title="Discounts & promotions"
        subtitle="3 active · $2,740 driven · 281 redemptions"
        actions={
          <button
            type="button"
            className="flex items-center gap-1.5 rounded-pill bg-foreground px-4 py-2 text-sm font-semibold text-white hover:bg-foreground/90"
          >
            <svg
              width="12"
              height="12"
              viewBox="0 0 20 20"
              fill="none"
              stroke="currentColor"
              strokeWidth="2"
              strokeLinecap="round"
              aria-hidden="true"
            >
              <title>plus</title>
              <path d="M10 4v12M4 10h12" />
            </svg>
            New promotion
          </button>
        }
      />

      {/* Stat row — right margin reserves space for the 460px drawer */}
      <div
        className="grid grid-cols-4 gap-3"
        style={{ padding: "20px 28px 0", marginRight: 460 }}
      >
        {stats.map((stat) => (
          <PromoStatCard key={stat.label} stat={stat} />
        ))}
      </div>

      {/* Tabs */}
      <div style={{ marginRight: 460 }}>
        <PromosTabs tabs={tabs} />
      </div>

      {/* Table card — joined to tabs, no top radius */}
      <div
        className="grow overflow-auto"
        style={{ padding: "0 28px 20px", marginRight: 460 }}
      >
        <div
          className="overflow-hidden rounded-xl border border-black/[0.06] bg-white"
          style={{
            marginTop: -1,
            borderTopLeftRadius: 0,
            borderTopRightRadius: 0,
          }}
        >
          <PromosTable promos={promos} />
        </div>
      </div>

      {/* Always-open new promotion drawer — absolutely positioned within relative wrapper */}
      <NewPromoDrawer />
    </div>
  );
}

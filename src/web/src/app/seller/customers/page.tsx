import { SellerTopbar } from "@/components/seller/shell/seller-topbar";

export default function CustomersPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar title="Customers" subtitle="All time" />
      <section className="flex-1 bg-canvas-parchment p-10">
        <p className="text-sm text-foreground/70">Coming soon</p>
      </section>
    </div>
  );
}

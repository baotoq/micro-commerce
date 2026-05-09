import { SellerTopbar } from "@/components/seller/seller-topbar";

export default function CustomersPage() {
  return (
    <div className="flex flex-col min-h-screen">
      <SellerTopbar title="Customers" subtitle="All time" />
      <section className="p-10">
        <p className="text-sm text-[#1d1d1f]/70">Coming soon</p>
      </section>
    </div>
  );
}

import { connection } from "next/server";
import { CustomersTable } from "@/components/seller/customers/customers-table";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { getCustomers } from "@/lib/seller/customers/data";

export default async function CustomersPage() {
  await connection();
  const [customers] = await Promise.all([getCustomers()]);

  return (
    <div className="flex h-screen min-w-0 flex-col overflow-hidden">
      <SellerTopbar title="Customers" subtitle="All time" />

      <div className="grow overflow-auto" style={{ padding: "20px 28px" }}>
        <div className="overflow-hidden rounded-xl border border-black/[0.06] bg-white">
          <CustomersTable rows={customers} />
        </div>
      </div>
    </div>
  );
}

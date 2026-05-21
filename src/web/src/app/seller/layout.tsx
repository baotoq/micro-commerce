import { SellerSidebar } from "@/components/seller/shell/sidebar";

export default function SellerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="grid min-h-screen grid-cols-[240px_1fr] bg-white text-[#1d1d1f]">
      <SellerSidebar />
      <main className="min-w-0">{children}</main>
    </div>
  );
}

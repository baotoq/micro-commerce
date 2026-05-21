// web/src/app/seller/layout.tsx
import { SellerSidebar } from "@/components/seller/shell/sidebar";

export default function SellerLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="grid min-h-screen grid-cols-[240px_1fr] bg-background text-foreground">
      <SellerSidebar />
      <main className="min-w-0">{children}</main>
    </div>
  );
}

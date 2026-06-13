import { Suspense } from "react";
import { ShopTopbar } from "@/components/storefront/topbar";

export default function StorefrontLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-h-screen flex-col">
      <ShopTopbar />
      {/* Cache Components: the storefront pages read searchParams/cookies
          (uncached request data), so the dynamic page tree must stream inside a
          <Suspense> boundary while the static chrome (topbar/footer) prerenders. */}
      <main className="flex-1">
        <Suspense fallback={null}>{children}</Suspense>
      </main>
      <footer className="border-t py-8 text-center text-xs text-muted-foreground">
        Micro Commerce · hand-thrown in Oakland · free local delivery
      </footer>
    </div>
  );
}

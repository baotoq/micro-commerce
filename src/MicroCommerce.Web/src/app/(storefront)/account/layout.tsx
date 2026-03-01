import { AccountSidebar } from "@/components/account/account-sidebar";

export default function AccountLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="mx-auto max-w-5xl px-4 py-8 sm:px-6 lg:px-10">
      <h1 className="mb-8 text-2xl font-bold tracking-tight text-foreground">
        My Account
      </h1>

      <div className="flex flex-col gap-8 lg:flex-row">
        {/* Sidebar - horizontal on mobile, vertical on desktop */}
        <aside className="w-full shrink-0 lg:w-[280px]">
          <AccountSidebar />
        </aside>

        <main className="min-w-0 flex-1">{children}</main>
      </div>
    </div>
  );
}

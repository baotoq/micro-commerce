import { AccountSidebar } from "@/components/account/account-sidebar";

export default function AccountLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="container mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-8 text-3xl font-bold">My Account</h1>

      <div className="grid gap-8 lg:grid-cols-[256px_1fr]">
        <aside className="hidden lg:block">
          <AccountSidebar />
        </aside>

        <div className="lg:hidden mb-6">
          <AccountSidebar />
        </div>

        <main>{children}</main>
      </div>
    </div>
  );
}

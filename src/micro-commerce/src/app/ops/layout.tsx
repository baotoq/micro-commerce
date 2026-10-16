import Link from "next/link";

export default function OperationsLayout({ children }: LayoutProps<"/ops">) {
  return (
    <div className="mx-auto flex w-full max-w-4xl flex-1 flex-col gap-8 px-6 py-10 sm:flex-row">
      <nav className="flex shrink-0 flex-col gap-2 sm:w-48">
        <h2 className="font-semibold">Operations</h2>
        <Link href="/ops/categories" className="text-sm text-muted-foreground hover:text-foreground">
          Categories
        </Link>
      </nav>
      <main className="min-w-0 flex-1">{children}</main>
    </div>
  );
}

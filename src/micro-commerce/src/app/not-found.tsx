import Link from "next/link";

export default function NotFound() {
  return (
    <main className="mx-auto w-full max-w-3xl px-4 py-12 sm:px-6">
      <h1 className="text-2xl font-semibold">This page could not be found.</h1>
      <p className="mt-2 text-muted-foreground">The page may have moved or is no longer available.</p>
      <Link
        href="/"
        className="mt-6 inline-flex rounded-sm font-medium underline underline-offset-4 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-ring"
      >
        Browse the Marketplace
      </Link>
    </main>
  );
}

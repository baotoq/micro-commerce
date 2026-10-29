"use client";

import Link from "next/link";
import { Button } from "@/components/ui/button";

export default function ErrorPage({ retry }: { error: Error & { digest?: string }; retry: () => void }) {
  return (
    <main className="mx-auto w-full max-w-3xl px-4 py-12 sm:px-6">
      <h1 className="text-2xl font-semibold">This page could not be loaded.</h1>
      <p className="mt-2 text-muted-foreground">Try again, or return to the Marketplace.</p>
      <div className="mt-6 flex flex-wrap gap-3">
        <Button onClick={retry}>Try again</Button>
        <Button asChild variant="outline">
          <Link href="/">Browse the Marketplace</Link>
        </Button>
      </div>
    </main>
  );
}

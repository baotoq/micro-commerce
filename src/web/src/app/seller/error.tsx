"use client";

// Route-level error boundary for `/seller/*`. App Router instantiates this
// when an async Server Component or nested client component throws — the
// `reset` callback re-renders the segment. We log to console (the project
// has no observability lib wired yet) so dev/QA can grab the error from
// agent-browser's `errors` channel.

import Link from "next/link";
import { useEffect } from "react";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

export default function SellerError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    console.error("[seller] route error:", error);
  }, [error]);

  return (
    <section className="flex min-h-[60vh] items-center justify-center px-7 py-6">
      <Card className="w-full max-w-md">
        <CardHeader>
          <CardTitle>Something went wrong</CardTitle>
          <CardDescription>
            We hit an error loading this part of the seller dashboard. Try again
            — if it keeps failing, the catalog service may be down.
          </CardDescription>
        </CardHeader>
        <CardContent className="flex flex-col gap-3">
          {error.digest && (
            <p className="font-mono text-[11px] text-muted-foreground">
              digest: {error.digest}
            </p>
          )}
          <div className="flex gap-2">
            <Button onClick={() => reset()}>Try again</Button>
            <Link
              href="/seller"
              className={buttonVariants({ variant: "outline" })}
            >
              Back to dashboard
            </Link>
          </div>
        </CardContent>
      </Card>
    </section>
  );
}

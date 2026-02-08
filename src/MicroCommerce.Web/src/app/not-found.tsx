import Link from "next/link";
import { ArrowLeft } from "lucide-react";

import { Button } from "@/components/ui/button";

export default function NotFound() {
  return (
    <div className="flex min-h-[60vh] flex-col items-center justify-center px-4 text-center">
      <h1 className="mb-2 text-6xl font-bold text-zinc-900">404</h1>
      <h2 className="mb-3 text-2xl font-semibold text-zinc-900">
        Page not found
      </h2>
      <p className="mb-8 max-w-md text-sm text-zinc-500">
        The page you&apos;re looking for doesn&apos;t exist or has been moved.
      </p>
      <Button asChild className="rounded-full" size="lg">
        <Link href="/">
          <ArrowLeft className="mr-2 size-4" />
          Back to Home
        </Link>
      </Button>
    </div>
  );
}

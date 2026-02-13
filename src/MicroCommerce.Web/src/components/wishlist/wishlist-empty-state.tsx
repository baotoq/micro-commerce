import Link from "next/link";
import { Heart } from "lucide-react";
import { Button } from "@/components/ui/button";

export function WishlistEmptyState() {
  return (
    <div className="flex flex-col items-center justify-center py-24 text-center">
      <Heart className="mb-4 size-12 text-zinc-300" />
      <h2 className="text-lg font-semibold text-zinc-900">
        Your wishlist is empty
      </h2>
      <p className="mt-1 text-sm text-zinc-500">
        Save items you love to find them later
      </p>
      <Button asChild className="mt-6 rounded-full" size="lg">
        <Link href="/">Browse Products</Link>
      </Button>
    </div>
  );
}

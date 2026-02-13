"use client";

import { WishlistItemCard } from "./wishlist-item-card";
import { Skeleton } from "@/components/ui/skeleton";
import type { WishlistItemDto } from "@/lib/api";

interface WishlistGridProps {
  items: WishlistItemDto[];
}

export function WishlistGrid({ items }: WishlistGridProps) {
  return (
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
      {items.map((item) => (
        <WishlistItemCard key={item.id} item={item} />
      ))}
    </div>
  );
}

export function WishlistGridSkeleton() {
  return (
    <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
      {[...Array(4)].map((_, i) => (
        <div key={i} className="overflow-hidden rounded-xl bg-white shadow-sm">
          <Skeleton className="aspect-square rounded-none" />
          <div className="p-4">
            <Skeleton className="h-4 w-3/4" />
            <Skeleton className="mt-2 h-4 w-1/4" />
            <Skeleton className="mt-3 h-8 w-full rounded-full" />
          </div>
        </div>
      ))}
    </div>
  );
}

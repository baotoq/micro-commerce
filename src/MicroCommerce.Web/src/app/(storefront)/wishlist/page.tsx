"use client";

import { useSession, signIn } from "next-auth/react";
import { WishlistGrid, WishlistGridSkeleton } from "@/components/wishlist/wishlist-grid";
import { WishlistEmptyState } from "@/components/wishlist/wishlist-empty-state";
import { useUserWishlist } from "@/hooks/use-wishlist";

export default function WishlistPage() {
  const { data: session } = useSession();
  const { data: wishlist, isLoading } = useUserWishlist();

  if (!session) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <h1 className="text-2xl font-bold text-zinc-900">Sign in to view your wishlist</h1>
          <p className="mt-2 text-sm text-zinc-500">
            You need to be signed in to save and view your wishlist
          </p>
          <button
            onClick={() => signIn("keycloak")}
            className="mt-6 rounded-full bg-zinc-900 px-6 py-2.5 text-sm font-medium text-white transition-colors hover:bg-zinc-700"
          >
            Sign In
          </button>
        </div>
      </div>
    );
  }

  const items = wishlist ?? [];

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-zinc-900">My Wishlist</h1>
        {!isLoading && items.length > 0 && (
          <p className="mt-1 text-sm text-zinc-500">
            {items.length} {items.length === 1 ? "item" : "items"}
          </p>
        )}
      </div>

      {isLoading ? (
        <WishlistGridSkeleton />
      ) : items.length === 0 ? (
        <WishlistEmptyState />
      ) : (
        <WishlistGrid items={items} />
      )}
    </div>
  );
}

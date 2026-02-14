"use client";

import { Heart } from "lucide-react";
import { useSession, signIn } from "next-auth/react";
import { useToggleWishlist } from "@/hooks/use-wishlist";

interface WishlistToggleButtonProps {
  productId: string;
  className?: string;
}

export function WishlistToggleButton({ productId, className }: WishlistToggleButtonProps) {
  const { data: session } = useSession();
  const { toggle, isInWishlist, isPending } = useToggleWishlist(productId);

  const handleClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();

    if (!session) {
      signIn("keycloak");
      return;
    }

    toggle();
  };

  return (
    <button
      onClick={handleClick}
      disabled={isPending}
      aria-label={isInWishlist ? "Remove from wishlist" : "Add to wishlist"}
      className={className}
    >
      <Heart
        className={`size-5 transition-colors ${
          isInWishlist
            ? "fill-red-500 text-red-500"
            : "text-zinc-400 hover:text-red-500"
        }`}
      />
    </button>
  );
}

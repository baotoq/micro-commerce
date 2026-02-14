"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import {
  getUserWishlist,
  getWishlistCount,
  getWishlistProductIds,
  addToWishlist,
  removeFromWishlist,
} from "@/lib/api";

const WISHLIST_QUERY_KEY = ["wishlist"] as const;
const WISHLIST_COUNT_QUERY_KEY = ["wishlist", "count"] as const;
const WISHLIST_PRODUCT_IDS_QUERY_KEY = ["wishlist", "product-ids"] as const;

export function useWishlistProductIds() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: WISHLIST_PRODUCT_IDS_QUERY_KEY,
    queryFn: async () => {
      if (!session?.accessToken) return new Set<string>();
      const ids = await getWishlistProductIds(session.accessToken);
      return new Set(ids);
    },
    enabled: !!session?.accessToken,
    initialData: () => new Set<string>(),
  });
}

export function useWishlistCount() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: WISHLIST_COUNT_QUERY_KEY,
    queryFn: () => {
      if (!session?.accessToken) throw new Error("No access token");
      return getWishlistCount(session.accessToken);
    },
    enabled: !!session?.accessToken,
  });
}

export function useUserWishlist() {
  const { data: session } = useSession();

  return useQuery({
    queryKey: WISHLIST_QUERY_KEY,
    queryFn: () => {
      if (!session?.accessToken) throw new Error("No access token");
      return getUserWishlist(session.accessToken);
    },
    enabled: !!session?.accessToken,
  });
}

export function useToggleWishlist(productId: string) {
  const queryClient = useQueryClient();
  const { data: session } = useSession();
  const { data: productIds } = useWishlistProductIds();

  const isInWishlist = productIds?.has(productId) ?? false;

  const addMutation = useMutation({
    mutationFn: () => {
      if (!session?.accessToken) throw new Error("No access token");
      return addToWishlist(session.accessToken, productId);
    },
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: WISHLIST_PRODUCT_IDS_QUERY_KEY });

      const previousProductIds = queryClient.getQueryData<Set<string>>(
        WISHLIST_PRODUCT_IDS_QUERY_KEY
      );

      queryClient.setQueryData<Set<string>>(WISHLIST_PRODUCT_IDS_QUERY_KEY, (old) => {
        const newSet = new Set(old);
        newSet.add(productId);
        return newSet;
      });

      return { previousProductIds };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousProductIds !== undefined) {
        queryClient.setQueryData(WISHLIST_PRODUCT_IDS_QUERY_KEY, context.previousProductIds);
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: [WISHLIST_QUERY_KEY[0]] });
    },
  });

  const removeMutation = useMutation({
    mutationFn: () => {
      if (!session?.accessToken) throw new Error("No access token");
      return removeFromWishlist(session.accessToken, productId);
    },
    onMutate: async () => {
      await queryClient.cancelQueries({ queryKey: WISHLIST_PRODUCT_IDS_QUERY_KEY });

      const previousProductIds = queryClient.getQueryData<Set<string>>(
        WISHLIST_PRODUCT_IDS_QUERY_KEY
      );

      queryClient.setQueryData<Set<string>>(WISHLIST_PRODUCT_IDS_QUERY_KEY, (old) => {
        const newSet = new Set(old);
        newSet.delete(productId);
        return newSet;
      });

      return { previousProductIds };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousProductIds !== undefined) {
        queryClient.setQueryData(WISHLIST_PRODUCT_IDS_QUERY_KEY, context.previousProductIds);
      }
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: [WISHLIST_QUERY_KEY[0]] });
    },
  });

  const toggle = () => {
    if (isInWishlist) {
      removeMutation.mutate();
    } else {
      addMutation.mutate();
    }
  };

  return {
    toggle,
    isInWishlist,
    isPending: addMutation.isPending || removeMutation.isPending,
  };
}

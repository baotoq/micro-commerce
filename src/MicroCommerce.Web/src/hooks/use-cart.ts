"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  type AddToCartRequest,
  type CartDto,
  addToCart,
  getCart,
  removeCartItem,
  updateCartItemQuantity,
} from "@/lib/api";

const CART_QUERY_KEY = ["cart"] as const;

export function useCart() {
  return useQuery({
    queryKey: CART_QUERY_KEY,
    queryFn: getCart,
  });
}

export function useCartItemCount() {
  return useQuery({
    queryKey: CART_QUERY_KEY,
    queryFn: getCart,
    select: (data) =>
      data?.items.reduce((sum, item) => sum + item.quantity, 0) ?? 0,
  });
}

export function useAddToCart() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: AddToCartRequest) => addToCart(data),
    onSuccess: (response) => {
      queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY });
      toast.success(response.isUpdate ? "Updated quantity" : "Added to cart");
    },
    onError: () => {
      toast.error("Failed to add to cart");
    },
  });
}

export function useUpdateCartItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ itemId, quantity }: { itemId: string; quantity: number }) =>
      updateCartItemQuantity(itemId, quantity),
    onMutate: async ({ itemId, quantity }) => {
      await queryClient.cancelQueries({ queryKey: CART_QUERY_KEY });

      const previousCart = queryClient.getQueryData<CartDto | null>(
        CART_QUERY_KEY
      );

      queryClient.setQueryData<CartDto | null>(CART_QUERY_KEY, (old) => {
        if (!old) return old;
        return {
          ...old,
          items: old.items.map((item) =>
            item.id === itemId
              ? {
                  ...item,
                  quantity,
                  lineTotal: item.unitPrice * quantity,
                }
              : item
          ),
          totalPrice: old.items.reduce(
            (sum, item) =>
              sum +
              (item.id === itemId
                ? item.unitPrice * quantity
                : item.lineTotal),
            0
          ),
          totalItems: old.items.reduce(
            (sum, item) =>
              sum + (item.id === itemId ? quantity : item.quantity),
            0
          ),
        };
      });

      return { previousCart };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousCart !== undefined) {
        queryClient.setQueryData(CART_QUERY_KEY, context.previousCart);
      }
      toast.error("Failed to update quantity");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY });
    },
  });
}

export function useRemoveCartItem() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (itemId: string) => removeCartItem(itemId),
    onMutate: async (itemId) => {
      await queryClient.cancelQueries({ queryKey: CART_QUERY_KEY });

      const previousCart = queryClient.getQueryData<CartDto | null>(
        CART_QUERY_KEY
      );

      queryClient.setQueryData<CartDto | null>(CART_QUERY_KEY, (old) => {
        if (!old) return old;
        const removedItem = old.items.find((item) => item.id === itemId);
        const filteredItems = old.items.filter((item) => item.id !== itemId);
        return {
          ...old,
          items: filteredItems,
          totalPrice: old.totalPrice - (removedItem?.lineTotal ?? 0),
          totalItems: old.totalItems - (removedItem?.quantity ?? 0),
        };
      });

      return { previousCart };
    },
    onError: (_err, _vars, context) => {
      if (context?.previousCart !== undefined) {
        queryClient.setQueryData(CART_QUERY_KEY, context.previousCart);
      }
      toast.error("Failed to remove item");
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY });
    },
  });
}

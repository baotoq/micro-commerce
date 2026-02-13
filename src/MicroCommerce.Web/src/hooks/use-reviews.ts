"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { useSession } from "next-auth/react";
import { toast } from "sonner";
import {
  getProductReviews,
  getMyReview,
  canReview,
  createReview,
  updateReview,
  deleteReview,
} from "@/lib/api";
import type { CreateReviewRequest, UpdateReviewRequest } from "@/lib/api";

export function useProductReviews(productId: string, page: number) {
  return useQuery({
    queryKey: ["reviews", productId, page],
    queryFn: () => getProductReviews(productId, page),
  });
}

export function useMyReview(productId: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["my-review", productId],
    queryFn: () => getMyReview(productId, session?.accessToken),
    enabled: !!session?.accessToken,
  });
}

export function useCanReview(productId: string) {
  const { data: session } = useSession();

  return useQuery({
    queryKey: ["can-review", productId],
    queryFn: () => canReview(productId, session?.accessToken),
    enabled: !!session?.accessToken,
  });
}

export function useCreateReview(productId: string) {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (data: CreateReviewRequest) => createReview(productId, data, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews", productId] });
      queryClient.invalidateQueries({ queryKey: ["my-review", productId] });
      queryClient.invalidateQueries({ queryKey: ["can-review", productId] });
      toast.success("Review submitted");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to submit review");
    },
  });
}

export function useUpdateReview(productId: string) {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: ({ reviewId, data }: { reviewId: string; data: UpdateReviewRequest }) =>
      updateReview(reviewId, data, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews", productId] });
      queryClient.invalidateQueries({ queryKey: ["my-review", productId] });
      queryClient.invalidateQueries({ queryKey: ["can-review", productId] });
      toast.success("Review updated");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update review");
    },
  });
}

export function useDeleteReview(productId: string) {
  const queryClient = useQueryClient();
  const { data: session } = useSession();

  return useMutation({
    mutationFn: (reviewId: string) => deleteReview(reviewId, session?.accessToken),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["reviews", productId] });
      queryClient.invalidateQueries({ queryKey: ["my-review", productId] });
      queryClient.invalidateQueries({ queryKey: ["can-review", productId] });
      toast.success("Review deleted");
    },
    onError: () => {
      toast.error("Failed to delete review");
    },
  });
}

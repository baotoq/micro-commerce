"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  type CreateCouponRequest,
  createCoupon,
  deleteCoupon,
  getCouponById,
  getCoupons,
  toggleCouponStatus,
  type UpdateCouponRequest,
  updateCoupon,
  validateCoupon,
} from "@/lib/api";

const COUPONS_QUERY_KEY = ["coupons"] as const;

export function useCoupons(
  params: {
    page?: number;
    pageSize?: number;
    isActive?: boolean;
    search?: string;
  } = {},
) {
  return useQuery({
    queryKey: [...COUPONS_QUERY_KEY, params],
    queryFn: () => getCoupons(params),
  });
}

export function useCoupon(id: string) {
  return useQuery({
    queryKey: [...COUPONS_QUERY_KEY, id],
    queryFn: () => getCouponById(id),
    enabled: !!id,
  });
}

export function useCreateCoupon() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCouponRequest) => createCoupon(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUPONS_QUERY_KEY });
      toast.success("Coupon created successfully");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to create coupon");
    },
  });
}

export function useUpdateCoupon() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCouponRequest }) =>
      updateCoupon(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUPONS_QUERY_KEY });
      toast.success("Coupon updated successfully");
    },
    onError: (error: Error) => {
      toast.error(error.message || "Failed to update coupon");
    },
  });
}

export function useDeleteCoupon() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCoupon(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUPONS_QUERY_KEY });
      toast.success("Coupon deleted");
    },
    onError: () => {
      toast.error("Failed to delete coupon");
    },
  });
}

export function useToggleCouponStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, isActive }: { id: string; isActive: boolean }) =>
      toggleCouponStatus(id, isActive),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: COUPONS_QUERY_KEY });
    },
    onError: () => {
      toast.error("Failed to update coupon status");
    },
  });
}

export function useValidateCoupon() {
  return useMutation({
    mutationFn: ({
      code,
      subtotal,
      userId,
    }: {
      code: string;
      subtotal: number;
      userId?: string;
    }) => validateCoupon(code, subtotal, userId),
  });
}

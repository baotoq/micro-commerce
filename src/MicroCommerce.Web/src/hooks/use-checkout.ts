"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  type SimulatePaymentRequest,
  type SubmitOrderRequest,
  getOrderById,
  simulatePayment,
  submitOrder,
} from "@/lib/api";

const CART_QUERY_KEY = ["cart"] as const;

export function useSubmitOrder() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: SubmitOrderRequest) => submitOrder(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: CART_QUERY_KEY });
    },
    onError: () => {
      toast.error("Failed to submit order");
    },
  });
}

export function useSimulatePayment() {
  return useMutation({
    mutationFn: ({
      orderId,
      data,
    }: {
      orderId: string;
      data: SimulatePaymentRequest;
    }) => simulatePayment(orderId, data),
  });
}

export function useOrder(orderId: string) {
  return useQuery({
    queryKey: ["order", orderId],
    queryFn: () => getOrderById(orderId),
    enabled: !!orderId,
  });
}

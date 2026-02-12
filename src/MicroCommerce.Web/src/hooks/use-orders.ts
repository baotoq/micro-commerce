"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";

import {
  getAllOrders,
  getOrderById,
  getOrderDashboard,
  getOrdersByBuyer,
  updateOrderStatus,
} from "@/lib/api";

const TERMINAL_STATUSES = ["Delivered", "Failed", "Cancelled"];

export function useOrdersByBuyer(params: {
  status?: string;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: ["orders", "buyer", params],
    queryFn: () => getOrdersByBuyer(params),
  });
}

export function useOrderWithPolling(orderId: string) {
  return useQuery({
    queryKey: ["order", orderId],
    queryFn: () => getOrderById(orderId),
    enabled: !!orderId,
    refetchInterval: (query) => {
      const status = query.state.data?.status;
      if (status && TERMINAL_STATUSES.includes(status)) {
        return false;
      }
      return 20_000;
    },
  });
}

export function useAllOrders(params: {
  status?: string;
  page?: number;
  pageSize?: number;
}) {
  return useQuery({
    queryKey: ["orders", "admin", params],
    queryFn: () => getAllOrders(params),
  });
}

export function useOrderDashboard(timeRange?: string) {
  return useQuery({
    queryKey: ["dashboard", timeRange],
    queryFn: () => getOrderDashboard(timeRange),
  });
}

export function useUpdateOrderStatus() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      orderId,
      newStatus,
    }: {
      orderId: string;
      newStatus: string;
    }) => updateOrderStatus(orderId, newStatus),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["orders", "admin"] });
      queryClient.invalidateQueries({ queryKey: ["dashboard"] });
    },
    onError: () => {
      toast.error("Failed to update order status");
    },
  });
}

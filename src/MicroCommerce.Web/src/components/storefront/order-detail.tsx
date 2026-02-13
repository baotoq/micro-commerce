"use client";

import Image from "next/image";
import { AlertCircle, MessageSquare } from "lucide-react";
import Link from "next/link";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { useOrderWithPolling } from "@/hooks/use-orders";
import { OrderStatusStepper } from "./order-status-stepper";

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

function formatDate(dateString: string): string {
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  }).format(new Date(dateString));
}

function getStatusBadgeClass(status: string): string {
  switch (status) {
    case "Submitted":
      return "bg-yellow-100 text-yellow-800";
    case "Confirmed":
      return "bg-blue-100 text-blue-800";
    case "Paid":
      return "bg-green-100 text-green-800";
    case "Shipped":
      return "bg-purple-100 text-purple-800";
    case "Delivered":
      return "bg-green-100 text-green-800";
    case "Failed":
      return "bg-red-100 text-red-800";
    case "Cancelled":
      return "bg-zinc-100 text-zinc-600";
    default:
      return "bg-zinc-100 text-zinc-600";
  }
}

interface OrderDetailProps {
  orderId: string;
}

export function OrderDetail({ orderId }: OrderDetailProps) {
  const { data: order, isLoading, isError } = useOrderWithPolling(orderId);

  if (isLoading) {
    return <OrderDetailSkeleton />;
  }

  if (isError || !order) {
    return (
      <div className="flex flex-col items-center justify-center py-24 text-center">
        <AlertCircle className="mb-4 size-12 text-zinc-300" />
        <h2 className="text-xl font-semibold text-zinc-900">
          Order not found
        </h2>
        <p className="mt-2 text-sm text-zinc-500">
          We couldn&apos;t find this order. It may have been removed or the link
          is incorrect.
        </p>
        <Button asChild className="mt-6 rounded-full" size="lg">
          <Link href="/orders">Back to Orders</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-wrap items-center gap-3">
        <h2 className="text-xl font-bold tracking-tight text-zinc-900">
          {order.orderNumber}
        </h2>
        <Badge className={`${getStatusBadgeClass(order.status)} border-0`}>
          {order.status}
        </Badge>
        <span className="text-sm text-zinc-500">
          {formatDate(order.createdAt)}
        </span>
      </div>

      {/* Status stepper */}
      <Card>
        <CardContent className="pt-6">
          <OrderStatusStepper
            status={order.status}
            failureReason={order.failureReason}
          />
        </CardContent>
      </Card>

      {/* Order Items */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Items</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {order.items.map((item) => {
            const canReview = ["Paid", "Confirmed", "Shipped", "Delivered"].includes(order.status);
            return (
              <div key={item.id} className="space-y-2">
                <div className="flex gap-4">
                  <div className="relative size-16 shrink-0 overflow-hidden rounded-md border border-zinc-200 bg-zinc-100">
                    {item.imageUrl ? (
                      <Image
                        src={item.imageUrl}
                        alt={item.productName}
                        fill
                        className="object-cover"
                        sizes="64px"
                      />
                    ) : (
                      <div className="flex size-full items-center justify-center text-xs text-zinc-400">
                        N/A
                      </div>
                    )}
                  </div>
                  <div className="flex flex-1 items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-zinc-900">
                        {item.productName}
                      </p>
                      <p className="text-sm text-zinc-500">
                        {item.quantity} x {formatPrice(item.unitPrice)}
                      </p>
                    </div>
                    <p className="shrink-0 text-sm font-medium text-zinc-900">
                      {formatPrice(item.lineTotal)}
                    </p>
                  </div>
                </div>
                {canReview && (
                  <Link
                    href={`/products/${item.productId}#reviews`}
                    className="flex items-center gap-1.5 text-sm text-blue-600 hover:underline"
                  >
                    <MessageSquare className="size-4" />
                    Write a Review
                  </Link>
                )}
              </div>
            );
          })}
        </CardContent>
      </Card>

      {/* Shipping Address */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Shipping Address</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-sm text-zinc-700">
            <p className="font-medium">{order.shippingAddress.name}</p>
            <p>{order.shippingAddress.street}</p>
            <p>
              {order.shippingAddress.city}, {order.shippingAddress.state}{" "}
              {order.shippingAddress.zipCode}
            </p>
            <p>{order.shippingAddress.email}</p>
          </div>
        </CardContent>
      </Card>

      {/* Order Summary */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Order Summary</CardTitle>
        </CardHeader>
        <CardContent className="space-y-3">
          <div className="flex justify-between text-sm">
            <span className="text-zinc-500">Subtotal</span>
            <span>{formatPrice(order.subtotal)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-zinc-500">Shipping</span>
            <span>{formatPrice(order.shippingCost)}</span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-zinc-500">Tax</span>
            <span>{formatPrice(order.tax)}</span>
          </div>

          <Separator />

          <div className="flex justify-between text-base font-semibold">
            <span>Total</span>
            <span>{formatPrice(order.total)}</span>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}

function OrderDetailSkeleton() {
  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Skeleton className="h-7 w-36" />
        <Skeleton className="h-6 w-20 rounded-full" />
        <Skeleton className="h-4 w-40" />
      </div>
      <Skeleton className="h-24 rounded-xl" />
      <Skeleton className="h-48 rounded-xl" />
      <Skeleton className="h-32 rounded-xl" />
      <Skeleton className="h-40 rounded-xl" />
    </div>
  );
}

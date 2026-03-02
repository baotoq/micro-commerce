"use client";

import { AlertCircle, MessageSquare } from "lucide-react";
import Image from "next/image";
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
    case "Confirmed":
    case "Processing":
      return "bg-info-bg text-info-foreground";
    case "Shipped":
    case "In Transit":
      return "bg-warning-bg text-warning-foreground";
    case "Paid":
    case "Delivered":
    case "Completed":
      return "bg-success-bg text-success-foreground";
    case "Failed":
    case "Cancelled":
      return "bg-error-bg text-error-foreground";
    default:
      return "bg-secondary text-secondary-foreground";
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
        <AlertCircle className="mb-4 size-12 text-muted-foreground/50" />
        <h2 className="text-xl font-semibold text-foreground">
          Order not found
        </h2>
        <p className="mt-2 text-sm text-muted-foreground">
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
        <h2 className="text-xl font-bold tracking-tight text-foreground">
          {order.orderNumber}
        </h2>
        <Badge
          className={`${getStatusBadgeClass(order.status)} border-0 text-xs font-semibold`}
        >
          {order.status}
        </Badge>
        <span className="text-sm text-muted-foreground">
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
          {order.items.map((item) => (
            <div key={item.id} className="flex gap-4">
              <div className="relative size-16 shrink-0 overflow-hidden rounded-md border border-border bg-muted">
                {item.imageUrl ? (
                  <Image
                    src={item.imageUrl}
                    alt={item.productName}
                    fill
                    className="object-cover"
                    sizes="64px"
                  />
                ) : (
                  <div className="flex size-full items-center justify-center text-xs text-muted-foreground">
                    N/A
                  </div>
                )}
              </div>
              <div className="flex flex-1 items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-foreground">
                    {item.productName}
                  </p>
                  <p className="text-sm text-muted-foreground">
                    {item.quantity} x {formatPrice(item.unitPrice)}
                  </p>
                </div>
                <p className="shrink-0 text-sm font-medium text-foreground">
                  {formatPrice(item.lineTotal)}
                </p>
              </div>
            </div>
          ))}
        </CardContent>
        {["Paid", "Confirmed", "Shipped", "Delivered"].includes(
          order.status,
        ) && (
          <div className="border-t border-border px-6 py-4">
            <Button asChild variant="outline" className="w-full rounded-full">
              <Link href={`/orders/${order.id}/review`}>
                <MessageSquare className="mr-2 size-4" />
                Review Products
              </Link>
            </Button>
          </div>
        )}
      </Card>

      {/* Shipping Address */}
      <Card>
        <CardHeader>
          <CardTitle className="text-base">Shipping Address</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="text-sm text-foreground">
            <p className="font-medium">{order.shippingAddress.name}</p>
            <p className="text-muted-foreground">
              {order.shippingAddress.street}
            </p>
            <p className="text-muted-foreground">
              {order.shippingAddress.city}, {order.shippingAddress.state}{" "}
              {order.shippingAddress.zipCode}
            </p>
            <p className="text-muted-foreground">
              {order.shippingAddress.email}
            </p>
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
            <span className="text-muted-foreground">Subtotal</span>
            <span className="text-foreground">
              {formatPrice(order.subtotal)}
            </span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Shipping</span>
            <span className="text-foreground">
              {formatPrice(order.shippingCost)}
            </span>
          </div>
          <div className="flex justify-between text-sm">
            <span className="text-muted-foreground">Tax</span>
            <span className="text-foreground">{formatPrice(order.tax)}</span>
          </div>

          <Separator />

          <div className="flex justify-between text-base font-semibold text-foreground">
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

"use client";

import { AlertCircle, Check } from "lucide-react";
import Image from "next/image";
import Link from "next/link";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { Skeleton } from "@/components/ui/skeleton";
import { useOrder } from "@/hooks/use-checkout";

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

function getStatusBadgeClasses(status: string): string {
  const normalized = status.toLowerCase();
  if (
    normalized === "paid" ||
    normalized === "completed" ||
    normalized === "confirmed"
  ) {
    return "bg-success-bg text-success-foreground border-transparent";
  }
  if (normalized === "failed" || normalized === "cancelled") {
    return "bg-error-bg text-error-foreground border-transparent";
  }
  // Default: processing / pending / submitted
  return "bg-warning-bg text-warning-foreground border-transparent";
}

function getStatusLabel(status: string): string {
  const normalized = status.toLowerCase();
  if (normalized === "paid") return "Paid";
  if (normalized === "completed") return "Completed";
  if (normalized === "confirmed") return "Confirmed";
  if (normalized === "failed") return "Failed";
  if (normalized === "cancelled") return "Cancelled";
  if (normalized === "submitted") return "Processing";
  return status.charAt(0).toUpperCase() + status.slice(1);
}

interface OrderConfirmationProps {
  orderId: string;
}

export function OrderConfirmation({ orderId }: OrderConfirmationProps) {
  const { data: order, isLoading, isError } = useOrder(orderId);

  if (isLoading) {
    return <OrderConfirmationSkeleton />;
  }

  if (isError || !order) {
    return (
      <div className="mx-auto max-w-2xl px-4 py-16 sm:px-6 lg:px-8">
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <AlertCircle className="mb-4 size-12 text-muted-foreground" />
          <h2 className="text-xl font-semibold text-foreground">
            Order not found
          </h2>
          <p className="mt-2 text-sm text-muted-foreground">
            We couldn&apos;t find this order. It may have been removed or the
            link is incorrect.
          </p>
          <Button asChild className="mt-6" size="lg">
            <Link href="/">Back to Shop</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-2xl flex-col items-center gap-8 px-4 py-16 sm:px-6 lg:px-20">
      {/* Success Banner */}
      <div className="flex flex-col items-center gap-4 text-center">
        <div className="flex size-[72px] items-center justify-center rounded-full bg-success-bg">
          <Check className="size-9 text-success" strokeWidth={2.5} />
        </div>
        <h1 className="text-[28px] font-bold tracking-tight text-foreground">
          Order Confirmed!
        </h1>
        <p className="max-w-[500px] text-[15px] leading-relaxed text-muted-foreground">
          Thank you for your purchase. Your order {order.orderNumber} has been
          placed successfully.
        </p>
      </div>

      {/* Order Summary Card */}
      <Card className="w-full max-w-[600px]">
        <CardContent className="space-y-4 p-6">
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Order Number</span>
            <span className="text-sm font-semibold text-foreground">
              {order.orderNumber}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Date</span>
            <span className="text-sm font-medium text-foreground">
              {formatDate(order.createdAt)}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Total</span>
            <span className="text-sm font-bold text-foreground">
              {formatPrice(order.total)}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span className="text-sm text-muted-foreground">Status</span>
            <Badge
              variant="outline"
              className={getStatusBadgeClasses(order.status)}
            >
              {getStatusLabel(order.status)}
            </Badge>
          </div>
        </CardContent>
      </Card>

      {/* Order Items Card */}
      <Card className="w-full max-w-[600px]">
        <CardHeader className="pb-3">
          <CardTitle className="text-base">Order Items</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {order.items.map((item) => (
            <div key={item.id} className="flex gap-4">
              <div className="relative size-14 shrink-0 overflow-hidden rounded-md border border-border bg-muted">
                {item.imageUrl ? (
                  <Image
                    src={item.imageUrl}
                    alt={item.productName}
                    fill
                    className="object-cover"
                    sizes="56px"
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
                    Qty: {item.quantity} x {formatPrice(item.unitPrice)}
                  </p>
                </div>
                <p className="shrink-0 text-sm font-medium text-foreground">
                  {formatPrice(item.lineTotal)}
                </p>
              </div>
            </div>
          ))}

          <Separator />

          {/* Summary Totals */}
          <div className="space-y-2">
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
            <div className="flex justify-between text-base font-semibold">
              <span className="text-foreground">Total</span>
              <span className="text-foreground">
                {formatPrice(order.total)}
              </span>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Shipping Address Card */}
      <Card className="w-full max-w-[600px]">
        <CardHeader className="pb-3">
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

      {/* Action Buttons */}
      <div className="flex items-center gap-3">
        <Button asChild>
          <Link href={`/orders/${orderId}`}>View Order Details</Link>
        </Button>
        <Button asChild variant="outline">
          <Link href="/">Continue Shopping</Link>
        </Button>
      </div>
    </div>
  );
}

function OrderConfirmationSkeleton() {
  return (
    <div className="mx-auto flex max-w-2xl flex-col items-center gap-8 px-4 py-16 sm:px-6 lg:px-20">
      <div className="flex flex-col items-center gap-4">
        <Skeleton className="size-[72px] rounded-full" />
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-5 w-80" />
      </div>
      <Skeleton className="h-48 w-full max-w-[600px] rounded-lg" />
      <Skeleton className="h-64 w-full max-w-[600px] rounded-lg" />
      <Skeleton className="h-32 w-full max-w-[600px] rounded-lg" />
      <div className="flex gap-3">
        <Skeleton className="h-10 w-40 rounded-md" />
        <Skeleton className="h-10 w-40 rounded-md" />
      </div>
    </div>
  );
}

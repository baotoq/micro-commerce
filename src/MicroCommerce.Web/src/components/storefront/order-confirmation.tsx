"use client";

import Image from "next/image";
import Link from "next/link";
import { CheckCircle2, AlertCircle } from "lucide-react";

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
          <AlertCircle className="mb-4 size-12 text-zinc-300" />
          <h2 className="text-xl font-semibold text-zinc-900">Order not found</h2>
          <p className="mt-2 text-sm text-zinc-500">
            We couldn&apos;t find this order. It may have been removed or the link is
            incorrect.
          </p>
          <Button asChild className="mt-6 rounded-full" size="lg">
            <Link href="/">Back to Shop</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl px-4 py-12 sm:px-6 lg:px-8">
      {/* Header */}
      <div className="mb-8 text-center">
        <CheckCircle2 className="mx-auto mb-4 size-16 text-green-500" />
        <h1 className="text-2xl font-bold tracking-tight text-zinc-900">
          Order Confirmed!
        </h1>
        <p className="mt-2 text-lg font-medium text-zinc-700">
          {order.orderNumber}
        </p>
        <p className="mt-1 text-sm text-zinc-500">
          A confirmation will be sent to{" "}
          <span className="font-medium">{order.buyerEmail}</span>
        </p>
      </div>

      {/* Order Items */}
      <Card className="mb-6">
        <CardHeader>
          <CardTitle className="text-base">Order Items</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          {order.items.map((item) => (
            <div key={item.id} className="flex gap-4">
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
                    Qty: {item.quantity} x {formatPrice(item.unitPrice)}
                  </p>
                </div>
                <p className="shrink-0 text-sm font-medium text-zinc-900">
                  {formatPrice(item.lineTotal)}
                </p>
              </div>
            </div>
          ))}
        </CardContent>
      </Card>

      {/* Shipping Address */}
      <Card className="mb-6">
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
      <Card className="mb-8">
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

      {/* Continue Shopping */}
      <div className="text-center">
        <Button asChild className="rounded-full" size="lg">
          <Link href="/">Continue Shopping</Link>
        </Button>
      </div>
    </div>
  );
}

function OrderConfirmationSkeleton() {
  return (
    <div className="mx-auto max-w-2xl px-4 py-12 sm:px-6 lg:px-8">
      <div className="mb-8 flex flex-col items-center">
        <Skeleton className="mb-4 size-16 rounded-full" />
        <Skeleton className="mb-2 h-8 w-48" />
        <Skeleton className="mb-1 h-5 w-32" />
        <Skeleton className="h-4 w-56" />
      </div>
      <Skeleton className="mb-6 h-48 rounded-lg" />
      <Skeleton className="mb-6 h-32 rounded-lg" />
      <Skeleton className="mb-8 h-40 rounded-lg" />
      <div className="flex justify-center">
        <Skeleton className="h-10 w-40 rounded-full" />
      </div>
    </div>
  );
}

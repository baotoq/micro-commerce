"use client";

import { use } from "react";
import Link from "next/link";
import Image from "next/image";
import {
  ChevronRight,
  Loader2,
  Package,
  Truck,
  CheckCircle,
} from "lucide-react";
import { toast } from "sonner";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Separator } from "@/components/ui/separator";
import { OrderStatusStepper } from "@/components/storefront/order-status-stepper";
import { useOrderWithPolling, useUpdateOrderStatus } from "@/hooks/use-orders";

const STATUS_BADGE_STYLES: Record<string, string> = {
  Submitted: "bg-yellow-100 text-yellow-800 border-yellow-200",
  Confirmed: "bg-blue-100 text-blue-800 border-blue-200",
  Paid: "bg-green-100 text-green-800 border-green-200",
  Shipped: "bg-purple-100 text-purple-800 border-purple-200",
  Delivered: "bg-green-100 text-green-800 border-green-200",
  Failed: "bg-red-100 text-red-800 border-red-200",
  Cancelled: "bg-gray-100 text-gray-800 border-gray-200",
};

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function formatFullDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

function DetailSkeleton() {
  return (
    <div className="space-y-6">
      {/* Breadcrumb skeleton */}
      <Skeleton className="h-4 w-48" />

      {/* Header skeleton */}
      <div className="flex items-center gap-4">
        <Skeleton className="h-8 w-48" />
        <Skeleton className="h-6 w-20 rounded-full" />
      </div>

      {/* Status stepper skeleton */}
      <Skeleton className="h-16 w-full" />

      {/* Content skeleton */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        <div className="lg:col-span-2">
          <Skeleton className="h-[300px] w-full rounded-lg" />
        </div>
        <div className="space-y-4">
          <Skeleton className="h-[200px] w-full rounded-lg" />
          <Skeleton className="h-[150px] w-full rounded-lg" />
        </div>
      </div>
    </div>
  );
}

export default function AdminOrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = use(params);
  const { data: order, isLoading, isError } = useOrderWithPolling(id);
  const updateOrderStatus = useUpdateOrderStatus();

  function handleStatusUpdate(newStatus: string) {
    updateOrderStatus.mutate(
      { orderId: id, newStatus },
      {
        onSuccess: () => {
          toast.success(`Order marked as ${newStatus}`);
        },
      }
    );
  }

  if (isLoading) {
    return <DetailSkeleton />;
  }

  if (isError || !order) {
    return (
      <div className="space-y-4">
        <nav className="flex items-center gap-1 text-sm text-gray-500">
          <Link
            href="/admin/orders"
            className="hover:text-gray-900 hover:underline"
          >
            Orders
          </Link>
          <ChevronRight className="h-3.5 w-3.5" />
          <span className="text-gray-900">Not Found</span>
        </nav>
        <div className="flex flex-col items-center justify-center py-16">
          <Package className="mb-4 h-12 w-12 text-gray-300" />
          <h2 className="text-lg font-semibold text-gray-900">
            Order not found
          </h2>
          <p className="mt-1 text-sm text-gray-500">
            The order you are looking for does not exist or has been removed.
          </p>
          <Link href="/admin/orders">
            <Button variant="outline" className="mt-4">
              Back to Kanban Board
            </Button>
          </Link>
        </div>
      </div>
    );
  }

  const canMarkShipped = order.status === "Confirmed";
  const canMarkDelivered = order.status === "Shipped";

  return (
    <div className="space-y-6">
      {/* Breadcrumb */}
      <nav className="flex items-center gap-1 text-sm text-gray-500">
        <Link
          href="/admin/orders"
          className="hover:text-gray-900 hover:underline"
        >
          Orders
        </Link>
        <ChevronRight className="h-3.5 w-3.5" />
        <span className="text-gray-900">Order #{order.orderNumber}</span>
      </nav>

      {/* Header */}
      <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
        <div className="flex items-center gap-3">
          <h1 className="text-2xl font-bold text-gray-900">
            Order #{order.orderNumber}
          </h1>
          <Badge
            variant="outline"
            className={
              STATUS_BADGE_STYLES[order.status] ??
              "bg-gray-100 text-gray-800 border-gray-200"
            }
          >
            {order.status}
          </Badge>
        </div>

        <div className="flex items-center gap-2">
          {canMarkShipped && (
            <Button
              onClick={() => handleStatusUpdate("Shipped")}
              disabled={updateOrderStatus.isPending}
            >
              {updateOrderStatus.isPending ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Truck className="mr-2 h-4 w-4" />
              )}
              Mark as Shipped
            </Button>
          )}
          {canMarkDelivered && (
            <Button
              onClick={() => handleStatusUpdate("Delivered")}
              disabled={updateOrderStatus.isPending}
            >
              {updateOrderStatus.isPending ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <CheckCircle className="mr-2 h-4 w-4" />
              )}
              Mark as Delivered
            </Button>
          )}
        </div>
      </div>

      {/* Date */}
      <p className="text-sm text-gray-500">
        Placed on {formatFullDate(order.createdAt)}
        {order.paidAt && <> &middot; Paid on {formatFullDate(order.paidAt)}</>}
      </p>

      {/* Status Stepper */}
      <Card>
        <CardContent className="pt-6">
          <OrderStatusStepper
            status={order.status}
            failureReason={order.failureReason}
          />
        </CardContent>
      </Card>

      {/* Main content: two-column layout */}
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Left: Items table */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Order Items</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[60px]">Image</TableHead>
                  <TableHead>Product</TableHead>
                  <TableHead className="text-right">Qty</TableHead>
                  <TableHead className="text-right">Unit Price</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {order.items.map((item) => (
                  <TableRow key={item.id}>
                    <TableCell>
                      {item.imageUrl ? (
                        <Image
                          src={item.imageUrl}
                          alt={item.productName}
                          width={40}
                          height={40}
                          className="rounded object-cover"
                        />
                      ) : (
                        <div className="flex h-10 w-10 items-center justify-center rounded bg-gray-100">
                          <Package className="h-5 w-5 text-gray-400" />
                        </div>
                      )}
                    </TableCell>
                    <TableCell className="font-medium">
                      {item.productName}
                    </TableCell>
                    <TableCell className="text-right">
                      {item.quantity}
                    </TableCell>
                    <TableCell className="text-right">
                      {formatCurrency(item.unitPrice)}
                    </TableCell>
                    <TableCell className="text-right font-medium">
                      {formatCurrency(item.lineTotal)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        {/* Right: Summary + Shipping */}
        <div className="space-y-6">
          {/* Order Summary */}
          <Card>
            <CardHeader>
              <CardTitle>Order Summary</CardTitle>
            </CardHeader>
            <CardContent className="space-y-3">
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Subtotal</span>
                <span>{formatCurrency(order.subtotal)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Shipping</span>
                <span>{formatCurrency(order.shippingCost)}</span>
              </div>
              <div className="flex justify-between text-sm">
                <span className="text-gray-500">Tax</span>
                <span>{formatCurrency(order.tax)}</span>
              </div>
              <Separator />
              <div className="flex justify-between font-semibold">
                <span>Total</span>
                <span>{formatCurrency(order.total)}</span>
              </div>
            </CardContent>
          </Card>

          {/* Shipping Address */}
          <Card>
            <CardHeader>
              <CardTitle>Shipping Address</CardTitle>
            </CardHeader>
            <CardContent className="space-y-1 text-sm">
              <p className="font-medium">{order.shippingAddress.name}</p>
              <p className="text-gray-500">{order.shippingAddress.street}</p>
              <p className="text-gray-500">
                {order.shippingAddress.city}, {order.shippingAddress.state}{" "}
                {order.shippingAddress.zipCode}
              </p>
            </CardContent>
          </Card>

          {/* Buyer Info */}
          <Card>
            <CardHeader>
              <CardTitle>Buyer</CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-gray-700">{order.buyerEmail}</p>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}

"use client";

import Image from "next/image";
import Link from "next/link";
import { AlertCircle, ArrowLeft } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useOrderWithPolling } from "@/hooks/use-orders";
import { useMyReview, useCanReview } from "@/hooks/use-reviews";
import { ReviewFormDialog } from "@/components/reviews/review-form-dialog";
import { StarRatingDisplay } from "@/components/reviews/star-rating-display";

interface OrderReviewPageProps {
  orderId: string;
}

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

interface OrderItemReviewRowProps {
  productId: string;
  productName: string;
  imageUrl: string | null;
  quantity: number;
  unitPrice: number;
}

function OrderItemReviewRow({
  productId,
  productName,
  imageUrl,
  quantity,
  unitPrice,
}: OrderItemReviewRowProps) {
  const { data: myReview, isLoading: reviewLoading } = useMyReview(productId);
  const { data: canReviewData, isLoading: canReviewLoading } = useCanReview(productId);

  const isLoading = reviewLoading || canReviewLoading;

  if (isLoading) {
    return (
      <div className="flex gap-4 border-b border-zinc-200 pb-6">
        <Skeleton className="size-16 shrink-0 rounded-md" />
        <div className="flex flex-1 flex-col gap-2">
          <Skeleton className="h-5 w-48" />
          <Skeleton className="h-4 w-32" />
          <Skeleton className="h-9 w-36 rounded-full" />
        </div>
      </div>
    );
  }

  const hasReview = !!myReview;
  const canReview = canReviewData?.hasPurchased && !canReviewData?.hasReviewed;

  return (
    <div className="flex gap-4 border-b border-zinc-200 pb-6">
      <div className="relative size-16 shrink-0 overflow-hidden rounded-md border border-zinc-200 bg-zinc-100">
        {imageUrl ? (
          <Image
            src={imageUrl}
            alt={productName}
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
      <div className="flex flex-1 flex-col gap-2">
        <div>
          <p className="text-sm font-medium text-zinc-900">{productName}</p>
          <p className="text-sm text-zinc-500">
            {quantity} x {formatPrice(unitPrice)}
          </p>
        </div>
        {hasReview ? (
          <div className="flex items-center gap-3">
            <StarRatingDisplay rating={myReview.rating} size="sm" />
            <ReviewFormDialog
              productId={productId}
              existingReview={myReview}
              trigger={
                <Button
                  variant="outline"
                  size="sm"
                  className="h-9 rounded-full text-xs"
                >
                  Edit Review
                </Button>
              }
            />
          </div>
        ) : canReview ? (
          <ReviewFormDialog
            productId={productId}
            trigger={
              <Button
                variant="outline"
                size="sm"
                className="h-9 w-fit rounded-full text-xs"
              >
                Write a Review
              </Button>
            }
          />
        ) : (
          <p className="text-xs text-zinc-500">
            Purchase verification required to review
          </p>
        )}
      </div>
    </div>
  );
}

export function OrderReviewPage({ orderId }: OrderReviewPageProps) {
  const { data: order, isLoading, isError } = useOrderWithPolling(orderId);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <div className="space-y-2">
          <Skeleton className="h-8 w-48" />
          <Skeleton className="h-5 w-56" />
        </div>
        <Skeleton className="h-10 w-32" />
        <div className="space-y-6">
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
          <Skeleton className="h-24 w-full" />
        </div>
      </div>
    );
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
      <div className="space-y-2">
        <h1 className="text-2xl font-bold tracking-tight text-zinc-900">
          Review Products
        </h1>
        <p className="text-sm text-zinc-500">{order.orderNumber}</p>
      </div>

      {/* Back link */}
      <Link
        href={`/orders/${orderId}`}
        className="inline-flex items-center gap-2 text-sm text-zinc-600 hover:text-zinc-900"
      >
        <ArrowLeft className="size-4" />
        Back to order
      </Link>

      {/* Order Items with Review Status */}
      <div className="space-y-6 pt-2">
        {order.items.map((item) => (
          <OrderItemReviewRow
            key={item.id}
            productId={item.productId}
            productName={item.productName}
            imageUrl={item.imageUrl}
            quantity={item.quantity}
            unitPrice={item.unitPrice}
          />
        ))}
      </div>
    </div>
  );
}

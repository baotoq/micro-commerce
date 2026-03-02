"use client";

import { useSession } from "next-auth/react";
import { useEffect, useState } from "react";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import {
  useCanReview,
  useDeleteReview,
  useMyReview,
  useProductReviews,
} from "@/hooks/use-reviews";
import type { ReviewDto } from "@/lib/api";
import { ReviewFormDialog } from "./review-form-dialog";
import { ReviewItem } from "./review-item";
import { StarRatingDisplay } from "./star-rating-display";
import { VerifiedBadge } from "./verified-badge";

interface ReviewListProps {
  productId: string;
  averageRating: number | null;
  reviewCount: number;
}

function getInitials(name: string): string {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
}

export function ReviewList({
  productId,
  averageRating,
  reviewCount,
}: ReviewListProps) {
  const { data: session } = useSession();
  const [page, setPage] = useState(1);
  const [allReviews, setAllReviews] = useState<ReviewDto[]>([]);

  const { data: reviewsData, isLoading } = useProductReviews(productId, page);
  useMyReview(productId);
  const { data: canReviewData } = useCanReview(productId);
  const deleteReview = useDeleteReview(productId);

  // Accumulate reviews across pages
  useEffect(() => {
    if (reviewsData) {
      if (page === 1) {
        setAllReviews(reviewsData.items);
      } else {
        setAllReviews((prev) => [...prev, ...reviewsData.items]);
      }
    }
  }, [reviewsData, page]);

  const handleLoadMore = () => {
    setPage((prev) => prev + 1);
  };

  const handleDelete = (reviewId: string) => {
    if (confirm("Are you sure you want to delete your review?")) {
      deleteReview.mutate(reviewId);
    }
  };

  const hasMore = reviewsData
    ? allReviews.length < reviewsData.totalCount
    : false;

  return (
    <div className="space-y-6">
      {/* Rating summary */}
      {reviewCount > 0 && averageRating !== null && (
        <div className="flex items-center gap-4 rounded-lg border border-border bg-card p-4">
          <div className="flex flex-col items-center gap-1">
            <span className="text-3xl font-semibold text-foreground">
              {averageRating.toFixed(1)}
            </span>
            <StarRatingDisplay rating={averageRating} size="sm" />
            <span className="text-xs text-muted-foreground">
              {reviewCount} {reviewCount === 1 ? "review" : "reviews"}
            </span>
          </div>
        </div>
      )}

      {/* Write a review button */}
      {session && canReviewData && (
        <div>
          {canReviewData.hasPurchased && !canReviewData.hasReviewed ? (
            <ReviewFormDialog
              productId={productId}
              trigger={
                <Button size="sm" variant="outline">
                  Write a Review
                </Button>
              }
              onSuccess={() => {
                // Reset to page 1 to see new review
                setPage(1);
              }}
            />
          ) : !canReviewData.hasPurchased ? (
            <p className="text-sm text-muted-foreground">
              Purchase this product to leave a review
            </p>
          ) : null}
        </div>
      )}

      {/* Reviews list */}
      {isLoading && page === 1 ? (
        <ReviewListSkeleton />
      ) : allReviews.length === 0 ? (
        <div className="py-8 text-center">
          <p className="text-sm text-muted-foreground">
            No reviews yet. Be the first to review this product.
          </p>
        </div>
      ) : (
        <div className="space-y-0">
          {allReviews.map((review) => {
            const isOwner = session?.user?.id === review.userId;
            return isOwner ? (
              <div key={review.id}>
                <div className="border-b border-border py-5 first:pt-0 last:border-0">
                  <div className="flex items-start gap-3">
                    <Avatar className="size-9 shrink-0">
                      <AvatarFallback className="bg-muted text-xs font-medium text-foreground">
                        {getInitials(review.displayName)}
                      </AvatarFallback>
                    </Avatar>

                    <div className="flex-1">
                      <div className="flex items-start justify-between">
                        <div>
                          <div className="flex items-center gap-2">
                            <span className="font-medium text-foreground">
                              {review.displayName}
                            </span>
                            {review.isVerifiedPurchase && <VerifiedBadge />}
                          </div>
                          <div className="mt-1 flex items-center gap-2">
                            <StarRatingDisplay
                              rating={review.rating}
                              size="sm"
                            />
                            <span className="text-xs text-muted-foreground">
                              {new Intl.DateTimeFormat("en-US", {
                                year: "numeric",
                                month: "long",
                                day: "numeric",
                              }).format(new Date(review.createdAt))}
                            </span>
                          </div>
                        </div>
                        <div className="flex items-center gap-3">
                          <ReviewFormDialog
                            productId={productId}
                            existingReview={review}
                            trigger={
                              <button
                                type="button"
                                className="text-sm text-primary hover:underline"
                              >
                                Edit
                              </button>
                            }
                            onSuccess={() => {
                              setPage(1);
                            }}
                          />
                          <button
                            type="button"
                            onClick={() => handleDelete(review.id)}
                            className="text-sm text-destructive hover:underline"
                          >
                            Delete
                          </button>
                        </div>
                      </div>
                      <p className="mt-3 whitespace-pre-line text-sm leading-relaxed text-muted-foreground">
                        {review.text}
                      </p>
                    </div>
                  </div>
                </div>
              </div>
            ) : (
              <ReviewItem key={review.id} review={review} isOwner={false} />
            );
          })}
        </div>
      )}

      {/* Load more button */}
      {hasMore && (
        <div className="flex justify-center pt-2">
          <Button
            variant="outline"
            onClick={handleLoadMore}
            disabled={isLoading}
          >
            {isLoading ? "Loading..." : "Load More"}
          </Button>
        </div>
      )}
    </div>
  );
}

function ReviewListSkeleton() {
  return (
    <div className="space-y-4">
      {[1, 2, 3].map((i) => (
        <div key={i} className="border-b border-border py-5">
          <div className="flex items-start gap-3">
            <Skeleton className="size-9 rounded-full" />
            <div className="flex-1">
              <div className="flex items-center gap-2">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="h-4 w-32" />
              </div>
              <Skeleton className="mt-2 h-4 w-32" />
              <Skeleton className="mt-3 h-16 w-full" />
            </div>
          </div>
        </div>
      ))}
    </div>
  );
}

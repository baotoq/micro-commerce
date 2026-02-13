"use client";

import { StarRatingDisplay } from "./star-rating-display";
import { VerifiedBadge } from "./verified-badge";
import type { ReviewDto } from "@/lib/api";

interface ReviewItemProps {
  review: ReviewDto;
  isOwner?: boolean;
  onEdit?: () => void;
  onDelete?: () => void;
}

function formatDate(dateString: string): string {
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  }).format(new Date(dateString));
}

export function ReviewItem({ review, isOwner = false, onEdit, onDelete }: ReviewItemProps) {
  return (
    <div className="border-b border-zinc-200 py-4 first:pt-0 last:border-0">
      <div className="flex items-start justify-between">
        <div className="flex-1">
          <div className="flex items-center gap-2">
            <span className="font-medium text-zinc-900">{review.displayName}</span>
            {review.isVerifiedPurchase && <VerifiedBadge />}
          </div>
          <div className="mt-1 flex items-center gap-2">
            <StarRatingDisplay rating={review.rating} size="sm" />
            <span className="text-xs text-zinc-500">{formatDate(review.createdAt)}</span>
          </div>
        </div>
        {isOwner && (
          <div className="flex items-center gap-3">
            {onEdit && (
              <button
                type="button"
                onClick={onEdit}
                className="text-sm text-blue-600 hover:underline"
              >
                Edit
              </button>
            )}
            {onDelete && (
              <button
                type="button"
                onClick={onDelete}
                className="text-sm text-red-600 hover:underline"
              >
                Delete
              </button>
            )}
          </div>
        )}
      </div>
      <p className="mt-3 whitespace-pre-line text-sm text-zinc-700">{review.text}</p>
    </div>
  );
}

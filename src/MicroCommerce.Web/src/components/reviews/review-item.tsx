"use client";

import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import type { ReviewDto } from "@/lib/api";
import { StarRatingDisplay } from "./star-rating-display";
import { VerifiedBadge } from "./verified-badge";

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

function getInitials(name: string): string {
  return name
    .split(" ")
    .map((part) => part[0])
    .join("")
    .toUpperCase()
    .slice(0, 2);
}

export function ReviewItem({
  review,
  isOwner = false,
  onEdit,
  onDelete,
}: ReviewItemProps) {
  return (
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
                <StarRatingDisplay rating={review.rating} size="sm" />
                <span className="text-xs text-muted-foreground">
                  {formatDate(review.createdAt)}
                </span>
              </div>
            </div>
            {isOwner && (
              <div className="flex items-center gap-3">
                {onEdit && (
                  <button
                    type="button"
                    onClick={onEdit}
                    className="text-sm text-primary hover:underline"
                  >
                    Edit
                  </button>
                )}
                {onDelete && (
                  <button
                    type="button"
                    onClick={onDelete}
                    className="text-sm text-destructive hover:underline"
                  >
                    Delete
                  </button>
                )}
              </div>
            )}
          </div>
          <p className="mt-3 whitespace-pre-line text-sm leading-relaxed text-muted-foreground">
            {review.text}
          </p>
        </div>
      </div>
    </div>
  );
}

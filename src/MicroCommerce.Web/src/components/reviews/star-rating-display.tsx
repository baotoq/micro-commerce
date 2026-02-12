"use client";

import { Star } from "lucide-react";

interface StarRatingDisplayProps {
  rating: number;
  count?: number;
  showCount?: boolean;
  size?: "sm" | "md";
}

export function StarRatingDisplay({ rating, count, showCount = false, size = "md" }: StarRatingDisplayProps) {
  const stars = [];
  const fullStars = Math.floor(rating);
  const hasHalfStar = rating % 1 >= 0.5;
  const iconSize = size === "sm" ? "size-3.5" : "size-4";

  for (let i = 1; i <= 5; i++) {
    if (i <= fullStars) {
      // Full star
      stars.push(
        <Star key={i} className={`${iconSize} fill-yellow-400 text-yellow-400`} />
      );
    } else if (i === fullStars + 1 && hasHalfStar) {
      // Half star (approximated with lighter fill)
      stars.push(
        <Star key={i} className={`${iconSize} fill-yellow-200 text-yellow-400`} />
      );
    } else {
      // Empty star
      stars.push(
        <Star key={i} className={`${iconSize} text-zinc-300`} />
      );
    }
  }

  return (
    <div className="flex items-center gap-1.5">
      <div className="flex items-center gap-0.5">
        {stars}
      </div>
      {showCount && count !== undefined && (
        <span className="text-sm text-zinc-600">
          {rating.toFixed(1)} ({count} {count === 1 ? "review" : "reviews"})
        </span>
      )}
    </div>
  );
}

"use client";

import { useState } from "react";
import { Star } from "lucide-react";

interface StarRatingInputProps {
  value: number;
  onChange: (rating: number) => void;
  max?: number;
}

export function StarRatingInput({ value, onChange, max = 5 }: StarRatingInputProps) {
  const [hoverValue, setHoverValue] = useState<number | null>(null);

  const displayValue = hoverValue !== null ? hoverValue : value;

  return (
    <div className="flex flex-col gap-2">
      <div
        className="flex items-center gap-1"
        onMouseLeave={() => setHoverValue(null)}
      >
        {Array.from({ length: max }, (_, i) => i + 1).map((star) => (
          <button
            key={star}
            type="button"
            aria-label={`Rate ${star} out of ${max} stars`}
            className="focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-zinc-400 rounded"
            onMouseEnter={() => setHoverValue(star)}
            onClick={() => onChange(star)}
          >
            <Star
              className={`size-8 transition-colors ${
                star <= displayValue
                  ? "fill-yellow-400 text-yellow-400"
                  : "text-zinc-300"
              }`}
            />
          </button>
        ))}
      </div>
      <input type="hidden" name="rating" value={value} />
    </div>
  );
}

"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { StarRatingInput } from "./star-rating-input";
import { useCreateReview, useUpdateReview } from "@/hooks/use-reviews";
import type { ReviewDto } from "@/lib/api";

interface ReviewFormDialogProps {
  productId: string;
  existingReview?: ReviewDto;
  trigger: React.ReactNode;
  onSuccess?: () => void;
}

export function ReviewFormDialog({ productId, existingReview, trigger, onSuccess }: ReviewFormDialogProps) {
  const [open, setOpen] = useState(false);
  const [rating, setRating] = useState(existingReview?.rating || 0);
  const [text, setText] = useState(existingReview?.text || "");
  const [errors, setErrors] = useState<Record<string, string>>({});

  const createReview = useCreateReview(productId);
  const updateReview = useUpdateReview(productId);

  const isEdit = !!existingReview;

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (rating === 0) {
      newErrors.rating = "Please select a rating";
    }

    if (text.trim().length < 10) {
      newErrors.text = "Review must be at least 10 characters";
    }

    if (text.trim().length > 1000) {
      newErrors.text = "Review must not exceed 1000 characters";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    const data = { rating, text: text.trim() };

    if (isEdit) {
      updateReview.mutate(
        { reviewId: existingReview.id, data },
        {
          onSuccess: () => {
            setOpen(false);
            onSuccess?.();
            setErrors({});
          },
        }
      );
    } else {
      createReview.mutate(data, {
        onSuccess: () => {
          setOpen(false);
          onSuccess?.();
          // Reset form
          setRating(0);
          setText("");
          setErrors({});
        },
      });
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    setOpen(newOpen);
    if (!newOpen) {
      // Reset form when closing
      setRating(existingReview?.rating || 0);
      setText(existingReview?.text || "");
      setErrors({});
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <div onClick={() => setOpen(true)}>{trigger}</div>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Review" : "Write a Review"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label>Rating</Label>
            <StarRatingInput value={rating} onChange={setRating} />
            {errors.rating && <p className="text-sm text-red-500">{errors.rating}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="review-text">Your Review</Label>
            <Textarea
              id="review-text"
              placeholder="Share your thoughts about this product..."
              value={text}
              onChange={(e) => setText(e.target.value)}
              rows={6}
              className="resize-none"
            />
            <div className="flex items-center justify-between">
              <span className="text-xs text-zinc-500">
                {text.length}/1000 characters
              </span>
              {errors.text && <p className="text-sm text-red-500">{errors.text}</p>}
            </div>
          </div>

          <div className="flex gap-2 pt-4">
            <Button
              type="submit"
              disabled={createReview.isPending || updateReview.isPending}
              className="flex-1"
            >
              {createReview.isPending || updateReview.isPending
                ? "Saving..."
                : isEdit
                  ? "Update Review"
                  : "Submit Review"}
            </Button>
            <Button type="button" variant="outline" onClick={() => setOpen(false)}>
              Cancel
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

"use client";

import { Filter, Star, X } from "lucide-react";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
} from "@/components/ui/sheet";
import { Slider } from "@/components/ui/slider";
import type { CategoryDto } from "@/lib/api";
import { getCategories } from "@/lib/api";

const RATING_OPTIONS = [
  { value: "4", label: "4 & up", stars: 4 },
  { value: "3", label: "3 & up", stars: 3 },
  { value: "2", label: "2 & up", stars: 2 },
  { value: "1", label: "1 & up", stars: 1 },
] as const;

const MAX_PRICE = 500;

function StarRatingRow({
  filledStars,
  label,
}: {
  filledStars: number;
  label: string;
}) {
  return (
    <span className="inline-flex items-center gap-0.5">
      {[0, 1, 2, 3, 4].map((starIndex) => (
        <Star
          key={`star-${starIndex}`}
          className={`size-3.5 ${
            starIndex < filledStars
              ? "fill-star text-star"
              : "fill-border text-border"
          }`}
        />
      ))}
      <span className="ml-1 text-xs text-muted-foreground">{label}</span>
    </span>
  );
}

function FilterContent({
  categories,
  activeCategories,
  priceRange,
  activeRating,
  onCategoryToggle,
  onPriceChange,
  onRatingChange,
  onReset,
  hasActiveFilters,
}: {
  categories: CategoryDto[];
  activeCategories: string[];
  priceRange: [number, number];
  activeRating: string;
  onCategoryToggle: (categoryId: string) => void;
  onPriceChange: (value: number[]) => void;
  onRatingChange: (value: string) => void;
  onReset: () => void;
  hasActiveFilters: boolean;
}) {
  return (
    <div className="flex flex-col gap-6">
      <h2 className="text-lg font-bold text-foreground">Filters</h2>

      {/* Category Section */}
      <div className="flex flex-col gap-3">
        <h3 className="text-sm font-semibold text-foreground">Category</h3>
        <div className="flex flex-col gap-2.5">
          {categories.map((category) => {
            const isChecked = activeCategories.includes(category.id);
            return (
              <div key={category.id} className="flex items-center gap-2.5">
                <Checkbox
                  checked={isChecked}
                  onCheckedChange={() => onCategoryToggle(category.id)}
                  id={`cat-${category.id}`}
                  aria-label={`Filter by ${category.name}`}
                />
                <span className="text-sm text-foreground">{category.name}</span>
              </div>
            );
          })}
        </div>
      </div>

      <div className="border-b border-border" />

      {/* Price Range Section */}
      <div className="flex flex-col gap-3">
        <h3 className="text-sm font-semibold text-foreground">Price Range</h3>
        <Slider
          value={priceRange}
          min={0}
          max={MAX_PRICE}
          step={10}
          onValueChange={onPriceChange}
          aria-label="Price range"
        />
        <div className="flex items-center gap-2">
          <div className="flex h-9 flex-1 items-center rounded-md border border-border px-3">
            <span className="text-sm text-muted-foreground">
              ${priceRange[0]}
            </span>
          </div>
          <span className="text-sm text-muted-foreground">&mdash;</span>
          <div className="flex h-9 flex-1 items-center rounded-md border border-border px-3">
            <span className="text-sm text-muted-foreground">
              ${priceRange[1]}
            </span>
          </div>
        </div>
      </div>

      <div className="border-b border-border" />

      {/* Rating Section */}
      <div className="flex flex-col gap-3">
        <h3 className="text-sm font-semibold text-foreground">Rating</h3>
        <div className="flex flex-col gap-2.5">
          {RATING_OPTIONS.map((option) => (
            <button
              type="button"
              key={option.value}
              className="flex cursor-pointer items-center gap-2"
              onClick={() => onRatingChange(option.value)}
              aria-label={`Filter by ${option.label} stars`}
            >
              <span
                className={`flex size-[18px] shrink-0 items-center justify-center rounded-full border-2 ${
                  activeRating === option.value
                    ? "border-primary bg-primary"
                    : "border-border"
                }`}
              >
                {activeRating === option.value && (
                  <span className="size-2 rounded-full bg-primary-foreground" />
                )}
              </span>
              <StarRatingRow filledStars={option.stars} label={option.label} />
            </button>
          ))}
        </div>
      </div>

      <div className="border-b border-border" />

      {/* Reset Filters */}
      {hasActiveFilters && (
        <Button variant="outline" className="w-full" onClick={onReset}>
          <X className="size-4" />
          Reset Filters
        </Button>
      )}
    </div>
  );
}

export function ProductFilters() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [categories, setCategories] = useState<CategoryDto[]>([]);

  const activeCategories = searchParams.get("category")
    ? (searchParams.get("category")?.split(",") ?? [])
    : [];
  const activeRating = searchParams.get("rating") ?? "";
  const priceMin = Number(searchParams.get("priceMin") ?? "0");
  const priceMax = Number(searchParams.get("priceMax") ?? String(MAX_PRICE));
  const priceRange: [number, number] = [priceMin, priceMax];

  useEffect(() => {
    getCategories()
      .then(setCategories)
      .catch(() => {
        // Silently handle - filters will show without categories
      });
  }, []);

  const updateParams = (updates: Record<string, string | null>) => {
    const params = new URLSearchParams(searchParams.toString());
    for (const [key, value] of Object.entries(updates)) {
      if (value) {
        params.set(key, value);
      } else {
        params.delete(key);
      }
    }
    router.replace(`?${params.toString()}`, { scroll: false });
  };

  const handleCategoryToggle = (categoryId: string) => {
    const current = new Set(activeCategories);
    if (current.has(categoryId)) {
      current.delete(categoryId);
    } else {
      current.add(categoryId);
    }
    const value = Array.from(current).join(",");
    updateParams({ category: value || null });
  };

  const handlePriceChange = (value: number[]) => {
    updateParams({
      priceMin: value[0] === 0 ? null : String(value[0]),
      priceMax: value[1] === MAX_PRICE ? null : String(value[1]),
    });
  };

  const handleRatingChange = (value: string) => {
    updateParams({
      rating: activeRating === value ? null : value,
    });
  };

  const handleReset = () => {
    updateParams({
      category: null,
      priceMin: null,
      priceMax: null,
      rating: null,
    });
  };

  const hasActiveFilters =
    activeCategories.length > 0 ||
    priceMin > 0 ||
    priceMax < MAX_PRICE ||
    activeRating !== "";

  const filterProps = {
    categories,
    activeCategories,
    priceRange,
    activeRating,
    onCategoryToggle: handleCategoryToggle,
    onPriceChange: handlePriceChange,
    onRatingChange: handleRatingChange,
    onReset: handleReset,
    hasActiveFilters,
  };

  return (
    <>
      {/* Desktop sidebar - hidden on mobile */}
      <aside className="hidden w-[260px] shrink-0 lg:block">
        <FilterContent {...filterProps} />
      </aside>

      {/* Mobile filter Sheet trigger */}
      <div className="lg:hidden">
        <Sheet>
          <SheetTrigger asChild>
            <Button variant="outline" size="sm">
              <Filter className="size-4" />
              Filters
              {hasActiveFilters && (
                <span className="ml-1 flex size-5 items-center justify-center rounded-full bg-primary text-xs text-primary-foreground">
                  {activeCategories.length +
                    (priceMin > 0 || priceMax < MAX_PRICE ? 1 : 0) +
                    (activeRating ? 1 : 0)}
                </span>
              )}
            </Button>
          </SheetTrigger>
          <SheetContent side="left" className="overflow-y-auto">
            <SheetHeader>
              <SheetTitle>Filters</SheetTitle>
            </SheetHeader>
            <div className="px-4 pb-4">
              <FilterContent {...filterProps} />
            </div>
          </SheetContent>
        </Sheet>
      </div>
    </>
  );
}

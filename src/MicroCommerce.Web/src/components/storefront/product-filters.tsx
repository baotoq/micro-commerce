"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";

import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import type { CategoryDto } from "@/lib/api";
import { getCategories } from "@/lib/api";

const SORT_OPTIONS = [
  { value: "newest", label: "Newest" },
  { value: "price-asc", label: "Price: Low to High" },
  { value: "price-desc", label: "Price: High to Low" },
  { value: "name-asc", label: "Name: A-Z" },
] as const;

export function ProductFilters() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [categories, setCategories] = useState<CategoryDto[]>([]);

  const activeCategory = searchParams.get("category") ?? "";
  const activeSort = searchParams.get("sort") ?? "newest";

  useEffect(() => {
    getCategories()
      .then(setCategories)
      .catch(() => {
        // Silently handle - filters will show without categories
      });
  }, []);

  const updateParam = (key: string, value: string | null) => {
    const params = new URLSearchParams(searchParams.toString());
    if (value) {
      params.set(key, value);
    } else {
      params.delete(key);
    }
    router.replace(`?${params.toString()}`, { scroll: false });
  };

  const handleCategoryClick = (categoryId: string) => {
    updateParam("category", categoryId || null);
  };

  const handleSortChange = (value: string) => {
    updateParam("sort", value === "newest" ? null : value);
  };

  return (
    <div className="flex flex-wrap items-center justify-between gap-4">
      {/* Category chips */}
      <div className="flex flex-wrap items-center gap-2">
        <button
          type="button"
          onClick={() => handleCategoryClick("")}
          className={`rounded-full px-3.5 py-1.5 text-xs font-medium transition-colors ${
            !activeCategory
              ? "bg-zinc-900 text-white"
              : "bg-zinc-100 text-zinc-600 hover:bg-zinc-200"
          }`}
        >
          All
        </button>
        {categories.map((category) => (
          <button
            key={category.id}
            type="button"
            onClick={() => handleCategoryClick(category.id)}
            className={`rounded-full px-3.5 py-1.5 text-xs font-medium transition-colors ${
              activeCategory === category.id
                ? "bg-zinc-900 text-white"
                : "bg-zinc-100 text-zinc-600 hover:bg-zinc-200"
            }`}
          >
            {category.name}
          </button>
        ))}
      </div>

      {/* Sort dropdown */}
      <Select value={activeSort} onValueChange={handleSortChange}>
        <SelectTrigger className="h-8 w-[180px] rounded-full border-zinc-200 text-xs">
          <SelectValue placeholder="Sort by" />
        </SelectTrigger>
        <SelectContent>
          {SORT_OPTIONS.map((option) => (
            <SelectItem key={option.value} value={option.value}>
              {option.label}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );
}

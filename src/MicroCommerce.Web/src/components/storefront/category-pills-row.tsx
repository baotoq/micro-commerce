"use client";

import type { LucideIcon } from "lucide-react";
import {
  Armchair,
  BookOpen,
  Dumbbell,
  Layers,
  Monitor,
  Shirt,
  Watch,
} from "lucide-react";
import { useRouter } from "next/navigation";
import { useEffect, useState } from "react";

import type { CategoryDto } from "@/lib/api";
import { getCategories } from "@/lib/api";
import { CategoryPill } from "./category-pill";

const CATEGORY_ICONS: Record<string, LucideIcon> = {
  clothing: Shirt,
  accessories: Watch,
  electronics: Monitor,
  home: Armchair,
  sports: Dumbbell,
  books: BookOpen,
};

function getCategoryIcon(name: string): LucideIcon | undefined {
  const key = name.toLowerCase();
  return CATEGORY_ICONS[key];
}

interface CategoryPillsRowProps {
  activeCategory?: string;
}

export function CategoryPillsRow({ activeCategory }: CategoryPillsRowProps) {
  const router = useRouter();
  const [categories, setCategories] = useState<CategoryDto[]>([]);

  useEffect(() => {
    getCategories()
      .then(setCategories)
      .catch(() => {
        // Silently handle - pills won't show
      });
  }, []);

  const handleCategoryClick = (categoryId: string | null) => {
    if (categoryId === null || activeCategory === categoryId) {
      router.replace("/", { scroll: false });
    } else {
      router.replace(`?category=${categoryId}`, { scroll: false });
    }
  };

  return (
    <div className="flex flex-wrap gap-3">
      <CategoryPill
        label="All"
        icon={Layers}
        active={!activeCategory}
        onClick={() => handleCategoryClick(null)}
      />
      {categories.map((category) => (
        <CategoryPill
          key={category.id}
          label={category.name}
          icon={getCategoryIcon(category.name)}
          active={activeCategory === category.id}
          onClick={() => handleCategoryClick(category.id)}
        />
      ))}
    </div>
  );
}

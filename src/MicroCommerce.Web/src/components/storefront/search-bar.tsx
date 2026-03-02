"use client";

import { Search, X } from "lucide-react";
import { useRouter, useSearchParams } from "next/navigation";
import { useCallback, useEffect, useRef, useState } from "react";

import { Input } from "@/components/ui/input";

const DEBOUNCE_MS = 300;

export function SearchBar() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [value, setValue] = useState(searchParams.get("search") ?? "");
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  // Sync local state when URL search param changes externally
  useEffect(() => {
    const urlValue = searchParams.get("search") ?? "";
    setValue(urlValue);
  }, [searchParams]);

  const updateUrl = useCallback(
    (searchValue: string) => {
      const params = new URLSearchParams(searchParams.toString());
      if (searchValue.trim()) {
        params.set("search", searchValue.trim());
      } else {
        params.delete("search");
      }
      router.replace(`?${params.toString()}`, { scroll: false });
    },
    [router, searchParams],
  );

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value;
    setValue(newValue);

    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }

    timerRef.current = setTimeout(() => {
      updateUrl(newValue);
    }, DEBOUNCE_MS);
  };

  const handleClear = () => {
    setValue("");
    if (timerRef.current) {
      clearTimeout(timerRef.current);
    }
    updateUrl("");
  };

  // Cleanup timer on unmount
  useEffect(() => {
    return () => {
      if (timerRef.current) {
        clearTimeout(timerRef.current);
      }
    };
  }, []);

  return (
    <div className="relative w-full">
      <Search className="absolute left-3 top-1/2 size-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        type="text"
        placeholder="Search products..."
        value={value}
        onChange={handleChange}
        className="h-10 rounded-full border-border bg-background pl-9 pr-9 text-sm placeholder:text-muted-foreground focus-visible:border-ring focus-visible:ring-ring/20"
      />
      {value && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute right-3 top-1/2 -translate-y-1/2 text-muted-foreground transition-colors hover:text-foreground"
          aria-label="Clear search"
        >
          <X className="size-4" />
        </button>
      )}
    </div>
  );
}

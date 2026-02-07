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
    [router, searchParams]
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
    <div className="relative w-full max-w-xs">
      <Search className="absolute left-2.5 top-1/2 h-3.5 w-3.5 -translate-y-1/2 text-zinc-400" />
      <Input
        type="text"
        placeholder="Search products..."
        value={value}
        onChange={handleChange}
        className="h-8 rounded-full border-zinc-200 bg-zinc-50 pl-8 pr-8 text-xs placeholder:text-zinc-400 focus-visible:border-zinc-300 focus-visible:ring-zinc-200/50"
      />
      {value && (
        <button
          type="button"
          onClick={handleClear}
          className="absolute right-2.5 top-1/2 -translate-y-1/2 text-zinc-400 transition-colors hover:text-zinc-600"
          aria-label="Clear search"
        >
          <X className="h-3.5 w-3.5" />
        </button>
      )}
    </div>
  );
}

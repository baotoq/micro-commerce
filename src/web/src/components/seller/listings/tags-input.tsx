"use client";

import { useState } from "react";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

const TAG_REGEX = /^[a-z0-9-]{2,24}$/;
const MAX_TAGS = 8;

type Props = {
  value: string[];
  onChange: (next: string[]) => void;
  placeholder?: string;
  className?: string;
};

export function TagsInput({ value, onChange, placeholder, className }: Props) {
  const [draft, setDraft] = useState("");

  const commit = (raw: string) => {
    const normalised = raw.trim().toLowerCase();
    if (!normalised) {
      setDraft("");
      return;
    }
    if (!TAG_REGEX.test(normalised)) {
      // Leave the draft so the user can correct it.
      return;
    }
    if (value.includes(normalised)) {
      setDraft("");
      return;
    }
    if (value.length >= MAX_TAGS) {
      // Cap reached; preserve the draft so the user knows nothing happened.
      return;
    }
    onChange([...value, normalised]);
    setDraft("");
  };

  const removeAt = (index: number) => {
    onChange(value.filter((_, i) => i !== index));
  };

  return (
    <div className={cn("flex flex-wrap items-center gap-1.5", className)}>
      {value.map((tag, idx) => (
        <span
          key={tag}
          className="inline-flex items-center gap-1 rounded-full bg-muted px-2 py-1 text-[11px] text-foreground"
        >
          {tag}
          <button
            type="button"
            aria-label={`Remove ${tag}`}
            onClick={() => removeAt(idx)}
            className="text-muted-foreground hover:text-foreground"
          >
            &times;
          </button>
        </span>
      ))}
      <Input
        type="text"
        value={draft}
        placeholder={placeholder ?? "Add a tag"}
        onChange={(e) => {
          const v = e.target.value;
          // Auto-commit when the user types a comma. We strip the comma and
          // pass the preceding text into commit().
          if (v.endsWith(",")) {
            commit(v.slice(0, -1));
            return;
          }
          setDraft(v);
        }}
        onKeyDown={(e) => {
          if (e.key === "Enter") {
            e.preventDefault();
            commit(draft);
            return;
          }
          if (e.key === "Backspace" && draft === "" && value.length > 0) {
            e.preventDefault();
            removeAt(value.length - 1);
          }
        }}
        className="h-7 w-auto flex-1 min-w-[120px] border-0 bg-transparent p-0 text-[11px] text-foreground placeholder:text-muted-foreground focus-visible:ring-0"
        autoComplete="off"
      />
    </div>
  );
}

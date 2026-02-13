"use client";

import { useRef } from "react";
import { User, Upload, Loader2 } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { useUploadAvatar, useRemoveAvatar } from "@/hooks/use-profile";

interface AvatarUploadProps {
  currentAvatarUrl?: string | null;
}

export function AvatarUpload({ currentAvatarUrl }: AvatarUploadProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const uploadMutation = useUploadAvatar();
  const removeMutation = useRemoveAvatar();

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    // Validate file type
    if (!file.type.startsWith("image/")) {
      alert("Please select an image file");
      return;
    }

    // Validate file size (5MB)
    if (file.size > 5 * 1024 * 1024) {
      alert("File size must be less than 5MB");
      return;
    }

    uploadMutation.mutate(file);

    // Reset input so same file can be selected again
    event.target.value = "";
  };

  const isPending = uploadMutation.isPending || removeMutation.isPending;

  return (
    <div className="flex flex-col items-center gap-3">
      <button
        type="button"
        onClick={() => fileInputRef.current?.click()}
        disabled={isPending}
        className="group relative h-20 w-20 rounded-full transition-opacity hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
      >
        <Avatar className="h-20 w-20">
          <AvatarImage src={currentAvatarUrl || undefined} alt="Avatar" />
          <AvatarFallback>
            <User className="h-10 w-10 text-zinc-400" />
          </AvatarFallback>
        </Avatar>

        {isPending ? (
          <div className="absolute inset-0 flex items-center justify-center rounded-full bg-black/40">
            <Loader2 className="h-6 w-6 animate-spin text-white" />
          </div>
        ) : (
          <div className="absolute inset-0 flex items-center justify-center rounded-full bg-black/40 opacity-0 transition-opacity group-hover:opacity-100">
            <Upload className="h-6 w-6 text-white" />
          </div>
        )}
      </button>

      <input
        ref={fileInputRef}
        type="file"
        accept="image/*"
        onChange={handleFileChange}
        className="hidden"
      />

      {currentAvatarUrl && !isPending && (
        <button
          type="button"
          onClick={() => removeMutation.mutate()}
          className="text-xs text-zinc-500 hover:text-zinc-900 transition-colors"
        >
          Remove
        </button>
      )}
    </div>
  );
}

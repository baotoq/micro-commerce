"use client";

import { Loader2, Upload, User } from "lucide-react";
import { useRef } from "react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { useRemoveAvatar, useUploadAvatar } from "@/hooks/use-profile";

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
    <div className="flex items-center gap-4">
      <button
        type="button"
        onClick={() => fileInputRef.current?.click()}
        disabled={isPending}
        className="group relative shrink-0 rounded-full transition-opacity hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
        aria-label="Upload avatar"
      >
        <Avatar className="h-24 w-24">
          <AvatarImage src={currentAvatarUrl || undefined} alt="Avatar" />
          <AvatarFallback className="bg-muted">
            <User className="h-10 w-10 text-muted-foreground" />
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

      <div className="flex flex-col gap-1.5">
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => fileInputRef.current?.click()}
          disabled={isPending}
        >
          Change Photo
        </Button>
        {currentAvatarUrl && !isPending && (
          <Button
            type="button"
            variant="ghost"
            size="sm"
            onClick={() => removeMutation.mutate()}
            className="text-muted-foreground hover:text-destructive"
          >
            Remove
          </Button>
        )}
      </div>
    </div>
  );
}

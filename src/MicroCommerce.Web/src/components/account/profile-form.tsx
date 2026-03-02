"use client";

import { Loader2 } from "lucide-react";
import { useSession } from "next-auth/react";
import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Skeleton } from "@/components/ui/skeleton";
import { useProfile, useUpdateProfile } from "@/hooks/use-profile";
import { AvatarUpload } from "./avatar-upload";

export function ProfileForm() {
  const { data: session } = useSession();
  const { data: profile, isLoading } = useProfile();
  const updateMutation = useUpdateProfile();
  const [isEditing, setIsEditing] = useState(false);
  const [displayName, setDisplayName] = useState("");

  const handleEdit = () => {
    setDisplayName(profile?.displayName || "");
    setIsEditing(true);
  };

  const handleCancel = () => {
    setIsEditing(false);
    setDisplayName("");
  };

  const handleSave = async () => {
    await updateMutation.mutateAsync({ displayName });
    setIsEditing(false);
  };

  if (isLoading) {
    return (
      <Card>
        <CardContent className="space-y-5 p-6">
          <Skeleton className="mb-1 h-5 w-40" />
          <div className="flex items-center gap-4">
            <Skeleton className="size-24 rounded-full" />
            <Skeleton className="h-9 w-28" />
          </div>
          <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-10 w-full" />
            </div>
            <div className="space-y-1.5">
              <Skeleton className="h-4 w-20" />
              <Skeleton className="h-10 w-full" />
            </div>
          </div>
          <div className="space-y-1.5">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-10 w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!profile) {
    return (
      <Card>
        <CardContent className="p-6 text-center text-muted-foreground">
          Failed to load profile
        </CardContent>
      </Card>
    );
  }

  const memberSince = new Date(profile.createdAt).toLocaleDateString("en-US", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });

  return (
    <Card>
      <CardContent className="space-y-5 p-6">
        <h3 className="text-lg font-bold text-foreground">
          Personal Information
        </h3>

        {/* Avatar section */}
        <AvatarUpload currentAvatarUrl={profile.avatarUrl} />

        {/* Name fields - 2 column grid */}
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          <div className="space-y-1.5">
            <Label htmlFor="displayName" className="text-sm font-medium">
              Display Name
            </Label>
            {isEditing ? (
              <Input
                id="displayName"
                value={displayName}
                onChange={(e) => setDisplayName(e.target.value)}
                placeholder="Enter your display name"
              />
            ) : (
              <p className="text-sm text-foreground">{profile.displayName}</p>
            )}
          </div>

          <div className="space-y-1.5">
            <Label className="text-sm font-medium">Member Since</Label>
            <p className="text-sm text-muted-foreground">{memberSince}</p>
          </div>
        </div>

        {/* Email field - full width */}
        <div className="space-y-1.5">
          <Label className="text-sm font-medium">Email Address</Label>
          <p className="text-sm text-muted-foreground">
            {session?.user?.email}
          </p>
        </div>

        {/* Action buttons */}
        <div className="flex justify-end gap-2 pt-2">
          {isEditing ? (
            <>
              <Button
                variant="outline"
                onClick={handleCancel}
                disabled={updateMutation.isPending}
              >
                Cancel
              </Button>
              <Button
                onClick={handleSave}
                disabled={updateMutation.isPending || !displayName.trim()}
              >
                {updateMutation.isPending && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                Save Changes
              </Button>
            </>
          ) : (
            <Button onClick={handleEdit}>Edit Profile</Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

"use client";

import { ProfileForm } from "@/components/account/profile-form";

export default function ProfilePage() {
  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold tracking-tight text-foreground">
          Profile
        </h2>
        <p className="mt-1 text-sm text-muted-foreground">
          Manage your display name and avatar
        </p>
      </div>
      <ProfileForm />
    </div>
  );
}

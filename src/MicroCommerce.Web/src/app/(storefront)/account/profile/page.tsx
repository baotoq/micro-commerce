"use client";

import { ProfileForm } from "@/components/account/profile-form";

export default function ProfilePage() {
  return (
    <div className="space-y-6">
      <h2 className="text-2xl font-semibold">Profile</h2>
      <ProfileForm />
    </div>
  );
}

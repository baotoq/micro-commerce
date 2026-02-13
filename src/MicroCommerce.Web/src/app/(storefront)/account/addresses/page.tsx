"use client";

import { useProfile } from "@/hooks/use-profile";
import { Card, CardContent } from "@/components/ui/card";

export default function AddressesPage() {
  const { data: profile } = useProfile();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold">Addresses</h2>
        {profile?.addresses && profile.addresses.length > 0 && (
          <span className="text-sm text-zinc-500">
            {profile.addresses.length} {profile.addresses.length === 1 ? "address" : "addresses"}
          </span>
        )}
      </div>

      <Card>
        <CardContent className="py-12 text-center text-zinc-500">
          Address book coming soon.
        </CardContent>
      </Card>
    </div>
  );
}

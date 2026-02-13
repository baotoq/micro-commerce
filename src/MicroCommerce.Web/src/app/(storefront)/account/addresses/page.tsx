"use client";

import { Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useProfile } from "@/hooks/use-profile";
import { AddressCard } from "@/components/account/address-card";
import { AddressFormDialog } from "@/components/account/address-form-dialog";

export default function AddressesPage() {
  const { data: profile, isLoading } = useProfile();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h2 className="text-2xl font-semibold">Addresses</h2>
        <AddressFormDialog
          trigger={
            <Button size="sm" className="gap-1">
              <Plus className="h-4 w-4" />
              Add Address
            </Button>
          }
        />
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {[1, 2].map((i) => (
            <div key={i} className="h-48 animate-pulse rounded-lg border bg-zinc-100" />
          ))}
        </div>
      ) : profile?.addresses && profile.addresses.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {profile.addresses.map((address) => (
            <AddressCard key={address.id} address={address} />
          ))}
        </div>
      ) : (
        <div className="rounded-lg border border-dashed border-zinc-300 py-12 text-center">
          <p className="text-sm text-zinc-500">
            No addresses saved yet. Add your first address.
          </p>
        </div>
      )}
    </div>
  );
}

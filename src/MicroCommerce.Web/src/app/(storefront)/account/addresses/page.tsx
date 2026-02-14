"use client";

import { MapPin, Plus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useProfile } from "@/hooks/use-profile";
import { AddressCard } from "@/components/account/address-card";
import { AddressFormDialog } from "@/components/account/address-form-dialog";

export default function AddressesPage() {
  const { data: profile, isLoading } = useProfile();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold tracking-tight">Addresses</h2>
          <p className="mt-1 text-sm text-zinc-500">Manage your shipping addresses</p>
        </div>
        <AddressFormDialog
          trigger={
            <Button size="sm" className="gap-1 rounded-full">
              <Plus className="h-4 w-4" />
              Add Address
            </Button>
          }
        />
      </div>

      {isLoading ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {[1, 2].map((i) => (
            <div key={i} className="rounded-xl border border-zinc-200 p-6 space-y-3">
              <Skeleton className="h-5 w-32" />
              <Skeleton className="h-4 w-48" />
              <Skeleton className="h-4 w-40" />
              <Skeleton className="h-4 w-24" />
              <div className="flex gap-2 pt-2">
                <Skeleton className="h-8 w-8 rounded-md" />
                <Skeleton className="h-8 w-8 rounded-md" />
                <Skeleton className="h-8 w-8 rounded-md" />
              </div>
            </div>
          ))}
        </div>
      ) : profile?.addresses && profile.addresses.length > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2">
          {profile.addresses.map((address) => (
            <AddressCard key={address.id} address={address} />
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <MapPin className="mb-4 size-12 text-zinc-300" />
          <h3 className="text-lg font-semibold text-zinc-900">No saved addresses</h3>
          <p className="mt-1 text-sm text-zinc-500">Add an address to make checkout faster next time.</p>
          <AddressFormDialog
            trigger={
              <Button className="mt-6 rounded-full" size="lg">
                <Plus className="mr-2 size-4" />
                Add Address
              </Button>
            }
          />
        </div>
      )}
    </div>
  );
}

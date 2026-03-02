"use client";

import { MapPin, Plus } from "lucide-react";
import { AddressCard } from "@/components/account/address-card";
import { AddressFormDialog } from "@/components/account/address-form-dialog";
import { Button } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { useProfile } from "@/hooks/use-profile";

export default function AddressesPage() {
  const { data: profile, isLoading } = useProfile();

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h2 className="text-2xl font-bold tracking-tight text-foreground">
            Addresses
          </h2>
          <p className="mt-1 text-sm text-muted-foreground">
            Manage your shipping addresses
          </p>
        </div>
        <AddressFormDialog
          trigger={
            <Button size="sm" className="gap-1.5">
              <Plus className="h-4 w-4" />
              Add Address
            </Button>
          }
        />
      </div>

      {isLoading ? (
        <div className="space-y-4">
          {[1, 2].map((i) => (
            <div key={i} className="space-y-3 rounded-lg border p-4">
              <Skeleton className="h-5 w-32" />
              <Skeleton className="h-4 w-48" />
              <div className="flex gap-2 pt-1">
                <Skeleton className="h-4 w-12" />
                <Skeleton className="h-4 w-12" />
              </div>
            </div>
          ))}
        </div>
      ) : profile?.addresses && profile.addresses.length > 0 ? (
        <div className="space-y-3">
          {profile.addresses.map((address) => (
            <AddressCard key={address.id} address={address} />
          ))}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <MapPin className="mb-4 size-12 text-muted-foreground/40" />
          <h3 className="text-lg font-semibold text-foreground">
            No saved addresses
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            Add an address to make checkout faster next time.
          </p>
          <AddressFormDialog
            trigger={
              <Button className="mt-6" size="lg">
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

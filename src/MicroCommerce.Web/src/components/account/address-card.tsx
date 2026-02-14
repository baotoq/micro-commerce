"use client";

import { Star, Edit, Trash2 } from "lucide-react";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useSetDefaultAddress } from "@/hooks/use-profile";
import { AddressFormDialog } from "./address-form-dialog";
import { DeleteAddressDialog } from "./delete-address-dialog";
import type { AddressDto } from "@/lib/api";

interface AddressCardProps {
  address: AddressDto;
}

export function AddressCard({ address }: AddressCardProps) {
  const setDefault = useSetDefaultAddress();

  const handleSetDefault = () => {
    if (!address.isDefault) {
      setDefault.mutate(address.id);
    }
  };

  return (
    <Card className="rounded-xl border-zinc-200">
      <CardContent className="pt-6">
        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <h3 className="font-semibold text-zinc-900">{address.name}</h3>
            {address.isDefault && (
              <div className="flex items-center gap-1 text-sm text-amber-600">
                <Star className="h-3.5 w-3.5 fill-amber-600" />
                <span>Default</span>
              </div>
            )}
          </div>
          <div className="text-sm text-zinc-500">
            <p>{address.street}</p>
            <p>
              {address.city}, {address.state} {address.zipCode}
            </p>
            <p>{address.country}</p>
          </div>
        </div>
      </CardContent>
      <CardFooter className="flex gap-2">
        {!address.isDefault && (
          <Button
            variant="ghost"
            size="sm"
            onClick={handleSetDefault}
            disabled={setDefault.isPending}
            className="gap-1"
          >
            <Star className="h-3.5 w-3.5" />
            Set as default
          </Button>
        )}
        <AddressFormDialog
          address={address}
          trigger={
            <Button variant="ghost" size="sm" className="gap-1">
              <Edit className="h-3.5 w-3.5" />
              Edit
            </Button>
          }
        />
        <DeleteAddressDialog
          addressId={address.id}
          addressName={address.name}
          trigger={
            <Button variant="ghost" size="sm" className="gap-1 text-red-600 hover:text-red-700">
              <Trash2 className="h-3.5 w-3.5" />
              Delete
            </Button>
          }
        />
      </CardFooter>
    </Card>
  );
}

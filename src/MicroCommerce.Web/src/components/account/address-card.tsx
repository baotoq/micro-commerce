"use client";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { useSetDefaultAddress } from "@/hooks/use-profile";
import type { AddressDto } from "@/lib/api";
import { AddressFormDialog } from "./address-form-dialog";
import { DeleteAddressDialog } from "./delete-address-dialog";

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

  const fullAddress = [
    address.street,
    [address.city, address.state, address.zipCode].filter(Boolean).join(", "),
    address.country,
  ]
    .filter(Boolean)
    .join(", ");

  return (
    <Card>
      <CardContent className="flex items-start justify-between gap-4 p-4">
        <div className="space-y-1">
          <div className="flex items-center gap-2">
            <span className="text-sm font-semibold text-foreground">
              {address.name}
            </span>
            {address.isDefault && (
              <Badge variant="secondary" className="text-xs">
                Default
              </Badge>
            )}
          </div>
          <p className="text-[13px] text-muted-foreground">{fullAddress}</p>
        </div>

        <div className="flex shrink-0 items-center gap-2">
          {!address.isDefault && (
            <Button
              variant="ghost"
              size="sm"
              onClick={handleSetDefault}
              disabled={setDefault.isPending}
              className="h-auto px-0 py-0 text-[13px] font-medium text-primary hover:bg-transparent hover:underline"
            >
              Set default
            </Button>
          )}
          <AddressFormDialog
            address={address}
            trigger={
              <Button
                variant="ghost"
                size="sm"
                className="h-auto gap-1 px-0 py-0 text-[13px] font-medium text-primary hover:bg-transparent hover:underline"
              >
                Edit
              </Button>
            }
          />
          <DeleteAddressDialog
            addressId={address.id}
            addressName={address.name}
            trigger={
              <Button
                variant="ghost"
                size="sm"
                className="h-auto gap-1 px-0 py-0 text-[13px] font-medium text-destructive hover:bg-transparent hover:underline"
              >
                Delete
              </Button>
            }
          />
        </div>
      </CardContent>
    </Card>
  );
}

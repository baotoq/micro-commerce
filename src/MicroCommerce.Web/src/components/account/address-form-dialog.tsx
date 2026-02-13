"use client";

import { useState } from "react";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAddAddress, useUpdateAddress } from "@/hooks/use-profile";
import type { AddressDto } from "@/lib/api";

interface AddressFormDialogProps {
  address?: AddressDto;
  trigger: React.ReactNode;
  onClose?: () => void;
}

export function AddressFormDialog({ address, trigger, onClose }: AddressFormDialogProps) {
  const [open, setOpen] = useState(false);
  const [formData, setFormData] = useState({
    name: address?.name || "",
    street: address?.street || "",
    city: address?.city || "",
    state: address?.state || "",
    zipCode: address?.zipCode || "",
    country: address?.country || "",
  });
  const [errors, setErrors] = useState<Record<string, string>>({});

  const addAddress = useAddAddress();
  const updateAddress = useUpdateAddress();

  const isEdit = !!address;

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    if (!formData.name.trim()) {
      newErrors.name = "Name is required";
    }
    if (!formData.street.trim()) {
      newErrors.street = "Street address is required";
    }
    if (!formData.city.trim()) {
      newErrors.city = "City is required";
    }
    if (!formData.state.trim()) {
      newErrors.state = "State is required";
    }
    if (!formData.zipCode.trim()) {
      newErrors.zipCode = "Zip code is required";
    }
    if (!formData.country.trim()) {
      newErrors.country = "Country is required";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    if (isEdit) {
      updateAddress.mutate(
        { id: address.id, ...formData },
        {
          onSuccess: () => {
            setOpen(false);
            onClose?.();
            setErrors({});
          },
        }
      );
    } else {
      addAddress.mutate(formData, {
        onSuccess: () => {
          setOpen(false);
          onClose?.();
          // Reset form
          setFormData({
            name: "",
            street: "",
            city: "",
            state: "",
            zipCode: "",
            country: "",
          });
          setErrors({});
        },
      });
    }
  };

  const handleOpenChange = (newOpen: boolean) => {
    setOpen(newOpen);
    if (!newOpen) {
      // Reset form when closing
      setFormData({
        name: address?.name || "",
        street: address?.street || "",
        city: address?.city || "",
        state: address?.state || "",
        zipCode: address?.zipCode || "",
        country: address?.country || "",
      });
      setErrors({});
    }
  };

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <div onClick={() => setOpen(true)}>{trigger}</div>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>{isEdit ? "Edit Address" : "Add Address"}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name</Label>
            <Input
              id="name"
              placeholder="Home, Work, etc."
              value={formData.name}
              onChange={(e) => setFormData({ ...formData, name: e.target.value })}
            />
            {errors.name && <p className="text-sm text-red-500">{errors.name}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="street">Street Address</Label>
            <Input
              id="street"
              placeholder="123 Main St"
              value={formData.street}
              onChange={(e) => setFormData({ ...formData, street: e.target.value })}
            />
            {errors.street && <p className="text-sm text-red-500">{errors.street}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="city">City</Label>
            <Input
              id="city"
              placeholder="New York"
              value={formData.city}
              onChange={(e) => setFormData({ ...formData, city: e.target.value })}
            />
            {errors.city && <p className="text-sm text-red-500">{errors.city}</p>}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="state">State</Label>
              <Input
                id="state"
                placeholder="NY"
                value={formData.state}
                onChange={(e) => setFormData({ ...formData, state: e.target.value })}
              />
              {errors.state && <p className="text-sm text-red-500">{errors.state}</p>}
            </div>

            <div className="space-y-2">
              <Label htmlFor="zipCode">Zip Code</Label>
              <Input
                id="zipCode"
                placeholder="10001"
                value={formData.zipCode}
                onChange={(e) => setFormData({ ...formData, zipCode: e.target.value })}
              />
              {errors.zipCode && <p className="text-sm text-red-500">{errors.zipCode}</p>}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="country">Country</Label>
            <Input
              id="country"
              placeholder="United States"
              value={formData.country}
              onChange={(e) => setFormData({ ...formData, country: e.target.value })}
            />
            {errors.country && <p className="text-sm text-red-500">{errors.country}</p>}
          </div>

          <div className="flex gap-2 pt-4">
            <Button
              type="submit"
              disabled={addAddress.isPending || updateAddress.isPending}
              className="flex-1"
            >
              {addAddress.isPending || updateAddress.isPending
                ? "Saving..."
                : isEdit
                  ? "Update Address"
                  : "Add Address"}
            </Button>
            <Button type="button" variant="outline" onClick={() => setOpen(false)}>
              Cancel
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}

"use client";

import { useState } from "react";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { useDeleteAddress } from "@/hooks/use-profile";

interface DeleteAddressDialogProps {
  addressId: string;
  addressName: string;
  trigger: React.ReactNode;
}

export function DeleteAddressDialog({
  addressId,
  addressName,
  trigger,
}: DeleteAddressDialogProps) {
  const [open, setOpen] = useState(false);
  const deleteAddress = useDeleteAddress();

  const handleDelete = () => {
    deleteAddress.mutate(addressId, {
      onSuccess: () => {
        setOpen(false);
      },
    });
  };

  return (
    <AlertDialog open={open} onOpenChange={setOpen}>
      <button
        type="button"
        onClick={() => setOpen(true)}
        className="appearance-none text-left"
      >
        {trigger}
      </button>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Address?</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete &ldquo;{addressName}&rdquo;? This
            action cannot be undone.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={deleteAddress.isPending}
            className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
          >
            {deleteAddress.isPending ? "Deleting..." : "Delete"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

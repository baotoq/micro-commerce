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

export function DeleteAddressDialog({ addressId, addressName, trigger }: DeleteAddressDialogProps) {
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
      <div onClick={() => setOpen(true)}>{trigger}</div>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Address?</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete '{addressName}'? This action cannot be undone.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={deleteAddress.isPending}
            className="bg-red-600 hover:bg-red-700"
          >
            {deleteAddress.isPending ? "Deleting..." : "Delete"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}

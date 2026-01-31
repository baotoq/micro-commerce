'use client';

import { useState } from 'react';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { Loader2 } from 'lucide-react';
import { CategoryDto, deleteCategory } from '@/lib/api';
import { toast } from 'sonner';

interface DeleteCategoryDialogProps {
  open: boolean;
  onClose: () => void;
  category: CategoryDto | null;
  onDelete: () => void;
}

export function DeleteCategoryDialog({
  open,
  onClose,
  category,
  onDelete,
}: DeleteCategoryDialogProps) {
  const [deleting, setDeleting] = useState(false);

  const handleDelete = async () => {
    if (!category) return;

    setDeleting(true);

    try {
      await deleteCategory(category.id);
      toast.success('Category deleted successfully');
      onDelete();
      onClose();
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Failed to delete category';
      // Check for conflict error (has products)
      if (message.includes('products')) {
        toast.error('Cannot delete category that has products. Remove or reassign products first.');
      } else {
        toast.error(message);
      }
    } finally {
      setDeleting(false);
    }
  };

  return (
    <AlertDialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Delete Category</AlertDialogTitle>
          <AlertDialogDescription>
            Are you sure you want to delete &quot;{category?.name}&quot;? This action cannot be undone.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={deleting}>Cancel</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleDelete}
            disabled={deleting}
            className="bg-red-600 hover:bg-red-700"
          >
            {deleting && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Delete
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}


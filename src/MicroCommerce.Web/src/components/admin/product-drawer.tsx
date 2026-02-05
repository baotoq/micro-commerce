'use client';

import { useState, useEffect } from 'react';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Loader2 } from 'lucide-react';
import { ImageUpload } from './image-upload';
import {
  ProductDto,
  CategoryDto,
  createProduct,
  updateProduct,
  CreateProductRequest,
} from '@/lib/api';
import { toast } from 'sonner';

interface ProductDrawerProps {
  open: boolean;
  onClose: () => void;
  product: ProductDto | null;
  categories: CategoryDto[];
  onSave: () => void;
}

interface FormData {
  name: string;
  description: string;
  price: string;
  categoryId: string;
  imageUrl: string;
  sku: string;
}

interface FormErrors {
  name?: string;
  description?: string;
  price?: string;
  categoryId?: string;
}

const initialFormData: FormData = {
  name: '',
  description: '',
  price: '',
  categoryId: '',
  imageUrl: '',
  sku: '',
};

export function ProductDrawer({
  open,
  onClose,
  product,
  categories,
  onSave,
}: ProductDrawerProps) {
  const [formData, setFormData] = useState<FormData>(initialFormData);
  const [errors, setErrors] = useState<FormErrors>({});
  const [saving, setSaving] = useState(false);

  const isEditing = !!product;

  // Reset form when drawer opens/closes or product changes
  useEffect(() => {
    if (open && product) {
      setFormData({
        name: product.name,
        description: product.description,
        price: product.price.toString(),
        categoryId: product.categoryId,
        imageUrl: product.imageUrl || '',
        sku: product.sku || '',
      });
    } else if (open) {
      setFormData(initialFormData);
    }
    setErrors({});
  }, [open, product]);

  const validate = (): boolean => {
    const newErrors: FormErrors = {};

    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    } else if (formData.name.length < 2) {
      newErrors.name = 'Name must be at least 2 characters';
    } else if (formData.name.length > 200) {
      newErrors.name = 'Name cannot exceed 200 characters';
    }

    if (!formData.description.trim()) {
      newErrors.description = 'Description is required';
    } else if (formData.description.length > 4000) {
      newErrors.description = 'Description cannot exceed 4000 characters';
    }

    const price = parseFloat(formData.price);
    if (isNaN(price)) {
      newErrors.price = 'Price must be a number';
    } else if (price < 0) {
      newErrors.price = 'Price cannot be negative';
    }

    if (!formData.categoryId) {
      newErrors.categoryId = 'Category is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!validate()) return;

    setSaving(true);

    try {
      const data: CreateProductRequest = {
        name: formData.name.trim(),
        description: formData.description.trim(),
        price: parseFloat(formData.price),
        categoryId: formData.categoryId,
        imageUrl: formData.imageUrl || undefined,
        sku: formData.sku.trim() || undefined,
      };

      if (isEditing) {
        await updateProduct(product.id, data);
        toast.success('Product updated successfully');
      } else {
        await createProduct(data);
        toast.success('Product created successfully');
      }

      onSave();
      onClose();
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to save product');
    } finally {
      setSaving(false);
    }
  };

  const handleChange = (field: keyof FormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
    // Clear error when user types
    if (errors[field as keyof FormErrors]) {
      setErrors((prev) => ({ ...prev, [field]: undefined }));
    }
  };

  return (
    <Sheet open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <SheetContent className="w-full sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <SheetTitle>{isEditing ? 'Edit Product' : 'Add Product'}</SheetTitle>
          <SheetDescription>
            {isEditing
              ? 'Update the product details below.'
              : 'Fill in the details to create a new product.'}
          </SheetDescription>
        </SheetHeader>

        <form onSubmit={handleSubmit} className="mt-6 space-y-6">
          {/* Image Upload */}
          <div className="space-y-2">
            <Label>Product Image</Label>
            <ImageUpload
              value={formData.imageUrl || undefined}
              onChange={(url) => handleChange('imageUrl', url || '')}
              disabled={saving}
            />
          </div>

          {/* Name */}
          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input
              id="name"
              value={formData.name}
              onChange={(e) => handleChange('name', e.target.value)}
              placeholder="Product name"
              disabled={saving}
              className={errors.name ? 'border-red-500' : ''}
            />
            {errors.name && (
              <p className="text-sm text-red-500">{errors.name}</p>
            )}
          </div>

          {/* Description */}
          <div className="space-y-2">
            <Label htmlFor="description">Description *</Label>
            <Textarea
              id="description"
              value={formData.description}
              onChange={(e) => handleChange('description', e.target.value)}
              placeholder="Product description"
              rows={4}
              disabled={saving}
              className={errors.description ? 'border-red-500' : ''}
            />
            {errors.description && (
              <p className="text-sm text-red-500">{errors.description}</p>
            )}
          </div>

          {/* Price */}
          <div className="space-y-2">
            <Label htmlFor="price">Price (USD) *</Label>
            <Input
              id="price"
              type="number"
              step="0.01"
              min="0"
              value={formData.price}
              onChange={(e) => handleChange('price', e.target.value)}
              placeholder="0.00"
              disabled={saving}
              className={errors.price ? 'border-red-500' : ''}
            />
            {errors.price && (
              <p className="text-sm text-red-500">{errors.price}</p>
            )}
          </div>

          {/* Category */}
          <div className="space-y-2">
            <Label htmlFor="category">Category *</Label>
            <Select
              value={formData.categoryId}
              onValueChange={(value) => handleChange('categoryId', value)}
              disabled={saving}
            >
              <SelectTrigger className={errors.categoryId ? 'border-red-500' : ''}>
                <SelectValue placeholder="Select a category" />
              </SelectTrigger>
              <SelectContent>
                {categories.map((category) => (
                  <SelectItem key={category.id} value={category.id}>
                    {category.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.categoryId && (
              <p className="text-sm text-red-500">{errors.categoryId}</p>
            )}
          </div>

          {/* SKU */}
          <div className="space-y-2">
            <Label htmlFor="sku">SKU (optional)</Label>
            <Input
              id="sku"
              value={formData.sku}
              onChange={(e) => handleChange('sku', e.target.value)}
              placeholder="Product SKU"
              disabled={saving}
            />
          </div>

          {/* Actions */}
          <div className="flex justify-end gap-3 pt-4">
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={saving}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={saving}>
              {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEditing ? 'Update Product' : 'Create Product'}
            </Button>
          </div>
        </form>
      </SheetContent>
    </Sheet>
  );
}


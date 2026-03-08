"use client";

import { Loader2 } from "lucide-react";
import { useEffect, useState } from "react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { useCreateCoupon, useUpdateCoupon } from "@/hooks/use-coupons";
import type { CouponDto } from "@/lib/api";

interface CouponDialogProps {
  open: boolean;
  onClose: () => void;
  coupon: CouponDto | null;
}

interface FormData {
  code: string;
  description: string;
  discountType: string;
  discountValue: string;
  validFrom: string;
  validUntil: string;
  minOrderAmount: string;
  maxDiscountAmount: string;
  usageLimit: string;
  usagePerUser: string;
}

function toDateInputValue(iso: string | null | undefined): string {
  if (!iso) return "";
  return iso.slice(0, 10);
}

const EMPTY_FORM: FormData = {
  code: "",
  description: "",
  discountType: "Percentage",
  discountValue: "",
  validFrom: new Date().toISOString().slice(0, 10),
  validUntil: "",
  minOrderAmount: "",
  maxDiscountAmount: "",
  usageLimit: "",
  usagePerUser: "",
};

export function CouponDialog({ open, onClose, coupon }: CouponDialogProps) {
  const isEditing = !!coupon;
  const createCoupon = useCreateCoupon();
  const updateCoupon = useUpdateCoupon();
  const isPending = createCoupon.isPending || updateCoupon.isPending;

  const [formData, setFormData] = useState<FormData>(EMPTY_FORM);
  const [errors, setErrors] = useState<Partial<FormData>>({});

  useEffect(() => {
    if (open) {
      if (coupon) {
        setFormData({
          code: coupon.code,
          description: coupon.description,
          discountType: coupon.discountType,
          discountValue: coupon.discountValue.toString(),
          validFrom: toDateInputValue(coupon.validFrom),
          validUntil: toDateInputValue(coupon.validUntil),
          minOrderAmount: coupon.minOrderAmount?.toString() ?? "",
          maxDiscountAmount: coupon.maxDiscountAmount?.toString() ?? "",
          usageLimit: coupon.usageLimit?.toString() ?? "",
          usagePerUser: coupon.usagePerUser?.toString() ?? "",
        });
      } else {
        setFormData(EMPTY_FORM);
      }
      setErrors({});
    }
  }, [open, coupon]);

  function validate(): boolean {
    const newErrors: Partial<FormData> = {};
    if (!formData.code.trim()) newErrors.code = "Code is required";
    if (!formData.description.trim())
      newErrors.description = "Description is required";
    if (!formData.discountValue || Number(formData.discountValue) <= 0)
      newErrors.discountValue = "Discount value must be greater than 0";
    if (
      formData.discountType === "Percentage" &&
      Number(formData.discountValue) > 100
    )
      newErrors.discountValue = "Percentage cannot exceed 100";
    if (!formData.validFrom)
      newErrors.validFrom = "Valid from date is required";
    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  function field(key: keyof FormData) {
    return {
      value: formData[key],
      onChange: (
        e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>,
      ) => {
        setFormData((prev) => ({ ...prev, [key]: e.target.value }));
        if (errors[key]) setErrors((prev) => ({ ...prev, [key]: undefined }));
      },
    };
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    const payload = {
      description: formData.description.trim(),
      discountType: formData.discountType,
      discountValue: Number(formData.discountValue),
      validFrom: new Date(formData.validFrom).toISOString(),
      validUntil: formData.validUntil
        ? new Date(formData.validUntil).toISOString()
        : undefined,
      minOrderAmount: formData.minOrderAmount
        ? Number(formData.minOrderAmount)
        : undefined,
      maxDiscountAmount: formData.maxDiscountAmount
        ? Number(formData.maxDiscountAmount)
        : undefined,
      usageLimit: formData.usageLimit ? Number(formData.usageLimit) : undefined,
      usagePerUser: formData.usagePerUser
        ? Number(formData.usagePerUser)
        : undefined,
    };

    if (isEditing && coupon) {
      updateCoupon.mutate(
        { id: coupon.id, data: payload },
        { onSuccess: onClose },
      );
    } else {
      createCoupon.mutate(
        { ...payload, code: formData.code.trim().toUpperCase() },
        { onSuccess: onClose },
      );
    }
  }

  return (
    <Dialog open={open} onOpenChange={(isOpen) => !isOpen && onClose()}>
      <DialogContent className="sm:max-w-[560px] max-h-[90vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>
            {isEditing ? "Edit Coupon" : "Create Coupon"}
          </DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the coupon details below."
              : "Create a new discount coupon."}
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            {/* Code — only on create */}
            {!isEditing && (
              <div className="space-y-1.5">
                <Label htmlFor="code">Code *</Label>
                <Input
                  id="code"
                  placeholder="SUMMER20"
                  disabled={isPending}
                  className={errors.code ? "border-destructive" : ""}
                  {...field("code")}
                  onChange={(e) => {
                    setFormData((prev) => ({
                      ...prev,
                      code: e.target.value.toUpperCase(),
                    }));
                    if (errors.code)
                      setErrors((prev) => ({ ...prev, code: undefined }));
                  }}
                />
                {errors.code && (
                  <p className="text-sm text-destructive">{errors.code}</p>
                )}
              </div>
            )}

            {/* Description */}
            <div className="space-y-1.5">
              <Label htmlFor="description">Description *</Label>
              <Textarea
                id="description"
                placeholder="Short description of this coupon"
                rows={2}
                disabled={isPending}
                className={errors.description ? "border-destructive" : ""}
                {...field("description")}
              />
              {errors.description && (
                <p className="text-sm text-destructive">{errors.description}</p>
              )}
            </div>

            {/* Discount Type + Value */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label>Discount Type *</Label>
                <Select
                  value={formData.discountType}
                  onValueChange={(v) =>
                    setFormData((prev) => ({ ...prev, discountType: v }))
                  }
                  disabled={isPending}
                >
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="Percentage">Percentage (%)</SelectItem>
                    <SelectItem value="FixedAmount">
                      Fixed Amount ($)
                    </SelectItem>
                  </SelectContent>
                </Select>
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="discountValue">
                  Value {formData.discountType === "Percentage" ? "(%)" : "($)"}{" "}
                  *
                </Label>
                <Input
                  id="discountValue"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder={
                    formData.discountType === "Percentage" ? "20" : "10.00"
                  }
                  disabled={isPending}
                  className={errors.discountValue ? "border-destructive" : ""}
                  {...field("discountValue")}
                />
                {errors.discountValue && (
                  <p className="text-sm text-destructive">
                    {errors.discountValue}
                  </p>
                )}
              </div>
            </div>

            {/* Valid From + Valid Until */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="validFrom">Valid From *</Label>
                <Input
                  id="validFrom"
                  type="date"
                  disabled={isPending}
                  className={errors.validFrom ? "border-destructive" : ""}
                  {...field("validFrom")}
                />
                {errors.validFrom && (
                  <p className="text-sm text-destructive">{errors.validFrom}</p>
                )}
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="validUntil">Valid Until</Label>
                <Input
                  id="validUntil"
                  type="date"
                  disabled={isPending}
                  {...field("validUntil")}
                />
              </div>
            </div>

            {/* Optional constraints */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="minOrderAmount">Min Order Amount ($)</Label>
                <Input
                  id="minOrderAmount"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="50.00"
                  disabled={isPending}
                  {...field("minOrderAmount")}
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="maxDiscountAmount">Max Discount ($)</Label>
                <Input
                  id="maxDiscountAmount"
                  type="number"
                  min="0"
                  step="0.01"
                  placeholder="100.00"
                  disabled={isPending}
                  {...field("maxDiscountAmount")}
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1.5">
                <Label htmlFor="usageLimit">Total Usage Limit</Label>
                <Input
                  id="usageLimit"
                  type="number"
                  min="1"
                  placeholder="100"
                  disabled={isPending}
                  {...field("usageLimit")}
                />
              </div>
              <div className="space-y-1.5">
                <Label htmlFor="usagePerUser">Per-User Limit</Label>
                <Input
                  id="usagePerUser"
                  type="number"
                  min="1"
                  placeholder="1"
                  disabled={isPending}
                  {...field("usagePerUser")}
                />
              </div>
            </div>
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={onClose}
              disabled={isPending}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {isEditing ? "Update" : "Create"}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

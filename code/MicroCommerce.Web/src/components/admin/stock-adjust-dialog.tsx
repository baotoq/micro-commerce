'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Loader2 } from 'lucide-react';
import { adjustStock } from '@/lib/api';
import { toast } from 'sonner';

interface StockAdjustDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  productId: string | null;
  productName: string;
  currentStock: number;
  onAdjusted: () => void;
}

export function StockAdjustDialog({
  open,
  onOpenChange,
  productId,
  productName,
  currentStock,
  onAdjusted,
}: StockAdjustDialogProps) {
  const [adjustment, setAdjustment] = useState('');
  const [reason, setReason] = useState('');
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (open) {
      setAdjustment('');
      setReason('');
    }
  }, [open]);

  const adjustmentValue = adjustment === '' || adjustment === '-' ? 0 : parseInt(adjustment, 10);
  const isValidNumber = adjustment !== '' && adjustment !== '-' && !isNaN(adjustmentValue);
  const newQuantity = currentStock + adjustmentValue;
  const wouldBeNegative = isValidNumber && newQuantity < 0;
  const canSubmit = isValidNumber && adjustmentValue !== 0 && !wouldBeNegative;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!productId || !canSubmit) return;

    setSaving(true);
    try {
      await adjustStock(productId, {
        adjustment: adjustmentValue,
        reason: reason.trim() || undefined,
      });
      toast.success('Stock adjusted successfully');
      onAdjusted();
      onOpenChange(false);
    } catch (error) {
      toast.error(error instanceof Error ? error.message : 'Failed to adjust stock');
    } finally {
      setSaving(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Adjust Stock - {productName}</DialogTitle>
          <DialogDescription>
            Enter a positive or negative number to adjust the stock level.
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit}>
          <div className="grid gap-4 py-4">
            <div className="flex items-center justify-between rounded-lg bg-gray-50 p-3">
              <span className="text-sm text-gray-600">Current stock</span>
              <span className="text-lg font-semibold">{currentStock}</span>
            </div>

            <div className="space-y-2">
              <Label htmlFor="adjustment">Adjustment</Label>
              <Input
                id="adjustment"
                type="number"
                value={adjustment}
                onChange={(e) => setAdjustment(e.target.value)}
                placeholder="+10 or -5"
                className="text-center text-lg"
                disabled={saving}
              />
            </div>

            <div className="space-y-2">
              <Label htmlFor="reason">Reason (optional)</Label>
              <Textarea
                id="reason"
                value={reason}
                onChange={(e) => setReason(e.target.value)}
                placeholder="Reason for adjustment (optional)"
                rows={2}
                disabled={saving}
              />
            </div>

            {isValidNumber && adjustmentValue !== 0 && (
              <div
                className={`flex items-center justify-between rounded-lg p-3 ${
                  wouldBeNegative
                    ? 'bg-red-50 text-red-700'
                    : 'bg-blue-50 text-blue-700'
                }`}
              >
                <span className="text-sm">New quantity</span>
                <span className="text-lg font-semibold">
                  {newQuantity}
                  {wouldBeNegative && (
                    <span className="ml-2 text-xs font-normal">Cannot be negative</span>
                  )}
                </span>
              </div>
            )}
          </div>

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={saving}
            >
              Cancel
            </Button>
            <Button type="submit" disabled={!canSubmit || saving}>
              {saving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Adjust Stock
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

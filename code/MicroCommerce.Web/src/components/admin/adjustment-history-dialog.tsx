'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { AdjustmentDto, getAdjustmentHistory } from '@/lib/api';

interface AdjustmentHistoryDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  productId: string | null;
  productName: string;
}

export function AdjustmentHistoryDialog({
  open,
  onOpenChange,
  productId,
  productName,
}: AdjustmentHistoryDialogProps) {
  const [history, setHistory] = useState<AdjustmentDto[]>([]);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (open && productId) {
      setLoading(true);
      getAdjustmentHistory(productId)
        .then(setHistory)
        .catch((error) => {
          console.error('Failed to fetch adjustment history:', error);
          setHistory([]);
        })
        .finally(() => setLoading(false));
    }
  }, [open, productId]);

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px] max-h-[80vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle>Adjustment History - {productName}</DialogTitle>
          <DialogDescription>
            View all stock adjustments for this product.
          </DialogDescription>
        </DialogHeader>

        <div className="overflow-auto flex-1 -mx-6 px-6">
          {loading ? (
            <div className="space-y-3">
              {[...Array(4)].map((_, i) => (
                <div key={i} className="flex items-center space-x-4">
                  <Skeleton className="h-4 w-[100px]" />
                  <Skeleton className="h-4 w-[60px]" />
                  <Skeleton className="h-4 w-[60px]" />
                  <Skeleton className="h-4 w-[120px]" />
                  <Skeleton className="h-4 w-[80px]" />
                </div>
              ))}
            </div>
          ) : history.length === 0 ? (
            <div className="py-8 text-center text-gray-500">
              No adjustments recorded.
            </div>
          ) : (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Date</TableHead>
                  <TableHead className="text-right">Adjustment</TableHead>
                  <TableHead className="text-right">After</TableHead>
                  <TableHead>Reason</TableHead>
                  <TableHead>By</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {history.map((entry) => (
                  <TableRow key={entry.id}>
                    <TableCell className="text-sm whitespace-nowrap">
                      {formatDate(entry.createdAt)}
                    </TableCell>
                    <TableCell className="text-right font-mono">
                      <span
                        className={
                          entry.adjustment > 0
                            ? 'text-green-600'
                            : 'text-red-600'
                        }
                      >
                        {entry.adjustment > 0 ? '+' : ''}
                        {entry.adjustment}
                      </span>
                    </TableCell>
                    <TableCell className="text-right">
                      {entry.quantityAfter}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600 max-w-[200px] truncate">
                      {entry.reason || '-'}
                    </TableCell>
                    <TableCell className="text-sm text-gray-600">
                      {entry.adjustedBy || 'system'}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}

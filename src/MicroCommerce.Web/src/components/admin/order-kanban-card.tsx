"use client";

import { useDraggable } from "@dnd-kit/core";
import { CSS } from "@dnd-kit/utilities";
import { GripVertical } from "lucide-react";
import Link from "next/link";

import { Badge } from "@/components/ui/badge";
import { Card } from "@/components/ui/card";
import type { OrderSummaryDto } from "@/lib/api";

const STATUS_BADGE_STYLES: Record<string, string> = {
  Submitted: "bg-warning-bg text-warning-foreground border-transparent",
  Confirmed: "bg-info-bg text-info-foreground border-transparent",
  Paid: "bg-success-bg text-success-foreground border-transparent",
  Shipped: "bg-accent text-accent-foreground border-transparent",
  Delivered: "bg-success-bg text-success-foreground border-transparent",
  Failed: "bg-error-bg text-error-foreground border-transparent",
  Cancelled: "bg-muted text-muted-foreground border-transparent",
};

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function formatShortDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
}

interface OrderKanbanCardProps {
  order: OrderSummaryDto;
}

export function OrderKanbanCard({ order }: OrderKanbanCardProps) {
  const { attributes, listeners, setNodeRef, transform, isDragging } =
    useDraggable({
      id: order.id,
      data: { order },
    });

  const style = transform
    ? {
        transform: CSS.Translate.toString(transform),
      }
    : undefined;

  return (
    <Card
      ref={setNodeRef}
      style={style}
      className={`p-3 transition-shadow hover:shadow-md ${
        isDragging ? "z-50 opacity-50 shadow-lg" : ""
      }`}
    >
      <div className="flex items-start gap-2">
        <button
          className="mt-0.5 cursor-grab touch-none text-muted-foreground hover:text-foreground"
          aria-label="Drag handle"
          {...listeners}
          {...attributes}
        >
          <GripVertical className="h-4 w-4" />
        </button>

        <Link
          href={`/admin/orders/${order.id}`}
          className="min-w-0 flex-1 space-y-1.5"
        >
          <div className="flex items-center justify-between gap-2">
            <span className="truncate text-sm font-semibold text-foreground">
              {order.orderNumber}
            </span>
            <Badge
              variant="outline"
              className={`shrink-0 text-[10px] ${
                STATUS_BADGE_STYLES[order.status] ??
                "bg-muted text-muted-foreground border-transparent"
              }`}
            >
              {order.status}
            </Badge>
          </div>

          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span className="font-medium text-foreground">
              {formatCurrency(order.total)}
            </span>
            <span>
              {order.itemCount} {order.itemCount === 1 ? "item" : "items"}
            </span>
          </div>

          <div className="text-[11px] text-muted-foreground">
            {formatShortDate(order.createdAt)}
          </div>
        </Link>
      </div>
    </Card>
  );
}

export function OrderKanbanCardOverlay({ order }: OrderKanbanCardProps) {
  return (
    <Card className="w-[250px] p-3 shadow-lg">
      <div className="flex items-start gap-2">
        <GripVertical className="mt-0.5 h-4 w-4 text-muted-foreground" />
        <div className="min-w-0 flex-1 space-y-1.5">
          <div className="flex items-center justify-between gap-2">
            <span className="truncate text-sm font-semibold text-foreground">
              {order.orderNumber}
            </span>
            <Badge
              variant="outline"
              className={`shrink-0 text-[10px] ${
                STATUS_BADGE_STYLES[order.status] ??
                "bg-muted text-muted-foreground border-transparent"
              }`}
            >
              {order.status}
            </Badge>
          </div>
          <div className="flex items-center justify-between text-xs text-muted-foreground">
            <span className="font-medium text-foreground">
              {formatCurrency(order.total)}
            </span>
            <span>
              {order.itemCount} {order.itemCount === 1 ? "item" : "items"}
            </span>
          </div>
          <div className="text-[11px] text-muted-foreground">
            {formatShortDate(order.createdAt)}
          </div>
        </div>
      </div>
    </Card>
  );
}

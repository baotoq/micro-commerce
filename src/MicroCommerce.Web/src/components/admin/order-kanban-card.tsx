"use client";

import { useDraggable } from "@dnd-kit/core";
import { CSS } from "@dnd-kit/utilities";
import Link from "next/link";
import { GripVertical } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import type { OrderSummaryDto } from "@/lib/api";

const STATUS_BADGE_STYLES: Record<string, string> = {
  Submitted: "bg-yellow-100 text-yellow-800 border-yellow-200",
  Confirmed: "bg-blue-100 text-blue-800 border-blue-200",
  Paid: "bg-green-100 text-green-800 border-green-200",
  Shipped: "bg-purple-100 text-purple-800 border-purple-200",
  Delivered: "bg-green-100 text-green-800 border-green-200",
  Failed: "bg-red-100 text-red-800 border-red-200",
  Cancelled: "bg-gray-100 text-gray-800 border-gray-200",
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
    <div
      ref={setNodeRef}
      style={style}
      className={`rounded-lg border bg-white p-3 shadow-sm transition-shadow hover:shadow-md ${
        isDragging ? "z-50 opacity-50 shadow-lg" : ""
      }`}
    >
      <div className="flex items-start gap-2">
        <button
          className="mt-0.5 cursor-grab touch-none text-gray-400 hover:text-gray-600"
          aria-label="Drag handle"
          {...listeners}
          {...attributes}
        >
          <GripVertical className="h-4 w-4" />
        </button>

        <Link
          href={`/admin/orders/${order.id}`}
          className="flex-1 min-w-0 space-y-1.5"
        >
          <div className="flex items-center justify-between gap-2">
            <span className="truncate text-sm font-semibold text-gray-900">
              {order.orderNumber}
            </span>
            <Badge
              variant="outline"
              className={`shrink-0 text-[10px] ${
                STATUS_BADGE_STYLES[order.status] ??
                "bg-gray-100 text-gray-800 border-gray-200"
              }`}
            >
              {order.status}
            </Badge>
          </div>

          <div className="flex items-center justify-between text-xs text-gray-500">
            <span className="font-medium text-gray-700">
              {formatCurrency(order.total)}
            </span>
            <span>
              {order.itemCount} {order.itemCount === 1 ? "item" : "items"}
            </span>
          </div>

          <div className="text-[11px] text-gray-400">
            {formatShortDate(order.createdAt)}
          </div>
        </Link>
      </div>
    </div>
  );
}

export function OrderKanbanCardOverlay({ order }: OrderKanbanCardProps) {
  return (
    <div className="w-[250px] rounded-lg border bg-white p-3 shadow-lg">
      <div className="flex items-start gap-2">
        <GripVertical className="mt-0.5 h-4 w-4 text-gray-400" />
        <div className="flex-1 min-w-0 space-y-1.5">
          <div className="flex items-center justify-between gap-2">
            <span className="truncate text-sm font-semibold text-gray-900">
              {order.orderNumber}
            </span>
            <Badge
              variant="outline"
              className={`shrink-0 text-[10px] ${
                STATUS_BADGE_STYLES[order.status] ??
                "bg-gray-100 text-gray-800 border-gray-200"
              }`}
            >
              {order.status}
            </Badge>
          </div>
          <div className="flex items-center justify-between text-xs text-gray-500">
            <span className="font-medium text-gray-700">
              {formatCurrency(order.total)}
            </span>
            <span>
              {order.itemCount} {order.itemCount === 1 ? "item" : "items"}
            </span>
          </div>
          <div className="text-[11px] text-gray-400">
            {formatShortDate(order.createdAt)}
          </div>
        </div>
      </div>
    </div>
  );
}

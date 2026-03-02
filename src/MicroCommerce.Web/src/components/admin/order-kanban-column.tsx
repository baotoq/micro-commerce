"use client";

import { useDroppable } from "@dnd-kit/core";

import { OrderKanbanCard } from "@/components/admin/order-kanban-card";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import type { OrderSummaryDto } from "@/lib/api";

const STATUS_HEADER_STYLES: Record<string, string> = {
  Submitted: "text-warning-foreground",
  Confirmed: "text-info-foreground",
  Paid: "text-success-foreground",
  Shipped: "text-accent-foreground",
  Delivered: "text-success-foreground",
};

const STATUS_COUNT_STYLES: Record<string, string> = {
  Submitted: "bg-warning-bg text-warning-foreground",
  Confirmed: "bg-info-bg text-info-foreground",
  Paid: "bg-success-bg text-success-foreground",
  Shipped: "bg-accent text-accent-foreground",
  Delivered: "bg-success-bg text-success-foreground",
};

interface OrderKanbanColumnProps {
  status: string;
  orders: OrderSummaryDto[];
  isValidDrop: boolean;
  isDragActive: boolean;
}

export function OrderKanbanColumn({
  status,
  orders,
  isValidDrop,
  isDragActive,
}: OrderKanbanColumnProps) {
  const { isOver, setNodeRef } = useDroppable({
    id: status,
  });

  const borderClass = isOver
    ? isValidDrop
      ? "border-success bg-success-bg/50"
      : "border-destructive bg-error-bg/50"
    : isDragActive && isValidDrop
      ? "border-primary border-dashed"
      : "border-border";

  return (
    <Card
      ref={setNodeRef}
      className={`flex min-w-[250px] flex-col border-2 bg-muted/30 transition-colors ${borderClass}`}
    >
      {/* Column Header */}
      <CardHeader className="flex flex-row items-center justify-between border-b border-border px-3 py-2.5">
        <h3
          className={`text-sm font-semibold ${STATUS_HEADER_STYLES[status] ?? "text-foreground"}`}
        >
          {status}
        </h3>
        <Badge
          variant="secondary"
          className={`text-xs ${STATUS_COUNT_STYLES[status] ?? ""}`}
        >
          {orders.length}
        </Badge>
      </CardHeader>

      {/* Column Body */}
      <CardContent
        className="flex flex-1 flex-col gap-2 overflow-y-auto p-2"
        style={{ maxHeight: "calc(100vh - 250px)" }}
      >
        {orders.length === 0 ? (
          <div className="flex items-center justify-center py-8 text-xs text-muted-foreground">
            No orders
          </div>
        ) : (
          orders.map((order) => (
            <OrderKanbanCard key={order.id} order={order} />
          ))
        )}
      </CardContent>
    </Card>
  );
}

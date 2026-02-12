"use client";

import { useDroppable } from "@dnd-kit/core";

import { Badge } from "@/components/ui/badge";
import { OrderKanbanCard } from "@/components/admin/order-kanban-card";
import type { OrderSummaryDto } from "@/lib/api";

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
      ? "border-green-400 bg-green-50/50"
      : "border-red-400 bg-red-50/50"
    : isDragActive && isValidDrop
      ? "border-blue-300 border-dashed"
      : "border-gray-200";

  return (
    <div
      ref={setNodeRef}
      className={`flex min-w-[250px] flex-col rounded-lg border-2 bg-gray-50 transition-colors ${borderClass}`}
    >
      {/* Column Header */}
      <div className="flex items-center justify-between border-b border-gray-200 px-3 py-2.5">
        <h3 className="text-sm font-semibold text-gray-700">{status}</h3>
        <Badge variant="secondary" className="text-xs">
          {orders.length}
        </Badge>
      </div>

      {/* Column Body */}
      <div
        className="flex flex-1 flex-col gap-2 overflow-y-auto p-2"
        style={{ maxHeight: "calc(100vh - 250px)" }}
      >
        {orders.length === 0 ? (
          <div className="flex items-center justify-center py-8 text-xs text-gray-400">
            No orders
          </div>
        ) : (
          orders.map((order) => (
            <OrderKanbanCard key={order.id} order={order} />
          ))
        )}
      </div>
    </div>
  );
}

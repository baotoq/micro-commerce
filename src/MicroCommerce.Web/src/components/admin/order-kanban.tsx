"use client";

import { useState } from "react";
import {
  DndContext,
  DragOverlay,
  closestCorners,
  PointerSensor,
  useSensor,
  useSensors,
  type DragEndEvent,
  type DragStartEvent,
} from "@dnd-kit/core";
import { toast } from "sonner";

import { Skeleton } from "@/components/ui/skeleton";
import { OrderKanbanColumn } from "@/components/admin/order-kanban-column";
import { OrderKanbanCardOverlay } from "@/components/admin/order-kanban-card";
import { useAllOrders, useUpdateOrderStatus } from "@/hooks/use-orders";
import type { OrderSummaryDto } from "@/lib/api";

const KANBAN_STATUSES = [
  "Submitted",
  "Confirmed",
  "Paid",
  "Shipped",
  "Delivered",
] as const;

const VALID_TRANSITIONS: Record<string, string> = {
  Confirmed: "Shipped",
  Shipped: "Delivered",
};

function isValidTransition(source: string, target: string): boolean {
  return VALID_TRANSITIONS[source] === target;
}

function getValidDropTarget(status: string): string | null {
  return VALID_TRANSITIONS[status] ?? null;
}

export function OrderKanban() {
  const { data: orders, isLoading } = useAllOrders({ pageSize: 200 });
  const updateOrderStatus = useUpdateOrderStatus();
  const [activeCard, setActiveCard] = useState<OrderSummaryDto | null>(null);

  const sensors = useSensors(
    useSensor(PointerSensor, {
      activationConstraint: {
        distance: 8,
      },
    })
  );

  const ordersByStatus: Record<string, OrderSummaryDto[]> = {};
  for (const status of KANBAN_STATUSES) {
    ordersByStatus[status] = [];
  }

  if (orders?.items) {
    for (const order of orders.items) {
      if (order.status in ordersByStatus) {
        ordersByStatus[order.status].push(order);
      }
    }
  }

  function handleDragStart(event: DragStartEvent) {
    const order = event.active.data.current?.order as
      | OrderSummaryDto
      | undefined;
    setActiveCard(order ?? null);
  }

  function handleDragEnd(event: DragEndEvent) {
    setActiveCard(null);

    const { active, over } = event;
    if (!over) return;

    const sourceOrder = active.data.current?.order as
      | OrderSummaryDto
      | undefined;
    if (!sourceOrder) return;

    const sourceStatus = sourceOrder.status;
    const targetStatus = over.id as string;

    if (sourceStatus === targetStatus) return;

    if (!isValidTransition(sourceStatus, targetStatus)) {
      toast.error(
        "Invalid transition. Only Confirmed to Shipped and Shipped to Delivered are allowed."
      );
      return;
    }

    updateOrderStatus.mutate(
      { orderId: sourceOrder.id, newStatus: targetStatus },
      {
        onSuccess: () => {
          toast.success(
            `Order ${sourceOrder.orderNumber} moved to ${targetStatus}`
          );
        },
      }
    );
  }

  function handleDragCancel() {
    setActiveCard(null);
  }

  if (isLoading) {
    return (
      <div className="flex gap-4 overflow-x-auto pb-4">
        {KANBAN_STATUSES.map((status) => (
          <div
            key={status}
            className="min-w-[250px] rounded-lg border-2 border-gray-200 bg-gray-50"
          >
            <div className="border-b border-gray-200 px-3 py-2.5">
              <Skeleton className="h-4 w-20" />
            </div>
            <div className="space-y-2 p-2">
              {Array.from({ length: 3 }).map((_, i) => (
                <Skeleton key={i} className="h-[80px] w-full rounded-lg" />
              ))}
            </div>
          </div>
        ))}
      </div>
    );
  }

  const validDropTarget = activeCard
    ? getValidDropTarget(activeCard.status)
    : null;

  return (
    <DndContext
      sensors={sensors}
      collisionDetection={closestCorners}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onDragCancel={handleDragCancel}
    >
      <div className="flex gap-4 overflow-x-auto pb-4">
        {KANBAN_STATUSES.map((status) => (
          <OrderKanbanColumn
            key={status}
            status={status}
            orders={ordersByStatus[status]}
            isValidDrop={validDropTarget === status}
            isDragActive={activeCard !== null}
          />
        ))}
      </div>

      <DragOverlay>
        {activeCard ? <OrderKanbanCardOverlay order={activeCard} /> : null}
      </DragOverlay>
    </DndContext>
  );
}

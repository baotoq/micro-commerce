"use client";

import { OrderKanban } from "@/components/admin/order-kanban";

export default function AdminOrdersPage() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">
        Order Management
      </h1>
      <OrderKanban />
    </div>
  );
}

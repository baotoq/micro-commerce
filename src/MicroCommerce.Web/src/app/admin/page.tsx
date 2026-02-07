"use client";

import { OrderDashboard } from "@/components/admin/order-dashboard";

export default function AdminDashboardPage() {
  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Dashboard</h1>
      <OrderDashboard />
    </div>
  );
}

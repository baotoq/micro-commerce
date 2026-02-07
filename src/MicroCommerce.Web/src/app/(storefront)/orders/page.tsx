"use client";

import { redirect } from "next/navigation";
import { useSession } from "next-auth/react";

import { Skeleton } from "@/components/ui/skeleton";
import { OrderHistoryList } from "@/components/storefront/order-history-list";

export default function OrdersPage() {
  const { status } = useSession();

  if (status === "loading") {
    return (
      <div className="mx-auto max-w-4xl px-4 py-8">
        <Skeleton className="mb-8 h-8 w-36" />
        <div className="mb-6 flex gap-2">
          {Array.from({ length: 6 }).map((_, i) => (
            <Skeleton key={i} className="h-8 w-20 rounded-full" />
          ))}
        </div>
        <div className="space-y-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Skeleton key={i} className="h-20 rounded-xl" />
          ))}
        </div>
      </div>
    );
  }

  if (status === "unauthenticated") {
    redirect("/api/auth/signin");
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-8 text-2xl font-bold tracking-tight text-zinc-900">
        My Orders
      </h1>
      <OrderHistoryList />
    </div>
  );
}

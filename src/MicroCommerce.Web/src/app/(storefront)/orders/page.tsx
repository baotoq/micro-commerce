"use client";

import { redirect } from "next/navigation";
import { useSession } from "next-auth/react";

import { OrderHistoryList } from "@/components/storefront/order-history-list";
import { Skeleton } from "@/components/ui/skeleton";

export default function OrdersPage() {
  const { status } = useSession();

  if (status === "loading") {
    return (
      <div className="mx-auto max-w-5xl px-4 py-12 sm:px-6 lg:px-20">
        <Skeleton className="mb-8 h-8 w-36" />
        <div className="mb-6 flex gap-2">
          {[
            "skel-tab-1",
            "skel-tab-2",
            "skel-tab-3",
            "skel-tab-4",
            "skel-tab-5",
            "skel-tab-6",
          ].map((id) => (
            <Skeleton key={id} className="h-8 w-20 rounded-full" />
          ))}
        </div>
        <div className="space-y-4">
          {["skel-row-1", "skel-row-2", "skel-row-3", "skel-row-4"].map(
            (id) => (
              <Skeleton key={id} className="h-20 rounded-xl" />
            ),
          )}
        </div>
      </div>
    );
  }

  if (status === "unauthenticated") {
    redirect("/api/auth/signin");
  }

  return (
    <div className="mx-auto max-w-5xl px-4 py-12 sm:px-6 lg:px-20">
      <h1 className="mb-6 text-2xl font-bold tracking-tight text-foreground">
        Order History
      </h1>
      <OrderHistoryList />
    </div>
  );
}

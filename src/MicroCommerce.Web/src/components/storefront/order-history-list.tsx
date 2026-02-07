"use client";

import Image from "next/image";
import Link from "next/link";
import { useState } from "react";
import { Package, ChevronRight } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useOrdersByBuyer } from "@/hooks/use-orders";

const TABS = [
  "All",
  "Submitted",
  "Paid",
  "Confirmed",
  "Shipped",
  "Delivered",
] as const;

function getStatusBadgeClass(status: string): string {
  switch (status) {
    case "Submitted":
      return "bg-yellow-100 text-yellow-800";
    case "Confirmed":
      return "bg-blue-100 text-blue-800";
    case "Paid":
      return "bg-green-100 text-green-800";
    case "Shipped":
      return "bg-purple-100 text-purple-800";
    case "Delivered":
      return "bg-green-100 text-green-800";
    case "Failed":
      return "bg-red-100 text-red-800";
    case "Cancelled":
      return "bg-zinc-100 text-zinc-600";
    default:
      return "bg-zinc-100 text-zinc-600";
  }
}

function formatDate(dateString: string): string {
  return new Intl.DateTimeFormat("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  }).format(new Date(dateString));
}

function formatPrice(price: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(price);
}

const PAGE_SIZE = 10;

export function OrderHistoryList() {
  const [activeTab, setActiveTab] = useState<string>("All");
  const [page, setPage] = useState(1);

  const { data, isLoading } = useOrdersByBuyer({
    status: activeTab === "All" ? undefined : activeTab,
    page,
    pageSize: PAGE_SIZE,
  });

  function handleTabChange(tab: string) {
    setActiveTab(tab);
    setPage(1);
  }

  const totalPages = data ? Math.ceil(data.totalCount / PAGE_SIZE) : 0;

  return (
    <div className="space-y-6">
      {/* Filter tabs */}
      <div className="flex gap-2 overflow-x-auto pb-1">
        {TABS.map((tab) => (
          <button
            key={tab}
            type="button"
            onClick={() => handleTabChange(tab)}
            className={`shrink-0 rounded-full px-4 py-1.5 text-sm font-medium transition-colors ${
              activeTab === tab
                ? "bg-zinc-900 text-white"
                : "bg-zinc-100 text-zinc-600 hover:bg-zinc-200"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Order list */}
      {isLoading ? (
        <div className="space-y-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <Card key={i} className="py-4">
              <CardContent className="flex items-center gap-4">
                <div className="flex -space-x-2">
                  <Skeleton className="size-10 rounded-full" />
                  <Skeleton className="size-10 rounded-full" />
                </div>
                <div className="flex-1 space-y-2">
                  <Skeleton className="h-4 w-32" />
                  <Skeleton className="h-3 w-48" />
                </div>
                <Skeleton className="h-6 w-20 rounded-full" />
              </CardContent>
            </Card>
          ))}
        </div>
      ) : !data || data.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <Package className="mb-4 size-12 text-zinc-300" />
          <h3 className="text-lg font-semibold text-zinc-900">
            No orders yet
          </h3>
          <p className="mt-1 text-sm text-zinc-500">
            When you place an order, it will appear here.
          </p>
          <Button asChild className="mt-6 rounded-full" size="lg">
            <Link href="/">Browse Products</Link>
          </Button>
        </div>
      ) : (
        <>
          <div className="space-y-4">
            {data.items.map((order) => (
              <Link key={order.id} href={`/orders/${order.id}`}>
                <Card className="cursor-pointer py-4 transition-shadow hover:shadow-md">
                  <CardContent className="flex items-center gap-4">
                    {/* Thumbnails */}
                    <div className="flex -space-x-2">
                      {order.itemThumbnails.slice(0, 3).map((thumb, i) => (
                        <div
                          key={i}
                          className="relative size-10 shrink-0 overflow-hidden rounded-full border-2 border-white bg-zinc-100"
                        >
                          {thumb ? (
                            <Image
                              src={thumb}
                              alt=""
                              fill
                              className="object-cover"
                              sizes="40px"
                            />
                          ) : (
                            <div className="flex size-full items-center justify-center text-[8px] text-zinc-400">
                              N/A
                            </div>
                          )}
                        </div>
                      ))}
                      {order.itemCount > 3 && (
                        <div className="flex size-10 shrink-0 items-center justify-center rounded-full border-2 border-white bg-zinc-100 text-xs font-medium text-zinc-500">
                          +{order.itemCount - 3}
                        </div>
                      )}
                    </div>

                    {/* Order info */}
                    <div className="min-w-0 flex-1">
                      <div className="flex items-center gap-2">
                        <p className="truncate text-sm font-semibold text-zinc-900">
                          {order.orderNumber}
                        </p>
                        <Badge
                          className={`${getStatusBadgeClass(order.status)} border-0`}
                        >
                          {order.status}
                        </Badge>
                      </div>
                      <p className="mt-0.5 text-xs text-zinc-500">
                        {formatDate(order.createdAt)} &middot;{" "}
                        {order.itemCount}{" "}
                        {order.itemCount === 1 ? "item" : "items"} &middot;{" "}
                        {formatPrice(order.total)}
                      </p>
                    </div>

                    {/* Arrow */}
                    <ChevronRight className="size-4 shrink-0 text-zinc-400" />
                  </CardContent>
                </Card>
              </Link>
            ))}
          </div>

          {/* Pagination */}
          {totalPages > 1 && (
            <div className="flex items-center justify-center gap-4 pt-2">
              <Button
                variant="outline"
                size="sm"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
                className="rounded-full"
              >
                Previous
              </Button>
              <span className="text-sm text-zinc-500">
                Page {page} of {totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={page >= totalPages}
                onClick={() => setPage((p) => p + 1)}
                className="rounded-full"
              >
                Next
              </Button>
            </div>
          )}
        </>
      )}
    </div>
  );
}

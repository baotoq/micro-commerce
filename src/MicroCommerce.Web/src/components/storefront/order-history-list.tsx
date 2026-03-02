"use client";

import { Package } from "lucide-react";
import Link from "next/link";
import { useState } from "react";

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
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
    case "Confirmed":
    case "Processing":
      return "bg-info-bg text-info-foreground";
    case "Shipped":
    case "In Transit":
      return "bg-warning-bg text-warning-foreground";
    case "Paid":
    case "Delivered":
    case "Completed":
      return "bg-success-bg text-success-foreground";
    case "Failed":
    case "Cancelled":
      return "bg-error-bg text-error-foreground";
    default:
      return "bg-secondary text-secondary-foreground";
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
                ? "bg-primary text-primary-foreground"
                : "bg-secondary text-secondary-foreground hover:bg-muted"
            }`}
          >
            {tab}
          </button>
        ))}
      </div>

      {/* Order list */}
      {isLoading ? (
        <div className="space-y-4">
          {["skeleton-1", "skeleton-2", "skeleton-3", "skeleton-4"].map(
            (id) => (
              <Card key={id} className="py-4">
                <CardContent className="flex items-center gap-4">
                  <div className="flex-1 space-y-2">
                    <Skeleton className="h-4 w-32" />
                    <Skeleton className="h-3 w-48" />
                  </div>
                  <Skeleton className="h-6 w-20 rounded-full" />
                </CardContent>
              </Card>
            ),
          )}
        </div>
      ) : !data || data.items.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-24 text-center">
          <Package className="mb-4 size-12 text-muted-foreground/50" />
          <h3 className="text-lg font-semibold text-foreground">
            No orders yet
          </h3>
          <p className="mt-1 text-sm text-muted-foreground">
            When you place an order, it will appear here.
          </p>
          <Button asChild className="mt-6 rounded-full" size="lg">
            <Link href="/">Browse Products</Link>
          </Button>
        </div>
      ) : (
        <>
          {/* Desktop table view */}
          <div className="hidden overflow-hidden rounded-lg border border-border md:block">
            <Table>
              <TableHeader>
                <TableRow className="bg-muted hover:bg-muted">
                  <TableHead className="px-5 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    Order
                  </TableHead>
                  <TableHead className="px-5 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    Date
                  </TableHead>
                  <TableHead className="px-5 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    Status
                  </TableHead>
                  <TableHead className="px-5 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    Total
                  </TableHead>
                  <TableHead className="px-5 py-3 text-xs font-semibold uppercase tracking-wide text-muted-foreground">
                    Actions
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.items.map((order) => (
                  <TableRow key={order.id} className="hover:bg-muted/30">
                    <TableCell className="px-5 py-3.5">
                      <span className="text-sm font-medium text-primary">
                        {order.orderNumber}
                      </span>
                    </TableCell>
                    <TableCell className="px-5 py-3.5 text-sm text-foreground">
                      {formatDate(order.createdAt)}
                    </TableCell>
                    <TableCell className="px-5 py-3.5">
                      <Badge
                        className={`${getStatusBadgeClass(order.status)} border-0 text-xs font-semibold`}
                      >
                        {order.status}
                      </Badge>
                    </TableCell>
                    <TableCell className="px-5 py-3.5 text-sm font-semibold text-foreground">
                      {formatPrice(order.total)}
                    </TableCell>
                    <TableCell className="px-5 py-3.5">
                      <Link
                        href={`/orders/${order.id}`}
                        className="text-[13px] font-medium text-primary hover:underline"
                      >
                        View
                      </Link>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {/* Mobile card view */}
          <div className="space-y-3 md:hidden">
            {data.items.map((order) => (
              <Link key={order.id} href={`/orders/${order.id}`}>
                <Card className="cursor-pointer py-4 transition-shadow hover:shadow-md">
                  <CardContent className="space-y-2">
                    <div className="flex items-center justify-between">
                      <span className="text-sm font-medium text-primary">
                        {order.orderNumber}
                      </span>
                      <Badge
                        className={`${getStatusBadgeClass(order.status)} border-0 text-xs font-semibold`}
                      >
                        {order.status}
                      </Badge>
                    </div>
                    <div className="flex items-center justify-between text-sm">
                      <span className="text-muted-foreground">
                        {formatDate(order.createdAt)}
                      </span>
                      <span className="font-semibold text-foreground">
                        {formatPrice(order.total)}
                      </span>
                    </div>
                    <p className="text-xs text-muted-foreground">
                      {order.itemCount}{" "}
                      {order.itemCount === 1 ? "item" : "items"}
                    </p>
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
              <span className="text-sm text-muted-foreground">
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

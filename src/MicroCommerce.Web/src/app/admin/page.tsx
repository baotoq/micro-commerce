"use client";

import {
  ArrowDownRight,
  ArrowUpRight,
  Clock,
  DollarSign,
  Package,
  ShoppingCart,
  TrendingUp,
} from "lucide-react";
import Link from "next/link";
import { OrderDashboard } from "@/components/admin/order-dashboard";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { useOrderDashboard } from "@/hooks/use-orders";

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function StatCardSkeleton() {
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between pb-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-8 w-8 rounded-lg" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-8 w-32 mb-1" />
        <Skeleton className="h-3 w-20" />
      </CardContent>
    </Card>
  );
}

export default function AdminDashboardPage() {
  const { data: dashboard, isLoading } = useOrderDashboard("today");

  return (
    <div className="space-y-8">
      {/* Page Header */}
      <div>
        <h1 className="text-2xl font-bold text-foreground">Dashboard</h1>
        <p className="text-sm text-muted-foreground">
          Overview of your store performance
        </p>
      </div>

      {/* Summary Stat Cards */}
      {isLoading ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <StatCardSkeleton />
          <StatCardSkeleton />
          <StatCardSkeleton />
          <StatCardSkeleton />
        </div>
      ) : dashboard ? (
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Total Orders
              </CardTitle>
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-info-bg">
                <ShoppingCart className="h-4 w-4 text-info" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-foreground">
                {dashboard.totalOrders.toLocaleString()}
              </div>
              <p className="mt-1 flex items-center text-xs text-muted-foreground">
                <ArrowUpRight className="mr-1 h-3 w-3 text-success" />
                All time orders
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Revenue
              </CardTitle>
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-success-bg">
                <DollarSign className="h-4 w-4 text-success" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-foreground">
                {formatCurrency(dashboard.revenue)}
              </div>
              <p className="mt-1 flex items-center text-xs text-muted-foreground">
                <ArrowUpRight className="mr-1 h-3 w-3 text-success" />
                Total revenue
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Avg. Order Value
              </CardTitle>
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-warning-bg">
                <TrendingUp className="h-4 w-4 text-warning" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-foreground">
                {formatCurrency(dashboard.averageOrderValue)}
              </div>
              <p className="mt-1 flex items-center text-xs text-muted-foreground">
                Per order average
              </p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader className="flex flex-row items-center justify-between pb-2">
              <CardTitle className="text-sm font-medium text-muted-foreground">
                Pending Orders
              </CardTitle>
              <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-error-bg">
                <Clock className="h-4 w-4 text-error" />
              </div>
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold text-foreground">
                {dashboard.pendingOrders.toLocaleString()}
              </div>
              <p className="mt-1 flex items-center text-xs text-muted-foreground">
                {dashboard.pendingOrders > 0 ? (
                  <>
                    <ArrowDownRight className="mr-1 h-3 w-3 text-error" />
                    Needs attention
                  </>
                ) : (
                  "All caught up"
                )}
              </p>
            </CardContent>
          </Card>
        </div>
      ) : null}

      {/* Quick Links */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <Link href="/admin/products">
          <Card className="cursor-pointer transition-shadow hover:shadow-md">
            <CardHeader className="flex flex-row items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-accent">
                <Package className="h-5 w-5 text-primary" />
              </div>
              <div>
                <CardTitle className="text-sm">Products</CardTitle>
                <CardDescription className="text-xs">
                  Manage catalog
                </CardDescription>
              </div>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/admin/orders">
          <Card className="cursor-pointer transition-shadow hover:shadow-md">
            <CardHeader className="flex flex-row items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-info-bg">
                <ShoppingCart className="h-5 w-5 text-info" />
              </div>
              <div>
                <CardTitle className="text-sm">Orders</CardTitle>
                <CardDescription className="text-xs">
                  View & manage orders
                </CardDescription>
              </div>
            </CardHeader>
          </Card>
        </Link>
        <Link href="/admin/categories">
          <Card className="cursor-pointer transition-shadow hover:shadow-md">
            <CardHeader className="flex flex-row items-center gap-3">
              <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-success-bg">
                <TrendingUp className="h-5 w-5 text-success" />
              </div>
              <div>
                <CardTitle className="text-sm">Categories</CardTitle>
                <CardDescription className="text-xs">
                  Organize products
                </CardDescription>
              </div>
            </CardHeader>
          </Card>
        </Link>
      </div>

      {/* Order Dashboard (charts + recent orders) */}
      <OrderDashboard />
    </div>
  );
}

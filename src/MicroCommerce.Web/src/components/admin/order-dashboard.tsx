"use client";

import Link from "next/link";
import { useState } from "react";
import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import {
  type ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";

import { useAllOrders, useOrderDashboard } from "@/hooks/use-orders";

const chartConfig = {
  count: { label: "Orders", color: "hsl(var(--chart-1))" },
} satisfies ChartConfig;

const TIME_RANGE_OPTIONS = [
  { value: "today", label: "Today" },
  { value: "7d", label: "Last 7 Days" },
  { value: "30d", label: "Last 30 Days" },
  { value: "all", label: "All Time" },
] as const;

const STATUS_BADGE_STYLES: Record<string, string> = {
  Submitted: "bg-warning-bg text-warning-foreground border-transparent",
  Confirmed: "bg-info-bg text-info-foreground border-transparent",
  Paid: "bg-success-bg text-success-foreground border-transparent",
  Shipped: "bg-accent text-accent-foreground border-transparent",
  Delivered: "bg-success-bg text-success-foreground border-transparent",
  Failed: "bg-error-bg text-error-foreground border-transparent",
  Cancelled: "bg-muted text-muted-foreground border-transparent",
};

function formatCurrency(value: number): string {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(value);
}

function formatDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
}

function formatFullDate(dateString: string): string {
  return new Date(dateString).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
    hour: "numeric",
    minute: "2-digit",
  });
}

function ChartSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-48" />
      </CardHeader>
      <CardContent>
        <Skeleton className="h-[300px] w-full" />
      </CardContent>
    </Card>
  );
}

function TableSkeleton() {
  return (
    <Card>
      <CardHeader>
        <Skeleton className="h-5 w-32" />
      </CardHeader>
      <CardContent>
        <div className="space-y-3">
          {["s1", "s2", "s3", "s4", "s5"].map((key) => (
            <Skeleton key={key} className="h-10 w-full" />
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

export function OrderDashboard() {
  const [timeRange, setTimeRange] = useState("today");
  const { data: dashboard, isLoading: isDashboardLoading } =
    useOrderDashboard(timeRange);
  const { data: recentOrders, isLoading: isOrdersLoading } = useAllOrders({
    pageSize: 10,
  });

  return (
    <div className="space-y-6">
      {/* Time Range Selector */}
      <div className="flex justify-end">
        <Select value={timeRange} onValueChange={setTimeRange}>
          <SelectTrigger className="w-[180px]">
            <SelectValue placeholder="Select time range" />
          </SelectTrigger>
          <SelectContent>
            {TIME_RANGE_OPTIONS.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {/* Orders Per Day Chart */}
      {isDashboardLoading ? (
        <ChartSkeleton />
      ) : dashboard && dashboard.ordersPerDay.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Orders Per Day (Last 7 Days)</CardTitle>
          </CardHeader>
          <CardContent>
            <ChartContainer
              config={chartConfig}
              className="min-h-[300px] w-full"
            >
              <BarChart data={dashboard.ordersPerDay}>
                <CartesianGrid vertical={false} />
                <XAxis
                  dataKey="date"
                  tickLine={false}
                  axisLine={false}
                  tickFormatter={(value: string) => formatDate(value)}
                />
                <YAxis
                  allowDecimals={false}
                  tickLine={false}
                  axisLine={false}
                />
                <ChartTooltip
                  content={
                    <ChartTooltipContent
                      labelFormatter={(value: string) => formatDate(value)}
                    />
                  }
                />
                <Bar dataKey="count" fill="var(--color-count)" radius={4} />
              </BarChart>
            </ChartContainer>
          </CardContent>
        </Card>
      ) : !isDashboardLoading ? (
        <Card>
          <CardHeader>
            <CardTitle>Orders Per Day (Last 7 Days)</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex h-[300px] items-center justify-center text-muted-foreground">
              No orders yet
            </div>
          </CardContent>
        </Card>
      ) : null}

      {/* Recent Orders Table */}
      {isOrdersLoading ? (
        <TableSkeleton />
      ) : recentOrders && recentOrders.items.length > 0 ? (
        <Card>
          <CardHeader>
            <CardTitle>Recent Orders</CardTitle>
          </CardHeader>
          <CardContent>
            <Table>
              <TableHeader>
                <TableRow className="bg-muted/50">
                  <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
                    Order Number
                  </TableHead>
                  <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
                    Status
                  </TableHead>
                  <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
                    Total
                  </TableHead>
                  <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
                    Items
                  </TableHead>
                  <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
                    Date
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {recentOrders.items.map((order) => (
                  <TableRow key={order.id} className="hover:bg-muted/30">
                    <TableCell>
                      <Link
                        href={`/admin/orders/${order.id}`}
                        className="font-medium text-primary hover:underline"
                      >
                        {order.orderNumber}
                      </Link>
                    </TableCell>
                    <TableCell>
                      <Badge
                        variant="outline"
                        className={
                          STATUS_BADGE_STYLES[order.status] ??
                          "bg-muted text-muted-foreground border-transparent"
                        }
                      >
                        {order.status}
                      </Badge>
                    </TableCell>
                    <TableCell>{formatCurrency(order.total)}</TableCell>
                    <TableCell>{order.itemCount}</TableCell>
                    <TableCell className="text-muted-foreground">
                      {formatFullDate(order.createdAt)}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      ) : !isOrdersLoading ? (
        <Card>
          <CardHeader>
            <CardTitle>Recent Orders</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="flex h-32 items-center justify-center text-muted-foreground">
              No orders yet
            </div>
          </CardContent>
        </Card>
      ) : null}
    </div>
  );
}

"use client";

import { Percent, Plus, Search, Tag } from "lucide-react";
import { useState } from "react";

import { CouponDialog } from "@/components/admin/coupon-dialog";
import { Pagination } from "@/components/admin/pagination";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import {
  useCoupons,
  useDeleteCoupon,
  useToggleCouponStatus,
} from "@/hooks/use-coupons";
import type { CouponDto } from "@/lib/api";

const PAGE_SIZE = 20;

function formatDiscount(coupon: CouponDto): string {
  if (coupon.discountType === "Percentage") {
    return `${coupon.discountValue}%`;
  }
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(coupon.discountValue);
}

function formatDate(iso: string | null | undefined): string {
  if (!iso) return "—";
  return new Date(iso).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function CouponStatusBadge({ isActive }: { isActive: boolean }) {
  return (
    <Badge
      variant={isActive ? "default" : "secondary"}
      className={
        isActive
          ? "bg-success/15 text-success-foreground border-success/30"
          : ""
      }
    >
      {isActive ? "Active" : "Inactive"}
    </Badge>
  );
}

export default function CouponsPage() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [activeFilter, setActiveFilter] = useState<string>("all");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingCoupon, setEditingCoupon] = useState<CouponDto | null>(null);

  const isActiveParam =
    activeFilter === "active"
      ? true
      : activeFilter === "inactive"
        ? false
        : undefined;

  const { data, isLoading } = useCoupons({
    page,
    pageSize: PAGE_SIZE,
    isActive: isActiveParam,
    search: search || undefined,
  });

  const deleteCoupon = useDeleteCoupon();
  const toggleStatus = useToggleCouponStatus();

  function handleEdit(coupon: CouponDto) {
    setEditingCoupon(coupon);
    setDialogOpen(true);
  }

  function handleAddNew() {
    setEditingCoupon(null);
    setDialogOpen(true);
  }

  function handleDialogClose() {
    setDialogOpen(false);
    setEditingCoupon(null);
  }

  function handleToggleStatus(coupon: CouponDto) {
    toggleStatus.mutate({ id: coupon.id, isActive: !coupon.isActive });
  }

  function handleDelete(id: string) {
    if (!window.confirm("Delete this coupon? This action cannot be undone."))
      return;
    deleteCoupon.mutate(id);
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-foreground">Coupons</h1>
          <p className="text-muted-foreground">
            Manage discount coupons and promo codes
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Coupon
        </Button>
      </div>

      {/* Filters */}
      <div className="flex gap-3">
        <div className="relative flex-1 max-w-xs">
          <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search coupons..."
            value={search}
            onChange={(e) => {
              setSearch(e.target.value);
              setPage(1);
            }}
            className="pl-9"
          />
        </div>
        <Select
          value={activeFilter}
          onValueChange={(v) => {
            setActiveFilter(v);
            setPage(1);
          }}
        >
          <SelectTrigger className="w-36">
            <SelectValue placeholder="Status" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Status</SelectItem>
            <SelectItem value="active">Active</SelectItem>
            <SelectItem value="inactive">Inactive</SelectItem>
          </SelectContent>
        </Select>
      </div>

      {/* Table */}
      <div className="rounded-lg border bg-card">
        {isLoading ? (
          <div className="p-6 space-y-4">
            {["a", "b", "c", "d", "e"].map((k) => (
              <div key={k} className="flex items-center space-x-4">
                <Skeleton className="h-10 w-24 rounded" />
                <div className="space-y-2 flex-1">
                  <Skeleton className="h-4 w-[200px]" />
                  <Skeleton className="h-4 w-[120px]" />
                </div>
                <Skeleton className="h-8 w-16 rounded" />
              </div>
            ))}
          </div>
        ) : data && data.items.length > 0 ? (
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Code</TableHead>
                <TableHead>Discount</TableHead>
                <TableHead>Usage</TableHead>
                <TableHead>Valid Period</TableHead>
                <TableHead>Status</TableHead>
                <TableHead className="text-right">Actions</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.items.map((coupon) => (
                <TableRow key={coupon.id}>
                  <TableCell>
                    <div>
                      <div className="flex items-center gap-2">
                        {coupon.discountType === "Percentage" ? (
                          <Percent className="h-3.5 w-3.5 text-muted-foreground" />
                        ) : (
                          <Tag className="h-3.5 w-3.5 text-muted-foreground" />
                        )}
                        <span className="font-mono font-semibold text-sm">
                          {coupon.code}
                        </span>
                      </div>
                      <p className="text-xs text-muted-foreground mt-0.5 max-w-[200px] truncate">
                        {coupon.description}
                      </p>
                    </div>
                  </TableCell>
                  <TableCell>
                    <div className="font-medium">{formatDiscount(coupon)}</div>
                    {coupon.minOrderAmount && (
                      <div className="text-xs text-muted-foreground">
                        Min ${coupon.minOrderAmount}
                      </div>
                    )}
                  </TableCell>
                  <TableCell>
                    <span className="text-sm">
                      {coupon.timesUsed}
                      {coupon.usageLimit ? ` / ${coupon.usageLimit}` : ""}
                    </span>
                  </TableCell>
                  <TableCell>
                    <div className="text-sm">
                      {formatDate(coupon.validFrom)}
                    </div>
                    {coupon.validUntil && (
                      <div className="text-xs text-muted-foreground">
                        → {formatDate(coupon.validUntil)}
                      </div>
                    )}
                  </TableCell>
                  <TableCell>
                    <CouponStatusBadge isActive={coupon.isActive} />
                  </TableCell>
                  <TableCell className="text-right">
                    <div className="flex items-center justify-end gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleToggleStatus(coupon)}
                        disabled={toggleStatus.isPending}
                      >
                        {coupon.isActive ? "Deactivate" : "Activate"}
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleEdit(coupon)}
                      >
                        Edit
                      </Button>
                      <Button
                        size="sm"
                        variant="outline"
                        className="text-destructive hover:text-destructive"
                        onClick={() => handleDelete(coupon.id)}
                        disabled={deleteCoupon.isPending}
                      >
                        Delete
                      </Button>
                    </div>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        ) : (
          <div className="p-12 text-center">
            <div className="mb-4 text-muted-foreground">
              <Tag className="mx-auto h-12 w-12" />
            </div>
            <h3 className="text-lg font-medium text-foreground">
              No coupons found
            </h3>
            <p className="mt-1 text-muted-foreground">
              {search || activeFilter !== "all"
                ? "Try adjusting your filters"
                : "Get started by creating your first coupon"}
            </p>
            {!search && activeFilter === "all" && (
              <Button onClick={handleAddNew} className="mt-4">
                <Plus className="mr-2 h-4 w-4" />
                Add Coupon
              </Button>
            )}
          </div>
        )}
      </div>

      {/* Pagination */}
      {data && data.totalCount > 0 && (
        <Pagination
          page={data.page}
          pageSize={data.pageSize}
          totalCount={data.totalCount}
          onPageChange={setPage}
        />
      )}

      {/* Create/Edit Dialog */}
      <CouponDialog
        open={dialogOpen}
        onClose={handleDialogClose}
        coupon={editingCoupon}
      />
    </div>
  );
}

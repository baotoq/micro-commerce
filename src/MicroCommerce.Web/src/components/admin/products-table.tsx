"use client";

import {
  Archive,
  Eye,
  EyeOff,
  History,
  MoreHorizontal,
  Package,
  Pencil,
} from "lucide-react";
import Image from "next/image";
import { useState } from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  archiveProduct,
  changeProductStatus,
  type ProductDto,
  type StockInfoDto,
} from "@/lib/api";

interface ProductsTableProps {
  products: ProductDto[];
  stockLevels: Record<string, StockInfoDto>;
  onEdit: (product: ProductDto) => void;
  onRefresh: () => void;
  onAdjustStock: (productId: string) => void;
  onViewHistory: (productId: string) => void;
}

export function ProductsTable({
  products,
  stockLevels,
  onEdit,
  onRefresh,
  onAdjustStock,
  onViewHistory,
}: ProductsTableProps) {
  const [loading, setLoading] = useState<string | null>(null);

  const handleStatusChange = async (product: ProductDto) => {
    const newStatus = product.status === "Published" ? "Draft" : "Published";
    setLoading(product.id);
    try {
      await changeProductStatus(product.id, newStatus);
      onRefresh();
    } catch (error) {
      console.error("Failed to change status:", error);
    } finally {
      setLoading(null);
    }
  };

  const handleArchive = async (product: ProductDto) => {
    if (!confirm(`Are you sure you want to archive "${product.name}"?`)) return;
    setLoading(product.id);
    try {
      await archiveProduct(product.id);
      onRefresh();
    } catch (error) {
      console.error("Failed to archive:", error);
    } finally {
      setLoading(null);
    }
  };

  const formatPrice = (price: number, currency: string) => {
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: currency,
    }).format(price);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case "Published":
        return (
          <Badge className="bg-success-bg text-success-foreground border-transparent">
            Published
          </Badge>
        );
      case "Draft":
        return <Badge variant="secondary">Draft</Badge>;
      case "Archived":
        return (
          <Badge variant="outline" className="text-muted-foreground">
            Archived
          </Badge>
        );
      default:
        return <Badge>{status}</Badge>;
    }
  };

  const getStockBadge = (productId: string) => {
    const stock = stockLevels[productId];

    if (!stock) {
      return <span className="text-muted-foreground">-</span>;
    }

    if (stock.availableQuantity === 0) {
      return <Badge variant="destructive">Out of Stock</Badge>;
    }

    if (stock.isLowStock) {
      return (
        <Badge className="border-warning bg-warning-bg text-warning-foreground">
          Only {stock.availableQuantity} left
        </Badge>
      );
    }

    return (
      <Badge variant="secondary">In Stock ({stock.availableQuantity})</Badge>
    );
  };

  return (
    <Table>
      <TableHeader>
        <TableRow className="bg-muted/50">
          <TableHead className="w-[80px] text-xs font-semibold uppercase text-muted-foreground">
            Image
          </TableHead>
          <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
            Name
          </TableHead>
          <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
            Category
          </TableHead>
          <TableHead className="text-right text-xs font-semibold uppercase text-muted-foreground">
            Price
          </TableHead>
          <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
            Stock
          </TableHead>
          <TableHead className="text-xs font-semibold uppercase text-muted-foreground">
            Status
          </TableHead>
          <TableHead className="w-[70px] text-xs font-semibold uppercase text-muted-foreground">
            Actions
          </TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {products.map((product) => (
          <TableRow key={product.id} className="hover:bg-muted/30">
            <TableCell>
              {product.imageUrl ? (
                <Image
                  src={product.imageUrl}
                  alt={product.name}
                  width={48}
                  height={48}
                  className="rounded object-cover"
                />
              ) : (
                <div className="flex h-12 w-12 items-center justify-center rounded bg-muted text-muted-foreground">
                  <Package className="h-6 w-6" />
                </div>
              )}
            </TableCell>
            <TableCell>
              <div>
                <div className="font-medium text-foreground">
                  {product.name}
                </div>
                {product.sku && (
                  <div className="text-sm text-muted-foreground">
                    SKU: {product.sku}
                  </div>
                )}
              </div>
            </TableCell>
            <TableCell className="text-foreground">
              {product.categoryName}
            </TableCell>
            <TableCell className="text-right text-foreground">
              {formatPrice(product.price, product.priceCurrency)}
            </TableCell>
            <TableCell>
              <div className="flex items-center gap-1">
                <button
                  type="button"
                  className="cursor-pointer transition-opacity hover:opacity-80"
                  onClick={() => onAdjustStock(product.id)}
                  title="Adjust stock"
                >
                  {getStockBadge(product.id)}
                </button>
                <Button
                  variant="ghost"
                  size="icon"
                  className="h-6 w-6"
                  onClick={() => onViewHistory(product.id)}
                  title="View adjustment history"
                >
                  <History className="h-3.5 w-3.5 text-muted-foreground" />
                </Button>
              </div>
            </TableCell>
            <TableCell>{getStatusBadge(product.status)}</TableCell>
            <TableCell>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button
                    variant="ghost"
                    className="h-8 w-8 p-0"
                    disabled={loading === product.id}
                  >
                    <MoreHorizontal className="h-4 w-4" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end">
                  <DropdownMenuItem onClick={() => onEdit(product)}>
                    <Pencil className="mr-2 h-4 w-4" />
                    Edit
                  </DropdownMenuItem>
                  {product.status !== "Archived" && (
                    <DropdownMenuItem
                      onClick={() => handleStatusChange(product)}
                    >
                      {product.status === "Published" ? (
                        <>
                          <EyeOff className="mr-2 h-4 w-4" />
                          Unpublish
                        </>
                      ) : (
                        <>
                          <Eye className="mr-2 h-4 w-4" />
                          Publish
                        </>
                      )}
                    </DropdownMenuItem>
                  )}
                  {product.status !== "Archived" && (
                    <DropdownMenuItem
                      onClick={() => handleArchive(product)}
                      className="text-destructive"
                    >
                      <Archive className="mr-2 h-4 w-4" />
                      Archive
                    </DropdownMenuItem>
                  )}
                </DropdownMenuContent>
              </DropdownMenu>
            </TableCell>
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}

'use client';

import { useState } from 'react';
import Image from 'next/image';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { MoreHorizontal, Pencil, Archive, Eye, EyeOff, Package, History } from 'lucide-react';
import { ProductDto, StockInfoDto, changeProductStatus, archiveProduct } from '@/lib/api';

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
    const newStatus = product.status === 'Published' ? 'Draft' : 'Published';
    setLoading(product.id);
    try {
      await changeProductStatus(product.id, newStatus);
      onRefresh();
    } catch (error) {
      console.error('Failed to change status:', error);
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
      console.error('Failed to archive:', error);
    } finally {
      setLoading(null);
    }
  };

  const formatPrice = (price: number, currency: string) => {
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: currency,
    }).format(price);
  };

  const getStatusBadge = (status: string) => {
    switch (status) {
      case 'Published':
        return <Badge className="bg-green-100 text-green-800">Published</Badge>;
      case 'Draft':
        return <Badge variant="secondary">Draft</Badge>;
      case 'Archived':
        return <Badge variant="outline" className="text-gray-500">Archived</Badge>;
      default:
        return <Badge>{status}</Badge>;
    }
  };

  const getStockBadge = (productId: string) => {
    const stock = stockLevels[productId];

    if (!stock) {
      return <span className="text-gray-400">-</span>;
    }

    if (stock.availableQuantity === 0) {
      return (
        <Badge variant="destructive">Out of Stock</Badge>
      );
    }

    if (stock.isLowStock) {
      return (
        <Badge variant="outline" className="border-amber-300 bg-amber-50 text-amber-700">
          Only {stock.availableQuantity} left
        </Badge>
      );
    }

    return (
      <Badge variant="secondary">
        In Stock ({stock.availableQuantity})
      </Badge>
    );
  };

  return (
    <Table>
      <TableHeader>
        <TableRow>
          <TableHead className="w-[80px]">Image</TableHead>
          <TableHead>Name</TableHead>
          <TableHead>Category</TableHead>
          <TableHead className="text-right">Price</TableHead>
          <TableHead>Stock</TableHead>
          <TableHead>Status</TableHead>
          <TableHead className="w-[70px]">Actions</TableHead>
        </TableRow>
      </TableHeader>
      <TableBody>
        {products.map((product) => (
          <TableRow key={product.id}>
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
                <div className="w-12 h-12 bg-gray-100 rounded flex items-center justify-center text-gray-400">
                  <Package className="w-6 h-6" />
                </div>
              )}
            </TableCell>
            <TableCell>
              <div>
                <div className="font-medium">{product.name}</div>
                {product.sku && (
                  <div className="text-sm text-gray-500">SKU: {product.sku}</div>
                )}
              </div>
            </TableCell>
            <TableCell>{product.categoryName}</TableCell>
            <TableCell className="text-right">
              {formatPrice(product.price, product.priceCurrency)}
            </TableCell>
            <TableCell>
              <div className="flex items-center gap-1">
                <button
                  type="button"
                  className="cursor-pointer hover:opacity-80 transition-opacity"
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
                  <History className="h-3.5 w-3.5 text-gray-400" />
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
                  {product.status !== 'Archived' && (
                    <DropdownMenuItem onClick={() => handleStatusChange(product)}>
                      {product.status === 'Published' ? (
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
                  {product.status !== 'Archived' && (
                    <DropdownMenuItem
                      onClick={() => handleArchive(product)}
                      className="text-red-600"
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

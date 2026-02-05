'use client';

import { useState, useEffect, useCallback } from 'react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Plus, Package } from 'lucide-react';
import { ProductsTable } from '@/components/admin/products-table';
import { ProductFilters } from '@/components/admin/product-filters';
import { Pagination } from '@/components/admin/pagination';
import { ProductDrawer } from '@/components/admin/product-drawer';
import { StockAdjustDialog } from '@/components/admin/stock-adjust-dialog';
import { AdjustmentHistoryDialog } from '@/components/admin/adjustment-history-dialog';
import {
  getProducts,
  getCategories,
  getStockLevels,
  ProductDto,
  ProductListDto,
  CategoryDto,
  StockInfoDto,
} from '@/lib/api';

export default function ProductsPage() {
  const [products, setProducts] = useState<ProductListDto | null>(null);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [stockLevels, setStockLevels] = useState<Record<string, StockInfoDto>>({});

  // Filter state
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('all');
  const [status, setStatus] = useState('all');
  const [page, setPage] = useState(1);

  // Drawer state
  const [editingProduct, setEditingProduct] = useState<ProductDto | null>(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);

  // Stock adjust dialog state
  const [adjustProductId, setAdjustProductId] = useState<string | null>(null);
  const [isAdjustOpen, setIsAdjustOpen] = useState(false);

  // History dialog state
  const [historyProductId, setHistoryProductId] = useState<string | null>(null);
  const [isHistoryOpen, setIsHistoryOpen] = useState(false);

  const fetchStockLevels = useCallback(async (productIds: string[]) => {
    if (productIds.length === 0) return;
    try {
      const levels = await getStockLevels(productIds);
      const map: Record<string, StockInfoDto> = {};
      for (const level of levels) {
        map[level.productId] = level;
      }
      setStockLevels(map);
    } catch (error) {
      console.error('Failed to fetch stock levels:', error);
    }
  }, []);

  const fetchProducts = useCallback(async () => {
    setLoading(true);
    try {
      const data = await getProducts({
        page,
        pageSize: 20,
        categoryId: categoryId !== 'all' ? categoryId : undefined,
        status: status !== 'all' ? status : undefined,
        search: search || undefined,
      });
      setProducts(data);
      // Fetch stock levels for all products on this page
      const productIds = data.items.map((p) => p.id);
      fetchStockLevels(productIds);
    } catch (error) {
      console.error('Failed to fetch products:', error);
    } finally {
      setLoading(false);
    }
  }, [page, categoryId, status, search, fetchStockLevels]);

  const fetchCategories = useCallback(async () => {
    try {
      const data = await getCategories();
      setCategories(data);
    } catch (error) {
      console.error('Failed to fetch categories:', error);
    }
  }, []);

  useEffect(() => {
    fetchCategories();
  }, [fetchCategories]);

  useEffect(() => {
    const debounce = setTimeout(() => {
      fetchProducts();
    }, search ? 300 : 0); // Debounce search

    return () => clearTimeout(debounce);
  }, [fetchProducts, search]);

  // Reset to page 1 when filters change
  useEffect(() => {
    setPage(1);
  }, [categoryId, status, search]);

  const handleEdit = (product: ProductDto) => {
    setEditingProduct(product);
    setIsDrawerOpen(true);
  };

  const handleAddNew = () => {
    setEditingProduct(null);
    setIsDrawerOpen(true);
  };

  const handleAdjustStock = (productId: string) => {
    setAdjustProductId(productId);
    setIsAdjustOpen(true);
  };

  const handleViewHistory = (productId: string) => {
    setHistoryProductId(productId);
    setIsHistoryOpen(true);
  };

  const handleStockAdjusted = () => {
    // Refetch stock levels after adjustment
    if (products) {
      const productIds = products.items.map((p) => p.id);
      fetchStockLevels(productIds);
    }
  };

  // Helper to find product name by ID
  const getProductName = (productId: string | null) => {
    if (!productId || !products) return '';
    const product = products.items.find((p) => p.id === productId);
    return product?.name || '';
  };

  const getCurrentStock = (productId: string | null) => {
    if (!productId) return 0;
    return stockLevels[productId]?.availableQuantity ?? 0;
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Products</h1>
          <p className="text-gray-500">Manage your product catalog</p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Product
        </Button>
      </div>

      {/* Filters */}
      <ProductFilters
        categories={categories}
        search={search}
        categoryId={categoryId}
        status={status}
        onSearchChange={setSearch}
        onCategoryChange={setCategoryId}
        onStatusChange={setStatus}
      />

      {/* Table */}
      <div className="bg-white rounded-lg border">
        {loading ? (
          <div className="p-6 space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="flex items-center space-x-4">
                <Skeleton className="h-12 w-12 rounded" />
                <div className="space-y-2 flex-1">
                  <Skeleton className="h-4 w-[200px]" />
                  <Skeleton className="h-4 w-[100px]" />
                </div>
              </div>
            ))}
          </div>
        ) : products && products.items.length > 0 ? (
          <ProductsTable
            products={products.items}
            stockLevels={stockLevels}
            onEdit={handleEdit}
            onRefresh={fetchProducts}
            onAdjustStock={handleAdjustStock}
            onViewHistory={handleViewHistory}
          />
        ) : (
          <div className="p-12 text-center">
            <div className="text-gray-400 mb-4">
              <Package className="h-12 w-12 mx-auto" />
            </div>
            <h3 className="text-lg font-medium text-gray-900">No products found</h3>
            <p className="text-gray-500 mt-1">
              {search || categoryId !== 'all' || status !== 'all'
                ? 'Try adjusting your filters'
                : 'Get started by adding your first product'}
            </p>
            {!search && categoryId === 'all' && status === 'all' && (
              <Button onClick={handleAddNew} className="mt-4">
                <Plus className="mr-2 h-4 w-4" />
                Add Product
              </Button>
            )}
          </div>
        )}
      </div>

      {/* Pagination */}
      {products && products.totalCount > 0 && (
        <Pagination
          page={products.page}
          pageSize={products.pageSize}
          totalCount={products.totalCount}
          onPageChange={setPage}
        />
      )}

      {/* Drawer for create/edit */}
      <ProductDrawer
        open={isDrawerOpen}
        onClose={() => {
          setIsDrawerOpen(false);
          setEditingProduct(null);
        }}
        product={editingProduct}
        categories={categories}
        onSave={fetchProducts}
      />

      {/* Stock adjustment dialog */}
      <StockAdjustDialog
        open={isAdjustOpen}
        onOpenChange={setIsAdjustOpen}
        productId={adjustProductId}
        productName={getProductName(adjustProductId)}
        currentStock={getCurrentStock(adjustProductId)}
        onAdjusted={handleStockAdjusted}
      />

      {/* Adjustment history dialog */}
      <AdjustmentHistoryDialog
        open={isHistoryOpen}
        onOpenChange={setIsHistoryOpen}
        productId={historyProductId}
        productName={getProductName(historyProductId)}
      />
    </div>
  );
}

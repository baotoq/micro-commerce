'use client';

import { useState, useEffect, useCallback } from 'react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { Plus, Package } from 'lucide-react';
import { ProductsTable } from '@/components/admin/products-table';
import { ProductFilters } from '@/components/admin/product-filters';
import { Pagination } from '@/components/admin/pagination';
import { ProductDrawer } from '@/components/admin/product-drawer';
import { getProducts, getCategories, ProductDto, ProductListDto, CategoryDto } from '@/lib/api';

export default function ProductsPage() {
  const [products, setProducts] = useState<ProductListDto | null>(null);
  const [categories, setCategories] = useState<CategoryDto[]>([]);
  const [loading, setLoading] = useState(true);

  // Filter state
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('all');
  const [status, setStatus] = useState('all');
  const [page, setPage] = useState(1);

  // Drawer state
  const [editingProduct, setEditingProduct] = useState<ProductDto | null>(null);
  const [isDrawerOpen, setIsDrawerOpen] = useState(false);

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
    } catch (error) {
      console.error('Failed to fetch products:', error);
    } finally {
      setLoading(false);
    }
  }, [page, categoryId, status, search]);

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
            onEdit={handleEdit}
            onRefresh={fetchProducts}
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
    </div>
  );
}


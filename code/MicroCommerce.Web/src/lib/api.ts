const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000';

// Product types
export interface ProductDto {
  id: string;
  name: string;
  description: string;
  price: number;
  priceCurrency: string;
  imageUrl: string | null;
  sku: string | null;
  status: 'Draft' | 'Published' | 'Archived';
  categoryId: string;
  categoryName: string;
  createdAt: string;
  updatedAt: string | null;
}

export interface ProductListDto {
  items: ProductDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// Category types
export interface CategoryDto {
  id: string;
  name: string;
  description: string | null;
  createdAt: string;
}

// Request types
export interface GetProductsParams {
  page?: number;
  pageSize?: number;
  categoryId?: string;
  status?: string;
  search?: string;
  sortBy?: string;
  sortDirection?: string;
}

export interface CreateProductRequest {
  name: string;
  description: string;
  price: number;
  categoryId: string;
  imageUrl?: string;
  sku?: string;
}

export interface UpdateProductRequest extends CreateProductRequest {}

export interface CreateCategoryRequest {
  name: string;
  description?: string;
}

export interface UpdateCategoryRequest {
  name: string;
  description?: string;
}

// Product API functions
export async function getProducts(params: GetProductsParams = {}): Promise<ProductListDto> {
  const searchParams = new URLSearchParams();
  if (params.page) searchParams.set('page', params.page.toString());
  if (params.pageSize) searchParams.set('pageSize', params.pageSize.toString());
  if (params.categoryId) searchParams.set('categoryId', params.categoryId);
  if (params.status) searchParams.set('status', params.status);
  if (params.search) searchParams.set('search', params.search);
  if (params.sortBy) searchParams.set('sortBy', params.sortBy);
  if (params.sortDirection) searchParams.set('sortDirection', params.sortDirection);

  const response = await fetch(`${API_BASE}/api/catalog/products?${searchParams}`, {
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch products');
  }

  return response.json();
}

export async function getProductById(id: string): Promise<ProductDto> {
  const response = await fetch(`${API_BASE}/api/catalog/products/${id}`, {
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch product');
  }

  return response.json();
}

export async function createProduct(data: CreateProductRequest): Promise<{ id: string }> {
  const response = await fetch(`${API_BASE}/api/catalog/products`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to create product');
  }

  return response.json();
}

export async function updateProduct(id: string, data: UpdateProductRequest): Promise<void> {
  const response = await fetch(`${API_BASE}/api/catalog/products/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to update product');
  }
}

export async function changeProductStatus(id: string, status: string): Promise<void> {
  const response = await fetch(`${API_BASE}/api/catalog/products/${id}/status`, {
    method: 'PATCH',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ status }),
  });

  if (!response.ok) {
    throw new Error('Failed to change product status');
  }
}

export async function archiveProduct(id: string): Promise<void> {
  const response = await fetch(`${API_BASE}/api/catalog/products/${id}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    throw new Error('Failed to archive product');
  }
}

// Image upload
export async function uploadImage(file: File): Promise<string> {
  const formData = new FormData();
  formData.append('file', file);

  const response = await fetch(`${API_BASE}/api/catalog/images`, {
    method: 'POST',
    body: formData,
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to upload image');
  }

  const result = await response.json();
  return result.imageUrl;
}

// Category API functions
export async function getCategories(): Promise<CategoryDto[]> {
  const response = await fetch(`${API_BASE}/api/catalog/categories`, {
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch categories');
  }

  return response.json();
}

export async function getCategoryById(id: string): Promise<CategoryDto> {
  const response = await fetch(`${API_BASE}/api/catalog/categories/${id}`, {
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch category');
  }

  return response.json();
}

export async function createCategory(data: CreateCategoryRequest): Promise<{ id: string }> {
  const response = await fetch(`${API_BASE}/api/catalog/categories`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to create category');
  }

  return response.json();
}

export async function updateCategory(id: string, data: UpdateCategoryRequest): Promise<void> {
  const response = await fetch(`${API_BASE}/api/catalog/categories/${id}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to update category');
  }
}

export async function deleteCategory(id: string): Promise<void> {
  const response = await fetch(`${API_BASE}/api/catalog/categories/${id}`, {
    method: 'DELETE',
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to delete category');
  }
}

// Storefront API functions
export type GetStorefrontProductsParams = Omit<GetProductsParams, 'status'>;

export async function getStorefrontProducts(
  params: GetStorefrontProductsParams = {}
): Promise<ProductListDto> {
  return getProducts({ ...params, status: 'Published' });
}

// Inventory types
export interface StockInfoDto {
  productId: string;
  quantityOnHand: number;
  availableQuantity: number;
  isInStock: boolean;
  isLowStock: boolean;
}

export interface AdjustmentDto {
  id: string;
  adjustment: number;
  quantityAfter: number;
  reason: string | null;
  adjustedBy: string | null;
  createdAt: string;
}

export interface AdjustStockRequest {
  adjustment: number;
  reason?: string;
}

// Inventory API functions
export async function getStockByProductId(productId: string): Promise<StockInfoDto | null> {
  const response = await fetch(`${API_BASE}/api/inventory/stock/${productId}`, {
    cache: 'no-store',
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error('Failed to fetch stock info');
  }

  return response.json();
}

export async function getStockLevels(productIds: string[]): Promise<StockInfoDto[]> {
  if (productIds.length === 0) {
    return [];
  }

  const response = await fetch(
    `${API_BASE}/api/inventory/stock?productIds=${productIds.join(',')}`,
    { cache: 'no-store' }
  );

  if (!response.ok) {
    throw new Error('Failed to fetch stock levels');
  }

  return response.json();
}

export async function adjustStock(productId: string, data: AdjustStockRequest): Promise<void> {
  const response = await fetch(`${API_BASE}/api/inventory/stock/${productId}/adjust`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to adjust stock');
  }
}

export async function getAdjustmentHistory(productId: string): Promise<AdjustmentDto[]> {
  const response = await fetch(`${API_BASE}/api/inventory/stock/${productId}/adjustments`, {
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch adjustment history');
  }

  return response.json();
}


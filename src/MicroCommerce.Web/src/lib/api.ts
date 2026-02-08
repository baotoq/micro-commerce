const API_BASE = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5200';

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

// Cart types
export interface CartItemDto {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  imageUrl: string | null;
  quantity: number;
  lineTotal: number;
}

export interface CartDto {
  id: string;
  items: CartItemDto[];
  totalPrice: number;
  totalItems: number;
}

export interface AddToCartRequest {
  productId: string;
  productName: string;
  unitPrice: number;
  imageUrl: string | null;
  quantity: number;
}

export interface AddToCartResponse {
  isUpdate: boolean;
}

export interface UpdateCartItemRequest {
  quantity: number;
}

// Cart API functions
export async function getCart(): Promise<CartDto | null> {
  const response = await fetch(`${API_BASE}/api/cart`, {
    credentials: "include",
    cache: "no-store",
  });

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error("Failed to fetch cart");
  }

  return response.json();
}

export async function addToCart(data: AddToCartRequest): Promise<AddToCartResponse> {
  const response = await fetch(`${API_BASE}/api/cart/items`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || "Failed to add to cart");
  }

  return response.json();
}

export async function updateCartItemQuantity(itemId: string, quantity: number): Promise<void> {
  const response = await fetch(`${API_BASE}/api/cart/items/${itemId}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify({ quantity } satisfies UpdateCartItemRequest),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || "Failed to update cart item");
  }
}

export async function removeCartItem(itemId: string): Promise<void> {
  const response = await fetch(`${API_BASE}/api/cart/items/${itemId}`, {
    method: "DELETE",
    credentials: "include",
  });

  if (!response.ok) {
    throw new Error("Failed to remove cart item");
  }
}

// Dead Letter Queue types
export interface DeadLetterMessageDto {
  sequenceNumber: number;
  messageType: string;
  errorDescription: string;
  correlationId: string | null;
  enqueuedTime: string;
  queueName: string;
}

export interface DeadLetterMessagesResponse {
  messages: DeadLetterMessageDto[];
  queueNames: string[];
}

// Dead Letter Queue API functions
export async function getDeadLetterMessages(
  queueName?: string,
  maxMessages?: number
): Promise<DeadLetterMessagesResponse> {
  const searchParams = new URLSearchParams();
  if (queueName) searchParams.set('queueName', queueName);
  if (maxMessages) searchParams.set('maxMessages', maxMessages.toString());

  const response = await fetch(`${API_BASE}/api/messaging/dead-letters?${searchParams}`, {
    cache: 'no-store',
  });

  if (!response.ok) {
    throw new Error('Failed to fetch dead-letter messages');
  }

  return response.json();
}

export async function retryDeadLetterMessage(
  queueName: string,
  sequenceNumber: number
): Promise<void> {
  const response = await fetch(`${API_BASE}/api/messaging/dead-letters/retry`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ queueName, sequenceNumber }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to retry dead-letter message');
  }
}

export async function purgeDeadLetterMessages(queueName: string): Promise<number> {
  const response = await fetch(`${API_BASE}/api/messaging/dead-letters/purge`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ queueName }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || 'Failed to purge dead-letter messages');
  }

  const result = await response.json();
  return result.purgedCount;
}

// Ordering types
export interface OrderItemDto {
  id: string;
  productId: string;
  productName: string;
  unitPrice: number;
  imageUrl: string | null;
  quantity: number;
  lineTotal: number;
}

export interface ShippingAddressDto {
  name: string;
  email: string;
  street: string;
  city: string;
  state: string;
  zipCode: string;
}

export interface OrderDto {
  id: string;
  orderNumber: string;
  buyerEmail: string;
  status: string;
  shippingAddress: ShippingAddressDto;
  items: OrderItemDto[];
  subtotal: number;
  shippingCost: number;
  tax: number;
  total: number;
  createdAt: string;
  paidAt: string | null;
  failureReason: string | null;
}

export interface SubmitOrderRequest {
  email: string;
  shippingAddress: ShippingAddressDto;
  items: {
    productId: string;
    productName: string;
    unitPrice: number;
    imageUrl: string | null;
    quantity: number;
  }[];
}

export interface SubmitOrderResult {
  orderId: string;
  orderNumber: string;
}

export interface SimulatePaymentRequest {
  shouldSucceed: boolean;
}

export interface SimulatePaymentResult {
  success: boolean;
  failureReason: string | null;
}

// Ordering API functions
export async function submitOrder(data: SubmitOrderRequest): Promise<SubmitOrderResult> {
  const response = await fetch(`${API_BASE}/api/ordering/checkout`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || "Failed to submit order");
  }

  return response.json();
}

export async function simulatePayment(
  orderId: string,
  data: SimulatePaymentRequest
): Promise<SimulatePaymentResult> {
  const response = await fetch(`${API_BASE}/api/ordering/orders/${orderId}/pay`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || "Failed to process payment");
  }

  return response.json();
}

export async function getOrderById(orderId: string): Promise<OrderDto> {
  const response = await fetch(`${API_BASE}/api/ordering/orders/${orderId}`, {
    credentials: "include",
    cache: "no-store",
  });

  if (!response.ok) {
    throw new Error("Failed to fetch order");
  }

  return response.json();
}

// Order history & management types
export interface OrderSummaryDto {
  id: string;
  orderNumber: string;
  status: string;
  total: number;
  itemCount: number;
  itemThumbnails: (string | null)[];
  createdAt: string;
  failureReason: string | null;
}

export interface OrderListDto {
  items: OrderSummaryDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface OrderDashboardDto {
  totalOrders: number;
  revenue: number;
  averageOrderValue: number;
  pendingOrders: number;
  ordersPerDay: { date: string; count: number }[];
}

export interface UpdateOrderStatusRequest {
  newStatus: string;
}

// Order history & management API functions
export async function getOrdersByBuyer(params: {
  status?: string;
  page?: number;
  pageSize?: number;
}): Promise<OrderListDto> {
  const searchParams = new URLSearchParams();
  if (params.status) searchParams.set("status", params.status);
  if (params.page) searchParams.set("page", params.page.toString());
  if (params.pageSize) searchParams.set("pageSize", params.pageSize.toString());

  const response = await fetch(
    `${API_BASE}/api/ordering/orders/my?${searchParams}`,
    {
      credentials: "include",
      cache: "no-store",
    }
  );

  if (!response.ok) {
    throw new Error("Failed to fetch buyer orders");
  }

  return response.json();
}

export async function getAllOrders(params: {
  status?: string;
  page?: number;
  pageSize?: number;
}): Promise<OrderListDto> {
  const searchParams = new URLSearchParams();
  if (params.status) searchParams.set("status", params.status);
  if (params.page) searchParams.set("page", params.page.toString());
  if (params.pageSize) searchParams.set("pageSize", params.pageSize.toString());

  const response = await fetch(
    `${API_BASE}/api/ordering/orders?${searchParams}`,
    {
      cache: "no-store",
    }
  );

  if (!response.ok) {
    throw new Error("Failed to fetch orders");
  }

  return response.json();
}

export async function getOrderDashboard(
  timeRange?: string
): Promise<OrderDashboardDto> {
  const searchParams = new URLSearchParams();
  if (timeRange) searchParams.set("timeRange", timeRange);

  const response = await fetch(
    `${API_BASE}/api/ordering/dashboard?${searchParams}`,
    {
      cache: "no-store",
    }
  );

  if (!response.ok) {
    throw new Error("Failed to fetch order dashboard");
  }

  return response.json();
}

export async function updateOrderStatus(
  orderId: string,
  newStatus: string
): Promise<void> {
  const response = await fetch(
    `${API_BASE}/api/ordering/orders/${orderId}/status`,
    {
      method: "PATCH",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ newStatus } satisfies UpdateOrderStatusRequest),
    }
  );

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.detail || "Failed to update order status");
  }
}


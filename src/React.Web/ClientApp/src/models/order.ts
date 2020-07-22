export interface Order {
  subTotal: number;
  customerName: string;
  orderStatus: OrderStatus;
  orderNote: string;
  OrderItemss: OrderItem[];
}

export interface OrderItem {
  productName: string;
  price: number;
  quantity: number;
}

export enum OrderStatus {
  Pending,
  Approved,
  NotApproved,
}

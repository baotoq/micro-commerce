export interface Order {
  id: number;
  subTotal: number;
  customerName: string;
  orderStatus: OrderStatus;
  orderNote: string;
  orderItems: OrderItem[];
}

export interface OrderItem {
  id: number;
  productName: string;
  productPrice: number;
  quantity: number;
}

export enum OrderStatus {
  New,
  PaymentReceived,
  Invoiced,
  Shipping,
  Completed,
  Canceled,
  Closed
}

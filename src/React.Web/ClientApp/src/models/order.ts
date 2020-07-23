export interface Order {
  id: number;
  subTotal: number;
  customerName: string;
  orderStatus: OrderStatus;
  orderNote: string;
  OrderItems: OrderItem[];
}

export interface OrderItem {
  productName: string;
  price: number;
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

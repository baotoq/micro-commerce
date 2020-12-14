import { Product } from "./product";

export interface Cart {
  id: number;
  items: CartItem[];
}

export interface CartItem {
  id: number;
  product: Product;
  quantity: number;
}

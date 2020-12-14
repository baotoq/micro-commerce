import { createHttpClient } from "./http-client";
import { Cart } from "../models/index";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/carts";

class CartService {
  public async loadCartAsync() {
    return await httpClient.get<Cart>(`${resource}`);
  }
  public async AddToCartAsync(productId: number, quantity: number) {
    return await httpClient.post<number>(`${resource}/add-to-cart`, { productId, quantity });
  }
  public async updateQuantityAsync(cartItemId: number, quantity: number) {
    return await httpClient.put(`${resource}/update-quantity`, { cartItemId, quantity });
  }
  public async removeFromCart(cartItemId: number) {
    return await httpClient.post(`${resource}/remove-from-cart`, { cartItemId });
  }
}

export default new CartService();

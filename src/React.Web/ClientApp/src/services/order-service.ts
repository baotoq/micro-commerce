import { createHttpClient } from "./http-client";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/orders";

class OrderService {
  public async createAsync(cartId: number) {
    return await httpClient.post(`${resource}/create-order`, { cartId });
  }
}

export default new OrderService();

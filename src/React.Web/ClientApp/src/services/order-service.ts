import { createHttpClient } from "./http-client";
import { OffsetPaged, OffsetPagedQuery, Order, OrderStatus } from "../models";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/orders";

export interface FindOrdersQuery extends OffsetPagedQuery {
  queryString?: string;
}

class OrderService {
  public async findAsync(query?: FindOrdersQuery) {
    const { data } = await httpClient.get<OffsetPaged<Order>>(resource, { params: query });
    return data;
  }
  public async createAsync(cartId: number) {
    return await httpClient.post(`${resource}/create-order`, { cartId });
  }
  public async changeOrderStatusAsync(cartId: number, orderStatus: OrderStatus) {
    return await httpClient.put(`${resource}/${cartId}/change-status`, { orderStatus });
  }
  public async deleteAsync(id: number) {
    return await httpClient.delete(`${resource}/${id}`);
  }
}

export default new OrderService();

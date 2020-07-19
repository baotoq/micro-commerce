import { createHttpClient } from "./http-client";
import { OffsetPaged, OffsetPagedQuery } from "../models/index";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);

export interface ProductResponse {
  id: number;
  name: string;
  price: number;
  description: string;
  imageUri: string;
  ratingAverage: number;
  categories: [{ id: number; name: string }];
}

export interface FindProductsQuery extends OffsetPagedQuery {
  queryString?: string;
}

const resource = "/api/products";

class ProductService {
  public async findByIdAsync(id: number) {
    const { data } = await httpClient.get<ProductResponse>(`${resource}/${id}`);
    return data;
  }
  public async findAsync(query?: FindProductsQuery) {
    const { data } = await httpClient.get<OffsetPaged<ProductResponse>>(resource, { params: query });
    return data;
  }
  public async createAsync(name: string) {
    await httpClient.post(`${resource}`, { name });
  }
  public async updateAsync(id: number, name: string) {
    await httpClient.put(`${resource}/${id}`, { name });
  }
  public async deleteAsync(id: number) {
    await httpClient.delete(`${resource}/${id}`);
  }
}

export default new ProductService();

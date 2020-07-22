import { createHttpClient } from "./http-client";
import { OffsetPaged, OffsetPagedQuery, Category, Product } from "../models/index";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/categories";

export interface FindCategoriesQuery extends OffsetPagedQuery {
  queryString?: string;
}

class CategoryService {
  public async findProductsByCategoryIdAsync(id: number, page: number, pageSize: number) {
    const params = {
      page,
      pageSize,
    };
    const { data } = await httpClient.get<OffsetPaged<Product>>(`${resource}/${id}/products`, { params });
    return data;
  }
  public async findAsync(query?: FindCategoriesQuery) {
    const { data } = await httpClient.get<OffsetPaged<Category>>(resource, { params: query });
    return data;
  }
  public async createAsync(name: string) {
    return await httpClient.post(`${resource}`, { name });
  }
  public async updateAsync(id: number, name: string) {
    return await httpClient.put(`${resource}/${id}`, { name });
  }
  public async deleteAsync(id: number) {
    return await httpClient.delete(`${resource}/${id}`);
  }
}

export default new CategoryService();

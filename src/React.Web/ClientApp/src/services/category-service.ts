import { createHttpClient } from "./http-client";
import { OffsetPaged, OffsetPagedQuery } from "../models/index";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/categories";

export interface CategoryResponse {
  id: number;
  name: string;
  products: ProductResponse[];
}

export interface ProductResponse {
  id: number;
  name: string;
  price: number;
  description: string;
  imageUri: string;
}

export interface FindCategoriesQuery extends OffsetPagedQuery {
  queryString?: string;
}

class CategoryService {
  public async findProductsByCategoryIdAsync(id: number, page: number, pageSize: number) {
    const params = {
      page,
      pageSize,
    };
    const { data } = await httpClient.get<OffsetPaged<ProductResponse>>(`${resource}/${id}/products`, { params });
    return data;
  }
  public async findAsync(query?: FindCategoriesQuery) {
    const { data } = await httpClient.get<OffsetPaged<CategoryResponse>>(resource, { params: query });
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

export default new CategoryService();

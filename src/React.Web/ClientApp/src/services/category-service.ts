import { httpClient } from "./http-client";
import { CursorPaged, OffsetPaged } from "../models/index";

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

class CategoryService {
  public async findProductsByCategoryIdAsync(id: number, page: number, pageSize: number) {
    const params = {
      page,
      pageSize
    };
    const { data } = await httpClient.get<OffsetPaged<ProductResponse>>(`/api/categories/${id}/products`, { params });
    return data;
  }

  public async findAllAsync() {
    const { data } = await httpClient.get<CursorPaged<CategoryResponse>>("/api/categories");
    return data;
  }
}

export default new CategoryService();

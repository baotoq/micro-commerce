import { httpClient } from "./http-client";

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

export interface OffsetPaged<T> {
  data: T[],
  totalPages: number
  totalCount: number,
}

export interface CursorPaged<T> {
  data: T[],
  previousPageToken: number
  nextPageToken: number
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

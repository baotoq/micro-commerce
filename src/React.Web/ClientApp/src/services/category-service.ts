import { httpClient } from "./http-client";

export interface CategoryResponse {
  id: number;
  name: string;
}

class CategoryService {
  public async find(id: number) {
    const { data } = await httpClient.get<CategoryResponse>(`/api/categories/${id}`);
    return data;
  }

  public async findAll() {
    const { data } = await httpClient.get<CategoryResponse[]>("/api/categories");
    return data;
  }
}

export default new CategoryService();

import { httpClient } from "./http-client";

export interface ProductResponse {
  id: number;
  name: string;
  price: number;
  description: string;
  imageFileName: string;
  categories: [{ id: number, name: string }]
}

class ProductService {
  public async findAsync(id: number) {
    const { data } = await httpClient.get<ProductResponse>(`/api/products/${id}`);
    return data;
  }

  public async findAllAsync() {
    const { data } = await httpClient.get<ProductResponse[]>("/api/products");
    return data;
  }
}

export default new ProductService();

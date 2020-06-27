import { httpClient } from "./http-client";
import { OffsetPaged } from "../models/index";

export interface ProductResponse {
  id: number;
  name: string;
  price: number;
  description: string;
  imageUri: string;
  categories: [{ id: number, name: string }]
}

class ProductService {
  public async findAsync(id: number) {
    const { data } = await httpClient.get<ProductResponse>(`/api/products/${id}`);
    return data;
  }

  public async findAllAsync() {
    const { data } = await httpClient.get<OffsetPaged<ProductResponse>>("/api/products");
    return data;
  }
}

export default new ProductService();

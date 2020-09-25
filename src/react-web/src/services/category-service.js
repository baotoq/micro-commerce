import { createHttpClient } from "./http-client";

const httpClient = createHttpClient(process.env.REACT_APP_CATALOG_URI);
const resource = "/api/categories";

class CategoryService {
  async findProductsByCategoryIdAsync(id, page, pageSize) {
    const params = {
      page,
      pageSize,
    };
    const { data } = await httpClient.get(`${resource}/${id}/products`, {
      params,
    });
    return data;
  }
  async findAsync(query) {
    const { data } = await httpClient.get(resource, { params: query });
    return data;
  }
  async createAsync(name) {
    return await httpClient.post(`${resource}`, { name });
  }
  async updateAsync(id, name) {
    return await httpClient.put(`${resource}/${id}`, { name });
  }
  async deleteAsync(id) {
    return await httpClient.delete(`${resource}/${id}`);
  }
}

export default new CategoryService();

import { createHttpClient } from "./http-client";
import { OffsetPaged, User, OffsetPagedQuery } from "../models";

const httpClient = createHttpClient(process.env.REACT_APP_IDENTITY_URI);

export interface FindUsersQuery extends OffsetPagedQuery {
  queryString?: string;
}

class UserService {
  public async findAsync(query: FindUsersQuery) {
    const params = query;
    const { data } = await httpClient.get<OffsetPaged<User>>("/api/users", { params });
    return data;
  }
  public async updateAsync(userId: string, roleId: string) {
    await httpClient.put(`/api/users/${userId}/role/${roleId}`);
  }
  public async deleteAsync(id: string) {
    await httpClient.delete(`/api/users/${id}`);
  }
}

export default new UserService();

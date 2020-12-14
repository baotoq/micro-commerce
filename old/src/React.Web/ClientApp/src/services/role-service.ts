import { createHttpClient } from "./http-client";
import { Role } from "../models";

const httpClient = createHttpClient(process.env.REACT_APP_IDENTITY_URI);

class RoleService {
  public async findAllAsync() {
    const { data } = await httpClient.get<Role[]>("/api/roles");
    return data;
  }
}

export default new RoleService();

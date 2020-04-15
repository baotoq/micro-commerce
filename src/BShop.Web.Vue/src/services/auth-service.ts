import { httpClient } from "./http-client";

export interface LoginRequest {
  userName: string;
  password: string;
}

export interface RegisterRequest {
  userName: string;
  email: string;
  password: string;
}

export class AuthService {
  public async login(request: LoginRequest) {
    const { data } = await httpClient.post<string>("/login", request);
    return data;
  }

  public async register(request: RegisterRequest) {
    const { data } = await httpClient.post<string>("/register", request);
    return data;
  }
}

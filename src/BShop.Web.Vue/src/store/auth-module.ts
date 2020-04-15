import { Module, VuexModule, Mutation, Action } from "vuex-module-decorators";
import { AuthService } from "../services/auth-service";

const authService = new AuthService();

@Module({ namespaced: true })
export class AuthModule extends VuexModule {
  userName?: string = undefined;

  @Mutation
  loginSuccess({ token }: { token: string }) {
    console.log(`Login success ${token}`);
  }

  @Action({ commit: "loginSuccess" })
  async login(userName: string, password: string) {
    const token = await authService.login({ userName, password });
    return { token };
  }
}

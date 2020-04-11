import { Module, VuexModule, Mutation, Action } from "vuex-module-decorators";
import { LoginPayload } from "./types";

@Module({ namespaced: true })
export default class Auth extends VuexModule {
  userName?: string = undefined;

  @Mutation
  loginSuccess({ token }: { token: string }) {
    console.log(`Login success ${token}`);
  }

  @Action({ commit: "loginSuccess" })
  login({ userName, password }: LoginPayload) {
    console.log(userName);
    console.log(password);

    return { token: "12312312312" };
  }
}

import Vue from "vue";
import Vuex, { StoreOptions } from "vuex";
import { RootState } from "./types";

import { AuthModule } from "./auth-module";
import { CatalogModule } from "./catalog-module";

Vue.use(Vuex);

const store: StoreOptions<RootState> = {
  state: {
    version: "1.0.0"
  },
  modules: {
    auth: AuthModule,
    catalog: CatalogModule
  }
};

export default new Vuex.Store<RootState>(store);

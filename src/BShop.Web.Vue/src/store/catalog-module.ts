import { Module, VuexModule, Mutation, Action } from "vuex-module-decorators";

import { CatalogService } from "../services/catalog-service";

const catalogService = new CatalogService();

export interface Catalog {
  id: number;
  name: string;
}

@Module({ namespaced: true })
export class CatalogModule extends VuexModule {
  catalogs: Catalog[] = [];

  @Mutation
  setCatalogs(payload: Catalog[]) {
    this.catalogs = payload;
  }

  @Action({ commit: "setCatalogs" })
  async fetchAll() {
    const response = await catalogService.findAll();
    return response;
  }
}

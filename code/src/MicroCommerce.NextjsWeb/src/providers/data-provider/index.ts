"use client";

import dataProviderSimpleRest from "@refinedev/simple-rest";
import { DataProvider } from "@refinedev/core";

const API_URL2 = "https://api.fake-rest.refine.dev";
const API_URL = "https://localhost:7510/api";

export const dataProvider = dataProviderSimpleRest(API_URL2);

export const categoryDataProvider: DataProvider = {
  getOne: async ({ id, resource }) => {
    debugger
    const response = await fetch(`${API_URL}/${resource}/${id}`);
    const data = await response.json();

    return { data };
  },

  create: async () => {
    throw new Error("Not implemented");
  },
  update: async () => {
    throw new Error("Not implemented");
  },
  deleteOne: async () => {
    throw new Error("Not implemented");
  },
  getList: async ({ resource, pagination, filters, sorters, meta }) => {
    const response = await fetch(`${API_URL}/${resource}`);

    if (response.status < 200 || response.status > 299) throw response;

    const data = await response.json();

    return {
      data,
      total: 0,
    };
  },
  getApiUrl: () => API_URL,
};

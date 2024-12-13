"use client";

import { DataProvider } from "@refinedev/core";

const API_URL = "https://localhost:7477/api";

export const productDataProvider: DataProvider = {
  getOne: async ({ id, resource }) => {
    const response = await fetch(`${API_URL}/${resource}/${id}`);
    const data = await response.json();

    return {
      data,
    };
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
  getList: async ({ resource }) => {
    const response = await fetch(`${API_URL}/${resource}`);
    const { data } = await response.json();

    return {
      data,
      total: data.length,
    };
  },
  getApiUrl: () => API_URL,
};

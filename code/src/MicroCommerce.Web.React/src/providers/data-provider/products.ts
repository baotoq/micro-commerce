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

  create: async ({ resource, variables }) => {
    console.log(variables, JSON.stringify(variables));
    const response = await fetch(`${API_URL}/${resource}`, {
      method: "POST",
      body: JSON.stringify(variables),
      headers: {
        "Content-Type": "application/json",
      },
    });
    const data = await response.json();

    return {
      data,
    };
  },
  update: async ({ resource, id, variables }) => {
    console.log(variables, JSON.stringify(variables));
    const response = await fetch(`${API_URL}/${resource}/${id}`, {
      method: "PUT",
      body: JSON.stringify(variables),
      headers: {
        "Content-Type": "application/json",
      },
    });
    const data = await response.json();

    return {
      data,
    };
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

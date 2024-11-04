"use client";

import dataProviderSimpleRest from "@refinedev/simple-rest";
import { DataProvider } from "@refinedev/core";

const API_URL2 = "https://api.fake-rest.refine.dev";
const API_URL = "https://localhost:7510/api";

export const sampleDataProvider = dataProviderSimpleRest(API_URL2);

export const defaultDataProvider: DataProvider = {
  ...dataProviderSimpleRest(API_URL),
};

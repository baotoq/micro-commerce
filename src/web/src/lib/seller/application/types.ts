// web/src/lib/seller/application/types.ts
export type SellerApplication = {
  shopName: string;
  domain: string;
  available: boolean;
  category: string;
  categories: string[];
  stepsLeft: number;
};

export type SetupStep = {
  label: string;
  status: "done" | "active" | "pending";
};

export type LaunchTask = {
  label: string;
  subject: string;
  done: boolean;
  hint?: string;
};

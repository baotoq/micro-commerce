import { z } from "zod";
import { apiFetch } from "@/lib/api";

// Mirrors PickupAddress in the API.
export type PickupAddress = {
  street: string;
  ward: string;
  district: string;
  province: string;
};

// Mirrors MerchantResponse in the API.
export type Merchant = {
  id: string;
  shopName: string;
  description: string | null;
  pickupAddress: PickupAddress;
  createdAt: string;
};

// Mirrors MeResponse in the API.
export type Me = {
  id: string;
  email: string | null;
  displayName: string;
  isPlatformOperator: boolean;
  merchant: Merchant | null;
};

// Server-side: the caller passes the session's access token. The first call creates the Account.
export function getMe(accessToken: string) {
  return apiFetch<Me>("/me", { headers: { Authorization: `Bearer ${accessToken}` } });
}

const requiredText = (label: string, max: number) =>
  z
    .string()
    .trim()
    .min(1, `${label} is required`)
    .max(max, `${label} must be ${max} characters or fewer`);

// Mirrors OpenMerchantValidator / UpdateShopValidator in the API; the API stays the source of truth.
export const shopSchema = z.object({
  shopName: requiredText("Shop name", 100),
  description: z.string().trim().max(2000, "Description must be 2000 characters or fewer"),
  pickupAddress: z.object({
    street: requiredText("Street", 200),
    ward: requiredText("Ward", 100),
    district: requiredText("District", 100),
    province: requiredText("Province", 100),
  }),
});

export type ShopInput = z.infer<typeof shopSchema>;

// Browser-side, through the /api proxy, as the signed-in Account.
export function openMerchant({ shopName, pickupAddress }: ShopInput) {
  return apiFetch<Merchant>("/me/merchant", {
    method: "POST",
    body: JSON.stringify({ shopName, pickupAddress }),
  });
}

export function updateShop({ shopName, description, pickupAddress }: ShopInput) {
  return apiFetch<Merchant>("/merchant/shop", {
    method: "PUT",
    body: JSON.stringify({ shopName, description: description || null, pickupAddress }),
  });
}

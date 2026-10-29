"use server";

import { redirect } from "next/navigation";
import { ApiError, errorMessage } from "@/lib/api";
import { placeCheckout, type CheckoutConflict, type DeliveryAddress } from "@/lib/checkout";
import { getAccessToken, signInUrl } from "@/lib/session";

export type CheckoutActionState = {
  error?: string;
  lines?: CheckoutConflict["lines"];
};

export async function placeCheckoutAction(
  _previousState: CheckoutActionState,
  formData: FormData,
): Promise<CheckoutActionState> {
  const accessToken = await getAccessToken();
  if (!accessToken) redirect(signInUrl("/checkout"));

  const itemsTotal = Number(formData.get("itemsTotal"));
  const deliveryAddress: DeliveryAddress = {
    recipientName: String(formData.get("recipientName") ?? ""),
    phone: String(formData.get("phone") ?? ""),
    street: String(formData.get("street") ?? ""),
    ward: String(formData.get("ward") ?? ""),
    province: String(formData.get("province") ?? ""),
  };
  if (!Number.isSafeInteger(itemsTotal) || itemsTotal < 0) {
    return { error: "Review the Cart total and try again." };
  }

  try {
    await placeCheckout(deliveryAddress, itemsTotal, accessToken);
  } catch (error) {
    if (error instanceof ApiError && error.status === 409) {
      return {
        error: error.problem?.detail ?? errorMessage(error),
        lines: error.problem?.lines,
      };
    }
    return { error: errorMessage(error) };
  }

  redirect("/orders");
}

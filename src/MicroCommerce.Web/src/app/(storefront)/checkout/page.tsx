import type { Metadata } from "next";

import { CheckoutPage } from "@/components/storefront/checkout-page";

export const metadata: Metadata = {
  title: "Checkout | MicroCommerce",
};

export default function CheckoutRoute() {
  return <CheckoutPage />;
}

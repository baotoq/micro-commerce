import type { Metadata } from "next";

import { CartPage } from "@/components/storefront/cart-page";

export const metadata: Metadata = {
  title: "Cart | MicroCommerce",
};

export default function CartRoute() {
  return <CartPage />;
}

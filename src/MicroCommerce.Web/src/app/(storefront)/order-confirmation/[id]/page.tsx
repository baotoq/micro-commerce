import type { Metadata } from "next";

import { OrderConfirmation } from "@/components/storefront/order-confirmation";

export const metadata: Metadata = {
  title: "Order Confirmation | MicroCommerce",
};

export default async function OrderConfirmationRoute({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return <OrderConfirmation orderId={id} />;
}

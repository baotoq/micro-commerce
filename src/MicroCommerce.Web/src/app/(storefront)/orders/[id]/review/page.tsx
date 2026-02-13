"use client";

import { OrderReviewPage } from "@/components/storefront/order-review-page";

interface PageProps {
  params: { id: string };
}

export default function OrderReviewProductsPage({ params }: PageProps) {
  return <OrderReviewPage orderId={params.id} />;
}

"use client";

import { ChevronRight } from "lucide-react";
import Link from "next/link";
import { useParams } from "next/navigation";

import { OrderDetail } from "@/components/storefront/order-detail";

export default function OrderDetailPage() {
  const params = useParams<{ id: string }>();
  const orderId = params.id;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      {/* Breadcrumb */}
      <nav className="mb-6 flex items-center gap-1 text-sm text-muted-foreground">
        <Link
          href="/orders"
          className="transition-colors hover:text-foreground"
        >
          My Orders
        </Link>
        <ChevronRight className="size-3.5" />
        <span className="text-foreground">Order Details</span>
      </nav>

      <OrderDetail orderId={orderId} />
    </div>
  );
}

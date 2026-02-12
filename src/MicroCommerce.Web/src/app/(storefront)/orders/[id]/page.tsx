"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import { ChevronRight } from "lucide-react";

import { OrderDetail } from "@/components/storefront/order-detail";

export default function OrderDetailPage() {
  const params = useParams<{ id: string }>();
  const orderId = params.id;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      {/* Breadcrumb */}
      <nav className="mb-6 flex items-center gap-1 text-sm text-zinc-500">
        <Link
          href="/orders"
          className="transition-colors hover:text-zinc-900"
        >
          My Orders
        </Link>
        <ChevronRight className="size-3.5" />
        <span className="text-zinc-900">Order Details</span>
      </nav>

      <OrderDetail orderId={orderId} />
    </div>
  );
}

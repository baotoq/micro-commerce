"use client";

import { useRouter } from "next/navigation";
import { createProduct } from "@/lib/catalog";
import { type LeafCategory, ProductForm } from "../product-form";

export function NewProductForm({ categories }: { categories: LeafCategory[] }) {
  const router = useRouter();

  return (
    <ProductForm
      categories={categories}
      submitLabel="Create Product"
      onSubmit={async (input) => {
        await createProduct(input);
        router.push("/merchant/products");
        router.refresh();
      }}
      onCancel={() => router.push("/merchant/products")}
    />
  );
}

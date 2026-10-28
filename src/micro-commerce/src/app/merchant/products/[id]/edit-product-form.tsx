"use client";

import { useRouter } from "next/navigation";
import { type Product, updateProduct } from "@/lib/catalog";
import { type LeafCategory, ProductForm } from "../product-form";

export function EditProductForm({ product, categories }: { product: Product; categories: LeafCategory[] }) {
  const router = useRouter();
  // Products have a single default Variant until they get options.
  const [variant] = product.variants;

  return (
    <ProductForm
      categories={categories}
      defaultValues={{
        name: product.name,
        description: product.description ?? "",
        categoryId: product.categoryId,
        price: variant.price,
        stock: variant.stock,
        weightGrams: variant.weightGrams,
      }}
      submitLabel="Save"
      onSubmit={async (input) => {
        await updateProduct(product.id, input);
        router.push("/merchant/products");
        router.refresh();
      }}
      onCancel={() => router.push("/merchant/products")}
    />
  );
}

"use client";

import { useRouter } from "next/navigation";
import { type Product, updateProduct } from "@/lib/catalog";
import { type LeafCategory, ProductForm } from "../product-form";

export function EditProductForm({ product, categories }: { product: Product; categories: LeafCategory[] }) {
  const router = useRouter();

  return (
    <ProductForm
      categories={categories}
      defaultValues={{
        name: product.name,
        description: product.description ?? "",
        categoryId: product.categoryId,
        options: product.options.map((option) => ({ name: option.name, values: option.values.join(", ") })),
        // The API keeps the Variant IDs of the combinations an edit keeps, so the form doesn't send them.
        variants: product.variants.map(({ optionValues, price, stock, weightGrams }) => ({
          optionValues,
          price,
          stock,
          weightGrams,
        })),
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

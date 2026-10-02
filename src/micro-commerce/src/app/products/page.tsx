import { dehydrate, HydrationBoundary } from "@tanstack/react-query";
import { connection } from "next/server";
import { productsQueryOptions } from "@/lib/products";
import { getQueryClient } from "@/lib/query-client";
import { CreateProductForm } from "./create-product-form";
import { ProductList } from "./product-list";

export default async function ProductsPage() {
  // Render at request time: API_URL doesn't exist during `next build`.
  await connection();

  const queryClient = getQueryClient();
  await queryClient.prefetchQuery(productsQueryOptions);

  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-16">
      <h1 className="mb-6 text-2xl font-semibold">Products</h1>
      <HydrationBoundary state={dehydrate(queryClient)}>
        <CreateProductForm />
        <ProductList />
      </HydrationBoundary>
    </main>
  );
}

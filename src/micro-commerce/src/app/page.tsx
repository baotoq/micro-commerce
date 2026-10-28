import { ProductGrid } from "@/components/product-grid";
import { listStorefrontProducts, pageNumber } from "@/lib/storefront";

// The storefront home: every Merchant's Published Products, newest first.
export default async function Home({ searchParams }: PageProps<"/">) {
  const page = pageNumber((await searchParams).page);
  const products = await listStorefrontProducts({ page });

  return (
    <main className="mx-auto w-full max-w-5xl px-6 py-10">
      <h1 className="mb-6 text-2xl font-semibold">Shop the Marketplace</h1>
      <ProductGrid products={products} href={(p) => (p === 1 ? "/" : `/?page=${p}`)} />
    </main>
  );
}

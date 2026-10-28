import { notFound } from "next/navigation";
import { ProductGrid } from "@/components/product-grid";
import { ApiError } from "@/lib/api";
import { getShop, listStorefrontProducts, pageNumber } from "@/lib/storefront";

// A Merchant's Shop page: its name, description and Published Products.
export default async function ShopPage({ params, searchParams }: PageProps<"/shops/[id]">) {
  const { id } = await params;
  const page = pageNumber((await searchParams).page);
  const [shop, products] = await Promise.all([
    getShop(id).catch((error) => {
      if (error instanceof ApiError && error.status === 404) notFound();
      throw error;
    }),
    listStorefrontProducts({ page, shopId: id }),
  ]);

  return (
    <main className="mx-auto w-full max-w-5xl px-6 py-10">
      <h1 className="text-2xl font-semibold">{shop.shopName}</h1>
      {shop.description && <p className="mt-2 whitespace-pre-line text-muted-foreground">{shop.description}</p>}
      <h2 className="mt-8 mb-4 font-semibold">Products</h2>
      <ProductGrid
        products={products}
        showShop={false}
        href={(p) => (p === 1 ? `/shops/${id}` : `/shops/${id}?page=${p}`)}
      />
    </main>
  );
}

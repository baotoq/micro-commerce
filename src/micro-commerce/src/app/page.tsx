import { ProductGrid } from "@/components/product-grid";
import { StorefrontSearch } from "@/components/storefront-search";
import { allCategories, getCategoryTree } from "@/lib/catalog";
import { idParam, listStorefrontProducts, pageNumber, searchParam, searchQueryMaxLength } from "@/lib/storefront";

// The storefront home: every Merchant's Published Products, newest first, or those matching a search
// (`q`) and Category (`category`, including the Categories below it), best matches first.
export default async function Home({ searchParams }: PageProps<"/">) {
  const params = await searchParams;
  const page = pageNumber(params.page);
  const query = searchParam(params.q)?.slice(0, searchQueryMaxLength);
  const categoryId = idParam(params.category);
  const [products, tree] = await Promise.all([
    listStorefrontProducts({ page, query, categoryId }),
    getCategoryTree(),
  ]);

  const href = (p: number) => {
    const next = new URLSearchParams();
    if (query) next.set("q", query);
    if (categoryId) next.set("category", categoryId);
    if (p > 1) next.set("page", String(p));
    return next.size > 0 ? `/?${next}` : "/";
  };
  const searching = query !== undefined || categoryId !== undefined;

  return (
    <main className="mx-auto w-full max-w-5xl px-6 py-10">
      <h1 className="mb-6 text-2xl font-semibold">Shop the Marketplace</h1>
      <div className="mb-8">
        <StorefrontSearch query={query} categoryId={categoryId} categories={allCategories(tree)} />
      </div>
      <ProductGrid
        products={products}
        href={href}
        empty={searching ? "No Products match your search." : "No Products yet."}
      />
    </main>
  );
}

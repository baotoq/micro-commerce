import Link from "next/link";
import { ProductCard } from "@/components/storefront/product-card";
import { getShopCategories, getShopProducts } from "@/lib/storefront/data";

type Search = { category?: string; sort?: string; q?: string; page?: string };

/**
 * Single source of truth for every shop link. Seeds from the active params and
 * applies overrides, so sort/pagination/category links all preserve the rest of
 * the filter state instead of silently dropping `q`, `sort`, or `page`.
 * An override of `undefined` clears that param; `null` keeps the current value.
 */
function shopHref(params: Search, overrides: Partial<Search> = {}) {
  const merged: Search = { ...params, ...overrides };
  const next = new URLSearchParams();
  if (merged.category) next.set("category", merged.category);
  if (merged.sort) next.set("sort", merged.sort);
  if (merged.q) next.set("q", merged.q);
  if (merged.page && merged.page !== "1") next.set("page", merged.page);
  const qs = next.toString();
  return qs ? `/?${qs}` : "/";
}

/** Category chip: switch the category and reset to page 1, keeping sort + q. */
function chipHref(params: Search, category?: string) {
  return shopHref(params, { category, page: undefined });
}

export default async function ShopHome({
  searchParams,
}: {
  searchParams: Promise<Search>;
}) {
  const params = await searchParams;
  const sort =
    params.sort === "price-asc" || params.sort === "price-desc"
      ? params.sort
      : undefined;
  const [page, categories] = await Promise.all([
    getShopProducts({
      page: params.page ? Number(params.page) : 1,
      category: params.category,
      sort,
      search: params.q,
    }),
    getShopCategories(),
  ]);

  return (
    <div>
      {/* Hero strip — full-bleed gradient panel per Home_Tiles */}
      <section className="relative flex h-52 items-center overflow-hidden bg-gradient-to-r from-[#7A4630] via-[#C96F4A] to-[#DCB9AC]">
        <div className="mx-auto w-full max-w-6xl px-6 text-white">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-white/80">
            Spring ’26 · Vessels
          </p>
          <h1 className="font-display mt-2 max-w-md text-4xl leading-[1.05]">
            Hand-thrown for slow <i>mornings.</i>
          </h1>
          <div className="mt-4 flex gap-2">
            <Link
              href="#shop"
              className="rounded-full bg-white px-4 py-2 text-xs font-semibold text-foreground"
            >
              Shop the drop →
            </Link>
          </div>
        </div>
      </section>

      {/* Filter bar */}
      <section className="border-b">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-3 px-6 py-3.5">
          <div className="flex flex-wrap gap-2">
            <Link
              href={chipHref(params)}
              className={`rounded-full border px-3 py-1 text-xs font-medium ${!params.category ? "border-foreground bg-foreground text-background" : "text-muted-foreground hover:text-foreground"}`}
            >
              All
            </Link>
            {categories.map((c) => (
              <Link
                key={c}
                href={chipHref(params, c)}
                className={`rounded-full border px-3 py-1 text-xs font-medium ${params.category === c ? "border-foreground bg-foreground text-background" : "text-muted-foreground hover:text-foreground"}`}
              >
                {c}
              </Link>
            ))}
          </div>
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <span>{page.total} pieces</span>
            <span className="flex gap-2">
              <Link
                href={shopHref(params, { sort: "price-asc", page: undefined })}
                className={
                  params.sort === "price-asc"
                    ? "font-semibold text-foreground"
                    : ""
                }
              >
                Price ↑
              </Link>
              <Link
                href={shopHref(params, { sort: "price-desc", page: undefined })}
                className={
                  params.sort === "price-desc"
                    ? "font-semibold text-foreground"
                    : ""
                }
              >
                Price ↓
              </Link>
            </span>
          </div>
        </div>
      </section>

      {/* Grid */}
      <section id="shop" className="mx-auto max-w-6xl px-6 py-6">
        {page.items.length === 0 ? (
          <p className="py-16 text-center text-sm text-muted-foreground">
            Nothing matches — try another search.
          </p>
        ) : (
          <div className="grid grid-cols-2 gap-5 md:grid-cols-4">
            {page.items.map((p) => (
              <ProductCard key={p.sku} product={p} />
            ))}
          </div>
        )}
        {page.total > page.pageSize && (
          <div className="mt-8 flex justify-center gap-3 text-xs">
            {page.page > 1 && (
              <Link
                href={shopHref(params, { page: String(page.page - 1) })}
                className="rounded-full border px-4 py-2"
              >
                ← Previous
              </Link>
            )}
            {page.page * page.pageSize < page.total && (
              <Link
                href={shopHref(params, { page: String(page.page + 1) })}
                className="rounded-full border px-4 py-2"
              >
                Next →
              </Link>
            )}
          </div>
        )}
      </section>
    </div>
  );
}

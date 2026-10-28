import Link from "next/link";
import { Button } from "@/components/ui/button";
import { formatPriceRange } from "@/lib/catalog";
import type { Page, StorefrontProductSummary } from "@/lib/storefront";

// A page of storefront Products, with links to the neighbouring pages at `href(page)`.
export function ProductGrid({
  products,
  href,
  showShop = true,
}: {
  products: Page<StorefrontProductSummary>;
  href: (page: number) => string;
  showShop?: boolean;
}) {
  const { items, page, pageSize, totalCount } = products;
  const pageCount = Math.max(1, Math.ceil(totalCount / pageSize));

  if (items.length === 0) {
    return (
      <p className="text-muted-foreground">
        {totalCount === 0 ? "No Products yet." : "No Products on this page."}{" "}
        {page > 1 && (
          <Link href={href(1)} className="underline">
            Back to the first page
          </Link>
        )}
      </p>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <ul className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3" aria-label="Products">
        {items.map((product) => (
          <li key={product.id} className="flex flex-col gap-1 rounded-lg border p-4">
            <Link href={`/products/${product.id}`} className="font-medium hover:underline">
              {product.name}
            </Link>
            <span className="tabular-nums">{formatPriceRange(product)}</span>
            {!product.inStock && <span className="text-sm text-muted-foreground">Out of stock</span>}
            {showShop && (
              <Link href={`/shops/${product.shopId}`} className="text-sm text-muted-foreground hover:underline">
                {product.shopName}
              </Link>
            )}
          </li>
        ))}
      </ul>
      {pageCount > 1 && (
        <nav className="flex items-center justify-center gap-4" aria-label="Pagination">
          {page > 1 ? (
            <Button asChild variant="outline" size="sm">
              <Link href={href(page - 1)}>Previous</Link>
            </Button>
          ) : (
            <Button variant="outline" size="sm" disabled>
              Previous
            </Button>
          )}
          <span className="text-sm text-muted-foreground">
            Page {page} of {pageCount}
          </span>
          {page < pageCount ? (
            <Button asChild variant="outline" size="sm">
              <Link href={href(page + 1)}>Next</Link>
            </Button>
          ) : (
            <Button variant="outline" size="sm" disabled>
              Next
            </Button>
          )}
        </nav>
      )}
    </div>
  );
}

import Link from "next/link";
import { ProductImage } from "@/components/storefront/product-image";
import { money } from "@/lib/money";
import type { Listing } from "@/lib/seller/listings/types";

export function ProductCard({ product }: { product: Listing }) {
  return (
    <Link
      href={`/products/${encodeURIComponent(product.sku)}`}
      className="group block"
    >
      <div className="relative">
        <ProductImage
          photoUrl={product.photoUrls?.[0]}
          category={product.category}
          name={product.name}
          className="aspect-square w-full transition-opacity group-hover:opacity-90"
        />
        {product.inventory <= 3 && (
          <span className="absolute left-3 top-3 rounded-full bg-white/90 px-2.5 py-1 text-[11px] font-semibold">
            {product.inventory} left
          </span>
        )}
      </div>
      <div className="mt-2.5 flex items-baseline justify-between gap-2">
        <span className="text-sm font-semibold">{product.name}</span>
        <span className="text-sm font-semibold tabular-nums">
          {money(product.price)}
        </span>
      </div>
      <div className="mt-0.5 text-xs text-muted-foreground">
        {product.category}
      </div>
    </Link>
  );
}

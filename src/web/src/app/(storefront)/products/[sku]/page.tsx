import Link from "next/link";
import { notFound } from "next/navigation";
import { AddToBag } from "@/components/storefront/add-to-bag";
import { ProductImage } from "@/components/storefront/product-image";
import { money } from "@/lib/money";
import { getShopProduct } from "@/lib/storefront/data";

export default async function ProductPage({
  params,
}: {
  params: Promise<{ sku: string }>;
}) {
  const { sku } = await params;
  const product = await getShopProduct(decodeURIComponent(sku));
  if (!product) notFound();

  const photos = product.photoUrls ?? [];

  return (
    <div className="mx-auto max-w-6xl px-6 py-5">
      <nav className="mb-4 text-xs text-muted-foreground">
        <Link href="/" className="hover:text-foreground">
          Shop
        </Link>
        <span className="mx-1.5">/</span>
        <Link
          href={`/?category=${encodeURIComponent(product.category)}`}
          className="hover:text-foreground"
        >
          {product.category}
        </Link>
        <span className="mx-1.5">/</span>
        <span className="text-foreground">{product.name}</span>
      </nav>

      <div className="grid gap-5 lg:grid-cols-[64px_1fr_380px]">
        {/* Thumb rail */}
        <div className="order-2 flex gap-2 lg:order-1 lg:flex-col">
          {(photos.length > 0 ? photos : [undefined])
            .slice(0, 5)
            .map((url, i) => (
              <ProductImage
                key={url ?? i}
                photoUrl={url}
                category={product.category}
                name={`${product.name} photo ${i + 1}`}
                className="h-16 w-16 rounded-lg"
              />
            ))}
        </div>

        {/* Hero image */}
        <div className="order-1 lg:order-2">
          <ProductImage
            photoUrl={photos[0]}
            category={product.category}
            name={product.name}
            className="aspect-[4/3] w-full rounded-xl"
            sizes="(min-width: 1024px) 50vw, 100vw"
          />
        </div>

        {/* Info column */}
        <div className="order-3">
          <p className="text-[11px] font-semibold uppercase tracking-[0.14em] text-muted-foreground">
            Micro Commerce · Oakland
          </p>
          <h1 className="font-display mt-1.5 text-3xl">{product.name}</h1>
          <div className="mt-3 flex items-baseline gap-2">
            <span className="text-2xl font-semibold tabular-nums">
              {money(product.price)}
            </span>
            <span className="text-xs text-muted-foreground">+ tax</span>
          </div>

          {product.inventory <= 3 && product.inventory > 0 && (
            <p className="mt-2 text-xs font-semibold text-[#B45A38]">
              Only {product.inventory} left
            </p>
          )}
          {product.inventory === 0 && (
            <p className="mt-2 text-xs font-semibold text-red-600">Sold out</p>
          )}

          {product.description && (
            <p className="mt-4 text-sm leading-relaxed text-muted-foreground">
              {product.description}
            </p>
          )}

          {(product.tags?.length ?? 0) > 0 && (
            <div className="mt-4 flex flex-wrap gap-1.5">
              {product.tags?.map((tag) => (
                <span
                  key={tag}
                  className="rounded-full border px-2.5 py-0.5 text-[11px] text-muted-foreground"
                >
                  {tag}
                </span>
              ))}
            </div>
          )}

          <div className="mt-5">
            <AddToBag
              sku={product.sku}
              price={product.price}
              maxQty={product.inventory}
            />
          </div>

          <div className="mt-4 space-y-1 text-xs text-muted-foreground">
            <p>🚚 Free local delivery · ships in 3–5 days</p>
            <p>↩︎ 14-day returns · made one at a time</p>
            {product.origin && <p>Origin: {product.origin}</p>}
          </div>
        </div>
      </div>
    </div>
  );
}

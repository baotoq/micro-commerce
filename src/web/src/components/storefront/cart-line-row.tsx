import Link from "next/link";
import { ProductImage } from "@/components/storefront/product-image";
import { money } from "@/lib/money";
import type { Listing } from "@/lib/seller/listings/types";
import { removeCartLine, setCartQty } from "@/lib/storefront/actions";

export function CartLineRow({
  product,
  qty,
}: {
  product: Listing;
  qty: number;
}) {
  const setQty = async (formData: FormData) => {
    "use server";
    await setCartQty(String(formData.get("sku")), Number(formData.get("qty")));
  };
  const remove = async (formData: FormData) => {
    "use server";
    await removeCartLine(String(formData.get("sku")));
  };

  return (
    <div className="flex gap-4 border-b py-4 last:border-b-0">
      <Link
        href={`/products/${encodeURIComponent(product.sku)}`}
        className="shrink-0"
      >
        <ProductImage
          photoUrl={product.photoUrls?.[0]}
          category={product.category}
          name={product.name}
          className="h-20 w-20"
        />
      </Link>
      <div className="flex grow flex-col">
        <div className="flex justify-between gap-2">
          <span className="text-sm font-semibold">{product.name}</span>
          <span className="text-sm font-semibold tabular-nums">
            {money(product.price * qty)}
          </span>
        </div>
        <span className="mt-0.5 text-xs text-muted-foreground">
          {product.category} · SKU {product.sku}
        </span>
        <div className="mt-auto flex items-center justify-between pt-2">
          <div className="flex h-7 items-center rounded-full border px-1 text-sm">
            <form action={setQty}>
              <input type="hidden" name="sku" value={product.sku} />
              <input type="hidden" name="qty" value={qty - 1} />
              <button
                type="submit"
                aria-label={`Decrease ${product.name} quantity`}
                className="px-2"
              >
                −
              </button>
            </form>
            <span className="w-5 text-center text-xs font-semibold tabular-nums">
              {qty}
            </span>
            <form action={setQty}>
              <input type="hidden" name="sku" value={product.sku} />
              <input
                type="hidden"
                name="qty"
                value={Math.min(qty + 1, product.inventory)}
              />
              <button
                type="submit"
                aria-label={`Increase ${product.name} quantity`}
                className="px-2 disabled:opacity-30"
                disabled={qty >= product.inventory}
              >
                +
              </button>
            </form>
          </div>
          <form action={remove}>
            <input type="hidden" name="sku" value={product.sku} />
            <button
              type="submit"
              className="text-xs text-muted-foreground hover:text-foreground"
            >
              Remove
            </button>
          </form>
        </div>
      </div>
    </div>
  );
}

import Link from "next/link";
import { notFound } from "next/navigation";
import { ApiError } from "@/lib/api";
import { categoryPath, formatPrice, formatPriceRange, getCategoryTree } from "@/lib/catalog";
import { getAccessToken, signInUrl } from "@/lib/session";
import { getStorefrontProduct } from "@/lib/storefront";
import { AddToCart } from "./add-to-cart";

export default async function ProductPage({ params }: PageProps<"/products/[id]">) {
  const { id } = await params;
  const [product, tree, accessToken] = await Promise.all([
    // A Draft or unknown Product is a 404 from the API too.
    getStorefrontProduct(id).catch((error) => {
      if (error instanceof ApiError && error.status === 404) notFound();
      throw error;
    }),
    getCategoryTree(),
    getAccessToken(),
  ]);
  const prices = product.variants.map((v) => v.price);

  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-10">
      <p className="mb-2 text-sm text-muted-foreground">{categoryPath(tree, product.categoryId)}</p>
      <h1 className="text-2xl font-semibold">{product.name}</h1>
      <p className="mt-1 text-sm">
        Sold by{" "}
        <Link href={`/shops/${product.shopId}`} className="underline">
          {product.shopName}
        </Link>
      </p>
      <p className="mt-4 text-xl tabular-nums">
        {formatPriceRange({ minPrice: Math.min(...prices), maxPrice: Math.max(...prices) })}
      </p>
      {accessToken ? (
        <AddToCart product={product} />
      ) : (
        <p className="mt-6">
          <Link href={signInUrl(`/products/${product.id}`)} className="underline">
            Sign in to add it to your Cart
          </Link>
        </p>
      )}
      {product.description && <p className="mt-6 whitespace-pre-line">{product.description}</p>}

      <h2 className="mt-8 mb-3 font-semibold">{product.options.length > 0 ? "Variants" : "Availability"}</h2>
      <table className="w-full text-sm">
        <thead className="text-left text-muted-foreground">
          <tr className="border-b">
            {product.options.map((option) => (
              <th key={option.name} className="py-2 font-normal">
                {option.name}
              </th>
            ))}
            <th className="py-2 text-right font-normal">Price</th>
            <th className="py-2 text-right font-normal">In stock</th>
          </tr>
        </thead>
        <tbody>
          {product.variants.map((variant) => (
            <tr key={variant.id} className="border-b">
              {variant.optionValues.map((value, i) => (
                <td key={product.options[i]?.name ?? i} className="py-2">
                  {value}
                </td>
              ))}
              <td className="py-2 text-right tabular-nums">{formatPrice(variant.price)}</td>
              <td className="py-2 text-right tabular-nums">{variant.stock > 0 ? variant.stock : "Out of stock"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </main>
  );
}

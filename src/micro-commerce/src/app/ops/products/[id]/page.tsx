import Link from "next/link";
import { notFound } from "next/navigation";
import { ApiError } from "@/lib/api";
import { categoryPath, formatPrice, getCategoryTree, getOperatorProduct } from "@/lib/catalog";
import { getAccessToken, requirePlatformOperator } from "@/lib/session";
import { RemoveProductButton } from "./remove-product-button";

// Any Product, whatever its Merchant or status, for a Platform Operator to review and remove.
export default async function OperatorProductPage({ params }: PageProps<"/ops/products/[id]">) {
  await requirePlatformOperator();
  // requirePlatformOperator only lets a signed-in Account through, so there is a token.
  const accessToken = (await getAccessToken())!;
  const { id } = await params;
  const [product, tree] = await Promise.all([
    getOperatorProduct(accessToken, id).catch((error) => {
      if (error instanceof ApiError && error.status === 404) notFound();
      throw error;
    }),
    getCategoryTree(),
  ]);

  return (
    <section className="flex flex-col gap-6">
      <div>
        <Link href="/ops/products" className="text-sm text-muted-foreground hover:underline">
          ← Find another Product
        </Link>
        <div className="mt-2 flex flex-wrap items-center justify-between gap-4">
          <h1 className="text-2xl font-semibold">{product.name}</h1>
          <div className="flex items-center gap-3">
            {product.status === "Published" && (
              <Link href={`/products/${product.id}`} className="text-sm hover:underline">
                View in store
              </Link>
            )}
            <span className="text-sm text-muted-foreground">{product.status}</span>
            {product.status !== "Removed" && <RemoveProductButton product={product} />}
          </div>
        </div>
      </div>

      <dl className="grid grid-cols-[max-content_1fr] gap-x-6 gap-y-2 text-sm">
        <dt className="text-muted-foreground">Shop</dt>
        <dd>
          {product.shopName ? (
            <Link href={`/shops/${product.merchantId}`} className="hover:underline">
              {product.shopName}
            </Link>
          ) : (
            <span className="text-muted-foreground">Not on the storefront yet</span>
          )}
        </dd>
        <dt className="text-muted-foreground">Merchant ID</dt>
        <dd className="font-mono text-xs leading-5">{product.merchantId}</dd>
        <dt className="text-muted-foreground">Category</dt>
        <dd>{categoryPath(tree, product.categoryId) ?? product.categoryId}</dd>
        <dt className="text-muted-foreground">Created</dt>
        <dd>{new Date(product.createdAt).toLocaleString("en-GB", { dateStyle: "medium", timeStyle: "short" })}</dd>
      </dl>

      {product.description && <p className="whitespace-pre-line">{product.description}</p>}

      <table className="w-full text-sm">
        <thead className="text-left text-muted-foreground">
          <tr className="border-b">
            <th className="py-2 font-normal">Variant</th>
            <th className="py-2 text-right font-normal">Price</th>
            <th className="py-2 text-right font-normal">Stock</th>
            <th className="py-2 text-right font-normal">Weight</th>
          </tr>
        </thead>
        <tbody>
          {product.variants.map((variant) => (
            <tr key={variant.id} className="border-b">
              <td className="py-2">{variant.optionValues.join(" / ") || "Default"}</td>
              <td className="py-2 text-right tabular-nums">{formatPrice(variant.price)}</td>
              <td className="py-2 text-right tabular-nums">{variant.stock}</td>
              <td className="py-2 text-right tabular-nums">{variant.weightGrams} g</td>
            </tr>
          ))}
        </tbody>
      </table>
    </section>
  );
}

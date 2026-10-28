import Link from "next/link";
import { Button } from "@/components/ui/button";
import { formatPriceRange, listMerchantProducts } from "@/lib/catalog";
import { requireMerchantSession } from "@/lib/session";
import { ProductStatusButton } from "./product-status-button";

export default async function ProductsPage() {
  const { accessToken } = await requireMerchantSession();
  const products = await listMerchantProducts(accessToken);

  return (
    <section>
      <div className="mb-6 flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">Products</h1>
        <Button asChild>
          <Link href="/merchant/products/new">New Product</Link>
        </Button>
      </div>
      {products.length === 0 ? (
        <p className="text-muted-foreground">No Products yet.</p>
      ) : (
        <table className="w-full text-sm">
          <thead className="text-left text-muted-foreground">
            <tr className="border-b">
              <th className="py-2 font-normal">Name</th>
              <th className="py-2 font-normal">Status</th>
              <th className="py-2 text-right font-normal">Price</th>
              <th className="py-2 text-right font-normal">Stock</th>
              <th className="py-2" />
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.id} className="border-b">
                <td className="py-2">
                  <Link href={`/merchant/products/${product.id}`} className="hover:underline">
                    {product.name}
                  </Link>
                </td>
                <td className="py-2">{product.status}</td>
                <td className="py-2 text-right tabular-nums">{formatPriceRange(product)}</td>
                <td className="py-2 text-right tabular-nums">{product.totalStock}</td>
                <td className="py-2 text-right">
                  <ProductStatusButton product={product} />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </section>
  );
}

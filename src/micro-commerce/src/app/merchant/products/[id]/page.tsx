import { notFound } from "next/navigation";
import { ApiError } from "@/lib/api";
import { getCategoryTree, getMerchantProduct, leafCategories } from "@/lib/catalog";
import { requireMerchantSession } from "@/lib/session";
import { ProductStatusButton } from "../product-status-button";
import { EditProductForm } from "./edit-product-form";

export default async function EditProductPage({ params }: PageProps<"/merchant/products/[id]">) {
  const { accessToken } = await requireMerchantSession();
  const { id } = await params;
  const [product, tree] = await Promise.all([
    // Another Merchant's Product, or an unknown one, is a 404 from the API too.
    getMerchantProduct(accessToken, id).catch((error) => {
      if (error instanceof ApiError && error.status === 404) notFound();
      throw error;
    }),
    getCategoryTree(),
  ]);

  return (
    <section>
      <div className="mb-6 flex items-center justify-between gap-4">
        <h1 className="text-2xl font-semibold">Edit {product.name}</h1>
        <div className="flex items-center gap-3">
          <span className="text-sm text-muted-foreground">{product.status}</span>
          <ProductStatusButton product={product} />
        </div>
      </div>
      <EditProductForm product={product} categories={leafCategories(tree)} />
    </section>
  );
}

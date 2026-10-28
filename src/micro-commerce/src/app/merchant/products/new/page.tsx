import { getCategoryTree, leafCategories } from "@/lib/catalog";
import { requireMerchant } from "@/lib/session";
import { NewProductForm } from "./new-product-form";

export default async function NewProductPage() {
  await requireMerchant();
  const categories = leafCategories(await getCategoryTree());

  return (
    <section>
      <h1 className="mb-6 text-2xl font-semibold">New Product</h1>
      <p className="mb-6 text-muted-foreground">New Products start as Draft; Buyers see them once you publish.</p>
      <NewProductForm categories={categories} />
    </section>
  );
}

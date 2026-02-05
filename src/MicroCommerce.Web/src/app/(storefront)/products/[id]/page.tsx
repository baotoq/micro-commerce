import { ProductDetail } from "@/components/storefront/product-detail";

export default async function ProductPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;

  return (
    <div className="mx-auto max-w-7xl px-4 py-10 sm:px-6 lg:px-8">
      <ProductDetail productId={id} />
    </div>
  );
}

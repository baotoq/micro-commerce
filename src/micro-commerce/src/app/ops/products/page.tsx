import Form from "next/form";
import { redirect } from "next/navigation";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { requirePlatformOperator } from "@/lib/session";
import { searchParam } from "@/lib/storefront";

const productId = /[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}/i;

// Finds any Product by its ID, or by a link holding it (such as its storefront page), to review it.
export default async function ProductLookupPage({ searchParams }: PageProps<"/ops/products">) {
  await requirePlatformOperator();
  const lookup = searchParam((await searchParams).product);
  const id = lookup?.match(productId)?.[0];
  if (id) redirect(`/ops/products/${id.toLowerCase()}`);

  return (
    <section>
      <h1 className="mb-6 text-2xl font-semibold">Products</h1>
      <Form action="/ops/products" className="flex flex-col gap-3 sm:flex-row sm:items-end">
        <div className="flex flex-1 flex-col gap-1.5">
          <Label htmlFor="product">Product ID or link</Label>
          <Input
            key={lookup}
            id="product"
            name="product"
            defaultValue={lookup}
            placeholder="http://localhost:3000/products/…"
            aria-invalid={lookup !== undefined}
            aria-describedby={lookup !== undefined ? "product-error" : undefined}
          />
        </div>
        <Button type="submit">Find Product</Button>
      </Form>
      {lookup !== undefined && (
        <p id="product-error" role="alert" className="mt-2 text-sm text-destructive">
          That isn&apos;t a Product ID or a link to a Product.
        </p>
      )}
    </section>
  );
}

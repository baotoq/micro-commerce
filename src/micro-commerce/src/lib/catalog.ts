import { z } from "zod";
import { apiFetch } from "@/lib/api";

// Mirrors CategoryResponse in the API.
export type Category = {
  id: string;
  name: string;
  parentId: string | null;
};

// Mirrors CategoryNodeResponse in the API: a Category and its descendants, children ordered by name.
export type CategoryNode = {
  id: string;
  name: string;
  children: CategoryNode[];
};

// Anonymous, so it works from the server and the browser alike.
export function getCategoryTree() {
  return apiFetch<CategoryNode[]>("/storefront/categories");
}

// Mirrors CreateCategoryValidator / RenameCategoryValidator in the API; the API stays the source of truth.
export const categoryNameSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(100, "Name must be 100 characters or fewer"),
});

export type CategoryNameInput = z.infer<typeof categoryNameSchema>;

// Browser-side, through the /api proxy, as the signed-in Platform Operator.
export function createCategory({ name }: CategoryNameInput, parentId: string | null = null) {
  return apiFetch<Category>("/ops/categories", {
    method: "POST",
    body: JSON.stringify({ name, parentId }),
  });
}

export function renameCategory(id: string, { name }: CategoryNameInput) {
  return apiFetch<Category>(`/ops/categories/${id}`, {
    method: "PUT",
    body: JSON.stringify({ name }),
  });
}

export function deleteCategory(id: string) {
  return apiFetch<void>(`/ops/categories/${id}`, { method: "DELETE" });
}

// Every Category without children, with its path from the top ("Fashion › Men › T-Shirts"), in tree order.
// Products can only be listed in one of these.
export function leafCategories(tree: CategoryNode[], parents: string[] = []): { id: string; path: string }[] {
  return tree.flatMap((node) =>
    node.children.length === 0
      ? [{ id: node.id, path: [...parents, node.name].join(" › ") }]
      : leafCategories(node.children, [...parents, node.name]),
  );
}

// Mirrors ProductStatus in the API.
export type ProductStatus = "Draft" | "Published";

// Mirrors ProductOption in the API, e.g. Colour with Red and Blue.
export type ProductOption = {
  name: string;
  values: string[];
};

// Mirrors VariantResponse in the API: one value of each option, in option order (none for a default
// Variant); price is whole VND, weight is in grams.
export type Variant = {
  id: string;
  optionValues: string[];
  price: number;
  stock: number;
  weightGrams: number;
};

// Mirrors ProductResponse in the API: a Product as its Merchant sees it.
export type Product = {
  id: string;
  name: string;
  description: string | null;
  categoryId: string;
  status: ProductStatus;
  options: ProductOption[];
  variants: Variant[];
  createdAt: string;
};

// Mirrors ProductSummaryResponse in the API: a row of the Merchant's Product list.
export type ProductSummary = {
  id: string;
  name: string;
  categoryId: string;
  status: ProductStatus;
  variantCount: number;
  minPrice: number;
  maxPrice: number;
  totalStock: number;
  createdAt: string;
};

const vnd = new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" });

export function formatPrice(amount: number) {
  return vnd.format(amount);
}

// Server-side: the caller passes the session's access token.
export function listMerchantProducts(accessToken: string) {
  return apiFetch<ProductSummary[]>("/merchant/products", {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

export function getMerchantProduct(accessToken: string, id: string) {
  return apiFetch<Product>(`/merchant/products/${encodeURIComponent(id)}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

const wholeNumber = (label: string) =>
  z.number({ error: `${label} is required` }).int(`${label} must be a whole number`);

// Option names and values compare ignoring case, as in the API.
const sameText = (a: string, b: string) => a.toLocaleLowerCase() === b.toLocaleLowerCase();
const distinct = (values: string[]) =>
  values.every((value, i) => values.findIndex((other) => sameText(other, value)) === i);

// As in the API, which also keeps line breaks out of names and values.
const maxOptionValues = 20;
const oneLine = /^\P{Cc}*$/u;
const oneLineMessage = "Can't contain control characters";

// The form takes an option's values as one text, one value per line, so any value round-trips.
export function splitOptionValues(text: string) {
  return text
    .split(/\r?\n/)
    .map((value) => value.trim())
    .filter((value) => value.length > 0);
}

// Every combination of one value of each option, in option order with the last option varying
// fastest; a single empty combination without options, as the API orders Variants.
export function combinations(values: string[][]): string[][] {
  return values.reduce<string[][]>((combos, option) => combos.flatMap((c) => option.map((v) => [...c, v])), [[]]);
}

const optionSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Option name is required")
    .max(50, "Option name must be 50 characters or fewer")
    .regex(oneLine, oneLineMessage),
  values: z
    .string()
    .transform(splitOptionValues)
    .pipe(
      z
        .array(z.string().max(50, "Each value must be 50 characters or fewer").regex(oneLine, oneLineMessage))
        .min(1, "Give at least one value")
        .max(maxOptionValues, `An option can have at most ${maxOptionValues} values`)
        .refine(distinct, "Each value must be different"),
    ),
});

const variantSchema = z.object({
  optionValues: z.array(z.string()),
  price: wholeNumber("Price")
    .positive("Price must be more than 0")
    .max(Number.MAX_SAFE_INTEGER, "Price is too high"),
  stock: wholeNumber("Stock").min(0, "Stock can't be negative"),
  weightGrams: wholeNumber("Weight").positive("Weight must be more than 0"),
});

// Mirrors ProductDetailsValidator in the API; the API stays the source of truth. The form keeps the
// Variants in step with the options, so it doesn't re-check the combinations.
export const productSchema = z.object({
  name: z
    .string()
    .trim()
    .min(1, "Name is required")
    .max(200, "Name must be 200 characters or fewer"),
  description: z.string().trim().max(5000, "Description must be 5000 characters or fewer"),
  categoryId: z.string().min(1, "Category is required"),
  options: z
    .array(optionSchema)
    .max(2, "A Product can have at most 2 options")
    .superRefine((options, ctx) =>
      options.forEach((option, i) => {
        if (options.findIndex((other) => sameText(other.name, option.name)) !== i)
          ctx.addIssue({ code: "custom", path: [i, "name"], message: "Each option needs a different name" });
      }),
    ),
  variants: z.array(variantSchema).min(1, "Give each option at least one value"),
});

// What the form edits: option values as text, one per line.
export type ProductFormValues = z.input<typeof productSchema>;
export type VariantFormValues = ProductFormValues["variants"][number];
// What the API takes.
export type ProductInput = z.output<typeof productSchema>;

// Values hold no line breaks, so this tells combinations apart; it ignores case, as the API does.
const combinationKey = (optionValues: string[]) => optionValues.map((v) => v.toLocaleLowerCase()).join("\n");

// The Variants for the options' current values: one per combination. `remembered` keeps what was entered
// for every combination seen so far (this adds the current Variants to it), so a combination that briefly
// disappears while its values are typed gets its price, stock and weight back. Only the first values an
// option may have count, so pasting a long list can't build an enormous grid.
export function variantsFor(
  options: ProductFormValues["options"],
  current: VariantFormValues[],
  remembered: Map<string, VariantFormValues>,
) {
  for (const variant of current) remembered.set(combinationKey(variant.optionValues), variant);

  const values = options.map((option) =>
    splitOptionValues(option.values)
      .filter((value, i, all) => all.findIndex((v) => sameText(v, value)) === i)
      .slice(0, maxOptionValues),
  );
  return combinations(values).map((combo) => ({
    ...(remembered.get(combinationKey(combo)) ?? { price: Number.NaN, stock: Number.NaN, weightGrams: Number.NaN }),
    optionValues: combo,
  }));
}

const productBody = ({ description, ...input }: ProductInput) =>
  JSON.stringify({ ...input, description: description || null });

// Browser-side, through the /api proxy, as the signed-in Merchant.
export function createProduct(input: ProductInput) {
  return apiFetch<Product>("/merchant/products", { method: "POST", body: productBody(input) });
}

export function updateProduct(id: string, input: ProductInput) {
  return apiFetch<Product>(`/merchant/products/${id}`, { method: "PUT", body: productBody(input) });
}

export function publishProduct(id: string) {
  return apiFetch<Product>(`/merchant/products/${id}/publish`, { method: "POST" });
}

export function unpublishProduct(id: string) {
  return apiFetch<Product>(`/merchant/products/${id}/unpublish`, { method: "POST" });
}

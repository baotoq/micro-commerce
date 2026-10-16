import { getCategoryTree } from "@/lib/catalog";
import { requirePlatformOperator } from "@/lib/session";
import { CategoryTreeEditor } from "./category-tree-editor";

export default async function CategoriesPage() {
  await requirePlatformOperator();
  const tree = await getCategoryTree();

  return <CategoryTreeEditor tree={tree} />;
}

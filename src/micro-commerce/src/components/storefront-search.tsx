import Form from "next/form";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { searchQueryMaxLength } from "@/lib/storefront";

// Searches the storefront home by words and Category; submitting navigates to `/?q=…&category=…`,
// starting again from the first page.
export function StorefrontSearch({
  query = "",
  categoryId = "",
  categories,
}: {
  query?: string;
  categoryId?: string;
  categories: { id: string; path: string }[];
}) {
  return (
    <Form action="/" role="search" className="flex flex-col gap-3 sm:flex-row sm:items-end">
      <div className="flex flex-1 flex-col gap-1.5">
        <Label htmlFor="q">Search</Label>
        {/* Keyed so the fields follow the URL when navigating between searches. */}
        <Input
          key={query}
          id="q"
          name="q"
          type="search"
          defaultValue={query}
          maxLength={searchQueryMaxLength}
          placeholder="Search Products"
        />
      </div>
      <div className="flex flex-col gap-1.5 sm:w-64">
        <Label htmlFor="category">Category</Label>
        <select
          key={categoryId}
          id="category"
          name="category"
          defaultValue={categoryId}
          className="h-8 w-full rounded-lg border border-input bg-transparent px-2.5 text-base outline-none focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50 md:text-sm dark:bg-input/30"
        >
          <option value="">All Categories</option>
          {categories.map((category) => (
            <option key={category.id} value={category.id}>
              {category.path}
            </option>
          ))}
        </select>
      </div>
      <Button type="submit">Search</Button>
    </Form>
  );
}

// web/src/app/seller/listings/page.tsx

import { FilterChips } from "@/components/seller/filter-chips";
import { ListingsTable } from "@/components/seller/listings-table";
import { Button } from "@/components/ui/button";
import { getListingCounts, getListings } from "@/lib/seller/data";

export default function ListingsPage() {
  const counts = getListingCounts();
  const published = counts.active + counts.low + counts.out;
  return (
    <section className="px-10 py-8">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <h1 className="text-3xl font-semibold tracking-tight">Listings</h1>
          <p className="mt-1 text-sm text-[#1d1d1f]/70">
            {counts.total} listings · {published} published
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="rounded-full">
            Import CSV
          </Button>
          <Button className="rounded-full">New listing</Button>
        </div>
      </header>

      <div className="mt-6">
        <FilterChips counts={counts} active="all" />
      </div>

      <div className="mt-6">
        <ListingsTable listings={getListings()} pageSize={9} />
      </div>
    </section>
  );
}

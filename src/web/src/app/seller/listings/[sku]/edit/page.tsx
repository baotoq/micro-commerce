import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { DeleteListingButton } from "@/components/seller/listings/delete-listing-button";
import { EditListingForm } from "@/components/seller/listings/edit-listing-form";
import { buttonVariants } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { getListingBySku } from "@/lib/seller/listings/data";

export async function generateMetadata({
  params,
}: {
  params: Promise<{ sku: string }>;
}): Promise<Metadata> {
  const { sku } = await params;
  return { title: `Edit ${sku} · Micro Commerce` };
}

type Variant = {
  label: string;
  sku: string;
  price: number;
  stock: number;
  status: "Active" | "Low" | "Out";
  changed?: boolean;
};

const VARIANTS: Variant[] = [
  {
    label: "Small · Persimmon",
    sku: "MS-VS-001-S",
    price: 70,
    stock: 6,
    status: "Active",
    changed: true,
  },
  {
    label: "Medium · Persimmon",
    sku: "MS-VS-001-M",
    price: 95,
    stock: 4,
    status: "Active",
    changed: true,
  },
  {
    label: "Large · Persimmon",
    sku: "MS-VS-001-L",
    price: 136,
    stock: 0,
    status: "Out",
  },
  {
    label: "Small · Cream",
    sku: "MS-VS-001-SC",
    price: 70,
    stock: 8,
    status: "Active",
  },
  {
    label: "Medium · Cream",
    sku: "MS-VS-001-MC",
    price: 95,
    stock: 5,
    status: "Active",
  },
  {
    label: "Large · Cream",
    sku: "MS-VS-001-LC",
    price: 136,
    stock: 2,
    status: "Low",
  },
];

const PHOTO_TONES = ["#e2d5c8", "#efe8d9", "#cfc7c2", "#d8c0a8"];

const STATUS_CHIP: Record<Variant["status"], string> = {
  Active:
    "rounded-full bg-good/15 px-2 py-0.5 text-[11px] font-medium text-good",
  Low: "rounded-full bg-warn/15 px-2 py-0.5 text-[11px] font-medium text-warn",
  Out: "rounded-full bg-bad/15 px-2 py-0.5 text-[11px] font-medium text-bad",
};

const money = (n: number) => `$${n}`;

export default async function ListingEditPage({
  params,
}: {
  params: Promise<{ sku: string }>;
}) {
  const { sku } = await params;
  const listing = await getListingBySku(sku);
  if (!listing) notFound();

  return (
    <div className="flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between border-b border-black/[0.06] bg-white px-7 py-4">
        <div className="flex items-center gap-3">
          <Link
            href="/seller/listings"
            aria-label="Back"
            className="flex size-8 items-center justify-center rounded-md hover:bg-canvas-parchment"
          >
            ‹
          </Link>
          <div>
            <p className="text-[11px] uppercase tracking-wider text-foreground/60">
              Listings · {listing.category}
            </p>
            <h1 className="mt-0.5 text-[24px] font-semibold leading-none tracking-tight text-foreground">
              {listing.name}
            </h1>
          </div>
        </div>
        {/* Action area */}
        <div className="flex gap-2">
          <DeleteListingButton sku={sku} />
          <Link
            href="/seller/listings"
            className={buttonVariants({ variant: "ghost" })}
          >
            Cancel
          </Link>
        </div>
      </div>

      {/* Body */}
      <div className="flex-1 overflow-auto px-7 py-6">
        <div
          className="grid gap-3"
          style={{ gridTemplateColumns: "1.5fr 1fr" }}
        >
          {/* Variant matrix */}
          <div className="rounded-xl border border-black/[0.06] bg-white p-5">
            <div className="mb-3.5 flex items-end justify-between">
              <div>
                <h2 className="text-[15px] font-semibold text-foreground">
                  Variant matrix
                </h2>
                <p className="text-[11px] text-foreground/60">
                  Size × Glaze · 6 combinations
                </p>
              </div>
            </div>

            <Table>
              <TableHeader>
                <TableRow className="text-[11px] uppercase tracking-wider text-foreground/50">
                  <TableHead className="pr-2">Variant</TableHead>
                  <TableHead>SKU</TableHead>
                  <TableHead>Price</TableHead>
                  <TableHead>Stock</TableHead>
                  <TableHead>Status</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {VARIANTS.map((v) => (
                  <TableRow
                    key={v.sku}
                    className={v.changed ? "bg-good/[0.04]" : undefined}
                  >
                    <TableCell className="py-3 pr-2 font-medium text-foreground">
                      <span className="inline-flex items-center gap-2">
                        {v.changed && (
                          <span className="size-1.5 rounded-full bg-good" />
                        )}
                        {v.label}
                      </span>
                    </TableCell>
                    <TableCell className="py-3 font-mono text-[12px] text-foreground/60">
                      {v.sku}
                    </TableCell>
                    <TableCell
                      className={`py-3 tabular-nums ${v.changed ? "font-semibold text-good" : "font-normal text-foreground"}`}
                    >
                      {money(v.price)}
                    </TableCell>
                    <TableCell
                      className={`py-3 tabular-nums ${v.stock === 0 ? "text-bad" : v.stock < 5 ? "text-warn" : "text-foreground"}`}
                    >
                      {v.stock}
                    </TableCell>
                    <TableCell className="py-3">
                      <span className={STATUS_CHIP[v.status]}>{v.status}</span>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
            <p className="mt-3.5 text-[11px] text-foreground/60">
              ● 2 variants updated · prices +10%
            </p>
          </div>

          {/* Right column */}
          <div className="flex flex-col gap-3">
            {/* Photos card */}
            <div className="rounded-xl border border-black/[0.06] bg-white p-[18px]">
              <h3 className="mb-3 text-[13.5px] font-semibold text-foreground">
                Photos · 4 of 8
              </h3>
              <div className="grid grid-cols-3 gap-2">
                {PHOTO_TONES.map((tone) => (
                  <div
                    key={tone}
                    className="rounded-md"
                    style={{ height: 88, background: tone }}
                  />
                ))}
                <div
                  className="flex items-center justify-center rounded-md border-[1.5px] border-dashed border-black/20 text-foreground/40"
                  style={{ height: 88 }}
                >
                  +
                </div>
              </div>
            </div>

            {/* Edit form — replaces static header card / pricing sections */}
            <EditListingForm listing={listing} />
          </div>
        </div>
      </div>
    </div>
  );
}

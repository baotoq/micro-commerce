// web/src/app/seller/listings/published/page.tsx
import { Check } from "lucide-react";
import Link from "next/link";
import { SellerTopbar } from "@/components/seller/shell/seller-topbar";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { cn } from "@/lib/utils";

const KPIS = [
  { label: "Active listings", value: "39", delta: "+1" },
  { label: "Variants in stock", value: "128", delta: "+2" },
  { label: "Out-of-stock items", value: "1", delta: "−2" },
];

const ACTIVITY = [
  {
    when: "just now",
    item: "Persimmon vase · Medium",
    change: "Price · $86 → $95",
    by: "You",
  },
  {
    when: "just now",
    item: "Persimmon vase · Small",
    change: "Price · $64 → $70",
    by: "You",
  },
  {
    when: "just now",
    item: "Persimmon vase · Large",
    change: "Status · Out → still out (no stock)",
    by: "You",
  },
  {
    when: "12 min ago",
    item: "Ember tea bowl",
    change: "Price · $36 → $39.60",
    by: "You · bulk",
  },
  {
    when: "12 min ago",
    item: "Peat serving bowl",
    change: "Price · $90 → $99",
    by: "You · bulk",
  },
];

export default function ListingsPublishedPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <SellerTopbar
        title="Listings"
        subtitle="42 products · 39 active"
        actions={
          <Link
            href="/seller/listings/new"
            className={cn(buttonVariants(), "rounded-full")}
          >
            + New listing
          </Link>
        }
      />

      <div className="border-b border-black/[0.06] bg-good-soft px-7 py-3.5">
        <div className="flex items-center gap-3">
          <span
            aria-hidden="true"
            className="flex h-[26px] w-[26px] shrink-0 items-center justify-center rounded-full bg-good text-white"
          >
            <Check className="size-3.5" strokeWidth={2.5} />
          </span>
          <span className="flex-1 text-[13.5px] font-semibold text-foreground">
            Persimmon vase published · 2 variants updated, 1 went live.
          </span>
          <Link
            href="/"
            className={cn(buttonVariants({ variant: "ghost", size: "sm" }))}
          >
            View shop →
          </Link>
          <Button variant="ghost" size="sm">
            Undo
          </Button>
        </div>
      </div>

      <div className="flex-1 overflow-auto px-7 py-6">
        <div className="mb-5 grid grid-cols-3 gap-3">
          {KPIS.map((k) => (
            <div
              key={k.label}
              className="rounded-xl border border-black/[0.06] bg-white p-[18px]"
            >
              <div className="text-[11px] text-foreground/50">{k.label}</div>
              <div className="mt-1.5 flex items-end gap-2">
                <div className="text-[28px] font-semibold leading-none tabular-nums text-foreground">
                  {k.value}
                </div>
                <span className="mb-1 text-[11px] font-semibold text-good">
                  ↑ {k.delta}
                </span>
              </div>
            </div>
          ))}
        </div>

        <div className="rounded-xl border border-black/[0.06] bg-white">
          <div className="flex items-center justify-between border-b border-black/[0.06] px-5 py-3.5">
            <h2 className="text-[15px] font-semibold text-foreground">
              What just changed
            </h2>
            <Link
              href="/seller/listings/activity"
              className="text-[13px] font-medium text-primary"
            >
              Activity log →
            </Link>
          </div>
          <Table>
            <TableHeader>
              <TableRow className="text-[11px] uppercase tracking-wider text-foreground/50">
                <TableHead className="px-5">When</TableHead>
                <TableHead className="px-5">Item</TableHead>
                <TableHead className="px-5">Change</TableHead>
                <TableHead className="px-5">By</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {ACTIVITY.map((r) => (
                <TableRow key={r.item}>
                  <TableCell className="px-5 py-3 text-[11px] text-foreground/60">
                    {r.when}
                  </TableCell>
                  <TableCell className="px-5 py-3 font-medium text-foreground">
                    {r.item}
                  </TableCell>
                  <TableCell className="px-5 py-3 text-foreground/70">
                    {r.change}
                  </TableCell>
                  <TableCell className="px-5 py-3 text-[13px]">
                    {r.by}
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}

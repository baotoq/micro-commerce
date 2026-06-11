import { cacheTag } from "next/cache";
import type { CustomerDto } from "@/lib/catalog/customers";
import { fetchCustomerByEmail, fetchCustomers } from "@/lib/catalog/customers";
import { money } from "@/lib/money";
import type { CustomerRow } from "@/lib/seller/customers/types";
import { DEMO_NOW } from "@/lib/seller/demo-clock";
import { formatRelative } from "@/lib/seller/orders/format";

/** "$284" for whole dollars, "$136.08" otherwise — built on the shared money helper. */
function formatSpend(amount: number): string {
  return money(amount).replace(/\.00$/, "");
}

function lastOrderLabel(lastOrderAt: string | null): string {
  if (!lastOrderAt) return "—";
  return formatRelative(new Date(lastOrderAt), DEMO_NOW);
}

function dtoToRow(dto: CustomerDto): CustomerRow {
  return {
    name: dto.name,
    email: dto.email,
    city: dto.city,
    tags: dto.tags,
    lifetimeSpend: formatSpend(dto.lifetimeSpend),
    lifetimeOrderCount: dto.lifetimeOrderCount,
    lastOrder: lastOrderLabel(dto.lastOrderAt),
  };
}

export async function getCustomers(): Promise<CustomerRow[]> {
  "use cache";
  cacheTag("customers");
  const page = await fetchCustomers();
  return page.items.map(dtoToRow);
}

export async function getCustomerByEmail(
  email: string,
): Promise<CustomerRow | null> {
  "use cache";
  cacheTag("customers");
  const dto = await fetchCustomerByEmail(email);
  return dto ? dtoToRow(dto) : null;
}

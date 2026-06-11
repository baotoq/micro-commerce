import { afterEach, describe, expect, it, vi } from "vitest";

vi.mock("server-only", () => ({}));
vi.mock("next/cache", () => ({ cacheTag: vi.fn() }));
vi.mock("@/lib/catalog/customers", () => ({
  fetchCustomers: vi.fn(),
  fetchCustomerByEmail: vi.fn(),
}));

import type { CustomerDto } from "@/lib/catalog/customers";
import * as customersClient from "@/lib/catalog/customers";
import { getCustomerByEmail, getCustomers } from "@/lib/seller/customers/data";

function makeDto(overrides: Partial<CustomerDto> = {}): CustomerDto {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    name: "Sasha Leblanc",
    email: "sasha.l@gmail.com",
    city: "San Francisco, CA",
    tags: ["VIP", "Repeat buyer"],
    lifetimeOrderCount: 3,
    lifetimeSpend: 284,
    firstOrderAt: "2026-03-12T16:00:00Z",
    // 2026-04-08T14:14:00-07:00 == 2026-04-08T21:14:00Z, ~2h before DEMO_NOW
    lastOrderAt: "2026-04-08T21:14:00Z",
    createdAt: "2026-03-12T16:00:00Z",
    ...overrides,
  };
}

describe("getCustomers", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("maps CustomerDto to a customer row with name, email, city and tags", async () => {
    vi.mocked(customersClient.fetchCustomers).mockResolvedValueOnce({
      items: [makeDto()],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const rows = await getCustomers();

    expect(rows).toHaveLength(1);
    expect(rows[0].name).toBe("Sasha Leblanc");
    expect(rows[0].email).toBe("sasha.l@gmail.com");
    expect(rows[0].city).toBe("San Francisco, CA");
    expect(rows[0].tags).toEqual(["VIP", "Repeat buyer"]);
  });

  it('formats lifetime spend as whole dollars via money helper (e.g. "$284" for Sasha)', async () => {
    vi.mocked(customersClient.fetchCustomers).mockResolvedValueOnce({
      items: [makeDto()],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const rows = await getCustomers();

    expect(rows[0].lifetimeSpend).toBe("$284");
    expect(rows[0].lifetimeOrderCount).toBe(3);
  });

  it("keeps cents when the spend is not a whole dollar amount", async () => {
    vi.mocked(customersClient.fetchCustomers).mockResolvedValueOnce({
      items: [makeDto({ lifetimeSpend: 136.08 })],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const rows = await getCustomers();
    expect(rows[0].lifetimeSpend).toBe("$136.08");
  });

  it("derives the lastOrder relative label from lastOrderAt against the demo clock", async () => {
    vi.mocked(customersClient.fetchCustomers).mockResolvedValueOnce({
      items: [makeDto()],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const rows = await getCustomers();
    expect(rows[0].lastOrder).toBe("2h ago");
  });

  it("shows an em dash for the lastOrder label when there is no last order", async () => {
    vi.mocked(customersClient.fetchCustomers).mockResolvedValueOnce({
      items: [makeDto({ lastOrderAt: null })],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const rows = await getCustomers();
    expect(rows[0].lastOrder).toBe("—");
  });

  it("renders an empty tags array when the customer has no tags", async () => {
    vi.mocked(customersClient.fetchCustomers).mockResolvedValueOnce({
      items: [
        makeDto({ name: "Dev Patel", email: "dev.p@gmail.com", tags: [] }),
      ],
      total: 1,
      page: 1,
      pageSize: 20,
    });

    const rows = await getCustomers();
    expect(rows[0].tags).toEqual([]);
  });
});

describe("getCustomerByEmail", () => {
  afterEach(() => {
    vi.clearAllMocks();
  });

  it("maps a single customer DTO to a row", async () => {
    vi.mocked(customersClient.fetchCustomerByEmail).mockResolvedValueOnce(
      makeDto(),
    );

    const row = await getCustomerByEmail("sasha.l@gmail.com");

    expect(row).not.toBeNull();
    expect(row?.name).toBe("Sasha Leblanc");
    expect(row?.lifetimeSpend).toBe("$284");
    expect(row?.lastOrder).toBe("2h ago");
  });

  it("returns null when the customer is not found", async () => {
    vi.mocked(customersClient.fetchCustomerByEmail).mockResolvedValueOnce(null);

    const row = await getCustomerByEmail("nobody@example.com");
    expect(row).toBeNull();
  });
});

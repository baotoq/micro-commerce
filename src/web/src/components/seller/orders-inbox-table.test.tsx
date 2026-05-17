import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { OrderInboxRow } from "@/lib/seller/types";
import { OrdersInboxTable } from "./orders-inbox-table";

const FIXTURE: OrderInboxRow[] = [
  {
    id: "#1042",
    placedLabel: "Today · 2:14 PM",
    customer: "Sasha L.",
    city: "San Francisco, CA",
    items: "Persimmon vase, Ash budstem",
    qty: 2,
    total: 152,
    ship: "USPS Priority",
    status: "New",
    tone: "warn",
    age: "2h",
    starred: true,
  },
  {
    id: "#1041",
    placedLabel: "Today · 11:08 AM",
    customer: "Devon T.",
    city: "Brooklyn, NY",
    items: "Forest bowl, lg.",
    qty: 1,
    total: 64,
    ship: "USPS Ground",
    status: "New",
    tone: "warn",
    age: "5h",
  },
  {
    id: "#1040",
    placedLabel: "Today · 9:41 AM",
    customer: "Ari K.",
    city: "Portland, OR",
    items: "Cream tumbler set",
    qty: 1,
    total: 48,
    ship: "USPS Ground",
    status: "New",
    tone: "warn",
    age: "7h",
  },
  {
    id: "#1039",
    placedLabel: "Yesterday",
    customer: "June P.",
    city: "Seattle, WA",
    items: "Indigo carafe",
    qty: 1,
    total: 110,
    ship: "UPS Ground",
    status: "Packed",
    tone: "mute",
    age: "1d",
  },
  {
    id: "#1038",
    placedLabel: "2 days ago",
    customer: "Theo R.",
    city: "Austin, TX",
    items: "Soft hand vessel +2",
    qty: 3,
    total: 218,
    ship: "USPS Priority",
    status: "Shipped",
    tone: "mute",
    age: "2d",
  },
  {
    id: "#1037",
    placedLabel: "3 days ago",
    customer: "Liu W.",
    city: "Vancouver, BC",
    items: "Ceremony bowl",
    qty: 1,
    total: 142,
    ship: "USPS Intl",
    status: "Shipped",
    tone: "mute",
    age: "3d",
  },
  {
    id: "#1036",
    placedLabel: "4 days ago",
    customer: "Marisol G.",
    city: "Mexico City, MX",
    items: "Field cup × 4",
    qty: 4,
    total: 88,
    ship: "USPS Intl",
    status: "Refund req.",
    tone: "bad",
    age: "4d",
  },
  {
    id: "#1035",
    placedLabel: "5 days ago",
    customer: "Sam D.",
    city: "Chicago, IL",
    items: "Storm bowl",
    qty: 1,
    total: 58,
    ship: "USPS Ground",
    status: "Delivered",
    tone: "good",
    age: "5d",
  },
  {
    id: "#1034",
    placedLabel: "6 days ago",
    customer: "Hana R.",
    city: "Oakland, CA",
    items: "Earth tumbler ×2",
    qty: 2,
    total: 56,
    ship: "Local pickup",
    status: "Delivered",
    tone: "good",
    age: "6d",
  },
  {
    id: "#1033",
    placedLabel: "1 week ago",
    customer: "Paul N.",
    city: "Los Angeles, CA",
    items: "Linen vase wrap",
    qty: 1,
    total: 18,
    ship: "USPS First",
    status: "Cancelled",
    tone: "mute",
    age: "7d",
  },
];

describe("OrdersInboxTable", () => {
  it("renders 10 rows when given 10-row fixture (11 total with header)", () => {
    render(<OrdersInboxTable rows={FIXTURE} selectedCount={3} />);
    const rows = screen.getAllByRole("row");
    expect(rows).toHaveLength(11);
  });

  it("renders status chip text matching each row status", () => {
    render(<OrdersInboxTable rows={FIXTURE} selectedCount={3} />);
    expect(screen.getAllByText("New")).toHaveLength(3);
    expect(screen.getByText("Packed")).toBeInTheDocument();
    expect(screen.getAllByText("Shipped")).toHaveLength(2);
    expect(screen.getByText("Refund req.")).toBeInTheDocument();
    expect(screen.getAllByText("Delivered")).toHaveLength(2);
    expect(screen.getByText("Cancelled")).toBeInTheDocument();
  });

  it("first 3 rows have selected styling via data-selected attribute", () => {
    render(<OrdersInboxTable rows={FIXTURE} selectedCount={3} />);
    const allRows = screen.getAllByRole("row");
    const dataRows = allRows.slice(1);
    expect(dataRows[0]).toHaveAttribute("data-selected", "true");
    expect(dataRows[1]).toHaveAttribute("data-selected", "true");
    expect(dataRows[2]).toHaveAttribute("data-selected", "true");
    expect(dataRows[3]).not.toHaveAttribute("data-selected", "true");
  });
});

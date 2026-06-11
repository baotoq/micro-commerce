// web/src/lib/seller/customers/types.ts

// View model for a single row in the seller Customers table.
export type CustomerRow = {
  name: string;
  email: string;
  city: string;
  tags: string[];
  lifetimeSpend: string; // formatted via @/lib/money, e.g. "$284" / "$136.08"
  lifetimeOrderCount: number;
  lastOrder: string; // relative label, e.g. "2h ago"; "—" when no orders
};

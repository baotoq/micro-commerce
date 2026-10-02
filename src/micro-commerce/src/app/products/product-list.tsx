"use client";

import { useQuery } from "@tanstack/react-query";
import { productsQueryOptions } from "@/lib/products";

export function ProductList() {
  const { data: products, isPending, error } = useQuery(productsQueryOptions);

  if (isPending) return <p className="text-muted-foreground">Loading…</p>;
  if (error) return <p className="text-destructive">{error.message}</p>;
  if (products.length === 0) return <p className="text-muted-foreground">No products yet.</p>;

  return (
    <ul className="divide-y rounded-lg border">
      {products.map((product) => (
        <li key={product.id} className="flex justify-between px-4 py-3">
          <span>{product.name}</span>
          <span className="tabular-nums">${product.price.toFixed(2)}</span>
        </li>
      ))}
    </ul>
  );
}

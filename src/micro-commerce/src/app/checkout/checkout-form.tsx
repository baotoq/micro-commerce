"use client";

import { useActionState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { provinceNames } from "@/lib/checkout";
import { placeCheckoutAction } from "./actions";

export function CheckoutForm({ itemsTotal }: { itemsTotal: number }) {
  const [state, formAction, pending] = useActionState(placeCheckoutAction, {});

  return (
    <form action={formAction} className="mt-8 grid gap-5">
      <input type="hidden" name="itemsTotal" value={itemsTotal} />
      <div className="grid gap-2">
        <Label htmlFor="recipientName">Recipient name</Label>
        <Input id="recipientName" name="recipientName" required maxLength={100} />
      </div>
      <div className="grid gap-2">
        <Label htmlFor="phone">Phone</Label>
        <Input id="phone" name="phone" type="tel" required maxLength={30} />
      </div>
      <div className="grid gap-2">
        <Label htmlFor="street">Street address</Label>
        <Input id="street" name="street" required maxLength={200} />
      </div>
      <div className="grid gap-2">
        <Label htmlFor="ward">Ward</Label>
        <Input id="ward" name="ward" required maxLength={100} />
      </div>
      <div className="grid gap-2">
        <Label htmlFor="province">Province / city</Label>
        <select
          id="province"
          name="province"
          required
          defaultValue=""
          className="h-10 rounded-md border bg-background px-3 text-sm"
        >
          <option value="" disabled>
            Choose a province or city
          </option>
          {provinceNames.map((province) => (
            <option key={province} value={province}>
              {province}
            </option>
          ))}
        </select>
      </div>
      {state.error && (
        <div role="alert" className="grid gap-2 text-sm text-destructive">
          <p>{state.error}</p>
          {state.lines?.map((line) => (
            <p key={line.variantId}>
              {line.reason}
              {line.available !== null ? ` Available: ${line.available}.` : ""}
            </p>
          ))}
        </div>
      )}
      <Button type="submit" disabled={pending}>
        {pending ? "Placing Checkout…" : "Place Checkout"}
      </Button>
    </form>
  );
}

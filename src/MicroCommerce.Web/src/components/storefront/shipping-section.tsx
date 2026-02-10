"use client";

import { type FormEvent, useState } from "react";
import { useSession } from "next-auth/react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import type { ShippingAddressDto } from "@/lib/api";

interface ShippingSectionProps {
  onComplete: (data: ShippingAddressDto) => void;
}

export function ShippingSection({ onComplete }: ShippingSectionProps) {
  const { data: session } = useSession();

  const [name, setName] = useState("");
  const [email, setEmail] = useState(session?.user?.email ?? "");
  const [street, setStreet] = useState("");
  const [city, setCity] = useState("");
  const [state, setState] = useState("");
  const [zipCode, setZipCode] = useState("");
  const [errors, setErrors] = useState<Record<string, string>>({});

  function validate(): boolean {
    const newErrors: Record<string, string> = {};

    if (!name.trim()) newErrors.name = "Name is required";
    if (!email.trim()) {
      newErrors.email = "Email is required";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      newErrors.email = "Invalid email format";
    }
    if (!street.trim()) newErrors.street = "Street address is required";
    if (!city.trim()) newErrors.city = "City is required";
    if (!state.trim()) newErrors.state = "State is required";
    if (!zipCode.trim()) newErrors.zipCode = "ZIP code is required";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  }

  function handleSubmit(e: FormEvent) {
    e.preventDefault();
    if (!validate()) return;

    onComplete({
      name: name.trim(),
      email: email.trim(),
      street: street.trim(),
      city: city.trim(),
      state: state.trim(),
      zipCode: zipCode.trim(),
    });
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="space-y-2">
        <Label htmlFor="name">Full Name</Label>
        <Input
          id="name"
          value={name}
          onChange={(e) => setName(e.target.value)}
          placeholder="John Doe"
          aria-invalid={!!errors.name}
        />
        {errors.name && (
          <p className="text-xs text-red-500">{errors.name}</p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="email">Email</Label>
        <Input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          placeholder="john@example.com"
          aria-invalid={!!errors.email}
        />
        {errors.email && (
          <p className="text-xs text-red-500">{errors.email}</p>
        )}
      </div>

      <div className="space-y-2">
        <Label htmlFor="street">Street Address</Label>
        <Input
          id="street"
          value={street}
          onChange={(e) => setStreet(e.target.value)}
          placeholder="123 Main St"
          aria-invalid={!!errors.street}
        />
        {errors.street && (
          <p className="text-xs text-red-500">{errors.street}</p>
        )}
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div className="space-y-2">
          <Label htmlFor="city">City</Label>
          <Input
            id="city"
            value={city}
            onChange={(e) => setCity(e.target.value)}
            placeholder="New York"
            aria-invalid={!!errors.city}
          />
          {errors.city && (
            <p className="text-xs text-red-500">{errors.city}</p>
          )}
        </div>

        <div className="space-y-2">
          <Label htmlFor="state">State</Label>
          <Input
            id="state"
            value={state}
            onChange={(e) => setState(e.target.value)}
            placeholder="NY"
            aria-invalid={!!errors.state}
          />
          {errors.state && (
            <p className="text-xs text-red-500">{errors.state}</p>
          )}
        </div>
      </div>

      <div className="space-y-2">
        <Label htmlFor="zipCode">ZIP Code</Label>
        <Input
          id="zipCode"
          value={zipCode}
          onChange={(e) => setZipCode(e.target.value)}
          placeholder="10001"
          aria-invalid={!!errors.zipCode}
        />
        {errors.zipCode && (
          <p className="text-xs text-red-500">{errors.zipCode}</p>
        )}
      </div>

      <Button type="submit" className="w-full" size="lg">
        Continue to Payment
      </Button>
    </form>
  );
}

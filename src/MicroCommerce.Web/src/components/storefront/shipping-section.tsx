"use client";

import { useSession } from "next-auth/react";
import { type FormEvent, useState } from "react";

import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import type { ShippingAddressDto } from "@/lib/api";

const SHIPPING_METHODS = [
  {
    id: "standard",
    name: "Standard Shipping",
    description: "5-7 business days",
    price: "$5.99",
  },
  {
    id: "express",
    name: "Express Shipping",
    description: "2-3 business days",
    price: "$12.99",
  },
  {
    id: "overnight",
    name: "Overnight Shipping",
    description: "1 business day",
    price: "$24.99",
  },
];

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
  const [shippingMethod, setShippingMethod] = useState("standard");
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
    <form id="shipping-form" onSubmit={handleSubmit} className="space-y-5">
      {/* Name and Email row */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div className="space-y-1.5">
          <Label htmlFor="name" className="text-sm font-medium">
            Full Name
          </Label>
          <Input
            id="name"
            value={name}
            onChange={(e) => setName(e.target.value)}
            placeholder="John Doe"
            aria-invalid={!!errors.name}
          />
          {errors.name && (
            <p className="text-xs text-destructive">{errors.name}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="email" className="text-sm font-medium">
            Email
          </Label>
          <Input
            id="email"
            type="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="john@example.com"
            aria-invalid={!!errors.email}
          />
          {errors.email && (
            <p className="text-xs text-destructive">{errors.email}</p>
          )}
        </div>
      </div>

      {/* Street Address */}
      <div className="space-y-1.5">
        <Label htmlFor="street" className="text-sm font-medium">
          Street Address
        </Label>
        <Input
          id="street"
          value={street}
          onChange={(e) => setStreet(e.target.value)}
          placeholder="123 Main Street, Apt 4B"
          aria-invalid={!!errors.street}
        />
        {errors.street && (
          <p className="text-xs text-destructive">{errors.street}</p>
        )}
      </div>

      {/* City, State, ZIP row */}
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <div className="space-y-1.5">
          <Label htmlFor="city" className="text-sm font-medium">
            City
          </Label>
          <Input
            id="city"
            value={city}
            onChange={(e) => setCity(e.target.value)}
            placeholder="New York"
            aria-invalid={!!errors.city}
          />
          {errors.city && (
            <p className="text-xs text-destructive">{errors.city}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="state" className="text-sm font-medium">
            State
          </Label>
          <Input
            id="state"
            value={state}
            onChange={(e) => setState(e.target.value)}
            placeholder="NY"
            aria-invalid={!!errors.state}
          />
          {errors.state && (
            <p className="text-xs text-destructive">{errors.state}</p>
          )}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="zipCode" className="text-sm font-medium">
            ZIP Code
          </Label>
          <Input
            id="zipCode"
            value={zipCode}
            onChange={(e) => setZipCode(e.target.value)}
            placeholder="10001"
            aria-invalid={!!errors.zipCode}
          />
          {errors.zipCode && (
            <p className="text-xs text-destructive">{errors.zipCode}</p>
          )}
        </div>
      </div>

      {/* Shipping Method Selection */}
      <div className="space-y-3">
        <Label className="text-sm font-medium">Shipping Method</Label>
        <RadioGroup
          value={shippingMethod}
          onValueChange={setShippingMethod}
          className="space-y-2"
        >
          {SHIPPING_METHODS.map((method) => (
            <label
              key={method.id}
              htmlFor={`shipping-${method.id}`}
              className="flex cursor-pointer items-center gap-3 rounded-lg border border-border bg-background p-4 transition-colors has-[[data-state=checked]]:border-primary has-[[data-state=checked]]:bg-accent"
            >
              <RadioGroupItem value={method.id} id={`shipping-${method.id}`} />
              <div className="flex flex-1 items-center justify-between">
                <div>
                  <p className="text-sm font-medium text-foreground">
                    {method.name}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {method.description}
                  </p>
                </div>
                <span className="text-sm font-semibold text-foreground">
                  {method.price}
                </span>
              </div>
            </label>
          ))}
        </RadioGroup>
      </div>
    </form>
  );
}

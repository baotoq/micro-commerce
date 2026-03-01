"use client";

import { Shield, ShoppingBag } from "lucide-react";
import { useSearchParams } from "next/navigation";
import { signIn } from "next-auth/react";
import { Suspense } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";

function LoginForm() {
  const searchParams = useSearchParams();
  const callbackUrl = searchParams.get("callbackUrl") ?? "/";

  function handleKeycloakSignIn() {
    signIn("keycloak", { callbackUrl });
  }

  return (
    <div className="flex min-h-screen">
      {/* Left side - Brand panel */}
      <div className="hidden flex-1 flex-col justify-center gap-6 bg-gradient-to-b from-[#2563EB] to-[#1D4ED8] p-16 lg:flex">
        <div className="flex items-center gap-2">
          <ShoppingBag className="size-7 text-white" />
          <span className="text-[22px] font-bold text-white">
            MicroCommerce
          </span>
        </div>
        <h1 className="max-w-[400px] text-[32px] font-bold leading-[1.3] text-white">
          Discover premium products at unbeatable prices.
        </h1>
        <p className="max-w-[380px] text-[15px] leading-[1.5] text-white/80">
          Join thousands of happy customers shopping with confidence.
        </p>
      </div>

      {/* Right side - Login form */}
      <div className="flex w-full flex-col justify-center gap-7 px-8 py-16 sm:px-14 lg:w-[520px] lg:shrink-0">
        {/* Mobile brand header */}
        <div className="mb-4 flex items-center gap-2 lg:hidden">
          <ShoppingBag className="size-6 text-primary" />
          <span className="text-lg font-bold">MicroCommerce</span>
        </div>

        <div className="flex flex-col gap-2">
          <h2 className="text-2xl font-bold text-foreground">Welcome back</h2>
          <p className="text-sm text-muted-foreground">
            Sign in to your account
          </p>
        </div>

        <div className="flex flex-col gap-1.5">
          <Label htmlFor="email">Email</Label>
          <Input
            id="email"
            type="email"
            placeholder="john@example.com"
            disabled
            aria-label="Email address (authentication handled by Keycloak)"
          />
        </div>

        <div className="flex flex-col gap-1.5">
          <Label htmlFor="password">Password</Label>
          <Input
            id="password"
            type="password"
            placeholder="••••••••"
            disabled
            aria-label="Password (authentication handled by Keycloak)"
          />
        </div>

        <Button
          size="lg"
          className="h-[46px] w-full text-[15px] font-semibold"
          onClick={handleKeycloakSignIn}
        >
          Sign In
        </Button>

        <div className="flex items-center gap-3">
          <Separator className="flex-1" />
          <span className="text-[13px] text-muted-foreground">or</span>
          <Separator className="flex-1" />
        </div>

        <Button
          variant="outline"
          size="lg"
          className="h-[46px] w-full gap-2 text-sm font-medium"
          onClick={handleKeycloakSignIn}
        >
          <Shield className="size-[18px]" />
          Continue with Keycloak SSO
        </Button>

        <div className="flex items-center justify-center gap-1">
          <span className="text-[13px] text-muted-foreground">
            Don&apos;t have an account?
          </span>
          <button
            type="button"
            className="text-[13px] font-semibold text-primary hover:underline"
            onClick={handleKeycloakSignIn}
          >
            Sign up
          </button>
        </div>
      </div>
    </div>
  );
}

export default function LoginPage() {
  return (
    <Suspense
      fallback={
        <div className="flex min-h-screen items-center justify-center">
          <div className="text-muted-foreground">Loading...</div>
        </div>
      }
    >
      <LoginForm />
    </Suspense>
  );
}

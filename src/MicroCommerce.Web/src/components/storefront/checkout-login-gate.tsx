"use client";

import { LogIn, User } from "lucide-react";
import { signIn, useSession } from "next-auth/react";
import { useEffect } from "react";

import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";

interface CheckoutLoginGateProps {
  onContinueAsGuest: () => void;
}

export function CheckoutLoginGate({ onContinueAsGuest }: CheckoutLoginGateProps) {
  const { data: session, status } = useSession();

  useEffect(() => {
    if (session) {
      onContinueAsGuest();
    }
  }, [session, onContinueAsGuest]);

  if (status === "loading") {
    return null;
  }

  return (
    <div className="flex items-center justify-center py-16">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <CardTitle className="text-xl">How would you like to checkout?</CardTitle>
          <p className="text-sm text-zinc-500">
            Sign in for a faster experience or continue as a guest.
          </p>
        </CardHeader>
        <CardContent className="space-y-3">
          <Button
            className="w-full"
            size="lg"
            onClick={() => signIn("keycloak")}
            aria-label="Sign in to checkout"
          >
            <LogIn className="mr-2 size-4" />
            Sign In
          </Button>
          <Button
            className="w-full"
            size="lg"
            variant="outline"
            onClick={onContinueAsGuest}
            aria-label="Continue as guest"
          >
            <User className="mr-2 size-4" />
            Continue as Guest
          </Button>
        </CardContent>
      </Card>
    </div>
  );
}

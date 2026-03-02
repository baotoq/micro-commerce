"use client";

import { LogIn, User } from "lucide-react";
import { signIn, useSession } from "next-auth/react";
import { useEffect } from "react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";

interface CheckoutLoginGateProps {
  onContinueAsGuest: () => void;
}

export function CheckoutLoginGate({
  onContinueAsGuest,
}: CheckoutLoginGateProps) {
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
          <CardTitle className="text-xl">Sign in to continue</CardTitle>
          <CardDescription>
            Sign in for a faster experience or continue as a guest.
          </CardDescription>
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
          <div className="relative py-2">
            <div className="absolute inset-0 flex items-center">
              <span className="w-full border-t" />
            </div>
            <div className="relative flex justify-center text-xs uppercase">
              <span className="bg-card px-2 text-muted-foreground">
                or continue as guest
              </span>
            </div>
          </div>
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

import { ThemedLayout } from "@components/themed-layout";
import { authProviderServer } from "@providers/auth-provider";
import { redirect } from "next/navigation";
import React from "react";

export default async function Layout({ children }: React.PropsWithChildren) {
  

  return <ThemedLayout>{children}</ThemedLayout>;
}

async function getData() {
  const { authenticated, redirectTo } = await authProviderServer.check();

  return {
    authenticated,
    redirectTo,
  };
}

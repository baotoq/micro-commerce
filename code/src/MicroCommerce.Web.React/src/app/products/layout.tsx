"use client";

import { Header } from "@components/header";
import { ThemedLayoutV2 } from "@refinedev/antd";
import { useIsAuthenticated } from "@refinedev/core";
import { redirect } from "next/navigation";
import React from "react";

export default function Layout({ children }: React.PropsWithChildren) {
  const { data } = useIsAuthenticated();

  if (!data?.authenticated) {
    return redirect(data?.redirectTo || "/login");
  }

  return <ThemedLayoutV2 Header={Header}>{children}</ThemedLayoutV2>;
}

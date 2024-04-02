import type { Metadata } from "next";
import { Inter } from "next/font/google";

import { AppRouterCacheProvider } from "@mui/material-nextjs/v13-appRouter";
import Button from "@mui/material/Button";

import "./globals.css";
import NavLink from "@/components/nav-link";

const inter = Inter({ subsets: ["latin"] });

export const metadata: Metadata = {
  title: "Micro Commerce",
  description: "",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <Button variant="text">Text</Button>
        <AppRouterCacheProvider>{children}</AppRouterCacheProvider>
      </body>
    </html>
  );
}

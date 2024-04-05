import type { Metadata } from "next";

import "./globals.css";

import { Inter } from "next/font/google";
const inter = Inter({ subsets: ["latin"] });

import { AppRouterCacheProvider } from "@mui/material-nextjs/v13-appRouter";

import SessionProvider from "@/components/session-provider";
import Navbar from "@/components/navbar";

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
        <SessionProvider>
          <Navbar />
          <AppRouterCacheProvider>{children}</AppRouterCacheProvider>
        </SessionProvider>
      </body>
    </html>
  );
}

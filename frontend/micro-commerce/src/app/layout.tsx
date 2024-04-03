import type { Metadata } from "next";
import Link from "next/link";

import "./globals.css";

import { Inter } from "next/font/google";
const inter = Inter({ subsets: ["latin"] });

import { AppRouterCacheProvider } from "@mui/material-nextjs/v13-appRouter";

import NextAuthProvider from "../components/auth-provider";
import Navbar from "../components/navbar";

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
        <NextAuthProvider>
          <Navbar />
          <AppRouterCacheProvider>{children}</AppRouterCacheProvider>
        </NextAuthProvider>
      </body>
    </html>
  );
}

import type { Metadata } from "next";

import "./globals.css";

import { Inter } from "next/font/google";
const inter = Inter({ subsets: ["latin"] });

import { AppRouterCacheProvider } from "@mui/material-nextjs/v13-appRouter";

import SessionProvider from "@/components/session-provider";
import Navbar from "@/components/navbar";
import { getServerSession } from "next-auth";

export const metadata: Metadata = {
  title: "Micro Commerce",
  description: "",
};

export default async function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const session = await getServerSession();

  return (
    <html lang="en">
      <body className={inter.className}>
        <SessionProvider session={session}>
          <Navbar />
          <AppRouterCacheProvider>{children}</AppRouterCacheProvider>
        </SessionProvider>
      </body>
    </html>
  );
}

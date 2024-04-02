import type { Metadata } from "next";
import { Inter } from "next/font/google";

import {
  DeploymentUnitOutlined,
  HeartTwoTone,
  PieChartOutlined,
  SlidersOutlined,
  TeamOutlined,
  UnorderedListOutlined,
} from "@ant-design/icons";


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
        <div>{children}</div>
      </body>
    </html>
  );
}

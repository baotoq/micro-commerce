import type { Metadata } from "next";
import Link from "next/link";

import "./globals.css";

import { Inter } from "next/font/google";
const inter = Inter({ subsets: ["latin"] });

import { AppRouterCacheProvider } from "@mui/material-nextjs/v13-appRouter";
import AppBar from "@mui/material/AppBar";
import Box from "@mui/material/Box";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import MenuIcon from "@mui/icons-material/Menu";

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
        <Box sx={{ flexGrow: 1 }}>
          <AppBar position="static">
            <Toolbar>
              <IconButton
                size="large"
                edge="start"
                color="inherit"
                aria-label="menu"
                sx={{ mr: 2 }}
              >
                <MenuIcon />
              </IconButton>
              <Typography
                variant="h6"
                component={Link}
                href="/"
                sx={{ flexGrow: 1 }}
              >
                My Commerce
              </Typography>
              <Button component={Link} color="inherit" href="/login">
                Login
              </Button>
              <Button component={Link} color="inherit" href="/register">
                Register
              </Button>
            </Toolbar>
          </AppBar>
        </Box>
        <AppRouterCacheProvider>{children}</AppRouterCacheProvider>
      </body>
    </html>
  );
}

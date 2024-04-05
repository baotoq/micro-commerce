"use client";
import React from "react";
import AppBar from "@mui/material/AppBar";
import Box from "@mui/material/Box";
import Toolbar from "@mui/material/Toolbar";
import Typography from "@mui/material/Typography";
import Button from "@mui/material/Button";
import IconButton from "@mui/material/IconButton";
import MenuIcon from "@mui/icons-material/Menu";
import Link from "next/link";
import { signIn, signOut, useSession } from "next-auth/react";

export default function Navbar() {
  const { data: session, status } = useSession();

  return (
    <div>
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
            <Typography variant="h6" sx={{ flexGrow: 1 }}>
              <Link href="/">My Commerce</Link>
            </Typography>
            {status === "authenticated" ? (
              <>
                <Button color="inherit">{session.user?.email}</Button>
                <Button color="inherit" onClick={() => signOut()}>
                  Logout
                </Button>
              </>
            ) : (
              <>
                <Button color="inherit" onClick={() => signIn()}>
                  Login
                </Button>
                <Button component={Link} color="inherit" href="/register">
                  Register
                </Button>
              </>
            )}
          </Toolbar>
        </AppBar>
      </Box>
    </div>
  );
}

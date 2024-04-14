"use client";
import React from "react";
import MenuIcon from "@mui/icons-material/Menu";
import Link from "next/link";
import { signIn, signOut, useSession } from "next-auth/react";
import {
  Avatar,
  Button,
  IconButton,
  Typography,
  Toolbar,
  Box,
  AppBar,
  BadgeProps,
  Badge,
  Popover,
  Divider,
} from "@mui/material";
import { styled } from "@mui/material/styles";
import ShoppingCartIcon from "@mui/icons-material/ShoppingCart";
import { useProductItemsCountSelector } from "@/lib/store";

const StyledBadge = styled(Badge)<BadgeProps>(({ theme }) => ({
  "& .MuiBadge-badge": {
    right: -3,
    top: 13,
    border: `2px solid ${theme.palette.background.paper}`,
    padding: "0 4px",
  },
}));

export default function Navbar() {
  const productItemsCount = useProductItemsCountSelector(); 

  const { data: session, status } = useSession();
  const [anchorEl, setAnchorEl] = React.useState<HTMLButtonElement | null>(null);

  const handleClick = (event: React.MouseEvent<HTMLButtonElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };
  const open = Boolean(anchorEl);
  const id = open ? "simple-popover" : undefined;

  return (
    <div>
      <Box sx={{ flexGrow: 1 }}>
        <AppBar position="static">
          <Toolbar>
            <IconButton size="large" edge="start" color="inherit" aria-label="menu" sx={{ mr: 2 }}>
              <MenuIcon />
            </IconButton>
            <Typography variant="h6" sx={{ flexGrow: 1 }}>
              <Link href="/">My Commerce</Link>
            </Typography>
            {status === "authenticated" ? (
              <>
                <Button color="inherit" onClick={handleClick}>
                  <Avatar className="mr-2" src="/broken-image.jpg" sx={{ width: 32, height: 32 }} />
                  {session.user?.email}
                </Button>

                <Popover
                  id={id}
                  open={open}
                  anchorEl={anchorEl}
                  onClose={handleClose}
                  anchorOrigin={{
                    vertical: "bottom",
                    horizontal: "left",
                  }}
                >
                  <Typography className="py-1">
                    <div className="py-1 px-5">
                      <Button color="inherit" component={Link} href="/profile">
                        Profile
                      </Button>
                    </div>
                    <Divider />
                    <div className="py-1 px-5">
                      <Button color="inherit" onClick={() => signOut()}>
                        Logout
                      </Button>
                    </div>
                  </Typography>
                </Popover>
                <IconButton className="text-white">
                  <StyledBadge badgeContent={productItemsCount} color="success">
                    <ShoppingCartIcon />
                  </StyledBadge>
                </IconButton>
              </>
            ) : (
              <>
                <Button color="inherit" component={Link} href="/auth/signin">
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

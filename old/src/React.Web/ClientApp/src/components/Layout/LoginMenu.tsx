import React from "react";
import { Link } from "react-router-dom";

import { IconButton, MenuItem, Menu, Button } from "@material-ui/core";
import { AccountCircle } from "@material-ui/icons";

interface LoginMenuProps {
  isAuthenticated: boolean;
  userName?: string;
}

const LoginMenu = ({ isAuthenticated, userName }: LoginMenuProps) => {
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const open = Boolean(anchorEl);

  const handleMenu = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleClose = () => {
    setAnchorEl(null);
  };

  if (isAuthenticated) {
    return (
      <div>
        <IconButton
          aria-label="account of current user"
          aria-controls="menu-appbar"
          aria-haspopup="true"
          onClick={handleMenu}
          color="inherit"
        >
          <AccountCircle />
        </IconButton>
        <Menu
          id="menu-appbar"
          anchorEl={anchorEl}
          anchorOrigin={{
            vertical: "top",
            horizontal: "right",
          }}
          keepMounted
          transformOrigin={{
            vertical: "top",
            horizontal: "right",
          }}
          open={open}
          onClose={handleClose}
        >
          <MenuItem onClick={handleClose}>{userName}</MenuItem>
          <MenuItem onClick={handleClose} component={Link} to="/authentication/logout">
            Logout
          </MenuItem>
        </Menu>
      </div>
    );
  }

  return (
    <Button color="inherit" component={Link} to="/authentication/login">
      Login
    </Button>
  );
};

export default LoginMenu;

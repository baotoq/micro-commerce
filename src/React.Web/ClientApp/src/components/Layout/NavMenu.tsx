import React from "react";
import { useSelector } from "react-redux";
import { NavLink } from "react-router-dom";
import LoginMenu from "./LoginMenu";

import { selectIsAuthenticated, selectUser } from "../../store/slices/auth-slice";
import { selectTotalItemsInCart } from "../../store/slices/cart-slice";

import { createStyles, makeStyles, Theme } from "@material-ui/core/styles";
import Icon from "@material-ui/core/Icon";

import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
import IconButton from "@material-ui/core/IconButton";
import Badge from "@material-ui/core/Badge";

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      flexGrow: 1,
    },
    menuButton: {
      marginRight: theme.spacing(2),
    },
    title: {
      flexGrow: 1,
    },
  })
);

const NavMenu = () => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const userName = useSelector(selectUser)?.name;
  const totalItemsInCart = useSelector(selectTotalItemsInCart);

  const classes = useStyles();

  return (
    <div className={classes.root}>
      <AppBar position="relative">
        <Toolbar>
          <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
            <NavLink to="/">
              <Icon style={{ color: "white" }}>home</Icon>
            </NavLink>
          </IconButton>
          <Typography variant="h6" className={classes.title}>
            BShop
          </Typography>
          <LoginMenu isAuthenticated={isAuthenticated} userName={userName} />
          <IconButton>
            <Badge badgeContent={totalItemsInCart} color="secondary">
              <NavLink to="/cart">
                <Icon style={{ color: "white" }}>shopping_cart</Icon>
              </NavLink>
            </Badge>
          </IconButton>
        </Toolbar>
      </AppBar>
    </div>
  );
};

export default NavMenu;

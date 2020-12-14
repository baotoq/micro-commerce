import React from "react";
import { useSelector } from "react-redux";
import { NavLink, Link } from "react-router-dom";
import LoginMenu from "./LoginMenu";

import { selectIsAuthenticated, selectUser } from "../../store/slices/auth-slice";
import { selectTotalItemsInCart } from "../../store/slices/cart-slice";

import { fade, createStyles, makeStyles, Theme } from "@material-ui/core/styles";
import Icon from "@material-ui/core/Icon";

import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
import IconButton from "@material-ui/core/IconButton";
import Badge from "@material-ui/core/Badge";
import InputBase from "@material-ui/core/InputBase";
import SearchIcon from "@material-ui/icons/Search";
import Button from "@material-ui/core/Button";

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
    titleText: {
      display: "inline",
      marginRight: "5px",
    },
    search: {
      position: "relative",
      borderRadius: theme.shape.borderRadius,
      backgroundColor: fade(theme.palette.common.white, 0.15),
      "&:hover": {
        backgroundColor: fade(theme.palette.common.white, 0.25),
      },
      marginLeft: 0,
      width: "100%",
      [theme.breakpoints.up("sm")]: {
        marginLeft: theme.spacing(1),
        width: "auto",
      },
    },
    searchIcon: {
      padding: theme.spacing(0, 2),
      height: "100%",
      position: "absolute",
      pointerEvents: "none",
      display: "flex",
      alignItems: "center",
      justifyContent: "center",
    },
    inputRoot: {
      color: "inherit",
    },
    inputInput: {
      padding: theme.spacing(1, 1, 1, 0),
      // vertical padding + font size from searchIcon
      paddingLeft: `calc(1em + ${theme.spacing(4)}px)`,
      transition: theme.transitions.create("width"),
      width: "100%",
      [theme.breakpoints.up("sm")]: {
        width: "12ch",
        "&:focus": {
          width: "20ch",
        },
      },
    },
  })
);

const NavMenu = () => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const user = useSelector(selectUser);
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
          <div className={classes.title}>
            <Typography variant="h6" className={classes.titleText}>
              BShop
            </Typography>
            {user?.role === "Admin" && (
              <span>
                <Button color="inherit" component={Link} to="/admin/dashboard">
                  Dashboard
                </Button>
                <Button color="inherit" component={Link} to="/admin/user">
                  User
                </Button>
                <Button color="inherit" component={Link} to="/admin/category">
                  Category
                </Button>
                <Button color="inherit" component={Link} to="/admin/product">
                  Product
                </Button>
                <Button color="inherit" component={Link} to="/admin/review">
                  Review
                </Button>
                <Button color="inherit" component={Link} to="/admin/order">
                  Order
                </Button>
              </span>
            )}
          </div>
          <div className={classes.search}>
            <div className={classes.searchIcon}>
              <SearchIcon />
            </div>
            <InputBase
              placeholder="Search…"
              classes={{
                root: classes.inputRoot,
                input: classes.inputInput,
              }}
              inputProps={{ "aria-label": "search" }}
            />
          </div>
          <LoginMenu isAuthenticated={isAuthenticated} userName={user?.name} />
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

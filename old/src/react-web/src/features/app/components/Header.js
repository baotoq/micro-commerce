import React from "react";
import { useSelector } from "react-redux";
import { NavLink } from "react-router-dom";
import { AuthMenu } from "./AuthMenu";

import {
  selectIsAuthenticated,
  selectUser,
} from "../../../store/slices/auth-slice.js";

import { fade, createStyles, makeStyles } from "@material-ui/core/styles";
import Icon from "@material-ui/core/Icon";

import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
import IconButton from "@material-ui/core/IconButton";

const useStyles = makeStyles((theme) =>
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

export const Header = () => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const user = useSelector(selectUser);

  const classes = useStyles();

  return (
    <div className={classes.root}>
      <AppBar position="relative">
        <Toolbar>
          <IconButton
            edge="start"
            className={classes.menuButton}
            color="inherit"
            aria-label="menu"
          >
            <NavLink to="/">
              <Icon style={{ color: "white" }}>home</Icon>
            </NavLink>
          </IconButton>
          <div className={classes.title}>
            <Typography variant="h6" className={classes.titleText}>
              BShop
            </Typography>
          </div>
          <AuthMenu isAuthenticated={isAuthenticated} userName={user?.name} />
        </Toolbar>
      </AppBar>
    </div>
  );
};

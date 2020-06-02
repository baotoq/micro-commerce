import React from "react";
import { useSelector } from "react-redux";
import { Link } from "react-router-dom";
import LoginMenu from "./LoginMenu";

import { selectIsAuthenticated, selectUser } from "../../store/slices/auth-slice";
import { selectCategories } from "../../store/slices/category-slice";

import { AppBar, Toolbar, Typography, IconButton } from "@material-ui/core";
import { createStyles, makeStyles, Theme } from "@material-ui/core/styles";
import MenuIcon from "@material-ui/icons/Menu";

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
  const categories = useSelector(selectCategories);

  const classes = useStyles();

  return (
    <div className={classes.root}>
      <AppBar position="relative">
        <Toolbar>
          <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
            <MenuIcon />
          </IconButton>
          <Typography variant="h6" className={classes.title}>
            BShop
          </Typography>
          <LoginMenu isAuthenticated={isAuthenticated} userName={userName} />
        </Toolbar>
      </AppBar>
    </div>
  );
};

export default NavMenu;

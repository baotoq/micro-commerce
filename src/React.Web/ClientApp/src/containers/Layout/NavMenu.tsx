import React from "react";
import { useSelector } from "react-redux";
import { NavLink, Link } from "react-router-dom";
import LoginMenu from "./LoginMenu";

import { selectIsAuthenticated, selectUser } from "../../store/slices/auth-slice";
import { selectCategories } from "../../store/slices/category-slice";

import { createStyles, makeStyles, Theme } from "@material-ui/core/styles";
import Icon from "@material-ui/core/Icon";

import { AppBar, Toolbar, Typography, IconButton } from "@material-ui/core";
import Tabs from "@material-ui/core/Tabs";
import Tab from "@material-ui/core/Tab";

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

  const classes = useStyles();

  return (
    <div className={classes.root}>
      <AppBar position="relative">
        <Toolbar>
          <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
            <NavLink to="/">
              <Icon>home</Icon>
            </NavLink>
          </IconButton>
          <Typography variant="h6" className={classes.title}>
            BShop
          </Typography>
          <LoginMenu isAuthenticated={isAuthenticated} userName={userName} />
        </Toolbar>
      </AppBar>
      <NavTabs></NavTabs>
    </div>
  );
};

const NavTabs = () => {
  const [tabValue, setTabValue] = React.useState(0);
  const categories = useSelector(selectCategories);

  return (
    <div>
      <AppBar position="static" color="default">
        <Tabs
          variant="scrollable"
          value={tabValue}
          onChange={(event, newValue) => setTabValue(newValue)}
          scrollButtons="auto"
        >
          <Tab to="/" component={Link} label="Home" value={0} />
          {categories.map((c) => (
            <Tab to={`/category/${c.id}`} component={Link} label={c.name} value={c.id} key={c.id} />
          ))}
        </Tabs>
      </AppBar>
    </div>
  );
};

export default NavMenu;

import React from "react";
import Link from "next/link";

import { signIn, signOut, useSession } from "next-auth/client";

import { createStyles, makeStyles, Theme } from "@material-ui/core/styles";
import AppBar from "@material-ui/core/AppBar";
import Toolbar from "@material-ui/core/Toolbar";
import Typography from "@material-ui/core/Typography";
import Button from "@material-ui/core/Button";
import IconButton from "@material-ui/core/IconButton";
import Icon from "@material-ui/core/Icon";

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

export default function SimpleAppBar() {
  const classes = useStyles();
  const [session] = useSession();

  return (
    <div className={classes.root}>
      <AppBar position="static">
        <Toolbar>
          <IconButton edge="start" className={classes.menuButton} color="inherit" aria-label="menu">
            <Icon>menu</Icon>
          </IconButton>
          <Typography variant="h6" className={classes.title}>
            <Link href="/">Micro Commerce</Link>
          </Typography>
          {!session && (
            <>
              <Button color="inherit" onClick={() => signIn("identity-server4")}>
                Login
              </Button>
            </>
          )}
          {session && (
            <>
              <Button color="inherit">{session.user.name}</Button>
              <Button color="inherit" onClick={() => signOut()}>
                Log out
              </Button>
            </>
          )}
        </Toolbar>
      </AppBar>
    </div>
  );
}

import React from "react";
import Link from "next/link";

import Typography from "@material-ui/core/Typography";
import Container from "@material-ui/core/Container";
import MuLink from "@material-ui/core/Link";

import { makeStyles, Theme } from "@material-ui/core/styles";

const useStyles = makeStyles((theme: Theme) => ({
  footer: {
    padding: theme.spacing(3, 2),
    marginTop: theme.spacing(4),
    textAlign: "center",
  },
}));

export default function Footer() {
  const classes = useStyles();

  return (
    <footer className={classes.footer}>
      <Container maxWidth="sm">
        <Typography variant="body2" color="textSecondary">
          {"Copyright © "}
          <Link href="/" passHref>
            <MuLink>Micro Commerce</MuLink>
          </Link>{" "}
          {new Date().getFullYear()}
          {"."}
        </Typography>
      </Container>
    </footer>
  );
}

import Head from "next/head";
import Router from "next/router";
import { AppProps } from "next/app";

import Container from "@material-ui/core/Container";
import CssBaseline from "@material-ui/core/CssBaseline";
import { ThemeProvider, createMuiTheme, makeStyles } from "@material-ui/core/styles";

import NProgress from "nprogress";

import { Provider as AuthProvider } from "next-auth/client";

import { Provider } from "react-redux";
import store from "~/store";

import AppBar from "~/components/app-bar";
import Footer from "~/components/footer";

import "nprogress/nprogress.css";
import "~/styles/globals.scss";

const useStyles = makeStyles((theme) => ({
  main: {
    marginTop: theme.spacing(3),
    marginBottom: theme.spacing(2),
  },
}));

export default function App({ Component, pageProps }: AppProps) {
  const classes = useStyles();

  return (
    <AuthProvider session={pageProps.session}>
      <Provider store={store}>
        <Head>
          <title>Micro Commerce</title>
          <link rel="icon" href="/favicon.ico" />
          <meta name="viewport" content="minimum-scale=1, initial-scale=1, width=device-width" />
          <meta name="description" content="MicroCommerce Web" />
        </Head>
        <ThemeProvider theme={createMuiTheme()}>
          <div>
            <CssBaseline />
            <AppBar />
            <Container component="main" className={classes.main} maxWidth="lg">
              <Component {...pageProps} />
            </Container>
            <Footer />
          </div>
        </ThemeProvider>
      </Provider>
    </AuthProvider>
  );
}

NProgress.configure({ showSpinner: false });

Router.events.on("routeChangeStart", (url) => NProgress.start());
Router.events.on("routeChangeComplete", () => NProgress.done());
Router.events.on("routeChangeError", () => NProgress.done());

import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { BrowserRouter, Switch, Route } from "react-router-dom";

import CssBaseline from "@material-ui/core/CssBaseline";
import Container from "@material-ui/core/Container";

import NavMenu from "../components/Layout/NavMenu";

import Home from "./Home";
import Category from "./Category";
import Product from "./Product";
import Cart from "./Cart";
import Auth from "./Auth";
import CategoriesTabs from "../components/CategoriesTabs";
import Loading from "../components/Loading";

import { fetchCategoriesAsync } from "../store/slices/category-slice";

import { selectLoading } from "../store/slices/app-slice";

const App = () => {
  const loading = useSelector(selectLoading);
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchCategoriesAsync());
  }, []);

  return (
    <BrowserRouter basename={"/"}>
      <Loading open={loading} />
      <CssBaseline />
      <NavMenu />
      <CategoriesTabs />
      <div style={{ marginBottom: "10px" }}></div>
      <Container maxWidth="lg">
        <Switch>
          <Route path="/authentication/:action" component={Auth} />
          <Route path="/category/:id/page/:page" component={Category} />
          <Route path="/product/:id" component={Product} />
          <Route path="/cart" component={Cart} />
          <Route path="/" component={Home} />
        </Switch>
      </Container>
    </BrowserRouter>
  );
};

export default App;

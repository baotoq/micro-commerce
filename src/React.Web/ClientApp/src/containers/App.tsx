import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { BrowserRouter, Switch, Route } from "react-router-dom";

import { Container, Jumbotron } from "react-bootstrap";

import NavMenu from "./Layout/NavMenu";

import Home from "./Home";
import Category from "./Category";
import Product from "./Product";
import Auth from "./Auth";

import { fetchCategoriesAsync } from "../store/slices/category-slice";

const App = () => {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchCategoriesAsync());
  }, [dispatch]);

  return (
    <BrowserRouter basename={"/"}>
      <NavMenu />
      <Jumbotron>
        <h1 className="display-4 text-primary">BShop</h1>
      </Jumbotron>
      <Container fluid>
        <Switch>
          <Route path="/authentication/:action" component={Auth} />
          <Route path="/category/:id" component={Category} />
          <Route path="/product/:id" component={Product} />
          <Route path="/" component={Home} />
        </Switch>
      </Container>
    </BrowserRouter>
  );
};

export default App;

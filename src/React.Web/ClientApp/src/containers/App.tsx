import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { BrowserRouter, Switch, Route, Link } from "react-router-dom";

import NavMenu from "./Layout/NavMenu";

import Home from "./Home";
import Product from "./Product";
import Auth from "./Auth";

import { fetchCategoriesAsync } from "../store/slices/categories-slice";

const App = () => {
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(fetchCategoriesAsync());
  }, [dispatch]);

  return (
    <BrowserRouter basename={"/"}>
      <NavMenu />
      <div className="container">
        <div>
          <Link to="/product">Product</Link>
        </div>
        <Switch>
          <Route path="/authentication/:action" component={Auth} />
          <Route path="/product" component={Product} />
          <Route path="/" component={Home} />
        </Switch>
      </div>
    </BrowserRouter>
  );
};

export default App;

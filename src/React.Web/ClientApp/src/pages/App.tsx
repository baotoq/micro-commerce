import React, { useEffect } from "react";
import { useDispatch, useSelector } from "react-redux";
import { BrowserRouter, Switch, Route } from "react-router-dom";

import DefaultLayout from "../components/Layout/DefaultLayout";
import AdminLayout from "../components/Layout/AdminLayout";

import CssBaseline from "@material-ui/core/CssBaseline";

import NavMenu from "../components/Layout/NavMenu";

import Home from "./Home";
import Category from "./Category";
import Product from "./Product";
import Cart from "./Cart";
import Auth from "./Auth";

import Dashboard from "./Admin/Dashboard";
import AdminUser from "./Admin/User";
import AdminCategory from "./Admin/Category";
import AdminProduct from "./Admin/Product";
import AdminReview from "./Admin/Review";

import Loading from "../components/Loading";

import { selectLoading } from "../store/slices/app-slice";
import { checkLoginAsync } from "../store/slices/auth-slice";

const App = () => {
  const loading = useSelector(selectLoading);
  const dispatch = useDispatch();

  useEffect(() => {
    dispatch(checkLoginAsync());
  }, []);

  return (
    <div>
      <Loading open={loading} />
      <CssBaseline />
      <NavMenu />
      <Switch>
        <Route path="/authentication/:action" component={Auth} />
        <Route path="/admin/:path?" exact>
          <AdminLayout>
            <Switch>
              <Route path="/admin/dashboard" exact component={Dashboard} />
              <Route path="/admin/user" exact component={AdminUser} />
              <Route path="/admin/category" exact component={AdminCategory} />
              <Route path="/admin/product" exact component={AdminProduct} />
              <Route path="/admin/review" exact component={AdminReview} />
              <Route component={() => <div>404 Main Admin</div>} exact path="/admin/*" />
            </Switch>
          </AdminLayout>
        </Route>
        <Route>
          <DefaultLayout>
            <Switch>
              <Route path="/category/:id/page/:page" component={Category} />
              <Route path="/product/:id" component={Product} />
              <Route path="/cart" component={Cart} />
              <Route path="/" exact component={Home} />
              <Route component={() => <div>404 Main</div>} exact path="/*" />
            </Switch>
          </DefaultLayout>
        </Route>
      </Switch>
    </div>
  );
};

export default App;

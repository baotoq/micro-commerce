import React from "react";
import { useDispatch, useSelector } from "react-redux";
import { Switch, Route, Link } from "react-router-dom";

import CssBaseline from "@material-ui/core/CssBaseline";

import { Header } from "./components/Header";
import { Auth } from "../auth/Auth";
import { Home } from "../home/Home";

import {
  fetchCategoriesAsync,
  selectCategories,
} from "../../store/slices/category-slice";

export const App = () => {
  const dispatch = useDispatch();
  const categories = useSelector(selectCategories);

  React.useEffect(() => {
    dispatch(fetchCategoriesAsync());
  }, [dispatch]);

  return (
    <div className="App">
      <CssBaseline />
      <Header />
      {categories.map((c) => (
        <div key={c.id}>
          <Link to={`/categories/${c.id}`}>{c.name}</Link>
        </div>
      ))}
      <Switch>
        <Switch>
          <Route exact path="/" component={Home} />
          <Route path="/authentication/:action" component={Auth} />
        </Switch>
      </Switch>
    </div>
  );
};

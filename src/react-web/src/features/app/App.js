import React from "react";
import { Switch, Route } from "react-router-dom";

import { Header } from "./components/Header";
import { Auth } from "../auth/Auth";

import CssBaseline from "@material-ui/core/CssBaseline";

export const App = () => {
  return (
    <div className="App">
      <CssBaseline />
      <Header />
      <Switch>
        <Switch>
          <Route path="/authentication/:action" component={Auth} />
        </Switch>
      </Switch>
    </div>
  );
}

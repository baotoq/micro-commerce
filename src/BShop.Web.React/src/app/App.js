import React from "react";
import PropTypes from "prop-types";

import { Provider } from "react-redux";
import { BrowserRouter as Router, Route } from "react-router-dom";

import Todo from "../features/todo/Todo";
import Login from "../features/auth/login/Login";
import Register from "../features/auth/register/Register";
import Catalog from "../features/catalog/Catalog";

const App = ({ store }) => (
  <Provider store={store}>
    <Router>
      <Route exact path="/" component={Todo} />
      <Route path="/catalog" component={Catalog} />
      <Route path="/login" component={Login} />
      <Route path="/register" component={Register} />
    </Router>
  </Provider>
);

App.propTypes = {
  store: PropTypes.object.isRequired
};

export default App;

import React from "react";
import ReactDOM from "react-dom";

import store from "./store";
import App from "./app/App";

import * as serviceWorker from "./serviceWorker";

const rootElement = document.getElementById("root");
ReactDOM.render(<App store={store} />, rootElement);

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: https://bit.ly/CRA-PWA
serviceWorker.unregister();

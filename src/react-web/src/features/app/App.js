import React from "react";
import { Switch, Route, Link } from "react-router-dom";

import { Counter } from "../counter/Counter";
import "./App.css";
function Home() {
  console.log("b");
  return <h2>Home</h2>;
}

function About() {
  console.log("a");
  return <h2>About</h2>;
}
function App() {
  return (
    <div className="App">
      <header className="App-header">
        <Counter />
        <p>
          Edit <code>src/App.js</code> and save to reload.
        </p>
        <span>
          <span>Learn </span>
          <a
            className="App-link"
            href="https://reactjs.org/"
            target="_blank"
            rel="noopener noreferrer"
          >
            React
          </a>
          <span>, </span>
          <a
            className="App-link"
            href="https://redux.js.org/"
            target="_blank"
            rel="noopener noreferrer"
          >
            Redux
          </a>
          <span>, </span>
          <a
            className="App-link"
            href="https://redux-toolkit.js.org/"
            target="_blank"
            rel="noopener noreferrer"
          >
            Redux Toolkit
          </a>
          ,<span> and </span>
          <a
            className="App-link"
            href="https://react-redux.js.org/"
            target="_blank"
            rel="noopener noreferrer"
          >
            React Redux
          </a>
        </span>
      </header>
      <div>
        <ul>
          <li>
            <Link to="/">Home</Link>
          </li>
          <li>
            <Link to="/about">About</Link>
          </li>
          <li>
            <Link to="/topics">Topics</Link>
          </li>
        </ul>

        <Switch>
          <Route path="/about" component={About}></Route>
          <Route path="/topics">asdas</Route>
          <Route exact path="/">
            <Home />
          </Route>
        </Switch>
      </div>
    </div>
  );
}

export default App;

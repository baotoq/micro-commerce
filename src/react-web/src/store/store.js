import {
  configureStore,
  combineReducers,
  getDefaultMiddleware,
} from "@reduxjs/toolkit";

import { connectRouter, routerMiddleware } from "connected-react-router";
import { createBrowserHistory } from "history";

import { createEpicMiddleware, combineEpics } from "redux-observable";

import createEpics from "./createEpics";
import * as countSlice from "../features/counter/counterSlice";

export const history = createBrowserHistory();

const epicMiddleware = createEpicMiddleware();

const middleware = [
  ...getDefaultMiddleware(),
  routerMiddleware(history),
  epicMiddleware,
];

const rootEpic = combineEpics(...createEpics([countSlice]));

export default configureStore({
  reducer: combineReducers({
    router: connectRouter(history),
    counter: countSlice.default,
  }),
  middleware,
});

epicMiddleware.run(rootEpic);

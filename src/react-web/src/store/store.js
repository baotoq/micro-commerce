import {
  configureStore,
  combineReducers,
  getDefaultMiddleware,
} from "@reduxjs/toolkit";

import { connectRouter, routerMiddleware } from "connected-react-router";
import { createBrowserHistory } from "history";

import { createEpicMiddleware, combineEpics } from "redux-observable";

import createEpics from "./create-epics";
import * as authSlice from "./slices/auth-slice";

export const history = createBrowserHistory();

const epicMiddleware = createEpicMiddleware();

const middleware = [
  ...getDefaultMiddleware(),
  routerMiddleware(history),
  epicMiddleware,
];

const rootEpic = combineEpics(...createEpics([authSlice]));

export default configureStore({
  reducer: combineReducers({
    router: connectRouter(history),
    auth: authSlice.default,
  }),
  middleware,
});

epicMiddleware.run(rootEpic);

import {
  configureStore,
  combineReducers,
  getDefaultMiddleware,
} from "@reduxjs/toolkit";
import { createEpicMiddleware, combineEpics } from "redux-observable";

import createEpics from "./createEpics";
import * as countSlice from "../features/counter/counterSlice";
const epicMiddleware = createEpicMiddleware();

const middleware = [...getDefaultMiddleware(), epicMiddleware];

const rootEpic = combineEpics(...createEpics([countSlice]));

export default configureStore({
  reducer: combineReducers({
    counter: countSlice.default,
  }),
  middleware,
});

epicMiddleware.run(rootEpic);

import { configureStore, ThunkAction, Action, ThunkDispatch, getDefaultMiddleware } from "@reduxjs/toolkit";
import { combineReducers } from "redux";
import { connectRouter, routerMiddleware } from "connected-react-router";
import { createBrowserHistory, History } from "history";
import app from "./slices/app-slice";
import auth from "./slices/auth-slice";
import category from "./slices/category-slice";
import cart from "./slices/cart-slice";

export const history = createBrowserHistory();

const createRootReducer = (history: History) =>
  combineReducers({
    router: connectRouter(history),
    app,
    auth,
    category,
    cart,
  });

const middleware = [...getDefaultMiddleware(), routerMiddleware(history)];

export const store = configureStore({
  reducer: createRootReducer(history),
  middleware,
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = ThunkDispatch<RootState, unknown, Action<string>>;
export type AppThunk<ReturnType = void> = ThunkAction<ReturnType, RootState, unknown, Action<string>>;

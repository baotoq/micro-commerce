import { configureStore, ThunkAction, Action } from "@reduxjs/toolkit";
import app from "./slices/app-slice";
import auth from "./slices/auth-slice";
import category from "./slices/category-slice";
import cart from "./slices/cart-slice";

export const store = configureStore({
  reducer: {
    app,
    auth,
    category,
    cart,
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<ReturnType, RootState, unknown, Action<string>>;

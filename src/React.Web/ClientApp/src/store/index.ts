import { configureStore, ThunkAction, Action } from "@reduxjs/toolkit";
import authReducer from "./slices/auth-slice";
import categoriesReducer from "./slices/categories-slice";

export const store = configureStore({
  reducer: {
    auth: authReducer,
    categories: categoriesReducer
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<ReturnType, RootState, unknown, Action<string>>;

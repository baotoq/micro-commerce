import { configureStore, ThunkAction, Action } from "@reduxjs/toolkit";
import authReducer from "./slices/auth-slice";
import categoryReducer from "./slices/category-slice";

export const store = configureStore({
  reducer: {
    auth: authReducer,
    category: categoryReducer
  },
});

export type RootState = ReturnType<typeof store.getState>;
export type AppThunk<ReturnType = void> = ThunkAction<ReturnType, RootState, unknown, Action<string>>;

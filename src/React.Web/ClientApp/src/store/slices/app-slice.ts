import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "../";

interface AppState {
  loading: boolean;
}

const initialState: AppState = {
  loading: false,
};

export const appSlice = createSlice({
  name: "app",
  initialState,
  reducers: {
    setLoading: (state, { payload }: PayloadAction<boolean>) => {
      state.loading = payload;
    },
  },
});

export const { setLoading } = appSlice.actions;

export const selectLoading = (state: RootState) => state.app.loading;

export default appSlice.reducer;

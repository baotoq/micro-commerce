import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "../";

import authService from "../../services/auth-service";
import { User } from "../../models";

interface AuthState {
  user?: User;
}

const initialState: AuthState = {
  user: undefined,
};

export const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    loginSuccess: (state, { payload }: PayloadAction<User>) => {
      state.user = payload;
    },
    logoutSuccess: (state) => {
      state.user = undefined;
    },
  },
});

export const { loginSuccess, logoutSuccess } = authSlice.actions;

export const loginAsync = (): AppThunk => async (dispatch) => {
  await authService.loginAsync();
};

export const completeLoginAsync = (): AppThunk => async (dispatch) => {
  await authService.completeLoginAsync(window.location.href);
  const user = await authService.getUserAsync();
  dispatch(loginSuccess({ id: user?.profile.sub, name: user?.profile.name } as User));
};

export const logoutAsync = (): AppThunk => async (dispatch) => {
  await authService.logoutAsync();
  dispatch(logoutSuccess());
};

export const completeLogoutAsync = (): AppThunk => async (dispatch) => {
  await authService.completeLogoutAsync(window.location.href);
};

export const selectIsAuthenticated = (state: RootState) => !!state.auth.user;
export const selectUser = (state: RootState) => state.auth.user;

export default authSlice.reducer;

import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { AppThunk, RootState } from "../";
import { push } from "connected-react-router";
import { setLoading } from "./app-slice";
import { loadCartAsync } from "./cart-slice";
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

export const completeLoginAsync = (): AppThunk<Promise<void>> => async (dispatch) => {
  dispatch(setLoading(true));
  await authService.completeLoginAsync(window.location.href);
  const user = await authService.getUserAsync();
  dispatch(loginSuccess({ id: user?.profile.sub, name: user?.profile.name, role: user?.profile.role } as User));
  dispatch(setLoading(false));
  dispatch(push("/"));
};

export const logoutAsync = (): AppThunk => async (dispatch) => {
  await authService.logoutAsync();
};

export const completeLogoutAsync = (): AppThunk => async (dispatch) => {
  dispatch(setLoading(true));
  await authService.completeLogoutAsync(window.location.href);
  dispatch(logoutSuccess());
  dispatch(setLoading(false));
  dispatch(push("/"));
};

export const checkLoginAsync = (): AppThunk => async (dispatch) => {
  const user = await authService.getUserAsync();
  if (user) {
    dispatch(loginSuccess({ id: user.profile.sub, name: user.profile.name, role: user.profile.role } as User));
    dispatch(loadCartAsync());
  }
};

export const selectIsAuthenticated = (state: RootState) => !!state.auth.user;
export const selectUser = (state: RootState) => state.auth.user;

export default authSlice.reducer;

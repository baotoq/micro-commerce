import { createSlice } from "@reduxjs/toolkit";
import { push } from "connected-react-router";
import authService from "../../services/authService";

const initialState = {
  user: undefined,
};

export const authSlice = createSlice({
  name: "auth",
  initialState,
  reducers: {
    loginSuccess: (state, { payload }) => {
      state.user = payload;
    },
    logoutSuccess: (state) => {
      state.user = undefined;
    },
  },
});

export const { loginSuccess, logoutSuccess } = authSlice.actions;

export const loginAsync = () => async (dispatch) => {
  await authService.loginAsync();
};

export const completeLoginAsync = () => async (dispatch) => {
  await authService.completeLoginAsync(window.location.href);
  const user = await authService.getUserAsync();
  dispatch(loginSuccess({ id: user.profile.sub, name: user.profile.name, role: user.profile.role }));
  dispatch(push("/"));
};

export const logoutAsync = () => async (dispatch) => {
  await authService.logoutAsync();
};

export const completeLogoutAsync = () => async (dispatch) => {
  await authService.completeLogoutAsync(window.location.href);
  dispatch(logoutSuccess());
  dispatch(push("/"));
};

export const checkLoginAsync = () => async (dispatch) => {
  const user = await authService.getUserAsync();
  if (user) {
    dispatch(loginSuccess({ id: user.profile.sub, name: user.profile.name, role: user.profile.role }));
  }
};

export const selectIsAuthenticated = (state) => !!state.auth.user;
export const selectUser = (state) => state.auth.user;

export default authSlice.reducer;

import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useParams } from "react-router-dom";
import {
  loginAsync,
  logoutAsync,
  completeLoginAsync,
  completeLogoutAsync,
} from "./authSlice";

const Auth = () => {
  const dispatch = useDispatch();
  const { action } = useParams();

  useEffect(() => {
    switch (action) {
      case "login":
        dispatch(loginAsync());
        break;
      case "login-callback":
        dispatch(completeLoginAsync());
        break;
      case "logout":
        dispatch(logoutAsync());
        break;
      case "logout-callback":
        dispatch(completeLogoutAsync());
        break;
      default:
        break;
    }
  }, [dispatch, action]);

  return <div></div>;
};

export default Auth;

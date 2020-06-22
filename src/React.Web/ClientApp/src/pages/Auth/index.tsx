import React, { useEffect } from "react";
import { useDispatch } from "react-redux";
import { useHistory, useParams } from "react-router-dom";
import { loginAsync, logoutAsync, completeLoginAsync, completeLogoutAsync } from "../../store/slices/auth-slice";
import { makeStyles, Theme, createStyles } from "@material-ui/core/styles";
import CircularProgress from "@material-ui/core/CircularProgress";

const useStyles = makeStyles((theme: Theme) =>
  createStyles({
    root: {
      margin: "auto",
    },
  })
);
const Auth = () => {
  const history = useHistory();
  const dispatch = useDispatch();
  const { action } = useParams<{ action: string }>();
  const classes = useStyles();

  useEffect(() => {
    switch (action) {
      case "login":
        dispatch(loginAsync());
        break;
      case "login-callback":
        dispatch(completeLoginAsync());
        history.push("/");
        break;
      case "logout":
        dispatch(logoutAsync());
        break;
      case "logout-callback":
        dispatch(completeLogoutAsync());
        history.push("/");
        break;
      default:
        break;
    }
  }, [dispatch, history, action]);

  return (
    <div className={classes.root}>
      <CircularProgress />
    </div>
  );
};

export default Auth;

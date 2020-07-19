import * as React from "react";
import { Redirect, Route, RouteProps } from "react-router";

export interface ProtectedRouteProps extends RouteProps {
  isAuthenticated: boolean;
  isAllowed: boolean;
  restrictedPath: string;
  authenticationPath: string;
}

const ProtectedRoute = (props: ProtectedRouteProps) => {
  let redirectPath = "";
  if (!props.isAuthenticated) {
    redirectPath = props.authenticationPath;
  }
  if (props.isAuthenticated && !props.isAllowed) {
    redirectPath = props.restrictedPath;
  }

  if (redirectPath) {
    const renderComponent = () => <Redirect to={{ pathname: redirectPath }} />;
    return <Route {...props} component={renderComponent} render={undefined} />;
  } else {
    return <Route {...props} />;
  }
};

export default ProtectedRoute;

import React from "react";
import { Link } from "react-router-dom";
import { NavItem, NavLink } from "reactstrap";

interface IProps {
  isAuthenticated: boolean;
  userName?: string;
}

const LoginMenu = ({ isAuthenticated, userName }: IProps) => {
  if (isAuthenticated) {
    return (
      <>
        <NavItem>
          <NavLink tag={Link} to="/">
            Hello {userName}
          </NavLink>
        </NavItem>
        <NavItem>
          <NavLink tag={Link} to="/authentication/logout">
            Logout
          </NavLink>
        </NavItem>
      </>
    );
  }

  return (
    <>
      <NavItem>
        <NavLink tag={Link} to="/">
          Register
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink tag={Link} to="/authentication/login">
          Login
        </NavLink>
      </NavItem>
    </>
  );
};

export default LoginMenu;

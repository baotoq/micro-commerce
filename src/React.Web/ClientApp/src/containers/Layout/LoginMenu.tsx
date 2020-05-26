import React from "react";
import { Link } from "react-router-dom";

import { Nav, Navbar } from "react-bootstrap";

interface LoginMenuProps {
  isAuthenticated: boolean;
  userName?: string;
}

const LoginMenu = ({ isAuthenticated, userName }: LoginMenuProps) => {
  if (isAuthenticated) {
    return (
      <Nav>
        <Navbar.Text>
          Signed in as: <span className="text-white">{userName}</span>
        </Navbar.Text>
        <Nav.Link as={Link} to="/authentication/logout">
          Log out
        </Nav.Link>
      </Nav>
    );
  }

  return (
    <Nav>
      <Nav.Link as={Link} to="/">
        Register
      </Nav.Link>
      <Nav.Link as={Link} to="/authentication/login">
        Log in
      </Nav.Link>
    </Nav>
  );
};

export default LoginMenu;

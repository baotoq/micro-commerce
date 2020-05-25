import React from "react";
import { Link } from "react-router-dom";

import Nav from "react-bootstrap/Nav";
import NavBar from "react-bootstrap/NavBar";

interface IProps {
  isAuthenticated: boolean;
  userName?: string;
}

const LoginMenu = ({ isAuthenticated, userName }: IProps) => {
  if (isAuthenticated) {
    return (
      <Nav>
        <Nav.Link href="#deets">More deets</Nav.Link>
        <Nav.Link eventKey={2} href="#memes">
          Dank memes
        </Nav.Link>
        {/* <NavItem>
          <NavLink tag={Link} to="/">
            Hello {userName}
          </NavLink>
        </NavItem>
        <NavItem>
          <NavLink tag={Link} to="/authentication/logout">
            Logout
          </NavLink>
        </NavItem> */}
      </Nav>
    );
  }

  return (
    <Nav>
      <Navbar.Text>
        Signed in as: <a href="#login">Mark Otto</a>
      </Navbar.Text>
      <Nav.Link href="#deets">More deets</Nav.Link>
      <Nav.Link eventKey={2} href="#memes">
        Dank memes
      </Nav.Link>
      {/* <NavItem>
        <NavLink tag={Link} to="/">
          Register
        </NavLink>
      </NavItem>
      <NavItem>
        <NavLink tag={Link} to="/authentication/login">
          Login
        </NavLink>
      </NavItem> */}
    </Nav>
  );
};

export default LoginMenu;

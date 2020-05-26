import React from "react";
import { useSelector } from "react-redux";
import { Link } from "react-router-dom";
import LoginMenu from "./LoginMenu";

import { Nav, Navbar, NavDropdown } from "react-bootstrap";

import { selectIsAuthenticated, selectUser } from "../../store/slices/auth-slice";
import { selectCategories } from "../../store/slices/category-slice";

const NavMenu = () => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const userName = useSelector(selectUser)?.name;
  const categories = useSelector(selectCategories);

  return (
    <header>
      <Navbar collapseOnSelect expand="lg" bg="dark" variant="dark">
        <Navbar.Brand as={Link} to={`/`}>
          BShop
        </Navbar.Brand>
        <Navbar.Toggle aria-controls="responsive-navbar-nav" />
        <Navbar.Collapse id="responsive-navbar-nav">
          <Nav className="mr-auto">
            <NavDropdown title="Categories" id="collasible-nav-dropdown">
              {categories.map((category) => (
                <NavDropdown.Item key={category.id} as={Link} to={`/category/${category.id}`}>
                  {category.name}
                </NavDropdown.Item>
              ))}
            </NavDropdown>
          </Nav>
          <LoginMenu isAuthenticated={isAuthenticated} userName={userName} />
        </Navbar.Collapse>
      </Navbar>
    </header>
  );
};

export default NavMenu;

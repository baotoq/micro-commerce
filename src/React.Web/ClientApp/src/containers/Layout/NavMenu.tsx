import React, { useState } from "react";
import { useSelector } from "react-redux";
import { Link } from "react-router-dom";
import {
  Collapse,
  Navbar,
  NavbarBrand,
  NavbarToggler,
  NavItem,
  NavLink,
  Nav,
  DropdownItem,
  DropdownMenu,
  DropdownToggle,
  UncontrolledDropdown,
} from "reactstrap";
import LoginMenu from "./LoginMenu";

import { selectIsAuthenticated, selectUser } from "../../store/slices/auth-slice";
import { selectCategories } from "../../store/slices/categories-slice";

const NavMenu = () => {
  const isAuthenticated = useSelector(selectIsAuthenticated);
  const userName = useSelector(selectUser)?.name;
  const categories = useSelector(selectCategories);

  const [isOpen, setIsOpen] = useState(false);
  const toggle = () => setIsOpen(!isOpen);

  return (
    <header>
      <Navbar color="light" light expand="md">
        <NavbarBrand tag={Link} to="/">
          React.Web
        </NavbarBrand>
        <NavbarToggler onClick={toggle} className="mr-2" />
        <Collapse isOpen={isOpen} navbar>
          <Nav navbar>
            <NavItem>
              <NavLink tag={Link} to="/">
                Home
              </NavLink>
            </NavItem>
            <UncontrolledDropdown nav inNavbar>
              <DropdownToggle nav caret>
                Categories
              </DropdownToggle>
              <DropdownMenu right>
                {categories.map((category) => (
                  <DropdownItem key={category.id}>{category.name}</DropdownItem>
                ))}
              </DropdownMenu>
            </UncontrolledDropdown>
          </Nav>
          <Nav className="ml-auto" navbar>
            <LoginMenu isAuthenticated={isAuthenticated} userName={userName} />
          </Nav>
        </Collapse>
      </Navbar>
    </header>
  );
};

export default NavMenu;

import React from "react";
import renderer from "react-test-renderer";
import { render, screen } from "@testing-library/react";

import Login from "../login";

describe("Login", () => {
  it("should match snapshot", () => {
    const component = renderer.create(<Login />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  it("should show Login title", () => {
    render(<Login />);
    expect(screen.getByTestId("title")).toBeInTheDocument();
  });
});

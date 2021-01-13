import React from "react";
import renderer from "react-test-renderer";
import { render, screen } from "@testing-library/react";

import Register from "../register";

describe("Register", () => {
  it("should match snapshot", () => {
    const component = renderer.create(<Register />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  it("should show Register title", () => {
    render(<Register />);
    expect(screen.getByTestId("title")).toBeInTheDocument();
  });
});

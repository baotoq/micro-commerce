import React from "react";
import renderer from "react-test-renderer";
import { render, screen } from "@testing-library/react";

import AppBar from "../app-bar";

describe("AppBar", () => {
  it("should match snapshot", () => {
    const component = renderer.create(<AppBar />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  it("should show app name", () => {
    render(<AppBar />);
    expect(screen.getByText("Micro Commerce")).toBeInTheDocument();
  });
});

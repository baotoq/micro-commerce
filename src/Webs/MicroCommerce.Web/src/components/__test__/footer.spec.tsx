import React from "react";
import renderer from "react-test-renderer";
import { render, screen } from "@testing-library/react";

import Footer from "../footer";

describe("Footer", () => {
  it("should match snapshot", () => {
    const component = renderer.create(<Footer />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });

  it("should show app name", () => {
    render(<Footer />);
    expect(screen.getByText("Micro Commerce")).toBeInTheDocument();
  });
});

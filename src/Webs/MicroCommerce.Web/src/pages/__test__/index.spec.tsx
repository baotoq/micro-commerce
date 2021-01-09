import React from "react";
import renderer from "react-test-renderer";
import { render, screen } from "@testing-library/react";

import Index from "../index";

describe("Index", () => {
  it("should match snapshot", () => {
    const component = renderer.create(<Index />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});

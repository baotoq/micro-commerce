import React from "react";
import renderer from "react-test-renderer";
import Footer from "../footer";

describe("Footer", () => {
  it("should match snapshot", () => {
    const component = renderer.create(<Footer />);
    const tree = component.toJSON();
    expect(tree).toMatchSnapshot();
  });
});

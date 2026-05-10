import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import {
  Card,
  CardAction,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "./card";

describe("Card", () => {
  it("renders children", () => {
    render(<Card>Card body</Card>);
    expect(screen.getByText("Card body")).toBeInTheDocument();
  });

  it("has card data-slot attribute", () => {
    render(<Card data-testid="card">Content</Card>);
    expect(screen.getByTestId("card")).toHaveAttribute("data-slot", "card");
  });

  it("applies default size data attribute", () => {
    render(<Card data-testid="card">Content</Card>);
    expect(screen.getByTestId("card")).toHaveAttribute("data-size", "default");
  });

  it("applies sm size data attribute", () => {
    render(
      <Card size="sm" data-testid="card-sm">
        Content
      </Card>,
    );
    expect(screen.getByTestId("card-sm")).toHaveAttribute("data-size", "sm");
  });

  it("merges custom className", () => {
    render(
      <Card className="my-card" data-testid="card-cls">
        Content
      </Card>,
    );
    expect(screen.getByTestId("card-cls").className).toMatch(/my-card/);
  });
});

describe("CardHeader", () => {
  it("renders children", () => {
    render(<CardHeader>Header content</CardHeader>);
    expect(screen.getByText("Header content")).toBeInTheDocument();
  });

  it("has card-header data-slot attribute", () => {
    render(<CardHeader data-testid="ch">Header</CardHeader>);
    expect(screen.getByTestId("ch")).toHaveAttribute(
      "data-slot",
      "card-header",
    );
  });
});

describe("CardTitle", () => {
  it("renders children", () => {
    render(<CardTitle>My Title</CardTitle>);
    expect(screen.getByText("My Title")).toBeInTheDocument();
  });

  it("has card-title data-slot attribute", () => {
    render(<CardTitle data-testid="ct">Title</CardTitle>);
    expect(screen.getByTestId("ct")).toHaveAttribute("data-slot", "card-title");
  });
});

describe("CardDescription", () => {
  it("renders children", () => {
    render(<CardDescription>Some description</CardDescription>);
    expect(screen.getByText("Some description")).toBeInTheDocument();
  });

  it("has card-description data-slot attribute", () => {
    render(<CardDescription data-testid="cd">Desc</CardDescription>);
    expect(screen.getByTestId("cd")).toHaveAttribute(
      "data-slot",
      "card-description",
    );
  });
});

describe("CardContent", () => {
  it("renders children", () => {
    render(<CardContent>Main content</CardContent>);
    expect(screen.getByText("Main content")).toBeInTheDocument();
  });

  it("has card-content data-slot attribute", () => {
    render(<CardContent data-testid="cc">Content</CardContent>);
    expect(screen.getByTestId("cc")).toHaveAttribute(
      "data-slot",
      "card-content",
    );
  });
});

describe("CardFooter", () => {
  it("renders children", () => {
    render(<CardFooter>Footer content</CardFooter>);
    expect(screen.getByText("Footer content")).toBeInTheDocument();
  });

  it("has card-footer data-slot attribute", () => {
    render(<CardFooter data-testid="cf">Footer</CardFooter>);
    expect(screen.getByTestId("cf")).toHaveAttribute(
      "data-slot",
      "card-footer",
    );
  });
});

describe("CardAction", () => {
  it("renders children", () => {
    render(<CardAction>Action</CardAction>);
    expect(screen.getByText("Action")).toBeInTheDocument();
  });

  it("has card-action data-slot attribute", () => {
    render(<CardAction data-testid="ca">Action</CardAction>);
    expect(screen.getByTestId("ca")).toHaveAttribute(
      "data-slot",
      "card-action",
    );
  });
});

describe("Card composition", () => {
  it("renders full card structure with all subcomponents", () => {
    render(
      <Card data-testid="full-card">
        <CardHeader data-testid="header">
          <CardTitle>Title text</CardTitle>
          <CardDescription>Description text</CardDescription>
          <CardAction>Action text</CardAction>
        </CardHeader>
        <CardContent>Body text</CardContent>
        <CardFooter>Footer text</CardFooter>
      </Card>,
    );
    expect(screen.getByTestId("full-card")).toBeInTheDocument();
    expect(screen.getByText("Title text")).toBeInTheDocument();
    expect(screen.getByText("Description text")).toBeInTheDocument();
    expect(screen.getByText("Action text")).toBeInTheDocument();
    expect(screen.getByText("Body text")).toBeInTheDocument();
    expect(screen.getByText("Footer text")).toBeInTheDocument();
  });
});

import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";
import type { OrderTimelineEvent } from "@/lib/seller/orders/types";
import { OrderDetailTimeline } from "./order-detail-timeline";

const TIMELINE: OrderTimelineEvent[] = [
  {
    icon: "check",
    title: "Order placed",
    sub: "Payment confirmed",
    when: "May 5",
    on: true,
  },
  {
    icon: "box",
    title: "Packed",
    sub: "Ready for pickup",
    when: "May 6",
    on: true,
  },
  {
    icon: "truck",
    title: "Shipped",
    sub: "In transit",
    when: "May 7",
    on: false,
  },
  {
    icon: "chat",
    title: "Message sent",
    sub: "Tracking info shared",
    when: "May 8",
  },
];

describe("OrderDetailTimeline", () => {
  it("renders the Timeline heading", () => {
    render(<OrderDetailTimeline timeline={TIMELINE} />);
    expect(screen.getByText("Timeline")).toBeInTheDocument();
  });

  it("renders all event titles in order", () => {
    render(<OrderDetailTimeline timeline={TIMELINE} />);
    const titles = screen.getAllByText(
      /Order placed|Packed|Shipped|Message sent/,
    );
    expect(titles).toHaveLength(4);
  });

  it("renders event titles", () => {
    render(<OrderDetailTimeline timeline={TIMELINE} />);
    expect(screen.getByText("Order placed")).toBeInTheDocument();
    expect(screen.getByText("Packed")).toBeInTheDocument();
    expect(screen.getByText("Shipped")).toBeInTheDocument();
    expect(screen.getByText("Message sent")).toBeInTheDocument();
  });

  it("renders event sub-labels", () => {
    render(<OrderDetailTimeline timeline={TIMELINE} />);
    expect(screen.getByText("Payment confirmed")).toBeInTheDocument();
    expect(screen.getByText("Ready for pickup")).toBeInTheDocument();
    expect(screen.getByText("In transit")).toBeInTheDocument();
    expect(screen.getByText("Tracking info shared")).toBeInTheDocument();
  });

  it("renders event timestamps", () => {
    render(<OrderDetailTimeline timeline={TIMELINE} />);
    expect(screen.getByText("May 5")).toBeInTheDocument();
    expect(screen.getByText("May 6")).toBeInTheDocument();
    expect(screen.getByText("May 7")).toBeInTheDocument();
    expect(screen.getByText("May 8")).toBeInTheDocument();
  });

  it("renders a single event without errors", () => {
    const single: OrderTimelineEvent[] = [
      { icon: "info", title: "Note added", sub: "Internal only", when: "Now" },
    ];
    render(<OrderDetailTimeline timeline={single} />);
    expect(screen.getByText("Note added")).toBeInTheDocument();
    expect(screen.getByText("Internal only")).toBeInTheDocument();
    expect(screen.getByText("Now")).toBeInTheDocument();
  });

  it("renders a warn-tone event", () => {
    const warnTimeline: OrderTimelineEvent[] = [
      {
        icon: "info",
        title: "Flagged",
        sub: "Needs review",
        when: "Today",
        tone: "warn",
      },
    ];
    render(<OrderDetailTimeline timeline={warnTimeline} />);
    expect(screen.getByText("Flagged")).toBeInTheDocument();
    expect(screen.getByText("Needs review")).toBeInTheDocument();
  });

  it("preserves event ordering as provided", () => {
    render(<OrderDetailTimeline timeline={TIMELINE} />);
    const container = screen.getByText("Order placed").closest(".flex-1");
    expect(container).not.toBeNull();
    const allTitles = screen
      .getAllByText(/Order placed|Packed|Shipped|Message sent/)
      .map((el) => el.textContent);
    expect(allTitles[0]).toBe("Order placed");
    expect(allTitles[1]).toBe("Packed");
    expect(allTitles[2]).toBe("Shipped");
    expect(allTitles[3]).toBe("Message sent");
  });
});

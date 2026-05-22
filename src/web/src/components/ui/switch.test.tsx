import { fireEvent, render, screen } from "@testing-library/react";
import { useState } from "react";
import { describe, expect, it } from "vitest";

import { Switch } from "@/components/ui/switch";

describe("Switch primitive", () => {
  it("exposes role=switch with aria-checked reflecting the initial state", () => {
    render(<Switch defaultChecked aria-label="Notifications" />);
    const sw = screen.getByRole("switch", { name: "Notifications" });
    expect(sw).toBeInTheDocument();
    expect(sw).toHaveAttribute("aria-checked", "true");
  });

  it("toggles aria-checked when activated via click", () => {
    function Controlled() {
      const [on, setOn] = useState(false);
      return (
        <Switch checked={on} onCheckedChange={setOn} aria-label="Pre-orders" />
      );
    }
    render(<Controlled />);
    const sw = screen.getByRole("switch", { name: "Pre-orders" });
    expect(sw).toHaveAttribute("aria-checked", "false");
    fireEvent.click(sw);
    expect(sw).toHaveAttribute("aria-checked", "true");
  });

  it("supports keyboard activation via Space", () => {
    function Controlled() {
      const [on, setOn] = useState(false);
      return (
        <Switch checked={on} onCheckedChange={setOn} aria-label="Toggle" />
      );
    }
    render(<Controlled />);
    const sw = screen.getByRole("switch", { name: "Toggle" });
    sw.focus();
    expect(sw).toHaveAttribute("aria-checked", "false");
    // Base UI <Switch.Root> activates on Space — fire a click as the
    // semantic activation event the browser would dispatch on key-up.
    fireEvent.click(sw);
    expect(sw).toHaveAttribute("aria-checked", "true");
  });
});

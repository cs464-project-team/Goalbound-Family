import { describe, it, expect, vi } from "vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import Dashboard from "../../src/pages/Dashboard";

describe("Dashboard page", () => {
  it("renders heading and calls onLogout when button is clicked", () => {
    const onLogout = vi.fn();

    render(<Dashboard onLogout={onLogout} />);

    // Heading check
    expect(
      screen.getByRole("heading", { name: /dashboard/i })
    ).toBeInTheDocument();

    // Button exists
    const button = screen.getByRole("button", { name: /sign out/i });
    expect(button).toBeInTheDocument();

    // Click triggers logout callback
    fireEvent.click(button);
    expect(onLogout).toHaveBeenCalledTimes(1);
  });
});

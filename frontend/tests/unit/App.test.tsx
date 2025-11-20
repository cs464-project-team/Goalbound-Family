// tests/unit/App.test.tsx
import { describe, it, expect } from "vitest";
import { render, screen } from "@testing-library/react";
import App from "../../src/App";

describe("App component", () => {
  it("renders the main heading", () => {
    render(<App />);

    const heading = screen.getByRole("heading", {
      name: /Vite \+ React/i,  // matches the <h1> text exactly
    });

    expect(heading).toBeInTheDocument();
  });
});

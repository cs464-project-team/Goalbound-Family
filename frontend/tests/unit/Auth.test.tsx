import { describe, it, expect, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import Auth from "../../src/pages/Auth";

// Mock the auth context
vi.mock("../../src/context/AuthProvider", () => ({
  useAuthContext: () => ({
    session: null,
    signupError: null,
    loginError: null,
    signUp: vi.fn(),
    signIn: vi.fn(),
    signOut: vi.fn(),
  }),
}));

// Mock react-router-dom: useNavigate + Navigate
vi.mock("react-router-dom", async () => {
  const actual = await vi.importActual<typeof import("react-router-dom")>(
    "react-router-dom"
  );

  return {
    ...actual,
    useNavigate: () => vi.fn(),
    // Simple stub for <Navigate /> to avoid real routing
    Navigate: ({ to }: { to: string }) => <div>Redirect to {to}</div>,
  };
});

describe("Auth page", () => {
  it("renders Sign Up and Login sections when not logged in", () => {
    render(<Auth />);

    expect(
      screen.getByRole("heading", { name: /sign up/i })
    ).toBeInTheDocument();

    expect(
      screen.getByRole("heading", { name: /login/i })
    ).toBeInTheDocument();
  });
});

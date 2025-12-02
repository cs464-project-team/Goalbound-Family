import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import Dashboard from "../../src/pages/Dashboard";

// Mock fetch globally
globalThis.fetch = vi.fn(() =>
  Promise.resolve({
    ok: true,
    json: () => Promise.resolve([]),
  })
) as any;

// Mock Supabase client BEFORE any imports that use it
vi.mock("../../src/services/supabaseClient", () => ({
  __esModule: true,
  default: {
    auth: {
      getSession: vi.fn().mockResolvedValue({ data: { session: null }, error: null }),
      onAuthStateChange: vi.fn().mockReturnValue({
        data: { subscription: { unsubscribe: vi.fn() } }
      })
    }
  }
}));

// Mock AuthContext to provide a session
vi.mock("../../src/context/AuthProvider", () => ({
  useAuthContext: vi.fn(() => ({
    session: { user: { id: "test-user-id", email: "test@example.com" } },
    signupError: "",
    loginError: "",
    signUp: vi.fn(),
    signIn: vi.fn(),
    signOut: vi.fn(),
    setSignupError: vi.fn(),
    setLoginError: vi.fn()
  }))
}));

describe("Dashboard page", () => {
  beforeEach(() => {
    // Clear all mocks before each test
    vi.clearAllMocks();
  });

  it("renders dashboard heading", () => {
    render(
      <BrowserRouter>
        <Dashboard />
      </BrowserRouter>
    );

    // Heading check
    expect(
      screen.getByRole("heading", { name: /dashboard/i })
    ).toBeInTheDocument();
  });
});

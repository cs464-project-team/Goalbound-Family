import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import Expenses from "../../src/pages/Expenses";

// Mock session at module level
const mockSession = {
  user: {
    id: "test-user-id",
    email: "test@example.com",
  },
};

// Mock AuthProvider at module level (hoisted)
vi.mock("../../src/context/AuthProvider", () => ({
  useAuthContext: () => ({
    session: mockSession,
  }),
}));

// Mock API URL
vi.mock("../../src/config/api", () => ({
  getApiUrl: (path: string) => `http://localhost:5000${path}`,
}));

describe("Expenses Page", () => {
  beforeEach(() => {
    // Mock fetch
    global.fetch = vi.fn((url: string) => {
      if (url.includes("/api/householdmembers/user/")) {
        return Promise.resolve({
          ok: true,
          json: async () => [
            { id: "household-1", name: "Test Household", parentId: "parent-1", memberCount: 2 },
          ],
        } as Response);
      }
      if (url.includes("/api/receipts/household/")) {
        return Promise.resolve({
          ok: true,
          json: async () => [],
        } as Response);
      }
      if (url.includes("/api/expenses/user/")) {
        return Promise.resolve({
          ok: true,
          json: async () => [],
        } as Response);
      }
      return Promise.reject(new Error("Unknown URL"));
    });
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it("renders without crashing", () => {
    const { container } = render(
      <BrowserRouter>
        <Expenses />
      </BrowserRouter>
    );
    expect(container).toBeTruthy();
  });

  it("displays Expense History heading", async () => {
    render(
      <BrowserRouter>
        <Expenses />
      </BrowserRouter>
    );

    await waitFor(
      () => {
        const headings = screen.queryAllByText(/expense history/i);
        expect(headings.length).toBeGreaterThan(0);
      },
      { timeout: 2000 }
    );
  });

  it("makes API calls to fetch data", async () => {
    render(
      <BrowserRouter>
        <Expenses />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalled();
    });
  });
});

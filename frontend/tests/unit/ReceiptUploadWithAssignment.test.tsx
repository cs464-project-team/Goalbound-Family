import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, waitFor } from "@testing-library/react";
import { BrowserRouter } from "react-router-dom";
import ReceiptUploadWithAssignment from "../../src/components/ReceiptUploadWithAssignment";

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

describe("ReceiptUploadWithAssignment Component", () => {
  beforeEach(() => {
    // Mock fetch
    global.fetch = vi.fn((url: string) => {
      if (url.includes("/api/households/user/")) {
        return Promise.resolve({
          ok: true,
          json: async () => [{ id: "household-1", name: "Test Household" }],
        } as Response);
      }
      if (url.includes("/api/households/") && url.includes("/members")) {
        return Promise.resolve({
          ok: true,
          json: async () => [
            { id: "member-1", userId: "user-1", userName: "John Doe", role: "Parent" },
          ],
        } as Response);
      }
      if (url.includes("/api/budgets/categories/")) {
        return Promise.resolve({
          ok: true,
          json: async () => [{ id: "category-1", name: "Groceries" }],
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
        <ReceiptUploadWithAssignment />
      </BrowserRouter>
    );
    expect(container).toBeTruthy();
  });

  it("displays the Expense Management title", async () => {
    render(
      <BrowserRouter>
        <ReceiptUploadWithAssignment />
      </BrowserRouter>
    );

    await waitFor(
      () => {
        const titles = screen.queryAllByText(/expense management/i);
        expect(titles.length).toBeGreaterThan(0);
      },
      { timeout: 2000 }
    );
  });

  it("loads households on mount", async () => {
    render(
      <BrowserRouter>
        <ReceiptUploadWithAssignment />
      </BrowserRouter>
    );

    await waitFor(() => {
      expect(global.fetch).toHaveBeenCalledWith(
        expect.stringContaining("/api/households/user/")
      );
    });
  });

  it("displays mode selection buttons", async () => {
    render(
      <BrowserRouter>
        <ReceiptUploadWithAssignment />
      </BrowserRouter>
    );

    await waitFor(
      () => {
        const ocrButtons = screen.queryAllByText(/scan receipt/i);
        const manualButtons = screen.queryAllByText(/manual entry/i);
        expect(ocrButtons.length > 0 || manualButtons.length > 0).toBeTruthy();
      },
      { timeout: 2000 }
    );
  });
});

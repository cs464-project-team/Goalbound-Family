/**
 * API Configuration
 *
 * Centralized API base URL configuration for the application.
 *
 * Development: Uses empty string to leverage Vite's proxy (localhost:5073)
 * Production: Uses VITE_API_URL from environment variables
 */

/**
 * Base URL for all API requests
 * - In development: Empty string (uses Vite proxy)
 * - In production: Full backend URL from VITE_API_URL
 */
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || '';

/**
 * Constructs a full API URL from a relative path
 * @param path - The API path (e.g., '/api/users' or 'api/users')
 * @returns Full URL for the API endpoint
 */
export function getApiUrl(path: string): string {
  // Ensure path starts with /
  const normalizedPath = path.startsWith('/') ? path : `/${path}`;

  return `${API_BASE_URL}${normalizedPath}`;
}

/**
 * Helper function for making API requests with proper URL construction
 * @param path - The API path
 * @param options - Fetch options
 * @returns Fetch promise
 */
export async function apiFetch(path: string, options?: RequestInit): Promise<Response> {
  const url = getApiUrl(path);
  return fetch(url, options);
}

import { getApiUrl } from '../config/api'

export interface AuthUser {
  userId: string
  email: string
  firstName: string
  lastName: string
}

export interface LoginCredentials {
  email: string
  password: string
}

export interface SignupCredentials {
  email: string
  password: string
  firstName: string
  lastName: string
}

export interface AuthResponse extends AuthUser {
  accessToken?: string
}

/**
 * Authentication service that uses backend API endpoints
 * Refresh tokens are managed via HttpOnly cookies
 * Access tokens are stored in memory and sent in Authorization headers
 */
class AuthService {
  private accessToken: string | null = null

  /**
   * Get the current access token
   */
  getAccessToken(): string | null {
    return this.accessToken
  }

  /**
   * Set the access token
   */
  private setAccessToken(token: string | null): void {
    this.accessToken = token
    if (token) {
      console.log('[Auth Debug] Access token stored in memory:', token.substring(0, 20) + '...')
    }
  }

  /**
   * Clear the access token
   */
  clearAccessToken(): void {
    this.accessToken = null
  }
  /**
   * Login with email and password
   * Backend sets HttpOnly cookie with refresh token and returns access token
   */
  async login(credentials: LoginCredentials): Promise<AuthUser> {
    const response = await fetch(getApiUrl('/api/auth/login'), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Important: Send cookies with request
      body: JSON.stringify(credentials),
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Login failed' }))
      throw new Error(error.message || 'Login failed')
    }

    const data: AuthResponse = await response.json()
    
    // Store access token in memory if provided
    if (data.accessToken) {
      this.setAccessToken(data.accessToken)
    }

    return data
  }

  /**
   * Sign up new user
   * Backend sets HttpOnly cookie with refresh token and returns access token
   */
  async signup(credentials: SignupCredentials): Promise<AuthUser> {
    const response = await fetch(getApiUrl('/api/auth/signup'), {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', // Important: Send cookies with request
      body: JSON.stringify(credentials),
    })

    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Signup failed' }))
      throw new Error(error.message || 'Signup failed')
    }

    const data: AuthResponse = await response.json()
    
    // Store access token in memory if provided
    if (data.accessToken) {
      this.setAccessToken(data.accessToken)
    }

    return data
  }

  /**
   * Get current user information
   * Backend refreshes session using HttpOnly cookie and returns new access token
   * Returns null if no valid session
   */
  async getCurrentUser(): Promise<AuthUser | null> {
    try {
      const response = await fetch(getApiUrl('/api/auth/me'), {
        method: 'GET',
        credentials: 'include', // Important: Send cookies with request
      })

      // 204 No Content means no session (returns when no cookie exists)
      if (response.status === 204) {
        this.clearAccessToken()
        return null
      }

      if (!response.ok) {
        // Other error - just return null
        this.clearAccessToken()
        return null
      }

      const data: AuthResponse = await response.json()
      
      // Store access token in memory if provided
      if (data.accessToken) {
        this.setAccessToken(data.accessToken)
      }

      return data
    } catch (error) {
      // Network error or JSON parse error - silently return null
      this.clearAccessToken()
      return null
    }
  }

  /**
   * Refresh the access token using the refresh token in HttpOnly cookie
   * Returns the new access token or null if refresh fails
   */
  async refreshAccessToken(): Promise<string | null> {
    try {
      const response = await fetch(getApiUrl('/api/auth/refresh'), {
        method: 'POST',
        credentials: 'include',
      })

      if (!response.ok) {
        this.clearAccessToken()
        return null
      }

      const data: AuthResponse = await response.json()
      
      if (data.accessToken) {
        this.setAccessToken(data.accessToken)
        return data.accessToken
      }

      return null
    } catch (error) {
      this.clearAccessToken()
      return null
    }
  }

  /**
   * Logout current user
   * Backend clears HttpOnly cookie
   */
  async logout(): Promise<void> {
    try {
      await fetch(getApiUrl('/api/auth/logout'), {
        method: 'POST',
        credentials: 'include', // Important: Send cookies with request
      })
    } catch (error) {
      // Silently continue with logout even if request fails
    } finally {
      this.clearAccessToken()
    }
  }
}

export const authService = new AuthService()

/**
 * Helper function to make authenticated API requests
 * Automatically includes the access token in the Authorization header
 * Handles token refresh on 401 responses
 */
export async function authenticatedFetch(
  url: string,
  options: RequestInit = {}
): Promise<Response> {
  let token = authService.getAccessToken()

  // If no token, try to refresh first
  if (!token) {
    console.log('[Auth Debug] No access token, attempting refresh before request...')
    token = await authService.refreshAccessToken()
  }

  // Add Authorization header if we have a token
  const headers = new Headers(options.headers)
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
    console.log('[Auth Debug] Sending request with Authorization header:', {
      url,
      hasToken: !!token,
      tokenPreview: token.substring(0, 20) + '...',
    })
  } else {
    console.warn('[Auth Debug] No access token available for request:', url)
  }

  // Always include credentials for cookie-based refresh token
  const response = await fetch(url, {
    ...options,
    headers,
    credentials: 'include',
  })

  // If we get a 401, try to refresh the token and retry once
  if (response.status === 401) {
    console.log('[Auth Debug] Received 401, attempting token refresh...')
    const newToken = await authService.refreshAccessToken()

    if (newToken) {
      console.log('[Auth Debug] Token refreshed successfully, retrying request')
      // Retry the request with the new token
      headers.set('Authorization', `Bearer ${newToken}`)
      return fetch(url, {
        ...options,
        headers,
        credentials: 'include',
      })
    } else {
      console.warn('[Auth Debug] Token refresh failed')
    }
  }

  return response
}

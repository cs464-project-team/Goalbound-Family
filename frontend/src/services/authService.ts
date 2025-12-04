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

/**
 * Authentication service that uses backend API endpoints
 * All tokens are managed via HttpOnly cookies
 */
class AuthService {
  /**
   * Login with email and password
   * Backend sets HttpOnly cookie with refresh token
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

    return response.json()
  }

  /**
   * Sign up new user
   * Backend sets HttpOnly cookie with refresh token
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

    return response.json()
  }

  /**
   * Get current user information
   * Backend refreshes session using HttpOnly cookie
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
        return null
      }

      if (!response.ok) {
        // Other error - just return null
        return null
      }

      return response.json()
    } catch (error) {
      // Network error or JSON parse error - silently return null
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
    }
  }
}

export const authService = new AuthService()

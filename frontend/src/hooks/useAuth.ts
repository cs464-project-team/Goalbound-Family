import { useState, useEffect } from 'react'
import { authService } from '../services/authService'

// Session interface to maintain compatibility with existing components
export interface AuthSession {
  user: {
    id: string
    email: string
  }
}

export function useAuth() {
  const [session, setSession] = useState<AuthSession | null>(null)
  const [signupError, setSignupError] = useState('')
  const [loginError, setLoginError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  // Restore session on mount from HttpOnly cookie
  useEffect(() => {
    const restoreSession = async () => {
      try {
        const user = await authService.getCurrentUser()
        if (user) {
          setSession({
            user: {
              id: user.userId,
              email: user.email,
            },
          })
        }
      } finally {
        setIsLoading(false)
      }
    }

    restoreSession()
  }, [])

  const signUp = async (email: string, password: string, firstName: string, lastName: string) => {
    setSignupError('')
    try {
      const user = await authService.signup({
        email,
        password,
        firstName,
        lastName,
      })

      // Set session
      setSession({
        user: {
          id: user.userId,
          email: user.email,
        },
      })

      return true
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Signup failed'
      setSignupError(message)
      return false
    }
  }

  const signIn = async (email: string, password: string) => {
    setLoginError('')
    try {
      const user = await authService.login({ email, password })

      // Set session
      setSession({
        user: {
          id: user.userId,
          email: user.email,
        },
      })

      return true
    } catch (error) {
      const message = error instanceof Error ? error.message : 'Login failed'
      setLoginError(message)
      return false
    }
  }

  const signOut = async () => {
    await authService.logout()
    setSession(null)
  }

  return {
    session,
    signupError,
    loginError,
    isLoading,
    signUp,
    signIn,
    signOut,
    setSignupError,
    setLoginError,
  }
}
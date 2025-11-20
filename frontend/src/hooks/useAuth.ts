import { useState, useEffect } from 'react'
import type { Session } from '@supabase/supabase-js'
import supabase from '../services/supabaseClient'

export function useAuth() {
  const [session, setSession] = useState<Session | null>(null)
  const [signupError, setSignupError] = useState('')
  const [loginError, setLoginError] = useState('')

  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => setSession(session))
    const { data: listener } = supabase.auth.onAuthStateChange((_event, session) => setSession(session))
    return () => listener.subscription.unsubscribe()
  }, [])

  const signUp = async (email: string, password: string, firstName: string, lastName: string) => {
    setSignupError('')
    const { data, error } = await supabase.auth.signUp({ email, password })
    if (error) {
      setSignupError(error.message)
      return false
    }
    if (data?.user?.id) {
      const response = await fetch('http://localhost:5073/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: data.user.id, firstName, lastName, email })
      })
      if (!response.ok) {
        const errorData = await response.json()
        setSignupError(errorData.message || 'Failed to create user profile.')
        return false
      }
    }
    return true
  }

  const signIn = async (email: string, password: string) => {
    setLoginError('')
    const { error } = await supabase.auth.signInWithPassword({ email, password })
    if (error) {
      setLoginError(error.message)
      return false
    }
    return true
  }

  const signOut = async () => {
    await supabase.auth.signOut()
    setSession(null)
  }

  return {
    session,
    signupError,
    loginError,
    signUp,
    signIn,
    signOut,
    setSignupError,
    setLoginError
  }
}
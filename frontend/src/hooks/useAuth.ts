import React, { useState, useEffect } from 'react'
import type { Session } from '@supabase/supabase-js'
import supabase from '../services/supabaseClient'
import { getApiUrl } from '../config/api'

export function useAuth() {
  const [session, setSession] = useState<Session | null>(null)
  const [userId, setUserId] = useState<string | null>(null)
  const [signupError, setSignupError] = useState('')
  const [loginError, setLoginError] = useState('')

  useEffect(() => {
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session)
      setUserId(session?.user?.id ?? null)
    })
    const { data: listener } = supabase.auth.onAuthStateChange((_event, session) => {
      setSession(session)
      setUserId(session?.user?.id ?? null) // update userId whenever auth state changes
    })
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
      setUserId(data.user.id) // store userId immediately

      const response = await fetch(getApiUrl('/api/users'), {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id: data.user.id, firstName, lastName, email })
      })
      if (!response.ok) {
        await supabase.auth.admin.deleteUser(data.user.id) // use Supabase Admin SDK
        const errorData = await response.json()
        setSignupError(errorData.message || 'Failed to create user profile.')
        setUserId(null) // clear userId if profile creation fails
        return false
      }
    }
    return true
  }

  const signIn = async (email: string, password: string) => {
    setLoginError('')
    const { data, error } = await supabase.auth.signInWithPassword({ email, password })
    if (error) {
      setLoginError(error.message)
      return false
    }
    setUserId(data?.user?.id ?? null)
    return true
  }

  const signOut = async () => {
    await supabase.auth.signOut()
    setSession(null)
    setUserId(null) // clear userId on logout
  }

  return {
    session,
    userId,   
    signupError,
    loginError,
    signUp,
    signIn,
    signOut,
    setSignupError,
    setLoginError
  }
}
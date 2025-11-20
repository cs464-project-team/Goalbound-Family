import { useState, useEffect } from 'react'
import type { Session } from '@supabase/supabase-js'
import supabase from '../services/supabaseClient'
import Dashboard from './Dashboard'
import '../styles/Auth.css' 

function Auth() {
  // Signup state
  const [signupEmail, setSignupEmail] = useState('')
  const [signupPassword, setSignupPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [signupError, setSignupError] = useState('')

  // Login state
  const [loginEmail, setLoginEmail] = useState('')
  const [loginPassword, setLoginPassword] = useState('')
  const [loginError, setLoginError] = useState('')

  // Supabase session state
  const [session, setSession] = useState<Session | null>(null)

  useEffect(() => {
    // Check session on mount
    supabase.auth.getSession().then(({ data: { session } }) => {
      setSession(session)
    })

    // Listen for session changes
    const { data: listener } = supabase.auth.onAuthStateChange((_event, session) => {
      setSession(session)
    })

    return () => {
      listener.subscription.unsubscribe()
    }
  }, [])

  const handleSignup = async (e: React.FormEvent) => {
    e.preventDefault()
    setSignupError('')
    const { data, error } = await supabase.auth.signUp({
      email: signupEmail,
      password: signupPassword,
    })
    if (error) {
        setSignupError(error.message)
        return
    }

    //insert into our own user table
    if (!error && data?.user?.id) {
        const response = await fetch('http://localhost:5073/api/users', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            id: data.user.id,
            firstName,
            lastName,
            email: signupEmail
        })
        })

        if (!response.ok) {
        const errorData = await response.json()
        setSignupError(errorData.message || 'Failed to create user profile.')
        }
  }}

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    setLoginError('')
    const { error } = await supabase.auth.signInWithPassword({
      email: loginEmail,
      password: loginPassword
    })
    if (error) setLoginError(error.message)
  }

  // Show dashboard if logged in
  if (session) return <Dashboard onLogout={() => setSession(null)} />

  return (
    <div className="auth-columns">
      <div className="auth-column">
        <h2>Sign Up</h2>
        <form onSubmit={handleSignup}>
          <input
            type="text"
            placeholder="First Name"
            value={firstName}
            onChange={e => setFirstName(e.target.value)}
            required
          />
          <input
            type="text"
            placeholder="Last Name"
            value={lastName}
            onChange={e => setLastName(e.target.value)}
            required
          />
          <input
            type="email"
            placeholder="Email"
            value={signupEmail}
            onChange={e => setSignupEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Password"
            value={signupPassword}
            onChange={e => setSignupPassword(e.target.value)}
            required
          />
          <button type="submit">Sign Up</button>
          {signupError && <p style={{ color: 'red' }}>{signupError}</p>}
        </form>
      </div>
      <div className="auth-column">
        <h2>Login</h2>
        <form onSubmit={handleLogin}>
          <input
            type="email"
            placeholder="Email"
            value={loginEmail}
            onChange={e => setLoginEmail(e.target.value)}
            required
          />
          <input
            type="password"
            placeholder="Password"
            value={loginPassword}
            onChange={e => setLoginPassword(e.target.value)}
            required
          />
          <button type="submit">Login</button>
          {loginError && <p style={{ color: 'red' }}>{loginError}</p>}
        </form>
      </div>
    </div>
  )
}

export default Auth
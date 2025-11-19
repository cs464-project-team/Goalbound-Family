import { useState } from 'react'
import supabase from '../services/supebaseClient'

function Auth() {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [error, setError] = useState('')
  const [mode, setMode] = useState<'login' | 'signup'>('login')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError('')
    if (mode === 'signup') {
      const { error } = await supabase.auth.signUp({
        email,
        password,
        options: { data: { first_name: firstName, last_name: lastName } }
      })
      if (error) setError(error.message)
    } else {
      const { error } = await supabase.auth.signInWithPassword({ email, password })
      if (error) setError(error.message)
    }
  }

  return (
    <div>
      <button onClick={() => setMode('login')}>Login</button>
      <button onClick={() => setMode('signup')}>Sign Up</button>
      <form onSubmit={handleSubmit}>
        {mode === 'signup' && (
          <>
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
          </>
        )}
        <input
          type="email"
          placeholder="Email"
          value={email}
          onChange={e => setEmail(e.target.value)}
          required
        />
        <input
          type="password"
          placeholder="Password"
          value={password}
          onChange={e => setPassword(e.target.value)}
          required
        />
        <button type="submit">{mode === 'signup' ? 'Sign Up' : 'Login'}</button>
        {error && <p style={{ color: 'red' }}>{error}</p>}
      </form>
    </div>
  )
}

export default Auth
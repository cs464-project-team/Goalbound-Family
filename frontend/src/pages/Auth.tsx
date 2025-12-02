import { useState } from 'react'
import { useAuthContext } from '../context/AuthProvider'
import '../styles/Auth.css'
import { Navigate, useNavigate, useLocation } from 'react-router-dom'

function Auth() {
  // form state
  const [signupEmail, setSignupEmail] = useState('')
  const [signupPassword, setSignupPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [loginEmail, setLoginEmail] = useState('')
  const [loginPassword, setLoginPassword] = useState('')

  // Auth hook
  const {
    session,
    signupError,
    loginError,
    signUp,
    signIn,
  } = useAuthContext()

  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as any)?.from?.pathname || '/dashboard';

  if (session) {
    const pendingToken = localStorage.getItem('pendingInviteToken');
    if (pendingToken) {
      return <Navigate to={`/accept-invite?token=${pendingToken}`} replace />;
    }
    return <Navigate to="/dashboard" replace />;
  }



  const handleSignup = async (e: React.FormEvent) => {
    e.preventDefault()
    const success = await signUp(signupEmail, signupPassword, firstName, lastName)
    if (success) {
      const pendingToken = localStorage.getItem('pendingInviteToken');
      if (pendingToken) {
        navigate(`/accept-invite?token=${pendingToken}`);
      } else {
        navigate('/dashboard');
      }
    }
  }

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    const success = await signIn(loginEmail, loginPassword)
    if (success) {
      const pendingToken = localStorage.getItem('pendingInviteToken');
      if (pendingToken) {
        navigate(`/accept-invite?token=${pendingToken}`);
      } else {
        navigate('/dashboard');
      }
    }
  }

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
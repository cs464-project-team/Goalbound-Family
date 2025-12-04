import { useState } from 'react'
import { useAuthContext } from '../context/AuthProvider'
import { Navigate, useNavigate } from 'react-router-dom'
import { PiggyBank } from 'lucide-react'

function Auth() {
  // form state
  const [signupEmail, setSignupEmail] = useState('')
  const [signupPassword, setSignupPassword] = useState('')
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [loginEmail, setLoginEmail] = useState('')
  const [loginPassword, setLoginPassword] = useState('')
  const [isSignupView, setIsSignupView] = useState(true)

  // Auth hook
  const {
    session,
    signupError,
    loginError,
    signUp,
    signIn,
  } = useAuthContext()

  const navigate = useNavigate();

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
    <div style={{
      minHeight: '100vh',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      padding: '1rem'
    }}>
      <div style={{
        width: '100%',
        maxWidth: '1000px',
        background: 'white',
        borderRadius: '20px',
        boxShadow: '0 20px 60px rgba(0,0,0,0.3)',
        overflow: 'hidden',
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 400px), 1fr))',
        minHeight: '600px'
      }}>
        {/* Left Panel - Branding */}
        <div style={{
          background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
          padding: 'clamp(2rem, 4vw, 3rem)',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          color: 'white'
        }}>
          <div style={{ marginBottom: '2rem' }}>
            <div style={{
              display: 'flex',
              alignItems: 'center',
              marginBottom: '1rem'
            }}>
              <div style={{
                background: 'rgba(255,255,255,0.2)',
                borderRadius: '12px',
                padding: '0.75rem',
                marginRight: '1rem'
              }}>
                <PiggyBank size={32} />
              </div>
              <h1 style={{
                fontSize: '2rem',
                fontWeight: 'bold',
                margin: 0
              }}>Goalbound Family</h1>
            </div>
            <p style={{
              fontSize: '1.1rem',
              opacity: 0.95,
              lineHeight: '1.6'
            }}>
              Track your family's finances together. Set budgets, manage expenses, and achieve your financial goals as a team.
            </p>
          </div>
          <div style={{
            background: 'rgba(255,255,255,0.1)',
            borderRadius: '12px',
            padding: '1.5rem'
          }}>
            <h3 style={{ fontSize: '1.2rem', marginBottom: '1rem' }}>Features</h3>
            <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
              {['Shared Household Budgets', 'Expense Tracking', 'Receipt Scanning', 'Spending Insights'].map((feature, i) => (
                <li key={i} style={{
                  padding: '0.5rem 0',
                  display: 'flex',
                  alignItems: 'center'
                }}>
                  <span style={{ marginRight: '0.5rem' }}>✓</span>
                  {feature}
                </li>
              ))}
            </ul>
          </div>
        </div>

        {/* Right Panel - Auth Forms */}
        <div style={{
          padding: 'clamp(2rem, 4vw, 3rem)',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center'
        }}>
          {/* Tab Switcher */}
          <div style={{
            display: 'flex',
            gap: '1rem',
            marginBottom: '2rem',
            borderBottom: '2px solid #f0f0f0'
          }}>
            <button
              type="button"
              onClick={() => setIsSignupView(true)}
              style={{
                flex: 1,
                padding: '1rem',
                background: 'none',
                border: 'none',
                borderBottom: isSignupView ? '3px solid #667eea' : '3px solid transparent',
                color: isSignupView ? '#667eea' : '#666',
                fontWeight: '600',
                fontSize: '1.1rem',
                cursor: 'pointer',
                transition: 'all 0.2s',
                marginBottom: '-2px'
              }}
            >
              Sign Up
            </button>
            <button
              type="button"
              onClick={() => setIsSignupView(false)}
              style={{
                flex: 1,
                padding: '1rem',
                background: 'none',
                border: 'none',
                borderBottom: !isSignupView ? '3px solid #667eea' : '3px solid transparent',
                color: !isSignupView ? '#667eea' : '#666',
                fontWeight: '600',
                fontSize: '1.1rem',
                cursor: 'pointer',
                transition: 'all 0.2s',
                marginBottom: '-2px'
              }}
            >
              Login
            </button>
          </div>

          {/* Sign Up Form */}
          {isSignupView ? (
            <form onSubmit={handleSignup} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#333' }}>
                    First Name
                  </label>
                  <input
                    type="text"
                    placeholder="John"
                    value={firstName}
                    onChange={e => setFirstName(e.target.value)}
                    required
                    style={{
                      width: '100%',
                      padding: '0.75rem',
                      borderRadius: '8px',
                      border: '2px solid #e0e0e0',
                      fontSize: '1rem',
                      transition: 'border 0.2s',
                      outline: 'none'
                    }}
                    onFocus={(e) => e.currentTarget.style.border = '2px solid #667eea'}
                    onBlur={(e) => e.currentTarget.style.border = '2px solid #e0e0e0'}
                  />
                </div>
                <div>
                  <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#333' }}>
                    Last Name
                  </label>
                  <input
                    type="text"
                    placeholder="Doe"
                    value={lastName}
                    onChange={e => setLastName(e.target.value)}
                    required
                    style={{
                      width: '100%',
                      padding: '0.75rem',
                      borderRadius: '8px',
                      border: '2px solid #e0e0e0',
                      fontSize: '1rem',
                      transition: 'border 0.2s',
                      outline: 'none'
                    }}
                    onFocus={(e) => e.currentTarget.style.border = '2px solid #667eea'}
                    onBlur={(e) => e.currentTarget.style.border = '2px solid #e0e0e0'}
                  />
                </div>
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#333' }}>
                  Email
                </label>
                <input
                  type="email"
                  placeholder="john@example.com"
                  value={signupEmail}
                  onChange={e => setSignupEmail(e.target.value)}
                  required
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    borderRadius: '8px',
                    border: '2px solid #e0e0e0',
                    fontSize: '1rem',
                    transition: 'border 0.2s',
                    outline: 'none'
                  }}
                  onFocus={(e) => e.currentTarget.style.border = '2px solid #667eea'}
                  onBlur={(e) => e.currentTarget.style.border = '2px solid #e0e0e0'}
                />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#333' }}>
                  Password
                </label>
                <input
                  type="password"
                  placeholder="••••••••"
                  value={signupPassword}
                  onChange={e => setSignupPassword(e.target.value)}
                  required
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    borderRadius: '8px',
                    border: '2px solid #e0e0e0',
                    fontSize: '1rem',
                    transition: 'border 0.2s',
                    outline: 'none'
                  }}
                  onFocus={(e) => e.currentTarget.style.border = '2px solid #667eea'}
                  onBlur={(e) => e.currentTarget.style.border = '2px solid #e0e0e0'}
                />
              </div>
              {signupError && (
                <div style={{
                  padding: '0.75rem',
                  background: '#fee',
                  color: '#e53e3e',
                  borderRadius: '8px',
                  fontSize: '0.9rem'
                }}>
                  {signupError}
                </div>
              )}
              <button
                type="submit"
                style={{
                  width: '100%',
                  padding: '0.875rem',
                  borderRadius: '8px',
                  border: 'none',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  fontWeight: '600',
                  fontSize: '1rem',
                  cursor: 'pointer',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  marginTop: '0.5rem'
                }}
                onMouseOver={(e) => {
                  e.currentTarget.style.transform = 'translateY(-2px)';
                  e.currentTarget.style.boxShadow = '0 4px 12px rgba(102, 126, 234, 0.4)';
                }}
                onMouseOut={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}
              >
                Create Account
              </button>
            </form>
          ) : (
            <form onSubmit={handleLogin} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#333' }}>
                  Email
                </label>
                <input
                  type="email"
                  placeholder="john@example.com"
                  value={loginEmail}
                  onChange={e => setLoginEmail(e.target.value)}
                  required
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    borderRadius: '8px',
                    border: '2px solid #e0e0e0',
                    fontSize: '1rem',
                    transition: 'border 0.2s',
                    outline: 'none'
                  }}
                  onFocus={(e) => e.currentTarget.style.border = '2px solid #667eea'}
                  onBlur={(e) => e.currentTarget.style.border = '2px solid #e0e0e0'}
                />
              </div>
              <div>
                <label style={{ display: 'block', marginBottom: '0.5rem', fontWeight: '500', color: '#333' }}>
                  Password
                </label>
                <input
                  type="password"
                  placeholder="••••••••"
                  value={loginPassword}
                  onChange={e => setLoginPassword(e.target.value)}
                  required
                  style={{
                    width: '100%',
                    padding: '0.75rem',
                    borderRadius: '8px',
                    border: '2px solid #e0e0e0',
                    fontSize: '1rem',
                    transition: 'border 0.2s',
                    outline: 'none'
                  }}
                  onFocus={(e) => e.currentTarget.style.border = '2px solid #667eea'}
                  onBlur={(e) => e.currentTarget.style.border = '2px solid #e0e0e0'}
                />
              </div>
              {loginError && (
                <div style={{
                  padding: '0.75rem',
                  background: '#fee',
                  color: '#e53e3e',
                  borderRadius: '8px',
                  fontSize: '0.9rem'
                }}>
                  {loginError}
                </div>
              )}
              <button
                type="submit"
                style={{
                  width: '100%',
                  padding: '0.875rem',
                  borderRadius: '8px',
                  border: 'none',
                  background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
                  color: 'white',
                  fontWeight: '600',
                  fontSize: '1rem',
                  cursor: 'pointer',
                  transition: 'transform 0.2s, box-shadow 0.2s',
                  marginTop: '0.5rem'
                }}
                onMouseOver={(e) => {
                  e.currentTarget.style.transform = 'translateY(-2px)';
                  e.currentTarget.style.boxShadow = '0 4px 12px rgba(102, 126, 234, 0.4)';
                }}
                onMouseOut={(e) => {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }}
              >
                Sign In
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  )
}

export default Auth

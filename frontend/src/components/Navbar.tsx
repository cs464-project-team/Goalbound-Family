// src/components/Navbar.tsx
import React from 'react'
import { useAuthContext } from '../context/AuthProvider'
import { useLocation } from 'react-router-dom'
import '../styles/Navbar.css'


export default function Navbar() {
  const { session, signOut } = useAuthContext()
  const location = useLocation()

  // Hide navbar on the login/signup page
  if (location.pathname === '/auth') return null
  // Hide navbar if user is not logged in
  if (!session) return null

  return (
    <nav className="navbar" aria-label="Main navigation">
      <div className="navbar__container">
        <span className="navbar__welcome">Welcome, {session.user.email}</span>
        <button
          className="navbar__button"
          onClick={signOut}
          aria-label="Sign out"
          type="button"
        >
          Logout
        </button>
      </div>
    </nav>
  )
}


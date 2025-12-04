import React from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthContext } from '../context/AuthProvider'

export default function RequireAuth({ children }: { children: React.ReactNode }) {
  const { session, isLoading } = useAuthContext()
  const location = useLocation()

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <div style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '100vh'
      }}>
        <div>Loading...</div>
      </div>
    )
  }

  // Redirect to auth if not authenticated
  if (!session) return <Navigate to="/auth" state={{ from: location }} replace />

  return children
}
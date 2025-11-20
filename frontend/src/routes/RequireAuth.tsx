import React from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { useAuthContext } from '../context/AuthProvider'

export default function RequireAuth({ children }: { children: React.ReactNode }) {
  const { session } = useAuthContext()
  const location = useLocation()
  if (!session) return <Navigate to="/auth" state={{ from: location }} replace />
  return children
}
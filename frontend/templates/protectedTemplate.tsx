// src/pages/ModuleTemplate.tsx
import React from 'react'
import { useAuthContext } from '../context/AuthProvider'

export default function ModuleTemplate() {
  // Access session if needed
  const { session } = useAuthContext()

  return (
    <div style={{ padding: '2rem' }}>
      <h1>Module Template</h1>

      {session ? (
        <div>
          <p>Logged in as: {session.user.email}</p>
          <p>This is a protected page template. Customize your content here.</p>
        </div>
      ) : (
        <p>You must be logged in to view this content.</p>
      )}
    </div>
  )
}
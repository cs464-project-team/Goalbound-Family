# Auth System Developer Guide

# Frontend Auth & Routing Guide

This guide explains how to add new pages to the app, handle authentication, and use the global Navbar. Everything you need is on this page, including copy-paste templates for both protected and public pages, and example route setup.

## Overview

- **Authentication** is managed globally using `AuthProvider` and the `useAuthContext` hook.
- **Protected pages** require users to be logged in and are wrapped with `RequireAuth` in the route.
- **Public pages** are accessible to everyone and do not need to be wrapped.
- The **Navbar** is displayed on all protected pages and shows the user's email and a Logout button.

You do **not** need to interact with Supabase or backend code directly to use authentication in your pages.

---

## How to Add a Protected Page (Requires Login)

1. **Create your page file** (e.g. `src/pages/ModuleTemplate.tsx`) using this template:

```tsx
// src/pages/ModuleTemplate.tsx
import React from 'react'
import { useAuthContext } from '../context/AuthProvider'

function ModuleTemplate() {
  const { session } = useAuthContext()
  // session.user.email is available
  return (
    <div style={{ padding: '2rem' }}>
      <h1>Protected Module Page</h1>
      <p>Welcome, {session?.user.email}!</p>
      {/* Your protected content here */}
    </div>
  )
}

export default ModuleTemplate
```

2. **Add a protected route in `App.tsx`:**

```tsx
import ModuleTemplate from './pages/ModuleTemplate'
import RequireAuth from './routes/RequireAuth'

// Inside your <Routes>:
<Route
  path="/module-template"
  element={
    <RequireAuth>
      <ModuleTemplate />
    </RequireAuth>
  }
/>
```

---

## How to Add a Public Page (No Login Required)

1. **Create your public page file** (e.g. `src/pages/PublicModuleTemplate.tsx`) using this template:

```tsx
// src/pages/PublicModuleTemplate.tsx
import React from 'react'

function PublicModuleTemplate() {
  return (
    <div style={{ padding: '2rem' }}>
      <h1>Public Module Page</h1>
      <p>This page is visible to everyone, logged in or not.</p>
      {/* Your public content here */}
    </div>
  )
}

export default PublicModuleTemplate
```

2. **Add a public route in `App.tsx`:**

```tsx
import PublicModuleTemplate from './pages/PublicModuleTemplate'

// Inside your <Routes>:
<Route path="/public-module" element={<PublicModuleTemplate />} />
```

---

## Using the Navbar

- The `Navbar` component is **automatically shown** on all protected pages (i.e. when the user is logged in and not on `/auth`).
- It displays the user's email and a Logout button.
- You do **not** need to add the Navbar manually to your pages.
- To log the user out, simply click the Logout button on the Navbar. The page will redirect to `/auth`.

---

## Accessing Auth Info in Your Page

If you need user info or logout functionality inside your page, use the `useAuthContext` hook:

```tsx
import { useAuthContext } from '../context/AuthProvider'

const { session, signOut } = useAuthContext()
// session.user.email is available
// signOut() will log the user out
```

---

## Quick Reference: Example Route Setup in App.tsx

```tsx
import { Routes, Route } from 'react-router-dom'
import RequireAuth from './routes/RequireAuth'
import ModuleTemplate from './pages/ModuleTemplate'
import PublicModuleTemplate from './pages/PublicModuleTemplate'

function App() {
  return (
    <Routes>
      {/* Protected route */}
      <Route
        path="/module-template"
        element={
          <RequireAuth>
            <ModuleTemplate />
          </RequireAuth>
        }
      />
      {/* Public route */}
      <Route path="/public-module" element={<PublicModuleTemplate />} />
      {/* ...other routes */}
    </Routes>
  )
}
```

---

**Summary:**  
1. Use the provided templates to create new pages.  
2. For protected pages, wrap the route in `<RequireAuth>`.  
3. For public pages, just add the route normally.  
4. The Navbar is shown automatically on protected pages.  
5. Use `useAuthContext()` for session info or logout in your components.
import { AuthProvider } from './context/AuthProvider'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import './App.css'
import Auth from './pages/Auth'
import Dashboard from './pages/Dashboard'
import RequireAuth from './routes/RequireAuth'
import { useAuthContext } from './context/AuthProvider'
import Navbar from './components/Navbar'

function AppRoutes() {
  const { signOut } = useAuthContext()

  return (
    <Routes>
      // default route
      <Route path="/" element={<Navigate to="/auth" replace />} />
      <Route path="/auth" element={<Auth />} />

      <Route
        path="/dashboard"
        element={
          <RequireAuth>
            <Dashboard onLogout={signOut} />
          </RequireAuth>
        }
      />

    </Routes>
  )
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Navbar />
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  )
}

export default App

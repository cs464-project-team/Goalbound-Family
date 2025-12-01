import { AuthProvider } from "./context/AuthProvider";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";
import Auth from "./pages/Auth";
import Dashboard from "./pages/Dashboard";
import Leaderboard from "./pages/Leaderboard";
import { Layout } from "./components/layout";
import RequireAuth from "./routes/RequireAuth";
import { useAuthContext } from "./context/AuthProvider";
import Navbar from "./components/Navbar";
import AcceptInvite from "./pages/AcceptInvite";

function AppRoutes() {
  const { signOut } = useAuthContext();

  return (
    <Routes>
      // default route
      <Route path="/" element={<Navigate to="/auth" replace />} />
      <Route path="/auth" element={<Auth />} />

      <Route path="/accept-invite" element={<AcceptInvite />} />

      <Route
        path="/dashboard"
        element={
          <RequireAuth>
            <Dashboard onLogout={signOut} />
          </RequireAuth>
        }
      />

      {/* <Route
        path="/leaderboard"
        element={
          <RequireAuth>
            <Leaderboard />
          </RequireAuth>
        }
      /> */}
      <Route
        path="/leaderboard"
        element={
          <Layout>
            <Leaderboard />
          </Layout>
        }
      />
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Navbar />
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;

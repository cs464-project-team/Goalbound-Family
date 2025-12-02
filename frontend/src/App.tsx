import { AuthProvider } from "./context/AuthProvider";
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";
import "./App.css";
import Auth from "./pages/Auth";
import Dashboard from "./pages/Dashboard";
import Leaderboard from "./pages/Leaderboard";
import ReceiptScanner from "./pages/ReceiptScanner";
import { Layout } from "./components/layout";
import RequireAuth from "./routes/RequireAuth";
import AcceptInvite from "./pages/AcceptInvite";

function AppRoutes() {
  // const { signOut } = useAuthContext();

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
            <Dashboard />
          </RequireAuth>
        }
      />

      <Route
        path="/leaderboard"
        element={
          <RequireAuth>
            <Layout>
              <Leaderboard />
            </Layout>
          </RequireAuth>
        }
      />

      <Route
        path="/scanner"
        element={
          <RequireAuth>
            <Layout>
              <ReceiptScanner />
            </Layout>
          </RequireAuth>
        }
      />
    </Routes>
  );
}

function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <AppRoutes />
      </BrowserRouter>
    </AuthProvider>
  );
}

export default App;

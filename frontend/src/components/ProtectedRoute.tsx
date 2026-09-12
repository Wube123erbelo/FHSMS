import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

/**
 * Guards a route by authentication and, optionally, role. Role-gating lives
 * here (not just in the sidebar) so a HotelCustomer or agent can't reach an
 * admin-only page just by typing its URL - the sidebar hiding it is a
 * convenience, not the actual boundary.
 */
export default function ProtectedRoute({ roles }: { roles?: string[] }) {
  const { isAuthenticated, role } = useAuth();
  if (!isAuthenticated) return <Navigate to="/welcome" replace />;
  if (roles && (!role || !roles.includes(role))) return <Navigate to="/" replace />;
  return <Outlet />;
}

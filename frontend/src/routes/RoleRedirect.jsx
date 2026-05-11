import { Navigate } from "react-router-dom";
import { ROLES } from "../context/authConstants";
import useAuth from "../context/useAuth";

const roleLandingPath = {
  [ROLES.ADMIN]: "/dashboard",
  [ROLES.DOCTOR]: "/doctor/appointments",
  [ROLES.RECEPTIONIST]: "/receptionist/book",
  [ROLES.PATIENT]: "/patient/appointments",
  [ROLES.GUARDIAN]: "/patient/appointments",
};

const RoleRedirect = () => {
  const { user } = useAuth();
  const landingPath = roleLandingPath[user?.role] || "/dashboard";
  return <Navigate to={landingPath} replace />;
};

export default RoleRedirect;

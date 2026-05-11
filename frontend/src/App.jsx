import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { ROLES } from "./context/authConstants";
import useAuth from "./context/useAuth";
import Login from "./pages/Login";
import Register from "./pages/Register";
import Dashboard from "./pages/Dashboard";
import Therapies from "./pages/Therapies";
import Doctors from "./pages/Doctors";
import Slots from "./pages/Slots";
import Receptionists from "./pages/Receptionists";
import BookAppointment from "./pages/BookAppointment";
import ReceptionistDoctors from "./pages/ReceptionistDoctors";
import DoctorAppointments from "./pages/DoctorAppointments";
import DoctorFindings from "./pages/DoctorFindings";
import PatientDoctors from "./pages/PatientDoctors";
import PatientSlots from "./pages/PatientSlots";
import PatientAppointments from "./pages/PatientAppointments";
import AdminReports from "./pages/AdminReports";
import GuardianPatients from "./pages/GuardianPatients";
import Unauthorized from "./pages/Unauthorized";
import NotFound from "./pages/NotFound";
import AppLayout from "./layouts/AppLayout";
import ProtectedRoute from "./routes/ProtectedRoute";
import RoleRedirect from "./routes/RoleRedirect";

function App() {
  const { isAuthenticated } = useAuth();

  return (
    <BrowserRouter>
      <Routes>
        <Route
          path="/"
          element={<Navigate to={isAuthenticated ? "/dashboard" : "/login"} replace />}
        />
        <Route
          path="/login"
          element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Login />}
        />
        <Route
          path="/register"
          element={isAuthenticated ? <Navigate to="/dashboard" replace /> : <Register />}
        />
        <Route path="/unauthorized" element={<Unauthorized />} />

        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/home" element={<RoleRedirect />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute allowedRoles={[ROLES.ADMIN]} />}>
          <Route element={<AppLayout />}>
            <Route
              path="/admin/therapies"
              element={<Therapies />}
            />
            <Route
              path="/admin/doctors"
              element={<Doctors />}
            />
            <Route
              path="/admin/slots"
              element={<Slots />}
            />
            <Route
              path="/admin/receptionists"
              element={<Receptionists />}
            />
            <Route path="/admin/reports" element={<AdminReports />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute allowedRoles={[ROLES.GUARDIAN]} />}>
          <Route element={<AppLayout />}>
            <Route path="/guardian/patients" element={<GuardianPatients />} />
          </Route>
        </Route>

        <Route element={<ProtectedRoute allowedRoles={[ROLES.RECEPTIONIST]} />}>
          <Route element={<AppLayout />}>
            <Route
              path="/receptionist/book"
              element={<BookAppointment />}
            />
            <Route
              path="/receptionist/doctors"
              element={<ReceptionistDoctors />}
            />
            <Route
              path="/receptionist/slots"
              element={<Slots />}
            />
          </Route>
        </Route>

        <Route element={<ProtectedRoute allowedRoles={[ROLES.DOCTOR]} />}>
          <Route element={<AppLayout />}>
            <Route
              path="/doctor/appointments"
              element={<DoctorAppointments />}
            />
            <Route
              path="/doctor/findings"
              element={<DoctorFindings />}
            />
          </Route>
        </Route>

        <Route element={<ProtectedRoute allowedRoles={[ROLES.PATIENT, ROLES.GUARDIAN]} />}>
          <Route element={<AppLayout />}>
            <Route
              path="/patient/doctors"
              element={<PatientDoctors />}
            />
            <Route
              path="/patient/slots"
              element={<PatientSlots />}
            />
            <Route
              path="/patient/appointments"
              element={<PatientAppointments />}
            />
          </Route>
        </Route>

        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
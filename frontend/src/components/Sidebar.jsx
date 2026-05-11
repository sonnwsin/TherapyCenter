import { NavLink } from "react-router-dom";
import { ROLES } from "../context/authConstants";
import useAuth from "../context/useAuth";

const menuByRole = {
  [ROLES.ADMIN]: [
    { label: "Overview", path: "/dashboard", icon: "bi-speedometer2" },
    { label: "Therapies", path: "/admin/therapies", icon: "bi-heart-pulse" },
    { label: "Doctors", path: "/admin/doctors", icon: "bi-person-badge" },
    { label: "Receptionists", path: "/admin/receptionists", icon: "bi-people" },
    { label: "Reports", path: "/admin/reports", icon: "bi-graph-up" },
    { label: "Slots", path: "/admin/slots", icon: "bi-calendar3" },
  ],
  [ROLES.DOCTOR]: [
    { label: "Overview", path: "/dashboard", icon: "bi-speedometer2" },
    { label: "Appointments", path: "/doctor/appointments", icon: "bi-calendar-check" },
    { label: "Findings", path: "/doctor/findings", icon: "bi-clipboard2-pulse" },
  ],
  [ROLES.RECEPTIONIST]: [
    { label: "Overview", path: "/dashboard", icon: "bi-speedometer2" },
    { label: "Book walk-in", path: "/receptionist/book", icon: "bi-calendar-plus" },
    { label: "Doctors", path: "/receptionist/doctors", icon: "bi-person-lines-fill" },
    { label: "Slots", path: "/receptionist/slots", icon: "bi-grid-3x2-gap" },
  ],
  [ROLES.PATIENT]: [
    { label: "Overview", path: "/dashboard", icon: "bi-speedometer2" },
    { label: "Doctors", path: "/patient/doctors", icon: "bi-hospital" },
    { label: "Book", path: "/patient/slots", icon: "bi-calendar2-week" },
    { label: "Appointments", path: "/patient/appointments", icon: "bi-list-task" },
  ],
  [ROLES.GUARDIAN]: [
    { label: "Overview", path: "/dashboard", icon: "bi-speedometer2" },
    { label: "Doctors", path: "/patient/doctors", icon: "bi-hospital" },
    { label: "Book", path: "/patient/slots", icon: "bi-calendar2-week" },
    { label: "My patients", path: "/guardian/patients", icon: "bi-person-hearts" },
    { label: "Appointments", path: "/patient/appointments", icon: "bi-list-task" },
  ],
};

const Sidebar = ({ mobileOpen, onNavigate }) => {
  const { user } = useAuth();
  const menu = menuByRole[user?.role] ?? [
    { label: "Overview", path: "/dashboard", icon: "bi-speedometer2" },
  ];

  return (
    <aside
      className={`app-sidebar p-3 p-lg-4 d-flex flex-column ${mobileOpen ? "tc-sidebar-open" : ""}`}
    >
      <div className="app-sidebar-brand d-flex align-items-center gap-3">
        <div className="app-sidebar-brand-mark">
          <i className="bi bi-heart-pulse text-white" aria-hidden="true" />
        </div>
        <div>
          <div className="app-sidebar-brand-title">Therapy Center</div>
          <div className="app-sidebar-brand-sub">Care &amp; scheduling</div>
        </div>
      </div>

      <nav className="d-flex flex-column gap-1 flex-grow-1">
        {menu.map((item) => (
          <NavLink
            key={item.path}
            to={item.path}
            onClick={onNavigate}
            className={({ isActive }) =>
              `sidebar-link ${isActive ? "sidebar-link-active" : ""}`
            }
            end={item.path === "/dashboard"}
          >
            <i className={`${item.icon} me-2`} aria-hidden="true" />
            {item.label}
          </NavLink>
        ))}
      </nav>

      <div className="mt-auto pt-3 small text-white-50 d-none d-lg-block">
        <i className="bi bi-shield-check me-1" aria-hidden="true" />
        Secure session
      </div>
    </aside>
  );
};

export default Sidebar;

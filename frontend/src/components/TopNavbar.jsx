import { useNavigate } from "react-router-dom";
import useAuth from "../context/useAuth";

const TopNavbar = ({ onToggleSidebar }) => {
  const navigate = useNavigate();
  const { user, logout } = useAuth();

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  return (
    <header className="top-navbar">
      <div className="d-flex align-items-center justify-content-between px-3 px-lg-4 py-3">
        <div className="d-flex align-items-center gap-2 min-w-0">
          <button
            type="button"
            className="btn btn-light border d-lg-none rounded-3 p-2"
            onClick={onToggleSidebar}
            aria-label="Open navigation menu"
          >
            <i className="bi bi-list fs-4" aria-hidden="true" />
          </button>
          <div className="d-flex align-items-center gap-3 min-w-0">
            <div
              className="d-none d-md-flex align-items-center justify-content-center rounded-3 p-2"
              style={{ width: 40, height: 40, background: "rgba(15, 118, 110, 0.12)", color: "#0f766e" }}
            >
              <i className="bi bi-house-door fs-5" aria-hidden="true" />
            </div>
            <div className="min-w-0">
              <div className="small text-secondary text-uppercase fw-semibold" style={{ letterSpacing: "0.06em", fontSize: "0.65rem" }}>
                Workspace
              </div>
              <div className="fw-semibold text-truncate" style={{ maxWidth: "min(42vw, 280px)" }}>
                {user?.email}
              </div>
              <span className="tc-role-badge d-md-none mt-1">{user?.role}</span>
            </div>
          </div>
        </div>

        <div className="d-flex align-items-center gap-2 flex-shrink-0">
          <div className="tc-user-pill d-none d-md-flex align-items-center gap-2">
            <span className="tc-role-badge">{user?.role}</span>
          </div>
          <button
            type="button"
            className="btn btn-outline-secondary btn-sm rounded-pill px-3"
            onClick={handleLogout}
          >
            <i className="bi bi-box-arrow-right me-1" aria-hidden="true" />
            Log out
          </button>
        </div>
      </div>
      <div className="tc-navbar-accent" aria-hidden="true" />
    </header>
  );
};

export default TopNavbar;

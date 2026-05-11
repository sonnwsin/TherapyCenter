import { Link } from "react-router-dom";

const NotFound = () => {
  return (
    <div className="auth-page">
      <div className="text-center px-3" style={{ maxWidth: 440 }}>
        <div className="display-4 fw-bold text-secondary mb-2">404</div>
        <h2 className="fw-bold mb-2" style={{ color: "var(--tc-slate-900)" }}>
          Page not found
        </h2>
        <p className="text-secondary mb-4">
          The link may be outdated or the page may have been moved.
        </p>
        <Link to="/dashboard" className="btn btn-primary btn-lg rounded-pill px-4">
          <i className="bi bi-house-door me-2" aria-hidden="true" />
          Go to dashboard
        </Link>
      </div>
    </div>
  );
};

export default NotFound;

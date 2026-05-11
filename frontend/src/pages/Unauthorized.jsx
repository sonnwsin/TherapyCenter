import { Link } from "react-router-dom";

const Unauthorized = () => {
  return (
    <div className="auth-page">
      <div className="text-center px-3" style={{ maxWidth: 440 }}>
        <div className="tc-empty-state-icon text-warning">
          <i className="bi bi-shield-exclamation" aria-hidden="true" />
        </div>
        <h2 className="fw-bold mb-2" style={{ color: "var(--tc-slate-900)" }}>
          Access restricted
        </h2>
        <p className="text-secondary mb-4">
          Your account role does not include this page. Contact an administrator if you need access.
        </p>
        <Link to="/dashboard" className="btn btn-primary btn-lg rounded-pill px-4">
          <i className="bi bi-arrow-left me-2" aria-hidden="true" />
          Back to dashboard
        </Link>
      </div>
    </div>
  );
};

export default Unauthorized;

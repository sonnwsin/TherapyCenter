import { useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import API from "../api/axios";
import useAuth from "../context/useAuth";
import { getApiErrorMessage } from "../utils/apiError";
import "./Auth.css";

const Login = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from?.pathname || "/dashboard";

  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const response = await API.post("/auth/login", formData);
      login(response.data);
      navigate(from, { replace: true });
    } catch (err) {
      setError(getApiErrorMessage(err, "Invalid credentials. Please try again."));
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="auth-page px-3">
      <div className="auth-card card border-0 p-4 p-md-5">
        <div className="auth-brand-row">
          <div className="auth-brand-icon">
            <i className="bi bi-heart-pulse" aria-hidden="true" />
          </div>
          <div>
            <h3 className="fw-bold mb-0" style={{ color: "var(--tc-slate-900)" }}>
              Welcome back
            </h3>
            <p className="text-secondary small mb-0">Therapy Center portal</p>
          </div>
        </div>
        <p className="text-secondary mb-4">Sign in with your work email.</p>

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label">Email</label>
            <input
              type="email"
              name="email"
              className="form-control form-control-lg"
              placeholder="you@example.com"
              value={formData.email}
              onChange={handleChange}
              required
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Password</label>
            <input
              type="password"
              name="password"
              className="form-control form-control-lg"
              placeholder="Enter password"
              value={formData.password}
              onChange={handleChange}
              required
            />
          </div>

          {error && (
            <div className="alert alert-danger py-2 small mb-3" role="alert">
              {error}
            </div>
          )}

          <button type="submit" className="btn btn-primary btn-lg w-100 rounded-3" disabled={loading}>
            {loading ? "Signing in..." : "Login"}
          </button>
        </form>

        <p className="small text-secondary text-center mt-4 mb-0">
          New user? <Link to="/register">Create account</Link>
        </p>
      </div>
    </section>
  );
};

export default Login;

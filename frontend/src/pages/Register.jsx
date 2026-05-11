import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import API from "../api/axios";
import useAuth from "../context/useAuth";
import { getApiErrorMessage } from "../utils/apiError";
import "./Auth.css";

const Register = () => {
  const navigate = useNavigate();
  const { login } = useAuth();

  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    email: "",
    password: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e) => {
    setFormData((prev) => ({
      ...prev,
      [e.target.name]: e.target.value,
    }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");

    try {
      const payload = { ...formData, role: "Guardian" };
      const response = await API.post("/auth/register", payload);
      login(response.data);
      navigate("/dashboard", { replace: true });
    } catch (err) {
      setError(getApiErrorMessage(err, "Unable to register with provided details."));
    } finally {
      setLoading(false);
    }
  };

  return (
    <section className="auth-page px-3">
      <div className="auth-card card border-0 p-4 p-md-5">
        <div className="auth-brand-row">
          <div className="auth-brand-icon">
            <i className="bi bi-person-plus" aria-hidden="true" />
          </div>
          <div>
            <h3 className="fw-bold mb-0" style={{ color: "var(--tc-slate-900)" }}>
              Guardian sign-up
            </h3>
            <p className="text-secondary small mb-0">Book care for your family</p>
          </div>
        </div>
        <p className="text-secondary mb-4">Create your portal account.</p>

        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label className="form-label">First Name</label>
            <input
              type="text"
              name="firstName"
              className="form-control form-control-lg"
              placeholder="Enter first name"
              value={formData.firstName}
              onChange={handleChange}
              required
            />
          </div>

          <div className="mb-3">
            <label className="form-label">Last Name</label>
            <input
              type="text"
              name="lastName"
              className="form-control form-control-lg"
              placeholder="Enter last name"
              value={formData.lastName}
              onChange={handleChange}
              required
            />
          </div>

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
            {loading ? "Creating account..." : "Register"}
          </button>
        </form>

        <p className="small text-secondary text-center mt-4 mb-0">
          Already registered? <Link to="/login">Go to login</Link>
        </p>
      </div>
    </section>
  );
};

export default Register;

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

  const [forgotOpen, setForgotOpen] = useState(false);
  const [forgotStep, setForgotStep] = useState("email");
  const [forgotEmail, setForgotEmail] = useState("");
  const [forgotOtp, setForgotOtp] = useState("");
  const [forgotNewPassword, setForgotNewPassword] = useState("");
  const [forgotConfirmPassword, setForgotConfirmPassword] = useState("");
  const [forgotLoading, setForgotLoading] = useState(false);
  const [forgotSuccess, setForgotSuccess] = useState("");
  const [forgotError, setForgotError] = useState("");

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

  const resetForgotFields = () => {
    setForgotStep("email");
    setForgotEmail("");
    setForgotOtp("");
    setForgotNewPassword("");
    setForgotConfirmPassword("");
    setForgotSuccess("");
    setForgotError("");
    setForgotLoading(false);
  };

  const openForgotModal = () => {
    setForgotStep("email");
    setForgotEmail(formData.email?.trim() || "");
    setForgotOtp("");
    setForgotNewPassword("");
    setForgotConfirmPassword("");
    setForgotSuccess("");
    setForgotError("");
    setForgotOpen(true);
  };

  const closeForgotModal = () => {
    setForgotOpen(false);
    resetForgotFields();
  };

  const handleForgotSendCode = async (e) => {
    e.preventDefault();
    setForgotLoading(true);
    setForgotSuccess("");
    setForgotError("");
    try {
      await API.post("/auth/forgot-password", { email: forgotEmail.trim() });
      setForgotStep("reset");
      setForgotOtp("");
      setForgotNewPassword("");
      setForgotConfirmPassword("");
      setForgotError("");
      setForgotSuccess("");
    } catch (err) {
      const raw = getApiErrorMessage(err, "Request failed.");
      const lower = raw.toLowerCase();
      if (lower.includes("no guardian account")) {
        setForgotError("Guardian email not found.");
      } else {
        setForgotError(raw);
      }
    } finally {
      setForgotLoading(false);
    }
  };

  const goBackToEmailStep = () => {
    setForgotStep("email");
    setForgotOtp("");
    setForgotNewPassword("");
    setForgotConfirmPassword("");
    setForgotSuccess("");
    setForgotError("");
  };

  /** Verify OTP with backend, then reset password (same modal). */
  const handleVerifyAndReset = async (e) => {
    e.preventDefault();
    setForgotSuccess("");
    setForgotError("");

    const email = forgotEmail.trim();
    const otp = forgotOtp.trim();
    if (!/^\d{6}$/.test(otp)) {
      setForgotError("Enter the 6-digit code from your email.");
      return;
    }
    if (!forgotNewPassword || forgotNewPassword.length < 6) {
      setForgotError("Password must be at least 6 characters.");
      return;
    }
    if (forgotNewPassword !== forgotConfirmPassword) {
      setForgotError("Passwords do not match.");
      return;
    }

    setForgotLoading(true);
    try {
      await API.post("/auth/verify-otp", { email, otp });
    } catch (err) {
      const apiStatus = err.response?.data?.status;
      const raw = getApiErrorMessage(err, "Verification failed.");
      if (apiStatus === "expired") {
        setForgotError("This code has expired. Request a new code.");
      } else if (apiStatus === "invalid") {
        setForgotError("Invalid OTP.");
      } else {
        const lower = raw.toLowerCase();
        if (lower.includes("expired") || lower.includes("not found")) {
          setForgotError("This code has expired. Request a new code.");
        } else if (lower.includes("invalid")) {
          setForgotError("Invalid OTP.");
        } else {
          setForgotError(raw);
        }
      }
      setForgotLoading(false);
      return;
    }

    try {
      await API.post("/auth/reset-password", { email, newPassword: forgotNewPassword });
    } catch (err) {
      setForgotError(getApiErrorMessage(err, "Could not update password. Try again or request a new code."));
      setForgotLoading(false);
      return;
    }

    setForgotSuccess("Password reset successfully");
    setForgotError("");
    setForgotLoading(false);
    window.setTimeout(() => {
      closeForgotModal();
    }, 2000);
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

          <div className="d-flex justify-content-end align-items-center gap-2 mb-3">
            <button type="button" className="btn btn-link btn-sm p-0 text-decoration-none" onClick={openForgotModal}>
              Forgot password?
            </button>
            <span className="text-secondary small">Guardians</span>
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

      {forgotOpen && (
        <div
          className="modal show d-block"
          tabIndex={-1}
          role="dialog"
          aria-modal="true"
          aria-labelledby="forgot-password-title"
          style={{ backgroundColor: "rgba(15, 23, 42, 0.45)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content border-0 shadow rounded-4">
              <div className="modal-header border-0 pb-0">
                <h2 id="forgot-password-title" className="modal-title h5 fw-bold">
                  {forgotStep === "email" ? "Forgot password" : "Reset password"}
                </h2>
                <button type="button" className="btn-close" aria-label="Close" onClick={closeForgotModal} />
              </div>
              <div className="modal-body pt-2">
                {forgotStep === "email" && (
                  <>
                    <p className="text-secondary small mb-3">
                      Enter the email on your <strong>guardian</strong> account. We will send a one-time code if it
                      matches.
                    </p>
                    <form onSubmit={handleForgotSendCode}>
                      <div className="mb-3">
                        <label className="form-label" htmlFor="forgot-email">
                          Email
                        </label>
                        <input
                          id="forgot-email"
                          type="email"
                          className="form-control form-control-lg"
                          placeholder="guardian@example.com"
                          value={forgotEmail}
                          onChange={(e) => setForgotEmail(e.target.value)}
                          required
                          autoComplete="email"
                        />
                      </div>
                      {forgotError && (
                        <div className="alert alert-danger py-2 small mb-3" role="alert">
                          {forgotError}
                        </div>
                      )}
                      <div className="d-flex gap-2 justify-content-end">
                        <button type="button" className="btn btn-outline-secondary rounded-3" onClick={closeForgotModal}>
                          Close
                        </button>
                        <button type="submit" className="btn btn-primary rounded-3" disabled={forgotLoading}>
                          {forgotLoading ? "Sending…" : "Send reset code"}
                        </button>
                      </div>
                    </form>
                  </>
                )}

                {forgotStep === "reset" && (
                  <>
                    <div className="alert alert-info py-2 small mb-3" role="status">
                      A verification code was sent to <strong>{forgotEmail}</strong>.
                    </div>
                    <form onSubmit={handleVerifyAndReset}>
                      <div className="mb-3">
                        <label className="form-label" htmlFor="forgot-otp">
                          One-time code
                        </label>
                        <input
                          id="forgot-otp"
                          type="text"
                          inputMode="numeric"
                          pattern="\d{6}"
                          maxLength={6}
                          className="form-control form-control-lg"
                          placeholder="000000"
                          value={forgotOtp}
                          onChange={(e) => setForgotOtp(e.target.value.replace(/\D/g, "").slice(0, 6))}
                          required
                          autoComplete="one-time-code"
                        />
                      </div>
                      <div className="mb-3">
                        <label className="form-label" htmlFor="forgot-new-password">
                          New password
                        </label>
                        <input
                          id="forgot-new-password"
                          type="password"
                          className="form-control form-control-lg"
                          placeholder="At least 6 characters"
                          value={forgotNewPassword}
                          onChange={(e) => setForgotNewPassword(e.target.value)}
                          required
                          minLength={6}
                          autoComplete="new-password"
                        />
                      </div>
                      <div className="mb-3">
                        <label className="form-label" htmlFor="forgot-confirm-password">
                          Confirm password
                        </label>
                        <input
                          id="forgot-confirm-password"
                          type="password"
                          className="form-control form-control-lg"
                          placeholder="Repeat new password"
                          value={forgotConfirmPassword}
                          onChange={(e) => setForgotConfirmPassword(e.target.value)}
                          required
                          minLength={6}
                          autoComplete="new-password"
                        />
                      </div>
                      {forgotSuccess && (
                        <div className="alert alert-success py-2 small mb-3" role="status">
                          {forgotSuccess}
                        </div>
                      )}
                      {forgotError && (
                        <div className="alert alert-danger py-2 small mb-3" role="alert">
                          {forgotError}
                        </div>
                      )}
                      <div className="d-flex flex-wrap gap-2 justify-content-between align-items-center">
                        <button type="button" className="btn btn-link btn-sm px-0" onClick={goBackToEmailStep}>
                          Request new code
                        </button>
                        <div className="d-flex gap-2 ms-auto">
                          <button type="button" className="btn btn-outline-secondary rounded-3" onClick={closeForgotModal}>
                            Close
                          </button>
                          <button type="submit" className="btn btn-primary rounded-3" disabled={forgotLoading}>
                            {forgotLoading ? "Working…" : "Verify and reset password"}
                          </button>
                        </div>
                      </div>
                    </form>
                  </>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </section>
  );
};

export default Login;

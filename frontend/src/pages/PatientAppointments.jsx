import { useEffect, useState, useCallback } from "react";
import API from "../api/axios";
import { getApiErrorMessage } from "../utils/apiError";

const PAID_IDS_KEY = "therapyCenterPaidAppointmentIds";

const readPaidIds = () => {
  try {
    const raw = sessionStorage.getItem(PAID_IDS_KEY);
    const arr = raw ? JSON.parse(raw) : [];
    return new Set((Array.isArray(arr) ? arr : []).map(Number));
  } catch {
    return new Set();
  }
};

const writePaidIds = (set) => {
  sessionStorage.setItem(PAID_IDS_KEY, JSON.stringify([...set]));
};

const PatientAppointments = () => {
  const [appointments, setAppointments] = useState([]);
  const [doctors, setDoctors] = useState([]);
  const [therapies, setTherapies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [paidIds, setPaidIds] = useState(() => readPaidIds());
  const [findings, setFindings] = useState(null);
  const [showFindings, setShowFindings] = useState(false);
  const [payModal, setPayModal] = useState(null);
  const [payBusy, setPayBusy] = useState(false);

  const doctorMap = Object.fromEntries(
    doctors.map((d) => [d.doctorId, d.fullName || `Doctor #${d.doctorId}`]),
  );
  const therapyMap = Object.fromEntries(
    therapies.map((t) => [t.therapyId, t.name || `Therapy #${t.therapyId}`]),
  );

  const loadAll = useCallback(async () => {
    setError("");
    try {
      setLoading(true);
      const [apptRes, docRes, thRes] = await Promise.all([
        API.get("/appointment/my"),
        API.get("/doctor/list"),
        API.get("/therapy"),
      ]);
      const appts = Array.isArray(apptRes.data) ? apptRes.data : [];
      const docList = Array.isArray(docRes.data) ? docRes.data : [];
      const thList = Array.isArray(thRes.data) ? thRes.data : [];
      setAppointments(appts);
      setDoctors(docList);
      setTherapies(thList);
    } catch (err) {
      setError(getApiErrorMessage(err, "Failed to load appointments."));
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAll();
  }, [loadAll]);

  const markPaid = (appointmentId) => {
    const next = new Set(paidIds);
    next.add(appointmentId);
    setPaidIds(next);
    writePaidIds(next);
  };

  const openFindings = async (appointmentId) => {
    setError("");
    try {
      const res = await API.get(`/doctorfinding/appointment/${appointmentId}`);
      const data = res.data;
      const row = Array.isArray(data) ? data[0] : data;
      setFindings(row || null);
      setShowFindings(true);
    } catch (err) {
      if (err?.response?.status === 404) {
        setFindings(null);
        setShowFindings(true);
      } else {
        setError(getApiErrorMessage(err, "Could not load findings."));
      }
    }
  };

  const openPay = (appointment) => {
    const therapy = therapies.find((t) => t.therapyId === appointment.therapyId);
    const amount = therapy != null ? Number(therapy.cost) : null;
    setPayModal({ ...appointment, amount });
  };

  const runRazorpay = (appointmentId, amountRupees, orderId, currency) => {
    const key = import.meta.env.VITE_RAZORPAY_KEY;
    if (!key) {
      alert("Missing VITE_RAZORPAY_KEY in .env");
      return;
    }

    const amountPaise = Math.round(Number(amountRupees) * 100);
    if (!amountPaise || amountPaise <= 0) {
      alert("Invalid payment amount.");
      return;
    }

    const options = {
      key,
      amount: amountPaise,
      currency: currency || "INR",
      name: "Therapy Center",
      description: "Appointment payment",
      order_id: orderId,
      handler: async (response) => {
        try {
          await API.post("/payment/verify", {
            appointmentId,
            razorpayOrderId: response.razorpay_order_id,
            razorpayPaymentId: response.razorpay_payment_id,
            razorpaySignature: response.razorpay_signature,
          });
          markPaid(appointmentId);
          setPayModal(null);
          alert("Payment successful.");
          loadAll();
        } catch (err) {
          alert(getApiErrorMessage(err, "Verification failed."));
        }
      },
      theme: { color: "#2563eb" },
    };

    const rzp = new window.Razorpay(options);
    rzp.on("payment.failed", () => {
      alert("Payment failed.");
    });
    rzp.open();
  };

  const startPayment = async () => {
    if (!payModal) return;
    const appointmentId = payModal.appointmentId;
    setPayBusy(true);
    try {
      const res = await API.post("/payment/create-order", { appointmentId });
      const { orderId, amount, currency } = res.data;
      runRazorpay(appointmentId, amount, orderId, currency);
    } catch (err) {
      const msg = getApiErrorMessage(err, "Could not start payment.");
      if (msg.toLowerCase().includes("already paid")) {
        markPaid(appointmentId);
        setPayModal(null);
        loadAll();
      }
      alert(msg);
    } finally {
      setPayBusy(false);
    }
  };

  const formatDate = (dateStr) => {
    if (!dateStr) return "—";
    return new Date(`${dateStr}T12:00:00`).toLocaleDateString(undefined, {
      weekday: "short",
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  const formatTime = (timeStr) => {
    if (!timeStr) return "—";
    const t = String(timeStr).substring(0, 5);
    const [h, m] = t.split(":").map(Number);
    const d = new Date(2000, 0, 1, h, m);
    return d.toLocaleTimeString(undefined, { hour: "numeric", minute: "2-digit" });
  };

  if (loading) {
    return (
      <div className="d-flex justify-content-center py-5">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid px-0">
      <div className="tc-page-header mb-4">
        <h1 className="tc-page-title">My appointments</h1>
        <p>Pay for upcoming visits and read clinical notes after sessions are completed.</p>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible border-0 shadow-sm" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError("")} aria-label="Close" />
        </div>
      )}

      <div className="tc-card">
        <div className="p-3 p-lg-4">
          {appointments.length === 0 ? (
            <div className="tc-empty-state py-4">
              <div className="tc-empty-state-icon">
                <i className="bi bi-calendar-x" aria-hidden="true" />
              </div>
              <p className="mb-0">No appointments yet. Book a slot from the Doctors page.</p>
            </div>
          ) : (
            <div className="table-responsive rounded-3 border">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Doctor</th>
                    <th>Therapy</th>
                    <th>Status</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {appointments.map((a) => (
                    <tr key={a.appointmentId}>
                      <td>{formatDate(a.appointmentDate)}</td>
                      <td>
                        {formatTime(a.startTime)} – {formatTime(a.endTime)}
                      </td>
                      <td>{doctorMap[a.doctorId] || "—"}</td>
                      <td>{therapyMap[a.therapyId] || "—"}</td>
                      <td>
                        <span className="badge rounded-pill bg-light text-dark border">{a.status}</span>
                      </td>
                      <td className="text-end">
                        {a.status === "Completed" && (
                          <button
                            type="button"
                            className="btn btn-sm btn-outline-primary rounded-pill"
                            onClick={() => openFindings(a.appointmentId)}
                          >
                            <i className="bi bi-journal-text me-1" aria-hidden="true" />
                            Findings
                          </button>
                        )}
                        {a.status === "Scheduled" &&
                          (paidIds.has(a.appointmentId) ? (
                            <span className="badge rounded-pill bg-success">Paid</span>
                          ) : (
                            <button
                              type="button"
                              className="btn btn-sm btn-success rounded-pill px-3"
                              onClick={() => openPay(a)}
                            >
                              <i className="bi bi-credit-card me-1" aria-hidden="true" />
                              Pay
                            </button>
                          ))}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {payModal && (
        <div
          className="modal d-block"
          tabIndex={-1}
          style={{ background: "rgba(0,0,0,0.45)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content border-0 shadow-lg rounded-4">
              <div className="modal-header border-0 pb-0">
                <h5 className="modal-title fw-semibold">Confirm payment</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setPayModal(null)}
                  aria-label="Close"
                />
              </div>
              <div className="modal-body">
                <p className="mb-1">
                  <strong>Amount:</strong>{" "}
                  {payModal.amount != null
                    ? `₹ ${Number(payModal.amount).toFixed(2)}`
                    : "—"}
                </p>
                <p className="small text-secondary mb-0">
                  You will complete payment in the Razorpay secure window.
                </p>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-outline-secondary rounded-3"
                  onClick={() => setPayModal(null)}
                >
                  Cancel
                </button>
                <button
                  type="button"
                  className="btn btn-primary rounded-3 px-4"
                  disabled={payBusy || payModal.amount == null}
                  onClick={startPayment}
                >
                  {payBusy ? "Please wait…" : "Pay with Razorpay"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {showFindings && (
        <div
          className="modal d-block"
          tabIndex={-1}
          style={{ background: "rgba(0,0,0,0.45)" }}
        >
          <div className="modal-dialog modal-dialog-centered">
            <div className="modal-content border-0 shadow-lg rounded-4">
              <div className="modal-header border-0 pb-0">
                <h5 className="modal-title fw-semibold">Doctor findings</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowFindings(false)}
                  aria-label="Close"
                />
              </div>
              <div className="modal-body">
                {!findings ? (
                  <p className="text-secondary mb-0">No findings recorded yet.</p>
                ) : (
                  <>
                    <p>
                      <strong>Observations:</strong> {findings.observations || "—"}
                    </p>
                    <p>
                      <strong>Recommendations:</strong>{" "}
                      {findings.recommendations || "—"}
                    </p>
                    <p className="mb-0">
                      <strong>Next session:</strong>{" "}
                      {findings.nextSessionDate
                        ? String(findings.nextSessionDate).slice(0, 10)
                        : "—"}
                    </p>
                  </>
                )}
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-primary rounded-3 w-100"
                  onClick={() => setShowFindings(false)}
                >
                  Close
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PatientAppointments;

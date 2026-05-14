import { Link } from "react-router-dom";
import { useEffect, useState } from "react";
import { ROLES } from "../context/authConstants";
import useAuth from "../context/useAuth";
import API from "../api/axios";
import { getApiErrorMessage } from "../utils/apiError";
import StatTile from "../components/StatTile";

const linkIcon = (path) => {
  if (path.includes("therap")) return "bi-heart-pulse";
  if (path.includes("doctor")) return "bi-person-badge";
  if (path.includes("receptionist")) return "bi-people";
  if (path.includes("report")) return "bi-graph-up";
  if (path.includes("slot")) return "bi-calendar3";
  if (path.includes("book")) return "bi-calendar-plus";
  if (path.includes("patient") && path.includes("appointment")) return "bi-list-task";
  if (path.includes("patient") && path.includes("doctor")) return "bi-hospital";
  if (path.includes("patient") && path.includes("slot")) return "bi-calendar2-week";
  if (path.includes("guardian")) return "bi-people-fill";
  if (path.includes("findings")) return "bi-clipboard2-pulse";
  if (path.includes("appointment")) return "bi-calendar-check";
  return "bi-arrow-right-circle";
};

const Dashboard = () => {
  const { user } = useAuth();
  const [adminStats, setAdminStats] = useState(null);
  const [adminStatsError, setAdminStatsError] = useState("");
  const [myApptCount, setMyApptCount] = useState(null);
  const [doctorApptCount, setDoctorApptCount] = useState(null);
  const [therapyPrices, setTherapyPrices] = useState([]);
  const [therapyPricesStatus, setTherapyPricesStatus] = useState("idle");
  const [therapyPricesError, setTherapyPricesError] = useState("");

  useEffect(() => {
    if (user?.role !== ROLES.ADMIN) return;
    const load = async () => {
      try {
        const res = await API.get("/reports/summary");
        setAdminStats(res.data);
      } catch (err) {
        setAdminStatsError(getApiErrorMessage(err, "Could not load summary."));
      }
    };
    load();
  }, [user?.role]);

  useEffect(() => {
    if (user?.role !== ROLES.GUARDIAN) return;
    const load = async () => {
      try {
        const res = await API.get("/appointment/my");
        const list = Array.isArray(res.data) ? res.data : [];
        setMyApptCount(list.length);
      } catch {
        setMyApptCount("—");
      }
    };
    load();
  }, [user?.role]);

  useEffect(() => {
    if (user?.role !== ROLES.GUARDIAN) return;
    let cancelled = false;
    (async () => {
      setTherapyPricesStatus("loading");
      setTherapyPrices([]);
      setTherapyPricesError("");
      try {
        const res = await API.get("/therapy/prices");
        const list = Array.isArray(res.data) ? res.data : [];
        if (!cancelled) {
          setTherapyPrices(list);
          setTherapyPricesStatus("success");
        }
      } catch (err) {
        if (!cancelled) {
          setTherapyPricesError(getApiErrorMessage(err, "Could not load therapy prices."));
          setTherapyPricesStatus("error");
        }
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [user?.role]);

  useEffect(() => {
    if (user?.role !== ROLES.DOCTOR) return;
    const load = async () => {
      try {
        const res = await API.get("/appointment/doctor");
        const list = Array.isArray(res.data?.data) ? res.data.data : [];
        setDoctorApptCount(list.length);
      } catch {
        setDoctorApptCount("—");
      }
    };
    load();
  }, [user?.role]);

  const heroByRole = {
    [ROLES.ADMIN]: {
      title: "Administrator workspace",
      text: "Configure therapies, staff, and review centre performance.",
    },
    [ROLES.DOCTOR]: {
      title: "Clinician workspace",
      text: "Manage your schedule, complete visits, and record findings.",
    },
    [ROLES.RECEPTIONIST]: {
      title: "Reception workspace",
      text: "Book walk-ins and help patients find the right time slot.",
    },
    [ROLES.GUARDIAN]: {
      title: "Family portal",
      text: "Book care for your dependents, pay securely, and read visit notes.",
    },
    [ROLES.PATIENT]: {
      title: "Patient portal",
      text: "Explore therapists and keep track of your appointments.",
    },
  };

  const hero = heroByRole[user?.role] ?? {
    title: "Welcome",
    text: "Therapy Center management portal.",
  };

  const adminLinks = [
    { path: "/admin/therapies", label: "Therapies", desc: "Types of care, duration, and pricing" },
    { path: "/admin/doctors", label: "Doctors", desc: "Profiles and weekly availability" },
    { path: "/admin/receptionists", label: "Receptionists", desc: "Front-desk staff accounts" },
    { path: "/admin/reports", label: "Reports", desc: "Simple counts: users, appointments, doctors, therapies" },
    { path: "/admin/slots", label: "Slot preview", desc: "Generated availability by doctor" },
  ];

  const receptionistLinks = [
    { path: "/receptionist/book", label: "Book walk-in", desc: "Register patient and reserve a time" },
    { path: "/receptionist/doctors", label: "Doctors", desc: "Directory for patients at the desk" },
    { path: "/receptionist/slots", label: "Slots", desc: "Check generated times for a therapy" },
  ];

  const doctorLinks = [
    { path: "/doctor/appointments", label: "Appointments", desc: "Confirm, complete, or cancel visits" },
    { path: "/doctor/findings", label: "Clinical findings", desc: "Observations and follow-up plans" },
  ];

  const guardianLinks = [
    { path: "/patient/doctors", label: "Find a doctor", desc: "Compare specialisations" },
    { path: "/patient/slots", label: "Book a slot", desc: "Pick therapy, date, and time" },
    { path: "/guardian/patients", label: "My patients", desc: "Dependents on your account" },
    { path: "/patient/appointments", label: "Appointments & pay", desc: "Razorpay and visit notes" },
  ];

  const patientLinks = [
    { path: "/patient/doctors", label: "Doctors", desc: "Who is available to help you" },
    { path: "/patient/appointments", label: "My appointments", desc: "Upcoming and past visits" },
  ];

  const linksByRole = {
    [ROLES.ADMIN]: adminLinks,
    [ROLES.RECEPTIONIST]: receptionistLinks,
    [ROLES.DOCTOR]: doctorLinks,
    [ROLES.GUARDIAN]: guardianLinks,
    [ROLES.PATIENT]: patientLinks,
  };

  const links = linksByRole[user?.role] ?? [];

  return (
    <div className="container-fluid px-0">
      <div className="tc-hero mb-4">
        <h1>{hero.title}</h1>
        <p>{hero.text}</p>
      </div>

      {user?.role === ROLES.ADMIN && adminStats && (
        <div className="tc-stat-grid mb-4">
          <StatTile iconClass="bi-people" variant="teal" label="Users" value={adminStats.totalUsers} />
          <StatTile iconClass="bi-calendar3" variant="blue" label="Appointments" value={adminStats.totalAppointments} />
          <StatTile iconClass="bi-person-badge" variant="slate" label="Doctors" value={adminStats.totalDoctors} />
          <StatTile iconClass="bi-heart-pulse" variant="slate" label="Therapies" value={adminStats.totalTherapies} />
        </div>
      )}

      {user?.role === ROLES.ADMIN && adminStatsError && (
        <div className="alert alert-warning border-0 shadow-sm">{adminStatsError}</div>
      )}

      {user?.role === ROLES.DOCTOR && doctorApptCount != null && (
        <div className="tc-stat-grid mb-4" style={{ maxWidth: 400 }}>
          <StatTile iconClass="bi-calendar-check" variant="teal" label="Your appointments" value={doctorApptCount} />
        </div>
      )}

      {user?.role === ROLES.GUARDIAN && myApptCount != null && (
        <div className="tc-stat-grid mb-4" style={{ maxWidth: 400 }}>
          <StatTile iconClass="bi-list-task" variant="blue" label="Family bookings" value={myApptCount} />
        </div>
      )}

      {user?.role === ROLES.GUARDIAN && (
        <div className="tc-card mb-4">
          <div className="tc-card-header d-flex align-items-center justify-content-between">
            <span>
              <i className="bi bi-heart-pulse me-2" aria-hidden="true" />
              Therapy prices
            </span>
          </div>
          <div className="p-3 p-md-4">
            {(therapyPricesStatus === "idle" || therapyPricesStatus === "loading") && (
              <div className="d-flex justify-content-center py-4">
                <div className="spinner-border text-primary" role="status">
                  <span className="visually-hidden">Loading therapy prices…</span>
                </div>
              </div>
            )}
            {therapyPricesStatus === "error" && therapyPricesError && (
              <div className="alert alert-danger border-0 shadow-sm mb-0" role="alert">
                {therapyPricesError}
              </div>
            )}
            {therapyPricesStatus === "success" && therapyPrices.length === 0 && (
              <div className="tc-empty-state py-3">
                <div className="tc-empty-state-icon">
                  <i className="bi bi-inbox" aria-hidden="true" />
                </div>
                <p className="mb-0 text-secondary">No therapies are listed yet. Check back later.</p>
              </div>
            )}
            {therapyPricesStatus === "success" && therapyPrices.length > 0 && (
              <div className="table-responsive rounded-3 border">
                <table className="table table-hover align-middle mb-0">
                  <thead>
                    <tr>
                      <th>Therapy</th>
                      <th className="text-end">Price</th>
                    </tr>
                  </thead>
                  <tbody>
                    {therapyPrices.map((row) => {
                      const id = row.id ?? row.Id;
                      const name = row.therapyName ?? row.TherapyName ?? "—";
                      const price = row.price ?? row.Price;
                      return (
                        <tr key={id}>
                          <td className="fw-medium">{name}</td>
                          <td className="text-end text-secondary">
                            {price != null && price !== "" ? `₹ ${Number(price).toFixed(2)}` : "—"}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </div>
      )}

      {user?.role === ROLES.RECEPTIONIST && (
        <div className="tc-card mb-4 p-3 p-md-4 d-flex align-items-start gap-3">
          <div className="tc-stat-tile-icon teal mb-0">
            <i className="bi bi-info-circle" aria-hidden="true" />
          </div>
          <div>
            <div className="fw-semibold text-secondary small text-uppercase">Reception tip</div>
            <p className="mb-0 text-secondary">
              Select a <strong>therapy</strong> before opening generated slots so times match session length.
            </p>
          </div>
        </div>
      )}

      <div className="row g-4">
        <div className="col-lg-7">
          <div className="tc-card">
            <div className="tc-card-header d-flex align-items-center justify-content-between">
              <span>
                <i className="bi bi-lightning-charge-fill text-warning me-2" aria-hidden="true" />
                Quick actions
              </span>
            </div>
            <div className="p-3 p-md-4">
              <div className="row g-3">
                {links.map((item) => (
                  <div className="col-md-6" key={item.path}>
                    <Link to={item.path} className="tc-action-card h-100">
                      <div className="tc-action-card-icon">
                        <i className={`bi ${linkIcon(item.path)}`} aria-hidden="true" />
                      </div>
                      <div>
                        <div className="tc-action-card-title">{item.label}</div>
                        <div className="tc-action-card-desc">{item.desc}</div>
                      </div>
                    </Link>
                  </div>
                ))}
                <div className="col-md-6">
                  <Link to="/home" className="tc-action-card h-100 border-dashed" style={{ borderStyle: "dashed" }}>
                    <div className="tc-action-card-icon">
                      <i className="bi bi-house-door" aria-hidden="true" />
                    </div>
                    <div>
                      <div className="tc-action-card-title">Role home</div>
                      <div className="tc-action-card-desc">Shortcut to your default landing page</div>
                    </div>
                  </Link>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div className="col-lg-5">
          <div className="tc-card h-100">
            <div className="tc-card-header">
              <i className="bi bi-clock-history me-2" aria-hidden="true" />
              Getting started
            </div>
            <div className="p-3 p-md-4">
              <ul className="text-secondary small mb-0 ps-3">
                <li className="mb-2">Use the sidebar for all modules.</li>
                <li className="mb-2">Keep patient data accurate when booking.</li>
                <li>Log out when you leave a shared computer.</li>
              </ul>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Dashboard;

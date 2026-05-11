import { useEffect, useState } from "react";
import API from "../api/axios";
import { getApiErrorMessage } from "../utils/apiError";
import StatTile from "../components/StatTile";

const AdminReports = () => {
  const [data, setData] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const load = async () => {
      setError("");
      try {
        setLoading(true);
        const res = await API.get("/reports/summary");
        setData(res.data);
      } catch (err) {
        setError(getApiErrorMessage(err, "Failed to load reports."));
      } finally {
        setLoading(false);
      }
    };
    load();
  }, []);

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
        <h1 className="tc-page-title">Reports summary</h1>
        <p>Simple totals from your database (admin only).</p>
      </div>

      {error && (
        <div className="alert alert-danger border-0 shadow-sm" role="alert">
          {error}
        </div>
      )}

      {data && (
        <div className="row g-3 mb-4">
          {[
            { label: "Users", value: data.totalUsers, icon: "bi-people", variant: "teal" },
            { label: "Appointments", value: data.totalAppointments, icon: "bi-calendar3", variant: "blue" },
            { label: "Doctors", value: data.totalDoctors, icon: "bi-person-badge", variant: "slate" },
            { label: "Therapies", value: data.totalTherapies, icon: "bi-heart-pulse", variant: "slate" },
          ].map((card) => (
            <div className="col-6 col-md-3" key={card.label}>
              <StatTile iconClass={card.icon} variant={card.variant} label={card.label} value={card.value} />
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default AdminReports;

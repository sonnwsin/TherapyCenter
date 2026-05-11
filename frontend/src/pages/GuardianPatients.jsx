import { useEffect, useState } from "react";
import API from "../api/axios";
import { getApiErrorMessage } from "../utils/apiError";

/** Lists patients linked to the logged-in guardian (GET /api/patients/mine). */
const GuardianPatients = () => {
  const [patients, setPatients] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const load = async () => {
      setError("");
      try {
        setLoading(true);
        const res = await API.get("/patients/mine");
        const list = Array.isArray(res.data) ? res.data : [];
        setPatients(list);
      } catch (err) {
        setError(getApiErrorMessage(err, "Failed to load patients."));
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
        <h1 className="tc-page-title">My patients</h1>
        <p>Dependents linked to your guardian account (from the patients API).</p>
      </div>

      {error && (
        <div className="alert alert-danger border-0 shadow-sm" role="alert">
          {error}
        </div>
      )}

      <div className="tc-card">
        <div className="p-4">
          {patients.length === 0 ? (
            <div className="tc-empty-state py-4">
              <div className="tc-empty-state-icon">
                <i className="bi bi-people" aria-hidden="true" />
              </div>
              <p className="mb-0">
                No patients yet. Book online once to create a patient profile.
              </p>
            </div>
          ) : (
            <div className="table-responsive rounded-3 border">
              <table className="table table-hover align-middle mb-0">
                <thead>
                  <tr>
                    <th>Name</th>
                    <th>Date of birth</th>
                    <th>Gender</th>
                  </tr>
                </thead>
                <tbody>
                  {patients.map((p) => (
                    <tr key={p.patientId ?? p.id}>
                      <td className="fw-medium">
                        {(p.firstName || "") + " " + (p.lastName || "")}
                      </td>
                      <td>{p.dateOfBirth ? String(p.dateOfBirth).slice(0, 10) : "—"}</td>
                      <td>{p.gender || "—"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default GuardianPatients;

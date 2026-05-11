import { useEffect, useState, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api/axios";

const PatientDoctors = () => {
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [searchTerm, setSearchTerm] = useState("");
  const navigate = useNavigate();

  const extractData = (response) => {
    return response?.data?.data || response?.data || [];
  };

  const formatTime = (timeStr) => {
    if (!timeStr) return "-";
    // Handle HH:MM:SS format - show only HH:MM
    return timeStr.substring(0, 5);
  };

  const loadDoctors = async () => {
    setError("");
    try {
      setLoading(true);
      const response = await API.get("/doctor/list");
      const data = extractData(response);
      setDoctors(Array.isArray(data) ? data : []);
    } catch (err) {
      const backendMessage =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        err?.message ||
        "Failed to load doctors.";
      setError(backendMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadDoctors();
  }, []);

  const handleViewSlots = (doctorId) => {
    navigate(`/patient/slots?doctorId=${doctorId}`);
  };

  const filteredDoctors = useMemo(() => {
    if (!searchTerm.trim()) return doctors;
    
    const term = searchTerm.toLowerCase();
    return doctors.filter((doctor) => {
      const fullName = (doctor?.fullName || "").toLowerCase();
      const specialization = (doctor?.specialization || "").toLowerCase();
      
      return fullName.includes(term) || specialization.includes(term);
    });
  }, [doctors, searchTerm]);

  if (loading) {
    return (
      <div className="container-fluid px-0">
        <div className="d-flex justify-content-center align-items-center" style={{ minHeight: "200px" }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid px-0">
      <div className="tc-page-header mb-4">
        <div className="d-flex flex-column flex-md-row justify-content-between align-items-start gap-3">
          <div>
            <h1 className="tc-page-title">Our therapists</h1>
            <p className="mb-0">Choose a doctor, then open real-time slots for booking.</p>
          </div>
          <div className="w-100 w-md-auto" style={{ maxWidth: "300px" }}>
            <div className="input-group">
              <span className="input-group-text bg-white border-end-0 rounded-start-3">
                <i className="bi bi-search text-secondary" aria-hidden="true" />
              </span>
              <input
                type="text"
                className="form-control border-start-0 rounded-end-3"
                placeholder="Search name or specialty…"
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible border-0 shadow-sm" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError("")} aria-label="Close" />
        </div>
      )}

      <div className="tc-card">
        <div className="p-3 p-lg-4">
          {filteredDoctors.length === 0 ? (
            <div className="tc-empty-state py-5">
              <div className="tc-empty-state-icon">
                <i className="bi bi-hospital" aria-hidden="true" />
              </div>
              <p className="mb-0">
                {searchTerm ? "No doctors match your search." : "No doctors available yet."}
              </p>
            </div>
          ) : (
            <div className="table-responsive rounded-3 border">
              <table className="table table-hover align-middle mb-0">
                <thead className="table-light">
                  <tr>
                    <th>Doctor Name</th>
                    <th>Specialization</th>
                    <th>Available Days</th>
                    <th>Time</th>
                    <th className="text-end">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredDoctors.map((doctor) => {
                    if (!doctor) return null;
                    const fullName = doctor?.fullName || "-";
                    const email = doctor?.email || "-";
                    
                    return (
                      <tr key={doctor.doctorId}>
                        <td>
                          <div className="d-flex align-items-center">
                            <div
                              className="rounded-circle d-flex align-items-center justify-content-center me-3"
                              style={{
                                width: 40,
                                height: 40,
                                background: "rgba(15, 118, 110, 0.12)",
                                color: "#0f766e",
                              }}
                            >
                              <span className="fw-semibold">
                                {(doctor?.fullName || "?")[0]}
                              </span>
                            </div>
                            <div>
                              <div className="fw-semibold">{fullName}</div>
                              <div className="small text-secondary">{email}</div>
                            </div>
                          </div>
                        </td>
                        <td>{doctor?.specialization || "-"}</td>
                        <td>{doctor?.availableDays || "-"}</td>
                        <td>
                          {formatTime(doctor?.startTime)} - {formatTime(doctor?.endTime)}
                        </td>
                        <td className="text-end">
                          <button
                            type="button"
                            className="btn btn-sm btn-primary rounded-pill px-3"
                            onClick={() => handleViewSlots(doctor.doctorId)}
                          >
                            <i className="bi bi-calendar2-check me-1" aria-hidden="true" />
                            Slots
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
          {searchTerm && filteredDoctors.length > 0 && (
            <div className="mt-3 text-secondary small">
              Showing {filteredDoctors.length} of {doctors.length} doctors
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PatientDoctors;
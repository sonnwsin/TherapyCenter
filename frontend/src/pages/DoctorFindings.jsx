import { useEffect, useState } from "react";
import API from "../api/axios";

const DoctorFindings = () => {
  const [appointments, setAppointments] = useState([]);
  const [findingsMap, setFindingsMap] = useState({});
  const [selectedAppointment, setSelectedAppointment] = useState(null);
  const [formData, setFormData] = useState({
    observations: "",
    recommendations: "",
    nextSessionDate: ""
  });
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState("");
  const [showModal, setShowModal] = useState(false);
  const [isEditMode, setIsEditMode] = useState(false);

  const extractData = (response) => {
    return response?.data?.data || response?.data || [];
  };

  const fetchFindingsForAppointment = async (appointmentId) => {
    try {
      const response = await API.get(`/doctorfinding/appointment/${appointmentId}`);
      const data = response?.data || [];
      // API returns array - use first element if exists
      const finding = Array.isArray(data) && data.length > 0 ? data[0] : null;
      setFindingsMap(prev => ({ ...prev, [appointmentId]: finding }));
      return finding;
    } catch (err) {
      if (err?.response?.status === 404) {
        setFindingsMap(prev => ({ ...prev, [appointmentId]: null }));
        return null;
      }
      console.error("Failed to fetch finding:", err);
      return null;
    }
  };

  const loadAppointmentsAndFindings = async () => {
    setError("");
    try {
      setLoading(true);
      const response = await API.get("/appointment/doctor");
      const data = extractData(response);
      const allAppointments = Array.isArray(data) ? data : [];

      const active = allAppointments.filter((apt) => apt?.status !== "Cancelled");

      setAppointments(active);

      for (const apt of active) {
        if (apt?.appointmentId) {
          await fetchFindingsForAppointment(apt.appointmentId);
        }
      }
    } catch (err) {
      const backendMessage =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        err?.message ||
        "Failed to load appointments.";
      setError(backendMessage);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAppointmentsAndFindings();
  }, []);

  const handleAddClick = (appointment) => {
    setSelectedAppointment(appointment);
    setFormData({
      observations: "",
      recommendations: "",
      nextSessionDate: ""
    });
    setIsEditMode(true);
    setShowModal(true);
  };

  const handleViewDetailsClick = (appointment, finding) => {
    setSelectedAppointment(appointment);
    setFormData({
      observations: finding?.observations || "",
      recommendations: finding?.recommendations || "",
      nextSessionDate: finding?.nextSessionDate?.split("T")[0] || ""
    });
    setIsEditMode(false);
    setShowModal(true);
  };

  const handleEnableEdit = () => {
    setIsEditMode(true);
  };

  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const handleSave = async () => {
    if (!selectedAppointment) return;
    
    setSaving(true);
    setError("");
    
    try {
      const finding = findingsMap[selectedAppointment.appointmentId];
      
      if (finding?.findingId) {
        // Update existing finding
        await API.put(`/doctorfinding/${finding.findingId}`, {
          findingId: finding.findingId,
          appointmentId: selectedAppointment.appointmentId,
          observations: formData.observations,
          recommendations: formData.recommendations,
          nextSessionDate: formData.nextSessionDate || null
        });
      } else {
        // Create new finding
        await API.post("/doctorfinding", {
          appointmentId: selectedAppointment.appointmentId,
          observations: formData.observations,
          recommendations: formData.recommendations,
          nextSessionDate: formData.nextSessionDate || null
        });
      }
      
      // Refresh findings
      await fetchFindingsForAppointment(selectedAppointment.appointmentId);
      setShowModal(false);
      setSelectedAppointment(null);
    } catch (err) {
      const backendMessage =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        err?.message ||
        "Failed to save finding.";
      setError(backendMessage);
    } finally {
      setSaving(false);
    }
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedAppointment(null);
    setIsEditMode(false);
    setError("");
  };

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
      {error && !showModal && (
        <div className="alert alert-danger alert-dismissible" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError("")}></button>
        </div>
      )}

      <div className="card border-0 shadow-sm">
        <div className="card-header bg-white border-0 pt-4 pb-0">
          <h4 className="mb-0 fw-semibold">Doctor Findings</h4>
          <p className="text-secondary small mb-0 mt-1">
            Manage findings for completed appointments
          </p>
        </div>
        <div className="card-body p-3 p-lg-4">
          {appointments.length === 0 ? (
            <p className="text-secondary mb-0">No completed appointments found.</p>
          ) : (
            <div className="table-responsive">
              <table className="table table-bordered table-striped align-middle mb-0">
                <thead className="table-light">
                  <tr>
                    <th>Patient ID</th>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Status</th>
                    <th className="text-end">Action</th>
                  </tr>
                </thead>
                <tbody>
                  {appointments.map((appointment) => {
                    if (!appointment) return null;
                    const finding = findingsMap[appointment.appointmentId];
                    const hasFinding = finding && finding.findingId;

                    return (
                      <tr key={appointment.appointmentId}>
                        <td>{appointment.patientId ?? "-"}</td>
                        <td>{appointment.appointmentDate ?? "-"}</td>
                        <td>
                          {appointment.startTime ?? "-"} - {appointment.endTime ?? "-"}
                        </td>
                        <td>
                          <span className="badge bg-success">{appointment.status}</span>
                        </td>
                        <td className="text-end">
                          {hasFinding ? (
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => handleViewDetailsClick(appointment, finding)}
                            >
                              View Details
                            </button>
                          ) : (
                            <button
                              type="button"
                              className="btn btn-sm btn-primary"
                              onClick={() => handleAddClick(appointment)}
                            >
                              Add Finding
                            </button>
                          )}
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

      {/* Modal */}
      {showModal && (
        <div 
          className="modal show d-block" 
          tabIndex="-1" 
          style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
        >
          <div className="modal-dialog modal-lg modal-dialog-centered">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">
                  {!findingsMap[selectedAppointment?.appointmentId]?.findingId 
                    ? "Add Finding" 
                    : isEditMode 
                      ? "Edit Finding" 
                      : "View Finding"}
                </h5>
                <button 
                  type="button" 
                  className="btn-close" 
                  onClick={handleCloseModal}
                  disabled={saving}
                ></button>
              </div>
              <div className="modal-body">
                {error && (
                  <div className="alert alert-danger" role="alert">
                    {error}
                  </div>
                )}
                
                <div className="mb-3">
                  <label className="form-label">Patient ID</label>
                  <input 
                    type="text" 
                    className="form-control" 
                    value={selectedAppointment?.patientId ?? "-"} 
                    disabled 
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label">Appointment Date</label>
                  <input 
                    type="text" 
                    className="form-control" 
                    value={selectedAppointment?.appointmentDate ?? "-"} 
                    disabled 
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Observations {!isEditMode ? "" : <span className="text-danger">*</span>}
                  </label>
                  <textarea
                    name="observations"
                    className="form-control"
                    rows="4"
                    value={formData.observations}
                    onChange={handleInputChange}
                    placeholder={isEditMode ? "Enter patient observations..." : ""}
                    required={isEditMode}
                    disabled={saving || !isEditMode}
                    readOnly={!isEditMode}
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Recommendations
                  </label>
                  <textarea
                    name="recommendations"
                    className="form-control"
                    rows="3"
                    value={formData.recommendations}
                    onChange={handleInputChange}
                    placeholder={isEditMode ? "Enter recommendations..." : ""}
                    disabled={saving || !isEditMode}
                    readOnly={!isEditMode}
                  />
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Next Session Date
                  </label>
                  <input
                    type="date"
                    name="nextSessionDate"
                    className="form-control"
                    value={formData.nextSessionDate}
                    onChange={handleInputChange}
                    disabled={saving || !isEditMode}
                    readOnly={!isEditMode}
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button 
                  type="button" 
                  className="btn btn-secondary" 
                  onClick={handleCloseModal}
                  disabled={saving}
                >
                  {isEditMode ? "Cancel" : "Close"}
                </button>
                {!isEditMode && findingsMap[selectedAppointment?.appointmentId]?.findingId && (
                  <button 
                    type="button" 
                    className="btn btn-primary"
                    onClick={handleEnableEdit}
                    disabled={saving}
                  >
                    Edit Finding
                  </button>
                )}
                {isEditMode && (
                  <button 
                    type="button" 
                    className="btn btn-primary"
                    onClick={handleSave}
                    disabled={saving || !formData.observations.trim()}
                  >
                    {saving ? (
                      <>
                        <span className="spinner-border spinner-border-sm me-2" role="status"></span>
                        Saving...
                      </>
                    ) : (
                      "Save Changes"
                    )}
                  </button>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default DoctorFindings;

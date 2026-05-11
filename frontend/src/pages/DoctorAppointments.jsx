import { useEffect, useState } from "react";
import API from "../api/axios";

const DoctorAppointments = () => {
  const [appointments, setAppointments] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionLoading, setActionLoading] = useState({});

  useEffect(() => {
    const loadInitial = async () => {
      setError("");
      try {
        setLoading(true);
        const response = await API.get("/appointment/doctor");
        const rawList = response?.data?.data || [];
        setAppointments(Array.isArray(rawList) ? rawList : []);
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

    loadInitial();
  }, []);

  const handleComplete = async (appointmentId) => {
    if (actionLoading[appointmentId]) return;
    
    setActionLoading(prev => ({ ...prev, [appointmentId]: true }));
    try {
      await API.put(`/appointment/${appointmentId}/complete`);
      const response = await API.get("/appointment/doctor");
      const rawList = response?.data?.data || [];
      setAppointments(Array.isArray(rawList) ? rawList : []);
    } catch (err) {
      const backendMessage =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        "Failed to complete appointment.";
      setError(backendMessage);
    } finally {
      setActionLoading(prev => ({ ...prev, [appointmentId]: false }));
    }
  };

  const handleCancel = async (appointmentId) => {
    if (actionLoading[appointmentId]) return;
    
    setActionLoading(prev => ({ ...prev, [appointmentId]: true }));
    try {
      await API.put(`/appointment/${appointmentId}/cancel`);
      const response = await API.get("/appointment/doctor");
      const rawList = response?.data?.data || [];
      setAppointments(Array.isArray(rawList) ? rawList : []);
    } catch (err) {
      const backendMessage =
        err?.response?.data?.message ||
        err?.response?.data?.title ||
        "Failed to cancel appointment.";
      setError(backendMessage);
    } finally {
      setActionLoading(prev => ({ ...prev, [appointmentId]: false }));
    }
  };

  return (
    <div className="container-fluid px-0">
      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <p className="mb-0">Loading...</p>
      ) : appointments.length === 0 ? (
        <p className="text-secondary mb-0">No appointments found.</p>
      ) : (
        <div className="card border-0 shadow-sm">
          <div className="card-header bg-white border-0 pt-4 pb-0">
            <h4 className="mb-0 fw-semibold">Assigned Appointments</h4>
          </div>
          <div className="card-body p-3 p-lg-4">
            <div className="table-responsive">
              <table className="table table-bordered table-striped align-middle mb-0">
                <thead className="table-light">
                  <tr>
                    <th>Patient ID</th>
                    <th>Date</th>
                    <th>Time</th>
                    <th>Status</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {appointments.map((appointment) => {
                    if (!appointment) return null;
                    const isScheduled = appointment.status === "Scheduled";

                    return (
                      <tr key={appointment.appointmentId}>
                        <td>{appointment.patientId ?? "-"}</td>
                        <td>{appointment.appointmentDate ?? "-"}</td>
                        <td>
                          {appointment.startTime ?? "-"} - {appointment.endTime ?? "-"}
                        </td>
                        <td>{appointment.status ?? "-"}</td>
                        <td className="text-end">
                          <div className="d-inline-flex gap-2">
                            <button
                              type="button"
                              className="btn btn-sm btn-success"
                              disabled={!isScheduled || actionLoading[appointment.appointmentId]}
                              onClick={() => handleComplete(appointment.appointmentId)}
                            >
                              {actionLoading[appointment.appointmentId] ? "..." : "Complete"}
                            </button>
                            <button
                              type="button"
                              className="btn btn-sm btn-danger"
                              disabled={!isScheduled || actionLoading[appointment.appointmentId]}
                              onClick={() => handleCancel(appointment.appointmentId)}
                            >
                              {actionLoading[appointment.appointmentId] ? "..." : "Cancel"}
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default DoctorAppointments;


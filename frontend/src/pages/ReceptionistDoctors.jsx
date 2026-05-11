import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import API from "../api/axios";

const normalizeDoctor = (doctor) => {
  const firstName = doctor.user?.firstName ?? doctor.firstName ?? "";
  const lastName = doctor.user?.lastName ?? doctor.lastName ?? "";
  return {
    ...doctor,
    doctorId: doctor.doctorId ?? doctor.id,
    fullName: `${firstName} ${lastName}`.trim() || doctor.fullName || "Unknown Doctor",
    specialization: doctor.specialization ?? "-",
    availableDays: Array.isArray(doctor.availableDays)
      ? doctor.availableDays.join(", ")
      : (doctor.availableDays ?? "-"),
    startTime: doctor.startTime ?? "-",
    endTime: doctor.endTime ?? "-",
  };
};

const ReceptionistDoctors = () => {
  const navigate = useNavigate();
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const fetchDoctors = async () => {
      try {
        setLoading(true);
        setError("");
        const response = await API.get("/doctor");
        const rawList = Array.isArray(response.data) ? response.data : [];
        setDoctors(rawList.map(normalizeDoctor));
      } catch {
        setError("Failed to load doctors.");
      } finally {
        setLoading(false);
      }
    };

    fetchDoctors();
  }, []);

  const handleBookClick = (doctorId) => {
    navigate(`/receptionist/book?doctorId=${doctorId}`);
  };

  return (
    <div className="container-fluid px-0">
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-body p-4">
          <h4 className="mb-1 fw-semibold">Doctors Directory</h4>
          <p className="text-secondary mb-0">Browse doctors and start appointment booking.</p>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <div className="card border-0 shadow-sm">
          <div className="card-body p-4">Loading doctors...</div>
        </div>
      ) : doctors.length === 0 ? (
        <div className="card border-0 shadow-sm">
          <div className="card-body p-4 text-secondary">No doctors available.</div>
        </div>
      ) : (
        <div className="row g-3">
          {doctors.map((doctor) => (
            <div key={doctor.doctorId} className="col-md-6 col-xl-4">
              <div className="card border-0 shadow-sm h-100">
                <div className="card-body p-4 d-flex flex-column">
                  <h5 className="fw-semibold mb-3">{doctor.fullName}</h5>
                  <p className="mb-2">
                    <span className="text-secondary">Specialization:</span> {doctor.specialization}
                  </p>
                  <p className="mb-2">
                    <span className="text-secondary">Available Days:</span> {doctor.availableDays}
                  </p>
                  <p className="mb-4">
                    <span className="text-secondary">Time:</span> {doctor.startTime} - {doctor.endTime}
                  </p>

                  <div className="mt-auto d-flex justify-content-end">
                    <button
                      type="button"
                      className="btn btn-primary btn-sm"
                      onClick={() => handleBookClick(doctor.doctorId)}
                    >
                      Book Appointment
                    </button>
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default ReceptionistDoctors;

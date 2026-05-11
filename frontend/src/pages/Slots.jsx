import { useEffect, useState } from "react";
import API from "../api/axios";

const getTodayDate = () => new Date().toISOString().split("T")[0];

const normalizeDoctor = (doctor) => ({
  ...doctor,
  doctorId: doctor.doctorId ?? doctor.id,
  fullName: doctor.fullName ?? `${doctor.firstName ?? ""} ${doctor.lastName ?? ""}`.trim(),
});

const normalizeSlot = (slot) => ({
  ...slot,
  slotId: slot.slotId ?? slot.id ?? `${slot.startTime}-${slot.endTime}`,
  startTime: slot.startTime ?? "",
  endTime: slot.endTime ?? "",
  status: slot.status ?? (slot.isBooked ? "Booked" : "Available"),
});

const Slots = () => {
  const [doctors, setDoctors] = useState([]);
  const [therapies, setTherapies] = useState([]);
  const [therapyId, setTherapyId] = useState("");
  const [slots, setSlots] = useState([]);
  const [selectedDoctorId, setSelectedDoctorId] = useState("");
  const [selectedDate, setSelectedDate] = useState(getTodayDate());
  const [loadingDoctors, setLoadingDoctors] = useState(true);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [error, setError] = useState("");

  const fetchDoctors = async () => {
    const response = await API.get("/doctor");
    const rawList = Array.isArray(response.data) ? response.data : [];
    return rawList.map(normalizeDoctor);
  };

  const fetchGeneratedSlots = async (doctorId, date, tid) => {
    const params = { date };
    if (tid) params.therapyId = tid;
    const response = await API.get(`/slot/doctor/${doctorId}/generated`, {
      params,
    });
    const rawList = Array.isArray(response.data) ? response.data : [];
    return rawList.map(normalizeSlot);
  };

  useEffect(() => {
    const loadDoctors = async () => {
      try {
        setLoadingDoctors(true);
        setError("");
        const doctorList = await fetchDoctors();
        setDoctors(doctorList);
      } catch {
        setError("Failed to load doctors.");
      } finally {
        setLoadingDoctors(false);
      }
    };

    loadDoctors();
  }, []);

  useEffect(() => {
    const loadTherapies = async () => {
      try {
        const res = await API.get("/therapy");
        setTherapies(Array.isArray(res.data) ? res.data : []);
      } catch {
        setTherapies([]);
      }
    };
    loadTherapies();
  }, []);

  const handleDoctorChange = async (doctorId) => {
    setSelectedDoctorId(doctorId);
    setError("");
    setSlots([]);

    if (!doctorId || !selectedDate || !therapyId) return;

    try {
      setLoadingSlots(true);
      const slotList = await fetchGeneratedSlots(doctorId, selectedDate, therapyId);
      setSlots(slotList);
    } catch {
      setError("Failed to load slots.");
    } finally {
      setLoadingSlots(false);
    }
  };

  const handleDateChange = async (date) => {
    setSelectedDate(date);
    setError("");
    setSlots([]);

    if (!date || !selectedDoctorId || !therapyId) return;

    try {
      setLoadingSlots(true);
      const slotList = await fetchGeneratedSlots(selectedDoctorId, date, therapyId);
      setSlots(slotList);
    } catch {
      setError("Failed to load slots.");
    } finally {
      setLoadingSlots(false);
    }
  };

  return (
    <div className="container-fluid px-0">
      <div className="tc-page-header mb-4">
        <h1 className="tc-page-title">Slot calendar</h1>
        <p>Preview generated availability — pick therapy so windows match session length.</p>
      </div>

      <div className="tc-card mb-4">
        <div className="tc-card-header">Filters</div>
        <div className="p-4">
          <div className="row g-3">
            <div className="col-md-6">
              <label className="form-label">Select Doctor</label>
              <select
                className="form-select"
                value={selectedDoctorId}
                onChange={(e) => handleDoctorChange(e.target.value)}
                disabled={loadingDoctors}
              >
                <option value="">Select Doctor</option>
                {doctors.map((doctor) => (
                  <option key={doctor.doctorId} value={doctor.doctorId}>
                    {doctor.fullName}
                  </option>
                ))}
              </select>
            </div>

            <div className="col-md-6">
              <label className="form-label">Date</label>
              <input
                type="date"
                className="form-control"
                value={selectedDate}
                onChange={(e) => handleDateChange(e.target.value)}
              />
            </div>

            <div className="col-md-6">
              <label className="form-label">Therapy (for slot length)</label>
              <select
                className="form-select"
                value={therapyId}
                onChange={async (e) => {
                  const tid = e.target.value;
                  setTherapyId(tid);
                  setSlots([]);
                  setError("");
                  if (!selectedDoctorId || !selectedDate || !tid) return;
                  try {
                    setLoadingSlots(true);
                    const slotList = await fetchGeneratedSlots(
                      selectedDoctorId,
                      selectedDate,
                      tid,
                    );
                    setSlots(slotList);
                  } catch {
                    setError("Failed to load slots.");
                  } finally {
                    setLoadingSlots(false);
                  }
                }}
              >
                <option value="">Select therapy</option>
                {therapies.map((t) => (
                  <option key={t.therapyId} value={t.therapyId}>
                    {t.name} ({t.durationMinutes} min)
                  </option>
                ))}
              </select>
            </div>
          </div>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger border-0 shadow-sm" role="alert">
          {error}
        </div>
      )}

      <div className="tc-card">
        <div className="tc-card-header">Generated slots</div>
        <div className="p-4">

          {!selectedDoctorId || !therapyId ? (
            <p className="text-secondary mb-0">Select doctor and therapy to view slots.</p>
          ) : loadingSlots ? (
            <p className="mb-0 text-secondary">
              <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true" />
              Loading slots…
            </p>
          ) : slots.length === 0 ? (
            <p className="text-secondary mb-0">No slots for this selection.</p>
          ) : (
            <div className="d-flex flex-wrap gap-2">
              {slots.map((slot) => {
                const isBooked = String(slot.status).toLowerCase() === "booked";
                return (
                  <button
                    key={slot.slotId}
                    type="button"
                    className={`btn btn-sm rounded-pill px-3 ${isBooked ? "btn-outline-secondary" : "btn-outline-primary"}`}
                    disabled={isBooked}
                  >
                    {slot.startTime} – {slot.endTime}
                  </button>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default Slots;

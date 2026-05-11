import { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import API from "../api/axios";
import { getApiErrorMessage } from "../utils/apiError";

const getTodayDate = () => new Date().toISOString().split("T")[0];

const toTimeApi = (t) => {
  const s = typeof t === "string" ? t : t == null ? "" : String(t);
  if (s.length >= 8) return s.substring(0, 8);
  if (s.length === 5) return `${s}:00`;
  return s;
};

const PatientSlots = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const doctorId = useMemo(
    () => new URLSearchParams(location.search).get("doctorId"),
    [location.search],
  );

  const [doctor, setDoctor] = useState(null);
  const [therapies, setTherapies] = useState([]);
  const [therapyId, setTherapyId] = useState("");
  const [selectedDate, setSelectedDate] = useState("");
  const [slots, setSlots] = useState([]);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [loadingDoctor, setLoadingDoctor] = useState(true);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [booking, setBooking] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [formData, setFormData] = useState({
    firstName: "",
    lastName: "",
    dateOfBirth: "",
    gender: "",
    medicalHistory: "",
  });

  useEffect(() => {
    const loadTherapies = async () => {
      try {
        const res = await API.get("/therapy");
        const list = Array.isArray(res.data) ? res.data : [];
        setTherapies(list);
      } catch {
        setTherapies([]);
      }
    };
    loadTherapies();
  }, []);

  useEffect(() => {
    if (!doctorId) {
      setError("No doctor selected. Go back and pick a doctor.");
      setLoadingDoctor(false);
      return;
    }

    const load = async () => {
      setError("");
      try {
        setLoadingDoctor(true);
        const res = await API.get("/doctor/list");
        const list = Array.isArray(res.data) ? res.data : [];
        const d = list.find((x) => Number(x.doctorId) === Number(doctorId));
        if (!d) {
          setError("Doctor not found.");
          setDoctor(null);
        } else {
          setDoctor(d);
        }
      } catch (err) {
        setError(getApiErrorMessage(err, "Failed to load doctors."));
      } finally {
        setLoadingDoctor(false);
      }
    };
    load();
  }, [doctorId]);

  useEffect(() => {
    const loadSlots = async () => {
      setSelectedSlot(null);
      setSlots([]);
      if (!doctorId || !therapyId || !selectedDate) return;

      setLoadingSlots(true);
      setError("");
      try {
        const res = await API.get(`/slot/doctor/${doctorId}/generated`, {
          params: { date: selectedDate, therapyId },
        });
        const raw = Array.isArray(res.data) ? res.data : [];
        setSlots(
          raw.map((s) => ({
            startTime: s.startTime,
            endTime: s.endTime,
            status: s.status || "Available",
          })),
        );
      } catch (err) {
        setError(getApiErrorMessage(err, "Failed to load slots."));
      } finally {
        setLoadingSlots(false);
      }
    };
    loadSlots();
  }, [doctorId, therapyId, selectedDate]);

  const handleBook = async () => {
    setError("");
    setSuccess("");
    if (!formData.firstName.trim() || !formData.lastName.trim()) {
      setError("First and last name are required.");
      return;
    }
    if (!therapyId || !selectedDate || !selectedSlot) {
      setError("Choose therapy, date, and a free slot.");
      return;
    }

    setBooking(true);
    try {
      await API.post("/appointment/online", {
        firstName: formData.firstName.trim(),
        lastName: formData.lastName.trim(),
        dateOfBirth: formData.dateOfBirth || null,
        gender: formData.gender || null,
        medicalHistory: formData.medicalHistory || null,
        doctorId: Number(doctorId),
        therapyId: Number(therapyId),
        appointmentDate: selectedDate,
        startTime: toTimeApi(selectedSlot.startTime),
        endTime: toTimeApi(selectedSlot.endTime),
      });
      setSuccess("Appointment booked. Redirecting…");
      setTimeout(() => navigate("/patient/appointments"), 1200);
    } catch (err) {
      setError(getApiErrorMessage(err, "Booking failed."));
    } finally {
      setBooking(false);
    }
  };

  if (loadingDoctor) {
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
        <h1 className="tc-page-title">Book online</h1>
        <p>Slots follow the therapy duration returned by the server.</p>
      </div>

      {error && (
        <div className="alert alert-danger alert-dismissible mb-3 border-0 shadow-sm" role="alert">
          {error}
          <button type="button" className="btn-close" onClick={() => setError("")} aria-label="Close" />
        </div>
      )}
      {success && <div className="alert alert-success mb-3 border-0 shadow-sm">{success}</div>}

      <div className="tc-card">
        <div className="p-4">
          {doctor && (
            <div className="rounded-3 p-3 mb-4 border bg-light">
              <div className="fw-semibold">{doctor.fullName}</div>
              <div className="small text-secondary">
                {doctor.specialization} · {doctor.availableDays}
              </div>
            </div>
          )}

          <div className="row g-3 mb-3">
            <div className="col-md-6">
              <label className="form-label">Therapy</label>
              <select
                className="form-select"
                value={therapyId}
                onChange={(e) => setTherapyId(e.target.value)}
              >
                <option value="">Select therapy</option>
                {therapies.map((t) => (
                  <option key={t.therapyId} value={t.therapyId}>
                    {t.name} ({t.durationMinutes} min)
                  </option>
                ))}
              </select>
            </div>
            <div className="col-md-6">
              <label className="form-label">Date</label>
              <input
                type="date"
                className="form-control"
                min={getTodayDate()}
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
              />
            </div>
          </div>

          <h6 className="fw-semibold mb-2">Patient</h6>
          <div className="row g-3 mb-4">
            <div className="col-md-6">
              <input
                className="form-control"
                placeholder="First name"
                value={formData.firstName}
                onChange={(e) =>
                  setFormData((p) => ({ ...p, firstName: e.target.value }))
                }
              />
            </div>
            <div className="col-md-6">
              <input
                className="form-control"
                placeholder="Last name"
                value={formData.lastName}
                onChange={(e) =>
                  setFormData((p) => ({ ...p, lastName: e.target.value }))
                }
              />
            </div>
            <div className="col-md-6">
              <input
                type="date"
                className="form-control"
                value={formData.dateOfBirth}
                onChange={(e) =>
                  setFormData((p) => ({ ...p, dateOfBirth: e.target.value }))
                }
              />
              <div className="form-text">Date of birth (optional)</div>
            </div>
            <div className="col-md-6">
              <select
                className="form-select"
                value={formData.gender}
                onChange={(e) =>
                  setFormData((p) => ({ ...p, gender: e.target.value }))
                }
              >
                <option value="">Gender (optional)</option>
                <option value="Male">Male</option>
                <option value="Female">Female</option>
                <option value="Other">Other</option>
              </select>
            </div>
            <div className="col-12">
              <textarea
                className="form-control"
                rows={2}
                placeholder="Medical history (optional)"
                value={formData.medicalHistory}
                onChange={(e) =>
                  setFormData((p) => ({ ...p, medicalHistory: e.target.value }))
                }
              />
            </div>
          </div>

          <h6 className="fw-semibold mb-2">Available times</h6>
          {!therapyId || !selectedDate ? (
            <p className="text-secondary small">Select therapy and date first.</p>
          ) : loadingSlots ? (
            <p className="small">Loading slots…</p>
          ) : slots.length === 0 ? (
            <p className="text-secondary small">No slots for this day.</p>
          ) : (
            <div className="d-flex flex-wrap gap-2 mb-4">
              {slots.map((slot, idx) => {
                const booked =
                  String(slot.status).toLowerCase() === "booked";
                const label = `${toTimeApi(slot.startTime).slice(0, 5)} – ${toTimeApi(slot.endTime).slice(0, 5)}`;
                const selected =
                  selectedSlot &&
                  toTimeApi(selectedSlot.startTime) === toTimeApi(slot.startTime) &&
                  toTimeApi(selectedSlot.endTime) === toTimeApi(slot.endTime);
                return (
                  <button
                    type="button"
                    key={`${label}-${idx}`}
                    className={`btn btn-sm rounded-pill px-3 ${booked ? "btn-outline-secondary" : selected ? "btn-primary" : "btn-outline-primary"}`}
                    disabled={booked}
                    onClick={() => setSelectedSlot(slot)}
                  >
                    {label}
                  </button>
                );
              })}
            </div>
          )}

          <div className="d-flex gap-2 flex-wrap">
            <button
              type="button"
              className="btn btn-outline-secondary rounded-3"
              onClick={() => navigate("/patient/doctors")}
            >
              Back
            </button>
            <button
              type="button"
              className="btn btn-primary rounded-3 px-4"
              disabled={booking}
              onClick={handleBook}
            >
              {booking ? "Booking…" : "Confirm booking"}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PatientSlots;

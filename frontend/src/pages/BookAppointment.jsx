import { useEffect, useMemo, useState } from "react";
import API from "../api/axios";

const getTodayDate = () => new Date().toISOString().split("T")[0];

const normalizeDoctor = (doctor) => {
  const firstName = doctor?.user?.firstName ?? doctor?.firstName ?? "";
  const lastName = doctor?.user?.lastName ?? doctor?.lastName ?? "";
  const fullName =
    `${firstName} ${lastName}`.trim() ||
    doctor?.fullName ||
    doctor?.email ||
    "";

  return {
    doctorId: Number(doctor?.doctorId ?? doctor?.id),
    fullName,
  };
};

const normalizeSlot = (slot) => ({
  startTime: slot?.startTime ?? "",
  endTime: slot?.endTime ?? "",
  status: slot?.status ?? "Available",
});

const BookAppointment = () => {
  const [doctors, setDoctors] = useState([]);
  const [selectedDoctorId, setSelectedDoctorId] = useState("");
  const [selectedDate, setSelectedDate] = useState(getTodayDate());

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");

  const [slots, setSlots] = useState([]);
  const [selectedSlot, setSelectedSlot] = useState(null);
  const [therapies, setTherapies] = useState([]);
  const [therapyId, setTherapyId] = useState("");

  const [loadingDoctors, setLoadingDoctors] = useState(false);
  const [loadingSlots, setLoadingSlots] = useState(false);
  const [booking, setBooking] = useState(false);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const canFetchSlots = useMemo(
    () => Boolean(selectedDoctorId && selectedDate && therapyId),
    [selectedDoctorId, selectedDate, therapyId],
  );

  useEffect(() => {
    const loadDoctors = async () => {
      setLoadingDoctors(true);
      setError("");
      setSuccess("");
      try {
        const response = await API.get("/doctor");
        const rawList = Array.isArray(response.data) ? response.data : [];
        setDoctors(rawList.map(normalizeDoctor));
      } catch (err) {
        console.error(err?.response?.data);
        setError(
          err?.response?.data?.message ||
            err?.response?.data?.title ||
            "Failed to load doctors.",
        );
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

  const fetchSlots = async (doctorId, date, tid) => {
    const params = { date };
    if (tid) params.therapyId = tid;
    const response = await API.get(`/slot/doctor/${doctorId}/generated`, {
      params,
    });
    const rawList = Array.isArray(response.data) ? response.data : [];
    return rawList.map(normalizeSlot);
  };

  useEffect(() => {
    const loadSlots = async () => {
      setError("");
      setSuccess("");
      setSelectedSlot(null);
      setSlots([]);

      if (!canFetchSlots) return;

      setLoadingSlots(true);
      try {
        const list = await fetchSlots(selectedDoctorId, selectedDate, therapyId);
        setSlots(list);
      } catch (err) {
        console.error(err?.response?.data);
        setError(
          err?.response?.data?.message ||
            err?.response?.data?.title ||
            "Failed to load slots.",
        );
      } finally {
        setLoadingSlots(false);
      }
    };

    loadSlots();
  }, [canFetchSlots, selectedDoctorId, selectedDate, therapyId]);

  const handleBook = async () => {
    setError("");
    setSuccess("");

    if (!selectedDoctorId) {
      setError("Doctor must be selected.");
      return;
    }
    if (!selectedSlot) {
      setError("Slot must be selected.");
      return;
    }
    if (!firstName.trim()) {
      setError("First name required.");
      return;
    }
    if (!lastName.trim()) {
      setError("Last name required.");
      return;
    }
    if (!therapyId) {
      setError("Select a therapy.");
      return;
    }

    // Backend walk-in contract (NO slotId)
    const payload = {
      doctorId: Number(selectedDoctorId),
      therapyId: Number(therapyId),
      date: selectedDate,
      startTime: selectedSlot.startTime,
      endTime: selectedSlot.endTime,
      firstName: firstName.trim(),
      lastName: lastName.trim(),
      notes: "Walk-in booking",
    };

    setBooking(true);
    try {
      await API.post("/appointment/walkin", payload);
      setSuccess("Appointment booked successfully.");
      setSelectedSlot(null);
      setFirstName("");
      setLastName("");

      // Refresh slots after booking
      if (selectedDoctorId && selectedDate && therapyId) {
        const list = await fetchSlots(selectedDoctorId, selectedDate, therapyId);
        setSlots(list);
      }
    } catch (err) {
      console.error(err?.response?.data);
      setError(
        err?.response?.data?.message ||
          err?.response?.data?.title ||
          "Booking failed.",
      );
    } finally {
      setBooking(false);
    }
  };

  const isSlotSelected = (slot) =>
    selectedSlot?.startTime === slot.startTime &&
    selectedSlot?.endTime === slot.endTime;

  return (
    <div className="container-fluid px-0">
      <div className="tc-page-header mb-4">
        <h1 className="tc-page-title">Walk-in booking</h1>
        <p>Choose doctor, therapy, and a free slot from the generated schedule.</p>
      </div>

      {error && (
        <div className="alert alert-danger border-0 shadow-sm" role="alert">
          {error}
        </div>
      )}
      {success && (
        <div className="alert alert-success border-0 shadow-sm" role="alert">
          {success}
        </div>
      )}

      <div className="tc-card mb-4">
        <div className="tc-card-header">Patient &amp; visit details</div>
        <div className="p-4">
          <div className="row g-3">
            <div className="col-md-6">
              <label className="form-label">Select Doctor</label>
              <select
                className="form-select"
                value={selectedDoctorId}
                onChange={(e) => setSelectedDoctorId(e.target.value)}
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
              <label className="form-label">Select Date</label>
              <input
                type="date"
                className="form-control"
                value={selectedDate}
                onChange={(e) => setSelectedDate(e.target.value)}
              />
            </div>

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
              <label className="form-label">First Name</label>
              <input
                type="text"
                className="form-control"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder="First name"
              />
            </div>

            <div className="col-md-6">
              <label className="form-label">Last Name</label>
              <input
                type="text"
                className="form-control"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                placeholder="Last name"
              />
            </div>
          </div>
        </div>
      </div>

      <div className="tc-card">
        <div className="tc-card-header">Available slots</div>
        <div className="p-4">
          {!selectedDoctorId || !therapyId ? (
            <p className="text-secondary mb-0">Select doctor and therapy to load slots.</p>
          ) : loadingSlots ? (
            <p className="mb-0 text-secondary">
              <span className="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true" />
              Loading slots…
            </p>
          ) : slots.length === 0 ? (
            <p className="text-secondary mb-0">No slots for this selection.</p>
          ) : (
            <div className="d-flex flex-wrap gap-2 mb-4">
              {slots.map((slot, idx) => {
                const isBooked = slot.status === "Booked";
                const selected = isSlotSelected(slot);

                // Backend contract has no IDs; key via time range
                const key = `${slot.startTime}-${slot.endTime}-${idx}`;

                return (
                  <button
                    key={key}
                    type="button"
                    className={`btn btn-sm rounded-pill px-3 ${
                      isBooked
                        ? "btn-outline-secondary"
                        : selected
                          ? "btn-primary"
                          : "btn-outline-primary"
                    }`}
                    disabled={isBooked}
                    onClick={() => setSelectedSlot(slot)}
                  >
                    {slot.startTime} – {slot.endTime}
                  </button>
                );
              })}
            </div>
          )}

          <button
            type="button"
            className="btn btn-primary rounded-3 px-4"
            onClick={handleBook}
            disabled={!selectedSlot || booking || !selectedDoctorId}
          >
            {booking ? "Booking…" : "Confirm booking"}
          </button>
        </div>
      </div>
    </div>
  );
};

export default BookAppointment;


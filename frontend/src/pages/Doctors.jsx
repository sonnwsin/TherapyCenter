import { useEffect, useMemo, useState } from "react";
import API from "../api/axios";

const initialFormData = {
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  phoneNumber: "",
  specialization: "",
  bio: "",
  availableDays: "",
  startTime: "",
  endTime: "",
};

const normalizeDoctor = (doctor) => ({
  ...doctor,
  doctorId: doctor.doctorId ?? doctor.id,
  firstName: doctor.firstName ?? "",
  lastName: doctor.lastName ?? "",
  fullName: doctor.fullName ?? `${doctor.firstName ?? ""} ${doctor.lastName ?? ""}`.trim(),
  email: doctor.email ?? "",
  phoneNumber: doctor.phoneNumber ?? "",
  specialization: doctor.specialization ?? "",
  bio: doctor.bio ?? "",
  availableDays: Array.isArray(doctor.availableDays)
    ? doctor.availableDays.join(", ")
    : (doctor.availableDays ?? ""),
  startTime: doctor.startTime ?? "",
  endTime: doctor.endTime ?? "",
});

const createSubmitPayload = (formData, isEditMode) => {
  const payload = {
    firstName: formData.firstName.trim(),
    lastName: formData.lastName.trim(),
    email: formData.email.trim(),
    phoneNumber: formData.phoneNumber.trim(),
    specialization: formData.specialization.trim(),
    bio: formData.bio.trim(),
    availableDays: formData.availableDays.trim(),
    startTime: formData.startTime,
    endTime: formData.endTime,
  };

  if (!isEditMode || formData.password.trim()) {
    payload.password = formData.password;
  }

  return payload;
};

const Doctors = () => {
  const [doctors, setDoctors] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [formData, setFormData] = useState(initialFormData);
  const [showModal, setShowModal] = useState(false);
  const [editId, setEditId] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const isEditMode = useMemo(() => Boolean(editId), [editId]);

  const getDoctors = async () => {
    const response = await API.get("/doctor");
    const rawList = Array.isArray(response.data) ? response.data : [];
    return rawList.map(normalizeDoctor);
  };

  const refreshDoctors = async () => {
    const list = await getDoctors();
    setDoctors(list);
  };

  useEffect(() => {
    const loadInitialDoctors = async () => {
      try {
        setLoading(true);
        setError("");
        const list = await getDoctors();
        setDoctors(list);
      } catch {
        setError("Failed to fetch doctors. Please try again.");
      } finally {
        setLoading(false);
      }
    };

    loadInitialDoctors();
  }, []);

  const openAddModal = () => {
    setFormData(initialFormData);
    setEditId(null);
    setShowModal(true);
  };

  const openEditModal = (doctor) => {
    setFormData({
      firstName: doctor.firstName ?? "",
      lastName: doctor.lastName ?? "",
      email: doctor.email ?? "",
      password: "",
      phoneNumber: doctor.phoneNumber ?? "",
      specialization: doctor.specialization ?? "",
      bio: doctor.bio ?? "",
      availableDays: doctor.availableDays ?? "",
      startTime: doctor.startTime ?? "",
      endTime: doctor.endTime ?? "",
    });
    setEditId(doctor.doctorId);
    setShowModal(true);
  };

  const closeModal = () => {
    setShowModal(false);
    setFormData(initialFormData);
    setEditId(null);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");

    try {
      const payload = createSubmitPayload(formData, isEditMode);

      if (isEditMode) {
        await API.put(`/doctor/${editId}`, payload);
      } else {
        await API.post("/doctor", payload);
      }

      closeModal();
      await refreshDoctors();
    } catch {
      setError(isEditMode ? "Failed to update doctor." : "Failed to create doctor.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (doctorId) => {
    const isConfirmed = window.confirm("Are you sure you want to delete this doctor?");
    if (!isConfirmed) return;

    try {
      setError("");
      await API.delete(`/doctor/${doctorId}`);
      await refreshDoctors();
    } catch {
      setError("Failed to delete doctor.");
    }
  };

  return (
    <div className="container-fluid px-0">
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-body p-4 d-flex flex-wrap align-items-center justify-content-between gap-3">
          <div>
            <h4 className="mb-1 fw-semibold">Doctor Management</h4>
            <p className="text-secondary mb-0">Create, update, and manage doctor profiles.</p>
          </div>
          <button type="button" className="btn btn-primary" onClick={openAddModal}>
            Add Doctor
          </button>
        </div>
      </div>

      {error && (
        <div className="alert alert-danger" role="alert">
          {error}
        </div>
      )}

      <div className="card border-0 shadow-sm">
        <div className="card-body p-0">
          {loading ? (
            <p className="p-4 mb-0">Loading doctors...</p>
          ) : (
            <div className="table-responsive">
              <table className="table mb-0 align-middle">
                <thead className="table-light">
                  <tr>
                    <th>Name</th>
                    <th>Email</th>
                    <th>Specialization</th>
                    <th>Available Days</th>
                    <th>Time</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {doctors.length === 0 ? (
                    <tr>
                      <td colSpan="6" className="text-center text-secondary py-4">
                        No doctors found.
                      </td>
                    </tr>
                  ) : (
                    doctors.map((doctor) => (
                      <tr key={doctor.doctorId}>
                        <td>{doctor.fullName}</td>
                        <td>{doctor.email || "-"}</td>
                        <td>{doctor.specialization || "-"}</td>
                        <td>{doctor.availableDays || "-"}</td>
                        <td>
                          {doctor.startTime || "-"} - {doctor.endTime || "-"}
                        </td>
                        <td className="text-end">
                          <div className="d-inline-flex gap-2">
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => openEditModal(doctor)}
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-danger"
                              onClick={() => handleDelete(doctor.doctorId)}
                            >
                              Delete
                            </button>
                          </div>
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>

      {showModal && (
        <div className="modal d-block" tabIndex="-1" role="dialog">
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <form onSubmit={handleSubmit}>
                <div className="modal-header">
                  <h5 className="modal-title">{isEditMode ? "Edit Doctor" : "Add Doctor"}</h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={closeModal}
                    aria-label="Close"
                    disabled={submitting}
                  />
                </div>

                <div className="modal-body">
                  <div className="row g-3">
                    <div className="col-md-6">
                      <label className="form-label">First Name</label>
                      <input
                        type="text"
                        name="firstName"
                        className="form-control"
                        value={formData.firstName}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-md-6">
                      <label className="form-label">Last Name</label>
                      <input
                        type="text"
                        name="lastName"
                        className="form-control"
                        value={formData.lastName}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-md-6">
                      <label className="form-label">Email</label>
                      <input
                        type="email"
                        name="email"
                        className="form-control"
                        value={formData.email}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-md-6">
                      <label className="form-label">
                        Password {isEditMode ? <span className="text-secondary">(optional)</span> : ""}
                      </label>
                      <input
                        type="password"
                        name="password"
                        className="form-control"
                        value={formData.password}
                        onChange={handleChange}
                        required={!isEditMode}
                      />
                    </div>

                    <div className="col-md-6">
                      <label className="form-label">Phone Number</label>
                      <input
                        type="text"
                        name="phoneNumber"
                        className="form-control"
                        value={formData.phoneNumber}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-md-6">
                      <label className="form-label">Specialization</label>
                      <input
                        type="text"
                        name="specialization"
                        className="form-control"
                        value={formData.specialization}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-12">
                      <label className="form-label">Bio</label>
                      <textarea
                        name="bio"
                        className="form-control"
                        rows="3"
                        value={formData.bio}
                        onChange={handleChange}
                      />
                    </div>

                    <div className="col-md-4">
                      <label className="form-label">Available Days</label>
                      <input
                        type="text"
                        name="availableDays"
                        className="form-control"
                        placeholder="Mon,Tue,Wed"
                        value={formData.availableDays}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-md-4">
                      <label className="form-label">Start Time</label>
                      <input
                        type="time"
                        name="startTime"
                        className="form-control"
                        value={formData.startTime}
                        onChange={handleChange}
                        required
                      />
                    </div>

                    <div className="col-md-4">
                      <label className="form-label">End Time</label>
                      <input
                        type="time"
                        name="endTime"
                        className="form-control"
                        value={formData.endTime}
                        onChange={handleChange}
                        required
                      />
                    </div>
                  </div>
                </div>

                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-outline-secondary"
                    onClick={closeModal}
                    disabled={submitting}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={submitting}>
                    {submitting ? "Saving..." : isEditMode ? "Update Doctor" : "Create Doctor"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
      {showModal && <div className="modal-backdrop fade show" onClick={closeModal} />}
    </div>
  );
};

export default Doctors;

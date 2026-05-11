import { useEffect, useMemo, useState } from "react";
import API from "../api/axios";

const initialFormData = {
  name: "",
  description: "",
  durationMinutes: "",
  cost: "",
};

const normalizeTherapy = (therapy) => ({
  ...therapy,
  therapyId: therapy.therapyId ?? therapy.id,
  name: therapy.name ?? "",
  description: therapy.description ?? "",
  durationMinutes: therapy.durationMinutes ?? "",
  cost: therapy.cost ?? "",
});

const createSubmitPayload = (formData) => ({
  name: formData.name.trim(),
  description: formData.description.trim(),
  durationMinutes: Number(formData.durationMinutes),
  cost: Number(formData.cost),
});

const Therapies = () => {
  const [therapies, setTherapies] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [formData, setFormData] = useState(initialFormData);
  const [showModal, setShowModal] = useState(false);
  const [editId, setEditId] = useState(null);
  const [submitting, setSubmitting] = useState(false);

  const isEditMode = useMemo(() => Boolean(editId), [editId]);

  const getTherapies = async () => {
    const response = await API.get("/therapy");
    const rawList = Array.isArray(response.data) ? response.data : [];
    return rawList.map(normalizeTherapy);
  };

  const refreshTherapies = async () => {
    const list = await getTherapies();
    setTherapies(list);
  };

  useEffect(() => {
    const loadInitialTherapies = async () => {
      try {
        setLoading(true);
        setError("");
        const list = await getTherapies();
        setTherapies(list);
      } catch {
        setError("Failed to fetch therapies. Please try again.");
      } finally {
        setLoading(false);
      }
    };

    loadInitialTherapies();
  }, []);

  const openAddModal = () => {
    setFormData(initialFormData);
    setEditId(null);
    setShowModal(true);
  };

  const openEditModal = (therapy) => {
    setFormData({
      name: therapy.name ?? "",
      description: therapy.description ?? "",
      durationMinutes: therapy.durationMinutes ?? "",
      cost: therapy.cost ?? "",
    });
    setEditId(therapy.therapyId);
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
      const payload = createSubmitPayload(formData);

      if (isEditMode) {
        await API.put(`/therapy/${editId}`, payload);
      } else {
        await API.post("/therapy", payload);
      }

      closeModal();
      await refreshTherapies();
    } catch {
      setError(isEditMode ? "Failed to update therapy." : "Failed to create therapy.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (therapyId) => {
    const isConfirmed = window.confirm("Are you sure you want to delete this therapy?");
    if (!isConfirmed) return;

    try {
      setError("");
      await API.delete(`/therapy/${therapyId}`);
      await refreshTherapies();
    } catch {
      setError("Failed to delete therapy.");
    }
  };

  return (
    <div className="container-fluid px-0">
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-body p-4 d-flex flex-wrap align-items-center justify-content-between gap-3">
          <div>
            <h4 className="mb-1 fw-semibold">Therapy Management</h4>
            <p className="text-secondary mb-0">Create, update, and remove therapy offerings.</p>
          </div>
          <button type="button" className="btn btn-primary" onClick={openAddModal}>
            Add Therapy
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
            <p className="p-4 mb-0">Loading therapies...</p>
          ) : (
            <div className="table-responsive">
              <table className="table mb-0 align-middle">
                <thead className="table-light">
                  <tr>
                    <th>Name</th>
                    <th>Description</th>
                    <th>Duration</th>
                    <th>Cost</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {therapies.length === 0 ? (
                    <tr>
                      <td colSpan="5" className="text-center text-secondary py-4">
                        No therapies found.
                      </td>
                    </tr>
                  ) : (
                    therapies.map((therapy) => (
                      <tr key={therapy.therapyId}>
                        <td>{therapy.name}</td>
                        <td>{therapy.description || "-"}</td>
                        <td>{therapy.durationMinutes} min</td>
                        <td>{therapy.cost}</td>
                        <td className="text-end">
                          <div className="d-inline-flex gap-2">
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => openEditModal(therapy)}
                            >
                              Edit
                            </button>
                            <button
                              type="button"
                              className="btn btn-sm btn-outline-danger"
                              onClick={() => handleDelete(therapy.therapyId)}
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
          <div className="modal-dialog">
            <div className="modal-content">
              <form onSubmit={handleSubmit}>
                <div className="modal-header">
                  <h5 className="modal-title">{isEditMode ? "Edit Therapy" : "Add Therapy"}</h5>
                  <button
                    type="button"
                    className="btn-close"
                    onClick={closeModal}
                    aria-label="Close"
                    disabled={submitting}
                  />
                </div>

                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Name</label>
                    <input
                      type="text"
                      name="name"
                      className="form-control"
                      value={formData.name}
                      onChange={handleChange}
                      required
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Description</label>
                    <textarea
                      name="description"
                      className="form-control"
                      rows="3"
                      value={formData.description}
                      onChange={handleChange}
                    />
                  </div>

                  <div className="mb-3">
                    <label className="form-label">Duration Minutes</label>
                    <input
                      type="number"
                      name="durationMinutes"
                      className="form-control"
                      value={formData.durationMinutes}
                      onChange={handleChange}
                      min="1"
                      required
                    />
                  </div>

                  <div className="mb-0">
                    <label className="form-label">Cost</label>
                    <input
                      type="number"
                      name="cost"
                      className="form-control"
                      value={formData.cost}
                      onChange={handleChange}
                      min="0"
                      step="0.01"
                      required
                    />
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
                    {submitting ? "Saving..." : isEditMode ? "Update Therapy" : "Create Therapy"}
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

export default Therapies;

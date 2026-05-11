import { useEffect, useMemo, useState } from "react";
import API from "../api/axios";
import "./Auth.css";

const initialFormData = {
  firstName: "",
  lastName: "",
  email: "",
  password: "",
  phoneNumber: "",
};

const normalizeReceptionist = (user) => ({
  ...user,
  id: user.id ?? user.userId,
  firstName: user.firstName ?? "",
  lastName: user.lastName ?? "",
  email: user.email ?? "",
  phoneNumber: user.phoneNumber ?? "",
});

const Receptionists = () => {
  const [receptionists, setReceptionists] = useState([]);
  const [formData, setFormData] = useState(initialFormData);
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [editId, setEditId] = useState(null);
  const [showModal, setShowModal] = useState(false);

  const isEditMode = useMemo(() => Boolean(editId), [editId]);

  const getReceptionists = async () => {
    const response = await API.get("/users/receptionists");
    const rawList = Array.isArray(response.data) ? response.data : [];
    return rawList.map(normalizeReceptionist);
  };

  const refreshReceptionists = async () => {
    const list = await getReceptionists();
    setReceptionists(list);
  };

  useEffect(() => {
    const loadReceptionists = async () => {
      try {
        setLoading(true);
        setError("");
        const list = await getReceptionists();
        setReceptionists(list);
      } catch {
        setError("Failed to load receptionists.");
      } finally {
        setLoading(false);
      }
    };

    loadReceptionists();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const resetForm = () => {
    setFormData(initialFormData);
    setEditId(null);
  };

  const openAddModal = () => {
    setError("");
    setSuccess("");
    setEditId(null);
    setFormData(initialFormData);
    setShowModal(true);
  };

  const openEditModal = (user) => {
    setError("");
    setSuccess("");
    setEditId(user.id);
    setFormData({
      firstName: user.firstName ?? "",
      lastName: user.lastName ?? "",
      email: user.email ?? "",
      password: "",
      phoneNumber: user.phoneNumber ?? "",
    });
    setShowModal(true);
  };

  const closeModal = () => {
    if (submitting) return;
    setShowModal(false);
    resetForm();
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    setSuccess("");

    try {
      if (isEditMode) {
        await API.put(`/users/${editId}`, {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber,
          role: "Receptionist", // 🔥 IMPORTANT
          password: formData.password || "", // 🔥 ALWAYS SEND
        });

        setSuccess("Receptionist updated successfully.");
      } else {
        await API.post("/auth/register", {
          firstName: formData.firstName,
          lastName: formData.lastName,
          email: formData.email,
          phoneNumber: formData.phoneNumber,
          password: formData.password,
          role: "Receptionist",
        });

        setSuccess("Receptionist created successfully.");
      }

      closeModal();
      await refreshReceptionists();
    } catch (err) {
      console.log(err); // 🔥 debug help
      setError(isEditMode ? "Failed to update receptionist." : "Failed to create receptionist.");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Are you sure you want to delete this receptionist?")) return;

    try {
      await API.delete(`/users/${id}`);
      await refreshReceptionists();
    } catch {
      setError("Failed to delete receptionist.");
    }
  };

  return (
    <div className="container-fluid px-0">
      <div className="card border-0 shadow-sm mb-4">
        <div className="card-body p-4 d-flex justify-content-between">
          <div>
            <h4 className="mb-1 fw-semibold">Receptionist Management</h4>
            <p className="text-secondary mb-0">Create and maintain receptionist accounts.</p>
          </div>
          <button className="btn btn-primary" onClick={openAddModal}>
            Add Receptionist
          </button>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      <div className="card border-0 shadow-sm">
        <div className="card-body p-0">
          {loading ? (
            <p className="p-4">Loading...</p>
          ) : (
            <table className="table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th className="text-end">Actions</th>
                </tr>
              </thead>
              <tbody>
                {receptionists.map((u) => (
                  <tr key={u.id}>
                    <td>{u.firstName} {u.lastName}</td>
                    <td>{u.email}</td>
                    <td>{u.phoneNumber || "-"}</td>
                    <td className="text-end">
                      <button className="btn btn-sm btn-outline-primary me-2" onClick={() => openEditModal(u)}>
                        Edit
                      </button>
                      <button className="btn btn-sm btn-outline-danger" onClick={() => handleDelete(u.id)}>
                        Delete
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>
      </div>

      {showModal && (
        <div className="modal d-block">
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <form onSubmit={handleSubmit}>
                <div className="modal-header">
                  <h5>{isEditMode ? "Edit Receptionist" : "Add Receptionist"}</h5>
                  <button className="btn-close" onClick={closeModal}></button>
                </div>

                <div className="modal-body row g-3">
                  <input className="form-control" name="firstName" placeholder="First Name" value={formData.firstName} onChange={handleChange} required />
                  <input className="form-control" name="lastName" placeholder="Last Name" value={formData.lastName} onChange={handleChange} required />
                  <input className="form-control" name="email" placeholder="Email" value={formData.email} onChange={handleChange} required />
                  <input className="form-control" name="phoneNumber" placeholder="Phone" value={formData.phoneNumber} onChange={handleChange} required />
                  <input className="form-control" type="password" name="password" placeholder="Password" value={formData.password} onChange={handleChange} required={!isEditMode} />
                </div>

                <div className="modal-footer">
                  <button className="btn btn-secondary" onClick={closeModal}>Cancel</button>
                  <button className="btn btn-primary">
                    {isEditMode ? "Update" : "Create"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Receptionists;
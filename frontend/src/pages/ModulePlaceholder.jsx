const ModulePlaceholder = ({ title, description }) => {
  return (
    <div className="card border-0 shadow-sm">
      <div className="card-body p-4">
        <h5 className="fw-semibold mb-2">{title}</h5>
        <p className="text-secondary mb-0">{description}</p>
      </div>
    </div>
  );
};

export default ModulePlaceholder;

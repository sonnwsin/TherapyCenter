/** Small dashboard metric card — Bootstrap + theme.css */
const StatTile = ({ iconClass, variant = "teal", label, value }) => (
  <div className="tc-stat-tile">
    <div className={`tc-stat-tile-icon ${variant}`}>
      <i className={`bi ${iconClass}`} aria-hidden="true" />
    </div>
    <div className="tc-stat-tile-label">{label}</div>
    <div className="tc-stat-tile-value">{value}</div>
  </div>
);

export default StatTile;

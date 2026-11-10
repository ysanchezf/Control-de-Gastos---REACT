export default function Card({
  title,
  children,
  className = "",
  eyebrow,
  actions,
  description,
}) {
  return (
    <section className={`panel ${className}`}>
      <div className="section-heading">
        <div>
          {eyebrow && <span className="eyebrow">{eyebrow}</span>}
          <h2>{title}</h2>
          {description && <p>{description}</p>}
        </div>
        {actions}
      </div>
      {children}
    </section>
  );
}

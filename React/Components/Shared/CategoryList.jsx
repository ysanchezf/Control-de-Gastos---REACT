import { money } from "./Summary";
export default function CategoryList({
  categories,
  expenses,
  busy,
  onEdit,
  onDelete,
}) {
  return (
    <section className="category-grid" aria-label="Tus categorías">
      {!categories.length && (
        <p className="empty">
          Aún no tienes categorías. Crea la primera para empezar.
        </p>
      )}
      {categories.map((c) => {
        const items = expenses.filter((g) => g.categoriaId === c.id);
        return (
          <article className="category-card" key={c.id}>
            <span className="feature-icon peach" aria-hidden="true">
              ▦
            </span>
            <h3>{c.nombre}</h3>
            <p>{c.descripcion}</p>
            <div>
              <span className="pill">{items.length} gastos</span>
              <strong>{money(items.reduce((n, g) => n + g.monto, 0))}</strong>
            </div>
            <div className="row-actions">
              <button
                className="text-button"
                disabled={busy}
                aria-label={`Editar ${c.nombre}`}
                onClick={() => onEdit(c)}
              >
                Editar
              </button>
              <button
                className="delete"
                disabled={busy}
                aria-label={`Eliminar ${c.nombre}`}
                onClick={() => onDelete(c)}
              >
                Eliminar
              </button>
            </div>
          </article>
        );
      })}
    </section>
  );
}

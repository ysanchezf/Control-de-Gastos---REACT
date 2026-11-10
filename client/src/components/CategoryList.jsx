import Card from "./Card";
import { money } from "./Summary";
export default function CategoryList({
  categories,
  expenses,
  busy,
  onEdit,
  onDelete,
}) {
  return (
    <Card title="Cada cosa en su lugar">
      {!categories.length ? (
        <p>Todavía no hay categorías. Crea la primera para comenzar.</p>
      ) : (
        <div className="category-grid">
          {categories.map((c) => {
            const items = expenses.filter((g) => g.categoriaId === c.id);
            return (
              <article className="category" key={c.id}>
                <span className="category-icon" aria-hidden="true">
                  ↗
                </span>
                <h3>{c.nombre}</h3>
                <p>{c.descripcion}</p>
                <small>
                  {items.length} movimientos ·{" "}
                  {money(items.reduce((n, g) => n + g.monto, 0))}
                </small>
                <div className="row-actions">
                  <button
                    className="text-button"
                    disabled={busy}
                    onClick={() => onEdit(c)}
                  >
                    Editar<span className="sr-only"> {c.nombre}</span>
                  </button>
                  <button
                    className="text-button danger"
                    disabled={busy}
                    onClick={() => onDelete(c)}
                  >
                    Eliminar<span className="sr-only"> {c.nombre}</span>
                  </button>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </Card>
  );
}

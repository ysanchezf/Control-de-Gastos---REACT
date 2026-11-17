import { money } from "./Summary";
export default function ExpenseTable({
  expenses,
  categories,
  busy,
  onEdit,
  onDelete,
  readOnly = false,
}) {
  return (
    <div className="table-wrap">
      {!expenses.length ? (
        <div className="empty">
          <span aria-hidden="true">→</span>
          <h3>Todavía no hay gastos aquí</h3>
          <p>
            Registra un gasto o cambia la búsqueda para ver tus movimientos.
          </p>
        </div>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Descripción</th>
              <th>Categoría</th>
              <th>Fecha</th>
              <th className="amount">Monto</th>
              {!readOnly && (
                <th>
                  <span className="sr-only">Acciones</span>
                </th>
              )}
            </tr>
          </thead>
          <tbody>
            {expenses.map((g) => (
              <tr key={g.id}>
                <td>
                  <span className="row-icon" aria-hidden="true">
                    →
                  </span>
                  <strong>{g.descripcion}</strong>
                </td>
                <td>
                  <span className="pill">
                    {categories.find((c) => c.id === g.categoriaId)?.nombre ||
                      "Sin categoría"}
                  </span>
                </td>
                <td className="muted">
                  {new Date(
                    g.fecha.slice(0, 10) + "T12:00:00",
                  ).toLocaleDateString("es-DO", {
                    day: "2-digit",
                    month: "short",
                    year: "numeric",
                  })}
                </td>
                <td className="amount money">{money(g.monto)}</td>
                {!readOnly && (
                  <td>
                    <div className="row-actions">
                      <button
                        className="text-button"
                        disabled={busy}
                        aria-label={`Editar ${g.descripcion}`}
                        onClick={() => onEdit(g)}
                      >
                        Editar
                      </button>
                      <button
                        className="delete"
                        disabled={busy}
                        aria-label={`Eliminar ${g.descripcion}`}
                        onClick={() => onDelete(g)}
                      >
                        Eliminar
                      </button>
                    </div>
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

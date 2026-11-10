import { money } from "./Summary";
import Card from "./Card";
export default function ExpenseTable({
  expenses,
  categories,
  busy,
  onEdit,
  onDelete,
}) {
  return (
    <Card title="Tus movimientos">
      {!expenses.length ? (
        <p>Aquí aparecerán tus gastos. Empieza con el primero.</p>
      ) : (
        <div className="table-wrap">
          <table>
            <thead>
              <tr>
                <th>Descripción</th>
                <th>Categoría</th>
                <th>Fecha</th>
                <th>Monto</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {[...expenses]
                .sort((a, b) => b.fecha.localeCompare(a.fecha))
                .map((g) => (
                  <tr key={g.id}>
                    <td>{g.descripcion}</td>
                    <td>
                      {categories.find((c) => c.id === g.categoriaId)?.nombre ||
                        "Sin categoría"}
                    </td>
                    <td>
                      {new Date(
                        g.fecha.slice(0, 10) + "T12:00:00",
                      ).toLocaleDateString("es-DO")}
                    </td>
                    <td className="amount">{money(g.monto)}</td>
                    <td>
                      <div className="row-actions">
                        <button
                          className="text-button"
                          disabled={busy}
                          onClick={() => onEdit(g)}
                        >
                          Editar
                          <span className="sr-only"> {g.descripcion}</span>
                        </button>
                        <button
                          className="text-button danger"
                          disabled={busy}
                          onClick={() => onDelete(g)}
                        >
                          Eliminar
                          <span className="sr-only"> {g.descripcion}</span>
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        </div>
      )}
    </Card>
  );
}

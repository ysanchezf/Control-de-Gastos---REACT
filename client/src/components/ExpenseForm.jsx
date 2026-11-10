import { useState } from "react";
import { Link } from "react-router-dom";
import Card from "./Card";
const today = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}-${String(d.getDate()).padStart(2, "0")}`;
};
export default function ExpenseForm({
  categories,
  editing,
  busy,
  onSave,
  onCancel,
}) {
  const [form, setForm] = useState(() =>
    editing
      ? {
          descripcion: editing.descripcion,
          monto: editing.monto,
          fecha: editing.fecha.slice(0, 10),
          categoriaId: editing.categoriaId,
        }
      : { descripcion: "", monto: "", fecha: today(), categoriaId: "" },
  );
  const change = (e) => setForm({ ...form, [e.target.name]: e.target.value });
  return (
    <Card
      className="form-panel"
      eyebrow="DETALLES DEL MOVIMIENTO"
      title={editing ? "Editar gasto" : "Registrar gasto"}
    >
      <form
        onSubmit={(e) => {
          e.preventDefault();
          onSave({
            ...form,
            monto: Number(form.monto),
            categoriaId: Number(form.categoriaId),
          });
        }}
      >
        <fieldset disabled={busy}>
          <div className="form-grid">
            <label>
              Descripción
              <input
                name="descripcion"
                placeholder="¿En qué gastaste?"
                required
                minLength="2"
                maxLength="150"
                value={form.descripcion}
                onChange={change}
              />
            </label>
            <label>
              Monto (RD$)
              <input
                name="monto"
                type="number"
                required
                min="0.01"
                max="999999999"
                step="0.01"
                value={form.monto}
                onChange={change}
              />
            </label>
            <label>
              Fecha
              <input
                name="fecha"
                type="date"
                required
                value={form.fecha}
                onChange={change}
              />
            </label>
            <div className="select-field">
              <label htmlFor="expense-category">Categoría</label>
              <select
                id="expense-category"
                name="categoriaId"
                required
                value={form.categoriaId}
                onChange={change}
              >
                <option value="">Selecciona una categoría</option>
                {categories.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.nombre}
                  </option>
                ))}
              </select>
            </div>
          </div>
          {!categories.length && (
            <p className="muted">
              Primero <Link to="/categorias">crea una categoría</Link> para
              organizar tu gasto.
            </p>
          )}
          <div className="form-actions">
            <span className="muted">
              Los montos se registran en pesos dominicanos.
            </span>
            {editing && (
              <button type="button" className="button quiet" onClick={onCancel}>
                Cancelar edición
              </button>
            )}
            <button
              className="button dark"
              disabled={busy || !categories.length}
            >
              {busy
                ? "Guardando…"
                : editing
                  ? "Guardar cambios"
                  : "Guardar gasto"}{" "}
              <span aria-hidden="true">→</span>
            </button>
          </div>
        </fieldset>
      </form>
    </Card>
  );
}

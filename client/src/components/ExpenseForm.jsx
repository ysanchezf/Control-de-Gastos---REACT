import { useState } from "react";
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
    <Card title={editing ? "Editar gasto" : "Anotar un gasto"}>
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
        <fieldset disabled={busy || !categories.length}>
          <label>
            Descripción
            <input
              name="descripcion"
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
          <button>
            {busy
              ? "Guardando…"
              : editing
                ? "Guardar cambios"
                : "Agregar gasto"}
          </button>
        </fieldset>
        {editing && (
          <button
            type="button"
            className="text-button"
            disabled={busy}
            onClick={onCancel}
          >
            Cancelar edición
          </button>
        )}
        {!categories.length && (
          <p>Crea primero una categoría para registrar gastos.</p>
        )}
      </form>
    </Card>
  );
}

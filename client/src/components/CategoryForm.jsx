import { useState } from "react";
import Card from "./Card";
export default function CategoryForm({ editing, busy, onSave, onCancel }) {
  const [form, setForm] = useState({
    nombre: editing?.nombre || "",
    descripcion: editing?.descripcion || "",
  });
  return (
    <Card
      className="form-panel"
      eyebrow="DETALLES DE LA CATEGORÍA"
      title={editing ? "Editar categoría" : "Nueva categoría"}
    >
      <form
        onSubmit={(e) => {
          e.preventDefault();
          onSave(form);
        }}
      >
        <fieldset disabled={busy}>
          <div className="form-grid">
            <label>
              Nombre
              <input
                placeholder="Ej. Alimentación"
                required
                minLength="2"
                maxLength="80"
                value={form.nombre}
                onChange={(e) => setForm({ ...form, nombre: e.target.value })}
              />
            </label>
            <label>
              Descripción
              <input
                placeholder="¿Qué gastos agrupa?"
                required
                maxLength="250"
                value={form.descripcion}
                onChange={(e) =>
                  setForm({ ...form, descripcion: e.target.value })
                }
              />
            </label>
          </div>
          <div className="form-actions">
            <span className="muted">
              Elige un nombre que reconozcas fácilmente.
            </span>
            {editing && (
              <button className="button quiet" type="button" onClick={onCancel}>
                Cancelar edición
              </button>
            )}
            <button className="button dark">
              {busy
                ? "Guardando…"
                : editing
                  ? "Guardar cambios"
                  : "Crear categoría"}{" "}
              <span aria-hidden="true">＋</span>
            </button>
          </div>
        </fieldset>
      </form>
    </Card>
  );
}

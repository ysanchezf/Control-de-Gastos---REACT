import { useState } from "react";
import Card from "./Card";
export default function CategoryForm({ editing, busy, onSave, onCancel }) {
  const [form, setForm] = useState({
    nombre: editing?.nombre || "",
    descripcion: editing?.descripcion || "",
  });
  return (
    <Card title={editing ? "Editar categoría" : "Una categoría nueva"}>
      <form
        onSubmit={(e) => {
          e.preventDefault();
          onSave(form);
        }}
      >
        <fieldset disabled={busy}>
          <label>
            Nombre
            <input
              required
              minLength="2"
              maxLength="80"
              value={form.nombre}
              onChange={(e) => setForm({ ...form, nombre: e.target.value })}
            />
          </label>
          <label>
            Descripción
            <textarea
              required
              maxLength="250"
              value={form.descripcion}
              onChange={(e) =>
                setForm({ ...form, descripcion: e.target.value })
              }
            />
          </label>
          <button>
            {busy
              ? "Guardando…"
              : editing
                ? "Guardar cambios"
                : "Crear categoría"}
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
      </form>
    </Card>
  );
}

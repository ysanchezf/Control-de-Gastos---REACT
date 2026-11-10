import { useState } from "react";
import CategoryForm from "../components/CategoryForm";
import CategoryList from "../components/CategoryList";
import Feedback from "../components/Feedback";
import { api } from "../services/api";
export default function Categories({ workspace: w }) {
  const [editing, setEditing] = useState(null),
    [version, setVersion] = useState(0),
    [busy, setBusy] = useState(false),
    [error, setError] = useState(""),
    [message, setMessage] = useState("");
  function reset() {
    setEditing(null);
    setVersion((v) => v + 1);
  }
  async function save(value) {
    setBusy(true);
    setError("");
    setMessage("");
    try {
      await api.saveCategoria(value, editing?.id);
      reset();
      await w.reload();
      setMessage("Categoría guardada.");
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  async function remove(c) {
    if (!window.confirm(`¿Eliminar «${c.nombre}»?`)) return;
    setBusy(true);
    setError("");
    setMessage("");
    try {
      await api.deleteCategoria(c.id);
      if (editing?.id === c.id) reset();
      await w.reload();
      setMessage("Categoría eliminada.");
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  function edit(c) {
    setEditing(c);
    setVersion((v) => v + 1);
    document
      .getElementById("category-editor")
      .scrollIntoView({ behavior: "smooth" });
  }
  return (
    <>
      <div className="page-heading">
        <p className="eyebrow">ORDEN A TU MANERA</p>
        <h1>Categorías</h1>
        <p>Alimentación, transporte, un gusto… tú eliges cómo organizarte.</p>
      </div>
      <Feedback {...w} onRetry={w.reload} />
      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      <p role="status">{message}</p>
      {!w.loading && !w.error && (
        <div className="two-column category-layout">
          <div id="category-editor">
            <CategoryForm
              key={version}
              editing={editing}
              busy={busy}
              onSave={save}
              onCancel={reset}
            />
          </div>
          <CategoryList
            categories={w.categorias}
            expenses={w.gastos}
            busy={busy}
            onEdit={edit}
            onDelete={remove}
          />
        </div>
      )}
    </>
  );
}

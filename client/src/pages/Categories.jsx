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
        <span className="eyebrow">MI ORGANIZACIÓN</span>
        <h1>Categorías</h1>
        <p>
          Crea grupos para tus gastos: comida, transporte, casa o lo que
          necesites.
        </p>
      </div>
      <Feedback {...w} onRetry={w.reload} />
      {error && (
        <p className="notice error" role="alert">
          {error}
        </p>
      )}
      {message && (
        <p className="notice success" role="status">
          {message}
        </p>
      )}
      {!w.loading && !w.error && (
        <>
          <div id="category-editor">
            <CategoryForm
              key={version}
              editing={editing}
              busy={busy}
              onSave={save}
              onCancel={reset}
            />
          </div>
          <div className="section-heading">
            <div>
              <h2>Tus categorías</h2>
              <p>Puedes cambiar el nombre y la descripción cuando quieras.</p>
            </div>
            <span className="pill">{w.categorias.length} categorías</span>
          </div>
          <CategoryList
            categories={w.categorias}
            expenses={w.gastos}
            busy={busy}
            onEdit={edit}
            onDelete={remove}
          />
        </>
      )}
    </>
  );
}

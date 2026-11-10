import { useState } from "react";
import ExpenseForm from "../components/ExpenseForm";
import ExpenseTable from "../components/ExpenseTable";
import Feedback from "../components/Feedback";
import { api } from "../services/api";
export default function Expenses({ workspace: w }) {
  const [editing, setEditing] = useState(null),
    [version, setVersion] = useState(0),
    [busy, setBusy] = useState(false),
    [message, setMessage] = useState(""),
    [error, setError] = useState(""),
    [search, setSearch] = useState("");
  const reset = () => {
    setEditing(null);
    setVersion((v) => v + 1);
  };
  async function save(value) {
    setBusy(true);
    setError("");
    setMessage("");
    try {
      await api.saveGasto(value, editing?.id);
      reset();
      await w.reload();
      setMessage("Gasto guardado.");
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  async function remove(g) {
    if (!window.confirm(`¿Eliminar «${g.descripcion}»?`)) return;
    setBusy(true);
    setError("");
    setMessage("");
    try {
      await api.deleteGasto(g.id);
      if (editing?.id === g.id) reset();
      await w.reload();
      setMessage("Gasto eliminado.");
    } catch (e) {
      setError(e.message);
    } finally {
      setBusy(false);
    }
  }
  function edit(g) {
    setEditing(g);
    setVersion((v) => v + 1);
    document
      .getElementById("expense-editor")
      .scrollIntoView({ behavior: "smooth" });
  }
  return (
    <>
      <div className="page-heading">
        <p className="eyebrow">PASO A PASO</p>
        <h1>Gastos</h1>
        <p>Lo que anotas hoy te ayuda a decidir mañana.</p>
      </div>
      <Feedback {...w} onRetry={w.reload} />
      {error && (
        <p className="error" role="alert">
          {error}
        </p>
      )}
      <p role="status">{message}</p>
      {!w.loading && !w.error && (
        <>
          <div id="expense-editor">
            <ExpenseForm
              key={version}
              categories={w.categorias}
              editing={editing}
              busy={busy}
              onSave={save}
              onCancel={reset}
            />
          </div>
          <label className="search">
            Buscar por descripción
            <input
              type="search"
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </label>
          <ExpenseTable
            expenses={w.gastos.filter((g) =>
              g.descripcion
                .toLocaleLowerCase()
                .includes(search.toLocaleLowerCase()),
            )}
            categories={w.categorias}
            busy={busy}
            onEdit={edit}
            onDelete={remove}
          />
        </>
      )}
    </>
  );
}

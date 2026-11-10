import { useState } from "react";
import Summary from "../components/Summary";
import Card from "../components/Card";
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
  const filtered = w.gastos
    .filter((g) =>
      g.descripcion.toLocaleLowerCase().includes(search.toLocaleLowerCase()),
    )
    .sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id);
  return (
    <>
      <div className="page-heading">
        <span className="eyebrow">MIS MOVIMIENTOS</span>
        <h1>Gastos</h1>
        <p>Lleva el registro de lo que pagas y corrige lo que necesites.</p>
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
          <Summary
            total={w.total}
            count={w.gastos.length}
            categories={w.categorias.length}
          />
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
          <Card
            title="Todos tus movimientos"
            description={`${filtered.length} resultados`}
            actions={
              <label className="search">
                <span className="sr-only">Buscar gastos</span>
                <input
                  aria-label="Buscar gastos"
                  placeholder="Buscar un gasto…"
                  type="search"
                  value={search}
                  onChange={(e) => setSearch(e.target.value)}
                />
              </label>
            }
          >
            <ExpenseTable
              expenses={filtered}
              categories={w.categorias}
              busy={busy}
              onEdit={edit}
              onDelete={remove}
            />
          </Card>
        </>
      )}
    </>
  );
}

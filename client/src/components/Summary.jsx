import { useState } from "react";
export const money = (n) =>
  Number(n).toLocaleString("es-DO", { style: "currency", currency: "DOP" });
export default function Summary({
  total,
  count,
  categories,
  allowVisibility = false,
}) {
  const [visible, setVisible] = useState(true);
  return (
    <section className="summary-grid" aria-label="Resumen de gastos">
      <article className="summary-card">
        <span>
          Total registrado{" "}
          {allowVisibility ? (
            <button
              type="button"
              className="visibility-button"
              aria-expanded={visible}
              aria-label={visible ? "Ocultar importe" : "Mostrar importe"}
              onClick={() => setVisible(!visible)}
            >
              {visible ? "Ocultar" : "Mostrar"}
            </button>
          ) : (
            <span className="muted" aria-hidden="true">
              →
            </span>
          )}
        </span>
        <strong>{visible ? money(total) : "••••••"}</strong>
        <small>Todos tus gastos, en una cifra</small>
      </article>
      <article className="summary-card">
        <span>
          Movimientos{" "}
          <span className="muted" aria-hidden="true">
            ≋
          </span>
        </span>
        <strong>
          {count} <small>gastos</small>
        </strong>
        <small>Un registro para cada decisión</small>
      </article>
      <article className="summary-card">
        <span>
          Categorías{" "}
          <span className="muted" aria-hidden="true">
            ▦
          </span>
        </span>
        <strong>
          {categories} <small>categorías</small>
        </strong>
        <small>Tu organización empieza aquí</small>
      </article>
    </section>
  );
}

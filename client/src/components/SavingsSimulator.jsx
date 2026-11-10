import { useState } from "react";
import Card from "./Card";
export default function SavingsSimulator() {
  const [months, setMonths] = useState(3);
  const [weekly, setWeekly] = useState(false);
  const [amount, setAmount] = useState(500);
  const total = amount * months * (weekly ? 4 : 1);
  return (
    <Card title="Una meta pequeña también cuenta" className="simulator">
      <p>
        Explora cuánto podrías guardar. Esta simulación no modifica tus gastos.
      </p>
      <label>
        Aporte en RD$
        <input
          type="number"
          min="0"
          max="1000000"
          value={amount}
          onChange={(e) =>
            setAmount(Math.max(0, Math.min(1000000, Number(e.target.value))))
          }
        />
      </label>
      <button
        className="secondary"
        aria-pressed={weekly}
        onClick={() => setWeekly(!weekly)}
      >
        Frecuencia: {weekly ? "semanal" : "mensual"}
      </button>
      <p>Meses para tu meta</p>
      <div className="counter">
        <button
          aria-label="Disminuir meses"
          disabled={months === 1}
          onClick={() => setMonths(months - 1)}
        >
          −
        </button>
        <output aria-live="polite">{months}</output>
        <button
          aria-label="Aumentar meses"
          disabled={months === 24}
          onClick={() => setMonths(months + 1)}
        >
          +
        </button>
      </div>
      <p className="large">
        {total.toLocaleString("es-DO", { style: "currency", currency: "DOP" })}
      </p>
      <small>
        Estimación sin intereses. Para el aporte semanal usamos 4 semanas por
        mes.
      </small>
    </Card>
  );
}

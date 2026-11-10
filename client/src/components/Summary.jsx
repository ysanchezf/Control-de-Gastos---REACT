import { useState } from "react";
import Card from "./Card";
export const money = (n) =>
  Number(n).toLocaleString("es-DO", { style: "currency", currency: "DOP" });
export default function Summary({ total, count, categories }) {
  const [visible, setVisible] = useState(true);
  return (
    <Card title="Tu panorama">
      <div className="summary">
        <div>
          <small>Total registrado</small>
          <p className="large">{visible ? money(total) : "••••••"}</p>
        </div>
        <div>
          <small>Movimientos</small>
          <p className="large">{count}</p>
        </div>
        <div>
          <small>Categorías</small>
          <p className="large">{categories}</p>
        </div>
      </div>
      <button
        className="text-button"
        aria-expanded={visible}
        onClick={() => setVisible(!visible)}
      >
        {visible ? "Ocultar importe" : "Mostrar importe"}
      </button>
    </Card>
  );
}

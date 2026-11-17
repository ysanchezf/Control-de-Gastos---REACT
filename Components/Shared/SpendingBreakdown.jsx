import Card from "./Card";
import { money } from "./Summary";
export default function SpendingBreakdown({ expenses, categories }) {
  const total = expenses.reduce((sum, g) => sum + g.monto, 0);
  const groups = Object.entries(
    expenses.reduce((result, g) => {
      result[g.categoriaId] = (result[g.categoriaId] || 0) + g.monto;
      return result;
    }, {}),
  )
    .sort((a, b) => b[1] - a[1])
    .slice(0, 5);
  return (
    <Card
      title="¿En qué se va el dinero?"
      description="Distribución de todos tus gastos registrados."
      className="breakdown"
    >
      {!expenses.length && (
        <p className="empty">
          Cuando registres gastos, aquí verás cómo se reparten.
        </p>
      )}
      {groups.map(([id, amount]) => (
        <div className="budget-row" key={id}>
          <div>
            <span>
              {categories.find((c) => c.id === Number(id))?.nombre ||
                "Sin categoría"}
            </span>
            <strong>{money(amount)}</strong>
          </div>
          <progress
            max="100"
            value={total ? (amount / total) * 100 : 0}
            aria-label={`${Math.round(total ? (amount / total) * 100 : 0)}% del total`}
          />
        </div>
      ))}
    </Card>
  );
}

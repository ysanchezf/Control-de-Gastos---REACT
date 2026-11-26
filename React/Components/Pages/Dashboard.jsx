import Summary from "../Shared/Summary";
import Card from "../Shared/Card";
import ExpenseTable from "../Shared/ExpenseTable";
import SpendingBreakdown from "../Shared/SpendingBreakdown";
import Feedback from "../Shared/Feedback";
export default function Dashboard({ workspace: w }) {
  const recent = [...w.gastos]
    .sort((a, b) => b.fecha.localeCompare(a.fecha) || b.id - a.id)
    .slice(0, 5);
  return (
    <>
      <h1 className="sr-only">Resumen</h1>
      <Feedback {...w} onRetry={w.reload} />
      {!w.loading && !w.error && (
        <>
          <Summary
            total={w.total}
            count={w.gastos.length}
            categories={w.categorias.length}
          />
          <Card
            title="Últimos movimientos"
            description="Los cinco gastos más recientes."
            actions={<span className="pill">Más recientes</span>}
          >
            <ExpenseTable
              expenses={recent}
              categories={w.categorias}
              readOnly
            />
          </Card>
          <SpendingBreakdown expenses={w.gastos} categories={w.categorias} />
        </>
      )}
    </>
  );
}

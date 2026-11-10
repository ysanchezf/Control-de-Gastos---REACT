import Summary from "../components/Summary";
import Card from "../components/Card";
import ExpenseTable from "../components/ExpenseTable";
import SpendingBreakdown from "../components/SpendingBreakdown";
import Feedback from "../components/Feedback";
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

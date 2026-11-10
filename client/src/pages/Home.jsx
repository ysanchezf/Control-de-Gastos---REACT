import { Link } from "react-router-dom";
import Summary from "../components/Summary";
import SavingsSimulator from "../components/SavingsSimulator";
import Feedback from "../components/Feedback";
export default function Home({ workspace }) {
  return (
    <div className="welcome-page">
      <section className="hero">
        <div>
          <p className="eyebrow">MENOS CUENTAS PENDIENTES. MÁS TRANQUILIDAD.</p>
          <h1>Haz espacio para lo que de verdad importa.</h1>
          <p>
            Registra tus gastos, encuentra en qué se va tu dinero y empieza a
            decidir con más claridad.
          </p>
          <div className="actions">
            <Link className="button dark" to="/gastos">
              Organizar mis gastos ↗
            </Link>
            <Link to="/categorias">Ver categorías</Link>
          </div>
          <span className="note">
            Espacio local de demostración · Sin registro
          </span>
        </div>
        <img
          src="/images/finance-illustration.svg"
          alt="Ilustración de organización de finanzas personales"
        />
      </section>
      <Feedback {...workspace} onRetry={workspace.reload} />
      {!workspace.loading && !workspace.error && (
        <Summary
          allowVisibility
          total={workspace.total}
          count={workspace.gastos.length}
          categories={workspace.categorias.length}
        />
      )}
      <div className="two-column landing-bottom">
        <section>
          <p className="eyebrow">UN HÁBITO SENCILLO</p>
          <h2>Tu dinero, con un poco más de contexto.</h2>
          <ol className="steps">
            <li>
              <strong>Dale un lugar.</strong> Crea categorías que se parezcan a
              tu vida.
            </li>
            <li>
              <strong>Anótalo al momento.</strong> Un gasto pequeño también
              forma parte del total.
            </li>
            <li>
              <strong>Mira el conjunto.</strong> Consulta tus movimientos y
              ajusta tus hábitos.
            </li>
          </ol>
        </section>
        <SavingsSimulator />
      </div>
    </div>
  );
}

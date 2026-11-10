import { Link, NavLink, Outlet } from "react-router-dom";
export function Header() {
  const date = new Date().toLocaleDateString("es-DO", {
    weekday: "long",
    day: "numeric",
    month: "long",
  });
  return (
    <header className="topbar">
      <span className="header-title">Control de gastos</span>
      <span className="date-label">{date}</span>
    </header>
  );
}
export function Footer() {
  return (
    <footer className="app-footer">
      <span>
        <strong>bolsillo</strong> · Control de gastos
      </span>
      <span>Tercer artefacto React · {new Date().getFullYear()}</span>
    </footer>
  );
}
export default function Layout() {
  return (
    <div className="app-shell">
      <a className="skip" href="#main-content">
        Saltar al contenido
      </a>
      <aside className="sidebar">
        <Link className="brand" to="/">
          <img src="/images/wallet.svg" alt="" /> bolsillo
        </Link>
        <p className="nav-label">MIS CUENTAS</p>
        <nav aria-label="Navegación principal">
          <NavLink to="/" end>
            <span aria-hidden="true">◈</span> Resumen
          </NavLink>
          <NavLink to="/gastos">
            <span aria-hidden="true">↔</span> Gastos
          </NavLink>
          <NavLink to="/categorias">
            <span aria-hidden="true">▦</span> Categorías
          </NavLink>
          <NavLink to="/bienvenida">
            <span aria-hidden="true">⌂</span> Inicio
          </NavLink>
        </nav>
        <div className="sidebar-note">
          <span className="eyebrow">MODO PRUEBA</span>
          <p>Datos de demostración.</p>
        </div>
        <div className="profile">
          <span className="avatar">P</span>
          <div>
            <strong>Prueba</strong>
            <small>Demostración</small>
          </div>
        </div>
      </aside>
      <div className="main-area">
        <Header />
        <main id="main-content">
          <Outlet />
        </main>
        <Footer />
      </div>
    </div>
  );
}

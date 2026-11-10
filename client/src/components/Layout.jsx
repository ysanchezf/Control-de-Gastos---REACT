import { NavLink, Outlet } from "react-router-dom";
export function Header() {
  return (
    <header className="site-header">
      <a className="brand" href="/">
        <img src="/images/wallet.svg" alt="" />
        bolsillo<span> / React</span>
      </a>
      <nav aria-label="Navegación principal">
        <NavLink to="/" end>
          Inicio
        </NavLink>
        <NavLink to="/gastos">Gastos</NavLink>
        <NavLink to="/categorias">Categorías</NavLink>
      </nav>
    </header>
  );
}
export function Footer() {
  return (
    <footer>
      <span>
        <strong>bolsillo.</strong> Un poco más de orden, todos los días.
      </span>
      <span>
        Tercer artefacto · React + ASP.NET Core · {new Date().getFullYear()}
      </span>
    </footer>
  );
}
export default function Layout() {
  return (
    <>
      <a className="skip" href="#contenido">
        Saltar al contenido
      </a>
      <Header />
      <main id="contenido">
        <Outlet />
      </main>
      <Footer />
    </>
  );
}

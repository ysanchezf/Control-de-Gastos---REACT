import { Link, Route, Routes } from "react-router-dom";
import Dashboard from "./Pages/Dashboard";
import Layout from "./Layout/Layout";
import Home from "./Pages/Home";
import Expenses from "./Pages/Expenses";
import Categories from "./Pages/Categories";
import useWorkspace from "./Hooks/useWorkspace";
export default function App() {
  const workspace = useWorkspace();
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Dashboard workspace={workspace} />} />
        <Route path="bienvenida" element={<Home workspace={workspace} />} />
        <Route path="gastos" element={<Expenses workspace={workspace} />} />
        <Route
          path="categorias"
          element={<Categories workspace={workspace} />}
        />
        <Route
          path="*"
          element={
            <>
              <h1>No encontramos esta página</h1>
              <Link to="/">Volver al inicio</Link>
            </>
          }
        />
      </Route>
    </Routes>
  );
}

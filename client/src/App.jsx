import { Link, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import Home from "./pages/Home";
import Expenses from "./pages/Expenses";
import Categories from "./pages/Categories";
import useWorkspace from "./hooks/useWorkspace";
export default function App() {
  const workspace = useWorkspace();
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Home workspace={workspace} />} />
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

import { useCallback, useEffect, useState } from "react";
import { api } from "../services/api";
export default function useWorkspace() {
  const [data, setData] = useState({ gastos: [], categorias: [], total: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const reload = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [gastos, categorias, summary] = await Promise.all([
        api.gastos(),
        api.categorias(),
        api.total(),
      ]);
      setData({ gastos, categorias, total: summary.totalGastos });
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, []);
  useEffect(() => {
    reload();
  }, [reload]);
  return { ...data, loading, error, reload };
}

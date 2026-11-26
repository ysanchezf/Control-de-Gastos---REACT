// Todas las peticiones usan el mismo origen; Vite las reenvía a ASP.NET Core.
export async function request(path, method = "GET", data) {
  const headers = { Accept: "application/json" };
  if (method !== "GET") {
    const response = await fetch("/cuenta/antiforgery", {
      credentials: "same-origin",
      cache: "no-store",
    });
    if (!response.ok)
      throw new Error("No pudimos preparar la operación. Intenta otra vez.");
    headers["X-CSRF-TOKEN"] = (await response.json()).token;
    headers["Content-Type"] = "application/json";
  }
  const response = await fetch(`/api/${path}`, {
    method,
    headers,
    credentials: "same-origin",
    cache: "no-store",
    body: data === undefined ? undefined : JSON.stringify(data),
  });
  if (!response.ok) {
    const problem = await response.json().catch(() => ({}));
    throw new Error(
      problem.message ||
        Object.values(problem.errors || {})
          .flat()
          .join(" ") ||
        "No pudimos completar la solicitud. Comprueba que la API está ejecutándose.",
    );
  }
  return response.status === 204 ? null : response.json();
}
export const api = {
  gastos: () => request("Gastos"),
  categorias: () => request("Categorias"),
  total: () => request("Gastos/total"),
  saveGasto: (value, id) =>
    request(`Gastos${id ? `/${id}` : ""}`, id ? "PUT" : "POST", value),
  deleteGasto: (id) => request(`Gastos/${id}`, "DELETE"),
  saveCategoria: (value, id) =>
    request(`Categorias${id ? `/${id}` : ""}`, id ? "PUT" : "POST", value),
  deleteCategoria: (id) => request(`Categorias/${id}`, "DELETE"),
};

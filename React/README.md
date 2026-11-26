# Tercer artefacto React

Este directorio conserva la interfaz React 19, JSX, Vite y React Router. Su backend y SQLite se encuentran en `../Backend/` y se comparten con Razor Pages.

Desde la raíz del repositorio ejecuta la API:

```powershell
dotnet run --project Backend/ControlGastos.Api.csproj --launch-profile http
```

En otra terminal:

```powershell
npm ci --prefix React
npm run dev --prefix React
```

Abre http://127.0.0.1:5173. Para compilar, `npm run build --prefix React`; después reinicia el backend y abre http://localhost:5080. No necesitas cambiar las URLs dentro de api.js: son relativas y el proxy apunta a 5080.

## Archivos y requisitos del tercer artefacto

- `Components/main.jsx` inicia React y `Components/App.jsx` define rutas.
- `/bienvenida`: landing con `Home`, `Summary` y `SavingsSimulator`.
- `/`: `Dashboard`, con resumen, últimos movimientos y distribución.
- `/gastos`: `Expenses`, con `ExpenseForm` y `ExpenseTable`.
- `/categorias`: `Categories`, con `CategoryForm` y `CategoryList`.
- `Components/Layout/Layout.jsx`: menú, header y footer.
- `Components/Shared/Card.jsx`: props y children para reutilizar el contenedor.
- `Components/Hooks/useWorkspace.js`: carga gastos, categorías y total con useEffect y Promise.all.
- `Services/api.js`: fetch, validación de respuestas y protección antifalsificación.
- `wwwroot/app.css` e `images/`: estilos e imágenes. Vite genera `wwwroot/build/`, ignorado por Git.
- `SavingsSimulator`: contador de meses y frecuencia semanal/mensual con useState.
- `Summary`: visibilidad del importe en la bienvenida.

Los formularios entregan datos mediante callbacks; la página guarda a través de api.js y recarga la información. El simulador calcula estimaciones y no modifica SQLite. Las reglas y migraciones del backend anterior se conservan.

Las instrucciones de preservación de datos, configuración, Visual Studio y ejecución conjunta están en el README de la raíz. La entrega de la URL y la exposición grupal se realizan por el grupo; no se envían automáticamente a la plataforma.

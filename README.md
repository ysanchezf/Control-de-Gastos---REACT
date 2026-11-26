# bolsillo · React y Razor Pages

Dos interfaces académicas para el mismo control de gastos. **Una API ASP.NET Core .NET 10 y la SQLite existente** atienden a React y a Razor Pages. Se conserva el modo local de demostración sin login, con identidad técnica «Prueba».

## Organización

```text
ControlGastos.sln
AGENTS.md
README.md
Backend/                  API común y SQLite existente
  Controllers/            Gastos y categorías
  Data/ Models/ Migrations/
  Services/               Totales, identidad local y traslado de App_Data
  Program.cs
  appsettings.json
  App_Data/               Datos y claves locales, ignorados por Git
React/                    Tercer artefacto completo
  Components/             Pages, Layout, Shared y Hooks
  Services/api.js
  wwwroot/                CSS, imágenes y bundle generado
  package.json
  tests/
RazorPages/
  Web/                    Proyecto ASP.NET Core Razor Pages
    Pages/                .cshtml y PageModel .cshtml.cs
    Models/
    Services/             Cliente HTTP tipado de la API
    wwwroot/              CSS, imágenes y JavaScript
    Program.cs
    appsettings.json
  tests/
```

React conserva las pantallas de Resumen, Gastos, Categorías e Inicio, además del simulador, contador, frecuencia y visibilidad. El nuevo Razor utiliza **Razor Pages, no componentes Blazor**.

## Antes de actualizar: conservar SQLite

1. Detén las ejecuciones anteriores y haz una copia de **toda** tu carpeta `App_Data`, incluidos `keys`, `react-demo.db` y los posibles `react-demo.db-wal` y `react-demo.db-shm`.
2. Actualiza el repositorio sin descartar modificaciones locales.
3. Al iniciar el backend con la conexión predeterminada, detecta `react-demo.db` en las ubicaciones anteriores: `App_Data/`, `server/App_Data/`, `React/App_Data/` o `React/server/App_Data/`.
4. Si encuentra una sola base anterior, primero respalda toda la carpeta en `local-backups/` y luego la traslada a `Backend/App_Data/`. Conserva las claves y la identidad anterior. No importa ni elimina registros.
5. Si hay más de una base, o el destino tiene archivos, **se detiene sin sobrescribir**. Conserva ambas copias y selecciona la correcta manualmente.

`Backend/appsettings.json` mantiene `Data Source=App_Data/react-demo.db`, ahora relativa al backend. Si ya utilizas otra ruta, establece `ConnectionStrings__DemoConnection` con su ruta absoluta antes de iniciar el backend; una conexión personalizada desactiva el traslado automático. No renombres una base de otro proyecto sin comprobar su esquema. Las bases de cuentas de Blazor no se seleccionan automáticamente.

La base personal de tu PC no está en GitHub y no puede copiarse desde la nube. En una instalación nueva sin base local, EF Core crea una base vacía con el mismo esquema; si ya tienes datos, verifica el respaldo y la ruta antes del primer arranque. **Razor y React muestran y modifican los mismos registros.**

## Requisitos

- SDK .NET 10; Visual Studio con Desarrollo de ASP.NET y web si utilizas ese editor.
- Node.js 22.12 o superior (también Node 24) y npm, solo para React.
- No necesitas SQL Server. Python no interviene en la aplicación.

## Ejecutar Razor Pages

Abre dos terminales en la raíz del repositorio.

Terminal 1, API común:

```powershell
dotnet run --project Backend/ControlGastos.Api.csproj --launch-profile http
```

Terminal 2, interfaz Razor:

```powershell
dotnet run --project RazorPages/Web/ControlGastos.Razor.Web.csproj --launch-profile http
```

Abre **http://localhost:5081**. La API está en **http://localhost:5080/swagger**. No es necesario iniciar React para utilizar Razor.

En Visual Studio abre `ControlGastos.sln`. En Propiedades de la solución → Proyectos de inicio, selecciona **Varios proyectos de inicio** y configura `ControlGastos.Api` y `ControlGastos.Razor.Web` como Iniciar, con sus perfiles `http`. Mantén ambos ejecutándose.

## Ejecutar React

Con la API anterior ejecutándose, abre otra terminal desde la raíz:

```powershell
npm ci --prefix React
npm run dev --prefix React
```

Abre **http://127.0.0.1:5173**. Vite reenvía `/api`, `/cuenta` y `/images` al backend en 5080. Para servir el bundle React desde el propio backend:

```powershell
npm ci --prefix React
npm run build --prefix React
```

Después inicia o reinicia el backend y abre **http://localhost:5080**. No ejecutes dos copias de la API sobre el mismo puerto ni la misma base. Puedes abrir React y Razor a la vez y recargar para ver los cambios compartidos.

## Puertos y configuración

| Aplicación | Puerto | Configuración |
|---|---|---|
| API compartida | 5080 | `Backend/Properties/launchSettings.json` |
| React en desarrollo | 5173 | `React/vite.config.js` |
| Razor Pages | 5081 | `RazorPages/Web/Properties/launchSettings.json` |

Razor lee `Api:BaseUrl` de `RazorPages/Web/appsettings.json` mediante opciones e inyección de dependencias. Si cambias el puerto de la API, actualiza también ese valor y el proxy de React. El sitio Razor usa `AppSettings` para el nombre y el presupuesto sugerido.

## Evaluación final Razor Pages

| Requisito del PDF | Implementación |
|---|---|
| Landing page | `Pages/Index.cshtml` y `Index.cshtml.cs` |
| Tres páginas adicionales como mínimo | Resumen, Gastos y Categorías; además Reportes, Presupuesto y detalle de gasto |
| Sintaxis Razor | `@page`, `@model`, `@Model`, `@if`, `@foreach` en las páginas |
| Propiedades de .cshtml.cs a .cshtml | Listas, total, categorías, nombre de aplicación y resultados del presupuesto |
| Propiedades de .cshtml a .cshtml.cs | `[BindProperty]`, formularios `method="post"` y campos `asp-for` |
| CSS, imágenes y JS | `RazorPages/Web/wwwroot/` |
| Llamar funciones JavaScript | Ocultar/mostrar importe, confirmar eliminación e imprimir referencia en `site.js` |
| Header y footer personalizados | `Pages/Shared/_Layout.cshtml` |
| Barra de navegación | Tag Helpers `asp-page` en el layout |
| Program.cs y appsettings.json | Registro de Razor Pages, HttpClient, opciones, localización, protección y configuración API |
| Conexión Web API ASP.NET Core | `Backend/Controllers/` |
| Clase de servicios | `Services/ControlGastosApi.cs` e interfaz `IControlGastosApi` |
| Inyección de servicios y configuración | Constructores PageModel, cliente tipado, `IOptions<ApiOptions>` e `IConfiguration` |
| Mostrar elementos de la API | Resumen, tabla de Gastos y tarjetas de Categorías |
| Modificar al menos un modelo | Crear, editar y eliminar gastos **y** categorías mediante API |
| Entregar URL del código | `RazorPages/ENTREGA.txt`; entrega y exposición a cargo del grupo |

Dos extras sobre lo solicitado: **reportes mensuales con exportación CSV** y **simulador de presupuesto mensual**, con cálculos del lado del servidor. El simulador no persiste un límite ni modifica gastos. El CSV protege descripciones que podrían interpretarse como fórmulas en una hoja de cálculo.

Los PDF de clase se usaron como material de referencia para PageModel, Tag Helpers, formularios, servicios e inyección de dependencias. No se activó login ni se configuró IIS: la evaluación no los exige y esta versión compartida permanece solo para Development.

## API y flujo de datos

- `GET /api/Gastos`, `/api/Gastos/{id}` y `/api/Gastos/total`.
- `POST /api/Gastos`, `PUT /api/Gastos/{id}`, `DELETE /api/Gastos/{id}`.
- `GET /api/Categorias`, `/api/Categorias/{id}`.
- `POST /api/Categorias`, `PUT /api/Categorias/{id}`, `DELETE /api/Categorias/{id}`.

En Razor: formulario → PageModel → `IControlGastosApi` → HTTP → controlador → EF Core → SQLite. Los PageModels no acceden a SQLite directamente. Tras guardar, redirigen a la lista para evitar repetir el POST al recargar. Los errores mantienen el formulario y muestran mensajes legibles.

Razor protege sus propios formularios y el cliente obtiene `/cuenta/antiforgery` antes de escribir en la API. El token y su cookie se envían juntos en esa operación, sin compartir un contenedor de cookies entre visitantes. Se conservan filtros por propietario y reglas: montos positivos, categoría existente, nombres duplicados rechazados y categorías con gastos no eliminables.

## Verificar

Desde la raíz:

```powershell
npm ci --prefix React
npm run build --prefix React
dotnet build ControlGastos.sln
```

Pruebas opcionales con Python, Playwright y Chromium: [RazorPages/tests/README.md](RazorPages/tests/README.md) y [React/tests/README.md](React/tests/README.md). Usan bases y claves temporales; no tocan tu SQLite local. Las capturas no se versionan.

## Guion de exposición

1. Mostrar landing y navegación; distinguir Razor Pages de React.
2. Crear categoría y gasto, consultar detalle y editar el monto.
3. Explicar `@Model`, `[BindProperty]`, `OnGetAsync`, `OnPostAsync` y Tag Helpers.
4. Señalar el servicio HTTP y su registro en Program.cs; mostrar la URL en appsettings.json.
5. Mostrar los mismos registros en React y Razor, porque comparten backend y SQLite.
6. Demostrar reporte CSV, presupuesto, visibilidad y confirmación JavaScript.
7. Mostrar Swagger y explicar validaciones y persistencia.

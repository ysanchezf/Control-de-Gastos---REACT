# Bolsillo · Tercer artefacto React

Aplicación de control de gastos con **React 19, JSX, Vite y ASP.NET Core .NET 10**, con SQLite. Pantallas de Resumen, Gastos y Categorías con el diseño del segundo artefacto Blazor. Landing adicional en `/bienvenida` (enlace Inicio). Permite crear, editar, consultar y eliminar gastos y categorías.

## Ejecutar en Windows / Visual Studio

Requisitos: Visual Studio con la carga **Desarrollo de ASP.NET y web**, SDK **.NET 10** y **Node.js 22.12 o superior** (también Node 24). npm se instala con Node. Comprueba `dotnet --version`, `node --version` y `npm --version`.

1. Clona el repositorio y abre `ControlGastos.React.sln` en Visual Studio.
2. Desde la raíz, prepara la interfaz:

```powershell
npm ci
npm run build
```

3. Selecciona `ControlGastos.Api` como proyecto de inicio, perfil **http**, y ejecuta con F5.
4. Abre **http://localhost:5080**. Swagger está en **http://localhost:5080/swagger**.

La interfaz compilada y la API se sirven en un solo proceso. Después de modificar JSX o CSS, vuelve a ejecutar `npm run build` y reinicia la aplicación.

### Alternativa desde la terminal

Desde la raíz del repositorio:

```powershell
npm ci
npm run build
dotnet run --project ControlGastos.Api.csproj --launch-profile http
```

### Desarrollo con recarga automática (opcional)

Ejecuta `dotnet run --project ControlGastos.Api.csproj --launch-profile http` en una terminal y `npm run dev` en otra, ambas desde la raíz. Abre **http://127.0.0.1:5173**. Vite reenvía `/api`, `/cuenta` y `/images` al puerto **5080**. Si cambias ese puerto, modifica también `vite.config.js`. Detén los procesos con Ctrl+C (o Shift+F5 en Visual Studio).

## Estructura compartida con LP2

No hay carpetas `client` ni `server`. La solución y ambas herramientas trabajan desde la raíz:

```text
Components/
  App.jsx                  # Rutas React
  main.jsx                 # Punto de entrada
  Pages/                   # Resumen, Gastos, Categorías e Inicio
  Layout/                  # Navegación, encabezado y pie
  Shared/                  # Formularios, tablas, tarjetas y simulador
  Hooks/                   # Estado compartido del espacio local
Controllers/               # API REST
Data/                      # DbContext
Models/
Migrations/
Services/                  # Servicios C# y cliente HTTP api.js
Properties/
wwwroot/
  app.css
  images/
  build/                   # Bundle generado, ignorado en Git
Program.cs
ControlGastos.Api.csproj
ControlGastos.React.sln
appsettings.json
index.html
package.json
vite.config.js
tests/
```

Se sigue la organización de LP2 conservando componentes JSX en lugar de Razor. Vite genera únicamente `wwwroot/build`; no borra CSS, imágenes ni datos. ASP.NET Core sirve las imágenes y el bundle, y resuelve las rutas React con `build/index.html`.

## Datos y alcance

La base `App_Data/react-demo.db` se crea automáticamente con las migraciones existentes. Empieza vacía: crea una categoría y después un gasto. Los datos persisten entre reinicios. Las claves de protección se generan en `App_Data/keys`. Ninguno de estos archivos se sube a GitHub.

### Conservar datos de una instalación anterior

Detén la aplicación anterior y haz una copia de seguridad de **toda** la carpeta `server/App_Data`, incluidas las claves y los posibles archivos SQLite `-wal` y `-shm`. Después de actualizar, mueve esa carpeta completa a `App_Data` en la raíz **antes del primer arranque**. No sobrescribas una carpeta `App_Data` existente: conserva ambas copias y decide cuál utilizar. La conexión sigue usando `Data Source=App_Data/react-demo.db`, las migraciones y la identidad local no cambian. Si utilizas `ConnectionStrings__DemoConnection`, conserva el valor y ajusta cualquier ruta relativa a la nueva raíz. No se incluye ni se sustituye ninguna base personal del repositorio LP2.

Este artefacto conserva el **modo local de demostración sin login**, con una identidad técnica para mantener los filtros de propietario de la API. No ofrece cuentas ni aislamiento entre visitantes: es un espacio compartido de desarrollo y el servidor rechaza ejecutarse fuera de Development. No debe publicarse como una aplicación multiusuario. No se copiaron bases personales ni claves del proyecto anterior.

## Reutilización del segundo artefacto

Origen: https://github.com/ysanchezf/Control-de-Gastos---LP2

Se verificó `origin/main` al preparar el proyecto (commit `8a22296`; los controladores y modelos coinciden con la versión local `e48c3c2`). Se reutilizaron sin alterar las reglas de negocio:

- `GastosController` y `CategoriasController`: CRUD, validaciones y filtro por propietario.
- `Data/AppDbContext`, modelos y migraciones de EF Core.
- `IGastoService`, `GastoService` y `LocalWorkspace`.
- Imágenes vectoriales de billetera y finanzas.

Se creó un `Program.cs` específico para React y se reemplazó la interfaz Blazor por componentes JSX. No se incluyen las pantallas ni controladores de administración de cuentas, que no forman parte de esta entrega.

## Requisitos del PDF y ubicación

| Requisito | Implementación |
|---|---|
| Landing page | `/bienvenida`, `Components/Pages/Home.jsx`: presentación, llamadas a la acción e ilustración |
| Dos páginas adicionales | `Components/Pages/Expenses.jsx` y `Components/Pages/Categories.jsx` |
| Dos componentes por página | Inicio: `Summary` y `SavingsSimulator`; Gastos: `ExpenseForm` y `ExpenseTable`; Categorías: `CategoryForm` y `CategoryList` |
| JSX | Archivos `.jsx` de páginas y componentes |
| Props | Datos y callbacks desde las páginas hacia formularios y listados |
| Children | `Card.jsx`, utilizado con contenido anidado en sus etiquetas |
| CSS e imágenes | `wwwroot/app.css` y `wwwroot/images/` |
| Header y footer personalizados | `Components/Layout/Layout.jsx` |
| Barra de navegación | `Header`, con `NavLink` y React Router |
| Conexión a ASP.NET Core | `Services/api.js`, mediante `fetch` y proxy Vite |
| Contador | Meses de ahorro en `SavingsSimulator`, de 1 a 24 |
| Cambio de estado | Frecuencia semanal/mensual del simulador (`useState`) |
| Cambio de visibilidad | Ocultar/mostrar importe en `Summary` |
| Información desde API | Resumen, tabla de gastos y categorías; carga con `useEffect` |

Los tres componentes de interacción son visibles en Inicio (`/bienvenida`) cuando la API está disponible. El simulador no escribe en la base: calcula un ahorro estimado, sin intereses, usando cuatro semanas por mes.

## Flujo y archivos importantes

`main.jsx` monta React y el router. `App.jsx` define rutas y comparte el estado de los datos. `useWorkspace.js` carga gastos, categorías y total con `Promise.all`; ofrece carga, error y reintento. Las páginas gestionan las acciones. Los formularios reciben callbacks y entregan los valores al padre. Tras guardar se consulta nuevamente la API.

`Services/api.js` centraliza HTTP. Antes de POST, PUT o DELETE solicita `/cuenta/antiforgery` y envía el token en `X-CSRF-TOKEN`, junto con las cookies del mismo origen. Muestra los mensajes de validación de ASP.NET Core. React no necesita JSInterop porque ejecuta JavaScript directamente.

`Program.cs` registra EF Core SQLite, controladores, autenticación de demostración, antiforgery y Swagger; aplica migraciones e inicializa la identidad técnica. No utiliza componentes Blazor.

### Rutas API

- `GET /api/Gastos`, `GET /api/Gastos/{id}`, `GET /api/Gastos/total`.
- `POST /api/Gastos`, `PUT /api/Gastos/{id}`, `DELETE /api/Gastos/{id}`.
- `GET /api/Categorias`, `GET /api/Categorias/{id}`.
- `POST /api/Categorias`, `PUT /api/Categorias/{id}`, `DELETE /api/Categorias/{id}`.

Una categoría con gastos no se puede eliminar: la API responde 409. Los montos deben ser positivos y la categoría del gasto debe pertenecer al espacio actual. Editar/eliminar exige confirmación o una acción explícita; los mensajes se presentan sin perder el formulario ante errores.

## Compilar y verificar

```powershell
dotnet build ControlGastos.React.sln
npm ci
npm run build
npm audit
```

Prueba manual: crear categoría, intentar duplicarla, crear y editar gasto, comprobar total, intentar borrar categoría con gastos, eliminar gasto y luego categoría. En Inicio probar contador, frecuencia y visibilidad. Revisar navegación y tamaño móvil.

La prueba automatizada opcional `tests/verify.py` usa Python con Playwright y Chromium y arranca procesos con una base temporal: no es requisito de ejecución de la aplicación. Instrucciones en `tests/README.md`.

## Guion breve de exposición

1. Inicio: explicar React, JSX y navegación; demostrar contador, estado y visibilidad.
2. Crear categoría y gasto, editar monto y consultar total.
3. Mostrar componentes, props y callbacks; señalar `Card` y `children`.
4. Explicar `useState`, `useEffect` y el servicio HTTP.
5. Mostrar Swagger, los controladores reutilizados y SQLite.

Entregar el archivo `ENTREGA.txt` con la URL del repositorio y realizar la exposición grupal que indica el PDF. Este repositorio no realiza la entrega en la plataforma académica automáticamente.

## Validación realizada

- Backend compilado sin errores ni advertencias; frontend compilado con Vite.
- `npm audit`: cero vulnerabilidades al verificar la entrega.
- Prueba integrada aprobada tanto con el bundle servido por ASP.NET Core como mediante Vite y su proxy.
- CRUD real de gastos y categorías, persistencia tras reiniciar el proceso, total actualizado, duplicados y eliminación de categoría en uso rechazados.
- Escritura sin token antifalsificación rechazada; contador, frecuencia y visibilidad comprobados.
- Pantallas revisadas a 1440 y 390 píxeles, con imágenes cargadas y sin errores JavaScript.

## Diseño del segundo artefacto

Las rutas `/`, `/gastos` y `/categorias` reproducen la estructura, textos y estilos de Blazor: barra lateral, encabezado con fecha, pie, tarjetas de resumen, tabla y distribución por categoría; formularios en dos columnas y tarjetas de categorías. El CSS base se reutilizó de `wwwroot/app.css` del repositorio original. El pie identifica correctamente la entrega React.

La landing con imágenes, contador, cambio de frecuencia y visibilidad se conserva en `/bienvenida`, accesible mediante **Inicio**. Así las pantallas de trabajo mantienen el diseño original y se conservan los requisitos del tercer artefacto. `Dashboard.jsx` compone `Summary`, `ExpenseTable` y el nuevo `SpendingBreakdown`.

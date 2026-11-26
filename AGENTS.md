# Preferencias del proyecto bolsillo

- Antes de trabajar, revisar el estado local y el historial actual de GitHub. Conservar modificaciones locales y commits existentes; no usar force push ni reescribir el historial.
- Los commits deben llamarse exactamente `cambio05`, `cambio06`, `cambio07`, etc. Consultar el historial y utilizar el siguiente número disponible; no reiniciar la secuencia.
- Mantener React y JSX con la estructura equivalente a Control-de-Gastos---LP2: Components/Layout, Components/Pages, Components/Shared, Controllers, Data, Models, Migrations, Services y wwwroot, con el proyecto y la configuración en la raíz.
- Conservar la apariencia de LP2 y las rutas /, /gastos, /categorias y /bienvenida, incluido el simulador y las interacciones académicas de contador, estado y visibilidad.
- Mantener el modo de demostración. No reactivar el login sin una solicitud del usuario; conservar la protección antifalsificación y las validaciones de la API.
- Conservar las bases y claves locales. No subir App_Data, bases SQLite ni sus auxiliares, claves, bin, obj o node_modules. Consultar en README cómo trasladar server/App_Data al actualizar una instalación anterior; no sobrescribir datos existentes.
- No restaurar documentos eliminados por el usuario ni reintroducir comentarios XML <summary> sin necesidad.
- Explicar los cambios en español sencillo, adecuado para una exposición académica.

## Verificación

Desde la raíz: `npm ci`, `npm run build` y `dotnet build ControlGastos.React.sln`.
Para cambios de funcionamiento o estructura, ejecutar las pruebas integradas documentadas en tests/README.md con una base temporal. Python y Playwright son herramientas opcionales de pruebas; no son requisitos de ejecución de la aplicación.

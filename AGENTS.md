# Preferencias del proyecto bolsillo

- Revisar el estado local y el historial de GitHub antes de trabajar. Conservar modificaciones locales y commits; no usar force push ni reescribir el historial.
- Los commits deben llamarse exactamente `cambio05`, `cambio06`, etc. Utilizar el siguiente número disponible según el historial.
- Estructura autorizada: `Backend/` contiene la API ASP.NET Core y la SQLite existente; `React/` conserva la interfaz del tercer artefacto; `RazorPages/` contiene la evaluación final. No volver a mezclar las fuentes de los proyectos.
- Ambas interfaces consumen la misma API y la misma base `Backend/App_Data/react-demo.db`. No crear una base independiente para Razor ni reemplazar datos locales con ejemplos.
- Conservar las pantallas de React, su apariencia, el simulador y las rutas /, /gastos, /categorias y /bienvenida.
- Razor Pages utiliza registro, login y logout por solicitud del usuario. Proteger sus páginas y conservar el aislamiento por propietario, contraseñas con hash, cookies HttpOnly y protección antifalsificación. React conserva compatibilidad con el espacio local anterior y puede usar la sesión de la API en el mismo host.
- No subir bases, auxiliares SQLite, claves, App_Data, local-backups, bin, obj, node_modules ni capturas de pruebas. El traslado local debe conservar toda App_Data y detenerse ante conflictos, sin sobrescribir.
- No restaurar documentos eliminados por el usuario. AGENTS.md y README.md fueron confirmados y deben conservarse según la solicitud actual.
- Evitar comentarios XML <summary> innecesarios. Explicar en español sencillo los cambios y los requisitos académicos.

## Verificación

Desde la raíz: `npm ci --prefix React`, `npm run build --prefix React`, `dotnet build ControlGastos.sln`.
Para cambios funcionales ejecutar `python RazorPages/tests/verify.py`, `python React/tests/verify.py` y, si cambia el proxy, `python React/tests/verify.py --dev`, con bases temporales y los puertos libres. Python y Playwright son herramientas opcionales de pruebas.

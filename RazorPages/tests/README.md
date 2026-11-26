# Verificación integrada de Razor Pages

Desde la raíz del repositorio, con .NET 10:

```powershell
npm ci --prefix React
npm run build --prefix React
dotnet build ControlGastos.sln
python -m pip install playwright
python -m playwright install chromium
python RazorPages/tests/verify.py
```

La prueba busca `/usr/bin/chromium` en Linux. Si instalas el navegador de Playwright, configura `CHROMIUM_PATH` con su ejecutable, por ejemplo en PowerShell: `$env:CHROMIUM_PATH = "C:\ruta\a\chrome.exe"`.

Los puertos 5080 y 5081 deben estar libres. El script inicia la API compartida y el sitio Razor sobre carpetas temporales con SQLite y claves aisladas. Comprueba que React y Razor ven los mismos datos, CRUD de gastos y categorías, detalle, total, persistencia, migración de ubicación de los datos anteriores con respaldo, errores de validación, duplicados, categoría en uso, antiforgery de ambos servidores, JavaScript, CSV, presupuesto, API inaccesible y rutas directas a 1440 y 390 píxeles. Las capturas quedan en `RazorPages/tests/artifacts/`, ignoradas por Git.

Python y Playwright no son necesarios para ejecutar la aplicación. No se usan bases personales ni la App_Data real del repositorio.

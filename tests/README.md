# Verificación integrada opcional

Requiere Python, `pip install playwright` y Chromium. En Windows instala el navegador con `playwright install chromium` y define `CHROMIUM_PATH` con la ruta al ejecutable. Python no interviene en la aplicación.

Desde la raíz, compila primero:

```
dotnet build ControlGastos.React.sln
npm ci --prefix client
npm run build --prefix client
python tests/verify.py
```

El puerto 5080 debe estar libre. La prueba ejecuta el bundle React con la API real en el mismo origen, crea una base y claves temporales y termina el servidor al finalizar. Verifica CRUD de ambas entidades, total, conflictos, protección antifalsificación, contador, cambio de estado, visibilidad, rutas directas, imágenes y ausencia de desbordamiento global en escritorio y móvil. Guarda capturas ignoradas por Git en `tests/artifacts`.

En Linux también puedes ejecutar `python tests/verify.py --dev` para repetir la prueba a través del servidor Vite y su proxy, con los puertos 5080 y 5173 libres.

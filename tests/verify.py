"""Prueba integrada con API real, SQLite temporal y navegador Chromium."""
import json, os, pathlib, shutil, subprocess, tempfile, time, urllib.request, sys
from playwright.sync_api import sync_playwright, expect

ROOT = pathlib.Path(__file__).resolve().parents[1]
DEV = '--dev' in sys.argv
UI = 'http://127.0.0.1:5173' if DEV else 'http://127.0.0.1:5080'

def wait(url, process):
    for _ in range(100):
        if process.poll() is not None:
            raise RuntimeError('El proceso terminó; revisa los archivos de registro temporales.')
        try:
            with urllib.request.urlopen(url, timeout=1) as response:
                if response.status == 200:
                    return
        except Exception:
            time.sleep(.2)
    raise RuntimeError('No respondió ' + url)

with tempfile.TemporaryDirectory(prefix='bolsillo-react-') as directory:
    temp = pathlib.Path(directory)
    # La raíz temporal aísla también las claves DataProtection.
    shutil.copy(ROOT/'server/appsettings.json', temp/'appsettings.json')
    shutil.copytree(ROOT/'client/dist', temp/'wwwroot')
    env = os.environ | {'ASPNETCORE_ENVIRONMENT': 'Development', 'ConnectionStrings__DemoConnection': f'Data Source={temp / "test.db"}'}
    log = open(temp/'server.log', 'w+')
    process = subprocess.Popen(['dotnet', str(ROOT/'server/bin/Debug/net10.0/ControlGastos.Api.dll'), '--urls', 'http://127.0.0.1:5080', '--contentRoot', str(temp)], cwd=temp, env=env, stdout=log, stderr=log)
    vite = None
    try:
        wait('http://127.0.0.1:5080/api/Gastos', process)
        if DEV:
            vite = subprocess.Popen(['node', str(ROOT/'client/node_modules/vite/bin/vite.js'), '--host', '127.0.0.1'], cwd=ROOT/'client', stdout=log, stderr=log)
            wait(UI, vite)
        with sync_playwright() as p:
            browser = p.chromium.launch(executable_path=os.environ.get('CHROMIUM_PATH', '/usr/bin/chromium'), headless=True, args=['--no-sandbox'])
            page = browser.new_page(viewport={'width': 1440, 'height': 1050})
            errors = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            page.goto(UI+'/')
            expect(page.get_by_role('heading', name='Haz espacio para lo que de verdad importa.')).to_be_visible()
            expect(page.get_by_role('heading', name='Tu panorama')).to_be_visible()
            expect(page.locator('.counter output')).to_have_text('3')
            page.get_by_role('button', name='Aumentar meses').click()
            expect(page.locator('.counter output')).to_have_text('4')
            page.get_by_role('button', name='Frecuencia: mensual').click()
            expect(page.get_by_role('button', name='Frecuencia: semanal')).to_have_attribute('aria-pressed', 'true')
            page.get_by_role('button', name='Ocultar importe').click()
            expect(page.get_by_text('••••••')).to_be_visible()
            page.get_by_role('button', name='Mostrar importe').click()
            page.get_by_role('navigation').get_by_role('link', name='Categorías').click()
            page.get_by_label('Nombre', exact=True).fill('Alimentación')
            page.get_by_label('Descripción', exact=True).fill('Compras del hogar')
            page.get_by_role('button', name='Crear categoría').click()
            expect(page.get_by_role('heading', name='Alimentación', exact=True)).to_be_visible()
            page.get_by_label('Nombre', exact=True).fill('Alimentación')
            page.get_by_label('Descripción', exact=True).fill('Duplicada')
            page.get_by_role('button', name='Crear categoría').click()
            expect(page.get_by_role('alert')).to_contain_text('Ya tienes una categoría')
            page.get_by_role('navigation').get_by_role('link', name='Gastos', exact=True).click()
            page.get_by_label('Descripción', exact=True).fill('Compra semanal')
            page.get_by_label('Monto (RD$)').fill('1250')
            page.get_by_label('Categoría', exact=True).select_option(label='Alimentación')
            page.get_by_role('button', name='Agregar gasto').click()
            expect(page.get_by_role('cell', name='Compra semanal', exact=True)).to_be_visible()
            page.get_by_role('button', name='Editar Compra semanal').click()
            page.get_by_label('Monto (RD$)').fill('1300')
            page.get_by_role('button', name='Guardar cambios').click()
            expect(page.get_by_text('Gasto guardado.', exact=True)).to_be_visible()
            total = page.request.get('http://127.0.0.1:5080/api/Gastos/total').json()['totalGastos']
            assert total == 1300, total
            page.on('dialog', lambda dialog: dialog.accept())
            page.get_by_role('navigation').get_by_role('link', name='Categorías').click()
            page.get_by_role('button', name='Eliminar Alimentación').click()
            expect(page.get_by_role('alert')).to_contain_text('Esta categoría tiene gastos')
            page.get_by_role('button', name='Editar Alimentación').click()
            page.get_by_label('Nombre', exact=True).fill('Hogar')
            page.get_by_role('button', name='Guardar cambios').click()
            expect(page.get_by_role('heading', name='Hogar', exact=True)).to_be_visible()
            # Demuestra que el filtro antiforgery rechaza escrituras sin token.
            response = page.request.post('http://127.0.0.1:5080/api/Categorias', data={'nombre':'Sin token','descripcion':'No debe crearse'})
            assert response.status == 400, response.status
            shots = ROOT/'tests/artifacts'
            shots.mkdir(exist_ok=True)
            for width in [1440, 390]:
                page.set_viewport_size({'width':width,'height':1000})
                for route in ['/', '/gastos', '/categorias']:
                    page.goto(UI+route)
                    expect(page.locator('h1')).to_be_visible()
                    expect(page.get_by_role('heading',name='Tu panorama') if route=='/' else page.get_by_role('heading',name='Tus movimientos' if route=='/gastos' else 'Cada cosa en su lugar')).to_be_visible()
                    assert page.evaluate('document.documentElement.scrollWidth <= innerWidth'), (width, route)
                    assert page.locator('header').count() == 1 and page.locator('footer').count() == 1
                    assert page.locator('img').evaluate_all('(images) => images.every(i => i.complete && i.naturalWidth > 0)')
                    page.screenshot(path=str(shots/f'{route.strip("/") or "inicio"}-{width}.png'), full_page=True)
            page.goto(UI+'/gastos')
            page.get_by_role('button', name='Eliminar Compra semanal').click()
            expect(page.get_by_text('Aquí aparecerán tus gastos. Empieza con el primero.')).to_be_visible()
            page.goto(UI+'/categorias')
            page.get_by_role('button', name='Eliminar Hogar').click()
            expect(page.get_by_text('Todavía no hay categorías. Crea la primera para comenzar.')).to_be_visible()
            assert not errors, errors
            browser.close()
        print('OK: CRUD real, total, duplicados, categoría en uso, antiforgery, contador, estado, visibilidad, rutas directas, imágenes y móvil; sin errores JavaScript.')
    except Exception:
        log.flush(); print((temp/'server.log').read_text()[-5000:]); raise
    finally:
        if vite is not None:
            vite.terminate(); vite.wait(timeout=10)
        process.terminate()
        try: process.wait(timeout=10)
        except subprocess.TimeoutExpired: process.kill(); process.wait()
        log.close()

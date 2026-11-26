"""Prueba Razor Pages + API real con SQLite y claves temporales, sin tocar datos personales."""
import csv
import io
import html
import json
import os
import pathlib
import shutil
import subprocess
import tempfile
import time
import urllib.error
import urllib.request
from datetime import date
from playwright.sync_api import sync_playwright, expect

ROOT = pathlib.Path(__file__).resolve().parents[1]
API = 'http://127.0.0.1:5080'
WEB = 'http://127.0.0.1:5081'

def wait(url, process):
    for _ in range(150):
        if process.poll() is not None:
            raise RuntimeError('Un proceso terminó antes de iniciar.')
        try:
            with urllib.request.urlopen(url, timeout=1) as r:
                if r.status == 200:
                    return
        except (urllib.error.URLError, TimeoutError):
            time.sleep(.2)
    raise RuntimeError('No respondió ' + url)

with tempfile.TemporaryDirectory(prefix='bolsillo-razor-') as directory:
    temp = pathlib.Path(directory)
    api_root = temp/'Backend'; web_root = temp/'web'
    (temp/'ControlGastos.sln').write_text('fixture de traslado local')
    api_root.mkdir(); web_root.mkdir()
    shutil.copy(ROOT.parent/'Backend/appsettings.json', api_root/'appsettings.json')
    shutil.copy(ROOT/'Web/appsettings.json', web_root/'appsettings.json')
    shutil.copytree(ROOT/'Web/wwwroot', web_root/'wwwroot')
    shutil.copytree(ROOT.parent/'React/wwwroot', temp/'React/wwwroot')
    log = open(temp/'process.log', 'w+')
    env = os.environ | {'ASPNETCORE_ENVIRONMENT':'Development'}
    def start_api():
        return subprocess.Popen(['dotnet', str(ROOT.parent/'Backend/bin/Debug/net10.0/ControlGastos.Api.dll'), '--urls', API, '--contentRoot', str(api_root)], cwd=api_root, env=env, stdout=log, stderr=log)
    backend = start_api()
    frontend = None
    try:
        wait(API+'/api/Gastos', backend)
        frontend = subprocess.Popen(['dotnet', str(ROOT/'Web/bin/Debug/net10.0/ControlGastos.Razor.Web.dll'), '--urls', WEB, '--contentRoot', str(web_root)], cwd=web_root, env=env | {'Api__BaseUrl': API+'/'}, stdout=log, stderr=log)
        wait(WEB, frontend)
        with sync_playwright() as p:
            browser = p.chromium.launch(executable_path=os.environ.get('CHROMIUM_PATH', '/usr/bin/chromium'), headless=True, args=['--no-sandbox'])
            page = browser.new_page(viewport={'width':1440, 'height':1000})
            page.set_default_timeout(10000)
            errors = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            page.goto(WEB)
            expect(page.get_by_role('heading', name='Haz espacio para lo que de verdad importa.')).to_be_visible()
            page.get_by_role('navigation').get_by_role('link', name='Categorías', exact=True).click()
            page.get_by_role('link', name='Crear categoría →').click()
            expect(page.get_by_role('heading', name='Crear categoría')).to_be_visible()
            # Evita la validación del navegador para comprobar la validación del servidor.
            token = page.locator('input[name="__RequestVerificationToken"]').input_value()
            invalid = page.request.post(WEB+'/Categorias/Editar', form={'__RequestVerificationToken':token, 'Input.Nombre':'A', 'Input.Descripcion':'Prueba inválida'})
            assert invalid.status == 200 and 'Usa entre 2 y 80 caracteres.' in invalid.text()
            assert page.request.get(API+'/api/Categorias').json() == []
            page.get_by_label('Nombre', exact=True).fill('Alimentación')
            page.get_by_label('Descripción', exact=True).fill('Compras del hogar')
            page.get_by_role('button', name='Crear categoría').click()
            expect(page.get_by_role('heading', name='Alimentación', exact=True)).to_be_visible()
            page.get_by_role('link', name='Crear categoría →').click()
            page.get_by_label('Nombre', exact=True).fill('Alimentación')
            page.get_by_label('Descripción', exact=True).fill('Duplicada')
            page.get_by_role('button', name='Crear categoría').click()
            expect(page.locator('.notice.error[role="alert"]')).to_contain_text('Ya tienes una categoría')
            expect(page.get_by_label('Nombre', exact=True)).to_have_value('Alimentación')
            page.get_by_role('navigation').get_by_role('link', name='Gastos', exact=True).click()
            page.get_by_role('link', name='Registrar gasto →').click()
            expect(page.get_by_role('heading', name='Registrar gasto')).to_be_visible()
            page.get_by_label('Descripción', exact=True).fill('Compra semanal')
            page.get_by_label('Monto (RD$)').fill('1250.50')
            page.get_by_label('Categoría', exact=True).select_option(label='Alimentación')
            page.get_by_role('button', name='Guardar gasto').click()
            expect(page.get_by_role('cell', name='Compra semanal', exact=True)).to_be_visible()
            page.get_by_role('link', name='Editar Compra semanal').click()
            page.get_by_label('Monto (RD$)').fill('1300.75')
            page.get_by_role('button', name='Guardar cambios').click()
            expect(page.get_by_role('status')).to_contain_text('Gasto guardado.')
            assert page.request.get(API+'/api/Gastos/total').json()['totalGastos'] == 1300.75
            react_page = browser.new_page()
            react_page.on('pageerror', lambda error: errors.append(str(error)))
            react_page.goto(API+'/gastos')
            expect(react_page.get_by_role('cell', name='Compra semanal', exact=True)).to_be_visible()
            expect(react_page.get_by_role('region', name='Resumen de gastos')).to_contain_text('1,300.75')
            react_page.close()
            page.get_by_role('link', name='Compra semanal', exact=True).click()
            expect(page.get_by_role('heading', name='Detalle del gasto')).to_be_visible()
            expect(page.get_by_text('Alimentación', exact=True)).to_be_visible()
            page.get_by_role('navigation').get_by_role('link', name='Resumen', exact=True).click()
            page.get_by_role('button', name='Ocultar importe').click()
            expect(page.locator('[data-total]')).to_have_text('••••••')
            page.get_by_role('button', name='Mostrar importe').click()
            expect(page.locator('[data-total]')).to_contain_text('1,300.75')
            # Persistencia con las mismas claves y base después de reiniciar la API.
            backend.terminate(); backend.wait(timeout=10)
            # Simula la ubicación anterior sin cambiar datos ni claves.
            key_files = {p.name:p.read_bytes() for p in (api_root/'App_Data/keys').glob('*.xml')}
            (api_root/'App_Data/conservar.txt').write_text('archivo local que no debe perderse')
            shutil.move(api_root/'App_Data', temp/'App_Data')
            backend = start_api(); wait(API+'/api/Gastos', backend)
            assert not (temp/'App_Data').exists()
            assert (api_root/'App_Data/conservar.txt').read_text() == 'archivo local que no debe perderse'
            for name, content in key_files.items():
                assert (api_root/'App_Data/keys'/name).read_bytes() == content
            assert list((temp/'local-backups').glob('*/react-demo.db'))
            # Dos bases candidatas deben detener el arranque sin sobrescribir.
            backend.terminate(); backend.wait(timeout=10)
            shutil.copytree(api_root/'App_Data', temp/'App_Data')
            conflict = start_api()
            assert conflict.wait(timeout=15) != 0
            assert (temp/'App_Data/react-demo.db').exists() and (api_root/'App_Data/react-demo.db').exists()
            shutil.rmtree(temp/'App_Data')
            backend = start_api(); wait(API+'/api/Gastos', backend)
            assert page.request.get(API+'/api/Gastos/total').json()['totalGastos'] == 1300.75
            page.get_by_role('navigation').get_by_role('link', name='Categorías', exact=True).click()
            page.once('dialog', lambda dialog: dialog.dismiss())
            page.get_by_role('button', name='Eliminar Alimentación').click()
            expect(page.get_by_role('heading', name='Alimentación', exact=True)).to_be_visible()
            page.once('dialog', lambda dialog: dialog.accept())
            page.get_by_role('button', name='Eliminar Alimentación').click()
            expect(page.locator('.notice.error[role="alert"]')).to_contain_text('Esta categoría tiene gastos')
            page.get_by_role('link', name='Editar Alimentación').click()
            page.get_by_label('Nombre', exact=True).fill('Hogar')
            page.get_by_role('button', name='Guardar cambios').click()
            expect(page.get_by_role('heading', name='Hogar', exact=True)).to_be_visible()
            page.get_by_role('navigation').get_by_role('link', name='Reportes', exact=True).click()
            expect(page.get_by_role('heading', name='Reportes', exact=True)).to_be_visible()
            with page.expect_download() as download_info:
                page.get_by_role('link', name='Descargar CSV').click()
            download = download_info.value
            rows = list(csv.reader(pathlib.Path(download.path()).read_text(encoding='utf-8-sig').splitlines()))
            assert rows[1][1:] == ['Compra semanal', 'Hogar', '1300.75'], rows
            page.get_by_role('navigation').get_by_role('link', name='Presupuesto', exact=True).click()
            page.get_by_label('Presupuesto mensual (RD$)').fill('1000')
            page.get_by_role('button', name='Calcular disponible →').click()
            expect(page.get_by_role('heading', name='Es momento de ajustar.')).to_be_visible()
            expect(page.locator('#disponible')).to_contain_text('-')
            assert page.request.post(API+'/api/Categorias', data={'nombre':'Sin token','descripcion':'No crear'}).status == 400
            assert page.request.post(WEB+'/Categorias/Editar', form={'Input.Nombre':'Sin token','Input.Descripcion':'No crear'}).status == 400
            assert page.request.get(WEB+'/Gastos/Detalle/999999').status == 404
            invalid_month = page.request.get(WEB+'/Reportes?Mes=2026-99')
            assert invalid_month.status == 200 and 'Selecciona un mes válido.' in html.unescape(invalid_month.text())
            # Texto no confiable: Razor lo codifica y el CSV no lo interpreta como fórmula.
            page.goto(WEB+'/Gastos/Editar')
            page.get_by_label('Descripción', exact=True).fill('=HYPERLINK("test")')
            page.get_by_label('Monto (RD$)').fill('1')
            page.get_by_label('Categoría', exact=True).select_option(label='Hogar')
            page.get_by_role('button', name='Guardar gasto').click()
            expect(page.get_by_role('cell', name='=HYPERLINK("test")', exact=True)).to_be_visible()
            csv_response = page.request.get(WEB+'/Reportes?handler=Exportar&Mes='+date.today().strftime('%Y-%m'))
            csv_rows = list(csv.reader(io.StringIO(csv_response.body().decode('utf-8-sig'))))
            assert any(row[1].startswith("'=HYPERLINK") for row in csv_rows[1:]), csv_rows
            shots = ROOT/'tests/artifacts'; shots.mkdir(exist_ok=True)
            for width in [1440,390]:
                page.set_viewport_size({'width':width,'height':1000})
                for route in ['/','/Resumen','/gastos','/categorias','/Reportes','/Presupuesto','/Gastos/Editar','/Categorias/Editar']:
                    page.goto(WEB+route)
                    expect(page.locator('h1')).to_be_visible()
                    assert page.locator('header').count() == 1 and page.locator('footer').count() == 1
                    assert page.evaluate('document.documentElement.scrollWidth <= innerWidth'), (width,route)
                    assert page.locator('img').evaluate_all('(images) => images.every(i => i.complete && i.naturalWidth > 0)')
                    assert not page.locator('main [role="alert"]').filter(has_text='No se pudo').count(), route
                    page.screenshot(path=str(shots/f'{route.strip("/").replace("/","-") or "inicio"}-{width}.png'), full_page=True)
            page.goto(WEB+'/gastos')
            page.on('dialog', lambda dialog: dialog.accept())
            page.get_by_role('button', name='Eliminar Compra semanal').click()
            page.get_by_role('button', name='Eliminar =HYPERLINK("test")', exact=True).click()
            expect(page.get_by_text('Todavía no hay gastos aquí o no coinciden con tu búsqueda.')).to_be_visible()
            page.goto(WEB+'/categorias')
            page.get_by_role('button', name='Eliminar Hogar').click()
            expect(page.get_by_text('Aún no tienes categorías. Crea la primera para empezar.')).to_be_visible()
            # Fallo de API: mensaje útil, sin datos de ejemplo inventados.
            backend.terminate(); backend.wait(timeout=10)
            page.goto(WEB+'/Resumen')
            expect(page.locator('.notice.error[role="alert"]')).to_contain_text('No se pudo conectar con la API')
            assert not errors, errors
            browser.close()
        print('OK Razor: CRUD, detalle, validación servidor, duplicados, categoría en uso, persistencia, CSRF web/API, JavaScript, CSV, presupuesto, error de API, rutas directas y móvil; sin errores JavaScript.')
    except Exception:
        log.flush(); print((temp/'process.log').read_text()[-6000:]); raise
    finally:
        for process in [frontend,backend]:
            if process is not None and process.poll() is None:
                process.terminate()
                try: process.wait(timeout=10)
                except subprocess.TimeoutExpired: process.kill(); process.wait()
        log.close()

"""Prueba Razor Pages + API real con SQLite y claves temporales, sin tocar datos personales."""
import csv
import re
import sqlite3
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

def api_write(request, path, data=None, method="post"):
    token = request.get(API+'/cuenta/antiforgery').json()['token']
    return getattr(request, method)(API+path, data=data, headers={'X-CSRF-TOKEN':token})

def register(page, username, name):
    page.goto(WEB+'/Cuenta/Registro')
    page.get_by_label('Nombre', exact=True).fill(name)
    page.get_by_label('Usuario', exact=True).fill(username)
    page.get_by_label('Contraseña', exact=True).fill('ClavePrueba2026!')
    page.get_by_label('Confirmar contraseña', exact=True).fill('ClavePrueba2026!')
    page.get_by_role('button', name='Crear mi cuenta →').click()
    expect(page.get_by_role('heading', name='Iniciar sesión', exact=True)).to_be_visible()

def login(page, username, password='ClavePrueba2026!'):
    page.get_by_label('Usuario', exact=True).fill(username)
    page.get_by_label('Contraseña', exact=True).fill(password)
    page.get_by_role('button', name='Entrar →').click()

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
            context = browser.new_context(viewport={'width':1440, 'height':1000})
            page = context.new_page()
            page.set_default_timeout(10000)
            errors = []
            page.on('pageerror', lambda error: errors.append(str(error)))
            page.goto(WEB)
            expect(page.get_by_role('heading', name='Haz espacio para lo que de verdad importa.')).to_be_visible()
            assert 'MODO PRUEBA' not in page.content() and 'Sin registro' not in page.content()
            for protected in ['/Resumen','/gastos','/categorias','/Reportes','/Presupuesto','/Gastos/Editar']:
                page.goto(WEB+protected)
                expect(page).to_have_url(re.compile(r'/Cuenta/Login\?ReturnUrl='))
            assert page.request.get(API+'/cuenta/actual').status == 401
            assert page.request.post(API+'/cuenta/registro', data={'displayName':'Sin token','userName':'sintoken','password':'ClavePrueba2026!'}).status == 400
            # Datos previos: se conservan y se vinculan solamente con la aceptación de la primera cuenta.
            legacy_category = api_write(page.request, '/api/Categorias', {'nombre':'Anteriores','descripcion':'Categoría previa'}).json()
            legacy_expense = api_write(page.request, '/api/Gastos', {'descripcion':'Gasto anterior','monto':700.45,'fecha':date.today().isoformat(),'categoriaId':legacy_category['id']}).json()
            register(page, 'titular', 'Cuenta Principal')
            expect(page.get_by_role('status')).to_contain_text('Cuenta creada.')
            login(page, 'titular', 'ContraseñaIncorrecta')
            expect(page.locator('.notice.error')).to_contain_text('Usuario o contraseña incorrectos')
            expect(page.get_by_label('Contraseña', exact=True)).to_have_value('')
            login(page, 'titular')
            expect(page.get_by_role('heading', name='Resumen', exact=True)).to_be_visible()
            expect(page.get_by_text('Gasto anterior', exact=True)).to_be_visible()
            expect(page.get_by_role('button', name='Cerrar sesión')).to_be_visible()
            with sqlite3.connect(api_root/'App_Data/react-demo.db') as db:
                saved_hash = db.execute("SELECT PasswordHash FROM Users WHERE UserName='titular'").fetchone()[0]
                assert saved_hash and saved_hash != 'ClavePrueba2026!'
            cookies = {c['name']: c for c in page.context.cookies()}
            assert cookies['Bolsillo.Razor.Session']['httpOnly'] and cookies['Bolsillo.Api.Session']['httpOnly']
            assert api_write(page.request, '/api/Gastos/'+str(legacy_expense['id']), method='delete').status == 204
            assert api_write(page.request, '/api/Categorias/'+str(legacy_category['id']), method='delete').status == 204
            page.get_by_role('navigation').get_by_role('link', name='Categorías', exact=True).click()
            page.get_by_role('link', name='Crear categoría →').click()
            expect(page.get_by_role('heading', name='Crear categoría')).to_be_visible()
            # Evita la validación del navegador para comprobar la validación del servidor.
            token = page.locator('main input[name="__RequestVerificationToken"]').input_value()
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
            react_page = page.context.new_page()
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
            # Un segundo navegador no comparte la identidad ni puede acceder a registros ajenos.
            own_expense = page.request.get(API+'/api/Gastos').json()[0]
            own_category = page.request.get(API+'/api/Categorias').json()[0]
            other = browser.new_context()
            other_page = other.new_page()
            other_page.set_default_timeout(10000)
            register(other_page, 'vecino', 'Segunda Cuenta')
            login(other_page, 'vecino')
            expect(other_page.get_by_role('heading', name='Resumen', exact=True)).to_be_visible()
            assert other_page.request.get(API+'/api/Gastos').json() == []
            assert other_page.request.get(API+'/api/Gastos/'+str(own_expense['id'])).status == 404
            assert other_page.request.get(WEB+'/Gastos/Editar/'+str(own_expense['id'])).status == 404
            assert api_write(other_page.request, '/api/Gastos/'+str(own_expense['id']), method='delete').status == 404
            assert api_write(other_page.request, '/api/Gastos', {'descripcion':'No permitido','monto':1,'fecha':date.today().isoformat(),'categoriaId':own_category['id']}).status == 400
            private_category = api_write(other_page.request, '/api/Categorias', {'nombre':'Hogar','descripcion':'Espacio de la segunda cuenta'}).json()
            assert api_write(other_page.request, '/api/Gastos', {'descripcion':'Gasto privado de otra cuenta','monto':99,'fecha':date.today().isoformat(),'categoriaId':private_category['id']}).status == 201
            page.goto(WEB+'/Resumen')
            expect(page.locator('[data-total]')).to_contain_text('1,300.75')
            expect(page.get_by_text('Gasto privado de otra cuenta', exact=True)).to_have_count(0)
            other_page.goto(WEB+'/Resumen')
            expect(other_page.locator('[data-total]')).to_contain_text('99.00')
            assert other_page.request.post(WEB+'/Cuenta/Logout').status == 400
            other_page.goto(WEB+'/Cuenta/Logout')
            expect(other_page.get_by_role('heading', name='Resumen', exact=True)).to_be_visible()
            other_page.get_by_role('button', name='Cerrar sesión').click()
            expect(other_page.get_by_role('heading', name='Iniciar sesión', exact=True)).to_be_visible()
            assert other_page.request.get(API+'/cuenta/actual').status == 401
            other_page.goto(WEB+'/Resumen')
            expect(other_page).to_have_url(re.compile(r'/Cuenta/Login\?ReturnUrl='))
            # Cinco fallos bloquean la cuenta; el bloqueo expira y no guarda contraseñas en claro.
            for _ in range(5):
                assert api_write(other_page.request, '/cuenta/login', {'userName':'vecino','password':'Incorrecta'}).status == 401
            assert api_write(other_page.request, '/cuenta/login', {'userName':'vecino','password':'ClavePrueba2026!'}).status == 401
            with sqlite3.connect(api_root/'App_Data/react-demo.db') as db:
                assert db.execute("SELECT FailedAttempts FROM Users WHERE UserName='vecino'").fetchone()[0] == 5
                db.execute("UPDATE Users SET LockedUntilUtc='2000-01-01 00:00:00' WHERE UserName='vecino'")
            login(other_page, 'vecino')
            expect(other_page.get_by_role('heading', name='Resumen', exact=True)).to_be_visible()
            with sqlite3.connect(api_root/'App_Data/react-demo.db') as db:
                db.execute("UPDATE Users SET IsActive=0 WHERE UserName='vecino'")
            other_page.goto(WEB+'/Resumen')
            expect(other_page).to_have_url(re.compile(r'/Cuenta/Login\?ReturnUrl='))
            other.close()
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
            page.get_by_role('button', name='Cerrar sesión').click()
            expect(page.get_by_role('heading', name='Iniciar sesión', exact=True)).to_be_visible()
            page.goto(WEB+'/Cuenta/Login?ReturnUrl=https%3A%2F%2Fexample.com')
            login(page, 'titular')
            expect(page).to_have_url(WEB+'/Resumen')
            # Registro y login se revisan también en móvil sin mostrar avisos de demo.
            auth_context = browser.new_context(viewport={'width':390,'height':900})
            auth_page = auth_context.new_page()
            for route in ['/Cuenta/Login','/Cuenta/Registro']:
                auth_page.goto(WEB+route)
                assert auth_page.evaluate('document.documentElement.scrollWidth <= innerWidth')
                auth_page.screenshot(path=str(shots/(route.rsplit('/',1)[1]+'-390.png')), full_page=True)
            auth_context.close()
            # Fallo de API: mensaje útil, sin datos de ejemplo inventados.
            backend.terminate(); backend.wait(timeout=10)
            page.goto(WEB+'/Resumen')
            expect(page.locator('.notice.error[role="alert"]')).to_contain_text('No se pudo conectar con la API')
            assert not errors, errors
            browser.close()
        print('OK Razor: registro, login, logout, hash, bloqueo, aislamiento de cuentas, datos anteriores, rutas protegidas, retorno local seguro, CRUD, detalle, validación servidor, duplicados, categoría en uso, persistencia, CSRF web/API, JavaScript, CSV, presupuesto, error de API, rutas directas y móvil; sin errores JavaScript.')
    except Exception:
        log.flush(); print((temp/'process.log').read_text()[-6000:]); raise
    finally:
        for process in [frontend,backend]:
            if process is not None and process.poll() is None:
                process.terminate()
                try: process.wait(timeout=10)
                except subprocess.TimeoutExpired: process.kill(); process.wait()
        log.close()

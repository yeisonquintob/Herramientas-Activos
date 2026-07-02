# NAVI Herramientas & Activos / Fenix365

Sistema web y móvil para la administración, trazabilidad, asignación, ubicación, mantenimiento, documentación, toma física, conciliación, reportes e importación de herramientas y activos fijos.

El proyecto nace como una extensión funcional tipo Fenix365 / Dynamics 365, orientada a talleres, almacenes, técnicos, responsables operativos, administradores y equipos de control de activos.

---

## 1. Objetivo del proyecto

NAVI Herramientas & Activos tiene como objetivo centralizar la gestión completa de herramientas y activos fijos dentro de una organización.

El sistema permite controlar:

- Inventario maestro de herramientas y activos fijos.
- Ubicación física y exacta de cada activo.
- Disponibilidad operativa.
- Asignaciones a responsables.
- Préstamos y devoluciones.
- Hoja de vida técnica.
- Documentos y evidencias.
- Mantenimientos.
- Solicitudes de compra.
- Daños y novedades.
- Toma física.
- Conciliación contra información externa.
- Reportes ejecutivos y operativos.
- Seguridad por roles y permisos.
- Sincronización, backup, exportación e importación.
- Evolución futura a multicompañía / multibase de datos.

---

## 2. Estado actual del proyecto

El proyecto cuenta con los siguientes frentes desarrollados o iniciados:

- API .NET 8.
- Admin Web en Blazor Server.
- Mobile PWA en Blazor WebAssembly.
- Base de datos SQL Server en Docker.
- MinIO para documentos y evidencias.
- Swagger para pruebas de API.
- Hangfire para procesos en segundo plano.
- Módulos funcionales para inventario, asignación, documentos, toma física, conciliación, reportes y configuración.
- Módulo inicial de sincronización y exportación.
- Autenticación y permisos en estado MVP.
- Estructura modular para crecer hacia un sistema empresarial.

---

## 3. Tecnologías principales

| Capa | Tecnología |
|---|---|
| Backend | .NET 8 / ASP.NET Core Web API |
| Frontend Admin | Blazor Server |
| Frontend Mobile | Blazor WebAssembly PWA |
| Base de datos | SQL Server 2022 en Docker |
| ORM | Entity Framework Core |
| Documentos | MinIO |
| Jobs | Hangfire |
| API Docs | Swagger / OpenAPI |
| Contenedores | Docker / Docker Compose |
| Lenguaje | C# |
| UI | HTML, CSS, Razor Components |
| Control de versiones | Git / GitHub |

---

## 4. Arquitectura general

El proyecto está organizado por capas:

```text
NAVI Herramientas & Activos
│
├── src/
│   ├── Navi.ToolsAssets.Api
│   ├── Navi.ToolsAssets.Admin
│   ├── Navi.ToolsAssets.MobilePwa
│   ├── Navi.ToolsAssets.Application
│   ├── Navi.ToolsAssets.Domain
│   ├── Navi.ToolsAssets.Infrastructure
│   └── Navi.ToolsAssets.Shared
│
├── docker/
│   └── env/
│
├── backups/
│
├── scripts/
│
├── docs/
│
└── README.md
cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

echo "============================================================"
echo "1. CONGELAR VERSION ACTUAL ESTABLE - NAVI"
echo "============================================================"

STAMP="$(date +%Y%m%d_%H%M%S)"
BRANCH_NAME="$(git branch --show-current)"
TAG_NAME="stable-before-refactor-security-multicompany-${STAMP}"

mkdir -p docs
mkdir -p backups
mkdir -p logs

echo "============================================================"
echo "Asegurando .gitignore"
echo "============================================================"

touch .gitignore

grep -qxF "bin/" .gitignore || echo "bin/" >> .gitignore
grep -qxF "obj/" .gitignore || echo "obj/" >> .gitignore
grep -qxF ".vs/" .gitignore || echo ".vs/" >> .gitignore
grep -qxF ".DS_Store" .gitignore || echo ".DS_Store" >> .gitignore
grep -qxF "backups/" .gitignore || echo "backups/" >> .gitignore
grep -qxF "*.bak" .gitignore || echo "*.bak" >> .gitignore
grep -qxF "*.bacpac" .gitignore || echo "*.bacpac" >> .gitignore
grep -qxF "*.zip" .gitignore || echo "*.zip" >> .gitignore
grep -qxF "*.7z" .gitignore || echo "*.7z" >> .gitignore
grep -qxF "docker/env/local.env" .gitignore || echo "docker/env/local.env" >> .gitignore
grep -qxF ".env" .gitignore || echo ".env" >> .gitignore
grep -qxF "appsettings.Development.json" .gitignore || echo "appsettings.Development.json" >> .gitignore
grep -qxF "appsettings.Local.json" .gitignore || echo "appsettings.Local.json" >> .gitignore
grep -qxF "logs/" .gitignore || echo "logs/" >> .gitignore
grep -qxF "*.log" .gitignore || echo "*.log" >> .gitignore

echo "============================================================"
echo "Generando documento de estado estable"
echo "============================================================"

cat > "docs/ESTADO_VERSION_ESTABLE_${STAMP}.txt" <<EOF
NAVI Herramientas & Activos / Fenix365
Estado congelado antes de limpieza, seguridad y multicompañía

Fecha: $(date)
Branch actual: ${BRANCH_NAME}
Tag sugerido: ${TAG_NAME}

Objetivo de este punto:
- Guardar el estado actual del proyecto.
- Tener un punto de retorno antes de refactorizar.
- Proteger backups y secretos.
- Dejar evidencia de compilación.
- Preparar el proyecto para las fases:
  1. Limpieza y estandarización.
  2. Seguridad.
  3. Multicompañía.
  4. Importación real desde Excel.

Puertos usados:
- API: http://localhost:5218
- Admin Web: http://localhost:5264
- Mobile PWA: http://localhost:5285
- Swagger: http://localhost:5218/swagger
- SQL Server Docker: 1433 / 1434
- MinIO API: 9100
- MinIO Console: 9101

Notas:
- No se deben subir archivos .bak.
- No se deben subir contraseñas.
- No se deben subir docker/env/local.env.
- No se deben subir appsettings locales con secretos.

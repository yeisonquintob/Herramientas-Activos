#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos"
cd "$PROJECT_ROOT"

STAMP="$(date +%Y%m%d_%H%M%S)"
REPORT_DIR="docs/limpieza_codigo_${STAMP}"
LOG_DIR="logs/limpieza_codigo_${STAMP}"

mkdir -p "$REPORT_DIR"
mkdir -p "$LOG_DIR"

echo "============================================================"
echo "LIMPIEZA GENERAL DE CÓDIGO - NAVI"
echo "Fecha: $(date)"
echo "============================================================"

CURRENT_BRANCH="$(git branch --show-current || echo main)"
CLEAN_BRANCH="chore/limpieza-general-codigo-${STAMP}"

echo "Branch actual: $CURRENT_BRANCH"

echo "============================================================"
echo "1. Creando rama de trabajo segura"
echo "============================================================"

if git diff --quiet && git diff --cached --quiet; then
  echo "Working tree limpio."
else
  echo "Hay cambios locales. Se continuará, pero revisa el diff antes de commit."
fi

git checkout -b "$CLEAN_BRANCH"

echo "Rama creada: $CLEAN_BRANCH"

echo "============================================================"
echo "2. Asegurando .gitignore"
echo "============================================================"

touch .gitignore

add_ignore() {
  grep -qxF "$1" .gitignore || echo "$1" >> .gitignore
}

add_ignore "bin/"
add_ignore "obj/"
add_ignore ".vs/"
add_ignore ".vscode/.ropeproject/"
add_ignore ".DS_Store"
add_ignore "Thumbs.db"
add_ignore "backups/"
add_ignore "*.bak"
add_ignore "*.bacpac"
add_ignore "*.zip"
add_ignore "*.7z"
add_ignore "*.rar"
add_ignore "docker/env/local.env"
add_ignore ".env"
add_ignore "appsettings.Development.json"
add_ignore "appsettings.Local.json"
add_ignore "src/**/appsettings.Development.json"
add_ignore "src/**/appsettings.Local.json"
add_ignore "logs/"
add_ignore "*.log"
add_ignore "node_modules/"
add_ignore "dist/"
add_ignore "coverage/"
add_ignore "TestResults/"
add_ignore ".idea/"
add_ignore "*.user"
add_ignore "*.suo"

echo "OK: .gitignore actualizado."

echo "============================================================"
echo "3. Creando/actualizando .editorconfig"
echo "============================================================"

cat > .editorconfig <<'EOF'
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
trim_trailing_whitespace = true
indent_style = space
indent_size = 4

[*.{cs,cshtml,razor}]
indent_size = 4

[*.{json,yml,yaml,html,css,js,ts,md,txt}]
indent_size = 2

[*.md]
trim_trailing_whitespace = false

[*.cs]
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false

dotnet_style_qualification_for_field = false:suggestion
dotnet_style_qualification_for_property = false:suggestion
dotnet_style_qualification_for_method = false:suggestion
dotnet_style_qualification_for_event = false:suggestion

dotnet_style_predefined_type_for_locals_parameters_members = true:suggestion
dotnet_style_predefined_type_for_member_access = true:suggestion

dotnet_style_object_initializer = true:suggestion
dotnet_style_collection_initializer = true:suggestion
dotnet_style_coalesce_expression = true:suggestion
dotnet_style_null_propagation = true:suggestion

csharp_style_var_for_built_in_types = false:suggestion
csharp_style_var_when_type_is_apparent = true:suggestion
csharp_style_var_elsewhere = false:suggestion

csharp_style_expression_bodied_methods = false:suggestion
csharp_style_expression_bodied_properties = true:suggestion
csharp_style_expression_bodied_accessors = true:suggestion

csharp_prefer_braces = true:suggestion
csharp_new_line_before_open_brace = all
EOF

echo "OK: .editorconfig actualizado."

echo "============================================================"
echo "4. Limpiando bin/obj con dotnet clean"
echo "============================================================"

dotnet clean -c Debug > "$LOG_DIR/dotnet_clean_debug.log" 2>&1 || true
dotnet clean -c Release > "$LOG_DIR/dotnet_clean_release.log" 2>&1 || true

echo "============================================================"
echo "5. Eliminando carpetas bin y obj"
echo "============================================================"

find . \
  -path "./.git" -prune -o \
  -type d \( -name "bin" -o -name "obj" \) \
  -print > "$REPORT_DIR/bin_obj_eliminados.txt"

while IFS= read -r dir; do
  if [ -n "$dir" ]; then
    rm -rf "$dir"
  fi
done < "$REPORT_DIR/bin_obj_eliminados.txt"

echo "OK: bin/obj eliminados."

echo "============================================================"
echo "6. Normalizando finales de línea y espacios básicos"
echo "============================================================"

python3 <<'PY'
from pathlib import Path

root = Path(".").resolve()

extensions = {
    ".cs", ".razor", ".cshtml", ".json", ".yml", ".yaml",
    ".html", ".css", ".js", ".ts", ".md", ".txt", ".xml",
    ".props", ".targets", ".sln", ".csproj"
}

skip_parts = {
    ".git", "bin", "obj", "backups", "node_modules",
    ".vs", "logs", "TestResults"
}

changed = []

for path in root.rglob("*"):
    if not path.is_file():
        continue

    if any(part in skip_parts for part in path.parts):
        continue

    if path.suffix.lower() not in extensions:
        continue

    try:
        raw = path.read_bytes()
    except Exception:
        continue

    try:
        text = raw.decode("utf-8-sig")
    except UnicodeDecodeError:
        continue

    original = text

    text = text.replace("\r\n", "\n").replace("\r", "\n")
    lines = [line.rstrip() for line in text.split("\n")]
    text = "\n".join(lines).rstrip() + "\n"

    if text != original:
        path.write_text(text, encoding="utf-8")
        changed.append(str(path.relative_to(root)))

report = root / "docs" / "limpieza_codigo_files_normalized.txt"
report.write_text("\n".join(changed) + ("\n" if changed else ""), encoding="utf-8")

print(f"Archivos normalizados: {len(changed)}")
PY

mv docs/limpieza_codigo_files_normalized.txt "$REPORT_DIR/files_normalized.txt" 2>/dev/null || true

echo "============================================================"
echo "7. Ejecutando dotnet format"
echo "============================================================"

SOLUTION_FILE="$(find . -maxdepth 2 -name "*.sln" | head -1 || true)"

if [ -n "$SOLUTION_FILE" ]; then
  echo "Solución detectada: $SOLUTION_FILE"
  dotnet format "$SOLUTION_FILE" --verbosity minimal > "$LOG_DIR/dotnet_format.log" 2>&1 || true
else
  echo "No se encontró .sln. Ejecutando dotnet format sobre el directorio actual."
  dotnet format --verbosity minimal > "$LOG_DIR/dotnet_format.log" 2>&1 || true
fi

echo "============================================================"
echo "8. Generando reporte de limpieza técnica"
echo "============================================================"

python3 <<'PY'
from pathlib import Path
import re
from collections import defaultdict

root = Path(".").resolve()
report_dir = sorted((root / "docs").glob("limpieza_codigo_*"))[-1]

skip_parts = {".git", "bin", "obj", "backups", "node_modules", ".vs", "logs", "TestResults"}

def should_skip(path: Path) -> bool:
    return any(part in skip_parts for part in path.parts)

files = [p for p in root.rglob("*") if p.is_file() and not should_skip(p)]

cs_files = [p for p in files if p.suffix.lower() == ".cs"]
razor_files = [p for p in files if p.suffix.lower() == ".razor"]
csproj_files = [p for p in files if p.suffix.lower() == ".csproj"]

# Conteo general
summary = []
summary.append("NAVI Herramientas & Activos - Reporte de limpieza general de código")
summary.append("")
summary.append(f"Total archivos revisados: {len(files)}")
summary.append(f"Archivos .cs: {len(cs_files)}")
summary.append(f"Archivos .razor: {len(razor_files)}")
summary.append(f"Archivos .csproj: {len(csproj_files)}")
summary.append("")

# Archivos grandes
large = []
for p in files:
    try:
        size = p.stat().st_size
    except Exception:
        continue
    if size >= 50_000:
        large.append((size, p))

large.sort(reverse=True)
summary.append("ARCHIVOS GRANDES >= 50 KB")
for size, p in large[:80]:
    summary.append(f"{size:>10} bytes  {p.relative_to(root)}")
summary.append("")

# Razor con muchas lineas
summary.append("COMPONENTES RAZOR GRANDES >= 500 LINEAS")
for p in razor_files:
    try:
        line_count = len(p.read_text(encoding="utf-8", errors="ignore").splitlines())
    except Exception:
        continue
    if line_count >= 500:
        summary.append(f"{line_count:>6} lineas  {p.relative_to(root)}")
summary.append("")

# Controladores grandes
summary.append("CONTROLADORES GRANDES >= 500 LINEAS")
for p in cs_files:
    if "Controller" not in p.name:
        continue
    try:
        line_count = len(p.read_text(encoding="utf-8", errors="ignore").splitlines())
    except Exception:
        continue
    if line_count >= 500:
        summary.append(f"{line_count:>6} lineas  {p.relative_to(root)}")
summary.append("")

# Rutas Razor duplicadas
routes = defaultdict(list)
route_pattern = re.compile(r'@page\s+"([^"]+)"')
for p in razor_files:
    try:
        text = p.read_text(encoding="utf-8", errors="ignore")
    except Exception:
        continue
    for route in route_pattern.findall(text):
        routes[route].append(str(p.relative_to(root)))

summary.append("RUTAS RAZOR DUPLICADAS")
duplicates = {k:v for k,v in routes.items() if len(v) > 1}
if duplicates:
    for route, paths in sorted(duplicates.items()):
        summary.append(f"Ruta duplicada: {route}")
        for path in paths:
            summary.append(f"  - {path}")
else:
    summary.append("No se detectaron rutas Razor duplicadas.")
summary.append("")

# Endpoints controller sin RequirePermission aparente
summary.append("CONTROLADORES API SIN RequirePermission DETECTABLE")
for p in cs_files:
    if "Controller" not in p.name:
        continue
    rel = str(p.relative_to(root))
    if "/Navi.ToolsAssets.Api/" not in "/" + rel:
        continue

    text = p.read_text(encoding="utf-8", errors="ignore")
    has_route = "[Route(" in text
    has_permission = "RequirePermission" in text

    if has_route and not has_permission:
        summary.append(f"- {rel}")
summary.append("")

# TODO, FIXME, HACK
summary.append("MARCADORES TODO / FIXME / HACK")
markers = []
for p in files:
    if p.suffix.lower() not in {".cs", ".razor", ".cshtml", ".json", ".md", ".txt", ".css", ".js", ".ts"}:
        continue
    text = p.read_text(encoding="utf-8", errors="ignore")
    for idx, line in enumerate(text.splitlines(), start=1):
        upper = line.upper()
        if "TODO" in upper or "FIXME" in upper or "HACK" in upper:
            markers.append(f"{p.relative_to(root)}:{idx}: {line.strip()}")

if markers:
    summary.extend(markers[:300])
else:
    summary.append("No se detectaron marcadores.")
summary.append("")

# Posibles secretos
summary.append("POSIBLES SECRETOS A REVISAR")
secret_patterns = [
    "Password=", "MSSQL_SA_PASSWORD", "SecretKey", "AccessKey",
    "MINIO_SECRET", "NAVI_BACKUP_ADMIN_PASSWORD", "ConnectionStrings"
]

secret_hits = []
for p in files:
    if p.suffix.lower() not in {".cs", ".json", ".config", ".xml", ".txt", ".env", ".yml", ".yaml"}:
        continue

    rel = str(p.relative_to(root))

    if rel.startswith("docs/limpieza_codigo_"):
        continue

    text = p.read_text(encoding="utf-8", errors="ignore")
    for idx, line in enumerate(text.splitlines(), start=1):
        if any(pattern in line for pattern in secret_patterns):
            secret_hits.append(f"{rel}:{idx}: {line.strip()}")

if secret_hits:
    summary.extend(secret_hits[:300])
else:
    summary.append("No se detectaron coincidencias simples.")
summary.append("")

# Recomendaciones
summary.append("RECOMENDACIONES DE LIMPIEZA")
summary.append("1. Dividir componentes Razor grandes en subcomponentes.")
summary.append("2. Dividir controladores grandes en servicios de aplicación.")
summary.append("3. Proteger controladores API sin RequirePermission o autenticación real.")
summary.append("4. Eliminar rutas duplicadas.")
summary.append("5. Mover secretos a variables de entorno.")
summary.append("6. Unificar DTOs repetidos.")
summary.append("7. Crear componentes visuales reutilizables.")
summary.append("8. Separar lógica de UI y lógica de negocio.")
summary.append("9. Mantener importación y backup en servicios separados.")
summary.append("10. Preparar seguridad antes de multicompañía.")

(report_dir / "REPORTE_LIMPIEZA_CODIGO.txt").write_text("\n".join(summary), encoding="utf-8")

print(report_dir / "REPORTE_LIMPIEZA_CODIGO.txt")
PY

echo "============================================================"
echo "9. Creando plan de refactor posterior"
echo "============================================================"

cat > "$REPORT_DIR/PLAN_REFACTOR_POSTERIOR.txt" <<'EOF'
NAVI Herramientas & Activos
Plan posterior a limpieza general de código

1. Dividir componentes Razor grandes
2. Dividir controladores API grandes
3. Crear servicios de aplicación por módulo
4. Estandarizar DTOs
5. Estandarizar respuestas API
6. Crear manejo centralizado de errores
7. Crear validadores por request
8. Crear componentes UI reutilizables
9. Estandarizar estilos Admin
10. Estandarizar estilos Mobile
11. Revisar endpoints sin permiso
12. Reemplazar seguridad basada en headers
13. Implementar autenticación real
14. Implementar multicompañía
15. Crear importador Excel controlado
EOF

echo "============================================================"
echo "10. Restaurando paquetes"
echo "============================================================"

dotnet restore > "$LOG_DIR/dotnet_restore_after_cleanup.log" 2>&1

echo "============================================================"
echo "11. Compilando solución después de limpieza"
echo "============================================================"

set +e
dotnet build -c Debug > "$LOG_DIR/dotnet_build_after_cleanup.log" 2>&1
BUILD_EXIT=$?
set -e

if [ "$BUILD_EXIT" -ne 0 ]; then
  echo "============================================================"
  echo "ATENCIÓN: La compilación falló después de la limpieza"
  echo "============================================================"
  echo "Revisa:"
  echo "$LOG_DIR/dotnet_build_after_cleanup.log"
  echo ""
  echo "Últimas líneas del error:"
  tail -80 "$LOG_DIR/dotnet_build_after_cleanup.log"

  cat > "$REPORT_DIR/RESULTADO_LIMPIEZA.txt" <<EOF
Resultado: COMPILACION FALLIDA
Fecha: $(date)
Branch: $CLEAN_BRANCH
Log: $LOG_DIR/dotnet_build_after_cleanup.log

Revisar errores antes de continuar.
EOF

  exit 1
fi

echo "OK: compilación correcta."

cat > "$REPORT_DIR/RESULTADO_LIMPIEZA.txt" <<EOF
Resultado: LIMPIEZA COMPLETADA Y COMPILACION CORRECTA
Fecha: $(date)
Branch: $CLEAN_BRANCH

Archivos generados:
- $REPORT_DIR/REPORTE_LIMPIEZA_CODIGO.txt
- $REPORT_DIR/PLAN_REFACTOR_POSTERIOR.txt
- $REPORT_DIR/RESULTADO_LIMPIEZA.txt

Logs:
- $LOG_DIR/dotnet_clean_debug.log
- $LOG_DIR/dotnet_clean_release.log
- $LOG_DIR/dotnet_format.log
- $LOG_DIR/dotnet_restore_after_cleanup.log
- $LOG_DIR/dotnet_build_after_cleanup.log
EOF

echo "============================================================"
echo "12. Estado Git"
echo "============================================================"

git status --short

echo ""
echo "============================================================"
echo "LIMPIEZA GENERAL COMPLETADA"
echo "============================================================"
echo "Reporte:"
echo "$REPORT_DIR/REPORTE_LIMPIEZA_CODIGO.txt"
echo ""
echo "Resultado:"
echo "$REPORT_DIR/RESULTADO_LIMPIEZA.txt"
echo ""
echo "Branch:"
echo "$CLEAN_BRANCH"
echo ""
echo "Para revisar cambios:"
echo "git diff --stat"
echo "git diff"
echo ""
echo "Si todo está bien:"
echo "git add ."
echo "git commit -m \"Limpieza general de código y formato base\""
echo "git push -u origin $CLEAN_BRANCH"

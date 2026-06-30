#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos"
STAMP="$(date +%Y%m%d_%H%M%S)"
OUT_DIR="$PROJECT_ROOT/export_codigo"
OUT_FILE="$OUT_DIR/PROYECTO_CODIGO_COMPLETO_$STAMP.txt"

cd "$PROJECT_ROOT"
mkdir -p "$OUT_DIR"

echo "============================================================"
echo "Exportando código completo del proyecto NAVI"
echo "Salida: $OUT_FILE"
echo "============================================================"

cat > "$OUT_FILE" <<HEADER
################################################################################
PROYECTO: NAVI Herramientas & Activos / Fenix365
FECHA EXPORTACIÓN: $(date)
RUTA PROYECTO: $PROJECT_ROOT
ARCHIVO GENERADO: $OUT_FILE
################################################################################

IMPORTANTE:
- Se excluyen carpetas pesadas o generadas: bin, obj, .git, node_modules, backups, export_codigo, etc.
- Se excluyen binarios: .bak, .zip, .png, .jpg, .pdf, .dll, .exe, etc.
- Se enmascaran posibles secretos: passwords, tokens, keys, secrets y connection strings.

################################################################################
ESTRUCTURA DEL PROYECTO
################################################################################

HEADER

find . \
  -path "./.git" -prune -o \
  -path "./bin" -prune -o \
  -path "./obj" -prune -o \
  -path "./.vs" -prune -o \
  -path "./.idea" -prune -o \
  -path "./node_modules" -prune -o \
  -path "./export_codigo" -prune -o \
  -path "./backups" -prune -o \
  -path "./backups_*" -prune -o \
  -path "./backups_patch_*" -prune -o \
  -path "./backups_fix_*" -prune -o \
  -path "./backups_vistas_*" -prune -o \
  -path "./backups_flujo_*" -prune -o \
  -path "./backups_estado_*" -prune -o \
  -path "./backups_ajustes_*" -prune -o \
  -path "./TestResults" -prune -o \
  -path "./coverage" -prune -o \
  -path "./publish" -prune -o \
  -type f \
  ! -name "*.bak" \
  ! -name "*.zip" \
  ! -name "*.7z" \
  ! -name "*.rar" \
  ! -name "*.tar" \
  ! -name "*.gz" \
  ! -name "*.png" \
  ! -name "*.jpg" \
  ! -name "*.jpeg" \
  ! -name "*.gif" \
  ! -name "*.webp" \
  ! -name "*.ico" \
  ! -name "*.pdf" \
  ! -name "*.docx" \
  ! -name "*.xlsx" \
  ! -name "*.pptx" \
  ! -name "*.dll" \
  ! -name "*.exe" \
  ! -name "*.pdb" \
  ! -name "*.cache" \
  ! -name "*.db" \
  ! -name "*.sqlite" \
  ! -name "*.mdf" \
  ! -name "*.ldf" \
  -print | sort >> "$OUT_FILE"

cat >> "$OUT_FILE" <<'SECTION'

################################################################################
INFORMACIÓN GIT
################################################################################

SECTION

{
  echo "Branch actual:"
  git branch --show-current 2>/dev/null || true
  echo ""
  echo "Estado:"
  git status --short 2>/dev/null || true
  echo ""
  echo "Últimos commits:"
  git log --oneline -10 2>/dev/null || true
} >> "$OUT_FILE"

cat >> "$OUT_FILE" <<'SECTION'

################################################################################
CONTENIDO DE ARCHIVOS
################################################################################

SECTION

should_include_file() {
  local file="$1"

  case "$file" in
    *.cs|*.razor|*.cshtml|*.csproj|*.sln|*.json|*.xml|*.config|*.props|*.targets|*.yml|*.yaml|*.sql|*.md|*.txt|*.html|*.css|*.js|*.ts|*.ps1|*.sh|*.env|Dockerfile|dockerfile|*.dockerfile)
      return 0
      ;;
    *)
      return 1
      ;;
  esac
}

mask_secrets() {
  python3 - "$1" <<'PY'
import re
import sys
from pathlib import Path

path = Path(sys.argv[1])

try:
    text = path.read_text(encoding="utf-8", errors="replace")
except Exception as exc:
    print(f"[NO SE PUDO LEER: {exc}]")
    sys.exit(0)

patterns = [
    r'(?i)(password\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(pwd\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(secret\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(token\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(apikey\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(api_key\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(accesskey\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(access_key\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(secretkey\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(secret_key\s*[:=]\s*)(".*?"|\'.*?\'|[^,\s]+)',
    r'(?i)(connectionstrings?\s*[:=]\s*)(".*?"|\'.*?\'|.+)',
    r'(?i)(connectionstring\s*[:=]\s*)(".*?"|\'.*?\'|.+)',
    r'(?i)(Server=.*?;.*?Password=)(.*?)(;)',
    r'(?i)(User Id=.*?;.*?Password=)(.*?)(;)',
]

for pattern in patterns:
    text = re.sub(pattern, lambda m: m.group(1) + "***OCULTO***" + (m.group(3) if len(m.groups()) >= 3 else ""), text)

print(text)
PY
}

while IFS= read -r file; do
  clean_file="${file#./}"

  if should_include_file "$clean_file"; then
    {
      echo ""
      echo ""
      echo "################################################################################"
      echo "ARCHIVO: $clean_file"
      echo "TAMAÑO: $(wc -c < "$file" | tr -d ' ') bytes"
      echo "################################################################################"
      echo ""
    } >> "$OUT_FILE"

    mask_secrets "$file" >> "$OUT_FILE"
  fi
done < <(
  find . \
    -path "./.git" -prune -o \
    -path "./bin" -prune -o \
    -path "./obj" -prune -o \
    -path "./.vs" -prune -o \
    -path "./.idea" -prune -o \
    -path "./node_modules" -prune -o \
    -path "./export_codigo" -prune -o \
    -path "./backups" -prune -o \
    -path "./backups_*" -prune -o \
    -path "./TestResults" -prune -o \
    -path "./coverage" -prune -o \
    -path "./publish" -prune -o \
    -type f \
    ! -name "*.bak" \
    ! -name "*.zip" \
    ! -name "*.7z" \
    ! -name "*.rar" \
    ! -name "*.tar" \
    ! -name "*.gz" \
    ! -name "*.png" \
    ! -name "*.jpg" \
    ! -name "*.jpeg" \
    ! -name "*.gif" \
    ! -name "*.webp" \
    ! -name "*.ico" \
    ! -name "*.pdf" \
    ! -name "*.docx" \
    ! -name "*.xlsx" \
    ! -name "*.pptx" \
    ! -name "*.dll" \
    ! -name "*.exe" \
    ! -name "*.pdb" \
    ! -name "*.cache" \
    ! -name "*.db" \
    ! -name "*.sqlite" \
    ! -name "*.mdf" \
    ! -name "*.ldf" \
    -print | sort
)

echo ""
echo "============================================================"
echo "EXPORTACIÓN COMPLETADA"
echo "Archivo generado:"
echo "$OUT_FILE"
echo "============================================================"
echo ""
echo "Tamaño:"
ls -lh "$OUT_FILE"

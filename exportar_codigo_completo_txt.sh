#!/bin/zsh

set -e

ROOT_DIR="$(pwd)"
EXPORT_DIR="$ROOT_DIR/export_codigo"
STAMP="$(date +%Y%m%d_%H%M%S)"
OUT_FILE="$EXPORT_DIR/PROYECTO_CODIGO_COMPLETO_${STAMP}.txt"

mkdir -p "$EXPORT_DIR"

echo "============================================================"
echo "EXPORTANDO CÓDIGO COMPLETO DEL PROYECTO NAVI"
echo "============================================================"
echo "Ruta proyecto: $ROOT_DIR"
echo "Carpeta salida: $EXPORT_DIR"
echo "Archivo salida: $OUT_FILE"
echo ""

{
  echo "################################################################################"
  echo "PROYECTO NAVI HERRAMIENTAS Y ACTIVOS"
  echo "EXPORTADO: $(date)"
  echo "RUTA: $ROOT_DIR"
  echo "CARPETA EXPORT: $EXPORT_DIR"
  echo "################################################################################"
  echo ""

  echo "================================================================================"
  echo "1. PROYECTOS INCLUIDOS"
  echo "================================================================================"
  echo ""
  echo "- API: src/Navi.ToolsAssets.Api"
  echo "- Admin Web: src/Navi.ToolsAssets.Admin"
  echo "- Mobile PWA: src/Navi.ToolsAssets.MobilePwa"
  echo "- Application: src/Navi.ToolsAssets.Application"
  echo "- Domain: src/Navi.ToolsAssets.Domain"
  echo "- Infrastructure: src/Navi.ToolsAssets.Infrastructure"
  echo "- Shared: src/Navi.ToolsAssets.Shared"
  echo "- Worker: src/Navi.ToolsAssets.Worker"
  echo "- Scripts, configuración, documentación y SQL del proyecto"
  echo ""

  echo "================================================================================"
  echo "2. ESTRUCTURA DEL PROYECTO"
  echo "================================================================================"
  echo ""

  find . \
    \( \
      -path "./.git" -o \
      -path "./.git/*" -o \
      -path "*/bin" -o \
      -path "*/bin/*" -o \
      -path "*/obj" -o \
      -path "*/obj/*" -o \
      -path "./backups" -o \
      -path "./backups/*" -o \
      -path "./backup" -o \
      -path "./backup/*" -o \
      -path "./local-backups" -o \
      -path "./local-backups/*" -o \
      -path "./tmp" -o \
      -path "./tmp/*" -o \
      -path "./temp" -o \
      -path "./temp/*" -o \
      -path "./export_codigo" -o \
      -path "./export_codigo/*" \
    \) -prune -o \
    -print \
    | sort \
    | sed 's#^\./##' \
    | while IFS= read -r ITEM; do
        if [ -n "$ITEM" ]; then
          echo "$ITEM"
        fi
      done

  echo ""
  echo "================================================================================"
  echo "3. ARCHIVOS DE CÓDIGO Y CONFIGURACIÓN"
  echo "================================================================================"
  echo ""

  find . \
    \( \
      -path "./.git" -o \
      -path "./.git/*" -o \
      -path "*/bin" -o \
      -path "*/bin/*" -o \
      -path "*/obj" -o \
      -path "*/obj/*" -o \
      -path "./backups" -o \
      -path "./backups/*" -o \
      -path "./backup" -o \
      -path "./backup/*" -o \
      -path "./local-backups" -o \
      -path "./local-backups/*" -o \
      -path "./tmp" -o \
      -path "./tmp/*" -o \
      -path "./temp" -o \
      -path "./temp/*" -o \
      -path "./export_codigo" -o \
      -path "./export_codigo/*" \
    \) -prune -o \
    \( \
      -type f \
      \( \
        -name "*.sln" -o \
        -name "*.csproj" -o \
        -name "*.props" -o \
        -name "*.targets" -o \
        -name "*.cs" -o \
        -name "*.razor" -o \
        -name "*.cshtml" -o \
        -name "*.css" -o \
        -name "*.scss" -o \
        -name "*.html" -o \
        -name "*.js" -o \
        -name "*.ts" -o \
        -name "*.json" -o \
        -name "*.xml" -o \
        -name "*.config" -o \
        -name "*.md" -o \
        -name "*.txt" -o \
        -name "*.sql" -o \
        -name "*.sh" -o \
        -name "Dockerfile" -o \
        -name "docker-compose.yml" -o \
        -name "docker-compose.yaml" \
      \) \
      ! -name "*.bak" \
      ! -name "*.bak_*" \
      ! -name "*.before_*" \
      ! -name "*.old" \
      ! -name "*.tmp" \
      ! -name "*.temp" \
      ! -name "*.log" \
      ! -name ".DS_Store" \
      ! -name "PROYECTO_CODIGO_COMPLETO_*.txt" \
    \) -print \
    | sort \
    | while IFS= read -r FILE; do
        if [ -f "$FILE" ]; then
          echo ""
          echo "################################################################################"
          echo "ARCHIVO: ${FILE#./}"
          echo "TAMAÑO: $(wc -c < "$FILE" | tr -d ' ') bytes"
          echo "################################################################################"
          echo ""
          cat "$FILE"
          echo ""
        fi
      done

  echo ""
  echo "################################################################################"
  echo "FIN DEL EXPORT COMPLETO"
  echo "################################################################################"

} > "$OUT_FILE"

echo ""
echo "Exportación finalizada correctamente:"
echo "$OUT_FILE"

echo ""
echo "Tamaño del archivo:"
ls -lh "$OUT_FILE"

echo ""
echo "Total de líneas:"
wc -l "$OUT_FILE"

echo ""
echo "Validación rápida de contenido:"
echo "API:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Api" "$OUT_FILE" || true

echo "Admin Web:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Admin" "$OUT_FILE" || true

echo "Mobile PWA:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.MobilePwa" "$OUT_FILE" || true

echo "Application:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Application" "$OUT_FILE" || true

echo "Domain:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Domain" "$OUT_FILE" || true

echo "Infrastructure:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Infrastructure" "$OUT_FILE" || true

echo "Shared:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Shared" "$OUT_FILE" || true

echo "Worker:"
grep -c "ARCHIVO: src/Navi.ToolsAssets.Worker" "$OUT_FILE" || true

echo ""
echo "Carpeta donde quedó guardado:"
echo "$EXPORT_DIR"

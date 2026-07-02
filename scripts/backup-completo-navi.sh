#!/usr/bin/env bash
set -euo pipefail

PROJECT_ROOT="/Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos"
cd "$PROJECT_ROOT"

STAMP="$(date +%Y%m%d_%H%M%S)"
BACKUP_ROOT="backups/full_${STAMP}"
PROJECT_BACKUP_DIR="${BACKUP_ROOT}/project"
DATABASE_BACKUP_DIR="${BACKUP_ROOT}/database"
SECURITY_BACKUP_DIR="${BACKUP_ROOT}/sensitive_encrypted"
LOG_DIR="${BACKUP_ROOT}/logs"

DB_CONTAINER="${DB_CONTAINER:-navi-tools-sqlserver}"
DB_NAME="${DB_NAME:-NaviToolsAssetsDb}"
SQL_CONTAINER_BACKUP_DIR="${SQL_CONTAINER_BACKUP_DIR:-/var/opt/mssql/backups}"

mkdir -p "$PROJECT_BACKUP_DIR"
mkdir -p "$DATABASE_BACKUP_DIR"
mkdir -p "$SECURITY_BACKUP_DIR"
mkdir -p "$LOG_DIR"

echo "============================================================"
echo "Backup root:"
echo "$BACKUP_ROOT"
echo "============================================================"

# ============================================================
# 1. Validar herramientas base
# ============================================================

echo "============================================================"
echo "1. Validando herramientas"
echo "============================================================"

command -v git >/dev/null 2>&1 || { echo "ERROR: git no está instalado."; exit 1; }
command -v tar >/dev/null 2>&1 || { echo "ERROR: tar no está instalado."; exit 1; }
command -v shasum >/dev/null 2>&1 || { echo "ERROR: shasum no está disponible."; exit 1; }
command -v docker >/dev/null 2>&1 || { echo "ERROR: docker no está instalado."; exit 1; }

echo "OK: herramientas base disponibles."

# ============================================================
# 2. Crear manifiesto inicial
# ============================================================

MANIFEST="${BACKUP_ROOT}/MANIFEST_BACKUP_${STAMP}.txt"

cat > "$MANIFEST" <<EOF
NAVI Herramientas & Activos / Fenix365
Backup completo del proyecto y base de datos

Fecha: $(date)
Timestamp: ${STAMP}
Ruta proyecto: ${PROJECT_ROOT}
Base de datos: ${DB_NAME}
Contenedor SQL esperado: ${DB_CONTAINER}

Contenido esperado:
- Código fuente comprimido.
- Git bundle con historial del repositorio.
- Estado Git.
- Logs de compilación.
- Backup SQL Server .bak.
- Verificación RESTORE VERIFYONLY.
- Resumen de tablas y conteos.
- Archivos sensibles cifrados si existen.
- Checksums SHA256.

IMPORTANTE:
- Los archivos .bak no deben subirse a GitHub.
- Los archivos sensibles se cifran y no se guardan en texto plano dentro del backup seguro.
EOF

# ============================================================
# 3. Guardar estado Git
# ============================================================

echo "============================================================"
echo "2. Guardando estado Git"
echo "============================================================"

git status > "${LOG_DIR}/git_status_${STAMP}.txt" || true
git branch --show-current > "${LOG_DIR}/git_branch_${STAMP}.txt" || true
git log --oneline -30 > "${LOG_DIR}/git_log_last_30_${STAMP}.txt" || true
git remote -v > "${LOG_DIR}/git_remotes_${STAMP}.txt" || true

echo "OK: estado Git guardado."

# ============================================================
# 4. Crear backup del historial Git
# ============================================================

echo "============================================================"
echo "3. Creando Git bundle"
echo "============================================================"

GIT_BUNDLE="${PROJECT_BACKUP_DIR}/NAVI_git_bundle_${STAMP}.bundle"

git bundle create "$GIT_BUNDLE" --all

echo "OK: Git bundle creado:"
ls -lh "$GIT_BUNDLE"

# ============================================================
# 5. Crear backup del código fuente sin basura ni backups pesados
# ============================================================

echo "============================================================"
echo "4. Creando backup del código fuente"
echo "============================================================"

SOURCE_TAR="${PROJECT_BACKUP_DIR}/NAVI_source_${STAMP}.tar.gz"

tar \
  --exclude="./.git" \
  --exclude="./bin" \
  --exclude="./obj" \
  --exclude="./.vs" \
  --exclude="./backups" \
  --exclude="./logs" \
  --exclude="./export_codigo" \
  --exclude="./node_modules" \
  --exclude="./storage/database-backups" \
  --exclude="./*.bak" \
  --exclude="./*.bacpac" \
  --exclude="./*.zip" \
  --exclude="./*.7z" \
  --exclude="./*.log" \
  --exclude="./.DS_Store" \
  --exclude="./docker/env/local.env" \
  --exclude="./.env" \
  --exclude="./appsettings.Development.json" \
  --exclude="./appsettings.Local.json" \
  --exclude="./src/**/appsettings.Development.json" \
  --exclude="./src/**/appsettings.Local.json" \
  -czf "$SOURCE_TAR" .

echo "OK: backup de código creado:"
ls -lh "$SOURCE_TAR"

# ============================================================
# 6. Crear listado de estructura del proyecto
# ============================================================

echo "============================================================"
echo "5. Guardando estructura del proyecto"
echo "============================================================"

find . \
  -path "./.git" -prune -o \
  -path "./bin" -prune -o \
  -path "./obj" -prune -o \
  -path "./backups" -prune -o \
  -path "./logs" -prune -o \
  -type f \
  -print | sort > "${PROJECT_BACKUP_DIR}/NAVI_file_tree_${STAMP}.txt"

echo "OK: estructura guardada."

# ============================================================
# 7. Compilación para evidencia
# ============================================================

echo "============================================================"
echo "6. Compilando solución para evidencia"
echo "============================================================"

set +e
dotnet restore > "${LOG_DIR}/dotnet_restore_${STAMP}.log" 2>&1
RESTORE_EXIT=$?

dotnet build -c Debug > "${LOG_DIR}/dotnet_build_${STAMP}.log" 2>&1
BUILD_EXIT=$?
set -e

if [ "$RESTORE_EXIT" -eq 0 ]; then
  echo "OK: restore correcto."
else
  echo "ADVERTENCIA: dotnet restore falló. Revisar log."
fi

if [ "$BUILD_EXIT" -eq 0 ]; then
  echo "OK: build correcto."
else
  echo "ADVERTENCIA: dotnet build falló. Revisar log."
fi

cat >> "$MANIFEST" <<EOF

Resultado dotnet restore: ${RESTORE_EXIT}
Resultado dotnet build: ${BUILD_EXIT}
Logs:
- ${LOG_DIR}/dotnet_restore_${STAMP}.log
- ${LOG_DIR}/dotnet_build_${STAMP}.log
EOF

# ============================================================
# 8. Backup cifrado de archivos sensibles locales
# ============================================================

echo "============================================================"
echo "7. Backup cifrado de archivos sensibles"
echo "============================================================"

SENSITIVE_LIST="${SECURITY_BACKUP_DIR}/sensitive_files_${STAMP}.txt"
SENSITIVE_TAR="${SECURITY_BACKUP_DIR}/NAVI_sensitive_plain_${STAMP}.tar"
SENSITIVE_ENC="${SECURITY_BACKUP_DIR}/NAVI_sensitive_encrypted_${STAMP}.tar.gz.enc"

: > "$SENSITIVE_LIST"

for file in \
  "docker/env/local.env" \
  ".env" \
  "appsettings.Development.json" \
  "appsettings.Local.json" \
  "src/Navi.ToolsAssets.Api/appsettings.Development.json" \
  "src/Navi.ToolsAssets.Api/appsettings.Local.json" \
  "src/Navi.ToolsAssets.Admin/appsettings.Development.json" \
  "src/Navi.ToolsAssets.Admin/appsettings.Local.json" \
  "src/Navi.ToolsAssets.MobilePwa/appsettings.Development.json" \
  "src/Navi.ToolsAssets.MobilePwa/appsettings.Local.json"
do
  if [ -f "$file" ]; then
    echo "$file" >> "$SENSITIVE_LIST"
  fi
done

if [ -s "$SENSITIVE_LIST" ]; then
  echo "Se encontraron archivos sensibles:"
  cat "$SENSITIVE_LIST"

  if command -v openssl >/dev/null 2>&1; then
    echo ""
    echo "Se cifrará el paquete sensible con contraseña."
    echo "Ingresa una contraseña para cifrar archivos sensibles."
    echo "Guárdala en un lugar seguro. Sin esta contraseña no podrás recuperar esos archivos."
    echo ""

    tar -cf "$SENSITIVE_TAR" -T "$SENSITIVE_LIST"

    gzip -c "$SENSITIVE_TAR" | openssl enc -aes-256-cbc -salt -pbkdf2 -out "$SENSITIVE_ENC"

    rm -f "$SENSITIVE_TAR"

    echo "OK: archivos sensibles cifrados:"
    ls -lh "$SENSITIVE_ENC"

    cat >> "$MANIFEST" <<EOF

Archivos sensibles:
- Se encontraron archivos sensibles.
- Se guardaron cifrados en:
  ${SENSITIVE_ENC}
- Se requiere la contraseña usada durante este backup para recuperarlos.
EOF
  else
    echo "ADVERTENCIA: openssl no está disponible. No se cifraron archivos sensibles."

    cat >> "$MANIFEST" <<EOF

Archivos sensibles:
- Se encontraron archivos sensibles.
- No se cifraron porque openssl no está disponible.
- Revisar:
  ${SENSITIVE_LIST}
EOF
  fi
else
  echo "No se encontraron archivos sensibles locales para cifrar."

  cat >> "$MANIFEST" <<EOF

Archivos sensibles:
- No se encontraron archivos sensibles locales en las rutas esperadas.
EOF
fi

# ============================================================
# 9. Obtener password SQL desde docker/env/local.env
# ============================================================

echo "============================================================"
echo "8. Preparando backup de base de datos"
echo "============================================================"

DB_PASSWORD=""

if [ -f "docker/env/local.env" ]; then
  DB_PASSWORD="$(grep -E '^MSSQL_SA_PASSWORD=' docker/env/local.env | tail -1 | cut -d= -f2- | tr -d '"' | tr -d "'" || true)"
fi

if [ -z "$DB_PASSWORD" ] && [ -n "${MSSQL_SA_PASSWORD:-}" ]; then
  DB_PASSWORD="$MSSQL_SA_PASSWORD"
fi

if [ -z "$DB_PASSWORD" ]; then
  echo "ERROR: No se encontró MSSQL_SA_PASSWORD."
  echo "Configúralo en docker/env/local.env o exporta MSSQL_SA_PASSWORD."
  exit 1
fi

if ! docker ps --format '{{.Names}}' | grep -q "^${DB_CONTAINER}$"; then
  echo "ERROR: No está activo el contenedor SQL esperado: $DB_CONTAINER"
  echo ""
  echo "Contenedores activos:"
  docker ps --format 'table {{.Names}}\t{{.Image}}\t{{.Ports}}'
  exit 1
fi

echo "OK: contenedor SQL activo: $DB_CONTAINER"

# ============================================================
# 10. Validar base de datos
# ============================================================

echo "============================================================"
echo "9. Validando base de datos"
echo "============================================================"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -Q "IF DB_ID('${DB_NAME}') IS NULL BEGIN SELECT name FROM sys.databases ORDER BY name; THROW 51000, 'No existe la base de datos indicada.', 1; END ELSE SELECT name, database_id, create_date FROM sys.databases WHERE name='${DB_NAME}';" \
  > "${DATABASE_BACKUP_DIR}/db_validation_${STAMP}.txt"

cat "${DATABASE_BACKUP_DIR}/db_validation_${STAMP}.txt"

# ============================================================
# 11. Guardar resumen de tablas y conteos
# ============================================================

echo "============================================================"
echo "10. Generando resumen de tablas y conteos"
echo "============================================================"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -d "$DB_NAME" \
  -Q "
SET NOCOUNT ON;

SELECT DB_NAME() AS DatabaseName;

SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(p.rows) AS RowCount
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
INNER JOIN sys.partitions p ON t.object_id = p.object_id
WHERE p.index_id IN (0, 1)
GROUP BY s.name, t.name
ORDER BY s.name, t.name;

SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
ORDER BY TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION;
" > "${DATABASE_BACKUP_DIR}/db_tables_columns_counts_${STAMP}.txt"

echo "OK: resumen SQL guardado:"
echo "${DATABASE_BACKUP_DIR}/db_tables_columns_counts_${STAMP}.txt"

# ============================================================
# 12. Crear backup .bak dentro del contenedor
# ============================================================

echo "============================================================"
echo "11. Creando backup SQL Server .bak"
echo "============================================================"

docker exec "$DB_CONTAINER" mkdir -p "$SQL_CONTAINER_BACKUP_DIR"

BAK_NAME="${DB_NAME}_${STAMP}.bak"
CONTAINER_BAK="${SQL_CONTAINER_BACKUP_DIR}/${BAK_NAME}"
HOST_BAK="${DATABASE_BACKUP_DIR}/${BAK_NAME}"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -d master \
  -Q "BACKUP DATABASE [${DB_NAME}] TO DISK = N'${CONTAINER_BAK}' WITH INIT, FORMAT, CHECKSUM, STATS = 10;" \
  | tee "${DATABASE_BACKUP_DIR}/backup_sql_output_${STAMP}.txt"

echo "============================================================"
echo "12. Verificando backup con RESTORE VERIFYONLY"
echo "============================================================"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -d master \
  -Q "RESTORE VERIFYONLY FROM DISK = N'${CONTAINER_BAK}' WITH CHECKSUM;" \
  | tee "${DATABASE_BACKUP_DIR}/restore_verifyonly_${STAMP}.txt"

echo "============================================================"
echo "13. Copiando .bak al proyecto"
echo "============================================================"

docker cp "${DB_CONTAINER}:${CONTAINER_BAK}" "$HOST_BAK"

if [ ! -f "$HOST_BAK" ]; then
  echo "ERROR: No se pudo copiar el backup .bak al host."
  exit 1
fi

echo "OK: backup SQL copiado:"
ls -lh "$HOST_BAK"

cat >> "$MANIFEST" <<EOF

Backup base de datos:
- Archivo: ${HOST_BAK}
- Ruta SQL Server: ${CONTAINER_BAK}
- Verificación: RESTORE VERIFYONLY ejecutado.
- Log backup: ${DATABASE_BACKUP_DIR}/backup_sql_output_${STAMP}.txt
- Log verify: ${DATABASE_BACKUP_DIR}/restore_verifyonly_${STAMP}.txt
- Resumen tablas: ${DATABASE_BACKUP_DIR}/db_tables_columns_counts_${STAMP}.txt
EOF

# ============================================================
# 13. Crear checksums
# ============================================================

echo "============================================================"
echo "14. Generando checksums SHA256"
echo "============================================================"

CHECKSUM_FILE="${BACKUP_ROOT}/SHA256SUMS_${STAMP}.txt"

find "$BACKUP_ROOT" -type f ! -name "SHA256SUMS_${STAMP}.txt" -print0 \
  | xargs -0 shasum -a 256 > "$CHECKSUM_FILE"

echo "OK: checksums generados:"
echo "$CHECKSUM_FILE"

# ============================================================
# 14. Comprimir carpeta completa del backup
# ============================================================

echo "============================================================"
echo "15. Comprimiendo backup completo"
echo "============================================================"

FINAL_TAR="backups/NAVI_BACKUP_COMPLETO_${STAMP}.tar.gz"

tar -czf "$FINAL_TAR" -C "backups" "full_${STAMP}"

echo "OK: paquete final creado:"
ls -lh "$FINAL_TAR"

# ============================================================
# 15. Resumen final
# ============================================================

echo "============================================================"
echo "BACKUP COMPLETO FINALIZADO"
echo "============================================================"
echo ""
echo "Carpeta del backup:"
echo "$BACKUP_ROOT"
echo ""
echo "Paquete completo:"
echo "$FINAL_TAR"
echo ""
echo "Backup base de datos:"
echo "$HOST_BAK"
echo ""
echo "Git bundle:"
echo "$GIT_BUNDLE"
echo ""
echo "Código fuente:"
echo "$SOURCE_TAR"
echo ""
echo "Checksums:"
echo "$CHECKSUM_FILE"
echo ""
echo "Manifiesto:"
echo "$MANIFEST"
echo ""

echo "============================================================"
echo "IMPORTANTE"
echo "============================================================"
echo "No subir a GitHub:"
echo "- backups/"
echo "- *.bak"
echo "- *.tar.gz de backups"
echo "- archivos cifrados de secretos si contienen información sensible"
echo ""
echo "Guarda una copia externa del archivo:"
echo "$FINAL_TAR"
echo ""

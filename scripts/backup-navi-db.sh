#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "$0")/.."

DB_CONTAINER="${DB_CONTAINER:-navi-tools-sqlserver}"
DB_NAME="${DB_NAME:-NaviToolsAssetsDb}"
BACKUP_DIR_HOST="${BACKUP_DIR_HOST:-backups}"
BACKUP_DIR_CONTAINER="${BACKUP_DIR_CONTAINER:-/var/opt/mssql/backups}"

mkdir -p "$BACKUP_DIR_HOST"

if [ -f "docker/env/local.env" ]; then
  DB_PASSWORD="$(grep -E '^MSSQL_SA_PASSWORD=' docker/env/local.env | tail -1 | cut -d= -f2- | tr -d '"' | tr -d "'")"
else
  DB_PASSWORD="${MSSQL_SA_PASSWORD:-}"
fi

if [ -z "${DB_PASSWORD:-}" ]; then
  echo "ERROR: No se encontró MSSQL_SA_PASSWORD en docker/env/local.env ni en variable de entorno."
  exit 1
fi

if ! docker ps --format '{{.Names}}' | grep -q "^${DB_CONTAINER}$"; then
  echo "ERROR: No está activo el contenedor $DB_CONTAINER."
  echo "Contenedores activos:"
  docker ps --format 'table {{.Names}}\t{{.Image}}\t{{.Ports}}'
  exit 1
fi

STAMP="$(date +%Y%m%d_%H%M%S)"
BACKUP_FILE="${DB_NAME}_${STAMP}.bak"
CONTAINER_BAK="${BACKUP_DIR_CONTAINER}/${BACKUP_FILE}"
HOST_BAK="${BACKUP_DIR_HOST}/${BACKUP_FILE}"

echo "============================================================"
echo "Validando base de datos"
echo "============================================================"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -Q "IF DB_ID('${DB_NAME}') IS NULL BEGIN SELECT name FROM sys.databases ORDER BY name; THROW 51000, 'No existe la base de datos indicada.', 1; END ELSE SELECT name, database_id, create_date FROM sys.databases WHERE name='${DB_NAME}';"

echo "============================================================"
echo "Creando carpeta de backups dentro del contenedor"
echo "============================================================"

docker exec "$DB_CONTAINER" mkdir -p "$BACKUP_DIR_CONTAINER"

echo "============================================================"
echo "Generando backup: $CONTAINER_BAK"
echo "============================================================"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -d master \
  -Q "BACKUP DATABASE [${DB_NAME}] TO DISK = N'${CONTAINER_BAK}' WITH INIT, FORMAT, CHECKSUM, STATS = 10;"

echo "============================================================"
echo "Verificando backup con RESTORE VERIFYONLY"
echo "============================================================"

docker exec -e SQLCMDPASSWORD="$DB_PASSWORD" "$DB_CONTAINER" \
  /opt/mssql-tools18/bin/sqlcmd \
  -S localhost \
  -U sa \
  -C \
  -d master \
  -Q "RESTORE VERIFYONLY FROM DISK = N'${CONTAINER_BAK}' WITH CHECKSUM;"

echo "============================================================"
echo "Copiando backup al proyecto"
echo "============================================================"

docker cp "${DB_CONTAINER}:${CONTAINER_BAK}" "$HOST_BAK"

echo "============================================================"
echo "Backup creado correctamente"
echo "============================================================"
ls -lh "$HOST_BAK"

echo ""
echo "Archivo:"
echo "$HOST_BAK"

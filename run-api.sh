#!/usr/bin/env bash
set -euo pipefail

cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

export ASPNETCORE_ENVIRONMENT="Development"

export MSSQL_SA_PASSWORD="$(grep '^MSSQL_SA_PASSWORD=' docker/env/local.env | cut -d= -f2-)"
export MINIO_ROOT_USER="$(grep '^MINIO_ROOT_USER=' docker/env/local.env | cut -d= -f2-)"
export MINIO_ROOT_PASSWORD="$(grep '^MINIO_ROOT_PASSWORD=' docker/env/local.env | cut -d= -f2-)"
export MINIO_BUCKET="$(grep '^MINIO_BUCKET=' docker/env/local.env | cut -d= -f2-)"

export ConnectionStrings__NaviToolsAssetsDb="Server=localhost,1433;Database=NaviToolsAssetsDb;User Id=sa;Password=$MSSQL_SA_PASSWORD;TrustServerCertificate=True;"

export Minio__Endpoint="localhost:9000"
export Minio__AccessKey="$MINIO_ROOT_USER"
export Minio__SecretKey="$MINIO_ROOT_PASSWORD"
export Minio__BucketName="$MINIO_BUCKET"
export Minio__UseSsl="false"

dotnet run \
  --project src/Navi.ToolsAssets.Api/Navi.ToolsAssets.Api.csproj \
  --urls "http://localhost:5218"

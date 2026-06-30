#!/usr/bin/env bash
set -euo pipefail

cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

PORT=5264

echo "============================================================"
echo "NAVI - Levantando Admin Web"
echo "URL: http://localhost:$PORT"
echo "============================================================"

PIDS="$(lsof -tiTCP:$PORT -sTCP:LISTEN || true)"

if [ -n "$PIDS" ]; then
  echo "Deteniendo proceso anterior en puerto $PORT: $PIDS"
  kill $PIDS || true
  sleep 1

  PIDS_FORCE="$(lsof -tiTCP:$PORT -sTCP:LISTEN || true)"
  if [ -n "$PIDS_FORCE" ]; then
    echo "Forzando cierre en puerto $PORT: $PIDS_FORCE"
    kill -9 $PIDS_FORCE || true
  fi
fi

export ASPNETCORE_ENVIRONMENT="Development"

export NaviApi__BaseUrl="http://localhost:5218"
export NaviApi__BaseAddress="http://localhost:5218"
export ApiBaseUrl="http://localhost:5218"

dotnet run \
  --project src/Navi.ToolsAssets.Admin/Navi.ToolsAssets.Admin.csproj \
  --urls "http://localhost:$PORT"

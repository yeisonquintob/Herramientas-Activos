#!/usr/bin/env bash
set -euo pipefail

cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

echo "============================================================"
echo "NAVI - Compilando API"
echo "============================================================"

dotnet build src/Navi.ToolsAssets.Api/Navi.ToolsAssets.Api.csproj -c Debug

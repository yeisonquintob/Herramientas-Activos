#!/usr/bin/env bash
set -euo pipefail

cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

echo "============================================================"
echo "NAVI - Compilando Admin Web"
echo "============================================================"

dotnet build src/Navi.ToolsAssets.Admin/Navi.ToolsAssets.Admin.csproj -c Debug

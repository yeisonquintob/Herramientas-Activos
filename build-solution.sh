#!/usr/bin/env bash
set -euo pipefail

cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

echo "============================================================"
echo "NAVI - Compilando solución completa"
echo "============================================================"

dotnet build Navitrans.ToolsAssets.Management.sln -c Debug

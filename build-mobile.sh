#!/usr/bin/env bash
set -euo pipefail

cd /Users/luu/Desktop/Proyecto_Navi/Projects/Herramientas-Activos

echo "============================================================"
echo "NAVI - Compilando Mobile PWA"
echo "============================================================"

dotnet build src/Navi.ToolsAssets.MobilePwa/Navi.ToolsAssets.MobilePwa.csproj -c Debug

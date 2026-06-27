# NAVI Herramientas - Inventario de base de datos

## Ruta

scripts\db-inventory

## Ejecutar

PowerShell:

Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass

cd "C:\Users\yquinto\OneDrive - Navitrans\Escritorio\Documentos\Herramientas\Herramientas&Activos\scripts\db-inventory"

.\Run-NaviDbInventory.ps1 -ServerInstance "localhost,1434" -Database "NaviToolsAssetsDb" -SqlUser "sa"

## Archivos de salida importantes

- 00_DB_Resumen_Estructura_Relaciones.txt
- 01_DB_Generar_DDL_Estructura.txt
- 02_DB_Exportar_Datos_Seguro.txt
- 03_DB_Exportar_Datos_JSON_Seguro.txt
- 04_DB_Auditoria_Roles_Permisos_Seguridad.txt
- 05_DB_Calidad_Datos_Y_Mejora.txt

# Pruebas

La suite `tests/Navi.ToolsAssets.Tests` cubre reglas unitarias, arquitectura,
permisos, tenant e integración HTTP sin conectarse a producción.

```bash
dotnet test Navitrans.ToolsAssets.Management.sln -c Debug --no-build --nologo
dotnet test Navitrans.ToolsAssets.Management.sln -c Release --no-build --nologo
```

`WebApplicationFactory` usa ambiente `Testing` y una conexión deliberadamente
inaccesible. Los smoke tests actuales no ejecutan endpoints que consultan esa
base.

Las pruebas de persistencia, sesión revocada, importación válida y aislamiento
requieren SQL/MinIO exclusivos de CI. Nunca deben apuntar a producción. Consulte
la matriz funcional y el reporte de seguridad para los casos pendientes.

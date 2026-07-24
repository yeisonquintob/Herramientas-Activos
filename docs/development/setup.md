# Preparación de desarrollo

1. Instale .NET SDK 8 y Docker.
2. Copie `docker/env/local.env.example` a `docker/env/local.env`.
3. Reemplace los marcadores localmente; el archivo está ignorado.
4. Inicie SQL y MinIO con el compose de desarrollo.
5. Configure variables para API, Admin y Mobile.
6. Aplique migraciones a una base de desarrollo.
7. Restaure, compile y pruebe la solución.

```bash
dotnet restore Navitrans.ToolsAssets.Management.sln
dotnet build Navitrans.ToolsAssets.Management.sln -c Debug --nologo
dotnet test Navitrans.ToolsAssets.Management.sln -c Debug --no-build --nologo
```

No use datos productivos. Para EF, use exclusivamente
`NAVI_TOOLS_DB_CONNECTION` o `NAVI_MASTER_DB_CONNECTION`.

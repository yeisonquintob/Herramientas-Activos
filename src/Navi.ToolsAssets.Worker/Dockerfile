FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Navi.ToolsAssets.Worker/Navi.ToolsAssets.Worker.csproj --nologo
RUN dotnet publish src/Navi.ToolsAssets.Worker/Navi.ToolsAssets.Worker.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --nologo

FROM mcr.microsoft.com/dotnet/runtime:8.0-bookworm-slim AS runtime
WORKDIR /app
COPY --from=build --chown=app:app /app/publish .
USER app
ENV DOTNET_EnableDiagnostics=0
HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD test -r /proc/1/cmdline && grep -qa "Navi.ToolsAssets.Worker.dll" /proc/1/cmdline || exit 1
ENTRYPOINT ["dotnet", "Navi.ToolsAssets.Worker.dll"]

FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/Navi.ToolsAssets.Admin/Navi.ToolsAssets.Admin.csproj --nologo
RUN dotnet publish src/Navi.ToolsAssets.Admin/Navi.ToolsAssets.Admin.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --nologo

FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim AS runtime
USER root
RUN apt-get update \
    && apt-get install --yes --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=build --chown=app:app /app/publish .
RUN mkdir -p /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /home/app/.aspnet
USER app
ENV ASPNETCORE_URLS=http://+:8080 \
    DOTNET_EnableDiagnostics=0
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
    CMD curl --fail --silent http://localhost:8080/ || exit 1
ENTRYPOINT ["dotnet", "Navi.ToolsAssets.Admin.dll"]

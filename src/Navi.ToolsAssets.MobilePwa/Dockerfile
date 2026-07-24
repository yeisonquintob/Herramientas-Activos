FROM mcr.microsoft.com/dotnet/sdk:8.0-bookworm-slim AS build
ARG NAVI_API_BASE_URL=http://localhost:5218/
WORKDIR /src
COPY . .
RUN dotnet restore src/Navi.ToolsAssets.MobilePwa/Navi.ToolsAssets.MobilePwa.csproj --nologo
RUN dotnet publish src/Navi.ToolsAssets.MobilePwa/Navi.ToolsAssets.MobilePwa.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    --nologo \
    && sed -i "s#http://localhost:5218/#${NAVI_API_BASE_URL}#g" /app/publish/wwwroot/appsettings.json

FROM nginxinc/nginx-unprivileged:1.27-alpine AS runtime
COPY docker/nginx/mobile.conf /etc/nginx/conf.d/default.conf
COPY --from=build --chown=101:101 /app/publish/wwwroot /usr/share/nginx/html
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD wget --quiet --tries=1 --spider http://localhost:8080/ || exit 1

using Navi.ToolsAssets.Admin.Services.Auth;

namespace Navi.ToolsAssets.Admin.Services.Api;

/// <summary>
/// Crea clientes HTTP dentro del mismo ámbito del circuito Blazor.
/// De esta forma, NaviPermissionHttpMessageHandler recibe la misma
/// instancia de WebAuthSessionService utilizada por la interfaz.
/// </summary>
public sealed class NaviScopedHttpClientFactory :
    IHttpClientFactory,
    IDisposable
{
    private const string NaviApiClientName = "NaviApi";

    private readonly Uri _baseAddress;
    private readonly TimeSpan _timeout;
    private readonly NaviPermissionHttpMessageHandler _authenticationHandler;
    private bool _disposed;

    public NaviScopedHttpClientFactory(
        IConfiguration configuration,
        WebAuthSessionService authSession)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(authSession);

        var configuredBaseUrl =
            configuration["NaviApi:BaseUrl"]
            ?? "http://localhost:5218";

        if (!Uri.TryCreate(
                configuredBaseUrl,
                UriKind.Absolute,
                out var baseAddress))
        {
            throw new InvalidOperationException(
                $"La configuración NaviApi:BaseUrl no es válida: {configuredBaseUrl}");
        }

        var timeoutSeconds =
            configuration.GetValue(
                "NaviApi:TimeoutSeconds",
                30);

        if (timeoutSeconds <= 0)
        {
            throw new InvalidOperationException(
                "NaviApi:TimeoutSeconds debe ser mayor que cero.");
        }

        _baseAddress = baseAddress;
        _timeout = TimeSpan.FromSeconds(timeoutSeconds);

        var primaryHandler = new SocketsHttpHandler
        {
            PooledConnectionLifetime =
                TimeSpan.FromMinutes(10),

            PooledConnectionIdleTimeout =
                TimeSpan.FromMinutes(2)
        };

        _authenticationHandler =
            new NaviPermissionHttpMessageHandler(
                authSession)
            {
                InnerHandler = primaryHandler
            };
    }

    public HttpClient CreateClient(string name)
    {
        ObjectDisposedException.ThrowIf(
            _disposed,
            this);

        if (!string.Equals(
                name,
                NaviApiClientName,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Cliente HTTP no registrado: {name}");
        }

        return new HttpClient(
            _authenticationHandler,
            disposeHandler: false)
        {
            BaseAddress = _baseAddress,
            Timeout = _timeout
        };
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _authenticationHandler.Dispose();
    }
}

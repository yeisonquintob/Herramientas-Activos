using System.Net.Http.Headers;

namespace Navi.ToolsAssets.Admin.Services.Auth;

public sealed class NaviPermissionHttpMessageHandler : DelegatingHandler
{
    private readonly WebAuthSessionService _authSession;

    public NaviPermissionHttpMessageHandler(WebAuthSessionService authSession)
    {
        _authSession = authSession;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (_authSession.IsAuthenticated &&
            !string.IsNullOrWhiteSpace(_authSession.AccessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _authSession.AccessToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}

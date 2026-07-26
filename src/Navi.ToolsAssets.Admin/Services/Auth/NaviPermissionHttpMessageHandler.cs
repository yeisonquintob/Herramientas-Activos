using System.Net;
using System.Net.Http.Headers;

namespace Navi.ToolsAssets.Admin.Services.Auth;

public sealed class NaviPermissionHttpMessageHandler : DelegatingHandler
{
    private readonly WebAuthSessionService _authSession;

    public NaviPermissionHttpMessageHandler(WebAuthSessionService authSession)
    {
        _authSession = authSession;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestHasSession =
            _authSession.IsAuthenticated &&
            !string.IsNullOrWhiteSpace(_authSession.AccessToken);

        if (requestHasSession)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                _authSession.AccessToken);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (requestHasSession &&
            response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _authSession.Logout();
        }

        return response;
    }
}

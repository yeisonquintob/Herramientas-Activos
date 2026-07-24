using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Tests.Security;

public sealed class RequirePermissionAttributeTests
{
    [Fact]
    public void MissingPermission_ReturnsForbidden()
    {
        var context = CreateContext(new Claim(ClaimTypes.Name, "operator"));
        var attribute = new RequirePermissionAttribute(PermissionCodes.ImportsApply);

        attribute.OnActionExecuting(context);

        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status403Forbidden, result.StatusCode);
    }

    [Fact]
    public void MatchingPermission_AllowsAction()
    {
        var context = CreateContext(
            new Claim(ClaimTypes.Name, "operator"),
            new Claim(NaviClaimTypes.Permission, PermissionCodes.ImportsApply));
        var attribute = new RequirePermissionAttribute(PermissionCodes.ImportsApply);

        attribute.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void AdminRole_AllowsAction()
    {
        var context = CreateContext(
            new Claim(ClaimTypes.Name, "admin"),
            new Claim(NaviClaimTypes.RoleCode, "ADMIN"));
        var attribute = new RequirePermissionAttribute(PermissionCodes.ImportsApply);

        attribute.OnActionExecuting(context);

        Assert.Null(context.Result);
    }

    private static ActionExecutingContext CreateContext(params Claim[] claims)
    {
        var identity = new ClaimsIdentity(claims, authenticationType: "test");
        var httpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new ActionDescriptor());

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new object());
    }
}

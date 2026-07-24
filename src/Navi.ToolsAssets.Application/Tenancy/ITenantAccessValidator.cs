namespace Navi.ToolsAssets.Application.Tenancy;

public interface ITenantAccessValidator
{
    Task<TenantResolutionResult> ResolveAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default);
}

public sealed record TenantResolutionResult(
    bool IsAllowed,
    string? CompanyCode = null,
    string? ConnectionString = null,
    string? ErrorCode = null);

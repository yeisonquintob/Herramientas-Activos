namespace Navi.ToolsAssets.Application.Tenancy;

public interface ICurrentTenant
{
    Guid? CompanyId { get; }
    string? CompanyCode { get; }
    string? ConnectionString { get; }
    bool IsResolved { get; }
}

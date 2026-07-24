namespace Navi.ToolsAssets.Application.Tenancy;

public sealed class TenantContext : ICurrentTenant
{
    public Guid? CompanyId { get; private set; }
    public string? CompanyCode { get; private set; }
    public string? ConnectionString { get; private set; }
    public bool IsResolved => CompanyId.HasValue && !string.IsNullOrWhiteSpace(ConnectionString);

    public void Resolve(Guid companyId, string companyCode, string connectionString)
    {
        if (IsResolved)
        {
            throw new InvalidOperationException("La compañía activa ya fue resuelta para este request.");
        }

        CompanyId = companyId;
        CompanyCode = companyCode;
        ConnectionString = connectionString;
    }
}

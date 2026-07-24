using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Navi.ToolsAssets.Application.Tenancy;
using Navi.ToolsAssets.Domain.Entities.Tenancy;

namespace Navi.ToolsAssets.Infrastructure.Tenancy;

public sealed class TenantAccessValidator : ITenantAccessValidator
{
    private readonly NaviMasterDbContext _masterContext;
    private readonly IConfiguration _configuration;

    public TenantAccessValidator(
        NaviMasterDbContext masterContext,
        IConfiguration configuration)
    {
        _masterContext = masterContext;
        _configuration = configuration;
    }

    public async Task<TenantResolutionResult> ResolveAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var access = await _masterContext.UserCompanyAccesses
            .AsNoTracking()
            .Include(x => x.Company)
                .ThenInclude(x => x!.Databases)
            .FirstOrDefaultAsync(
                x => x.UserId == userId &&
                     x.CompanyId == companyId &&
                     x.IsActive,
                cancellationToken);

        var company = access?.Company;
        var database = company?.Databases.FirstOrDefault(x =>
            !x.IsDeleted &&
            string.Equals(x.Status, "Ready", StringComparison.OrdinalIgnoreCase));

        if (company is null || !company.IsActive ||
            !string.Equals(company.Status, CompanyStatuses.Active, StringComparison.OrdinalIgnoreCase))
        {
            return new TenantResolutionResult(false, ErrorCode: "CompanyUnavailable");
        }

        if (database is null)
        {
            return new TenantResolutionResult(false, ErrorCode: "CompanyDatabaseUnavailable");
        }

        var connectionString = _configuration[$"TenantDatabases:{database.ConnectionKey}"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new TenantResolutionResult(false, ErrorCode: "CompanyConnectionNotConfigured");
        }

        return new TenantResolutionResult(
            true,
            company.Code,
            connectionString);
    }
}

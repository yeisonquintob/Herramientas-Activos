using Navi.ToolsAssets.Application.Tenancy;

namespace Navi.ToolsAssets.Tests.Tenancy;

public sealed class TenantContextTests
{
    [Fact]
    public void Resolve_StoresActiveCompany()
    {
        var context = new TenantContext();
        var companyId = Guid.NewGuid();

        context.Resolve(companyId, "ACME", "Server=tenant-db");

        Assert.True(context.IsResolved);
        Assert.Equal(companyId, context.CompanyId);
        Assert.Equal("ACME", context.CompanyCode);
        Assert.Equal("Server=tenant-db", context.ConnectionString);
    }

    [Fact]
    public void Resolve_Twice_IsRejected()
    {
        var context = new TenantContext();
        context.Resolve(Guid.NewGuid(), "ONE", "Server=one");

        Assert.Throws<InvalidOperationException>(() =>
            context.Resolve(Guid.NewGuid(), "TWO", "Server=two"));
    }
}

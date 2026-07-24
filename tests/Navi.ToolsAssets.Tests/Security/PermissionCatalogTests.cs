using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Tests.Security;

public sealed class PermissionCatalogTests
{
    [Fact]
    public void PermissionCodes_AreUniqueAndWellFormed()
    {
        var definitions = PermissionCatalog.All;

        Assert.NotEmpty(definitions);
        Assert.Equal(
            definitions.Count,
            definitions.Select(definition => definition.Code)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count());
        Assert.All(definitions, definition =>
        {
            Assert.Contains('.', definition.Code);
            Assert.False(string.IsNullOrWhiteSpace(definition.DisplayName));
            Assert.False(string.IsNullOrWhiteSpace(definition.Module));
            Assert.False(string.IsNullOrWhiteSpace(definition.Description));
        });
    }
}

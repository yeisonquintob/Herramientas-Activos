using Navi.ToolsAssets.Domain.Entities.Inventory;

namespace Navi.ToolsAssets.Tests.Architecture;

public sealed class ArchitectureTests
{
    [Fact]
    public void Domain_DoesNotReferenceInfrastructureFrameworks()
    {
        var forbiddenFragments = new[]
        {
            "EntityFrameworkCore",
            "AspNetCore",
            "Hangfire",
            "Minio",
            "SqlClient"
        };

        var references = typeof(ToolAsset).Assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name ?? string.Empty)
            .ToArray();

        var violations = references
            .Where(reference => forbiddenFragments.Any(fragment =>
                reference.Contains(fragment, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        Assert.Empty(violations);
    }
}

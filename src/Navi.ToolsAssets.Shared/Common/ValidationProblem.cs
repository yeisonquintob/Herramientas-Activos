namespace Navi.ToolsAssets.Shared.Common;

/// <summary>
/// Representación estable de errores de validación independiente de MVC.
/// </summary>
public sealed record ValidationProblem(
    string Title,
    IReadOnlyDictionary<string, string[]> Errors,
    string? Detail = null,
    string? TraceId = null);

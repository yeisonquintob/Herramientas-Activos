namespace Navi.ToolsAssets.Shared.Common;

/// <summary>
/// Resultado tipado para clientes que necesitan transportar datos o un error
/// funcional sin exponer excepciones de infraestructura.
/// </summary>
public sealed record ApiResult<T>
{
    public bool IsSuccess { get; init; }

    public T? Value { get; init; }

    public int? StatusCode { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyDictionary<string, string[]> ValidationErrors { get; init; } =
        new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

    public static ApiResult<T> Success(T value, int? statusCode = null) =>
        new()
        {
            IsSuccess = true,
            Value = value,
            StatusCode = statusCode
        };

    public static ApiResult<T> Failure(
        string errorMessage,
        int? statusCode = null,
        string? errorCode = null,
        IReadOnlyDictionary<string, string[]>? validationErrors = null) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode,
            ErrorCode = errorCode,
            ValidationErrors = validationErrors ??
                new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        };
}

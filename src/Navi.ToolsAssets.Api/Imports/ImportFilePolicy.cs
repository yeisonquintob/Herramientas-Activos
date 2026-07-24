namespace Navi.ToolsAssets.Api.Imports;

/// <summary>
/// Pure validation rules for Excel staging files. Keeping this policy free of
/// storage and database dependencies makes valid and invalid inputs testable.
/// </summary>
public static class ImportFilePolicy
{
    public const long DefaultMaximumFileSizeBytes = 10 * 1024 * 1024;
    public const long HardMaximumFileSizeBytes = 25 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/octet-stream",
        "application/zip"
    };

    public static ImportFileValidationResult ValidateMetadata(
        string? fileName,
        string? contentType,
        long length,
        long maximumFileSizeBytes)
    {
        if (length <= 0)
        {
            return ImportFileValidationResult.Invalid("Debe adjuntar un archivo Excel.");
        }

        if (!string.Equals(Path.GetExtension(fileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return ImportFileValidationResult.Invalid(
                "Solo se permiten archivos .xlsx sin macros. Los formatos .xls y .xlsm se rechazan por seguridad.");
        }

        if (length > maximumFileSizeBytes)
        {
            return ImportFileValidationResult.Invalid(
                $"El archivo supera el tamaño máximo permitido de {maximumFileSizeBytes / 1024 / 1024} MB.");
        }

        if (!string.IsNullOrWhiteSpace(contentType) && !AllowedContentTypes.Contains(contentType))
        {
            return ImportFileValidationResult.Invalid(
                "El tipo de contenido del archivo no corresponde a un libro .xlsx.");
        }

        return ImportFileValidationResult.Valid();
    }

    public static bool HasOpenXmlSignature(Stream stream)
    {
        if (!stream.CanSeek || stream.Length < 4)
        {
            return false;
        }

        var originalPosition = stream.Position;
        Span<byte> signature = stackalloc byte[4];
        stream.Position = 0;
        var bytesRead = stream.Read(signature);
        stream.Position = originalPosition;

        return bytesRead == signature.Length
            && signature[0] == 0x50
            && signature[1] == 0x4B
            && signature[2] is 0x03 or 0x05 or 0x07
            && signature[3] is 0x04 or 0x06 or 0x08;
    }
}

public sealed record ImportFileValidationResult(bool IsValid, string? Error)
{
    public static ImportFileValidationResult Valid() => new(true, null);

    public static ImportFileValidationResult Invalid(string error) => new(false, error);
}

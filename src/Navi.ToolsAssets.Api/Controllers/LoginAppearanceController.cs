using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Application.Documents;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/login-appearance")]
public sealed class LoginAppearanceController : ControllerBase
{
    private const string ParameterCode =
        "ADMIN_LOGIN_APPEARANCE_IMAGE";

    private const long MaxImageBytes =
        5L * 1024L * 1024L;

    private const long MaxRequestBytes =
        MaxImageBytes + (1L * 1024L * 1024L);

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly NaviToolsAssetsDbContext _context;
    private readonly IDocumentStorageService _storage;

    public LoginAppearanceController(
        NaviToolsAssetsDbContext context,
        IDocumentStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    /// <summary>
    /// Consulta pública requerida por la pantalla de acceso.
    /// No expone el objeto interno de MinIO.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<LoginAppearanceResponse>(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginAppearanceResponse>>
        GetConfiguration(
            CancellationToken cancellationToken)
    {
        var metadata =
            await ReadMetadataAsync(
                cancellationToken);

        if (
            metadata is null
            ||
            !await _storage.ExistsAsync(
                metadata.ObjectName,
                cancellationToken)
        )
        {
            return Ok(
                LoginAppearanceResponse.Empty);
        }

        return Ok(
            CreateResponse(metadata));
    }

    /// <summary>
    /// Entrega pública de la imagen configurada.
    /// El navegador únicamente conoce este endpoint.
    /// </summary>
    [HttpGet("image")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImage(
        CancellationToken cancellationToken)
    {
        var metadata =
            await ReadMetadataAsync(
                cancellationToken);

        if (
            metadata is null
            ||
            !await _storage.ExistsAsync(
                metadata.ObjectName,
                cancellationToken)
        )
        {
            return NotFound();
        }

        var stream =
            await _storage.DownloadAsync(
                metadata.ObjectName,
                cancellationToken);

        Response.Headers[
            "X-Content-Type-Options"
        ] = "nosniff";

        Response.Headers[
            "Cache-Control"
        ] = "public,max-age=86400";

        return File(
            stream,
            metadata.ContentType,
            enableRangeProcessing: true);
    }

    /// <summary>
    /// Carga o sustituye la imagen corporativa.
    /// </summary>
    [HttpPost("image")]
    [RequirePermission("Settings.Manage")]
    [RequestSizeLimit(MaxRequestBytes)]
    [RequestFormLimits(
        MultipartBodyLengthLimit = MaxRequestBytes)]
    [ProducesResponseType<LoginAppearanceResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginAppearanceResponse>>
        UploadImage(
            [FromForm] IFormFile? image,
            CancellationToken cancellationToken)
    {
        if (image is null || image.Length <= 0)
        {
            return BadRequest(
                new
                {
                    Message =
                        "Debe seleccionar una imagen."
                });
        }

        if (image.Length > MaxImageBytes)
        {
            return BadRequest(
                new
                {
                    Message =
                        "La imagen no puede superar 5 MB."
                });
        }

        await using var memoryStream =
            new MemoryStream();

        await image.CopyToAsync(
            memoryStream,
            cancellationToken);

        var bytes =
            memoryStream.ToArray();

        var detectedImage =
            DetectImage(bytes);

        if (detectedImage is null)
        {
            return BadRequest(
                new
                {
                    Message =
                        "El archivo no corresponde a una imagen " +
                        "JPG, PNG o WebP válida."
                });
        }

        var previousMetadata =
            await ReadMetadataAsync(
                cancellationToken);

        var now =
            DateTime.UtcNow;

        var objectName =
            "branding/login/"
            + $"admin-login-{now:yyyyMMddHHmmssfff}-"
            + $"{Guid.NewGuid():N}"
            + $".{detectedImage.Extension}";

        memoryStream.Position = 0;

        await _storage.UploadAsync(
            objectName,
            memoryStream,
            detectedImage.ContentType,
            cancellationToken);

        var metadata =
            new LoginImageMetadata(
                ObjectName: objectName,
                FileName: NormalizeFileName(
                    image.FileName),
                ContentType:
                    detectedImage.ContentType,
                UpdatedAtUtc: now);

        try
        {
            await SaveMetadataAsync(
                metadata,
                GetCurrentUser(),
                cancellationToken);
        }
        catch
        {
            try
            {
                await _storage.DeleteAsync(
                    objectName,
                    cancellationToken);
            }
            catch
            {
                // El error original de persistencia tiene prioridad.
            }

            throw;
        }

        if (
            previousMetadata is not null
            &&
            !string.Equals(
                previousMetadata.ObjectName,
                objectName,
                StringComparison.Ordinal)
        )
        {
            try
            {
                await _storage.DeleteAsync(
                    previousMetadata.ObjectName,
                    cancellationToken);
            }
            catch
            {
                /*
                 * La configuración nueva ya quedó vigente.
                 * Una limpieza posterior puede retirar el
                 * objeto anterior si MinIO no estaba disponible.
                 */
            }
        }

        return Ok(
            CreateResponse(metadata));
    }

    /// <summary>
    /// Elimina la imagen personalizada y deja el fondo
    /// corporativo predeterminado del login.
    /// </summary>
    [HttpDelete("image")]
    [RequirePermission("Settings.Manage")]
    [ProducesResponseType<LoginAppearanceResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginAppearanceResponse>>
        DeleteImage(
            CancellationToken cancellationToken)
    {
        var metadata =
            await ReadMetadataAsync(
                cancellationToken);

        var parameter =
            await _context.SystemParameters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    item =>
                        item.Code == ParameterCode,
                    cancellationToken);

        if (parameter is not null)
        {
            parameter.Value = null;
            parameter.IsActive = false;
            parameter.IsDeleted = false;
            parameter.UpdatedAt = DateTime.UtcNow;
            parameter.UpdatedBy = GetCurrentUser();

            await _context.SaveChangesAsync(
                cancellationToken);
        }

        if (metadata is not null)
        {
            try
            {
                await _storage.DeleteAsync(
                    metadata.ObjectName,
                    cancellationToken);
            }
            catch
            {
                /*
                 * La referencia ya fue retirada.
                 * La ausencia temporal de MinIO no debe impedir
                 * restablecer visualmente el login.
                 */
            }
        }

        return Ok(
            LoginAppearanceResponse.Empty);
    }

    private async Task<LoginImageMetadata?>
        ReadMetadataAsync(
            CancellationToken cancellationToken)
    {
        var parameter =
            await _context.SystemParameters
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    item =>
                        item.Code == ParameterCode
                        && item.IsActive,
                    cancellationToken);

        if (string.IsNullOrWhiteSpace(
            parameter?.Value))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<
                LoginImageMetadata>(
                    parameter.Value,
                    JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private async Task SaveMetadataAsync(
        LoginImageMetadata metadata,
        string userName,
        CancellationToken cancellationToken)
    {
        var parameter =
            await _context.SystemParameters
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(
                    item =>
                        item.Code == ParameterCode,
                    cancellationToken);

        var serializedValue =
            JsonSerializer.Serialize(
                metadata,
                JsonOptions);

        if (parameter is null)
        {
            parameter =
                new SystemParameter
                {
                    Code = ParameterCode,
                    Name =
                        "Imagen corporativa del login Admin",
                    Description =
                        "Referencia global de la imagen " +
                        "mostrada en el acceso del Admin Web.",
                    Value = serializedValue,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName
                };

            _context.SystemParameters.Add(
                parameter);
        }
        else
        {
            parameter.Name =
                "Imagen corporativa del login Admin";

            parameter.Description =
                "Referencia global de la imagen mostrada " +
                "en el acceso del Admin Web.";

            parameter.Value =
                serializedValue;

            parameter.IsActive = true;
            parameter.IsDeleted = false;
            parameter.UpdatedAt = DateTime.UtcNow;
            parameter.UpdatedBy = userName;
        }

        await _context.SaveChangesAsync(
            cancellationToken);
    }

    private string GetCurrentUser()
    {
        var value = User.GetUserName();

        if (string.IsNullOrWhiteSpace(value))
        {
            return "appearance-admin";
        }

        value = value.Trim();

        return value.Length <= 150
            ? value
            : value[..150];
    }

    private static LoginAppearanceResponse
        CreateResponse(
            LoginImageMetadata metadata)
    {
        return new LoginAppearanceResponse(
            HasImage: true,
            ImageUrl:
                "/api/login-appearance/image",
            FileName: metadata.FileName,
            ContentType: metadata.ContentType,
            UpdatedAtUtc:
                metadata.UpdatedAtUtc,
            Version:
                metadata.UpdatedAtUtc
                    .Ticks
                    .ToString());
    }

    private static string NormalizeFileName(
        string? fileName)
    {
        var normalized =
            Path.GetFileName(
                fileName ?? "imagen-login");

        if (string.IsNullOrWhiteSpace(
            normalized))
        {
            normalized =
                "imagen-login";
        }

        return normalized.Length <= 240
            ? normalized
            : normalized[..240];
    }

    private static DetectedImage?
        DetectImage(
            byte[] bytes)
    {
        if (
            bytes.Length >= 8
            &&
            bytes[0] == 0x89
            &&
            bytes[1] == 0x50
            &&
            bytes[2] == 0x4E
            &&
            bytes[3] == 0x47
            &&
            bytes[4] == 0x0D
            &&
            bytes[5] == 0x0A
            &&
            bytes[6] == 0x1A
            &&
            bytes[7] == 0x0A
        )
        {
            return new DetectedImage(
                "png",
                "image/png");
        }

        if (
            bytes.Length >= 3
            &&
            bytes[0] == 0xFF
            &&
            bytes[1] == 0xD8
            &&
            bytes[2] == 0xFF
        )
        {
            return new DetectedImage(
                "jpg",
                "image/jpeg");
        }

        if (
            bytes.Length >= 12
            &&
            bytes[0] == 0x52
            &&
            bytes[1] == 0x49
            &&
            bytes[2] == 0x46
            &&
            bytes[3] == 0x46
            &&
            bytes[8] == 0x57
            &&
            bytes[9] == 0x45
            &&
            bytes[10] == 0x42
            &&
            bytes[11] == 0x50
        )
        {
            return new DetectedImage(
                "webp",
                "image/webp");
        }

        return null;
    }

    private sealed record LoginImageMetadata(
        string ObjectName,
        string FileName,
        string ContentType,
        DateTime UpdatedAtUtc);

    private sealed record DetectedImage(
        string Extension,
        string ContentType);

    public sealed record LoginAppearanceResponse(
        bool HasImage,
        string? ImageUrl,
        string? FileName,
        string? ContentType,
        DateTime? UpdatedAtUtc,
        string? Version)
    {
        public static LoginAppearanceResponse Empty { get; } =
            new(
                HasImage: false,
                ImageUrl: null,
                FileName: null,
                ContentType: null,
                UpdatedAtUtc: null,
                Version: null);
    }
}

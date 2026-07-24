using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Navi.ToolsAssets.Api.Security;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/settings/sync-export")]
[RequirePermission("Settings.Manage")]
public sealed class SettingsSyncExportController : ControllerBase
{
    private const long MaxBackupSizeBytes = 5L * 1024L * 1024L * 1024L;

    private static readonly string[] AllowedExtensions =
    {
        ".bak",
        ".bacpac",
        ".zip",
        ".7z"
    };

    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public SettingsSyncExportController(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    [HttpGet("backups")]
    public IActionResult GetUploadedBackups()
    {
        var storagePath = EnsureBackupStoragePath();

        var items = Directory
            .GetFiles(storagePath)
            .Select(path =>
            {
                var file = new FileInfo(path);

                return new BackupFileResponse
                {
                    FileName = file.Name,
                    Extension = file.Extension,
                    SizeBytes = file.Length,
                    UploadedAt = file.CreationTimeUtc,
                    ModifiedAt = file.LastWriteTimeUtc,
                    StoragePath = storagePath
                };
            })
            .OrderByDescending(x => x.UploadedAt)
            .ToList();

        return Ok(items);
    }

    [HttpPost("backups")]
    [RequestSizeLimit(MaxBackupSizeBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxBackupSizeBytes)]
    public async Task<IActionResult> UploadBackup(
        [FromForm] IFormFile? file,
        [FromForm] string? uploadedBy,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { Message = "Debe seleccionar un archivo de backup." });
        }

        if (file.Length > MaxBackupSizeBytes)
        {
            return BadRequest(new { Message = "El backup supera el tamaño máximo permitido de 5 GB." });
        }

        var originalName = Path.GetFileName(file.FileName);
        var extension = Path.GetExtension(originalName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                Message = "Formato no permitido. Use .bak, .bacpac, .zip o .7z."
            });
        }

        var storagePath = EnsureBackupStoragePath();
        var safeName = BuildSafeBackupName(originalName);
        var fullPath = Path.Combine(storagePath, safeName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var infoPath = fullPath + ".info.txt";

        await System.IO.File.WriteAllTextAsync(
            infoPath,
            $"""
            NAVI Herramientas & Activos - Backup cargado

            Archivo original: {originalName}
            Archivo almacenado: {safeName}
            Tamaño bytes: {file.Length}
            Cargado por: {(string.IsNullOrWhiteSpace(uploadedBy) ? "admin-web" : uploadedBy)}
            Fecha UTC: {DateTime.UtcNow:O}
            Estado: Cargado. Restauración/exportación pendiente de implementación.
            """,
            cancellationToken);

        return Ok(new BackupUploadResponse
        {
            FileName = safeName,
            OriginalFileName = originalName,
            SizeBytes = file.Length,
            UploadedAt = DateTime.UtcNow,
            Message = "Backup cargado correctamente. La restauración y exportación quedan pendientes para la siguiente fase."
        });
    }

    private string EnsureBackupStoragePath()
    {
        var configuredPath = _configuration["DatabaseBackups:StoragePath"];

        var storagePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(_environment.ContentRootPath, "storage", "database-backups")
            : configuredPath;

        Directory.CreateDirectory(storagePath);

        return storagePath;
    }

    private static string BuildSafeBackupName(string originalName)
    {
        var extension = Path.GetExtension(originalName);
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalName);

        var safeBase = new string(
            nameWithoutExtension
                .Select(character =>
                    char.IsLetterOrDigit(character) ||
                    character is '-' or '_' or '.'
                        ? character
                        : '_')
                .ToArray());

        if (string.IsNullOrWhiteSpace(safeBase))
        {
            safeBase = "backup";
        }

        return $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{safeBase}{extension}";
    }

    public sealed class BackupFileResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string Extension { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public string StoragePath { get; set; } = string.Empty;
    }

    public sealed class BackupUploadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public DateTime UploadedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}

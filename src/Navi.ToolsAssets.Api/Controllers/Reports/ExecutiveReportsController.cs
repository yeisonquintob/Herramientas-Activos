using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Api.Security;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;
using Navi.ToolsAssets.Shared.Security;

namespace Navi.ToolsAssets.Api.Controllers.Reports;

/// <summary>
/// Consultas ejecutivas por compañía. El aislamiento se obtiene del
/// NaviToolsAssetsDbContext resuelto para la compañía activa.
/// </summary>
[ApiController]
[Route("api/reports/executive")]
[RequirePermission(PermissionCodes.ReportsView)]
public sealed class ExecutiveReportsController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ExecutiveReportsController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] ExecutiveReportFilter filter,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var tools = ApplyFilter(_context.ToolAssets.AsNoTracking(), filter);

        var statusCounts = await tools
            .GroupBy(tool => tool.OperationalStatus)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);

        var inventory = new
        {
            Total = statusCounts.Values.Sum(),
            Available = Count(statusCounts, ToolOperationalStatus.Available),
            Assigned = Count(statusCounts, ToolOperationalStatus.Assigned),
            Loaned = Count(statusCounts, ToolOperationalStatus.Loaned),
            InMaintenance = Count(statusCounts, ToolOperationalStatus.InMaintenance),
            Damaged = Count(statusCounts, ToolOperationalStatus.Damaged),
            NotSuitable = Count(statusCounts, ToolOperationalStatus.NotSuitable),
            NotLocated = Count(statusCounts, ToolOperationalStatus.NotLocated)
        };

        var withoutLocation = await tools.CountAsync(tool => tool.LocationId == null, cancellationToken);
        var withoutResponsible = await tools.CountAsync(tool => tool.ResponsiblePersonId == null, cancellationToken);
        var withDocuments = await tools.CountAsync(tool => tool.Documents.Any(document => !document.IsDeleted), cancellationToken);
        var withoutDocuments = inventory.Total - withDocuments;
        var withoutTechnicalRecord = await tools.CountAsync(
            tool => string.IsNullOrWhiteSpace(tool.Brand)
                || string.IsNullOrWhiteSpace(tool.Model)
                || string.IsNullOrWhiteSpace(tool.SerialNumber),
            cancellationToken);
        var certificationOverdue = await tools.CountAsync(
            tool => tool.CertificationExpirationDate != null
                && tool.CertificationExpirationDate < now,
            cancellationToken);
        var maintenanceOverdue = await tools.CountAsync(
            tool => tool.NextMaintenanceDate != null && tool.NextMaintenanceDate < now,
            cancellationToken);

        var physicalCounts = _context.PhysicalCounts.AsNoTracking();
        if (filter.BranchId.HasValue)
        {
            physicalCounts = physicalCounts.Where(count => count.BranchId == filter.BranchId.Value);
        }

        if (filter.DateFrom.HasValue)
        {
            physicalCounts = physicalCounts.Where(count => count.StartedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            physicalCounts = physicalCounts.Where(count => count.StartedAt < filter.DateTo.Value.AddDays(1));
        }

        var physicalCountIds = physicalCounts.Select(count => count.Id);
        var countItems = _context.PhysicalCountItems
            .AsNoTracking()
            .Where(item => physicalCountIds.Contains(item.PhysicalCountId));

        var importBatches = _context.ImportBatches.AsNoTracking();
        if (filter.DateFrom.HasValue)
        {
            importBatches = importBatches.Where(batch => batch.ProcessedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            importBatches = importBatches.Where(batch => batch.ProcessedAt < filter.DateTo.Value.AddDays(1));
        }

        var importQuality = await importBatches
            .GroupBy(_ => 1)
            .Select(group => new
            {
                Batches = group.Count(),
                Rows = group.Sum(batch => batch.TotalRows),
                Valid = group.Sum(batch => batch.ValidRows),
                Warnings = group.Sum(batch => batch.WarningRows),
                Invalid = group.Sum(batch => batch.ErrorRows),
                Duplicates = group.Sum(batch => batch.DuplicateRows),
                Created = group.Sum(batch => batch.CreatedTools),
                Updated = group.Sum(batch => batch.UpdatedTools),
                Ignored = group.Sum(batch => batch.IgnoredRows)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return Ok(new
        {
            GeneratedAtUtc = now,
            Filters = filter,
            Inventory = inventory,
            Completeness = new
            {
                WithoutLocation = withoutLocation,
                WithoutResponsible = withoutResponsible,
                WithDocuments = withDocuments,
                WithoutDocuments = withoutDocuments,
                WithoutTechnicalRecord = withoutTechnicalRecord
            },
            Expirations = new
            {
                Certification = certificationOverdue,
                Maintenance = maintenanceOverdue
            },
            PhysicalCounts = new
            {
                Campaigns = await physicalCounts.CountAsync(cancellationToken),
                Results = await countItems.CountAsync(cancellationToken),
                Differences = await countItems.CountAsync(
                    item => !item.WasFound || item.ExpectedLocation != item.FoundLocation,
                    cancellationToken)
            },
            ImportQuality = importQuality ?? new
            {
                Batches = 0,
                Rows = 0,
                Valid = 0,
                Warnings = 0,
                Invalid = 0,
                Duplicates = 0,
                Created = 0,
                Updated = 0,
                Ignored = 0
            }
        });
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory(
        [FromQuery] ExecutiveReportFilter filter,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 200);

        var query = ApplyFilter(_context.ToolAssets.AsNoTracking(), filter);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(tool => tool.InternalCode)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(tool => new
            {
                tool.Id,
                tool.InternalCode,
                tool.Name,
                tool.SerialNumber,
                tool.FixedAssetCode,
                Branch = tool.Branch == null ? null : tool.Branch.Code,
                Zone = tool.Zone == null ? null : tool.Zone.Code,
                Location = tool.Location == null ? null : tool.Location.Code,
                Responsible = tool.ResponsiblePerson == null ? null : tool.ResponsiblePerson.FullName,
                Type = tool.ToolType == null ? null : tool.ToolType.Name,
                Category = tool.ToolCategory == null ? null : tool.ToolCategory.Name,
                Status = tool.OperationalStatus.ToString(),
                HasDocuments = tool.Documents.Any(document => !document.IsDeleted),
                tool.NextMaintenanceDate,
                tool.CertificationExpirationDate
            })
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        });
    }

    [HttpGet("inventory.csv")]
    [RequirePermission(PermissionCodes.ReportsExport)]
    public async Task<IActionResult> ExportInventory(
        [FromQuery] ExecutiveReportFilter filter,
        CancellationToken cancellationToken)
    {
        Response.ContentType = "text/csv; charset=utf-8";
        Response.Headers.ContentDisposition =
            $"attachment; filename=\"inventario-ejecutivo-{DateTime.UtcNow:yyyyMMddHHmmss}.csv\"";

        await using var writer = new StreamWriter(
            Response.Body,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            16 * 1024,
            leaveOpen: true);

        await writer.WriteLineAsync(
            "Codigo,Nombre,Serial,ActivoFijo,Sede,Zona,Ubicacion,Responsable,Tipo,Categoria,Estado");

        var query = ApplyFilter(_context.ToolAssets.AsNoTracking(), filter)
            .OrderBy(tool => tool.InternalCode)
            .Select(tool => new
            {
                tool.InternalCode,
                tool.Name,
                tool.SerialNumber,
                tool.FixedAssetCode,
                Branch = tool.Branch == null ? null : tool.Branch.Code,
                Zone = tool.Zone == null ? null : tool.Zone.Code,
                Location = tool.Location == null ? null : tool.Location.Code,
                Responsible = tool.ResponsiblePerson == null ? null : tool.ResponsiblePerson.FullName,
                Type = tool.ToolType == null ? null : tool.ToolType.Name,
                Category = tool.ToolCategory == null ? null : tool.ToolCategory.Name,
                Status = tool.OperationalStatus.ToString()
            })
            .AsAsyncEnumerable();

        await foreach (var item in query.WithCancellation(cancellationToken))
        {
            await writer.WriteLineAsync(string.Join(',',
                Csv(item.InternalCode),
                Csv(item.Name),
                Csv(item.SerialNumber),
                Csv(item.FixedAssetCode),
                Csv(item.Branch),
                Csv(item.Zone),
                Csv(item.Location),
                Csv(item.Responsible),
                Csv(item.Type),
                Csv(item.Category),
                Csv(item.Status)));
        }

        await writer.FlushAsync(cancellationToken);
        return new EmptyResult();
    }

    private static IQueryable<ToolAsset> ApplyFilter(
        IQueryable<ToolAsset> query,
        ExecutiveReportFilter filter)
    {
        if (filter.DateFrom.HasValue)
        {
            query = query.Where(tool => tool.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            query = query.Where(tool => tool.CreatedAt < filter.DateTo.Value.AddDays(1));
        }

        if (filter.BranchId.HasValue)
        {
            query = query.Where(tool => tool.BranchId == filter.BranchId.Value);
        }

        if (filter.ZoneId.HasValue)
        {
            query = query.Where(tool => tool.ZoneId == filter.ZoneId.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(tool => tool.ToolCategoryId == filter.CategoryId.Value);
        }

        if (filter.ResponsiblePersonId.HasValue)
        {
            query = query.Where(tool => tool.ResponsiblePersonId == filter.ResponsiblePersonId.Value);
        }

        if (filter.ToolTypeId.HasValue)
        {
            query = query.Where(tool => tool.ToolTypeId == filter.ToolTypeId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(tool => tool.OperationalStatus == filter.Status.Value);
        }

        return query;
    }

    private static int Count(
        IReadOnlyDictionary<ToolOperationalStatus, int> values,
        ToolOperationalStatus status) =>
        values.TryGetValue(status, out var count) ? count : 0;

    private static string Csv(string? value)
    {
        var safeValue = value ?? string.Empty;
        if (safeValue.Length > 0 && safeValue[0] is '=' or '+' or '-' or '@')
        {
            safeValue = $"'{safeValue}";
        }

        return $"\"{safeValue.Replace("\"", "\"\"")}\"";
    }
}

public sealed class ExecutiveReportFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? ZoneId { get; set; }
    public Guid? CategoryId { get; set; }
    public ToolOperationalStatus? Status { get; set; }
    public Guid? ResponsiblePersonId { get; set; }
    public Guid? ToolTypeId { get; set; }
}

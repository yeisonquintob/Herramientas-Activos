using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolTechnicalDataController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ToolTechnicalDataController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpPatch("{id:guid}/technical-data")]
    public async Task<IActionResult> UpdateById(Guid id, [FromBody] UpdateToolTechnicalDataRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await UpdateTechnicalDataAsync(tool, request, cancellationToken);
    }

    [HttpPatch("by-code/{internalCode}/technical-data")]
    public async Task<IActionResult> UpdateByCode(string internalCode, [FromBody] UpdateToolTechnicalDataRequest request, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await UpdateTechnicalDataAsync(tool, request, cancellationToken);
    }

    private async Task<IActionResult> UpdateTechnicalDataAsync(
        Navi.ToolsAssets.Domain.Entities.Inventory.ToolAsset tool,
        UpdateToolTechnicalDataRequest request,
        CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "yquinto" : request.UpdatedBy.Trim();

        var changes = new List<string>();

        ApplyStringChange(nameof(tool.Brand), tool.Brand, request.Brand, value => tool.Brand = value, changes);
        ApplyStringChange(nameof(tool.Model), tool.Model, request.Model, value => tool.Model = value, changes);
        ApplyStringChange(nameof(tool.SerialNumber), tool.SerialNumber, request.SerialNumber, value => tool.SerialNumber = value, changes);
        ApplyStringChange(nameof(tool.FixedAssetCode), tool.FixedAssetCode, request.FixedAssetCode, value => tool.FixedAssetCode = value, changes);
        ApplyStringChange(nameof(tool.FenixCode), tool.FenixCode, request.FenixCode, value => tool.FenixCode = value, changes);
        ApplyStringChange(nameof(tool.Voltage), tool.Voltage, request.Voltage, value => tool.Voltage = value, changes);
        ApplyStringChange(nameof(tool.LoadCapacity), tool.LoadCapacity, request.LoadCapacity, value => tool.LoadCapacity = value, changes);
        ApplyStringChange(nameof(tool.Provider), tool.Provider, request.Provider, value => tool.Provider = value, changes);
        ApplyStringChange(nameof(tool.WarrantyType), tool.WarrantyType, request.WarrantyType, value => tool.WarrantyType = value, changes);

        ApplyNullableDateChange(nameof(tool.AcquisitionDate), tool.AcquisitionDate, request.AcquisitionDate, value => tool.AcquisitionDate = value, changes);
        ApplyNullableDateChange(nameof(tool.UsefulLifeStartDate), tool.UsefulLifeStartDate, request.UsefulLifeStartDate, value => tool.UsefulLifeStartDate = value, changes);
        ApplyNullableDateChange(nameof(tool.LastMaintenanceDate), tool.LastMaintenanceDate, request.LastMaintenanceDate, value => tool.LastMaintenanceDate = value, changes);
        ApplyNullableDateChange(nameof(tool.NextMaintenanceDate), tool.NextMaintenanceDate, request.NextMaintenanceDate, value => tool.NextMaintenanceDate = value, changes);
        ApplyNullableDateChange(nameof(tool.CertificationExpirationDate), tool.CertificationExpirationDate, request.CertificationExpirationDate, value => tool.CertificationExpirationDate = value, changes);

        ApplyNullableIntChange(nameof(tool.UsefulLifeMonths), tool.UsefulLifeMonths, request.UsefulLifeMonths, value => tool.UsefulLifeMonths = value, changes);
        ApplyNullableIntChange(nameof(tool.UsefulLifeDays), tool.UsefulLifeDays, request.UsefulLifeDays, value => tool.UsefulLifeDays = value, changes);
        ApplyNullableIntChange(nameof(tool.MaintenancePeriodMonths), tool.MaintenancePeriodMonths, request.MaintenancePeriodMonths, value => tool.MaintenancePeriodMonths = value, changes);

        ApplyBoolChange(nameof(tool.HasWarranty), tool.HasWarranty, request.HasWarranty, value => tool.HasWarranty = value, changes);
        ApplyBoolChange(nameof(tool.RequiresMaintenance), tool.RequiresMaintenance, request.RequiresMaintenance, value => tool.RequiresMaintenance = value, changes);
        ApplyBoolChange(nameof(tool.RequiresPreOperationalCheck), tool.RequiresPreOperationalCheck, request.RequiresPreOperationalCheck, value => tool.RequiresPreOperationalCheck = value, changes);
        ApplyBoolChange(nameof(tool.RequiresCertification), tool.RequiresCertification, request.RequiresCertification, value => tool.RequiresCertification = value, changes);

        if (changes.Count == 0)
        {
            return Ok(new
            {
                Message = "No se detectaron cambios en los datos técnicos.",
                tool.Id,
                tool.InternalCode,
                tool.Name,
                Changes = changes
            });
        }

        tool.UpdatedAt = DateTime.UtcNow;
        tool.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = tool.Id,
            EventType = "TechnicalDataUpdated",
            Title = "Datos técnicos actualizados",
            Description = string.IsNullOrWhiteSpace(request.Observation)
                ? "Se actualizaron datos técnicos de la hoja de vida."
                : request.Observation.Trim(),
            PreviousValue = null,
            NewValue = string.Join(" | ", changes),
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Datos técnicos actualizados correctamente.",
            tool.Id,
            tool.InternalCode,
            tool.Name,
            Changes = changes
        });
    }

    private static void ApplyStringChange(
        string fieldName,
        string? currentValue,
        string? newValue,
        Action<string?> assign,
        List<string> changes)
    {
        if (newValue is null)
        {
            return;
        }

        var normalizedNewValue = string.IsNullOrWhiteSpace(newValue) ? null : newValue.Trim();
        var normalizedCurrentValue = string.IsNullOrWhiteSpace(currentValue) ? null : currentValue.Trim();

        if (string.Equals(normalizedCurrentValue, normalizedNewValue, StringComparison.Ordinal))
        {
            return;
        }

        assign(normalizedNewValue);
        changes.Add($"{fieldName}: '{normalizedCurrentValue ?? "-"}' => '{normalizedNewValue ?? "-"}'");
    }

    private static void ApplyNullableDateChange(
        string fieldName,
        DateTime? currentValue,
        DateTime? newValue,
        Action<DateTime?> assign,
        List<string> changes)
    {
        if (!newValue.HasValue)
        {
            return;
        }

        var normalizedNewValue = newValue.Value.Date;
        var normalizedCurrentValue = currentValue?.Date;

        if (normalizedCurrentValue == normalizedNewValue)
        {
            return;
        }

        assign(normalizedNewValue);
        changes.Add($"{fieldName}: '{FormatDate(normalizedCurrentValue)}' => '{FormatDate(normalizedNewValue)}'");
    }

    private static void ApplyNullableIntChange(
        string fieldName,
        int? currentValue,
        int? newValue,
        Action<int?> assign,
        List<string> changes)
    {
        if (!newValue.HasValue)
        {
            return;
        }

        if (currentValue == newValue)
        {
            return;
        }

        assign(newValue);
        changes.Add($"{fieldName}: '{currentValue?.ToString() ?? "-"}' => '{newValue.Value}'");
    }

    private static void ApplyBoolChange(
        string fieldName,
        bool currentValue,
        bool? newValue,
        Action<bool> assign,
        List<string> changes)
    {
        if (!newValue.HasValue)
        {
            return;
        }

        if (currentValue == newValue.Value)
        {
            return;
        }

        assign(newValue.Value);
        changes.Add($"{fieldName}: '{BoolText(currentValue)}' => '{BoolText(newValue.Value)}'");
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("yyyy-MM-dd") : "-";
    }

    private static string BoolText(bool value)
    {
        return value ? "Sí" : "No";
    }
}

public sealed class UpdateToolTechnicalDataRequest
{
    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? FixedAssetCode { get; set; }

    public string? FenixCode { get; set; }

    public DateTime? AcquisitionDate { get; set; }

    public string? Voltage { get; set; }

    public string? LoadCapacity { get; set; }

    public string? Provider { get; set; }

    public bool? HasWarranty { get; set; }

    public string? WarrantyType { get; set; }

    public DateTime? UsefulLifeStartDate { get; set; }

    public int? UsefulLifeMonths { get; set; }

    public int? UsefulLifeDays { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }

    public DateTime? NextMaintenanceDate { get; set; }

    public int? MaintenancePeriodMonths { get; set; }

    public bool? RequiresMaintenance { get; set; }

    public bool? RequiresPreOperationalCheck { get; set; }

    public bool? RequiresCertification { get; set; }

    public DateTime? CertificationExpirationDate { get; set; }

    public string? UpdatedBy { get; set; }

    public string? Observation { get; set; }
}

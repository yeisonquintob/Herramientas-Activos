using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Documents;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Entities.Safety;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class TechnicalLifeRecordExcelExportController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public TechnicalLifeRecordExcelExportController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id:guid}/technical-life-record/export-excel")]
    public async Task<IActionResult> ExportById(Guid id, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await BuildExcelAsync(tool, cancellationToken);
    }

    [HttpGet("by-code/{internalCode}/technical-life-record/export-excel")]
    public async Task<IActionResult> ExportByCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await BuildExcelAsync(tool, cancellationToken);
    }

    private async Task<IActionResult> BuildExcelAsync(ToolAsset tool, CancellationToken cancellationToken)
    {
        var accessories = await _context.Set<ToolAccessory>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var safePractices = await _context.Set<ToolSafePractice>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && x.IsActive && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.PracticeName)
            .ToListAsync(cancellationToken);

        var documents = await _context.Set<ToolDocument>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && !x.IsDeleted)
            .OrderByDescending(x => x.UploadedAt)
            .ToListAsync(cancellationToken);

        var maintenanceRecords = await _context.Set<MaintenanceRecord>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && !x.IsDeleted)
            .OrderByDescending(x => x.ScheduledAt)
            .ToListAsync(cancellationToken);

        var events = await _context.Set<ToolLifeCycleEvent>()
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && !x.IsDeleted)
            .OrderByDescending(x => x.RegisteredAt)
            .Take(100)
            .ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();

        BuildMainSheet(workbook, tool, accessories, safePractices, documents, maintenanceRecords, events);
        BuildMaintenanceSheet(workbook, maintenanceRecords, documents);
        BuildDocumentsSheet(workbook, documents);
        BuildEventsSheet(workbook, events);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileName = $"HojaVidaTecnica_{SanitizeFileName(tool.InternalCode)}_{DateTime.Now:yyyyMMddHHmm}.xlsx";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName);
    }

    private static void BuildMainSheet(
        XLWorkbook workbook,
        ToolAsset tool,
        List<ToolAccessory> accessories,
        List<ToolSafePractice> safePractices,
        List<ToolDocument> documents,
        List<MaintenanceRecord> maintenanceRecords,
        List<ToolLifeCycleEvent> events)
    {
        var ws = workbook.Worksheets.Add("Hoja de vida");
        var row = 1;

        AddTitle(ws, ref row, "HOJA DE VIDA TÉCNICA DE HERRAMIENTA");
        AddKeyValue(ws, ref row, "Código interno", tool.InternalCode);
        AddKeyValue(ws, ref row, "Nombre", tool.Name);
        AddKeyValue(ws, ref row, "Marca", tool.Brand);
        AddKeyValue(ws, ref row, "Modelo", tool.Model);
        AddKeyValue(ws, ref row, "Serial", tool.SerialNumber);
        AddKeyValue(ws, ref row, "Activo fijo", tool.FixedAssetCode);
        AddKeyValue(ws, ref row, "Código Fenix365", tool.FenixCode);
        AddKeyValue(ws, ref row, "Estado operativo", tool.OperationalStatus.ToString());
        AddKeyValue(ws, ref row, "Estado físico", tool.PhysicalStatus.ToString());
        AddKeyValue(ws, ref row, "Custodia", tool.CustodyStatus.ToString());

        row++;
        AddSection(ws, ref row, "ESPECIFICACIONES TÉCNICAS");
        AddKeyValue(ws, ref row, "Voltaje", tool.Voltage);
        AddKeyValue(ws, ref row, "Capacidad de carga", tool.LoadCapacity);
        AddKeyValue(ws, ref row, "Proveedor", tool.Provider);
        AddKeyValue(ws, ref row, "Requiere mantenimiento", BoolText(tool.RequiresMaintenance));
        AddKeyValue(ws, ref row, "Requiere preoperacional", BoolText(tool.RequiresPreOperationalCheck));
        AddKeyValue(ws, ref row, "Requiere certificación", BoolText(tool.RequiresCertification));
        AddKeyValue(ws, ref row, "Vencimiento certificación", FormatDate(tool.CertificationExpirationDate));

        row++;
        AddSection(ws, ref row, "GARANTÍA Y VIDA ÚTIL");
        AddKeyValue(ws, ref row, "Tiene garantía", BoolText(tool.HasWarranty));
        AddKeyValue(ws, ref row, "Tipo garantía", tool.WarrantyType);
        AddKeyValue(ws, ref row, "Fecha adquisición", FormatDate(tool.AcquisitionDate));
        AddKeyValue(ws, ref row, "Inicio vida útil", FormatDate(tool.UsefulLifeStartDate));
        AddKeyValue(ws, ref row, "Vida útil meses", tool.UsefulLifeMonths?.ToString());
        AddKeyValue(ws, ref row, "Vida útil días", tool.UsefulLifeDays?.ToString());
        AddKeyValue(ws, ref row, "Último mantenimiento", FormatDate(tool.LastMaintenanceDate));
        AddKeyValue(ws, ref row, "Próximo mantenimiento", FormatDate(tool.NextMaintenanceDate));
        AddKeyValue(ws, ref row, "Periodicidad mantenimiento meses", tool.MaintenancePeriodMonths?.ToString());

        row++;
        AddSection(ws, ref row, "ACCESORIOS");
        AddHeader(ws, ref row, "Accesorio", "Cantidad", "Requiere mantenimiento", "Equipo medición", "Observación");

        foreach (var item in accessories)
        {
            AddRow(ws, ref row,
                item.Name,
                item.Quantity.ToString(),
                BoolText(item.RequiresMaintenance),
                BoolText(item.IsMeasurementEquipment),
                item.Observation);
        }

        row++;
        AddSection(ws, ref row, "DOCUMENTOS");
        AddHeader(ws, ref row, "Tipo", "Archivo", "Subido por", "Fecha", "Descripción");

        foreach (var item in documents)
        {
            AddRow(ws, ref row,
                item.DocumentType.ToString(),
                item.FileName,
                item.UploadedBy,
                FormatDateTime(item.UploadedAt),
                item.Description);
        }

        row++;
        AddSection(ws, ref row, "CRONOGRAMA DE MANTENIMIENTO");
        AddHeader(ws, ref row, "Número", "Tipo", "Estado", "Programado", "Proveedor", "Técnico", "Factura", "Evidencia", "Responsable", "Operativa");

        foreach (var item in maintenanceRecords)
        {
            var evidence = documents.FirstOrDefault(x => x.Id == item.EvidenceDocumentId);

            AddRow(ws, ref row,
                item.MaintenanceNumber,
                item.Type.ToString(),
                item.Status.ToString(),
                FormatDate(item.ScheduledAt),
                item.Provider,
                item.Technician,
                item.InvoiceNumber,
                evidence?.FileName,
                item.ResponsibleName,
                item.IsToolOperational == true ? "Sí" : "No");
        }

        row++;
        AddSection(ws, ref row, "PRÁCTICAS SEGURAS");
        AddHeader(ws, ref row, "Orden", "Práctica", "Descripción");

        foreach (var item in safePractices)
        {
            AddRow(ws, ref row,
                item.SortOrder.ToString(),
                item.PracticeName,
                item.Description);
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(1);
    }

    private static void BuildMaintenanceSheet(
        XLWorkbook workbook,
        List<MaintenanceRecord> records,
        List<ToolDocument> documents)
    {
        var ws = workbook.Worksheets.Add("Mantenimientos");
        var row = 1;

        AddTitle(ws, ref row, "MANTENIMIENTOS");
        AddHeader(ws, ref row, "Número", "Tipo", "Estado", "Programado", "Inicio", "Fin", "Proveedor", "Técnico", "Actividades", "Notas", "Factura", "Responsable", "Cargo", "Evidencia", "Costo", "Resultado");

        foreach (var item in records)
        {
            var evidence = documents.FirstOrDefault(x => x.Id == item.EvidenceDocumentId);

            AddRow(ws, ref row,
                item.MaintenanceNumber,
                item.Type.ToString(),
                item.Status.ToString(),
                FormatDate(item.ScheduledAt),
                FormatDate(item.StartedAt),
                FormatDate(item.FinishedAt),
                item.Provider,
                item.Technician,
                item.MaintenanceActivities,
                item.ExecutionNotes,
                item.InvoiceNumber,
                item.ResponsibleName,
                item.ResponsiblePosition,
                evidence?.FileName,
                item.Cost?.ToString("0"),
                item.Result);
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(2);
    }

    private static void BuildDocumentsSheet(XLWorkbook workbook, List<ToolDocument> documents)
    {
        var ws = workbook.Worksheets.Add("Documentos");
        var row = 1;

        AddTitle(ws, ref row, "DOCUMENTOS");
        AddHeader(ws, ref row, "Tipo", "Archivo", "Content Type", "Tamaño bytes", "Subido por", "Fecha", "Descripción", "Object Key");

        foreach (var item in documents)
        {
            AddRow(ws, ref row,
                item.DocumentType.ToString(),
                item.FileName,
                item.ContentType,
                item.SizeBytes.ToString(),
                item.UploadedBy,
                FormatDateTime(item.UploadedAt),
                item.Description,
                item.ObjectKey);
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(2);
    }

    private static void BuildEventsSheet(XLWorkbook workbook, List<ToolLifeCycleEvent> events)
    {
        var ws = workbook.Worksheets.Add("Eventos");
        var row = 1;

        AddTitle(ws, ref row, "EVENTOS DE HOJA DE VIDA");
        AddHeader(ws, ref row, "Fecha", "Tipo", "Título", "Descripción", "Valor anterior", "Valor nuevo", "Registrado por");

        foreach (var item in events)
        {
            AddRow(ws, ref row,
                FormatDateTime(item.RegisteredAt),
                item.EventType,
                item.Title,
                item.Description,
                item.PreviousValue,
                item.NewValue,
                item.RegisteredBy);
        }

        ws.Columns().AdjustToContents();
        ws.SheetView.FreezeRows(2);
    }

    private static void AddTitle(IXLWorksheet ws, ref int row, string title)
    {
        ws.Cell(row, 1).Value = title;
        ws.Range(row, 1, row, 8).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Font.FontSize = 16;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#4B005C");
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
        row += 2;
    }

    private static void AddSection(IXLWorksheet ws, ref int row, string title)
    {
        ws.Cell(row, 1).Value = title;
        ws.Range(row, 1, row, 8).Merge();
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#111111");
        ws.Cell(row, 1).Style.Font.FontColor = XLColor.White;
        row++;
    }

    private static void AddKeyValue(IXLWorksheet ws, ref int row, string key, string? value)
    {
        ws.Cell(row, 1).Value = key;
        ws.Cell(row, 1).Style.Font.Bold = true;
        ws.Cell(row, 2).Value = string.IsNullOrWhiteSpace(value) ? "-" : value;
        row++;
    }

    private static void AddHeader(IXLWorksheet ws, ref int row, params string[] headers)
    {
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#EDEDED");
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        row++;
    }

    private static void AddRow(IXLWorksheet ws, ref int row, params string?[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            var cell = ws.Cell(row, i + 1);
            cell.Value = string.IsNullOrWhiteSpace(values[i]) ? "-" : values[i];
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Alignment.WrapText = true;
        }

        row++;
    }

    private static string FormatDate(DateTime? value)
    {
        return value.HasValue ? value.Value.ToString("dd/MM/yyyy") : "-";
    }

    private static string FormatDateTime(DateTime value)
    {
        return value.ToString("dd/MM/yyyy HH:mm");
    }

    private static string BoolText(bool value)
    {
        return value ? "Sí" : "No";
    }

    private static string SanitizeFileName(string value)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(c, '-');
        }

        return value;
    }
}

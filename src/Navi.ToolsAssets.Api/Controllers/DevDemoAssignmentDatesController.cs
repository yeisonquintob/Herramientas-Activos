using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/dev/demo")]
public sealed class DevDemoAssignmentDatesController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DevDemoAssignmentDatesController(
        NaviToolsAssetsDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("apply-branch-assignment-dates")]
    public async Task<IActionResult> ApplyBranchAssignmentDates(
        [FromBody] ApplyBranchAssignmentDatesRequest request,
        CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return BadRequest(new
            {
                Message = "Este endpoint solo puede ejecutarse en ambiente Development."
            });
        }

        if (!string.Equals(request.Confirmation, "CONFIRMAR_FECHAS_ASIGNACION", StringComparison.Ordinal))
        {
            return BadRequest(new
            {
                Message = "Confirmación inválida. Debes enviar CONFIRMAR_FECHAS_ASIGNACION."
            });
        }

        var changedBy = string.IsNullOrWhiteSpace(request.ChangedBy)
            ? "admin-demo-assignment-dates"
            : request.ChangedBy.Trim();

        var baseDate = request.BaseDate?.Date ?? new DateTime(2026, 1, 5);

        var branches = await _context.Branches
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new
            {
                x.Id,
                x.Code,
                x.Name
            })
            .ToListAsync(cancellationToken);

        var tools = await _context.ToolAssets
            .Include(x => x.Branch)
            .Include(x => x.ResponsiblePerson)
            .Where(x =>
                !x.IsDeleted &&
                x.ResponsiblePersonId != null)
            .OrderBy(x => x.Branch!.Code)
            .ThenBy(x => x.InternalCode)
            .ToListAsync(cancellationToken);

        var toolIds = tools.Select(x => x.Id).ToList();

        var oldAssignmentEvents = await _context.ToolLifeCycleEvents
            .Where(x =>
                toolIds.Contains(x.ToolAssetId) &&
                x.EventType == "FixedAssetAssignedToResponsible")
            .ToListAsync(cancellationToken);

        _context.ToolLifeCycleEvents.RemoveRange(oldAssignmentEvents);

        var branchDates = branches
            .Select((branch, index) => new
            {
                branch.Id,
                branch.Code,
                branch.Name,
                AssignmentDate = baseDate.AddDays(index * request.DaysBetweenBranches)
            })
            .ToDictionary(x => x.Id, x => x);

        var createdEvents = 0;

        foreach (var tool in tools)
        {
            if (!branchDates.TryGetValue(tool.BranchId, out var branchDate))
            {
                continue;
            }

            var assignmentDate = branchDate.AssignmentDate
                .AddHours(8)
                .AddMinutes(30);

            tool.UpdatedAt = DateTime.UtcNow;
            tool.UpdatedBy = changedBy;

            _context.ToolLifeCycleEvents.Add(new ToolLifeCycleEvent
            {
                ToolAssetId = tool.Id,
                EventType = "FixedAssetAssignedToResponsible",
                Title = "Activo asignado a responsable",
                Description = $"Asignación inicial demo por sede. Sede: {branchDate.Code} - {branchDate.Name}. Responsable: {tool.ResponsiblePerson?.FullName}.",
                PreviousValue = "Bodega / sin responsable",
                NewValue = $"{tool.ResponsiblePerson?.FullName} | {branchDate.Code} | {assignmentDate:yyyy-MM-dd}",
                RegisteredAt = assignmentDate,
                RegisteredBy = changedBy,
                CreatedAt = assignmentDate,
                CreatedBy = changedBy
            });

            createdEvents++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Fechas de asignación aplicadas correctamente por sede.",
            Branches = branchDates.Values
                .OrderBy(x => x.Code)
                .Select(x => new
                {
                    x.Code,
                    x.Name,
                    AssignmentDate = x.AssignmentDate.ToString("yyyy-MM-dd")
                })
                .ToList(),
            ToolsWithResponsible = tools.Count,
            OldAssignmentEventsRemoved = oldAssignmentEvents.Count,
            AssignmentEventsCreated = createdEvents
        });
    }
}

public sealed class ApplyBranchAssignmentDatesRequest
{
    public string Confirmation { get; set; } = string.Empty;

    public string? ChangedBy { get; set; }

    public DateTime? BaseDate { get; set; }

    public int DaysBetweenBranches { get; set; } = 7;
}

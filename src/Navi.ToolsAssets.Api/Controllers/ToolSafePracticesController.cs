using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Safety;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class ToolSafePracticesController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public ToolSafePracticesController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }


    [HttpGet("{id:guid}/safe-practices")]
    public async Task<IActionResult> GetSafePracticesByToolId(Guid id, CancellationToken cancellationToken)
    {
        var toolExists = await _context.ToolAssets
            .AnyAsync(x => x.Id == id, cancellationToken);

        if (!toolExists)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var practices = await _context.ToolSafePractices
            .AsNoTracking()
            .Where(x => x.ToolAssetId == id && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.PracticeName)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.PracticeName,
                x.Description,
                x.SortOrder,
                x.IsActive,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(practices);
    }

    [HttpGet("by-code/{internalCode}/safe-practices")]
    public async Task<IActionResult> GetSafePracticesByToolCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        var practices = await _context.ToolSafePractices
            .AsNoTracking()
            .Where(x => x.ToolAssetId == tool.Id && x.IsActive)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.PracticeName)
            .Select(x => new
            {
                x.Id,
                x.ToolAssetId,
                x.PracticeName,
                x.Description,
                x.SortOrder,
                x.IsActive,
                x.CreatedAt,
                x.CreatedBy,
                x.UpdatedAt,
                x.UpdatedBy
            })
            .ToListAsync(cancellationToken);

        return Ok(practices);
    }
    [HttpPost("{id:guid}/safe-practices")]
    public async Task<IActionResult> Create(Guid id, [FromBody] CreateToolSafePracticeRequest request, CancellationToken cancellationToken)
    {
        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await CreateSafePracticeAsync(tool, request, cancellationToken);
    }

    [HttpPost("by-code/{internalCode}/safe-practices")]
    public async Task<IActionResult> CreateByCode(string internalCode, [FromBody] CreateToolSafePracticeRequest request, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await CreateSafePracticeAsync(tool, request, cancellationToken);
    }

    [HttpPost("{id:guid}/safe-practices/defaults")]
    public async Task<IActionResult> CreateDefaults(Guid id, CancellationToken cancellationToken)
    {
        var tool = await GetToolWithSafePracticesAsync(id, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await CreateDefaultSafePracticesAsync(tool, cancellationToken);
    }

    [HttpPost("by-code/{internalCode}/safe-practices/defaults")]
    public async Task<IActionResult> CreateDefaultsByCode(string internalCode, CancellationToken cancellationToken)
    {
        var normalizedCode = internalCode.Trim().ToUpperInvariant();

        var tool = await _context.ToolAssets
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Include(x => x.SafePractices)
            .FirstOrDefaultAsync(x => x.InternalCode == normalizedCode, cancellationToken);

        if (tool is null)
        {
            return NotFound(new { Message = "No se encontró la herramienta." });
        }

        return await CreateDefaultSafePracticesAsync(tool, cancellationToken);
    }


    [HttpPut("safe-practices/{safePracticeId:guid}")]
    public async Task<IActionResult> UpdateSafePractice(Guid safePracticeId, [FromBody] UpdateToolSafePracticeRequest request, CancellationToken cancellationToken)
    {
        var practice = await _context.ToolSafePractices
            .FirstOrDefaultAsync(x => x.Id == safePracticeId, cancellationToken);

        if (practice is null)
        {
            return NotFound(new { Message = "No se encontró la práctica segura." });
        }

        if (string.IsNullOrWhiteSpace(request.PracticeName))
        {
            return BadRequest(new { Message = "El nombre de la práctica segura es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { Message = "La descripción de la práctica segura es obligatoria." });
        }

        var user = string.IsNullOrWhiteSpace(request.UpdatedBy) ? "yquinto" : request.UpdatedBy.Trim();

        var previousValue = $"{practice.SortOrder}. {practice.PracticeName}";

        practice.PracticeName = request.PracticeName.Trim();
        practice.Description = request.Description.Trim();
        practice.SortOrder = request.SortOrder;
        practice.UpdatedAt = DateTime.UtcNow;
        practice.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = practice.ToolAssetId,
            EventType = "SafePracticeUpdated",
            Title = "Práctica segura actualizada",
            Description = $"Se actualizó la práctica segura: {practice.PracticeName}.",
            PreviousValue = previousValue,
            NewValue = $"{practice.SortOrder}. {practice.PracticeName}",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Práctica segura actualizada correctamente.",
            practice.Id,
            practice.ToolAssetId,
            practice.PracticeName,
            practice.Description,
            practice.SortOrder
        });
    }

    [HttpDelete("safe-practices/{safePracticeId:guid}")]
    public async Task<IActionResult> DeleteSafePractice(Guid safePracticeId, [FromQuery] string? deletedBy, CancellationToken cancellationToken)
    {
        var practice = await _context.ToolSafePractices
            .FirstOrDefaultAsync(x => x.Id == safePracticeId, cancellationToken);

        if (practice is null)
        {
            return NotFound(new { Message = "No se encontró la práctica segura." });
        }

        var user = string.IsNullOrWhiteSpace(deletedBy) ? "yquinto" : deletedBy.Trim();

        practice.IsActive = false;
        practice.IsDeleted = true;
        practice.UpdatedAt = DateTime.UtcNow;
        practice.UpdatedBy = user;

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = practice.ToolAssetId,
            EventType = "SafePracticeDeleted",
            Title = "Práctica segura desactivada",
            Description = $"Se desactivó la práctica segura: {practice.PracticeName}.",
            PreviousValue = practice.PracticeName,
            NewValue = "Desactivada",
            RegisteredBy = user,
            CreatedBy = user
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Práctica segura desactivada correctamente.",
            practice.Id,
            practice.ToolAssetId,
            practice.PracticeName
        });
    }
    private async Task<ToolAsset?> GetToolWithSafePracticesAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.ToolAssets
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Include(x => x.SafePractices)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private async Task<IActionResult> CreateSafePracticeAsync(ToolAsset tool, CreateToolSafePracticeRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.PracticeName))
        {
            return BadRequest(new { Message = "El nombre de la práctica segura es obligatorio." });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { Message = "La descripción de la práctica segura es obligatoria." });
        }

        var practice = new ToolSafePractice
        {
            ToolAssetId = tool.Id,
            PracticeName = request.PracticeName.Trim(),
            Description = request.Description.Trim(),
            SortOrder = request.SortOrder,
            CreatedBy = string.IsNullOrWhiteSpace(request.CreatedBy) ? "yquinto" : request.CreatedBy
        };

        _context.ToolSafePractices.Add(practice);

        _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
        {
            ToolAssetId = tool.Id,
            EventType = "SafePracticeCreated",
            Title = "Práctica segura registrada",
            Description = $"Se registró la práctica segura: {practice.PracticeName}.",
            NewValue = practice.PracticeName,
            RegisteredBy = practice.CreatedBy,
            CreatedBy = practice.CreatedBy
        });

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Práctica segura creada correctamente.",
            practice.Id,
            practice.ToolAssetId,
            practice.PracticeName,
            practice.Description,
            practice.SortOrder
        });
    }

    private async Task<IActionResult> CreateDefaultSafePracticesAsync(ToolAsset tool, CancellationToken cancellationToken)
    {
        var defaults = GetDefaultSafePractices(tool);

        var existingNames = tool.SafePractices
            .Where(x => !x.IsDeleted)
            .Select(x => x.PracticeName.Trim().ToUpperInvariant())
            .ToHashSet();

        var created = new List<ToolSafePractice>();

        foreach (var item in defaults)
        {
            var normalizedName = item.PracticeName.Trim().ToUpperInvariant();

            if (existingNames.Contains(normalizedName))
            {
                continue;
            }

            var practice = new ToolSafePractice
            {
                ToolAssetId = tool.Id,
                PracticeName = item.PracticeName,
                Description = item.Description,
                SortOrder = item.SortOrder,
                CreatedBy = "yquinto"
            };

            _context.ToolSafePractices.Add(practice);
            created.Add(practice);
        }

        if (created.Count > 0)
        {
            _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
            {
                ToolAssetId = tool.Id,
                EventType = "DefaultSafePracticesCreated",
                Title = "Prácticas seguras predeterminadas registradas",
                Description = $"Se registraron {created.Count} prácticas seguras predeterminadas.",
                NewValue = string.Join(", ", created.Select(x => x.PracticeName)),
                RegisteredBy = "yquinto",
                CreatedBy = "yquinto"
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = created.Count == 0
                ? "La herramienta ya tenía registradas las prácticas seguras predeterminadas."
                : "Prácticas seguras predeterminadas creadas correctamente.",
            ToolId = tool.Id,
            tool.InternalCode,
            CreatedCount = created.Count,
            Practices = created
                .OrderBy(x => x.SortOrder)
                .Select(x => new
                {
                    x.Id,
                    x.PracticeName,
                    x.Description,
                    x.SortOrder
                })
                .ToList()
        });
    }

    private static List<DefaultSafePracticeItem> GetDefaultSafePractices(ToolAsset tool)
    {
        var text = $"{tool.InternalCode} {tool.Name} {tool.Description} {tool.ToolType?.Name} {tool.ToolCategory?.Name}".ToUpperInvariant();

        if (text.Contains("COMPUTADOR") || text.Contains("ESCANER") || text.Contains("ESCÁNER") || text.Contains("SCANNER"))
        {
            return new List<DefaultSafePracticeItem>
            {
                new(1, "Protección contra sobrecalentamiento", "Asegurar que el computador tenga suficiente ventilación y no se bloquee su sistema de refrigeración."),
                new(2, "Uso en superficies planas", "Colocar el equipo en superficies estables y planas para evitar caídas o daños."),
                new(3, "Conexión segura a redes", "Usar conexiones Wi-Fi seguras o cables certificados para evitar interferencias o pérdida de datos durante el escaneo."),
                new(4, "Protección contra descargas eléctricas", "Utilizar protector de sobrecarga para evitar daños por picos de corriente."),
                new(5, "Evitar exposición a líquidos", "Mantener el computador alejado de líquidos y polvo que puedan dañar sus componentes."),
                new(6, "Uso de antivirus y software de seguridad", "Instalar y actualizar regularmente software antivirus para proteger los datos escaneados."),
                new(7, "Copia de seguridad de datos", "Realizar copias de seguridad periódicas de los datos escaneados para evitar pérdidas."),
                new(8, "Mantenimiento regular", "Limpiar y revisar el equipo regularmente para asegurar su buen funcionamiento y prevenir fallas."),
                new(9, "Protección de la pantalla", "Utilizar protectores de pantalla o fundas para evitar rayaduras y daños físicos."),
                new(10, "Carga adecuada", "Utilizar cargadores certificados y evitar sobrecargar la batería."),
                new(11, "Desconexión segura de dispositivos", "Desconectar adecuadamente escáneres, memorias USB u otros dispositivos externos para evitar daños."),
                new(12, "Revisión de software y actualizaciones", "Mantener actualizado el software del escáner y del sistema operativo para un rendimiento óptimo.")
            };
        }

        if (text.Contains("GATO") || text.Contains("PLUMA") || text.Contains("HIDRAULICO") || text.Contains("HIDRÁULICO"))
        {
            return new List<DefaultSafePracticeItem>
            {
                new(1, "Inspección previa al uso", "Verifica el estado de herramientas, mangueras y conexiones para detectar grietas, fugas o desgaste antes de usarlas."),
                new(2, "Uso de EPP", "Usa gafas de seguridad, guantes resistentes al aceite, protección auditiva y calzado con punta reforzada y suela antideslizante."),
                new(3, "Procedimientos de operación", "Asegúrate de que las conexiones estén firmes, no excedas la presión recomendada y mantén las mangueras alejadas de bordes afilados o superficies calientes."),
                new(4, "Posición segura", "Mantén una postura estable, limpia tu área de trabajo y evita colocarte frente a herramientas presurizadas."),
                new(5, "Mantenimiento regular", "Realiza inspecciones periódicas, reemplaza componentes desgastados y utiliza repuestos recomendados por el fabricante."),
                new(6, "Prevención de fugas", "Nunca uses las manos para buscar fugas; utiliza cartón o papel y repáralas solo cuando el sistema esté despresurizado."),
                new(7, "Capacitación y señalización", "Capacita al personal en el uso seguro de herramientas y coloca señalización de advertencia en las áreas de trabajo."),
                new(8, "Despresurización al finalizar", "Antes de desconectar herramientas, libera la presión del sistema. Nunca realices ajustes mientras el equipo esté presurizado."),
                new(9, "Almacenamiento adecuado", "Guarda herramientas y mangueras en un lugar limpio y seco, evitando doblarlas de forma incorrecta.")
            };
        }

        return new List<DefaultSafePracticeItem>
        {
            new(1, "Inspección previa al uso", "Verificar el estado general de la herramienta antes de utilizarla."),
            new(2, "Uso de elementos de protección personal", "Utilizar los elementos de protección personal requeridos para la operación."),
            new(3, "Uso correcto de la herramienta", "Operar la herramienta únicamente para las actividades para las que fue diseñada."),
            new(4, "Reporte de novedades", "Reportar daños, fallas o condiciones inseguras antes y después del uso."),
            new(5, "Limpieza y almacenamiento", "Limpiar la herramienta después del uso y almacenarla en el lugar asignado.")
        };
    }

    private sealed record DefaultSafePracticeItem(int SortOrder, string PracticeName, string Description);
}

public sealed class CreateToolSafePracticeRequest
{
    public string PracticeName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string? CreatedBy { get; set; }
}


public sealed class UpdateToolSafePracticeRequest
{
    public string PracticeName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public string? UpdatedBy { get; set; }
}

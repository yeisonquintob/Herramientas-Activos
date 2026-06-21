using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Maintenance;
using Navi.ToolsAssets.Domain.Entities.Safety;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/tools")]
public class TechnicalLifeRecordMockDataController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;

    public TechnicalLifeRecordMockDataController(NaviToolsAssetsDbContext context)
    {
        _context = context;
    }

    [HttpPost("technical-life-record/mock-data/apply-all")]
    public async Task<IActionResult> ApplyMockDataToAll(
        [FromQuery] string? createdBy,
        [FromQuery] bool overwrite,
        CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(createdBy) ? "yquinto" : createdBy.Trim();

        var tools = await _context.ToolAssets
            .Include(x => x.ToolType)
            .Include(x => x.ToolCategory)
            .Include(x => x.Accessories)
            .Include(x => x.SafePractices)
            .Include(x => x.MaintenanceRecords)
            .OrderBy(x => x.InternalCode)
            .ToListAsync(cancellationToken);

        var updatedTools = 0;
        var createdAccessories = 0;
        var createdSafePractices = 0;
        var createdMaintenanceRecords = 0;

        var results = new List<object>();

        foreach (var tool in tools)
        {
            var profile = GetProfile(tool);
            var changed = false;

            if (overwrite || string.IsNullOrWhiteSpace(tool.Brand))
            {
                tool.Brand = profile.Brand;
                changed = true;
            }

            if (overwrite || string.IsNullOrWhiteSpace(tool.Model))
            {
                tool.Model = profile.Model;
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(tool.SerialNumber))
            {
                tool.SerialNumber = $"SER-{NormalizeCode(tool.InternalCode)}";
                changed = true;
            }

            if (overwrite || string.IsNullOrWhiteSpace(tool.FixedAssetCode))
            {
                tool.FixedAssetCode = $"AF-{NormalizeCode(tool.InternalCode)}";
                changed = true;
            }

            if (overwrite || string.IsNullOrWhiteSpace(tool.FenixCode))
            {
                tool.FenixCode = $"FENIX-{NormalizeCode(tool.InternalCode)}";
                changed = true;
            }

            if (overwrite || !tool.AcquisitionDate.HasValue)
            {
                tool.AcquisitionDate = new DateTime(2024, 9, 12);
                changed = true;
            }

            if (overwrite || string.IsNullOrWhiteSpace(tool.Voltage))
            {
                tool.Voltage = profile.Voltage;
                changed = true;
            }

            if (overwrite || string.IsNullOrWhiteSpace(tool.LoadCapacity))
            {
                tool.LoadCapacity = profile.LoadCapacity;
                changed = true;
            }

            if (overwrite || string.IsNullOrWhiteSpace(tool.Provider))
            {
                tool.Provider = profile.Provider;
                changed = true;
            }

            if (overwrite)
            {
                tool.HasWarranty = false;
                tool.WarrantyType = "Equipo original";
                changed = true;
            }

            if (overwrite || !tool.UsefulLifeStartDate.HasValue)
            {
                tool.UsefulLifeStartDate = new DateTime(2024, 9, 12);
                changed = true;
            }

            if (overwrite || !tool.UsefulLifeMonths.HasValue)
            {
                tool.UsefulLifeMonths = 60;
                changed = true;
            }

            if (overwrite || !tool.UsefulLifeDays.HasValue)
            {
                tool.UsefulLifeDays = 1825;
                changed = true;
            }

            if (overwrite)
            {
                tool.RequiresMaintenance = true;
                tool.RequiresPreOperationalCheck = profile.RequiresPreOperationalCheck;
                tool.RequiresCertification = profile.RequiresCertification;
                changed = true;
            }

            var lastMaintenanceDate = DateTime.UtcNow.Date.AddMonths(-3);
            var nextMaintenanceDate = lastMaintenanceDate.AddMonths(6);

            if (overwrite || !tool.LastMaintenanceDate.HasValue)
            {
                tool.LastMaintenanceDate = lastMaintenanceDate;
                changed = true;
            }

            if (overwrite || !tool.NextMaintenanceDate.HasValue)
            {
                tool.NextMaintenanceDate = nextMaintenanceDate;
                changed = true;
            }

            if (overwrite || !tool.MaintenancePeriodMonths.HasValue)
            {
                tool.MaintenancePeriodMonths = 6;
                changed = true;
            }

            tool.UpdatedAt = DateTime.UtcNow;
            tool.UpdatedBy = user;

            var createdToolAccessories = AddAccessories(tool, profile, user);
            var createdToolSafePractices = AddSafePractices(tool, profile, user);
            var createdToolMaintenanceRecords = AddMaintenanceRecords(tool, profile, user);

            createdAccessories += createdToolAccessories;
            createdSafePractices += createdToolSafePractices;
            createdMaintenanceRecords += createdToolMaintenanceRecords;

            if (changed || createdToolAccessories > 0 || createdToolSafePractices > 0 || createdToolMaintenanceRecords > 0)
            {
                updatedTools++;

                _context.Set<ToolLifeCycleEvent>().Add(new ToolLifeCycleEvent
                {
                    ToolAssetId = tool.Id,
                    EventType = "MockTechnicalLifeRecordDataApplied",
                    Title = "Datos simulados de hoja de vida técnica aplicados",
                    Description = "Se completó la hoja de vida técnica con datos simulados para visualización, pruebas y demo.",
                    NewValue = "Datos técnicos, accesorios, prácticas seguras y cronograma de mantenimiento.",
                    RegisteredBy = user,
                    CreatedBy = user
                });
            }

            results.Add(new
            {
                tool.Id,
                tool.InternalCode,
                tool.Name,
                ToolProfile = profile.ProfileName,
                Updated = changed,
                AccessoriesCreated = createdToolAccessories,
                SafePracticesCreated = createdToolSafePractices,
                MaintenanceRecordsCreated = createdToolMaintenanceRecords
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Ok(new
        {
            Message = "Datos simulados aplicados correctamente a las hojas de vida técnicas.",
            TotalTools = tools.Count,
            UpdatedTools = updatedTools,
            CreatedAccessories = createdAccessories,
            CreatedSafePractices = createdSafePractices,
            CreatedMaintenanceRecords = createdMaintenanceRecords,
            Overwrite = overwrite,
            Results = results
        });
    }

    private int AddAccessories(ToolAsset tool, MockProfile profile, string user)
    {
        var existing = tool.Accessories
            .Where(x => !x.IsDeleted)
            .Select(x => x.Name.Trim().ToUpperInvariant())
            .ToHashSet();

        var created = 0;

        foreach (var item in profile.Accessories)
        {
            if (existing.Contains(item.Name.Trim().ToUpperInvariant()))
            {
                continue;
            }

            _context.Set<ToolAccessory>().Add(new ToolAccessory
            {
                ToolAssetId = tool.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                RequiresMaintenance = item.RequiresMaintenance,
                IsMeasurementEquipment = item.IsMeasurementEquipment,
                Observation = item.Observation,
                CreatedBy = user
            });

            created++;
        }

        return created;
    }

    private int AddSafePractices(ToolAsset tool, MockProfile profile, string user)
    {
        var existing = tool.SafePractices
            .Where(x => !x.IsDeleted)
            .Select(x => x.PracticeName.Trim().ToUpperInvariant())
            .ToHashSet();

        var created = 0;

        foreach (var item in profile.SafePractices)
        {
            if (existing.Contains(item.PracticeName.Trim().ToUpperInvariant()))
            {
                continue;
            }

            _context.Set<ToolSafePractice>().Add(new ToolSafePractice
            {
                ToolAssetId = tool.Id,
                PracticeName = item.PracticeName,
                Description = item.Description,
                SortOrder = item.SortOrder,
                CreatedBy = user
            });

            created++;
        }

        return created;
    }

    private int AddMaintenanceRecords(ToolAsset tool, MockProfile profile, string user)
    {
        var normalizedCode = NormalizeCode(tool.InternalCode);

        var existingNumbers = tool.MaintenanceRecords
            .Where(x => !x.IsDeleted)
            .Select(x => x.MaintenanceNumber.Trim().ToUpperInvariant())
            .ToHashSet();

        var created = 0;

        var completedNumber = $"MOCK-COMP-{normalizedCode}";
        var scheduledNumber = $"MOCK-NEXT-{normalizedCode}";

        var lastDate = tool.LastMaintenanceDate ?? DateTime.UtcNow.Date.AddMonths(-3);
        var nextDate = tool.NextMaintenanceDate ?? lastDate.AddMonths(6);

        if (!existingNumbers.Contains(completedNumber.ToUpperInvariant()))
        {
            _context.Set<MaintenanceRecord>().Add(new MaintenanceRecord
            {
                ToolAssetId = tool.Id,
                MaintenanceNumber = completedNumber,
                Type = ToolMaintenanceType.Preventive,
                Status = ToolMaintenanceStatus.Completed,
                ScheduledAt = lastDate,
                StartedAt = lastDate,
                FinishedAt = lastDate,
                Provider = profile.Provider,
                Technician = "Técnico de mantenimiento",
                Description = "Mantenimiento preventivo simulado para hoja de vida técnica.",
                MaintenanceActivities = string.Join(" | ", profile.MaintenanceActivities),
                ExecutionNotes = "Herramienta revisada y operativa. Registro generado como dato simulado.",
                InvoiceNumber = $"FV-MOCK-{normalizedCode}",
                ResponsibleName = "Jorge Cataño",
                ResponsiblePosition = "Auxiliar de almacenamiento",
                IsToolOperational = true,
                Cost = 0,
                Result = "Herramienta operativa",
                CreatedBy = user
            });

            created++;
        }

        if (!existingNumbers.Contains(scheduledNumber.ToUpperInvariant()))
        {
            _context.Set<MaintenanceRecord>().Add(new MaintenanceRecord
            {
                ToolAssetId = tool.Id,
                MaintenanceNumber = scheduledNumber,
                Type = ToolMaintenanceType.Preventive,
                Status = ToolMaintenanceStatus.Scheduled,
                ScheduledAt = nextDate,
                Provider = profile.Provider,
                Technician = "Pendiente por asignar",
                Description = "Próximo mantenimiento preventivo simulado.",
                MaintenanceActivities = string.Join(" | ", profile.MaintenanceActivities),
                ExecutionNotes = "Pendiente de ejecución.",
                ResponsibleName = "Herramientero AGU",
                ResponsiblePosition = "Responsable de herramientas",
                IsToolOperational = null,
                Cost = 0,
                Result = "Pendiente",
                CreatedBy = user
            });

            created++;
        }

        return created;
    }

    private static MockProfile GetProfile(ToolAsset tool)
    {
        var text = $"{tool.InternalCode} {tool.Name} {tool.Description} {tool.ToolType?.Name} {tool.ToolCategory?.Name}".ToUpperInvariant();

        if (text.Contains("COMPUTADOR") || text.Contains("ESCANER") || text.Contains("ESCÁNER") || text.Contains("SCANNER"))
        {
            return BuildComputerScannerProfile();
        }

        if (text.Contains("GATO") || text.Contains("PLUMA") || text.Contains("HIDRAULICO") || text.Contains("HIDRÁULICO"))
        {
            return BuildHydraulicJackProfile();
        }

        return BuildGenericToolProfile();
    }

    private static MockProfile BuildComputerScannerProfile()
    {
        return new MockProfile
        {
            ProfileName = "Computador escáner",
            Brand = "PANASONIC",
            Model = "FZ-55",
            Voltage = "15,6",
            LoadCapacity = "N/A",
            Provider = "N/A",
            RequiresPreOperationalCheck = false,
            RequiresCertification = false,
            Accessories =
            [
                new("CARGADOR", 1, false, false, "Cargador principal del equipo."),
                new("INTERFAZ - USB-LINK2", 1, true, true, "Interfaz de diagnóstico para conexión con vehículos.")
            ],
            SafePractices =
            [
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
                new(11, "Desconexión segura de dispositivos", "Desconectar adecuadamente cualquier dispositivo externo para evitar daños."),
                new(12, "Revisión de software y actualizaciones", "Mantener actualizado el software del escáner y el sistema operativo.")
            ],
            MaintenanceActivities =
            [
                "Limpieza general",
                "Verificación de licencia",
                "Actualizaciones",
                "Limpieza de archivos basura"
            ]
        };
    }

    private static MockProfile BuildHydraulicJackProfile()
    {
        return new MockProfile
        {
            ProfileName = "Gato pluma / hidráulico",
            Brand = "MIKELS",
            Model = "T-475",
            Voltage = "N/A",
            LoadCapacity = "2 TON",
            Provider = "Navitrans",
            RequiresPreOperationalCheck = true,
            RequiresCertification = false,
            Accessories =
            [
                new("CILINDRO HIDRÁULICO", 1, true, false, "Elemento principal de elevación."),
                new("GANCHO", 1, true, false, "Gancho de sujeción de carga."),
                new("RUEDAS", 4, true, false, "Ruedas de desplazamiento."),
                new("CUERPO", 1, true, false, "Estructura principal del equipo.")
            ],
            SafePractices =
            [
                new(1, "Inspección previa al uso", "Verifica el estado de herramientas, mangueras y conexiones para detectar grietas, fugas o desgaste antes de usarlas."),
                new(2, "Uso de EPP", "Usa gafas de seguridad, guantes resistentes al aceite, protección auditiva y calzado con punta reforzada y suela antideslizante."),
                new(3, "Procedimientos de operación", "Asegúrate de que las conexiones estén firmes, no excedas la presión recomendada y mantén las mangueras alejadas de bordes afilados o superficies calientes."),
                new(4, "Posición segura", "Mantén una postura estable, limpia tu área de trabajo y evita colocarte frente a herramientas presurizadas."),
                new(5, "Mantenimiento regular", "Realiza inspecciones periódicas, reemplaza componentes desgastados y utiliza repuestos recomendados por el fabricante."),
                new(6, "Prevención de fugas", "Nunca uses las manos para buscar fugas; utiliza cartón o papel y repáralas solo cuando el sistema esté despresurizado."),
                new(7, "Capacitación y señalización", "Capacita al personal en el uso seguro de herramientas y coloca señalización de advertencia en las áreas de trabajo."),
                new(8, "Despresurización al finalizar", "Antes de desconectar herramientas, libera la presión del sistema. Nunca realices ajustes mientras el equipo esté presurizado."),
                new(9, "Almacenamiento adecuado", "Guarda herramientas y mangueras en un lugar limpio y seco, evitando doblarlas de forma incorrecta.")
            ],
            MaintenanceActivities =
            [
                "Revisión de fluidos",
                "Inspección de fugas",
                "Inspección",
                "Limpieza",
                "Lubricación"
            ]
        };
    }

    private static MockProfile BuildGenericToolProfile()
    {
        return new MockProfile
        {
            ProfileName = "Herramienta general",
            Brand = "NAVITRANS",
            Model = "GEN-001",
            Voltage = "N/A",
            LoadCapacity = "N/A",
            Provider = "Navitrans",
            RequiresPreOperationalCheck = true,
            RequiresCertification = false,
            Accessories =
            [
                new("ACCESORIO PRINCIPAL", 1, true, false, "Accesorio principal de la herramienta."),
                new("ESTUCHE O SOPORTE", 1, false, false, "Elemento de almacenamiento o soporte.")
            ],
            SafePractices =
            [
                new(1, "Inspección previa al uso", "Verificar el estado general de la herramienta antes de utilizarla."),
                new(2, "Uso de elementos de protección personal", "Utilizar los elementos de protección personal requeridos para la operación."),
                new(3, "Uso correcto de la herramienta", "Operar la herramienta únicamente para las actividades para las que fue diseñada."),
                new(4, "Reporte de novedades", "Reportar daños, fallas o condiciones inseguras antes y después del uso."),
                new(5, "Limpieza y almacenamiento", "Limpiar la herramienta después del uso y almacenarla en el lugar asignado.")
            ],
            MaintenanceActivities =
            [
                "Limpieza general",
                "Inspección visual",
                "Verificación funcional",
                "Registro de observaciones"
            ]
        };
    }

    private static string NormalizeCode(string value)
    {
        var clean = new string(value
            .Trim()
            .ToUpperInvariant()
            .Where(c => char.IsLetterOrDigit(c) || c == '-')
            .ToArray());

        return string.IsNullOrWhiteSpace(clean) ? Guid.NewGuid().ToString("N")[..8].ToUpperInvariant() : clean;
    }

    private sealed class MockProfile
    {
        public string ProfileName { get; set; } = "Herramienta general";

        public string Brand { get; set; } = "NAVITRANS";

        public string Model { get; set; } = "GEN-001";

        public string? Voltage { get; set; }

        public string? LoadCapacity { get; set; }

        public string Provider { get; set; } = "Navitrans";

        public bool RequiresPreOperationalCheck { get; set; }

        public bool RequiresCertification { get; set; }

        public List<AccessorySeed> Accessories { get; set; } = new();

        public List<SafePracticeSeed> SafePractices { get; set; } = new();

        public List<string> MaintenanceActivities { get; set; } = new();
    }

    private sealed record AccessorySeed(
        string Name,
        int Quantity,
        bool RequiresMaintenance,
        bool IsMeasurementEquipment,
        string? Observation);

    private sealed record SafePracticeSeed(
        int SortOrder,
        string PracticeName,
        string Description);
}

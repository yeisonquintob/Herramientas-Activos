using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Navi.ToolsAssets.Domain.Entities.Inventory;
using Navi.ToolsAssets.Domain.Entities.LifeCycles;
using Navi.ToolsAssets.Domain.Entities.Organization;
using Navi.ToolsAssets.Domain.Entities.Safety;
using Navi.ToolsAssets.Domain.Entities.Security;
using Navi.ToolsAssets.Domain.Enums;
using Navi.ToolsAssets.Infrastructure.Persistence.Context;

namespace Navi.ToolsAssets.Api.Controllers;

[ApiController]
[Route("api/dev/demo")]
public sealed class DevDemoResetController : ControllerBase
{
    private readonly NaviToolsAssetsDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public DevDemoResetController(
        NaviToolsAssetsDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    [HttpPost("reset-and-seed")]
    public async Task<IActionResult> ResetAndSeed([FromBody] ResetDemoRequest request, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
        {
            return BadRequest(new { Message = "Este endpoint solo puede ejecutarse en Development." });
        }

        if (!string.Equals(request.Confirmation, "CONFIRMAR_RESETEO_NAVI", StringComparison.Ordinal))
        {
            return BadRequest(new { Message = "Confirmación inválida. Debes enviar CONFIRMAR_RESETEO_NAVI." });
        }

        try
        {
            var seedBy = string.IsNullOrWhiteSpace(request.SeedBy)
                ? "admin-demo-reset"
                : request.SeedBy.Trim();

            await ResetOperationalDataAsync(cancellationToken);

            var zone = await EnsureZoneAsync(seedBy, cancellationToken);
            var branches = await EnsureBranchesAsync(zone.Id, seedBy, cancellationToken);

            var toolType = await EnsureToolTypeAsync("HERRAMIENTA", "Herramienta", "Herramientas de taller.", seedBy, cancellationToken);
            var fixedAssetType = await EnsureToolTypeAsync("ACTIVO-FIJO", "Activo fijo", "Activos fijos tecnológicos y operativos.", seedBy, cancellationToken);

            var catManual = await EnsureCategoryAsync("HERR-MANUAL", "Herramienta manual", "Herramientas manuales.", seedBy, cancellationToken);
            var catElectrica = await EnsureCategoryAsync("HERR-ELECTRICA", "Herramienta eléctrica", "Herramientas eléctricas.", seedBy, cancellationToken);
            var catDiagnostico = await EnsureCategoryAsync("EQUIPO-DIAGNOSTICO", "Equipo de diagnóstico", "Equipos especializados de diagnóstico.", seedBy, cancellationToken);
            var catHidraulica = await EnsureCategoryAsync("EQUIPO-HIDRAULICO", "Equipo hidráulico", "Equipos hidráulicos especializados.", seedBy, cancellationToken);
            var catTecnologia = await EnsureCategoryAsync("EQUIPO-TECNOLOGICO", "Equipo tecnológico", "Celulares y computadores.", seedBy, cancellationToken);

            var roles = await SeedRolesAsync(seedBy, cancellationToken);

            var createdUsers = new List<AppUser>();
            var createdResponsibles = new List<ResponsiblePerson>();
            var createdAssets = new List<ToolAsset>();

            var adminResponsible = CreateResponsible("ADM-NAVI", "Administrador NAVI", "admin.navi@navitrans.demo", "Administrador", "TI Soluciones", seedBy);
            createdResponsibles.Add(adminResponsible);
            await _context.SaveChangesAsync(cancellationToken);

            createdUsers.Add(CreateUser("admin", "Administrador NAVI", "admin.navi@navitrans.demo", "Administrador", "TI Soluciones", roles["ADMIN"], null, adminResponsible.Id, seedBy));
            await _context.SaveChangesAsync(cancellationToken);

            foreach (var branch in branches)
            {
                var location = await EnsureLocationAsync(branch.Id, $"{branch.Code}-BOD-HERR", "Bodega de herramientas", seedBy, cancellationToken);

                var herramientero = CreateResponsible($"HER-{branch.Code}", $"Herramientero {branch.Code}", $"herramientero.{branch.Code.ToLowerInvariant()}@navitrans.demo", "Herramientero", "Herramientas", seedBy);
                var tecnico1 = CreateResponsible($"TEC1-{branch.Code}", $"Técnico 1 {branch.Code}", $"tecnico1.{branch.Code.ToLowerInvariant()}@navitrans.demo", "Técnico de taller", "Servicio técnico", seedBy);
                var tecnico2 = CreateResponsible($"TEC2-{branch.Code}", $"Técnico 2 {branch.Code}", $"tecnico2.{branch.Code.ToLowerInvariant()}@navitrans.demo", "Técnico de taller", "Servicio técnico", seedBy);
                var ingeniero = CreateResponsible($"ING-{branch.Code}", $"Ingeniero de servicios {branch.Code}", $"ingeniero.{branch.Code.ToLowerInvariant()}@navitrans.demo", "Ingeniero de servicios", "Servicios", seedBy);
                var coordinador = CreateResponsible($"COORD-{branch.Code}", $"Coordinador taller {branch.Code}", $"coordinador.{branch.Code.ToLowerInvariant()}@navitrans.demo", "Coordinador de taller", "Taller", seedBy);

                createdResponsibles.AddRange(new[] { herramientero, tecnico1, tecnico2, ingeniero, coordinador });
                await _context.SaveChangesAsync(cancellationToken);

                createdUsers.Add(CreateUser($"herramientero.{branch.Code.ToLowerInvariant()}", $"Herramientero {branch.Code}", herramientero.Email, herramientero.Position, herramientero.Area, roles["HERRAMIENTERO"], branch.Id, herramientero.Id, seedBy));
                createdUsers.Add(CreateUser($"tecnico1.{branch.Code.ToLowerInvariant()}", $"Técnico 1 {branch.Code}", tecnico1.Email, tecnico1.Position, tecnico1.Area, roles["TECNICO"], branch.Id, tecnico1.Id, seedBy));
                createdUsers.Add(CreateUser($"tecnico2.{branch.Code.ToLowerInvariant()}", $"Técnico 2 {branch.Code}", tecnico2.Email, tecnico2.Position, tecnico2.Area, roles["TECNICO"], branch.Id, tecnico2.Id, seedBy));
                createdUsers.Add(CreateUser($"ingeniero.{branch.Code.ToLowerInvariant()}", $"Ingeniero de servicios {branch.Code}", ingeniero.Email, ingeniero.Position, ingeniero.Area, roles["ING_SERVICIOS"], branch.Id, ingeniero.Id, seedBy));
                createdUsers.Add(CreateUser($"coordinador.{branch.Code.ToLowerInvariant()}", $"Coordinador taller {branch.Code}", coordinador.Email, coordinador.Position, coordinador.Area, roles["COORD_TALLER"], branch.Id, coordinador.Id, seedBy));
                await _context.SaveChangesAsync(cancellationToken);

                var pulidora = CreateAsset(branch, location, tecnico1, toolType, catElectrica,
                    $"{branch.Code}-PULIDORA-001", "Pulidora angular 4 1/2 pulgadas",
                    "Herramienta de taller para corte y desbaste.", "DEWALT", "DWE4010",
                    $"SER-{branch.Code}-PUL-001", false, "110V", "850W", true, true, false, seedBy);

                var taladro = CreateAsset(branch, location, tecnico1, toolType, catElectrica,
                    $"{branch.Code}-TALADRO-002", "Taladro percutor inalámbrico",
                    "Herramienta de taller para perforación y ensamble.", "BOSCH", "GSB 180-LI",
                    $"SER-{branch.Code}-TAL-002", false, "18V", "1.5Ah", true, true, false, seedBy);

                var multimetro = CreateAsset(branch, location, tecnico2, toolType, catDiagnostico,
                    $"{branch.Code}-MULTIMETRO-003", "Multímetro digital industrial",
                    "Equipo especializado para medición eléctrica.", "FLUKE", "179",
                    $"SER-{branch.Code}-MUL-003", true, "CAT III 1000V", "Medición eléctrica", true, true, true, seedBy);

                var gato = CreateAsset(branch, location, tecnico2, toolType, catHidraulica,
                    $"{branch.Code}-GATO-20T-004", "Gato hidráulico botella 20T",
                    "Herramienta especializada para levantamiento de carga pesada.", "POWERTEAM", "20T",
                    $"SER-{branch.Code}-GAT-004", true, "N/A", "20 toneladas", true, true, true, seedBy);

                var scanner = CreateAsset(branch, location, herramientero, toolType, catDiagnostico,
                    $"{branch.Code}-ESCANER-005", "Computador escáner diagnóstico",
                    "Equipo especializado para diagnóstico electrónico.", "LAUNCH", "X431 PRO",
                    $"SER-{branch.Code}-ESC-005", true, "12V / USB-C", "OBD diagnóstico", true, true, true, seedBy);

                createdAssets.AddRange(new[] { pulidora, taladro, multimetro, gato, scanner });

                AddToolAccessoriesAndSafety(pulidora, seedBy);
                AddToolAccessoriesAndSafety(taladro, seedBy);
                AddToolAccessoriesAndSafety(multimetro, seedBy);
                AddToolAccessoriesAndSafety(gato, seedBy);
                AddToolAccessoriesAndSafety(scanner, seedBy);

                foreach (var responsible in new[] { herramientero, tecnico1, tecnico2, ingeniero, coordinador })
                {
                    var normalizedResponsible = Normalize(responsible.EmployeeCode ?? responsible.FullName);

                    var phone = CreateAsset(branch, location, responsible, fixedAssetType, catTecnologia,
                        $"{branch.Code}-CEL-{normalizedResponsible}",
                        $"Celular corporativo {responsible.FullName}",
                        "Celular corporativo para evidencias, operación móvil y toma física.",
                        "SAMSUNG", "Galaxy A55 5G",
                        $"IMEI-{branch.Code}-{normalizedResponsible}",
                        false, "USB-C / 25W", "128GB / 8GB RAM", false, false, false, seedBy);

                    AddAccessory(phone, "Cargador USB-C 25W", 1, false, false, "Cargador principal del celular.", seedBy);
                    AddAccessory(phone, "Cable USB-C", 1, false, false, "Cable de carga y transferencia.", seedBy);
                    AddAccessory(phone, "Forro protector", 1, false, false, "Protección del celular.", seedBy);

                    createdAssets.Add(phone);
                }

                var laptopTec1 = CreateAsset(branch, location, tecnico1, fixedAssetType, catTecnologia,
                    $"{branch.Code}-PORTATIL-TEC1",
                    $"Computador portátil Técnico 1 {branch.Code}",
                    "Computador portátil asignado para diagnóstico, reportes y operación.",
                    "DELL", "Latitude 5450",
                    $"SER-{branch.Code}-LAT-TEC1",
                    false, "USB-C 65W", "Intel i5 / 16GB / 512GB SSD", false, false, false, seedBy);

                AddAccessory(laptopTec1, "Cargador USB-C 65W", 1, false, false, "Cargador original del computador.", seedBy);
                AddAccessory(laptopTec1, "Maletín portátil", 1, false, false, "Elemento de transporte.", seedBy);

                createdAssets.Add(laptopTec1);

                var laptopIng = CreateAsset(branch, location, ingeniero, fixedAssetType, catTecnologia,
                    $"{branch.Code}-PORTATIL-ING",
                    $"Computador portátil Ingeniero {branch.Code}",
                    "Computador portátil asignado a ingeniero de servicios.",
                    "HP", "ProBook 440",
                    $"SER-{branch.Code}-HP-ING",
                    false, "USB-C 65W", "Intel i7 / 16GB / 512GB SSD", false, false, false, seedBy);

                AddAccessory(laptopIng, "Cargador USB-C 65W", 1, false, false, "Cargador original del computador.", seedBy);
                createdAssets.Add(laptopIng);
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                Message = "Base limpiada y datos demo creados correctamente.",
                Branches = branches.Select(x => new { x.Id, x.Code, x.Name }).ToList(),
                RolesCreated = roles.Count,
                UsersCreated = createdUsers.Count,
                ResponsiblesCreated = createdResponsibles.Count,
                AssetsCreated = createdAssets.Count,
                ToolsCreated = createdAssets.Count(x => x.ToolType?.Code == "HERRAMIENTA" || x.ToolTypeId != Guid.Empty),
                CreatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Message = "ResetAndSeedException",
                Error = ex.Message,
                InnerError = ex.InnerException?.Message,
                StackTrace = ex.StackTrace
            });
        }
    }

    private async Task ResetOperationalDataAsync(CancellationToken cancellationToken)
    {
        var sql = """
IF OBJECT_ID('[PhysicalCounts].[PhysicalCountReportedItems]', 'U') IS NOT NULL DELETE FROM [PhysicalCounts].[PhysicalCountReportedItems];
IF OBJECT_ID('[PhysicalCounts].[PhysicalCountExtraItems]', 'U') IS NOT NULL DELETE FROM [PhysicalCounts].[PhysicalCountExtraItems];
IF OBJECT_ID('[PhysicalCounts].[PhysicalCountItems]', 'U') IS NOT NULL DELETE FROM [PhysicalCounts].[PhysicalCountItems];
IF OBJECT_ID('[PhysicalCounts].[PhysicalCountParticipants]', 'U') IS NOT NULL DELETE FROM [PhysicalCounts].[PhysicalCountParticipants];
IF OBJECT_ID('[PhysicalCounts].[PhysicalCounts]', 'U') IS NOT NULL DELETE FROM [PhysicalCounts].[PhysicalCounts];

IF OBJECT_ID('[PhysicalCount].[PhysicalCountReportedItems]', 'U') IS NOT NULL DELETE FROM [PhysicalCount].[PhysicalCountReportedItems];
IF OBJECT_ID('[PhysicalCount].[PhysicalCountExtraItems]', 'U') IS NOT NULL DELETE FROM [PhysicalCount].[PhysicalCountExtraItems];
IF OBJECT_ID('[PhysicalCount].[PhysicalCountItems]', 'U') IS NOT NULL DELETE FROM [PhysicalCount].[PhysicalCountItems];
IF OBJECT_ID('[PhysicalCount].[PhysicalCountParticipants]', 'U') IS NOT NULL DELETE FROM [PhysicalCount].[PhysicalCountParticipants];
IF OBJECT_ID('[PhysicalCount].[PhysicalCounts]', 'U') IS NOT NULL DELETE FROM [PhysicalCount].[PhysicalCounts];

IF OBJECT_ID('[Loans].[ToolLoanItems]', 'U') IS NOT NULL DELETE FROM [Loans].[ToolLoanItems];
IF OBJECT_ID('[Loans].[ToolLoans]', 'U') IS NOT NULL DELETE FROM [Loans].[ToolLoans];

IF OBJECT_ID('[Damages].[DamageReports]', 'U') IS NOT NULL DELETE FROM [Damages].[DamageReports];

IF OBJECT_ID('[Maintenance].[MaintenanceRecords]', 'U') IS NOT NULL DELETE FROM [Maintenance].[MaintenanceRecords];
IF OBJECT_ID('[Maintenance].[MaintenanceRequests]', 'U') IS NOT NULL DELETE FROM [Maintenance].[MaintenanceRequests];

IF OBJECT_ID('[Purchases].[PurchaseRequests]', 'U') IS NOT NULL DELETE FROM [Purchases].[PurchaseRequests];

IF OBJECT_ID('[Sync].[FenixReconciliationRecords]', 'U') IS NOT NULL DELETE FROM [Sync].[FenixReconciliationRecords];

IF OBJECT_ID('[Documents].[ToolDocuments]', 'U') IS NOT NULL DELETE FROM [Documents].[ToolDocuments];

IF OBJECT_ID('[Safety].[ToolSafePractices]', 'U') IS NOT NULL DELETE FROM [Safety].[ToolSafePractices];

IF OBJECT_ID('[Inventory].[ToolAccessories]', 'U') IS NOT NULL DELETE FROM [Inventory].[ToolAccessories];

IF OBJECT_ID('[LifeCycle].[ToolLifeCycleEvents]', 'U') IS NOT NULL DELETE FROM [LifeCycle].[ToolLifeCycleEvents];
IF OBJECT_ID('[LifeCycles].[ToolLifeCycleEvents]', 'U') IS NOT NULL DELETE FROM [LifeCycles].[ToolLifeCycleEvents];

IF OBJECT_ID('[Inventory].[ToolAssets]', 'U') IS NOT NULL DELETE FROM [Inventory].[ToolAssets];

IF OBJECT_ID('[Security].[AppUsers]', 'U') IS NOT NULL DELETE FROM [Security].[AppUsers];
IF OBJECT_ID('[dbo].[AppUsers]', 'U') IS NOT NULL DELETE FROM [dbo].[AppUsers];
IF OBJECT_ID('[AppUsers]', 'U') IS NOT NULL DELETE FROM [AppUsers];

IF OBJECT_ID('[Security].[AppRoles]', 'U') IS NOT NULL DELETE FROM [Security].[AppRoles];
IF OBJECT_ID('[dbo].[AppRoles]', 'U') IS NOT NULL DELETE FROM [dbo].[AppRoles];
IF OBJECT_ID('[AppRoles]', 'U') IS NOT NULL DELETE FROM [AppRoles];

IF OBJECT_ID('[Organization].[ResponsiblePeople]', 'U') IS NOT NULL DELETE FROM [Organization].[ResponsiblePeople];
""";

        await _context.Database.ExecuteSqlRawAsync(sql, cancellationToken);
    }

    private async Task<Zone> EnsureZoneAsync(string seedBy, CancellationToken cancellationToken)
    {
        var zone = await _context.Zones.FirstOrDefaultAsync(x => x.Code == "ANT", cancellationToken);

        if (zone is null)
        {
            zone = new Zone
            {
                Code = "ANT",
                Name = "Zona Antioquia",
                Description = "Zona principal para pruebas.",
                IsActive = true,
                CreatedBy = seedBy
            };

            _context.Zones.Add(zone);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return zone;
    }

    private async Task<List<Branch>> EnsureBranchesAsync(Guid zoneId, string seedBy, CancellationToken cancellationToken)
    {
        var branches = await _context.Branches
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        if (branches.Count >= 2)
        {
            return branches;
        }

        if (!branches.Any(x => x.Code == "AGU"))
        {
            _context.Branches.Add(new Branch
            {
                Code = "AGU",
                Name = "Aguacatala",
                City = "Medellín",
                Address = "Sede Aguacatala",
                ZoneId = zoneId,
                IsPilot = true,
                IsActive = true,
                CreatedBy = seedBy
            });
        }

        if (!branches.Any(x => x.Code == "BOG"))
        {
            _context.Branches.Add(new Branch
            {
                Code = "BOG",
                Name = "Bogotá",
                City = "Bogotá",
                Address = "Sede Bogotá",
                ZoneId = zoneId,
                IsPilot = false,
                IsActive = true,
                CreatedBy = seedBy
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await _context.Branches
            .Where(x => !x.IsDeleted && x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    private async Task<ToolLocation> EnsureLocationAsync(Guid branchId, string code, string name, string seedBy, CancellationToken cancellationToken)
    {
        var location = await _context.ToolLocations
            .FirstOrDefaultAsync(x => x.BranchId == branchId && x.Code == code, cancellationToken);

        if (location is null)
        {
            location = new ToolLocation
            {
                BranchId = branchId,
                Code = code,
                Name = name,
                Description = "Ubicación demo para herramientas y activos.",
                IsActive = true,
                CreatedBy = seedBy
            };

            _context.ToolLocations.Add(location);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return location;
    }

    private async Task<ToolType> EnsureToolTypeAsync(string code, string name, string description, string seedBy, CancellationToken cancellationToken)
    {
        var item = await _context.ToolTypes.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        if (item is null)
        {
            item = new ToolType
            {
                Code = code,
                Name = name,
                Description = description,
                IsActive = true,
                CreatedBy = seedBy
            };

            _context.ToolTypes.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return item;
    }

    private async Task<ToolCategory> EnsureCategoryAsync(string code, string name, string description, string seedBy, CancellationToken cancellationToken)
    {
        var item = await _context.ToolCategories.FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        if (item is null)
        {
            item = new ToolCategory
            {
                Code = code,
                Name = name,
                Description = description,
                IsActive = true,
                CreatedBy = seedBy
            };

            _context.ToolCategories.Add(item);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return item;
    }

    private async Task<Dictionary<string, AppRole>> SeedRolesAsync(string seedBy, CancellationToken cancellationToken)
    {
        var definitions = new[]
        {
            new RoleSeed("ADMIN", "Administrador NAVI", "Acceso total.", "ALL"),
            new RoleSeed("GERENCIAL", "Gerencial", "Consulta ejecutiva y aprobación.", Join("Dashboard.View", "Tools.View", "TechnicalLifeRecord.View", "Purchases.View", "Purchases.Approve", "Maintenance.View", "PhysicalCounts.View", "Reports.View")),
            new RoleSeed("HERRAMIENTERO", "Herramientero", "Gestión de herramientas, asignaciones, evidencias y tomas físicas.", Join("Dashboard.View", "Tools.View", "Tools.Create", "Tools.Edit", "AssetAvailability.View", "AssetAvailability.Edit", "AssetAssignment.View", "AssetAssignment.Assign", "AssetAssignment.Return", "AssetAssignment.History", "TechnicalLifeRecord.View", "TechnicalLifeRecord.Edit", "Documents.View", "Documents.Upload", "Documents.Download", "Maintenance.View", "Maintenance.Request", "PhysicalCounts.View", "PhysicalCounts.Create", "PhysicalCounts.Close", "Reports.View", "Mobile.Access", "Mobile.Tools.View", "Mobile.Tools.Review")),
            new RoleSeed("ING_SERVICIOS", "Ingeniero de servicios", "Aprobación y seguimiento técnico.", Join("Dashboard.View", "Tools.View", "Tools.Edit", "AssetAvailability.View", "AssetAssignment.View", "TechnicalLifeRecord.View", "Documents.View", "Documents.Upload", "Maintenance.View", "Maintenance.Request", "Maintenance.Execute", "Maintenance.Close", "Purchases.View", "Purchases.Approve", "PhysicalCounts.View", "Reports.View", "Mobile.Access", "Mobile.Tools.View")),
            new RoleSeed("COORD_TALLER", "Coordinador de taller", "Coordinación de taller, compras y mantenimiento.", Join("Dashboard.View", "Tools.View", "Tools.Create", "Tools.Edit", "AssetAvailability.View", "AssetAvailability.Edit", "AssetAssignment.View", "AssetAssignment.Assign", "AssetAssignment.Return", "TechnicalLifeRecord.View", "TechnicalLifeRecord.Edit", "Documents.View", "Documents.Upload", "Maintenance.View", "Maintenance.Request", "Maintenance.Execute", "Maintenance.Close", "Purchases.View", "Purchases.Request", "Purchases.Approve", "PhysicalCounts.View", "PhysicalCounts.Create", "PhysicalCounts.Close", "Reports.View", "Mobile.Access", "Mobile.Tools.View")),
            new RoleSeed("TECNICO", "Técnico", "Validación móvil, daños, evidencias y préstamos.", Join("Tools.View", "TechnicalLifeRecord.View", "Documents.View", "Documents.Upload", "Maintenance.View", "Maintenance.Request", "PhysicalCounts.View", "Mobile.Access", "Mobile.Tools.View", "Mobile.Tools.Review", "Mobile.PreOperational.Report", "Mobile.Damage.Report", "Mobile.Loans.Request"))
        };

        var result = new Dictionary<string, AppRole>(StringComparer.OrdinalIgnoreCase);

        foreach (var definition in definitions)
        {
            var role = new AppRole
            {
                Code = definition.Code,
                Name = definition.Name,
                Description = definition.Description,
                Permissions = definition.Permissions,
                IsActive = true,
                CreatedBy = seedBy
            };

            _context.AppRoles.Add(role);
            result[role.Code] = role;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return result;
    }

    private ResponsiblePerson CreateResponsible(string code, string name, string email, string position, string area, string seedBy)
    {
        var responsible = new ResponsiblePerson
        {
            EmployeeCode = code,
            FullName = name,
            Email = email,
            Position = position,
            Area = area,
            IsActive = true,
            CreatedBy = seedBy
        };

        _context.ResponsiblePeople.Add(responsible);

        return responsible;
    }

    private AppUser CreateUser(
        string userName,
        string displayName,
        string? email,
        string? position,
        string? area,
        AppRole role,
        Guid? branchId,
        Guid? responsiblePersonId,
        string seedBy)
    {
        var user = new AppUser
        {
            UserName = userName,
            DisplayName = displayName,
            Email = email,
            Position = position,
            Area = area,
            AppRoleId = role.Id,
            BranchId = branchId,
            ResponsiblePersonId = responsiblePersonId,
            IsActive = true,
            CreatedBy = seedBy
        };

        _context.AppUsers.Add(user);

        return user;
    }

    private ToolAsset CreateAsset(
        Branch branch,
        ToolLocation location,
        ResponsiblePerson responsible,
        ToolType type,
        ToolCategory category,
        string internalCode,
        string name,
        string description,
        string brand,
        string model,
        string serial,
        bool isSpecialized,
        string voltage,
        string loadCapacity,
        bool requiresMaintenance,
        bool requiresPreOperational,
        bool requiresCertification,
        string seedBy)
    {
        var acquisitionDate = DateTime.UtcNow.Date.AddMonths(-8);

        var asset = new ToolAsset
        {
            InternalCode = internalCode,
            Name = name,
            Description = description,
            Brand = brand,
            Model = model,
            SerialNumber = serial,
            FixedAssetCode = $"AF-{internalCode}",
            FenixCode = $"FENIX-{internalCode}",
            UnitOfMeasure = "UND",
            Quantity = 1,
            ZoneId = branch.ZoneId,
            BranchId = branch.Id,
            LocationId = location.Id,
            ResponsiblePersonId = responsible.Id,
            ToolTypeId = type.Id,
            ToolCategoryId = category.Id,
            ToolType = type,
            ToolCategory = category,
            OperationalStatus = ToolOperationalStatus.Assigned,
            PhysicalStatus = ToolPhysicalStatus.Good,
            CustodyStatus = ToolCustodyStatus.AssignedToResponsible,
            ReconciliationStatus = ToolReconciliationStatus.Validated,
            SyncStatus = ToolSyncStatus.Synced,
            IsSpecialized = isSpecialized,
            RequiresMaintenance = requiresMaintenance,
            RequiresPreOperationalCheck = requiresPreOperational,
            RequiresCertification = requiresCertification,
            CertificationExpirationDate = requiresCertification ? DateTime.UtcNow.Date.AddYears(1) : null,
            AcquisitionDate = acquisitionDate,
            UsefulLifeStartDate = acquisitionDate,
            UsefulLifeMonths = isSpecialized ? 72 : 60,
            UsefulLifeDays = (isSpecialized ? 72 : 60) * 30,
            LastMaintenanceDate = requiresMaintenance ? DateTime.UtcNow.Date.AddMonths(-2) : null,
            NextMaintenanceDate = requiresMaintenance ? DateTime.UtcNow.Date.AddMonths(4) : null,
            MaintenancePeriodMonths = requiresMaintenance ? 6 : null,
            Voltage = voltage,
            LoadCapacity = loadCapacity,
            Provider = "Proveedor demo Navitrans",
            HasWarranty = !isSpecialized,
            WarrantyType = isSpecialized ? "Certificación técnica" : "Garantía proveedor",
            CreatedBy = seedBy
        };

        _context.ToolAssets.Add(asset);

        _context.ToolLifeCycleEvents.Add(new ToolLifeCycleEvent
        {
            ToolAsset = asset,
            EventType = "DemoSeedCreated",
            Title = "Activo creado para pruebas",
            Description = "Registro creado por reseteo demo para pruebas de toma física.",
            NewValue = $"{internalCode} | {name}",
            CreatedBy = seedBy
        });

        return asset;
    }

    private void AddToolAccessoriesAndSafety(ToolAsset asset, string seedBy)
    {
        AddAccessory(asset, "Accesorio principal", 1, asset.RequiresMaintenance, false, "Accesorio principal asociado.", seedBy);
        AddAccessory(asset, "Estuche o soporte", 1, false, false, "Elemento de almacenamiento o soporte.", seedBy);

        AddSafePractice(asset, 1, "Inspección previa al uso", "Verificar estado general antes de utilizar.", seedBy);
        AddSafePractice(asset, 2, "Uso de EPP", "Utilizar elementos de protección personal requeridos.", seedBy);
        AddSafePractice(asset, 3, "Reporte de novedades", "Reportar daños, faltantes o condiciones inseguras.", seedBy);
    }

    private void AddAccessory(ToolAsset asset, int quantity, string seedBy)
    {
        AddAccessory(asset, "Accesorio principal", quantity, false, false, "Accesorio asociado al activo.", seedBy);
    }

    private void AddAccessory(ToolAsset asset, string name, int quantity, bool requiresMaintenance, bool isMeasurementEquipment, string observation, string seedBy)
    {
        _context.ToolAccessories.Add(new ToolAccessory
        {
            ToolAsset = asset,
            Name = name,
            Quantity = quantity <= 0 ? 1 : quantity,
            RequiresMaintenance = requiresMaintenance,
            IsMeasurementEquipment = isMeasurementEquipment,
            Observation = observation,
            CreatedBy = seedBy
        });
    }

    private void AddSafePractice(ToolAsset asset, int sortOrder, string name, string description, string seedBy)
    {
        _context.ToolSafePractices.Add(new ToolSafePractice
        {
            ToolAsset = asset,
            SortOrder = sortOrder,
            PracticeName = name,
            Description = description,
            CreatedBy = seedBy
        });
    }

    private static string Join(params string[] permissions)
    {
        return string.Join(";", permissions.Distinct(StringComparer.OrdinalIgnoreCase));
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "SIN-CODIGO";
        }

        var chars = value
            .Trim()
            .ToUpperInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        return new string(chars).Replace("--", "-").Trim('-');
    }

    private sealed record RoleSeed(string Code, string Name, string Description, string Permissions);
}

public sealed class ResetDemoRequest
{
    public string Confirmation { get; set; } = string.Empty;
    public string? SeedBy { get; set; }
}


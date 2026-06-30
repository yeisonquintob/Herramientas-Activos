using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Navi.ToolsAssets.Admin.Services.Auth;

public class DocumentsIndexBase : ComponentBase
{
    protected const long MaxFileSize = 50 * 1024 * 1024;
    protected const string PhysicalCountCategory = "Toma física";

    [Inject] protected IHttpClientFactory HttpClientFactory { get; set; } = default!;
    [Inject] protected WebAuthSessionService AuthSession { get; set; } = default!;

    protected bool CanDownloadDocuments => DocsHasPermission("Documents.Download");
    protected bool CanUploadDocuments => DocsHasPermission("Documents.Upload");
    protected bool CanDeleteDocuments => DocsHasPermission("Documents.Delete");

    protected bool DocsHasPermission(string permission)
    {
        return AuthSession.HasPermission(permission)
            || string.Equals(AuthSession.RoleCode, "ADMIN", StringComparison.OrdinalIgnoreCase)
            || string.Equals(AuthSession.RoleCode, "SUPERADMIN", StringComparison.OrdinalIgnoreCase);
    }



    protected bool isLoading;
    protected bool isSaving;
    protected string apiBaseUrl = "http://localhost:5218";
    protected string selectedCategory = "Ficha técnica";
    protected string selectedToolFilter = string.Empty;
    protected string selectedPhysicalCountId = string.Empty;
    protected string assetSearchText = string.Empty;
    protected string documentSearchText = string.Empty;
    protected string physicalSearchText = string.Empty;
    protected string documentName = string.Empty;
    protected string uploadedBy = string.Empty;
    protected string description = string.Empty;
    protected string? message;
    protected string messageClass = "alert alert-info";
    protected IBrowserFile? selectedFile;

    protected bool showManualModal;
    protected bool showDocumentModal;
    protected bool showPhysicalModal;
    protected string selectedManualCode = "Documents";
    protected DocumentViewItem? selectedDocument;

    protected List<ToolAssetOption> assets = new();
    protected List<DocumentViewItem> documents = new();
    protected List<PhysicalCountListItem> physicalCounts = new();
    protected PhysicalCountBoard selectedPhysicalBoard = new();
    protected ReportedItemsBoard selectedReportedBoard = new();

    protected bool CanSaveDocument =>
        selectedCategory != PhysicalCountCategory && SelectedTool is not null && selectedFile is not null && !isSaving;

    protected ToolAssetOption? SelectedTool => ResolveSelectedTool();

    protected PhysicalCountListItem? SelectedPhysicalCount =>
        Guid.TryParse(selectedPhysicalCountId, out var id)
            ? physicalCounts.FirstOrDefault(x => x.Id == id)
            : null;

    protected PhysicalCountSummary SelectedBoardSummary => selectedPhysicalBoard.Summary ?? new PhysicalCountSummary();
    protected ReportedItemsSummary SelectedReportedSummary => selectedReportedBoard.Summary ?? new ReportedItemsSummary();

    protected int PendingPhysicalDecisions =>
        SelectedReportedSummary.PendingReview +
        SelectedReportedSummary.RequiresUserClarification +
        SelectedReportedSummary.ApprovedForCreation +
        SelectedReportedSummary.MissingMinimumData;

    protected ManualOption SelectedManual =>
        ManualOptions.FirstOrDefault(x => x.Code == selectedManualCode) ?? ManualOptions.First();

    protected IEnumerable<ToolAssetOption> FilteredAssets
    {
        get
        {
            var query = assets.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(assetSearchText))
            {
                var value = assetSearchText.Trim();

                query = query.Where(x =>
                    Contains(x.InternalCode, value) ||
                    Contains(x.Name, value) ||
                    Contains(x.SerialNumber, value) ||
                    Contains(x.FixedAssetCode, value) ||
                    Contains(x.BranchCode, value) ||
                    Contains(x.ResponsiblePersonName, value) ||
                    Contains(FormatAssetOption(x), value));
            }

            return query
                .OrderBy(x => x.InternalCode)
                .Take(120);
        }
    }

    protected IEnumerable<DocumentViewItem> FilteredDocuments
    {
        get
        {
            var query = documents
                .Where(x => string.Equals(GetDocumentCategory(x), selectedCategory, StringComparison.OrdinalIgnoreCase));

            if (Guid.TryParse(selectedToolFilter, out var toolId))
            {
                query = query.Where(x => x.ToolAssetId == toolId);
            }

            if (!string.IsNullOrWhiteSpace(documentSearchText))
            {
                var value = documentSearchText.Trim();

                query = query.Where(x =>
                    Contains(x.FileName, value) ||
                    Contains(x.Description, value) ||
                    Contains(x.ToolInternalCode, value) ||
                    Contains(x.ToolName, value) ||
                    Contains(x.BranchCode, value) ||
                    Contains(x.UploadedBy, value) ||
                    Contains(GetDocumentCategory(x), value));
            }

            return query.OrderByDescending(x => x.UploadedAt);
        }
    }

    protected IEnumerable<ReportedItem> FilteredReportedItems
    {
        get
        {
            var query = selectedReportedBoard.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(physicalSearchText))
            {
                var value = physicalSearchText.Trim();

                query = query.Where(x =>
                    Contains(x.ToolInternalCode, value) ||
                    Contains(x.ToolName, value) ||
                    Contains(x.ReportedCode, value) ||
                    Contains(x.ReportedName, value) ||
                    Contains(x.ParticipantName, value) ||
                    Contains(x.ParticipantUserName, value) ||
                    Contains(x.ReportTypeLabel, value) ||
                    Contains(x.ReconciliationStatusLabel, value) ||
                    Contains(x.FoundLocation, value) ||
                    Contains(x.ExpectedLocation, value));
            }

            return query.OrderByDescending(x => x.ReportedAt);
        }
    }

    protected List<DocumentCategory> Categories { get; } = new()
    {
        new("📘", "Ficha técnica"),
        new("🧾", "Factura"),
        new("🛠️", "Evidencia mantenimiento"),
        new("📦", "Toma física"),
        new("✅", "Acta"),
        new("🛡️", "Garantía")
    };

    protected List<ManualOption> ManualOptions { get; } = new()
    {
        new(
            "Dashboard",
            "📊",
            "Dashboard",
            "Principal",
            "El Dashboard es la vista ejecutiva de NAVI Herramientas y AF. Resume el estado general del inventario, activos por sede, herramientas asignadas, pendientes de toma física, diferencias, documentos, mantenimientos, compras y conciliación. Su objetivo no es editar información, sino entregar lectura rápida del comportamiento del sistema para gerencia, coordinación, auditoría y administración.",
            "Permitir que el usuario identifique rápidamente alertas, pendientes y prioridades operativas sin entrar módulo por módulo.",
            new[]
            {
                "Ingresar al Dashboard desde el menú Principal.",
                "Revisar los indicadores superiores de inventario, documentos, mantenimiento, compras, tomas físicas y conciliación.",
                "Validar si existen activos pendientes por toma física, diferencias sin conciliar, mantenimientos próximos o compras en revisión.",
                "Usar los bloques de resumen para decidir a qué módulo ingresar: Inventario, Toma física, Conciliación, Documentos, Reportes o Mantenimiento.",
                "Actualizar la vista cuando se necesite refrescar información real proveniente de API y base de datos.",
                "Interpretar los indicadores como una lectura general, no como una autorización para modificar registros.",
                "Comparar sedes, responsables y estados para detectar concentración de pendientes o riesgos operativos.",
                "Escalar a coordinación o administración cuando el Dashboard evidencie inconsistencias relevantes."
            },
            new[]
            {
                "El Dashboard es principalmente de consulta.",
                "No debe permitir modificar activos, documentos, solicitudes ni tomas físicas directamente.",
                "La información debe provenir de módulos reales y no de datos decorativos.",
                "Los indicadores deben respetar el alcance del usuario cuando aplique.",
                "Los usuarios con pocos permisos solo deben ver la información habilitada por su matriz.",
                "Los datos cancelados o rechazados pueden mostrarse como histórico, pero no deben sumarse como ejecución positiva.",
                "El Dashboard debe mantenerse compacto, legible y alineado con el diseño corporativo NAVI."
            },
            new[]
            {
                "Inventario de Herramientas y AF.",
                "Toma física.",
                "Conciliación.",
                "Documentos.",
                "Mantenimientos.",
                "Compras.",
                "Reportes ejecutivos."
            }),

        new(
            "Inventory",
            "🧰",
            "Inventario de Herramientas y AF",
            "Activos fijos",
            "El inventario es el maestro operativo de herramientas y activos fijos. Permite consultar celulares, computadores, herramientas, equipos de medición, bancos de prueba, escáneres, activos de taller y activos administrativos. Muestra código interno, nombre, tipo, categoría, sede, ubicación, serial, estado, clase, responsable y acceso al detalle.",
            "Centralizar la información base de cada herramienta o activo para que los demás procesos trabajen sobre un inventario único y trazable.",
            new[]
            {
                "Ingresar a Inventario de Herramientas y AF.",
                "Buscar por código interno, nombre, serial, sede, categoría, tipo, estado o responsable.",
                "Identificar si el registro corresponde a herramienta, activo fijo, equipo tecnológico, celular, computador, equipo especializado o elemento de taller.",
                "Revisar la sede, ubicación, responsable y estado operativo actual.",
                "Abrir Detalle para ver información completa, acciones rápidas, reglas operativas y eventos.",
                "Abrir Hoja de vida si el usuario cuenta con permiso para consultar información técnica.",
                "Validar si el activo está disponible, asignado, prestado, en mantenimiento, no localizado, inconsistente o pendiente de baja.",
                "Usar el inventario como base para documentos, mantenimiento, toma física, asignaciones y reportes."
            },
            new[]
            {
                "El inventario debe mostrar herramientas y activos fijos, no solamente herramientas de taller.",
                "Los celulares y computadores deben aparecer cuando el filtro o permiso corresponda.",
                "La creación, edición, baja o cambio de estado depende de permisos específicos.",
                "Cambiar estado operativo debe exigir observación y dejar trazabilidad.",
                "Marcar una herramienta como especializada debe conservar auditoría.",
                "Los activos dados de baja no deben usarse para nuevas asignaciones ni solicitudes operativas.",
                "Los cambios de sede, ubicación o responsable deben reflejarse en disponibilidad, hoja de vida y reportes.",
                "Un activo no debe duplicarse si ya existe con el mismo código, serial o referencia interna."
            },
            new[]
            {
                "Ficha técnica.",
                "Factura.",
                "Garantía.",
                "Hoja de vida técnica.",
                "Eventos del activo.",
                "Documentos cargados.",
                "Registros de toma física."
            }),

        new(
            "ToolDetail",
            "🧾",
            "Detalle del activo",
            "Activos fijos",
            "El Detalle del activo concentra la información individual de una herramienta o activo fijo. Permite revisar datos principales, sede, ubicación, responsable, estado, clase, trazabilidad, acciones rápidas, actividad reciente y acceso a hoja de vida cuando el permiso lo permite.",
            "Dar una vista operativa completa de un activo específico para tomar decisiones de estado, ubicación, mantenimiento, asignación o documentación.",
            new[]
            {
                "Abrir Detalle desde inventario, disponibilidad, asignación o historial.",
                "Revisar los datos principales del activo: código, nombre, serial, categoría, tipo, sede y responsable.",
                "Validar el estado actual y la ubicación física registrada.",
                "Consultar reglas operativas para entender si el activo puede asignarse, prestarse, mantenerse o darse de baja.",
                "Usar acciones rápidas solo cuando se tenga permiso: cambiar estado, guardar observación, marcar especializada o actualizar datos.",
                "Abrir Hoja de vida únicamente si el rol cuenta con permiso de apertura desde detalle.",
                "Revisar actividad reciente para identificar cambios anteriores.",
                "Registrar observaciones claras cuando se realice una acción operativa."
            },
            new[]
            {
                "El detalle puede ser de solo lectura o editable según matriz de permisos.",
                "Las acciones rápidas dependen del permiso ToolDetail.Actions.",
                "Abrir hoja de vida desde detalle depende del permiso ToolDetail.LifeRecord.Open.",
                "Cambiar estado operativo depende del permiso correspondiente y debe dejar observación.",
                "No se deben mostrar acciones que el usuario no puede ejecutar.",
                "La seguridad debe aplicarse en botones, métodos y endpoint si existe acción en API.",
                "Los eventos del activo deben quedar asociados a usuario, fecha y descripción."
            },
            new[]
            {
                "Hoja de vida técnica.",
                "Ficha técnica.",
                "Documentos del activo.",
                "Historial de eventos.",
                "Historial de asignación.",
                "Registros de mantenimiento."
            }),

        new(
            "Availability",
            "📍",
            "Disponible y ubicación",
            "Activos fijos",
            "Disponible y ubicación permite saber dónde está físicamente una herramienta o activo, si está disponible, asignado, prestado, en mantenimiento, dañado, no localizado, inconsistente, pendiente de baja o dado de baja. También permite gestionar sede, taller, almacén, ubicación y estado operativo cuando el usuario tiene permisos.",
            "Controlar disponibilidad real, ubicación operativa y movimientos entre sedes, talleres, almacenes o responsables.",
            new[]
            {
                "Ingresar a Disponible y ubicación.",
                "Buscar el activo por código, nombre, sede, responsable, ubicación, estado o serial.",
                "Revisar el estado operativo y ubicación actual.",
                "Ingresar a gestionar ubicación cuando el permiso lo permita.",
                "Actualizar sede, taller, almacén o ubicación física.",
                "Cambiar estado operativo solo si el permiso está habilitado.",
                "Agregar observación clara justificando el movimiento o cambio de estado.",
                "Guardar y validar que la ubicación se refleje en inventario, detalle y reportes.",
                "Usar esta vista para validar disponibilidad antes de asignar, prestar o enviar a mantenimiento."
            },
            new[]
            {
                "AssetAvailability.View permite consultar disponibilidad.",
                "AssetAvailability.Edit permite cambiar sede, taller, almacén o ubicación.",
                "AssetAvailability.Status.Change permite cambiar estado operativo.",
                "Todo cambio debe dejar trazabilidad con usuario y fecha.",
                "No se deben modificar activos sin observación cuando el cambio afecta ubicación o estado.",
                "Los activos en mantenimiento, dados de baja o no aptos no deben tratarse como disponibles.",
                "El sistema debe impedir que un usuario sin permiso realice cambios aunque vea la pantalla."
            },
            new[]
            {
                "Historial de ubicación.",
                "Eventos del activo.",
                "Inventario.",
                "Asignaciones.",
                "Hoja de vida.",
                "Reportes de disponibilidad."
            }),

        new(
            "Assignment",
            "🔁",
            "Asignación de Herramientas y AF",
            "Activos fijos",
            "Asignación administra la responsabilidad operativa de herramientas y activos. Permite asignar a responsables, usuarios, talleres o almacenes; solicitar préstamos; aprobar o denegar solicitudes; regresar activos al taller o almacén; y consultar historial.",
            "Controlar quién tiene cada activo, cuándo lo recibió, cuándo debe devolverlo y qué movimientos se han realizado.",
            new[]
            {
                "Ingresar a Asignar Herramientas y AF.",
                "Usar Nueva asignación directa para seleccionar activo y responsable cuando el permiso lo permita.",
                "Registrar fecha, responsable destino, ubicación y observación.",
                "Guardar la asignación para que el activo cambie de responsable operativo.",
                "Para préstamos, el técnico solicita la herramienta indicando fecha de devolución y observación.",
                "El coordinador, herramientero o rol autorizado revisa la solicitud.",
                "Aprobar cuando el préstamo sea válido y el activo esté disponible.",
                "Denegar cuando el préstamo no proceda o el activo no pueda entregarse.",
                "Registrar regreso del activo al taller, almacén o ubicación definida.",
                "Consultar historial para validar movimientos anteriores."
            },
            new[]
            {
                "AssetAssignment.View permite ver asignaciones.",
                "AssetAssignment.Assign permite nueva asignación directa.",
                "AssetAssignment.Request permite solicitar préstamo.",
                "AssetAssignment.Approve permite aprobar préstamo.",
                "AssetAssignment.Deny permite denegar préstamo.",
                "AssetAssignment.Return permite regresar el activo.",
                "AssetAssignment.History permite consultar historial.",
                "Si el usuario no tiene responsable operativo asociado, debe mostrarse mensaje claro y no romper la pantalla.",
                "No se deben mostrar celulares o computadores si el filtro funcional excluye esos tipos; sí deben mostrarse cuando el proceso requiera activos fijos completos.",
                "Toda asignación debe quedar trazable por usuario, fecha, activo y observación."
            },
            new[]
            {
                "Solicitud de préstamo.",
                "Historial de asignaciones.",
                "Acta de entrega.",
                "Eventos del activo.",
                "Hoja de vida."
            }),

        new(
            "AssignmentHistory",
            "🕘",
            "Historial de asignaciones",
            "Activos fijos",
            "El historial de asignaciones muestra movimientos realizados sobre cada herramienta o activo: asignaciones directas, préstamos, devoluciones, cambios de responsable, cambios de ubicación y movimientos entre sedes o talleres.",
            "Permitir auditoría completa sobre la custodia y trazabilidad de los activos.",
            new[]
            {
                "Ingresar al historial desde el menú o desde el detalle de un activo.",
                "Filtrar por activo, código, usuario, responsable, sede, fecha o tipo de movimiento.",
                "Revisar quién recibió el activo y quién lo entregó.",
                "Validar fechas de asignación, devolución, préstamo y regreso.",
                "Consultar observaciones registradas durante el movimiento.",
                "Abrir el activo o su hoja de vida cuando el permiso lo permita.",
                "Usar el historial para resolver diferencias de toma física o conciliación.",
                "Exportar o reportar la información si el rol tiene permiso correspondiente."
            },
            new[]
            {
                "El historial no debe permitir modificar registros cerrados.",
                "Los movimientos deben quedar en orden cronológico.",
                "Debe poder consultarse por activo y por responsable.",
                "Los usuarios sin permiso solo deben ver la información autorizada por alcance.",
                "El historial debe servir como soporte para conciliación, auditoría y toma física.",
                "No se deben eliminar movimientos históricos sin permiso administrativo y validación."
            },
            new[]
            {
                "Asignaciones.",
                "Préstamos.",
                "Devoluciones.",
                "Actas.",
                "Eventos del activo.",
                "Reportes."
            }),

        new(
            "LifeRecord",
            "📋",
            "Hoja de vida de Herramientas y AF",
            "Activos fijos",
            "La Hoja de vida técnica conserva la historia completa del activo: datos principales, especificaciones, fechas de mantenimiento, último mantenimiento, días restantes, accesorios, fotos, ficha técnica, documentos, prácticas seguras, cronograma y eventos.",
            "Tener trazabilidad técnica y documental del activo desde su creación hasta su baja.",
            new[]
            {
                "Abrir Hoja de vida desde inventario, detalle, historial o planes de mantenimiento si el permiso lo permite.",
                "Revisar datos principales del equipo o herramienta.",
                "Consultar especificaciones técnicas, seriales, marca, modelo y vida útil.",
                "Validar último mantenimiento, próximo mantenimiento y días restantes.",
                "Gestionar accesorios cuando el permiso esté habilitado.",
                "Consultar o cargar documentación técnica según permisos.",
                "Gestionar cronograma de mantenimiento desde la seguridad correspondiente.",
                "Revisar prácticas seguras aplicables al activo.",
                "Consultar eventos de ciclo de vida.",
                "Exportar PDF o Excel solo si el permiso de exportación está habilitado."
            },
            new[]
            {
                "TechnicalLifeRecord.View permite ver hoja de vida.",
                "TechnicalLifeRecord.Edit permite editar datos técnicos.",
                "TechnicalLifeRecord.Accessories permite gestionar accesorios.",
                "TechnicalLifeRecord.Documents permite gestionar documentación técnica.",
                "TechnicalLifeRecord.Maintenance permite gestionar cronograma.",
                "TechnicalLifeRecord.SafePractices permite ver prácticas seguras.",
                "TechnicalLifeRecord.Export permite exportar.",
                "Hoja de vida desde planes solo debe abrir si el usuario tiene permisos de hoja de vida, no por tener solo permisos de planes.",
                "Los cambios deben alimentar eventos y documentos asociados."
            },
            new[]
            {
                "Ficha técnica.",
                "Factura.",
                "Garantía.",
                "Evidencia mantenimiento.",
                "Cronograma.",
                "Prácticas seguras.",
                "Eventos."
            }),

        new(
            "Purchase",
            "🛒",
            "Solicitud de compra Herramientas y AF",
            "Operaciones",
            "Solicitud de compra permite crear requerimientos SPC/MRO para herramientas, activos fijos, equipos, repuestos o elementos requeridos por operación. Incluye encabezado, herramientas solicitadas, borrador, confirmación, cotizaciones y generación de solicitud final.",
            "Formalizar necesidades de compra con trazabilidad de responsable, sede, prioridad, costo, proveedor y estado.",
            new[]
            {
                "Ingresar a Solicitar Compra Herramientas y AF.",
                "Crear encabezado con sede, solicitante, responsable aprobador, prioridad, fecha requerida y necesidad.",
                "Agregar una o varias herramientas o activos solicitados.",
                "Registrar nombre del equipo, cantidad, información técnica, detalle, novedad y evidencia si aplica.",
                "Guardar como borrador cuando la solicitud aún no esté lista.",
                "Confirmar solicitud cuando la información esté completa.",
                "Realizar cotización desde esta misma solicitud, no desde la consulta.",
                "Registrar proveedor, fecha, tiempo de entrega, precio antes de IVA y detalles.",
                "Seleccionar cotización válida.",
                "Generar solicitud final cuando exista cotización seleccionada.",
                "Consultar seguimiento posterior en Consulta Compras y Mtto."
            },
            new[]
            {
                "Purchases.Request permite crear borrador y confirmar solicitud.",
                "Purchases.Quote permite registrar cotización.",
                "Purchases.Generate permite generar solicitud final.",
                "La cotización solo se habilita cuando la solicitud está confirmada.",
                "La solicitud final requiere mínimo una cotización seleccionada.",
                "Borradores pueden editarse; solicitudes generadas quedan como histórico de proceso.",
                "Eliminar solo debe aplicar a borradores o registros locales permitidos, no a solicitudes ya procesadas.",
                "Los costos de solicitudes rechazadas, canceladas o en revisión no deben sumarse como costo final.",
                "Las solicitudes generadas deben aparecer en Consulta Compras y Mtto."
            },
            new[]
            {
                "Cotización.",
                "Factura.",
                "Acta de recepción.",
                "Ficha técnica.",
                "Solicitud final.",
                "Soporte de proveedor."
            }),

        new(
            "MaintenanceRequest",
            "🛠️",
            "Solicitud mantenimiento",
            "Operaciones",
            "Solicitud mantenimiento permite registrar necesidades preventivas, correctivas o de calibración sobre herramientas y activos. El flujo incluye encabezado, activo o herramienta, evidencia, criticidad, cotización, generación de solicitud final, ejecución, cierre y actualización del cronograma.",
            "Controlar mantenimientos con trazabilidad de costo, proveedor, técnico, estado, evidencia y cierre operativo.",
            new[]
            {
                "Ingresar a Solicitar Mantenimiento.",
                "Crear encabezado con sede, tipo de mantenimiento, prioridad, responsable y descripción.",
                "Seleccionar o registrar herramienta, equipo o activo a intervenir.",
                "Agregar evidencia, información técnica, serial, falla, novedad u observación.",
                "Guardar como borrador o confirmar solicitud.",
                "Registrar cotización de mantenimiento cuando la solicitud esté confirmada.",
                "Seleccionar proveedor, costo, tiempo de reparación y detalles técnicos.",
                "Generar solicitud final cuando exista cotización válida.",
                "Dar seguimiento desde Consulta Compras y Mtto.",
                "Ejecutar mantenimiento cuando esté aprobado o programado.",
                "Cerrar o aprobar mantenimiento al finalizar la intervención.",
                "Validar que el cierre actualice hoja de vida y cronograma cuando el activo esté asociado."
            },
            new[]
            {
                "Maintenance.Request permite crear solicitud.",
                "Maintenance.Quote permite registrar cotización.",
                "Maintenance.Generate permite generar solicitud final.",
                "Maintenance.Execute permite registrar ejecución.",
                "Maintenance.Close permite cerrar o aprobar mantenimiento.",
                "Maintenance.Reject permite rechazar mantenimiento.",
                "Un mantenimiento cerrado debe dejar evidencia técnica cuando aplique.",
                "El costo solo suma cuando la solicitud está aprobada, programada, en ejecución o cerrada.",
                "Rechazadas y canceladas se conservan en histórico, pero no suman costo.",
                "El activo puede pasar a mantenimiento según flujo aprobado."
            },
            new[]
            {
                "Evidencia mantenimiento.",
                "Factura.",
                "Cotización.",
                "Cierre técnico.",
                "Hoja de vida.",
                "Cronograma."
            }),

        new(
            "MaintenanceConsultation",
            "🔎",
            "Consulta Compras y Mtto.",
            "Operaciones",
            "Consulta Compras y Mtto. es la vista de seguimiento de solicitudes de compra y mantenimiento. No es la pantalla para cotizar ni generar desde cero. Permite consultar información, ver detalle en ventana emergente, revisar costos separados, aprobar, rechazar, ejecutar o cerrar según permisos.",
            "Dar trazabilidad al ciclo completo de compras y mantenimientos, diferenciando estados, costos, responsables y acciones disponibles.",
            new[]
            {
                "Ingresar a Consulta Compras y Mtto.",
                "Filtrar por compra, mantenimiento o ambos.",
                "Filtrar por estado: en revisión, aprobada, programada, en ejecución, cerrada, rechazada o cancelada.",
                "Buscar por solicitud, activo, proveedor, técnico, responsable o sede.",
                "Revisar la tabla diferenciada entre compras y mantenimientos.",
                "Usar Ver para abrir la ventana emergente de detalle de la solicitud.",
                "Aprobar compra solo cuando esté en revisión y el permiso Purchases.Approve esté habilitado.",
                "Rechazar compra solo cuando el permiso Purchases.Reject esté habilitado.",
                "Aprobar mantenimiento, ejecutar, cerrar o rechazar según estado y permisos.",
                "Validar el resumen de costos separado para compras y mantenimiento.",
                "Actualizar para traer nuevamente información desde API y almacenamiento local."
            },
            new[]
            {
                "La cotización se realiza desde la solicitud, no desde consulta.",
                "No debe existir botón Generar final en consulta.",
                "Todo usuario con acceso al módulo puede usar Ver.",
                "Las acciones deben depender de permisos: Purchases.Approve, Purchases.Reject, Maintenance.Execute, Maintenance.Close y Maintenance.Reject.",
                "Las solicitudes rechazadas, canceladas, borradores y en revisión se muestran, pero no suman costo.",
                "Compras y mantenimiento deben sumar costos por separado.",
                "Los registros de API tienen prioridad sobre datos locales duplicados.",
                "Los errores deben indicar si falta permiso, estado inválido o falla de API.",
                "Las solicitudes cerradas deben quedar en histórico, no desaparecer."
            },
            new[]
            {
                "Solicitud de compra.",
                "Solicitud mantenimiento.",
                "Cotizaciones.",
                "Factura.",
                "Cierre técnico.",
                "Costos.",
                "Histórico de estados."
            }),

        new(
            "MaintenancePlans",
            "🗓️",
            "Planes de mantenimiento",
            "Operaciones",
            "Planes de mantenimiento permite consultar activos con seguimiento preventivo, próximos vencimientos, estado del plan, indicadores, mantenimientos programados y cronograma por activo. Desde esta vista se puede entrar al cronograma cuando el permiso de planes lo permite.",
            "Planificar mantenimientos preventivos, correctivos o de calibración para reducir fallas y controlar vencimientos.",
            new[]
            {
                "Ingresar a Planes de mantenimiento.",
                "Revisar distribución del plan: al día, por vencer y vencidos.",
                "Consultar indicadores de activos, vencimientos, solicitudes y completados.",
                "Filtrar por estado o buscar código, activo, sede o responsable.",
                "Abrir Cronograma cuando se tenga MaintenancePlans.View, Create, Edit o Delete.",
                "Crear plan si el permiso MaintenancePlans.Create está habilitado.",
                "Editar plan si el permiso MaintenancePlans.Edit está habilitado.",
                "Eliminar o desactivar plan si el permiso MaintenancePlans.Delete está habilitado.",
                "Solicitar mantenimiento desde el plan solo si el permiso Maintenance.Request está habilitado.",
                "Abrir Hoja de vida solo si el usuario tiene permiso de hoja de vida."
            },
            new[]
            {
                "Ver planes depende de MaintenancePlans.View.",
                "Crear plan depende de MaintenancePlans.Create.",
                "Editar plan depende de MaintenancePlans.Edit.",
                "Eliminar o desactivar depende de MaintenancePlans.Delete.",
                "Cronograma se controla por permisos de planes.",
                "Hoja de vida no se habilita por permisos de planes, sino por permisos TechnicalLifeRecord.",
                "El botón superior Consulta M-C no debe aparecer en planes.",
                "Solicitar mantenimiento depende de Maintenance.Request.",
                "Los indicadores deben salir del inventario y registros reales de mantenimiento."
            },
            new[]
            {
                "Cronograma de mantenimiento.",
                "Hoja de vida.",
                "Solicitud mantenimiento.",
                "Evidencia mantenimiento.",
                "Reportes de mantenimiento."
            }),

        new(
            "PhysicalCount",
            "📦",
            "Toma física",
            "Control",
            "Toma física es el proceso de inventario físico por sede. Permite crear campañas, generar participantes, iniciar, cerrar o cancelar toma, validar activos asignados, reportar no listados, cargar evidencias y preparar conciliación. Incluye herramientas y activos de empresa: computadores, celulares, equipos, herramientas, activos de taller y activos administrativos.",
            "Validar físicamente lo que existe, lo asignado a cada usuario, lo no localizado, lo reportado como extra y lo que debe pasar a conciliación.",
            new[]
            {
                "Ingresar a Tomas Físicas.",
                "Crear toma física por sede cuando el usuario tenga PhysicalCounts.Create.",
                "Seleccionar sede y responsable de la toma.",
                "Generar participantes de acuerdo con usuarios, responsables y activos asociados a la sede.",
                "Iniciar toma física con PhysicalCounts.Start.",
                "Entrar al detalle para visualizar participantes y avance.",
                "Si el usuario no puede crear, debe ver únicamente su participante y sus pendientes.",
                "Presionar Pendientes para ver activos asignados al participante.",
                "Reportar activo asignado con estado físico, operativo, ubicación, observación y evidencia.",
                "Agregar activo no listado cuando exista permiso de reporte y el activo no esté en el sistema.",
                "Cargar evidencia cuando PhysicalCounts.Evidence.Upload esté habilitado.",
                "Cerrar toma cuando todos los participantes y pendientes estén gestionados.",
                "Ir a conciliación solo para seguimiento o decisión según permisos."
            },
            new[]
            {
                "PhysicalCounts.View permite consultar tomas.",
                "PhysicalCounts.Create permite crear campañas y ver todos los usuarios de la toma.",
                "PhysicalCounts.Start permite iniciar toma.",
                "PhysicalCounts.Close permite cerrar toma.",
                "PhysicalCounts.Cancel permite cancelar toma.",
                "PhysicalCounts.Report permite reportar activos asignados o no listados.",
                "PhysicalCounts.Evidence.Upload permite cargar evidencia.",
                "Si el usuario no tiene permiso de crear, solo debe ver su información y sus pendientes.",
                "La toma se genera por sede, no solo por taller.",
                "Usuarios de la sede deben participar aunque no pertenezcan al taller si tienen activos asignados.",
                "Los registros reportados no deben desaparecer; alimentan conciliación.",
                "La toma cancelada queda como histórico, pero no como ejecución positiva."
            },
            new[]
            {
                "Consolidado toma física.",
                "Evidencias.",
                "Acta de toma.",
                "Registros reportados.",
                "Conciliación.",
                "Reporte por participante."
            }),

        new(
            "Reconciliation",
            "⚖️",
            "Conciliación",
            "Control",
            "Conciliación resuelve diferencias entre inventario del sistema y toma física. Permite revisar activos encontrados, no encontrados, no listados, dañados, devueltos o con datos incompletos. Las acciones permiten aclarar, aprobar creación, conciliar o rechazar según permisos.",
            "Cerrar administrativamente las diferencias de toma física sin perder evidencia ni trazabilidad.",
            new[]
            {
                "Ingresar a Conciliación.",
                "Seleccionar o filtrar por toma física, sede, usuario, tipo de reporte, estado o datos mínimos.",
                "Revisar cada registro reportado por los usuarios.",
                "Usar Solicitar aclaración cuando la información requiera explicación del usuario.",
                "Usar Aprobar creación de activo cuando el registro no exista en inventario y tenga datos suficientes.",
                "Usar Conciliar cuando la información reportada corresponde al inventario real.",
                "Usar Rechazar cuando el registro no proceda.",
                "Consultar estado de activos propios cuando el usuario solo tenga vista.",
                "Validar que las decisiones queden trazadas en reportes y documentos."
            },
            new[]
            {
                "Reconciliation.View permite ver conciliación.",
                "Reconciliation.Clarify permite solicitar aclaración.",
                "Reconciliation.ApproveCreation permite aprobar creación de activo.",
                "Reconciliation.Manage permite conciliar o rechazar diferencias.",
                "Administrador puede ver todos los registros; usuarios sin rol administrativo solo deben ver sus reportes.",
                "Rechazar no borra evidencia, solo cierra la decisión.",
                "Aprobar creación debe llevar información suficiente para crear activo.",
                "Ir a conciliación desde toma física puede ser solo seguimiento si no hay permisos de decisión.",
                "Las acciones deben estar bloqueadas si no existe permiso."
            },
            new[]
            {
                "Toma física.",
                "Acta de conciliación.",
                "Registro no listado.",
                "Evidencias.",
                "Creación desde toma.",
                "Reportes de diferencias."
            }),

        new(
            "Documents",
            "📄",
            "Documentos",
            "Control documental",
            "Gestión documental permite cargar, consultar, visualizar y descargar soportes reales asociados a activos, herramientas y procesos. Maneja fichas técnicas, facturas, evidencias de mantenimiento, tomas físicas, actas y garantías. También contiene los manuales funcionales del sistema.",
            "Centralizar los documentos que soportan el ciclo de vida de cada activo y los procesos operativos de NAVI.",
            new[]
            {
                "Ingresar a Documentos.",
                "Seleccionar clasificación: ficha técnica, factura, evidencia mantenimiento, toma física, acta o garantía.",
                "Buscar y seleccionar un activo existente del inventario.",
                "Ingresar nombre del documento y responsable de carga.",
                "Seleccionar archivo PDF, imagen, Excel o Word.",
                "Agregar descripción clara del soporte.",
                "Guardar documento cuando Documents.Upload esté habilitado.",
                "Consultar documentos encontrados por clasificación, activo o responsable.",
                "Usar Ver para visualizar el documento si Documents.View está habilitado.",
                "Usar Descargar solo si Documents.Download está habilitado.",
                "Para toma física, visualizar o descargar consolidado según permisos; no subir documentos manuales en esa clasificación.",
                "Abrir manuales funcionales desde el panel lateral de Documentos."
            },
            new[]
            {
                "Documents.View permite ver documentos y manuales.",
                "Documents.Upload permite cargar documentos.",
                "Documents.Download permite descargar documentos o consolidado.",
                "Documents.Delete permite eliminar documentos cuando aplique.",
                "No se deben cargar documentos a activos inexistentes.",
                "La clasificación documental define el tipo de soporte.",
                "La descarga debe estar bloqueada si el permiso no existe.",
                "La vista Ver no debe permitir descarga indirecta si el permiso de descarga está restringido.",
                "Toma física no permite cargar documentos desde esta pantalla, solo visualizar consolidado.",
                "Los documentos deben alimentar hoja de vida y reportes."
            },
            new[]
            {
                "Ficha técnica.",
                "Factura.",
                "Evidencia mantenimiento.",
                "Acta.",
                "Garantía.",
                "Consolidado toma física.",
                "Manual funcional."
            }),

        new(
            "Reports",
            "📈",
            "Reportes",
            "Control",
            "Reportes permite consultar información ejecutiva y operativa de inventario, documentos, mantenimiento, compras, toma física y conciliación. Cada reporte puede filtrar por sede, periodo, estado o texto, y exportar cuando el permiso lo permite.",
            "Entregar información ordenada para análisis, auditoría, seguimiento gerencial y soporte operativo.",
            new[]
            {
                "Ingresar al Centro de reportes.",
                "Seleccionar tipo de reporte: resumen ejecutivo, inventario, documentos, mantenimiento, compras, toma física o conciliación.",
                "Aplicar filtros por sede, periodo, estado o búsqueda textual.",
                "Revisar indicadores principales del reporte seleccionado.",
                "Consultar resultados en tabla.",
                "Limpiar filtros cuando se requiera volver a la vista general.",
                "Descargar reporte únicamente cuando Reports.Export esté habilitado.",
                "Validar que el archivo exportado conserve filtros actuales.",
                "Usar reportes para seguimiento de pendientes, costos, documentos, diferencias o cumplimiento."
            },
            new[]
            {
                "Reports.View permite acceder a reportes.",
                "Reports.Export permite descargar reportes.",
                "Si no existe Reports.Export, los botones o enlaces de descarga deben quedar bloqueados.",
                "La exportación no debe saltarse seguridad por enlaces directos.",
                "Los reportes deben alimentarse de datos reales.",
                "El resumen lateral debe ser compacto y no invadir la tabla.",
                "Rechazadas y canceladas deben aparecer como histórico cuando aplique, pero no sumar como positivo.",
                "Los filtros aplicados deben conservarse en la exportación."
            },
            new[]
            {
                "CSV de reporte.",
                "Inventario.",
                "Documentos.",
                "Mantenimiento.",
                "Compras.",
                "Toma física.",
                "Conciliación."
            }),

        new(
            "Settings",
            "⚙️",
            "Configuración",
            "Administración",
            "Configuración administra los catálogos y parámetros base del sistema: sedes, zonas, almacenes, talleres, ubicaciones, responsables, tipos de activo, categorías, estados, tipos de documentos, tipos de mantenimiento, planes, causales y parámetros.",
            "Mantener datos maestros consistentes para que inventario, disponibilidad, asignación, toma física, documentos, mantenimiento y reportes funcionen correctamente.",
            new[]
            {
                "Ingresar a Configuración.",
                "Seleccionar la opción a gestionar.",
                "Crear registros nuevos cuando el permiso lo permita.",
                "Editar información existente cuando el permiso lo permita.",
                "Desactivar registros que no deben seguir operativos.",
                "Mantener sedes, zonas, talleres y ubicaciones alineadas con la operación real.",
                "Administrar responsables y tipos de activos usados en inventario y toma física.",
                "Actualizar tipos de documentos, mantenimiento, causales y estados.",
                "Validar que los catálogos alimenten correctamente las listas de otros módulos."
            },
            new[]
            {
                "Settings.View permite ver configuración.",
                "Settings.Manage permite gestionar catálogos operativos.",
                "Security.Users y Security.Roles controlan usuarios y roles.",
                "Usuarios y roles se manejan como seguridad, no como catálogo normal.",
                "No se deben duplicar sedes, zonas, ubicaciones o responsables.",
                "No se deben eliminar registros con trazabilidad histórica sin validación.",
                "Los banners no deben repetirse visualmente.",
                "Cada opción debe respetar la seguridad implementada."
            },
            new[]
            {
                "Sedes.",
                "Zonas.",
                "Almacenes.",
                "Talleres.",
                "Ubicaciones.",
                "Responsables.",
                "Tipos de activo.",
                "Categorías.",
                "Estados.",
                "Parámetros."
            }),

        new(
            "Users",
            "👤",
            "Usuarios",
            "Seguridad",
            "Usuarios administra las cuentas del sistema, datos básicos, rol asignado, sede, estado activo, contraseña inicial o cambio de contraseña. Es la base para que cada persona opere según rol, sede y alcance.",
            "Controlar quién puede entrar al sistema y bajo qué permisos operativos trabaja.",
            new[]
            {
                "Ingresar a Usuarios desde Configuración.",
                "Crear usuario con documento, nombre, correo, cargo, área, rol y sede.",
                "Asignar rol correcto de acuerdo con la función real.",
                "Definir contraseña inicial y confirmarla.",
                "Marcar usuario activo cuando deba ingresar al sistema.",
                "Editar usuario cuando cambie rol, sede, cargo o estado.",
                "Cambiar contraseña cuando sea necesario.",
                "Eliminar o desactivar usuarios solo si el permiso lo permite.",
                "Validar que el usuario tenga responsable operativo cuando deba asignar o reportar activos."
            },
            new[]
            {
                "Security.Users controla la gestión de usuarios.",
                "Un usuario sin rol no debe operar procesos.",
                "La sede del usuario afecta alcance en tomas físicas, reportes y conciliación.",
                "El rol define qué módulos aparecen y qué acciones puede ejecutar.",
                "Cambiar contraseña debe estar restringido.",
                "Usuarios técnicos o de toma física pueden tener permisos propios sin ser administradores.",
                "El usuario administrador no debe aparecer como participante de toma si no tiene activos asignados o no corresponde a la sede/proceso."
            },
            new[]
            {
                "Roles y permisos.",
                "Matriz de permisos.",
                "Sede del usuario.",
                "Responsable operativo.",
                "Sesión web.",
                "App móvil."
            }),

        new(
            "Roles",
            "🔐",
            "Roles y permisos",
            "Seguridad",
            "Roles y permisos define la matriz de acción del sistema. Permite indicar qué puede ver, crear, editar, aprobar, rechazar, reportar, exportar, cargar, descargar, administrar o usar desde móvil cada perfil.",
            "Aplicar seguridad real por rol, evitando que un usuario ejecute acciones que no tiene autorizadas.",
            new[]
            {
                "Ingresar a Roles y permisos.",
                "Seleccionar un rol existente o plantilla por cargo.",
                "Definir código, nombre y descripción.",
                "Marcar permisos por módulo y acción.",
                "Guardar matriz del rol.",
                "Asignar rol a usuarios.",
                "Validar con usuario de prueba que el menú, botones y endpoints respeten permisos.",
                "Probar módulos críticos: documentos, reportes, toma física, conciliación, planes, compras y mantenimiento.",
                "Ajustar permisos hasta que cada rol vea únicamente lo necesario."
            },
            new[]
            {
                "La seguridad no debe limitarse a ocultar botones.",
                "Los métodos de UI y endpoints también deben validar permisos.",
                "Los roles personalizados deben funcionar igual que roles administrativos.",
                "Técnicos deben operar solo sus pendientes o activos asignados.",
                "Crear toma física habilita vista global de participantes; solo consultar/reportar limita a información propia.",
                "Descargar documentos y reportes depende de permisos específicos.",
                "Hoja de vida, planes, mantenimiento y compras tienen permisos separados.",
                "El menú lateral debe mostrar únicamente módulos habilitados."
            },
            new[]
            {
                "Usuarios.",
                "PermissionGuard.",
                "RequirePermission.",
                "Matriz de permisos.",
                "Alcance por sede.",
                "App móvil."
            }),

        new(
            "Mobile",
            "📱",
            "App móvil",
            "Móvil",
            "La App móvil debe llamarse App móvil y no Móvil técnico. Debe aplicar la misma seguridad del aplicativo web: mostrar módulos, pantallas y acciones según permisos. Está pensada para técnicos, usuarios de toma física, responsables, herramienteros y personal operativo que reporta activos, novedades, daños, préstamos o evidencias desde campo.",
            "Permitir operación móvil controlada, segura y alineada con la matriz de permisos web.",
            new[]
            {
                "Ingresar a la App móvil con usuario y rol asignado.",
                "Mostrar únicamente los módulos habilitados por permisos Mobile y permisos funcionales.",
                "Consultar herramientas asignadas cuando Mobile.Tools.View esté habilitado.",
                "Reportar herramientas o activos durante toma física cuando PhysicalCounts.Report esté habilitado.",
                "Cargar evidencia desde cámara o archivo cuando PhysicalCounts.Evidence.Upload o Documents.Upload estén habilitados.",
                "Reportar daño o novedad cuando Mobile.Damage.Report esté habilitado.",
                "Reportar preoperacional SSTA cuando Mobile.PreOperational.Report esté habilitado.",
                "Solicitar préstamo desde móvil cuando Mobile.Loans.Request o AssetAssignment.Request estén habilitados.",
                "Restringir descarga, edición, aprobación o conciliación si no existe permiso.",
                "Mantener diseño simple, rápido y funcional para celular."
            },
            new[]
            {
                "El nombre del módulo debe ser App móvil.",
                "La App móvil debe respetar la misma matriz del aplicativo web.",
                "No debe mostrar pantallas sin permiso.",
                "No debe permitir acciones por enlace directo si el permiso no existe.",
                "El usuario técnico solo debe ver sus activos, pendientes o procesos asignados.",
                "La evidencia móvil debe asociarse al activo, toma física o proceso correcto.",
                "El registro móvil debe guardar usuario, fecha, sede y origen.",
                "La app móvil debe operar con enfoque PWA y sincronización controlada."
            },
            new[]
            {
                "Evidencias móviles.",
                "Toma física móvil.",
                "Herramientas asignadas.",
                "Reporte de daño.",
                "Solicitud de préstamo.",
                "Preoperacional SSTA."
            })
    };

    protected override async Task OnInitializedAsync()
    {
        uploadedBy = string.IsNullOrWhiteSpace(AuthSession.UserName)
            ? "admin"
            : AuthSession.UserName;

        await LoadAsync();
    }

    protected HttpClient CreateApiClient()
    {
        var api = HttpClientFactory.CreateClient("NaviApi");
        apiBaseUrl = api.BaseAddress?.ToString().TrimEnd('/') ?? "http://localhost:5218";
        return api;
    }

    protected async Task LoadAsync()
    {
        isLoading = true;
        message = null;

        try
        {
            var api = CreateApiClient();

            assets = await api.GetFromJsonAsync<List<ToolAssetOption>>("api/tools") ?? new();
            await LoadDocumentsAsync(api);
            await LoadPhysicalCountsAsync(api);
        }
        catch (Exception ex)
        {
            messageClass = "alert alert-danger";
            message = $"No se pudieron cargar los datos reales. Valida que la API esté activa en http://localhost:5218. Detalle: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    protected async Task LoadPhysicalCountsAsync(HttpClient api)
    {
        physicalCounts = await api.GetFromJsonAsync<List<PhysicalCountListItem>>("api/physical-counts") ?? new();

        if (physicalCounts.Count > 0 && string.IsNullOrWhiteSpace(selectedPhysicalCountId))
        {
            selectedPhysicalCountId = physicalCounts.First().Id.ToString();
        }

        await LoadSelectedPhysicalCountAsync(api);
    }

    protected async Task LoadSelectedPhysicalCountAsync(HttpClient api)
    {
        selectedPhysicalBoard = new PhysicalCountBoard();
        selectedReportedBoard = new ReportedItemsBoard();

        if (!Guid.TryParse(selectedPhysicalCountId, out var id))
        {
            return;
        }

        try
        {
            selectedPhysicalBoard = await api.GetFromJsonAsync<PhysicalCountBoard>($"api/physical-counts/{id}/participants-board")
                ?? new PhysicalCountBoard();
        }
        catch
        {
            selectedPhysicalBoard = new PhysicalCountBoard();
        }

        try
        {
            selectedReportedBoard = await api.GetFromJsonAsync<ReportedItemsBoard>($"api/physical-counts/{id}/reported-items-board")
                ?? new ReportedItemsBoard();
        }
        catch
        {
            selectedReportedBoard = new ReportedItemsBoard();
        }
    }

    protected async Task OnPhysicalCountSelected(ChangeEventArgs e)
    {
        selectedPhysicalCountId = e.Value?.ToString() ?? string.Empty;
        physicalSearchText = string.Empty;

        var api = CreateApiClient();
        await LoadSelectedPhysicalCountAsync(api);
    }

    protected async Task LoadDocumentsAsync(HttpClient api)
    {
        var result = new List<DocumentViewItem>();

        foreach (var asset in assets)
        {
            try
            {
                var toolDocuments = await api.GetFromJsonAsync<List<DocumentItemModel>>($"api/tools/{asset.Id}/documents") ?? new();

                foreach (var document in toolDocuments)
                {
                    result.Add(new DocumentViewItem
                    {
                        Id = document.Id,
                        ToolAssetId = document.ToolAssetId,
                        DocumentType = document.DocumentType,
                        FileName = document.FileName,
                        ObjectKey = document.ObjectKey,
                        ContentType = document.ContentType,
                        SizeBytes = document.SizeBytes,
                        UploadedBy = document.UploadedBy,
                        UploadedAt = document.UploadedAt,
                        Description = document.Description,
                        DownloadUrl = document.DownloadUrl,
                        ToolInternalCode = asset.InternalCode,
                        ToolName = asset.Name,
                        BranchCode = asset.BranchCode,
                        ResponsiblePersonName = asset.ResponsiblePersonName
                    });
                }
            }
            catch
            {
                // Continuar con los demás activos.
            }
        }

        documents = result;
    }

    protected void OnFileSelected(InputFileChangeEventArgs e)
    {
        selectedFile = e.File;
        message = null;
    }

    protected async Task UploadAsync()
    {
        if (!CanUploadDocuments)
        {
            messageClass = "alert alert-warning";
            message = "No tienes permiso para cargar documentos.";
            return;
        }

        if (selectedCategory == PhysicalCountCategory)
        {
            messageClass = "alert alert-warning";
            message = "Toma física no permite cargar documentos. Usa visualizar o descargar consolidado.";
            return;
        }

        if (SelectedTool is null)
        {
            messageClass = "alert alert-warning";
            message = "Debes seleccionar un activo existente del inventario.";
            return;
        }

        if (selectedFile is null)
        {
            messageClass = "alert alert-warning";
            message = "Debes seleccionar un archivo.";
            return;
        }

        try
        {
            isSaving = true;
            message = null;

            var api = CreateApiClient();

            await using var stream = selectedFile.OpenReadStream(MaxFileSize);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);

            fileContent.Headers.ContentType = new MediaTypeHeaderValue(
                string.IsNullOrWhiteSpace(selectedFile.ContentType)
                    ? "application/octet-stream"
                    : selectedFile.ContentType);

            content.Add(fileContent, "file", selectedFile.Name);
            content.Add(new StringContent(MapCategoryToDocumentType(selectedCategory)), "documentType");
            content.Add(new StringContent(BuildDescription()), "description");
            content.Add(new StringContent(string.IsNullOrWhiteSpace(uploadedBy) ? "admin" : uploadedBy.Trim()), "uploadedBy");

            var response = await api.PostAsync($"api/tools/{SelectedTool.Id}/documents", content);

            if (!response.IsSuccessStatusCode)
            {
                var detail = await response.Content.ReadAsStringAsync();

                messageClass = "alert alert-danger";
                message = $"No se pudo guardar el documento. Código: {(int)response.StatusCode}. Detalle: {detail}";
                return;
            }

            messageClass = "alert alert-success";
            message = $"Documento guardado correctamente para {SelectedTool.InternalCode} - {SelectedTool.Name}.";

            selectedFile = null;
            documentName = string.Empty;
            description = string.Empty;

            await LoadDocumentsAsync(api);
        }
        catch (Exception ex)
        {
            messageClass = "alert alert-danger";
            message = $"No se pudo guardar el documento. Detalle: {ex.Message}";
        }
        finally
        {
            isSaving = false;
        }
    }

    protected string BuildDescription()
    {
        var name = string.IsNullOrWhiteSpace(documentName)
            ? selectedFile?.Name ?? "Documento"
            : documentName.Trim();

        var detail = string.IsNullOrWhiteSpace(description)
            ? string.Empty
            : description.Trim();

        return $"[Clasificación: {selectedCategory}] [Nombre: {name}] {detail}".Trim();
    }

    protected void ClearForm()
    {
        assetSearchText = string.Empty;
        selectedFile = null;
        documentName = string.Empty;
        description = string.Empty;
        message = null;
    }

    protected void SelectCategory(string value)
    {
        selectedCategory = value;
        message = null;

        if (selectedCategory == PhysicalCountCategory)
        {
            selectedFile = null;
            documentName = string.Empty;
            description = string.Empty;
            selectedManualCode = "PhysicalCount";
        }
    }

    protected void OpenManual(string code)
    {
        selectedManualCode = code;
        showManualModal = true;
    }

    protected void CloseManual()
    {
        showManualModal = false;
    }

    protected void OpenDocument(DocumentViewItem item)
    {
        selectedDocument = item;
        showDocumentModal = true;
    }

    protected void CloseDocument()
    {
        showDocumentModal = false;
        selectedDocument = null;
    }

    protected void OpenPhysicalModal()
    {
        showPhysicalModal = true;
    }

    protected void ClosePhysicalModal()
    {
        showPhysicalModal = false;
    }

    protected ToolAssetOption? ResolveSelectedTool()
    {
        if (string.IsNullOrWhiteSpace(assetSearchText))
        {
            return null;
        }

        var clean = assetSearchText.Trim();

        return assets.FirstOrDefault(x =>
            string.Equals(FormatAssetOption(x), clean, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.InternalCode, clean, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.Name, clean, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(x.SerialNumber, clean, StringComparison.OrdinalIgnoreCase));
    }

    protected int CountByCategory(string category)
    {
        if (category == PhysicalCountCategory)
        {
            return physicalCounts.Count;
        }

        return documents.Count(x => string.Equals(GetDocumentCategory(x), category, StringComparison.OrdinalIgnoreCase));
    }

    protected string GetAssetHint()
    {
        if (assets.Count == 0)
        {
            return "No se cargó inventario. Valida que la API esté activa.";
        }

        if (string.IsNullOrWhiteSpace(assetSearchText))
        {
            return "Escribe y selecciona un activo existente del inventario.";
        }

        if (SelectedTool is null)
        {
            return "No coincide con un activo existente. Selecciona una opción de la lista.";
        }

        return $"Activo seleccionado: {SelectedTool.InternalCode} - {SelectedTool.Name}.";
    }

    protected string GetAssetHintClass()
    {
        return SelectedTool is null ? "docs-asset-hint new" : "docs-asset-hint ok";
    }

    protected string GetCategoryClass(string name)
    {
        return selectedCategory == name ? "active" : string.Empty;
    }

    protected string GetManualClass(string code)
    {
        return selectedManualCode == code ? "active" : string.Empty;
    }

    protected static string FormatAssetOption(ToolAssetOption asset)
    {
        var branch = string.IsNullOrWhiteSpace(asset.BranchCode) ? "Sin sede" : asset.BranchCode;
        var serial = string.IsNullOrWhiteSpace(asset.SerialNumber) ? "Sin serial" : asset.SerialNumber;

        return $"{asset.InternalCode} - {asset.Name} · {branch} · {serial}";
    }
protected string BuildPhysicalCountCsvDataUrl()
    {
        if (!CanDownloadDocuments)
        {
            return "#";
        }

        var csv = new StringBuilder();

        csv.AppendLine("Toma,Sede,Estado,Usuario,Codigo,Activo,Tipo,Proceso,Ubicacion,Observacion,Fecha");

        foreach (var item in FilteredReportedItems)
        {
            csv.AppendLine(string.Join(",", new[]
            {
                Csv(SelectedPhysicalCount?.CountNumber),
                Csv(SelectedPhysicalCount?.BranchCode),
                Csv(SelectedPhysicalCount?.StatusLabel),
                Csv(item.ParticipantName),
                Csv(item.ToolInternalCode),
                Csv(item.ToolName ?? item.ReportedName),
                Csv(item.ReportTypeLabel),
                Csv(GetReconciliationLabel(item)),
                Csv(item.FoundLocation),
                Csv(GetReportedObservation(item)),
                Csv(item.ReportedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm"))
            }));
        }

        return "data:text/csv;charset=utf-8," + Uri.EscapeDataString(csv.ToString());
    }


    protected static string Csv(string? value)
    {
        var safe = (value ?? string.Empty).Replace("\"", "\"\"");
        return "\"" + safe + "\"";
    }

    protected static string GetPhysicalStatusClass(string? status)
    {
        return status switch
        {
            "InProgress" => "docs-physical-status ok",
            "Finished" => "docs-physical-status ok",
            "FinishedWithDifferences" => "docs-physical-status pending",
            "Cancelled" => "docs-physical-status cancel",
            "Canceled" => "docs-physical-status cancel",
            _ => "docs-physical-status pending"
        };
    }
protected string BuildDownloadUrl(string? downloadUrl)
    {
        if (!CanDownloadDocuments)
        {
            return "#";
        }

        if (string.IsNullOrWhiteSpace(downloadUrl))
        {
            return "#";
        }

        if (downloadUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            return downloadUrl;
        }

        return $"{apiBaseUrl}{downloadUrl}";
    }


    protected static string GetDocumentCategory(DocumentViewItem item)
    {
        var fromDescription = TryGetDescriptionValue(item.Description, "Clasificación");

        if (!string.IsNullOrWhiteSpace(fromDescription))
        {
            return fromDescription;
        }

        return item.DocumentType switch
        {
            "TechnicalDocument" => "Ficha técnica",
            "MaintenanceSupport" => "Evidencia mantenimiento",
            "PhysicalCountEvidence" => "Toma física",
            "DeliveryAct" => "Acta",
            "ReturnAct" => "Acta",
            "Other" => "Factura",
            _ => "Ficha técnica"
        };
    }

    protected static string MapCategoryToDocumentType(string category)
    {
        return category switch
        {
            "Ficha técnica" => "TechnicalDocument",
            "Factura" => "Other",
            "Evidencia mantenimiento" => "MaintenanceSupport",
            "Acta" => "DeliveryAct",
            "Garantía" => "TechnicalDocument",
            _ => "Other"
        };
    }

    protected static string GetDocumentDisplayName(DocumentViewItem item)
    {
        var fromDescription = TryGetDescriptionValue(item.Description, "Nombre");

        return string.IsNullOrWhiteSpace(fromDescription)
            ? item.FileName
            : fromDescription;
    }

    protected static string? TryGetDescriptionValue(string? description, string key)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return null;
        }

        var token = $"[{key}:";
        var start = description.IndexOf(token, StringComparison.OrdinalIgnoreCase);

        if (start < 0)
        {
            return null;
        }

        start += token.Length;
        var end = description.IndexOf(']', start);

        if (end < 0)
        {
            return null;
        }

        return description[start..end].Trim();
    }

    protected static string GetReconciliationLabel(ReportedItem item)
    {
        if (!string.IsNullOrWhiteSpace(item.ReconciliationStatusLabel))
        {
            return item.ReconciliationStatusLabel;
        }

        return item.ReconciliationStatus switch
        {
            "RequiresUserClarification" => "Requiere aclaración",
            "ApprovedForCreation" => "Pendiente creación",
            "Reconciled" => "Conciliado",
            "Rejected" => "Rechazado",
            _ => item.RequiresUserClarification ? "Requiere aclaración" : "Pendiente revisión"
        };
    }

    protected static string GetReconciliationClass(ReportedItem item)
    {
        return item.ReconciliationStatus switch
        {
            "RequiresUserClarification" => "docs-physical-status pending",
            "ApprovedForCreation" => "docs-physical-status pending",
            "Reconciled" => "docs-physical-status ok",
            "Rejected" => "docs-physical-status cancel",
            _ => item.RequiresUserClarification ? "docs-physical-status pending" : "docs-physical-status pending"
        };
    }

    protected static bool Contains(string? source, string value)
    {
        return !string.IsNullOrWhiteSpace(source)
            && source.Contains(value, StringComparison.OrdinalIgnoreCase);
    }

    protected static string Show(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "—" : value;
    }

    protected static string FormatBytes(long value)
    {
        if (value >= 1024 * 1024)
        {
            return $"{value / 1024d / 1024d:0.00} MB";
        }

        if (value >= 1024)
        {
            return $"{value / 1024d:0.00} KB";
        }

        return $"{value} bytes";
    }

    protected sealed record DocumentCategory(string Icon, string Name);

    protected sealed class ToolAssetOption
    {
        public Guid Id { get; set; }
        public string InternalCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? SerialNumber { get; set; }
        public string? FixedAssetCode { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? ResponsiblePersonName { get; set; }
    }

    protected class DocumentItemModel
    {
        public Guid Id { get; set; }
        public Guid ToolAssetId { get; set; }
        public string DocumentType { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string? ObjectKey { get; set; }
        public string? ContentType { get; set; }
        public long SizeBytes { get; set; }
        public string? UploadedBy { get; set; }
        public DateTime UploadedAt { get; set; }
        public string? Description { get; set; }
        public string? DownloadUrl { get; set; }
    }

    protected sealed class DocumentViewItem : DocumentItemModel
    {
        public string ToolInternalCode { get; set; } = string.Empty;
        public string ToolName { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? ResponsiblePersonName { get; set; }
    }

    protected sealed class PhysicalCountListItem
    {
        public Guid Id { get; set; }
        public string CountNumber { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? FinishedAt { get; set; }
        public string? ResponsibleBy { get; set; }
        public string? Notes { get; set; }
        public int TotalItems { get; set; }
        public int FoundItems { get; set; }
        public int MissingItems { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    protected sealed class PhysicalCountBoard
    {
        public Guid Id { get; set; }
        public string CountNumber { get; set; } = string.Empty;
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public List<ParticipantItem> Participants { get; set; } = new();
        public PhysicalCountSummary Summary { get; set; } = new();
    }

    protected sealed class PhysicalCountSummary
    {
        public int TotalParticipants { get; set; }
        public int NotStarted { get; set; }
        public int InProgress { get; set; }
        public int Finished { get; set; }
        public int FinishedWithDifferences { get; set; }
        public int Expired { get; set; }
        public int TotalExpected { get; set; }
        public int TotalCounted { get; set; }
        public int TotalPending { get; set; }
        public int FoundItems { get; set; }
        public int MissingItems { get; set; }
        public int DifferentItems { get; set; }
        public int DamagedItems { get; set; }
        public int ExtraItems { get; set; }
        public decimal Progress { get; set; }
    }

    protected sealed class ParticipantItem
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public int ExpectedItems { get; set; }
        public int CountedItems { get; set; }
        public int PendingItems { get; set; }
    }

    protected sealed class ReportedItemsBoard
    {
        public Guid Id { get; set; }
        public string CountNumber { get; set; } = string.Empty;
        public List<ReportedItem> Items { get; set; } = new();
        public ReportedItemsSummary Summary { get; set; } = new();
    }

    protected sealed class ReportedItemsSummary
    {
        public int Total { get; set; }
        public int Found { get; set; }
        public int NotFound { get; set; }
        public int ReturnedOrDelivered { get; set; }
        public int Damaged { get; set; }
        public int DifferentLocation { get; set; }
        public int DifferentResponsible { get; set; }
        public int ExtraNotListed { get; set; }
        public int PendingReview { get; set; }
        public int RequiresUserClarification { get; set; }
        public int ApprovedForCreation { get; set; }
        public int Reconciled { get; set; }
        public int Rejected { get; set; }
        public int MissingMinimumData { get; set; }
    }

    protected sealed class ReportedItem
    {
        public Guid Id { get; set; }
        public Guid PhysicalCountId { get; set; }
        public string? ParticipantName { get; set; }
        public string? ParticipantUserName { get; set; }
        public string? ReportType { get; set; }
        public string? ReportTypeLabel { get; set; }
        public Guid? ToolAssetId { get; set; }
        public string? ToolInternalCode { get; set; }
        public string? ToolName { get; set; }
        public string? ReportedCode { get; set; }
        public string? ReportedName { get; set; }
        public string? ExpectedLocation { get; set; }
        public string? FoundLocation { get; set; }
        public string? ReconciliationStatus { get; set; }
        public string? ReconciliationStatusLabel { get; set; }
        public bool RequiresUserClarification { get; set; }
        public bool HasMinimumData { get; set; }
        public DateTime ReportedAt { get; set; }
    }

    protected sealed record ManualOption(
        string Code,
        string Icon,
        string Title,
        string Module,
        string Description,
        string Purpose,
        IReadOnlyList<string> Steps,
        IReadOnlyList<string> Rules,
        IReadOnlyList<string> RelatedDocuments);

    protected static string? GetReportedObservation(object? item)
    {
        if (item is null)
        {
            return null;
        }

        foreach (var propertyName in new[]
        {
            "Observation",
            "Observations",
            "Comment",
            "Comments",
            "Notes",
            "Description",
            "ReportObservation",
            "ReportNotes",
            "ReconciliationObservation"
        })
        {
            var property = item.GetType().GetProperty(propertyName);

            if (property is null)
            {
                continue;
            }

            var value = property.GetValue(item)?.ToString();

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }















}

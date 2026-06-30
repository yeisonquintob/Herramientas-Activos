# Matriz de roles y permisos - NAVI Herramientas y AF

## Reglas principales

- La matriz diferencia entre permisos de **ver** y permisos de **hacer**.
- El alcance puede ser **Total**, **Zona/Sede**, **Sede**, **Solo propio**, **Solo propios/asignados** o **No**.
- El rol **Técnico** está enfocado principalmente en la aplicación móvil.
- El técnico no debe ver conciliaciones administrativas.
- El técnico no debe crear solicitudes de compra.
- El técnico no debe gestionar planes de mantenimiento.
- El técnico puede reportar daños, novedades y preoperacionales.
- El técnico solo puede consultar su propio historial o sus herramientas asignadas.

## Pendientes de implementación

1. Revisar que el rol Técnico no tenga permisos de compra.
2. Revisar que el rol Técnico no tenga permisos de planes de mantenimiento.
3. Ajustar historial de técnico para que sea solo propio.
4. Bloquear conciliación para técnico.
5. Separar permiso de reportar daño de solicitud formal de mantenimiento.
6. Validar permisos móviles: Mobile.Access, Mobile.Tools.View, Mobile.Tools.Review, Mobile.Damage.Report, Mobile.PreOperational.Report.

## Matriz

| Módulo | Permiso | Acción | Tipo | Canal | Admin | Gerencial | Coordinador Taller | Ing. Servicios | Herramientero | Técnico | Auditor | Sede | Regla |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| Principal | Dashboard.View | Ver dashboard | Ver | Web | Total | Zona/Sede | Sede | Sede | Sede | No | Solo ver | Sede | El técnico no debe depender del dashboard web; su operación principal es móvil. |
| Inventario de AF | Tools.View | Consultar inventario | Ver | Web/Móvil | Total | Zona/Sede | Sede | Sede | Sede | Solo propios/asignados | Solo ver | Sede | Técnico solo debe ver herramientas asignadas o relacionadas con su operación. |
| Inventario de AF | Tools.Create | Crear activo/herramienta | Hacer | Web | Total | No | Sí | No | Sí | No | No | No | Técnico no crea activos desde web ni móvil. |
| Inventario de AF | Tools.Edit | Editar activo/herramienta | Hacer | Web | Total | No | Sí | Limitado | Sí | No | No | No | Técnico no edita datos maestros. |
| Inventario de AF | Tools.Delete | Anular/eliminar activo | Hacer | Web | Total | No | No | No | No | No | No | No | Acción crítica solo para administrador o flujo formal de baja. |
| Disponible y Ubicación | AssetAvailability.View | Ver disponibilidad y ubicación | Ver | Web | Total | Zona/Sede | Sede | Sede | Sede | No | Solo ver | Sede | Técnico no administra disponibilidad desde web. |
| Disponible y Ubicación | AssetAvailability.Edit | Cambiar sede, almacén, taller o ubicación | Hacer | Web | Total | No | Sí | No | Sí | No | No | No | Técnico no mueve activos administrativamente. |
| Asignar AF | AssetAssignment.View | Ver módulo de asignación | Ver | Web | Total | Zona/Sede | Sede | Sede | Sede | No | Solo ver | Sede | Técnico no debe gestionar asignaciones desde web. |
| Asignar AF | AssetAssignment.Assign | Asignar activo a responsable | Hacer | Web | Total | No | Sí | No | Sí | No | No | No | Asignación formal queda en rol administrativo. |
| Asignar AF | AssetAssignment.Return | Regresar activo a almacén/taller | Hacer | Web | Total | No | Sí | No | Sí | No | No | No | Devolución administrativa queda en herramientero/coordinador. |
| Historial de asignaciones | AssetAssignment.History | Ver historial de asignaciones | Ver | Web/Móvil | Total | Zona/Sede | Sede | Sede | Sede | Solo propio | Solo ver | Sede | Técnico solo puede ver su propio historial, nunca el historial global. |
| Hoja de Vida del AF | TechnicalLifeRecord.View | Ver hoja de vida | Ver | Web/Móvil | Total | Zona/Sede | Sede | Sede | Sede | Solo propios/asignados | Solo ver | Sede | Técnico puede consultar hoja de vida de activos asignados o intervenidos. |
| Hoja de Vida del AF | TechnicalLifeRecord.Edit | Editar datos técnicos | Hacer | Web | Total | No | Sí | Limitado | Sí | No | No | No | Técnico no edita hoja de vida administrativa. |
| Hoja de Vida del AF | TechnicalLifeRecord.Export | Exportar PDF/Excel | Hacer | Web | Total | Sí | Sí | Sí | Sí | No | Sí | No | Exportación no debe estar disponible para técnico móvil. |
| Documentos | Documents.View | Ver documentos | Ver | Web/Móvil | Total | Zona/Sede | Sede | Sede | Sede | Solo propios/asignados | Solo ver | Sede | Técnico puede ver documentos relacionados con su herramienta o novedad. |
| Documentos | Documents.Upload | Cargar evidencias/documentos | Hacer | Web/Móvil | Total | No | Sí | Sí | Sí | Solo evidencias propias | No | No | Técnico puede cargar evidencias desde móvil, no administrar documentos globales. |
| Documentos | Documents.Delete | Eliminar documentos | Hacer | Web | Total | No | No | No | No | No | No | No | Eliminar evidencia/documentos debe ser acción restringida. |
| Solicitar Compra AF | Purchases.View | Ver solicitudes de compra | Ver | Web | Total | Zona/Sede | Sede | Sede | No | No | Solo ver | No | Técnico no ve compras. |
| Solicitar Compra AF | Purchases.Request | Solicitar compra | Hacer | Web | Total | No | Sí | No | No | No | No | No | Técnico no puede solicitar compra. |
| Solicitar Compra AF | Purchases.Approve | Aprobar compra | Hacer | Web | Total | Sí | Sí | Sí | No | No | No | No | Aprobación queda fuera del técnico. |
| Consultar Mantenimientos | Maintenance.View | Ver mantenimientos | Ver | Web/Móvil | Total | Zona/Sede | Sede | Sede | Sede | Solo propios/intervenciones | Solo ver | Sede | Técnico solo consulta lo relacionado con sus herramientas o reportes. |
| Solicitar Mantenimiento | Maintenance.Request | Solicitar mantenimiento | Hacer | Web | Total | No | Sí | Sí | Sí | No | No | No | Técnico no debe crear solicitud formal de mantenimiento; reporta daño/novedad. |
| Planes de Mantenimiento | Maintenance.Plans.View | Ver planes de mantenimiento | Ver | Web | Total | Zona/Sede | Sede | Sede | Sede | No | Solo ver | Sede | Técnico no accede a planes de mantenimiento. |
| Planes de Mantenimiento | Maintenance.Plans.Manage | Gestionar planes de mantenimiento | Hacer | Web | Total | No | Sí | Sí | No | No | No | No | Técnico no crea, modifica ni administra planes. |
| Daños y novedades | Mobile.Damage.Report | Reportar daño o novedad | Hacer | Móvil | Sí | No | Sí | Sí | Sí | Sí | No | No | Técnico sí puede reportar daños y novedades desde la app móvil. |
| Daños y novedades | Mobile.PreOperational.Report | Reportar preoperacional SSTA | Hacer | Móvil | Sí | No | Sí | Sí | Sí | Sí | No | No | Técnico sí puede reportar preoperacionales desde móvil. |
| Tomas Físicas | PhysicalCounts.View | Consultar toma física | Ver | Web/Móvil | Total | Zona/Sede | Sede | Sede | Sede | Solo asignadas/propias | Solo ver | Sede | Técnico solo ve la toma que le corresponde ejecutar. |
| Tomas Físicas | PhysicalCounts.Create | Crear toma física | Hacer | Web | Total | No | Sí | No | Sí | No | No | No | Técnico no crea tomas físicas. |
| Tomas Físicas | Mobile.Tools.Review | Revisar herramienta en toma física móvil | Hacer | Móvil | Sí | No | Sí | Sí | Sí | Sí | No | No | Técnico ejecuta validación desde móvil, no desde conciliación web. |
| Conciliación | Reconciliation.View | Ver conciliación | Ver | Web | Total | Solo indicadores | Sede | Sede | No | No | Solo ver | No | Técnico no puede ver conciliación. |
| Conciliación | Reconciliation.Manage | Aclarar, aprobar creación, conciliar o rechazar | Hacer | Web | Total | No | Sí | Sí | No | No | No | No | Decisión administrativa. Técnico no participa en aprobación/conciliación. |
| Reportes | Reports.View | Ver reportes | Ver | Web | Total | Zona/Sede | Sede | Sede | Sede | No | Solo ver | Sede | Técnico no consulta reportes administrativos. |
| Configuración | Settings.View | Ver configuración | Ver | Web | Total | No | No | No | No | No | No | No | Solo administración. |
| Usuarios | Security.Users | Gestionar usuarios | Hacer | Web | Total | No | No | No | No | No | No | No | Solo administración. |
| Roles y permisos | Security.Roles | Gestionar roles y permisos | Hacer | Web | Total | No | No | No | No | No | No | No | Solo administración. |

## Perfil sugerido para Técnico

### Puede ver

- Sus herramientas asignadas.
- Su propia hoja de vida relacionada.
- Sus documentos/evidencias relacionados.
- Su propio historial.
- Tomas físicas asignadas a él.

### Puede hacer

- Revisar herramientas desde móvil.
- Reportar daño o novedad.
- Reportar preoperacional SSTA.
- Cargar evidencia propia.
- Solicitar préstamo si el flujo móvil lo requiere.

### No puede hacer

- No puede ver conciliación.
- No puede aprobar creación.
- No puede conciliar.
- No puede rechazar registros administrativos.
- No puede solicitar compra.
- No puede gestionar planes de mantenimiento.
- No puede ver historial global.
- No puede crear, editar ni anular activos.
- No puede gestionar usuarios, roles ni configuración.

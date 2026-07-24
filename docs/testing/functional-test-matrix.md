# Matriz de pruebas funcionales

Estados: **Auto** (automatizada), **Manual** (guion listo), **Pendiente** (exige
infraestructura o datos aislados).

| Módulo | Caso mínimo | Estado | Evidencia/condición |
|---|---|---|---|
| Login | credencial válida e inválida, bloqueo | Pendiente | SQL Testing con usuarios semilla |
| Logout | revocar sesión y rechazar token | Pendiente | SQL Testing |
| Sesión | expiración, inactividad, revocación | Pendiente | reloj/control de persistencia |
| Usuarios | crear, editar, bloquear, contraseña | Manual | rol `Security.Users` |
| Roles | crear y asignar permisos | Manual | rol `Security.Roles` |
| Permisos | permitir, denegar, bypass ADMIN | Auto | `RequirePermissionAttributeTests` |
| Compañías | listar, crear metadata, acceso | Pendiente | NaviMasterDb Testing |
| Selector | cambiar compañía y token | Pendiente | dos bases Testing |
| Inventario | listar, filtrar, detalle | Manual | datos aislados |
| Disponibilidad | sede, ubicación y estado | Manual | datos aislados |
| Asignaciones | solicitar, aprobar, asignar, regresar | Manual | matriz de permisos |
| Historial | filtrar y validar trazabilidad | Manual | movimientos controlados |
| Hoja de vida | ver/editar/exportar | Manual | activo de prueba |
| Compras | borrador, aprobación, cierre | Manual | solicitud de prueba |
| Mantenimiento | solicitar, cotizar, ejecutar, cerrar | Manual | activo de prueba |
| Planes | crear cronograma y vencimiento | Manual | reloj de pruebas |
| Tomas físicas | crear, iniciar, cerrar | Manual | sede de pruebas |
| Participantes | generar, reportar, finalizar | Manual | usuarios de pruebas |
| Conciliación | aclarar, crear, rechazar | Manual | reportes controlados |
| Documentos | cargar, ver, descargar | Pendiente | MinIO Testing |
| Importación | metadata/formato/firma válido e inválido | Auto | `ImportFilePolicyTests` |
| Importación | staging, aplicar y rollback | Pendiente | SQL + MinIO Testing |
| Backups | solicitar, completar, descargar | Pendiente | worker/proveedor real |
| Reportes | resumen, paginación y CSV | Manual | volumen de pruebas |
| Mobile | login, navegación, offline/actualización | Manual | navegador móvil |
| API | raíz, health, endpoint protegido | Auto | `ApiSecuritySmokeTests` |
| Arquitectura | Domain independiente | Auto | `ArchitectureTests` |
| Tenant | resolución única por request | Auto | `TenantContextTests` |
| Tenant isolation | no cruzar A/B por IDOR/export/import | Pendiente | dos bases SQL reales de Testing |

## Datos y ambientes

- Nunca usar Production.
- Crear `NaviMasterDb_Test`, `NaviTenantA_Test`, `NaviTenantB_Test` y bucket
  `navi-testing`.
- Reiniciar datos entre suites destructivas.
- Capturar trace id, usuario, compañía y resultado de auditoría.
- Los casos pendientes bloquean declarar el MVP estable.

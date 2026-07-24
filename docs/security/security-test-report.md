# Reporte de pruebas de seguridad

Fecha: 2026-07-24. Ambiente: `Testing`, sin conexión a bases productivas.

Resultado de la suite: **19 de 19 pruebas superadas, 0 omitidas y 0 fallidas**.

## Automatizadas

| Control | Resultado |
|---|---|
| Acceso anónimo a `/api/tools` | Pasa: 401 |
| Headers `X-Navi-*` falsificados | Pasa: 401 |
| Headers de seguridad en raíz | Pasa: 5 encabezados |
| Swagger fuera de Development | Pasa: 401/404 |
| Health liveness anónimo | Pasa: 200 |
| Permiso faltante | Pasa: 403 |
| Permiso presente | Pasa |
| Rol ADMIN | Pasa |
| Extensiones `.xls`, `.xlsm`, `.csv`, path sospechoso | Pasa: rechazadas |
| MIME/tamaño/firma OpenXML | Pasa |
| Permisos duplicados | Pasa: ninguno |
| Resolución doble de tenant | Pasa: rechazada |

La Fase 2 también verificó en runtime token alterado, identidad por headers y
encabezados de seguridad con el API temporal en ambiente Testing.

## Inspección de código

- JWT valida firma, issuer, audience, expiración y sesión.
- La identidad operativa se deriva de claims, no de headers del cliente.
- Swagger y Hangfire solo se registran en Development; Hangfire exige filtro.
- PasswordHasher ASP.NET Core conserva migración controlada del hash legado.
- Subidas Excel limitan tipo, tamaño, filas y firma.
- CSV neutraliza fórmulas.
- El fallback de autorización exige autenticación.
- CORS de Production no acepta orígenes locales arbitrarios.
- La consulta de vulnerabilidades NuGet no reporta paquetes vulnerables; la
  dependencia transitiva `Newtonsoft.Json 11.0.1` fue resuelta a `13.0.3`.

## Pendientes obligatorios

- Token vencido con reloj controlado.
- Usuario desactivado y sesión revocada sobre SQL Testing.
- Cambio malicioso de `CompanyId`, IDOR e import/export cruzados con dos tenants.
- Cookie inválida/antiforgery de Admin.
- Path traversal y XSS con documentos reales en MinIO Testing.
- SQL injection sobre filtros con base Testing.
- Rate limiting bajo concurrencia.
- Backup no autorizado y restore.

Estos casos no se simularon contra Production y bloquean la aprobación final de
seguridad multicompañía.

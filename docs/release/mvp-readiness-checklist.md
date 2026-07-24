# NAVI — Lista de preparación del MVP empresarial

Fecha de evaluación: 24 de julio de 2026

Rama: `feature/navi-mvp-empresarial`

Punto de retorno: `navi-pre-mvp-empresarial`

## Resultado

El código queda compilable, probado y con una base técnica considerablemente más
segura. Sin embargo, **todavía no se certifica como MVP empresarial estable**.
Faltan pruebas integrales contra infraestructura real, completar el
aprovisionamiento multicompañía y validar recuperación de respaldos.

## Compilación y pruebas

- [x] Restauración de paquetes completada.
- [x] Compilación Debug sin errores ni advertencias.
- [x] Pruebas Debug: 19 de 19 superadas.
- [x] Compilación Release sin errores ni advertencias.
- [x] Pruebas Release: 19 de 19 superadas.
- [x] Pruebas unitarias de reglas de importación.
- [x] Pruebas de arquitectura y catálogo de permisos.
- [x] Pruebas de contexto de compañía.
- [x] Pruebas HTTP de autenticación, headers y liveness.
- [ ] Pruebas funcionales completas con SQL Server y MinIO reales.
- [ ] Pruebas de carga con métricas y umbrales acordados.

## Base de datos y multicompañía

- [x] Migraciones operativas generadas y snapshot sincronizado.
- [x] Migración inicial de `NaviMasterDb` generada.
- [x] Catálogo maestro de compañías, bases y accesos.
- [x] Resolución de compañía y `DbContext` operacional dinámico.
- [x] Validación de acceso a compañía.
- [ ] Migrar usuarios y sesiones compartidas a la base maestra.
- [ ] Crear y validar una base plantilla operativa.
- [ ] Aprovisionar, clonar y migrar bases por compañía de extremo a extremo.
- [ ] Probar aislamiento con dos bases SQL Server reales.
- [ ] Hacer que las tareas Hangfire resuelvan explícitamente su compañía.

## Seguridad

- [x] Autenticación con JWT firmado.
- [x] Autorización por permisos y claims del servidor.
- [x] Headers `X-Navi-*` de identidad rechazados como fuente de confianza.
- [x] Contraseñas nuevas con `PasswordHasher`; hashes heredados se actualizan.
- [x] Bloqueo temporal por intentos fallidos.
- [x] Sesiones persistidas, expiración y revocación.
- [x] Auditoría de acciones críticas implementada.
- [x] Swagger y Hangfire protegidos fuera de desarrollo.
- [x] Rate limiting y encabezados de seguridad.
- [x] Cero paquetes vulnerables conocidos según NuGet al 24/07/2026.
- [x] No se encontraron secretos reales en archivos de configuración versionados.
- [x] Ningún flujo de login conserva contraseñas en almacenamiento del navegador.
- [ ] Migrar el almacenamiento del token de los clientes a cookie `HttpOnly`
  segura o implementar una estrategia BFF equivalente.
- [ ] Implementar rotación/refresh token si el tiempo de sesión lo requiere.
- [ ] Ejecutar pentest y pruebas CSRF/XSS en un ambiente desplegado.

## Importación y reportes

- [x] Solo se admiten archivos `.xlsx` con extensión, MIME y firma válidos.
- [x] Límites configurables de archivo y filas.
- [x] Staging, transacción e idempotencia por lote.
- [x] Normalización y reporte estructurado de calidad.
- [x] Decisiones crear, actualizar, ignorar, vincular y revisar.
- [x] Reportes de calidad JSON/CSV.
- [x] Reportes ejecutivos agregados, paginados y exportación CSV por streaming.
- [ ] Ejecutar importaciones válidas, inválidas y duplicadas contra SQL/MinIO reales.
- [ ] Validar importaciones simultáneas de dos compañías.

## Backups y recuperación

- [x] Diseño, estados y auditoría de backups documentados.
- [x] Volúmenes persistentes previstos para SQL, MinIO y llaves de protección.
- [x] Procedimiento de recuperación documentado.
- [ ] Implementar el worker real de backup/exportación por compañía.
- [ ] Incorporar checksum, retención, descarga autorizada y cifrado.
- [ ] Ejecutar y evidenciar una restauración completa.
- [ ] Definir y validar RPO/RTO.

## Docker y operación

- [x] Dockerfiles multi-stage para API, Admin, Mobile y Worker.
- [x] Procesos de aplicación ejecutados como usuario no root.
- [x] `docker-compose.production.yml` sin secretos embebidos.
- [x] `docker-compose.development.yml`.
- [x] Liveness y readiness de API.
- [x] Health checks de API, Admin, Mobile y Worker; reinicio, redes y volúmenes.
- [x] Configuración Compose validada.
- [ ] Publicar detrás de un reverse proxy con TLS administrado.
- [ ] Probar arranque completo con credenciales y servicios de un ambiente aislado.
- [ ] Probar persistencia después de recrear contenedores.

## Documentación

- [x] README actualizado.
- [x] Arquitectura, seguridad, datos y multicompañía.
- [x] Base de datos, migraciones y recuperación.
- [x] Desarrollo, permisos, clientes y pruebas.
- [x] Docker, producción y variables de entorno.
- [x] MinIO, Hangfire, autenticación y errores de API.
- [x] Importación y reportes.
- [x] Manual de usuario por rol.
- [x] Matriz de pruebas, seguridad y rendimiento.
- [x] Notas de versión e informe final.

## Decisión de salida

**NO-GO para declarar “NAVI MVP empresarial estable”.**

El siguiente hito requiere un entorno de integración aislado con SQL Server y
MinIO, dos compañías de prueba y datos controlados. Allí deben completarse
aprovisionamiento, aislamiento, importación, backup/restore, seguridad dinámica,
pruebas funcionales y carga. Hasta entonces, las fases 3, 4 y 5 permanecen
parcialmente pendientes.

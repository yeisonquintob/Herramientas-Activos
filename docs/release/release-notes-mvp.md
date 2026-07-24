# NAVI — Notas de versión candidata a MVP empresarial

Versión técnica: `mvp-candidate-2026.07.24`

Estado: candidata; no certificada para producción

Rama: `feature/navi-mvp-empresarial`

## Funciones y cambios

- Organización progresiva de componentes, clientes HTTP y contratos
  compartidos, preservando las vistas actuales.
- Catálogo único de permisos con metadatos de alcance y compatibilidad.
- Autenticación JWT, autorización por claims, hashing de contraseñas, bloqueo de
  acceso, sesiones persistidas, auditoría, rate limiting y encabezados seguros.
- Base maestra, catálogo de compañías, acceso por usuario y resolución dinámica
  de base operacional.
- Importación Excel con controles de archivo, staging, idempotencia, calidad y
  trazabilidad.
- Reportes ejecutivos agregados y exportación CSV por streaming.
- Endpoints `/health/live` y `/health/ready`.
- Proyecto automatizado de pruebas con 19 casos.
- Contenedores multi-stage para API, Admin, Mobile y Worker, con usuario no root.
- Documentación técnica, operativa y manual de usuario.
- Corrección de la dependencia transitiva vulnerable `Newtonsoft.Json 11.0.1`
  mediante resolución explícita a `13.0.3`.

## Migraciones incluidas

Base operacional:

- `20260724171429_AddEnterpriseSecurity`
- `20260724180238_AddImportQualityTracking`
- `20260724180947_AddImportCompletionTracking`

Base maestra:

- `20260724174641_InitialNaviMaster`

Las migraciones no se aplican automáticamente en este paquete. Deben respaldarse
las bases, revisar el SQL generado y ejecutarse mediante el procedimiento de
despliegue.

## Configuración requerida

Los valores se describen en
[`docs/deployment/environment-variables.md`](../deployment/environment-variables.md).
Como mínimo se requieren:

- conexiones de la base operacional y maestra;
- clave JWT aleatoria de alta entropía, emisor y audiencia;
- credenciales externas para SQL Server y MinIO;
- orígenes CORS exactos;
- URL pública de la API para la PWA;
- configuración explícita de multicompañía.

No se deben reutilizar valores de `.env.example` en producción.

## Procedimiento de despliegue

1. Completar la lista de preparación y aprobar ventana de cambio.
2. Respaldar bases y objetos; validar que el backup pueda leerse.
3. Crear secretos en el gestor del ambiente.
4. Construir imágenes desde el commit aprobado.
5. Aplicar migraciones primero en un ambiente espejo.
6. Iniciar SQL/MinIO, API, Worker, Admin y Mobile en ese orden.
7. Verificar readiness, login, permisos, compañía, inventario e importación.
8. Habilitar tráfico únicamente después de las pruebas de humo.

El detalle operativo está en
[`docs/deployment/production.md`](../deployment/production.md).

## Rollback

1. Retirar tráfico de la versión nueva.
2. Detener API, Admin, Mobile y Worker de la versión candidata.
3. Restaurar el backup validado si una migración modificó datos.
4. Desplegar las imágenes del commit anterior.
5. Comprobar login, inventario, documentos y tareas.
6. Conservar logs y evidencias para el análisis posterior.

No se recomienda ejecutar `Down()` automáticamente sobre una base con datos
reales sin revisar pérdida de columnas o incompatibilidades.

## Riesgos y limitaciones

- No se ha probado el aislamiento con dos bases reales.
- El aprovisionamiento y clonación de bases por compañía aún no es automático.
- Backup/restore por compañía no cuenta todavía con ejecución completa.
- Importación y MinIO no se han validado en infraestructura de integración.
- El token aún se conserva en almacenamiento del cliente.
- Permanecen URLs `localhost` como valores de desarrollo o fallback heredado.
- Falta ejecutar carga, pentest y recuperación ante desastre.

Por estos motivos esta versión es una **candidata técnica**, no la versión
“NAVI MVP empresarial estable”.

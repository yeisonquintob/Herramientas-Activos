# Manejo de errores HTTP

Production usa `ProblemDetails` y no expone stack traces.

| Estado | Uso |
|---|---|
| 400 | solicitud o archivo inválido |
| 401 | autenticación ausente o token/sesión inválidos |
| 403 | permiso o acceso a compañía denegado |
| 404 | recurso inexistente |
| 409 | estado incompatible, compañía requerida o coincidencia ambigua |
| 422 | validación de negocio cuando el endpoint la distinga |
| 429 | límite de solicitudes |
| 500 | fallo no controlado, con trace id y sin detalle técnico |

Los clientes deben conservar el código, título, detalle seguro y `traceId`. No
deben mostrar excepciones internas ni reintentar automáticamente escrituras no
idempotentes.

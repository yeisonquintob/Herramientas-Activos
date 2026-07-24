# Arquitectura de seguridad

## Alcance implementado

La API usa autenticación JWT Bearer y autorización basada en claims. La identidad, el rol,
los permisos, la sede, la compañía y la sesión se obtienen exclusivamente del token validado;
los encabezados `X-Navi-*` enviados por el navegador dejaron de ser una fuente de confianza.

El flujo es:

1. `POST /api/auth/login` valida usuario, contraseña, bloqueo y estado.
2. Se crea una sesión persistente con expiración por inactividad y expiración absoluta.
3. Se emite un JWT firmado con `UserId`, rol, permisos, sede, compañía, `SecurityStamp` y
   `SessionId`.
4. Cada request valida firma, emisor, audiencia, expiración, sesión persistente, estado del
   usuario y `SecurityStamp`.
5. Los endpoints y filtros de permiso responden `401` cuando no hay identidad válida y `403`
   cuando falta autorización.

## Contraseñas

- Los usuarios nuevos y los restablecimientos usan `PasswordHasher<AppUser>`.
- La política mínima exige ocho caracteres, mayúscula, minúscula y número.
- Los hashes SHA-256 heredados se aceptan únicamente durante un inicio válido y se reemplazan
  inmediatamente por el formato de `PasswordHasher`.
- Un hash vacío no autentica. Esos usuarios necesitan restablecimiento administrativo.
- Cinco intentos fallidos producen un bloqueo temporal de quince minutos.
- Cambiar o restablecer contraseña renueva `SecurityStamp` y revoca las sesiones activas.

## Sesiones

`Security.UserSessions` registra usuario, sello de seguridad, compañía activa, última actividad,
expiración absoluta, revocación, agente resumido e IP limitada. El cierre de sesión revoca la
sesión actual; el cambio de contraseña, el restablecimiento y la desactivación revocan todas las
sesiones del usuario.

## Auditoría

`Security.AuditLogs` conserva acciones mutativas, login, logout e intentos fallidos con usuario,
compañía, sede, resultado, correlación y metadatos limitados. El filtro de auditoría no almacena
contraseñas, tokens, archivos ni secretos.

## Endurecimiento HTTP

- `ProblemDetails` y manejador de excepciones fuera de Development.
- HSTS fuera de Development.
- CORS limitado a orígenes configurados; localhost adicional solo en Development.
- Rate limiting del login.
- `X-Content-Type-Options`, `Referrer-Policy`, `Permissions-Policy`, `X-Frame-Options` y CSP.
- Swagger y Hangfire solo se publican en Development; Hangfire además exige identidad y permiso
  administrativo.

## Configuración obligatoria

En Production debe existir una clave de al menos 32 bytes:

```text
Jwt__SigningKey=<secreto-largo-generado-fuera-del-repositorio>
```

También deben configurarse `Jwt__Issuer`, `Jwt__Audience`, conexiones SQL y credenciales MinIO
mediante variables de entorno o un gestor de secretos. La clave MinIO que estuvo previamente en
el repositorio debe rotarse antes de desplegar.

## Riesgos residuales

- Admin Web y Mobile PWA conservan el access token en almacenamiento del navegador por
  compatibilidad con la arquitectura actual. Una evolución recomendada es BFF/cookie `HttpOnly`
  para Admin y tokens de acceso en memoria con refresh rotativo para Mobile.
- No se implementó refresh token. Al expirar el access token el usuario debe autenticarse otra
  vez; esto reduce superficie de ataque, pero no ofrece renovación transparente.
- La pantalla administrativa de sesiones queda pendiente; la revocación ya está disponible en
  API.
- La CSP actual es apropiada para la API. Los encabezados de las aplicaciones Blazor deben
  validarse por separado antes de imponer una CSP más estricta a sus recursos estáticos.

## Validación realizada

Prueba local en ambiente `Testing`, sin aplicar migraciones ni tocar datos:

- raíz pública: `200`;
- `/api/tools` anónimo: `401`;
- `/api/tools` con headers de identidad falsificados: `401`;
- `/api/tools` con JWT alterado: `401`;
- cinco encabezados de seguridad presentes.

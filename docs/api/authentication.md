# Autenticación de la API

## Login

`POST /api/auth/login`

```json
{
  "userName": "usuario",
  "password": "contraseña"
}
```

Una respuesta válida incluye `accessToken`, `tokenExpiresAtUtc`, `sessionId`, usuario, rol,
permisos y alcance operativo. El cliente debe enviar:

```http
Authorization: Bearer <accessToken>
```

Los encabezados `X-Navi-User`, `X-Navi-Role`, `X-Navi-Permissions`, `X-Navi-Company` y
equivalentes no autentican ni autorizan.

## Expiración y revocación

El token tiene expiración corta configurable. Además, cada request comprueba la sesión
persistente:

- expiración por inactividad;
- expiración absoluta;
- revocación;
- usuario activo;
- coincidencia de `SecurityStamp`.

`POST /api/auth/logout` revoca la sesión actual. `POST /api/auth/sessions/revoke-all` exige el
permiso `Security.Users` y revoca todas las sesiones del usuario objetivo.

## Respuestas relevantes

- `400`: solicitud inválida o contraseña que no cumple la política.
- `401`: credenciales, token o sesión inválidos.
- `403`: identidad válida sin el permiso requerido.
- `429`: límite de intentos de autenticación.

Los clientes no deben registrar el token, la contraseña ni el cuerpo de login.

## Variables

```text
Jwt__Issuer=Navi.ToolsAssets
Jwt__Audience=Navi.ToolsAssets.Clients
Jwt__SigningKey=<secreto de 32 bytes o más>
Jwt__AccessTokenMinutes=15
Jwt__AbsoluteSessionHours=12
Jwt__InactivityMinutes=30
```

Development y Testing pueden generar una llave efímera si no existe configuración. Production
falla al iniciar si falta la llave; esto evita una instancia insegura.

# Clientes HTTP NAVI

## Admin Web

El cliente nombrado `NaviApi` se registra una sola vez en `Program.cs`:

- `BaseAddress` proviene de `NaviApi:BaseUrl`;
- timeout configurable mediante `NaviApi:TimeoutSeconds` (30 segundos por
  defecto);
- el handler de compatibilidad actual se agrega desde DI;
- las páginas obtienen clientes mediante `IHttpClientFactory`.

No se deben crear instancias con `new HttpClient()` dentro de páginas.

## Mobile PWA

Blazor WebAssembly registra un `HttpClient` scoped proporcionado por el host y
lo encapsula en `NaviMobileApiClient`. Esta es una excepción de plataforma a
`IHttpClientFactory`: no existen handlers de servidor ni sockets por cliente en
el navegador. La dirección y el timeout siguen proviniendo de configuración.

`NaviMobileApiClient`:

- centraliza JSON;
- interpreta errores de API;
- aplica temporalmente compatibilidad de identidad;
- propaga `CancellationToken` en todas las operaciones públicas;
- no registra tokens, contraseñas ni cuerpos sensibles;
- mantiene caché limitada a datos sin filtros.

La fase de seguridad sustituirá los headers de identidad por Bearer y evitará
almacenar secretos en almacenamiento accesible a JavaScript.

## Contratos comunes

`Navi.ToolsAssets.Shared.Common` contiene:

- `ApiResult<T>`;
- `PagedResult<T>`;
- `ValidationProblem`.

Su adopción es progresiva: no se envuelven respuestas existentes cuando hacerlo
rompería compatibilidad HTTP.

## Códigos de error

Los clientes deben distinguir al menos:

- 401: sesión no válida o expirada;
- 403: permiso insuficiente;
- 404: recurso inexistente;
- 409: conflicto de estado o duplicidad;
- 422: validación funcional.

Los detalles técnicos se registran del lado servidor; la interfaz recibe un
mensaje funcional y un identificador de correlación cuando esté disponible.

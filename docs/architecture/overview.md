# Arquitectura NAVI

## Dependencias actuales

```text
Domain
  ↑
Application ───→ Shared
  ↑
Infrastructure ─→ Shared
  ↑
API / Worker

Admin Web ──────→ Shared ──HTTP──→ API
Mobile PWA ─────→ Shared ──HTTP──→ API
```

Domain no referencia Entity Framework, ASP.NET Core, UI, HTTP, SQL Server,
MinIO ni Hangfire. Application contiene abstracciones funcionales y depende de
Domain/Shared. Infrastructure implementa persistencia y almacenamiento. API
expone HTTP y Worker ejecuta trabajo en segundo plano.

## Evolución incremental

- No se reescribe la solución.
- Contratos comunes se agregan en Shared.
- Casos de uso nuevos deben ubicarse en Application.
- EF Core, MinIO y servicios externos permanecen en Infrastructure.
- Los controladores deben reducir lógica progresivamente a casos de uso.
- Admin y Mobile consumen clientes configurados y no acceden a EF.

## Deuda prioritaria

- autenticación actual todavía no es una identidad ASP.NET autenticada;
- autorización basada en headers debe sustituirse por claims;
- falta base maestra y resolución de compañía;
- faltan proyectos de pruebas automatizadas;
- varios controladores y componentes son demasiado extensos.

Estas deudas se atienden por fases para conservar los procesos operativos.

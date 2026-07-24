# Hangfire

Hangfire usa SQL Server y el Worker procesa las colas:

- `default`
- `imports`
- `sync`
- `maintenance`
- `notifications`

El dashboard solo se habilita en Development y además exige autorización NAVI.
No se publica en Production.

Los jobs nuevos deben incluir `CompanyId`, resolver el tenant dentro del scope,
ser idempotentes y no transportar cadenas de conexión en argumentos. La
resolución completa por compañía para todos los jobs heredados sigue pendiente
y bloquea declarar multicompañía productiva.

# Convenciones

- Entidades en singular y colecciones en plural.
- Requests con sufijo `Request`; respuestas y DTO con nombre funcional.
- IDs `Guid`; fechas nuevas de auditoría preferiblemente UTC.
- Permisos `Módulo.Acción` definidos en `PermissionCodes` y catalogados una vez.
- `CancellationToken` en I/O asíncrono.
- `AsNoTracking`, proyección y paginación en consultas de lectura.
- No exponer entidades EF cuando un contrato estable sea necesario.
- No crear `HttpClient` directamente; usar clientes centralizados.
- No registrar tokens, contraseñas, conexiones ni contenido sensible.
- No agregar CSS global si el comportamiento es específico de un módulo.
- No usar `TODO` ambiguos: registrar deuda con alcance y criterio de cierre.
- Cambios de esquema solo con migración inspeccionada y respaldo.

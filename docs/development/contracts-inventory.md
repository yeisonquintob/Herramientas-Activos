# Inventario de contratos y nombres

Fecha de revisión: 2026-07-24.

## Estado encontrado

- 31 clases de entidad en Domain.
- 80 declaraciones detectadas con sufijos `Dto`, `Request`, `Response` o
  `ViewModel`.
- 30 controladores API.
- 112 componentes Razor en Admin Web.
- 52 componentes Razor en Mobile PWA.

## Convenciones

- Entidad: singular (`ToolAsset`).
- Colección: plural (`ToolAssets`).
- Entrada: acción + `Request`.
- Salida: recurso/acción + `Response`.
- Transferencia interna: nombre funcional + `Dto`.
- Identificadores: `Guid`, conservando la convención dominante.
- Auditoría y eventos nuevos: `DateTimeOffset` en UTC.
- Permisos: `Módulo.Acción`.
- Enums persistidos: valores explícitos antes de agregar nuevos miembros.

## Riesgos detectados

- Muchos DTO y modelos de vista todavía están declarados dentro de
  controladores o componentes Razor.
- Existen códigos de permiso en literales distribuidos por UI y controladores.
- Hay mezcla histórica español/inglés en nombres visibles y técnicos.
- Algunos endpoints proyectan directamente desde entidades EF, aunque no
  devuelven la entidad completa.
- Estados operativos se normalizan en varios módulos.

## Decisión

No se realiza un renombrado masivo. Se introdujeron contratos y catálogo
compartidos, manteniendo adaptadores para los nombres anteriores. Cada feature
debe mover sus contratos a Shared o Application cuando sea modificada por una
necesidad funcional y tenga pruebas de compatibilidad.

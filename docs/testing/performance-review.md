# Revisión de rendimiento

## Hallazgos

- El endpoint heredado `api/dashboard/executive` materializa inventario,
  sedes, responsables, mantenimientos y documentos antes de agregar. Se
  conserva por compatibilidad visual, pero no es adecuado para gran volumen.
- El importador usa staging en memoria hasta 10.000 filas y consultas por fila;
  está acotado, pero requiere medición con archivos máximos.
- Algunos módulos heredados mantienen `Include` y `ToList` amplios.
- Imágenes Mobile pueden depender de preview/base64 y deben medirse en equipos
  de baja memoria.

## Mejoras aplicadas

- Reportes ejecutivos nuevos con `AsNoTracking`, agregados SQL y proyección.
- Inventario ejecutivo paginado, máximo 200 filas.
- CSV de inventario escrito mediante `AsAsyncEnumerable`.
- `CancellationToken` en endpoints de importación y reportes.
- Límites de 10 MB/10.000 filas y aplicación configurable por lotes.
- Índices por lote/estado y activo destino.
- Respuestas de listado no incluyen navegaciones innecesarias.
- Health checks separan liveness de readiness.

## Medición

No se registran tiempos inventados: no existe aún un dataset de volumen
representativo ni SQL de pruebas en este entorno. Antes de Production se debe
medir p50/p95 y memoria con 10k, 100k y 1M activos, así como archivos de 1k y
10k filas. Objetivos iniciales sugeridos:

- resumen ejecutivo p95 menor de 2 s;
- página de 50 registros p95 menor de 1 s;
- memoria estable durante CSV;
- importación de 10k sin timeout HTTP ni crecimiento no acotado.

El dashboard heredado debe migrarse al endpoint proyectado después de pruebas
de paridad visual y funcional.

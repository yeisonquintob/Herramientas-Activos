# Reportes ejecutivos

La API de reportes ejecutivos utiliza el contexto de la compañía activa y no
acepta un identificador de compañía proporcionado por el cliente. Así se evita
consultar datos cruzados cuando la resolución multicompañía está habilitada.

## Resumen

`GET /api/reports/executive/summary`

Incluye:

- inventario total, disponible, asignado, prestado y en mantenimiento;
- dañados, no aptos y no localizados;
- activos sin ubicación, responsable, documentos o datos técnicos;
- vencimientos de certificación y mantenimiento;
- campañas, resultados y diferencias de tomas físicas;
- calidad acumulada de importaciones.

Filtros disponibles: fechas, sede, zona, categoría, estado, responsable y tipo.

## Detalle paginado

`GET /api/reports/executive/inventory?page=1&pageSize=50`

El tamaño de página se limita a 200. La consulta usa `AsNoTracking`,
proyección SQL, orden estable y paginación antes de materializar.

## Exportación

`GET /api/reports/executive/inventory.csv`

El CSV se escribe en flujo sobre la respuesta y no carga todo el inventario en
memoria. Los valores que comienzan con caracteres de fórmula se neutralizan.

## Permisos

- `Reports.View` para resumen y detalle.
- `Reports.Export` adicional para exportar.

PDF queda fuera del cierre de esta fase: requiere una plantilla corporativa
aprobada y validación visual. La exportación CSV queda funcional.

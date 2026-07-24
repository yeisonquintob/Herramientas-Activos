# Importación Excel por compañía

## Alcance

El importador opera sobre el `NaviToolsAssetsDbContext` de la compañía activa.
Cuando la resolución multicompañía está habilitada, cada lote, fila y activo
se guarda únicamente en la base operacional resuelta para esa compañía.

La carga inicial es staging: analizar un archivo no crea ni actualiza activos.
La aplicación definitiva exige el permiso correspondiente y una acción
posterior explícita.

## Seguridad y límites

- Solo se admite `.xlsx`; `.xls` y `.xlsm` se rechazan.
- Se valida extensión, MIME y firma ZIP/OpenXML.
- Límite predeterminado: 10 MB, con máximo técnico de 25 MB.
- Límite predeterminado: 10.000 filas, con máximo técnico de 50.000.
- El nombre se normaliza antes de almacenarlo en MinIO.
- El usuario se obtiene del JWT; los campos `ProcessedBy` y `ActionBy`
  históricos se conservan por compatibilidad, pero no son fuente de identidad.
- No se ejecutan macros ni se insertan datos durante la lectura inicial.

Configuración opcional:

```text
Imports__MaximumFileSizeBytes=10485760
Imports__MaximumRows=10000
Imports__ApplyBatchSize=250
```

## Encabezados reconocidos

El archivo debe contener al menos uno de estos identificadores: código NAVI o
interno, Fenix365, activo fijo o serial. También se reconocen nombre, sede,
ubicación, estantería, responsable, tipo, categoría, estado, marca, modelo,
voltaje, capacidad de carga y proveedor.

Los valores originales quedan en `RawDataJson`; los valores limpios quedan en
`NormalizedDataJson`. Los errores y advertencias se guardan como objetos con:
campo, valor, regla, mensaje, severidad y sugerencia.

## Estados y decisiones

- `NewCandidate`: no existe coincidencia y puede crearse.
- `Existing`: coincidencia exacta por código interno.
- `Inconsistent`: coincidencia exacta con diferencias.
- `PossibleDuplicate`: coincidencia por otro identificador; nunca se fusiona sola.
- `PendingReview`: enviada a revisión.
- `Reviewed`: revisada sin aplicar cambios.
- `Ignored`: omitida por decisión.
- `Linked`: asociada manualmente a un activo.
- `Created`: activo creado.
- `Updated`: cambios confirmados aplicados.
- `Error`: no cumple una regla obligatoria.

Las coincidencias usan, en orden, código interno, Fenix365, activo fijo y
serial. Crear es idempotente porque solo procesa filas `NewCandidate`; los
lotes se aplican en transacciones de tamaño configurable. Actualizar exige
`Confirm=true`, solo admite campos autorizados y deja evento de ciclo de vida.
Los responsables ambiguos no se asignan automáticamente.

## Permisos

- `Imports.View`
- `Imports.Upload`
- `Imports.Apply`
- `Imports.Reconcile`
- `Reports.Export` para descargar el CSV de calidad

## Endpoints principales

- `POST /api/imports/excel`
- `POST /api/imports/{id}/apply-new-candidates`
- `GET /api/imports/{id}/quality-report`
- `GET /api/imports/{id}/quality-report.csv`
- `PATCH /api/imports/rows/{rowId}/send-to-review`
- `PATCH /api/imports/rows/{rowId}/mark-reviewed`
- `PATCH /api/imports/rows/{rowId}/mark-ignored`
- `PATCH /api/imports/rows/{rowId}/link-to-tool/{toolId}`
- `PATCH /api/imports/rows/{rowId}/apply-update/{toolId}`

## Operación

La migración debe revisarse y aplicarse por ambiente mediante el procedimiento
controlado. El API no crea ni actualiza la base automáticamente. MinIO requiere
credenciales externas; si no están configuradas, la carga falla sin revelar
el secreto.

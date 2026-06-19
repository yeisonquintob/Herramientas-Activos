# Configuración de MinIO - NAVI Herramientas

## Servicio

MinIO se usa como almacenamiento de documentos y evidencias del sistema NAVI Herramientas.

## Acceso local

- API: http://localhost:9100
- Consola: http://localhost:9101

## Bucket principal

navi-tools-documents

## Uso del bucket

El bucket almacena:

- Hojas de vida digitales
- Evidencias fotográficas
- Actas de entrega
- Actas de devolución
- Soportes de mantenimiento
- Documentos de baja
- Archivos de importación
- Evidencias de toma física
- Documentos técnicos

## Estructura lógica

tools/
life-cycles/
evidences/
damages/
loans/
returns/
maintenance/
purchases/
disposals/
physical-counts/
imports/
reconciliation/
audit/

## Regla de seguridad

No se deben subir credenciales reales al repositorio.

Las credenciales locales deben quedar en archivos .env ignorados por Git.

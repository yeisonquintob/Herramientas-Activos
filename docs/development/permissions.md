# Convención de permisos NAVI

## Fuente canónica

- Códigos: `Navi.ToolsAssets.Shared.Security.PermissionCodes`.
- Metadatos: `Navi.ToolsAssets.Shared.Security.PermissionCatalog`.
- Definición: `PermissionDefinition`.

El formato canónico es `Módulo.Acción`. Los códigos existentes se conservan
para compatibilidad; no se realiza un renombrado masivo.

Cada definición contiene:

- código;
- nombre visible;
- módulo;
- categoría;
- descripción;
- alcance global, compañía o sede;
- indicador administrativo;
- indicador móvil;
- requisito de compañía;
- requisito de sede.

El endpoint `GET api/auth/permissions` obtiene ahora la información del catálogo
compartido. El campo histórico `Action` continúa presente como alias de
`Category`, evitando romper los consumidores actuales.

## Compatibilidad heredada

`AuthController.NormalizePermission` mantiene mapeos desde códigos históricos
como `INVENTORY.VIEW`, `LOCATION.MANAGE` o `PHYSICALCOUNT.CLOSE`. Estos
adaptadores deben mantenerse hasta que:

1. no existan roles persistidos con el código anterior;
2. las pruebas de autorización cubran el reemplazo;
3. Admin y Mobile consuman únicamente códigos canónicos.

Los permisos no deben confiar en la visibilidad de botones. La API debe validar
el permiso autenticado en cada operación protegida. La sustitución de headers
heredados por claims pertenece a la fase de seguridad.
